using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.Parts;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Feature.Services.Navigation.ContextNavigation;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.DataContext;

namespace ReSharperPlugin.SharedMeta
{
    /// <summary>
    /// Adds entries to Rider's <b>Navigate → Go To</b> submenu (and the right-click
    /// context menu) that bridge the SharedMeta boundary:
    /// <list type="bullet">
    ///   <item>standing on a generated client method (or a call site of one) — offer
    ///         <i>"MetaService Method"</i> to jump to the originating
    ///         <c>[MetaMethod]</c> on the <c>[MetaService]</c> interface;</item>
    ///   <item>standing on a <c>[MetaMethod]</c> — offer
    ///         <i>"Generated Client Method"</i> (or the count, if multiple counterparts
    ///         exist) to jump to the generated mirror.</item>
    /// </list>
    /// <para>
    /// Wired via <see cref="ContextNavigationProviderAttribute"/>, which is the
    /// supported registration point for <see cref="INavigateFromHereProvider"/>
    /// — distinct from <c>[ShellComponent]</c>. The hook only contributes new menu
    /// entries; it does not override Rider's existing Go to Declaration / Ctrl+Click
    /// behaviour, which is what made the earlier <c>GetNavigateToTargets</c> attempt
    /// regress (that hook replaces default targets rather than augmenting them).
    /// </para>
    /// </summary>
    [ContextNavigationProvider(Instantiation.DemandAnyThreadSafe)]
    public class MetaServiceNavigationProvider : INavigateFromHereImportantProvider
    {
        static MetaServiceNavigationProvider()
        {
            DiagLog.Write("MetaServiceNavigationProvider TYPE LOADED (static ctor)");
        }

        public MetaServiceNavigationProvider()
        {
            DiagLog.Write("MetaServiceNavigationProvider INSTANCE CREATED");
        }

        public IEnumerable<ContextNavigation> CreateWorkflow(IDataContext dataContext)
        {
            // Collect into a List<> rather than using `yield return` so the body runs
            // synchronously when the framework calls CreateWorkflow. With an iterator
            // method nothing executes (and nothing logs) until something walks the
            // returned IEnumerable — which the diagnostics in 0.4.3 never saw fire.
            DiagLog.Write("MetaServiceNavigationProvider.CreateWorkflow ENTERED");
            var result = new List<ContextNavigation>();

            var element = dataContext.GetData(PsiDataConstants.DECLARED_ELEMENT);
            DiagLog.Write($"  DECLARED_ELEMENT = {element?.GetType().Name ?? "<null>"} name={element?.ShortName ?? "<null>"}");
            if (element is not IMethod method)
            {
                DiagLog.Write("  not an IMethod — bail");
                return result;
            }

            var isMeta = MetaServiceMatcher.IsMetaMethod(method);
            var isGenerated = MetaServiceMatcher.IsGeneratedClientMethod(method);
            DiagLog.Write($"  classification: isMeta={isMeta} isGenerated={isGenerated}");

            if (isGenerated)
            {
                var meta = MetaServiceMatcher.FindMetaMethodCounterpart(method);
                if (meta != null)
                {
                    result.Add(new ContextNavigation(
                        title: "MetaService Method",
                        actionId: "GoToSharedMetaServiceMethod",
                        actionGroup: NavigationActionGroup.Blessed,
                        execution: () => meta.Navigate(transferFocus: true),
                        icon: null));
                }
            }
            else if (isMeta)
            {
                var generated = MetaServiceMatcher.FindGeneratedCounterparts(method, method.GetSolution());
                DiagLog.Write($"  forward path produced {generated.Count} counterparts");
                if (generated.Count > 0)
                {
                    var label = generated.Count == 1
                        ? "Generated Client Method"
                        : $"Generated Client Method ({generated.Count} counterparts)";
                    var first = generated[0];
                    result.Add(new ContextNavigation(
                        title: label,
                        actionId: "GoToSharedMetaGeneratedMethod",
                        actionGroup: NavigationActionGroup.Blessed,
                        execution: () => first.Navigate(transferFocus: true),
                        icon: null));
                }
            }

            DiagLog.Write($"  CreateWorkflow returning {result.Count} entries");
            return result;
        }
    }
}
