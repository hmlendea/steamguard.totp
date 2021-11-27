namespace SteamGuard.TOTP
{
    public interface ISteamGuard
    {
        string GenerateAuthenticationCode(string totpKey);
    }
}
