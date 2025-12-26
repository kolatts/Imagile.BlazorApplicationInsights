# Copilot Instructions - Imagile.BlazorApplicationInsights

## Project Overview
A Blazor WebAssembly library that wraps the Application Insights JavaScript SDK for telemetry tracking in Blazor applications. Targets .NET 10 and supports both standalone WASM apps and Blazor Web Apps (with SSR/Auto modes).

## Architecture

### Core Components
- **[src/Imagile.BlazorApplicationInsights](src/Imagile.BlazorApplicationInsights/)** - Main library (NuGet package source)
  - `ApplicationInsights.cs` - C# wrapper invoking App Insights JS via JSInterop
  - `Components/ApplicationInsightsInit.razor` - Razor component that injects the AI snippet script and initializes the SDK
  - `Logging/ApplicationInsightsLoggerProvider.cs` - ILoggerProvider implementation for WASM-only logging to App Insights
  - `Models/` - Telemetry models (EventTelemetry, ExceptionTelemetry, etc.) mirroring AI JS API
  - `wwwroot/BlazorApplicationInsights.lib.module.js` - JS interop helpers

### Key Design Patterns
- **Dual initialization modes**: Web App mode (config via C#) vs WASM standalone mode (config via JS snippet)
- **Platform-specific features**: `ILoggerProvider` only registers on `OperatingSystem.IsBrowser()` (WASM context)
- **JS interop boundary**: All tracking methods are async JSInterop calls to `appInsights.*` JavaScript functions
- **Component-based initialization**: `<ApplicationInsightsInit>` must be added to `App.razor` with appropriate `@rendermode` for Web Apps

## Project Structure Conventions

### Targeting & Versioning
- Single target framework: `net10.0` only (see [Imagile.BlazorApplicationInsights.csproj](src/Imagile.BlazorApplicationInsights/Imagile.BlazorApplicationInsights.csproj))
- SDK version pinned in [global.json](global.json): `"version": "10.0.100"`
- Versioning via GitVersion (mainline mode): [GitVersion.yml](GitVersion.yml) and [Directory.Build.props](Directory.Build.props)
- CI/CD injects version properties during build; local builds default to `0.0.1-local`

### Sample Projects
- **[samples/BlazorApplicationInsights.Sample.WebApp](samples/BlazorApplicationInsights.Sample.WebApp/)** - Blazor Web App (.NET 8+ style with Auto render mode)
- **[samples/BlazorApplicationInsights.Sample.Wasm](samples/BlazorApplicationInsights.Sample.Wasm/)** - Standalone Blazor WASM (client-only)
- **[samples/BlazorApplicationInsights.Sample.Server](samples/BlazorApplicationInsights.Sample.Server/)** - Legacy Blazor Server (pre-.NET 8 style)
- Samples demonstrate `AddBlazorApplicationInsights()` setup with real connection strings

## Developer Workflows

### Building
```powershell
dotnet build -c Release  # From solution root
```

### Testing
- Test project: [tests/BlazorApplicationInsights.Tests](tests/BlazorApplicationInsights.Tests/)
- Uses xUnit, Moq, FluentAssertions
- Playwright integration prepared but currently commented out in [.github/workflows/cicd.yml](.github/workflows/cicd.yml#L38-L51)
- Run tests: `dotnet test`

### Adding New Telemetry Methods
1. Add method signature to `IApplicationInsights` interface
2. Implement in `ApplicationInsights.cs` as JSInterop call to `appInsights.*` or `blazorApplicationInsights.*`
3. If needed, add JS helper to [wwwroot/BlazorApplicationInsights.lib.module.js](src/Imagile.BlazorApplicationInsights/wwwroot/BlazorApplicationInsights.lib.module.js)
4. Update README.md API list and code examples

### Package Publishing
- Automated via GitHub Actions: [.github/workflows/cicd.yml](.github/workflows/cicd.yml)
- Pushes to `master` trigger release flow with semantic versioning
- Package metadata in [Imagile.BlazorApplicationInsights.csproj](src/Imagile.BlazorApplicationInsights/Imagile.BlazorApplicationInsights.csproj#L7-L18)

## Critical Integration Points

### Client Registration Pattern
**Web App mode** (requires interactivity):
```csharp
// In Program.cs (both server and client projects)
builder.Services.AddBlazorApplicationInsights(x =>
{
    x.ConnectionString = "{connection string}";
});

// In App.razor
<ApplicationInsightsInit @rendermode="@InteractiveAuto" />
```

**WASM Standalone mode**:
```csharp
// Program.cs
builder.Services.AddBlazorApplicationInsights(); // Config in JS snippet

// App.razor
<ApplicationInsightsInit IsWasmStandalone="true" />
```

### JavaScript Interop Boundary
- All tracking methods in [ApplicationInsights.cs](src/Imagile.BlazorApplicationInsights/ApplicationInsights.cs) use `_jsRuntime.InvokeVoidAsync()` or `InvokeAsync<T>()`
- JavaScript SDK snippet injected via raw string in [ApplicationInsightsInit.razor.cs](src/Imagile.BlazorApplicationInsights/Components/ApplicationInsightsInit.razor.cs#L33)
- Custom helpers prefixed with `blazorApplicationInsights.*` namespace (e.g., `trackDependencyData`, `getContext`)

## Common Pitfalls

### Platform Detection
- `ILoggerProvider` registration checks `OperatingSystem.IsBrowser()` - don't bypass this or Server apps will fail
- Test helper: `IServiceCollectionExtensions.PretendBrowserPlatform` for unit tests

### Render Mode Requirements
- Web Apps: `<ApplicationInsightsInit>` needs `@rendermode` when using `onAppInsightsInit` callback (interactive component)
- Standalone WASM: Must set `IsWasmStandalone="true"` parameter

### Configuration Split
- Web App mode: Connection string configured in C# via `Config` object
- WASM standalone: Connection string must be in JS snippet `cfg` section OR updated via `UpdateCfg()`
- See [README.md](README.md#L42-L135) for complete setup examples per mode

## Debugging Tips
- Ad blockers/CSP can block AI script - check browser console for `Failed to load Application Insights SDK script`
- Error handling in [ApplicationInsightsInit.razor.cs](src/Imagile.BlazorApplicationInsights/Components/ApplicationInsightsInit.razor.cs#L62-L66) logs setup failures
- Use `await ApplicationInsights.Context()` to verify SDK initialization state

## Dependencies
- **Microsoft.AspNetCore.Components** (10.0.*) - Blazor component model
- **Microsoft.Extensions.Logging** (10.0.*) - ILoggerProvider integration
- **JetBrains.Annotations** (PublicAPI markers, PrivateAssets)
- Application Insights JS SDK loaded from CDN: `https://js.monitor.azure.com/scripts/b/ai.3.gbl.min.js`
