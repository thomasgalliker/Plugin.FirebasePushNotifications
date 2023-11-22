using System.Diagnostics;
using Android.App;
using Android.Runtime;

namespace MauiSampleApp
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.WriteLine($"TaskScheduler_UnobservedTaskException: {e.Exception}");
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}