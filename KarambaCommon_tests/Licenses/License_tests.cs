using NUnit.Framework;
using Karamba.Licenses;
using System.IO;

namespace KarambaCommon.Tests.License
{
    [TestFixture]
    public class License_tests
    {
#if ALL_TESTS
        [Test]
        public void ActivateLicense()
        {
            var license_path = Karamba.Licenses.License.licensePath();
            Karamba.Licenses.License.getLicense();
            var has_expired = Karamba.Licenses.License.has_expired();
            Assert.IsFalse(has_expired);
            var license_type = Karamba.Licenses.License.licenseType();
            Assert.True(license_type == feb.License.LicenseType.lic_trial || license_type == feb.License.LicenseType.lic_student);

            // load a new license 
            Karamba.Licenses.License.unload();

            var dir = Directory.GetCurrentDirectory();

            Assert.True(File.Exists(@"..\..\..\Resources\License\license_TRIAL.lic") && File.Exists(@"..\..\..\Resources\License\public.key"),
                @"In order that the tests work place 'license_TRIAL.lic' and 'public.key' under '..\Resources\License\'");

            // set the path to where you saved your license file
            Karamba.Licenses.License.getLicense(@"..\..\..\Resources\License\license_FREE.lic", @"..\..\..\Resources\License\public.key");
            license_type = Karamba.Licenses.License.licenseType();
            Assert.True(license_type == feb.License.LicenseType.lic_student 
                        || license_type == feb.License.LicenseType.lic_developer
                        || license_type == feb.License.LicenseType.lic_trial
                        || license_type == feb.License.LicenseType.lic_free);
        }
#endif
    }
}
