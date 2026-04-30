using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.Rider.Backend.Product;

namespace ReSharperPlugin.SharedMeta
{
    /// <summary>
    /// Declares the activation zones this plugin needs. ReSharper enables the
    /// plugin only when all referenced zones are active — Rider host + C# PSI +
    /// Feature Services. Without this marker the plugin would not load.
    /// </summary>
    [ZoneMarker]
    public class ZoneMarker :
        IRequire<IRiderProductEnvironmentZone>,
        IRequire<ILanguageCSharpZone>,
        IRequire<ICodeEditingZone>
    {
    }
}
