using Imagile.BlazorApplicationInsights.Models;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Imagile.BlazorApplicationInsights.Interfaces;

[EditorBrowsable(EditorBrowsableState.Never)]
[Browsable(false)]
public interface IPropertiesPlugin
{
    Task<TelemetryContext> Context();
}
