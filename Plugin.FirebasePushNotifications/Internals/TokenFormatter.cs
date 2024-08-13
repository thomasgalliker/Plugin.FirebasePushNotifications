﻿namespace Plugin.FirebasePushNotifications.Internals
{
    internal class TokenFormatter
    {
        internal static string AnonymizeToken(string token)
        {
            if (token == null)
            {
                return "null";
            }

            try
            {
                var substringLength = Math.Min(10, token.Length / 10);
                if (substringLength < 5)
                {
                    return Constants.SuppressedString;
                }

                return $"{Constants.SuppressedString}...{token[^substringLength..]}";
            }
            catch
            {
                return Constants.SuppressedString;
            }
        }
    }
}
