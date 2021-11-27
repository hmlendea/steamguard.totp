namespace SteamGuard.TOTP
{
    public interface ITimeStepProvider
    {
        long GetCurrentTimeStep();
    }
}
