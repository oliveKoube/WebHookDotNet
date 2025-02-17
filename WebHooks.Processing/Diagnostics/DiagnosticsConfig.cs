using System.Diagnostics;

namespace WebHooks.Processing.Diagnostics;

internal sealed class DiagnosticsConfig
{
    internal const string ActivitySourceName = "webhooks-processing";
    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}