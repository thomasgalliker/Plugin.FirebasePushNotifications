﻿using Android.App;
using Android.Content.PM;
using Android.OS;

namespace MauiSampleApp
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(new[] { "medication_intake" }, Categories = new[] { "android.intent.category.DEFAULT" })]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}