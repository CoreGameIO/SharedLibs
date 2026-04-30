using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.Rider.Backend.Product;

namespace ReSharperPlugin.SharedMeta
{
    /// <summary>
    /// Declares the activation zones this plugin needs. ReSharper enables the
    /// plugin only when all referenced zones are active.
    /// <para>
    /// Required zones, with what each unlocks for us:
    /// <list type="bullet">
    ///   <item><see cref="IRiderProductEnvironmentZone"/> — declares this is a Rider
    ///         (not standalone ReSharper) plugin.</item>
    ///   <item><see cref="ILanguageCSharpZone"/> — needed to read C# PSI symbols
    ///         (<c>IMethod</c>, attributes, declared elements).</item>
    ///   <item><see cref="ICodeEditingZone"/> — registers the searcher and
    ///         navigation subsystems used by Find Usages and Ctrl+Shift+G.</item>
    ///   <item><see cref="NavigationZone"/> — registers the
    ///         <c>INavigateFromHereProvider</c> contributor for the Ctrl+Shift+G popup.</item>
    /// </list>
    /// </para>
    /// </summary>
    [ZoneMarker]
    public class ZoneMarker :
        IRequire<IRiderProductEnvironmentZone>,
        IRequire<ILanguageCSharpZone>,
        IRequire<ICodeEditingZone>,
        IRequire<NavigationZone>
    {
    }
}
