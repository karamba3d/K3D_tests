#if ALL_TESTS

namespace KarambaCommon.Tests.License
{
    using System;
    using System.IO;
    using System.Reflection;
    using Karamba.Licenses;
    using NUnit.Framework;

    [TestFixture]
    public class License_tests
    {
        [Test]
        public void ActivateLicense()
        {
            // Arrange
            var workingDir = Directory.GetCurrentDirectory();
            var licensePath = License.licensePath();
            License.getLicense();

            // Act
            var hasExpired = License.has_expired();
            var licenseType = License.licenseType().ToString();

            // Assert
            var validTypes = Enum.GetNames(typeof(feb.LicenseType));
            Assert.That(hasExpired, Is.False);
            Assert.That(Array.Exists(validTypes, type => type == licenseType), Is.True);

            // Arrange
            // Load a new license
            License.unload();
        }
    }
}

#endif
