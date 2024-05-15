using Android.App;
using Android.Content.PM;

namespace MauiSampleApp
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true, 
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}