using Imagile.BlazorApplicationInsights.Interfaces;
using Imagile.BlazorApplicationInsights.Models;
using System;
using System.Threading.Tasks;

namespace Imagile.BlazorApplicationInsights
{
    public class ApplicationInsightsInitConfig
    {
        public Config? Config { get; set; }

        public Func<IApplicationInsights, Task>? OnAppInsightsInit { get; set; }
    }
}
