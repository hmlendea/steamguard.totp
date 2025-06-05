using System;

using NuciExtensions;

namespace SteamGuard.TOTP
{
    public sealed class TimeStepProvider : ITimeStepProvider
    {
        static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

        public long GetCurrentTimeStep()
            => DateTimeExtensions.GetElapsedUnixTime(DateTime.UtcNow).Ticks / Interval.Ticks;
    }
}
