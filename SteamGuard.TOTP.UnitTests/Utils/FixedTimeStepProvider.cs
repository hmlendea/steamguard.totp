namespace SteamGuard.TOTP.UnitTests.Utils
{
    public sealed class FixedTimeStepProvider(long value) : ITimeStepProvider
    {
        public long GetCurrentTimeStep() => value;
    }
}