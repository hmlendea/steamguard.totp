using System;

using NuciExtensions;

namespace SteamGuard.TOTP
{
    public sealed class TimeStepProvider : ITimeStepProvider
    {
        static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

        public long GetCurrentTimeStep()
        {
            TimeSpan elapsedUnixTime = DateTimeExtensions.GetElapsedUnixTime(DateTime.UtcNow);

            return elapsedUnixTime.Ticks / Interval.Ticks;
        }
    }
}
