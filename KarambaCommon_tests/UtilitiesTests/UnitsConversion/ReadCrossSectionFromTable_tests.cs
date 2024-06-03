#if ALL_TESTS

namespace KarambaCommon.Tests.CrossSections
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Karamba.CrossSections;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;
    using Helper = KarambaCommon.Tests.Helpers.Helper;

    [TestFixture]
    public class ReadCrossSectionFromTable_tests
    {
        [Test]
        public void ImperialUnits()
        {
            Helper.InitIniConfigTest(UnitSystem.imperial, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();

            // make temporary changes to the the ini-file and units-conversion
            UnitsConversionFactory.ClearSingleton();

            Trace.WriteLine($"us: {IniConfig["UnitsSystem"].Value}");
            // throw new Exception(IniConfig["UnitsSystem"].Value);

            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            // ^- XXX move to PathUtil.

            // get a cross section from the cross section table in the folder 'Resources'
            string crosecPath = PathUtil.CrossSectionValuesFile();
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out string info);
            List<CroSec> crosec_family = inCroSecs.crosecs.FindAll(x => x.family == "W");
            CroSec crosec_initial = crosec_family.Find(x => x.name == "W12X26");

            // clear temporary changes to the the ini-file and units-conversion
            IniConfig.ReSet();
            IniConfig.SetValue("UnitsSystem", UnitSystem.SI);
            UnitsConversionFactory.ClearSingleton();

            var cs = crosec_initial as CroSec_I;

            double m2feet = 3.28084;
            double height_feet = 0.31 * m2feet;
            double width_feet = 0.165 * m2feet;
            Assert.Multiple(() =>
            {
                Assert.That(height_feet, Is.EqualTo(cs._height).Within(1e-5));
                Assert.That(width_feet, Is.EqualTo(cs.lf_width).Within(1e-5));
            });
            double feet42inch4 = Math.Pow(12.0, 4);
            double cm42inch4 = Math.Pow(1.0 / 2.54, 4);
            double iyy_inch1 = 8491.12 * cm42inch4;
            double iyy_inch2 = cs.Iyy * feet42inch4;
            Assert.That(iyy_inch2, Is.EqualTo(iyy_inch1).Within(1e-1));
            IniConfig.ReSet();
        }
    }
}

#endif
