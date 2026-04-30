using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.ExtensionsAPI;

namespace ReSharperPlugin.SharedMeta
{
    /// <summary>
    /// Hooks into the ReSharper Find Usages pipeline to add a related declared element
    /// for every method we recognise on either side of the SharedMeta boundary:
    /// <list type="bullet">
    ///   <item>asking for usages of a <c>[MetaMethod]</c> interface method also runs the
    ///         search against the matching <c>*ApiClient.{Name}Async</c>,
    ///         <c>*ApiClient.{Name}Signal</c>, and <c>*EntityQueryApi.{Name}Async</c>;</item>
    ///   <item>asking for usages of any of those generated client methods also runs the
    ///         search against the originating <c>[MetaMethod]</c> interface method.</item>
    /// </list>
    /// <para>
    /// We don't synthesise new references — the call-site references already exist (they
    /// target the generated method). We just expand the search root by returning related
    /// declared elements, so both endpoints are queried in one Find Usages pass and the
    /// results land in the same window.
    /// </para>
    /// <para>
    /// Subclasses <see cref="DomainSpecificSearcherFactoryBase"/> so all the other factory
    /// hooks (text search, constant search, navigation, …) keep their default no-op
    /// behavior — we only override what we actually contribute.
    /// </para>
    /// </summary>
    [ShellComponent(Instantiation.DemandAnyThreadSafe)]
    public class MetaMethodSearcherFactory : DomainSpecificSearcherFactoryBase
    {
        public override bool IsCompatibleWithLanguage(PsiLanguageType languageType)
            => languageType.Is<CSharpLanguage>();

        public override IEnumerable<RelatedDeclaredElement> GetRelatedDeclaredElements(IDeclaredElement element)
        {
            DiagLog.Write($"GetRelatedDeclaredElements called: kind={element?.GetType().Name ?? "<null>"} name={element?.ShortName ?? "<null>"}");
            if (element is not IMethod method)
            {
                DiagLog.Write("  not an IMethod — bail");
                yield break;
            }

            var isMeta = MetaServiceMatcher.IsMetaMethod(method);
            var isGenerated = MetaServiceMatcher.IsGeneratedClientMethod(method);
            DiagLog.Write($"  classification: isMetaMethod={isMeta} isGeneratedClient={isGenerated}");

            if (isMeta)
            {
                var solution = method.GetSolution();
                var generated = MetaServiceMatcher.FindGeneratedCounterparts(method, solution);
                DiagLog.Write($"  forward path returned {generated.Count} counterpart(s)");
                foreach (var g in generated)
                {
                    DiagLog.Write($"    related -> {g.GetContainingType()?.GetClrName().FullName}.{g.ShortName}");
                    yield return new RelatedDeclaredElement(g);
                }
            }
            else if (isGenerated)
            {
                var meta = MetaServiceMatcher.FindMetaMethodCounterpart(method);
                if (meta != null)
                {
                    DiagLog.Write($"  reverse path -> {meta.GetContainingType()?.GetClrName().FullName}.{meta.ShortName}");
                    yield return new RelatedDeclaredElement(meta);
                }
                else
                {
                    DiagLog.Write("  reverse path -> NULL (attribute missing or interface lookup failed)");
                }
            }
        }

        /// <summary>
        /// Augments Go-To-Declaration / Ctrl+Click navigation with the SharedMeta
        /// counterpart of the symbol under the cursor:
        /// <list type="bullet">
        ///   <item>on a generated client method (call site or declaration) the original
        ///         <c>[MetaMethod]</c> on the <c>[MetaService]</c> interface is offered as
        ///         an additional jump target;</item>
        ///   <item>on a <c>[MetaMethod]</c> the generated counterparts are offered.</item>
        /// </list>
        /// In both cases <c>OriginalElementIsRelevant=true</c> keeps the standard target
        /// (the method's own declaration) in the popup so the existing behaviour is not
        /// replaced — the SharedMeta target is just appended to the choices.
        /// </summary>
        public override NavigateTargets GetNavigateToTargets(IDeclaredElement element)
        {
            if (element is not IMethod method) return new NavigateTargets();

            if (MetaServiceMatcher.IsMetaMethod(method))
            {
                var solution = method.GetSolution();
                var generated = MetaServiceMatcher.FindGeneratedCounterparts(method, solution);
                if (generated.Count == 0) return new NavigateTargets();
                DiagLog.Write($"GetNavigateToTargets(meta {method.ShortName}) -> {generated.Count} generated");
                return new NavigateTargets(generated.Cast<IDeclaredElement>().ToList(), originalElementIsRelevant: true);
            }

            if (MetaServiceMatcher.IsGeneratedClientMethod(method))
            {
                var meta = MetaServiceMatcher.FindMetaMethodCounterpart(method);
                if (meta == null) return new NavigateTargets();
                DiagLog.Write($"GetNavigateToTargets(generated {method.ShortName}) -> meta {meta.GetContainingType()?.ShortName}.{meta.ShortName}");
                return new NavigateTargets(meta, originalElementIsRelevant: true);
            }

            return new NavigateTargets();
        }
    }
}
