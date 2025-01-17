using System.Diagnostics;

namespace WebHooks.Api.Diagnostics;

internal sealed class DiagnosticsConfig
{
    internal const string ActivitySourceName = "Diagnostics";
    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}