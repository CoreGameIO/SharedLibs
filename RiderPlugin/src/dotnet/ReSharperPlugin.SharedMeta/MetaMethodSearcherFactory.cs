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
        static MetaMethodSearcherFactory()
        {
            DiagLog.Write("MetaMethodSearcherFactory TYPE LOADED (static ctor)");
        }

        public MetaMethodSearcherFactory()
        {
            DiagLog.Write("MetaMethodSearcherFactory INSTANCE CREATED");
        }

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

        // NOTE: GetNavigateToTargets was overridden in 0.3.0–0.3.2 to add the source
        // [MetaMethod] as a Ctrl+Click target on generated client methods, but the
        // override broke Rider's standard navigation in both directions:
        //   - on a [MetaMethod] interface declaration the default "Go to Implementation"
        //     lookup stopped finding the impl class
        //   - on a generated method or its call site, returning a non-empty
        //     NavigateTargets — even with originalElementIsRelevant=true — replaces
        //     the default declaration target instead of augmenting it
        // The hook is intentionally NOT overridden anymore; reverse navigation will
        // come back via INavigateFromHereProvider (separate Navigate menu entry) in a
        // later iteration once the SDK contract is properly understood.
    }
}
