﻿using Android.App.Job;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class PNFirebaseJobService : JobService
    {
        public override bool OnStartJob(JobParameters @params)
        {
            return false;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            return false;
        }
    }
}