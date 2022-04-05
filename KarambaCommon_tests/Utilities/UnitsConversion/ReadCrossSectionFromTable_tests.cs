#if ALL_TESTS

namespace KarambaCommon.Tests.CrossSections
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class ReadCrossSectionFromTable_tests
    {
        [Test]
        public void ImperialUnits()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            // make temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();

            var ini = INIReader.Instance();
            ini.Values["UnitsSystem"] = "imperial";

            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = Path.Combine(resourcePath, "CrossSectionValues.bin");
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out var info);
            var crosec_family = inCroSecs.crosecs.FindAll(x => x.family == "W");
            var crosec_initial = crosec_family.Find(x => x.name == "W12X26");

            // clear temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();

            var cs = crosec_initial as CroSec_I;

            var m2feet = 3.28084;
            var height_feet = 0.31 * m2feet;
            var width_feet = 0.165 * m2feet;
            Assert.That(height_feet, Is.EqualTo(cs._height).Within(1e-5));
            Assert.That(width_feet, Is.EqualTo(cs.lf_width).Within(1e-5));

            var feet42inch4 = Math.Pow(12.0, 4);
            var cm42inch4 = Math.Pow(1.0 / 2.54, 4);
            var iyy_inch1 = 8491.12 * cm42inch4;
            var iyy_inch2 = cs.Iyy * feet42inch4;
            Assert.That(iyy_inch2, Is.EqualTo(iyy_inch1).Within(1e-1));
        }
    }
}

#endif
