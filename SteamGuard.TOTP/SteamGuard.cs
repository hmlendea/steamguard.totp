using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace SteamGuard.TOTP
{
    public sealed class SteamGuard : ISteamGuard
    {
        static readonly int CodeLength = 5;
        static readonly string AllowedTotpKeyCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        static readonly string AuthenticationCodeAlphabet = "23456789BCDFGHJKMNPQRTVWXY";

        readonly ITimeStepProvider timeStepProvider;

        public SteamGuard()
            : this(new TimeStepProvider())
        {
        }

        public SteamGuard(ITimeStepProvider timeStepProvider)
        {
            this.timeStepProvider = timeStepProvider;
        }

        public string GenerateAuthenticationCode(string totpKey)
        {
            byte[] securityToken = EncodeToBase32(totpKey);

            using (HMACSHA1 hmacshA1 = new HMACSHA1(securityToken))
            {
                int binCode = ComputeTotp(hmacshA1);
                return GenerateCodeFromBinCode(binCode);
            }
        }

        int ComputeTotp(HashAlgorithm algorithm)
        {
            long currentTimeStep = timeStepProvider.GetCurrentTimeStep();
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)currentTimeStep));
            var hmac = algorithm.ComputeHash(bytes);
            byte offset = (byte)(hmac[19] & 15);

            int binCode = 
            (
                (((hmac[offset] & 127) << 24) |
                ((hmac[offset + 1] & 255) << 16)) |
                ((hmac[offset + 2] & 255) << 8)
            ) | (hmac[offset + 3] & 255);

            return binCode;
        }

        string GenerateCodeFromBinCode(int binCode)
        {
            string code = string.Empty;

            for (int i = 0; i < CodeLength; ++i)
            {
                code += AuthenticationCodeAlphabet[binCode % AuthenticationCodeAlphabet.Length];
                binCode /= AuthenticationCodeAlphabet.Length;
            }

            return code;
        }

        byte[] EncodeToBase32(string source)
        {
            string bits = source
                .ToUpper()
                .ToCharArray()
                .Select(c => Convert.ToString(AllowedTotpKeyCharacters.IndexOf(c), 2).PadLeft(5, '0'))
                .Aggregate((a, b) => a + b);

            return Enumerable
                .Range(0, bits.Length / 8)
                .Select(i => Convert.ToByte(bits.Substring(i * 8, 8), 2))
                .ToArray();
        }
    }
}
