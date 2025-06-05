using Moq;
using NUnit.Framework;

namespace SteamGuard.TOTP.UnitTests
{
    public class StringExtensionsTests
    {
        Mock<ITimeStepProvider> timeStepProviderMock;

        SteamGuard steamGuard;

        [SetUp]
        public void SetUp()
        {
            timeStepProviderMock = new Mock<ITimeStepProvider>();

            steamGuard = new SteamGuard(timeStepProviderMock.Object);
        }

        [Test]
        [TestCase("TY1YEH2Y21OCUAEIJ47AV8E068W8HUEM", 1, "XQ4CH")]
        [TestCase("TY1YEH2Y21OCUAEIJ47AV8E068W8HUEM", 3, "58RH6")]
        [TestCase("TY1YEH2Y21OCUAEIJ47AV8E068W8HUEM", 54600835, "7WWPY")]
        [TestCase("E4VVBJK7RU43Q2V1VSZC14BXMMH3ZLPB", 1, "P8JF7")]
        [TestCase("E4VVBJK7RU43Q2V1VSZC14BXMMH3ZLPB", 3, "58XVX")]
        [TestCase("E4VVBJK7RU43Q2V1VSZC14BXMMH3ZLPB", 54600835, "W32KJ")]
        public void GivenSteamGuard_WhenGeneratingAnAuthenticationCode_ThenTheCorrectCodeIsReturned(string totpKey, long timeStep, string expectedCode)
        {
            timeStepProviderMock
                .Setup(x => x.GetCurrentTimeStep())
                .Returns(timeStep);

            Assert.That(steamGuard.GenerateAuthenticationCode(totpKey), Is.EqualTo(expectedCode));
        }
    }
}
