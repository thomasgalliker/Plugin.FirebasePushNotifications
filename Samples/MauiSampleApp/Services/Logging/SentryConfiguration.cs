using Microsoft.Extensions.Logging;
using Sentry.Extensions.Logging;

namespace MauiSampleApp.Services.Logging
{
    public static class SentryConfiguration
    {
        public static void Configure(SentryLoggingOptions options)
        {
            options.InitializeSdk = true;
            options.Debug = false;
            options.Dsn = "https://a6a1f9dae37aa06035b28a7fe65ae4ce@o4507458300280832.ingest.de.sentry.io/4507526259998800";
            options.MinimumEventLevel = LogLevel.Warning;
            options.MinimumBreadcrumbLevel = LogLevel.Debug;
        }
    }
}