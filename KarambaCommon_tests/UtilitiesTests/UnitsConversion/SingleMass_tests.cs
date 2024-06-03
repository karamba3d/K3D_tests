#if ALL_TESTS

namespace KarambaCommon.Tests.UnitConversion
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
    // using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;

    [TestFixture]
    public class SingleMassTests
    {
        [Test]
        public void SinglemassEigenfrequency_SI()
        {
            // make temporary changes to the the ini-file and units-conversion
            UnitsConversionFactory.ClearSingleton();
            // INIReader.ClearSingleton();
            IniConfig.ReSet(); // clear changes.

            IniConfig.SetValue("UnitsSystem", UnitSystem.SI);

            double gravity = 9.81; // m/s2
            IniConfig.Gravity = gravity;
            IniConfig.SetValue("UnitLength", "ft");
            IniConfig.SetValue("UnitForce", "kN");

            var uc = UnitsConversionFactory.Conv();
            gravity = uc.gravity().toBase(); // cm/s2

            var k3d = new Toolkit();
            var e = uc["N/cm2"].toBase(70);
            var gamma = uc["N/cm3"].toBase(1.0);

            // input is not scaled to base units
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

            int b = 6; // (cm)
            int t = 3; // (cm)

            // input is scaled to base units
            var unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);

            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unit_crosec), };
            elems[0].bending_stiff = false;

            double l = uc["cm"].toBase(10.0);
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };

            var supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportFixedConditions),
                k3d.Support.Support(1, new List<bool>() { false, true, true, true, true, true }),
            };

            Model model = k3d.Model.AssembleModel(
                elems,
                supports,
                null,
                out string info,
                out double mass,
                out Point3 cog,
                out string msg,
                out bool runtimeWarning,
                new List<Joint>(),
                points);

            var forceToMass = uc.force2mass().toBase(1);
            var massTarget = unit_crosec.A * gamma * l * forceToMass;
            Assert.That(mass, Is.EqualTo(massTarget).Within(1e-5));

            // calculate the natural vibrations
            int from_shape_ind = 1;
            int shapes_num = 1;
            int max_iter = 100;
            double eps = 1e-8;
            var disp_dummy = new List<double>();
            var scaling = EigenShapesScalingType.matrix;
            model = k3d.Algorithms.NaturalVibes(
                model,
                from_shape_ind,
                shapes_num,
                max_iter,
                eps,
                disp_dummy,
                scaling,
                out List<double> nat_frequencies,
                out List<double> modal_masses,
                out List<Vector3> participation_facs,
                out List<double> participation_facs_disp,
                out model);

            var e_ = e; // N/cm2
            var a_ = unit_crosec.A; // cm2
            var l_ = l; // cm;
            var gamma_ = unit_material.gamma(); // N/cm3
            var c = e_ * a_ / l; // N / m
            var m = a_ * l_ * gamma_ * forceToMass * 0.5; // kg
            var omega0 = Math.Sqrt(c / m); // rad / sec
            var f0Target = omega0 / 2.0 / Math.PI; // 1 / sec = Hz
            var f0Calc = nat_frequencies[0];
            Assert.That(f0Target, Is.EqualTo(f0Calc).Within(1e-2));

            // clear temporary changes to the the ini-file and units-conversion
            // INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();
            IniConfig.ReSet();

            // switch back to SI units
            IniConfig.SetValue("UnitsSystem", UnitSystem.SI);
        }

        [Test]
        public void PointMassOnSpring_SI()
        {
            // make temporary changes to the the ini-file and units-conversion
            // INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();

            IniConfig.ReSet();
            IniConfig.SetValue("UnitsSystem", UnitSystem.SI);

            double gravity = 9.81; // m/s2
            IniConfig.Gravity = gravity;

            IniConfig.SetValue("UnitLength", "cm");
            IniConfig.SetValue("UnitForce", "N");

            var k3d = new Toolkit();

            var uc = UnitsConversionFactory.Conv();
            double c_trans = uc["N/cm"].toBase(50);
            var c_rot = uc["Ncm/rad"].toBase(50);
            var unitCrosec = k3d.CroSec.Spring(new double[] { c_trans, c_trans, c_trans, c_rot, c_rot, c_rot });
            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unitCrosec), };
            elems[0].bending_stiff = false;

            double l = uc["cm"].toBase(10.0);
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };

            var supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportFixedConditions),
                k3d.Support.Support(1, new List<bool>() { false, true, true, true, true, true }),
            };

            double m = uc["kg"].toBase(50); // kg
            var point_mass = new PointMass(1, m, 1);

            Model model = k3d.Model.AssembleModel(
                elems,
                supports,
                new List<Load>() { point_mass },
                out string info,
                out double mass,
                out Point3 cog,
                out string msg,
                out bool runtimeWarning,
                new List<Joint>(),
                points);

            // calculate the natural vibrations
            int from_shape_ind = 1;
            int shapes_num = 1;
            int max_iter = 100;
            double eps = 1e-8;
            var disp_dummy = new List<double>();
            var scaling = EigenShapesScalingType.matrix;
            model = k3d.Algorithms.NaturalVibes(
                model,
                from_shape_ind,
                shapes_num,
                max_iter,
                eps,
                disp_dummy,
                scaling,
                out List<double> nat_frequencies,
                out List<double> modal_masses,
                out List<Vector3> participation_facs,
                out List<double> participation_facs_disp,
                out model);

            double omega0 = Math.Sqrt(c_trans / m); // rad / sec
            double f0 = omega0 / 2.0 / Math.PI; // 1 / sec = Hz

            Assert.That(f0, Is.EqualTo(nat_frequencies[0]).Within(1e-2));

            // clear temporary changes to the the ini-file and units-conversion
            // INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();
            IniConfig.ReSet();

            // switch back to SI units
            IniConfig.SetValue("UnitsSystem", UnitSystem.SI);
        }
    }
}

#endif
