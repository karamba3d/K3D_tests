#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class MassTests
    {
        [Test]
        public void ModelMassImperialUnits()
        {
            // make temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();

            var ini = INIReader.Instance();
            ini.Values["UnitsSystem"] = "imperial";
            ini.Values["gravity"] = "9.80665";

            var k3d = new Toolkit();
            var gamma = 1.0; // (kip/ft3)

            var unit_material = new FemMaterial_Isotrop(
                "unit",
                "unit",
                1,
                0.5,
                0.5,
                gamma,
                1.0,
                -1.0,
                FemMaterial.FlowHypothesis.mises,
                1.0,
                null);
            var b = 12; // (inch)
            var t = 6; // (inch)
            var unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);

            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unit_crosec), };

            var l = 1.0; // in feet
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };

            k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out _,
                out var mass,
                out _,
                out _,
                out _,
                new List<Joint>(),
                points);

            // clear temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();
            ini = INIReader.Instance();

            // switch back to SI units
            ini.Values["UnitsSystem"] = "SI";

            var massTarget = unit_crosec.A * gamma * l;
            Assert.That(mass, Is.EqualTo(massTarget).Within(1e-10));
        }

        [Test]
        public void ModelMassSIUnits()
        {
            var k3d = new Toolkit();
            var gamma = 1.0; // (kN/m3)
            var unit_material = new FemMaterial_Isotrop(
                "unit",
                "unit",
                1,
                0.5,
                0.5,
                gamma,
                1.0,
                -1.0,
                FemMaterial.FlowHypothesis.mises,
                1.0,
                null);
            var b = 100; // (cm)
            var t = 50; // (cm)
            var unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);

            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unit_crosec), };

            var l = 1.0; // in m
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };

            k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out _,
                out var mass,
                out _,
                out _,
                out _,
                new List<Joint>(),
                points);

            var ucf = UnitsConversionFactory.Conv();
            mass = ucf.kg().toUnit(mass);

            Assert.That(mass, Is.EqualTo(100).Within(1e-10));
        }
    }
}

#endif