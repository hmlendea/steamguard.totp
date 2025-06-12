using System;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;

using SteamGuard.TOTP.UnitTests.Utils;

namespace SteamGuard.TOTP.UnitTests
{
    public class StringExtensionsTests
    {
        Mock<ITimeStepProvider> timeStepProviderMock;

        const string ValidTotpKey = "JBSWY3DPEHPK3PXP";
        const long FixedTimeStep = 123456789L;
        const string AllowedCharacters = "23456789BCDFGHJKMNPQRTVWXY";

        SteamGuard steamGuard;

        [SetUp]
        public void SetUp()
        {
            timeStepProviderMock = new Mock<ITimeStepProvider>();

            steamGuard = new SteamGuard(timeStepProviderMock.Object);
        }

        [Test]
        [TestCase("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3", 613, "D57RM")]
        [TestCase("E4VVBJK7RU43Q2V1VSZC14BXMMH3ZLPB", 1, "P8JF7")]
        [TestCase("E4VVBJK7RU43Q2V1VSZC14BXMMH3ZLPB", 3, "58XVX")]
        [TestCase("E4VVBJK7RU43Q2V1VSZC14BXMMH3ZLPB", 54600835, "W32KJ")]
        [TestCase("SQUTXBT56XIDXZ63UBVVMK3ORG4ZDRCF", 873, "W28WD")]
        [TestCase("TY1YEH2Y21OCUAEIJ47AV8E068W8HUEM", 1, "XQ4CH")]
        [TestCase("TY1YEH2Y21OCUAEIJ47AV8E068W8HUEM", 3, "58RH6")]
        [TestCase("TY1YEH2Y21OCUAEIJ47AV8E068W8HUEM", 54600835, "7WWPY")]
        public void GivenSteamGuard_WhenGeneratingAnAuthenticationCode_ThenTheCorrectCodeIsReturned(string totpKey, long timeStep, string expectedCode)
        {
            timeStepProviderMock
                .Setup(x => x.GetCurrentTimeStep())
                .Returns(timeStep);

            Assert.That(steamGuard.GenerateAuthenticationCode(totpKey), Is.EqualTo(expectedCode));
        }

        [Test]
        public void GenerateAuthenticationCode_ReturnsCodeOfCorrectLength()
        {
            SteamGuard sg = new(new FixedTimeStepProvider(FixedTimeStep));
            string code = sg.GenerateAuthenticationCode(ValidTotpKey);

            Assert.That(code.Length, Is.EqualTo(5));
        }

        [Test]
        public void GenerateAuthenticationCode_UsesAllowedCharacters()
        {
            SteamGuard sg = new(new FixedTimeStepProvider(FixedTimeStep));
            string code = sg.GenerateAuthenticationCode(ValidTotpKey);

            Assert.That(code.All(AllowedCharacters.Contains));
        }

        [Test]
        public void GenerateAuthenticationCode_WithDifferentTimeSteps_ProducesDifferentCodes()
        {
            SteamGuard sg1 = new(new FixedTimeStepProvider(FixedTimeStep));
            SteamGuard sg2 = new(new FixedTimeStepProvider(FixedTimeStep + 1));

            string code1 = sg1.GenerateAuthenticationCode(ValidTotpKey);
            string code2 = sg2.GenerateAuthenticationCode(ValidTotpKey);

            Assert.That(code1, Is.Not.EqualTo(code2));
        }

        [Test]
        public void GenerateAuthenticationCode_WithEmptyKey_ThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new SteamGuard(new FixedTimeStepProvider(FixedTimeStep)).GenerateAuthenticationCode(""));

        [Test]
        public void GenerateAuthenticationCode_WithNullKey_ThrowsNullReferenceException()
            => Assert.Throws<NullReferenceException>(() => new SteamGuard(new FixedTimeStepProvider(FixedTimeStep)).GenerateAuthenticationCode(null));

        [Test]
        public void EncodeToBase32_ValidInput_ProducesExpectedLength()
        {
            var method = typeof(SteamGuard).GetMethod("EncodeToBase32", BindingFlags.Static | BindingFlags.NonPublic);
            byte[] result = (byte[])method.Invoke(null, [ValidTotpKey]);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length > 0, Is.True);
        }

        [Test]
        public void GenerateCodeFromBinCode_ProducesCorrectLengthAndChars()
        {
            var method = typeof(SteamGuard).GetMethod("GenerateCodeFromBinCode", BindingFlags.Static | BindingFlags.NonPublic);
            string code = (string)method.Invoke(null, [123456789]);

            Assert.That(code.Length, Is.EqualTo(5));
            Assert.That(code.All(AllowedCharacters.Contains));
        }

        [Test]
        public void DefaultConstructor_UsesDefaultTimeStepProvider()
            => Assert.That(new SteamGuard(), Is.Not.Null);
    }
}
