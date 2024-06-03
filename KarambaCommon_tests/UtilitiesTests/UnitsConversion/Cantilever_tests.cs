#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    // using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;

    [TestFixture]
    public class CantileverTests
    {
        [Test]
        public void AxialVibration()
        {
            // make temporary changes to the the ini-file and units-conversion
            IniConfig.ReSet();
            UnitsConversionFactory.ClearSingleton();

            IniConfig.SetValue("UnitsSystem", UnitSystem.imperial);
            IniConfig.DefaultGravity();

            var k3d = new Toolkit();
            var e = 70000;   // (kip/ft2) == 486111.1 (psi)
            var gamma = 1.0; // (kip/ft3)
            var unit_material = new FemMaterial_Isotrop(
                "unit",
                "unit",
                e,
                0.5 * e,
                0.5 * e,
                gamma,
                1.0,
                -1.0,
                FemMaterial.FlowHypothesis.mises,
                1.0,
                null,
                out string _);
            var b = 6; // (inch)
            var t = 3; // (inch)

            // Iy = 108 inch^4
            // g = 20.833 lb/inch
            var unitCrosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);

            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unitCrosec), };

            var l = 10.0; // in feet
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };

            var supports = new List<Support>
            {
                k3d.Support.Support(points[0], new List<bool>() { false, true, true, true, true, true }),
                k3d.Support.Support(points[1], new List<bool>() { true, true, true, true, true, true }),
            };

            var model = k3d.Model.AssembleModel(
                elems,
                supports,
                null,
                out _,
                out double massStructure_base_units,
                out _,
                out _,
                out _,
                new List<Joint>(),
                points);

            var ucf = UnitsConversionFactory.Conv();
            var massStructure = ucf.kg().toUnit(massStructure_base_units) / 1000;

            double massTarg = l * unitCrosec.A * gamma;
            Assert.That(massStructure, Is.EqualTo(massTarg).Within(1e-2));

            // calculate the natural vibrations
            int from_shape_ind = 1;
            int shapes_num = 1;
            int max_iter = 100;
            double eps = 1e-8;
            var disp_dummy = new List<double>();
            var scaling = EigenShapesScalingType.matrix;
            k3d.Algorithms.NaturalVibes(
                model,
                from_shape_ind,
                shapes_num,
                max_iter,
                eps,
                disp_dummy,
                scaling,
                out var nat_frequencies,
                out _,
                out _,
                out _,
                out _);

            var A = unitCrosec.A;
            var c = (e * A) / l;
            var m = massStructure_base_units / 3.0;
            var f1Expected = Math.Sqrt(c / m) / (2.0 * Math.PI);
            var f1Calculated = nat_frequencies[0];
            Assert.That(f1Expected, Is.EqualTo(f1Calculated).Within(1e-2));

            // clear temporary changes to the the ini-file and units-conversion
            IniConfig.ReSet();
            UnitsConversionFactory.ClearSingleton();

            // switch back to SI units
            IniConfig.SetValue("UnitsSystem", UnitSystem.SI);
        }

        [Test]
        public void CantileverEigenfrequency()
        {
            // make temporary changes to the the ini-file and units-conversion
            IniConfig.ReSet();
            UnitsConversionFactory.ClearSingleton();

            IniConfig.SetValue("UnitsSystem", UnitSystem.imperial);
            IniConfig.DefaultGravity();

            var k3d = new Toolkit();
            int e = 70000; // (kip/ft2) == 486111.1 (psi)
            double gamma = 1.0; // (kip/ft3)
            var unit_material = new FemMaterial_Isotrop(
                "unit",
                "unit",
                e,
                0.5 * e,
                0.5 * e,
                gamma,
                1.0,
                -1.0,
                FemMaterial.FlowHypothesis.mises,
                1.0,
                null,
                out string _);
            int b = 6; // (inch)
            int t = 3; // (inch)

            // Iy = 108 inch^4
            // g = 20.833 lb/inch
            CroSec_Box unitCrosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);

            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unitCrosec), };

            double l = 10.0; // in feet
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };

            var supports = new List<Support> { k3d.Support.Support(0, k3d.Support.SupportFixedConditions) };

            Model model = k3d.Model.AssembleModel(
                elems,
                supports,
                null,
                out _,
                out double massStructure_base_units,
                out _,
                out _,
                out _,
                new List<Joint>(),
                points);

            var ucf = UnitsConversionFactory.Conv();
            var massStructure = ucf.kg().toUnit(massStructure_base_units) / 1000;

            double massTarg = l * unitCrosec.A * gamma;
            Assert.That(massStructure, Is.EqualTo(massTarg).Within(1e-2));

            // calculate the natural vibrations
            int from_shape_ind = 1;
            int shapes_num = 1;
            int max_iter = 100;
            double eps = 1e-8;
            var disp_dummy = new List<double>();
            var scaling = EigenShapesScalingType.matrix;
            k3d.Algorithms.NaturalVibes(
                model,
                from_shape_ind,
                shapes_num,
                max_iter,
                eps,
                disp_dummy,
                scaling,
                out List<double> nat_frequencies,
                out _,
                out _,
                out _,
                out _);

            // calculate the expected value of the first eigen-frequency
            // https://amesweb.info/Vibration/Cantilever-Beam-Natural-Frequency-Calculator.aspx
            var kn = 1.875 * 1.875;
            var g = 9.80665 / 0.3048;
            var f1Expected = kn / 2.0 / Math.PI * Math.Sqrt(e * unitCrosec.Iyy / unitCrosec.A / gamma * g / Math.Pow(l, 4));
            var f1Calculated = nat_frequencies[0];
            Assert.That(f1Expected, Is.EqualTo(f1Calculated).Within(1e-2));

            // clear temporary changes to the the ini-file and units-conversion
            IniConfig.ReSet();
            UnitsConversionFactory.ClearSingleton();

            // switch back to SI units
            IniConfig.SetValue("UnitsSystem", UnitSystem.SI);
        }

        [Test]
        public void ModelMassSIUnits()
        {
            var k3d = new Toolkit();
            double gamma = 1.0; // (kN/m3)
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
                null,
                out string _);
            int b = 100; // (cm)
            int t = 50; // (cm)
            CroSec_Box unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);

            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unit_crosec), };

            double l = 1.0; // in m
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };
            k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out _,
                out double mass,
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
