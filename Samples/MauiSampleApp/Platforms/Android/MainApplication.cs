using System.Diagnostics;
using Android.App;
using Android.Runtime;
using Microsoft.Extensions.Logging;

namespace MauiSampleApp
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            AndroidEnvironment.UnhandledExceptionRaiser += this.AndroidEnvironment_UnhandledExceptionRaiser;
            TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<MainApplication>>();
            logger.LogError(e.Exception, "AndroidEnvironment_UnhandledExceptionRaiser");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<MainApplication>>();
            logger.LogError(e.Exception, "TaskScheduler_UnobservedTaskException");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<MainApplication>>();
            logger.LogError(e.ExceptionObject as Exception, "CurrentDomain_UnhandledException");

            NLog.LogManager.Shutdown();
        }
    }
}