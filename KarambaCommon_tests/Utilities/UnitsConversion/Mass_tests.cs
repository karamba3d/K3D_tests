using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Joints;
using Karamba.Supports;
using Karamba.Models;
using Karamba.Utilities;
using Karamba.Algorithms;
using Karamba.Materials;

namespace KarambaCommon.Tests.Model
{
    [TestFixture]
    public class MassTests
    {
#if ALL_TESTS
        [Test]
        public void ModelMassImperialUnits()
        {
            // make temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactories.ClearSingleton();

            var ini = INIReader.Instance();
            ini.Values["UnitsSystem"] = "imperial";
            ini.Values["gravity"] = "9.80665";

            var k3d = new Toolkit();
            var gamma = 1.0; // (kip/ft3)
            var unit_material = new FemMaterial_Isotrop("unit", "unit", 1, 0.5, 0.5, gamma, 1.0, 1.0, null);
            var b = 12; // (inch)
            var t = 6; // (inch)
            var unit_crosec = k3d.CroSec.Box(b,b,b,t,t,t,0,0,unit_material);

            var elems = new List<BuilderBeam>() {
                k3d.Part.IndexToBeam(0, 1, "A", unit_crosec),
            };

            var L = 1.0; // in feet
            var points = new List<Point3> { new Point3(), new Point3(L, 0, 0) };

            var model = k3d.Model.AssembleModel(elems, null, null, out var info, out var mass, out var cog, out var msg,
                out var runtimeWarning, new List<Joint>(), points);

            // clear temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactories.ClearSingleton();
            ini = INIReader.Instance();
            // switch back to SI units
            ini.Values["UnitsSystem"] = "SI";

            Assert.AreEqual(mass, 1000, 1e-10);
        }

        [Test]
        public void ModelMassSIUnits()
        {
            var k3d = new Toolkit();
            var gamma = 1.0; // (kN/m3)
            var unit_material = new FemMaterial_Isotrop("unit", "unit", 1, 0.5, 0.5, gamma, 1.0, 1.0, null);
            var b = 100; // (cm)
            var t = 50; // (cm)
            var unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);

            var elems = new List<BuilderBeam>() {
                k3d.Part.IndexToBeam(0, 1, "A", unit_crosec),
            };

            var L = 1.0; // in m
            var points = new List<Point3> { new Point3(), new Point3(L, 0, 0) };

            var model = k3d.Model.AssembleModel(elems, null, null, out var info, out var mass, out var cog, out var msg,
                out var runtimeWarning, new List<Joint>(), points);

            Assert.AreEqual(mass, 100, 1e-10);
        }
#endif
    }
}
