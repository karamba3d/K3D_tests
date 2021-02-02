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
            Assert.True(license_type == feb.LicenseType.lic_trial ||
                        license_type == feb.LicenseType.lic_developer ||
                        license_type == feb.LicenseType.lic_student);

            // load a new license 
            Karamba.Licenses.License.unload();

            Assert.True(File.Exists(@"C:\temp\LicenseTest\license.lic") && File.Exists(@"C:\temp\LicenseTest\public.key"),
                @"In order that the tests work place 'license.lic' and 'public.key' under 'C:\temp\LicenseTest\'");
            
            // set the path to where you saved your license file
            Karamba.Licenses.License.getLicense(@"C:\temp\LicenseTest\license.lic", @"C:\temp\LicenseTest\public.key");
            license_type = Karamba.Licenses.License.licenseType();
            Assert.True(license_type == feb.LicenseType.lic_student || license_type == feb.LicenseType.lic_developer || license_type == feb.LicenseType.lic_trial);
        }
#endif
    }
}
