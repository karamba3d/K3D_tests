#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
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
    public class SingleMassTests
    {
        [Test]
        public void SinglemassEigenfrequency_SI()
        {
            // make temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();

            var ini = INIReader.Instance();
            ini.Values["UnitsSystem"] = "SI";
            ini.Values["UnitsSystem"] = "SI";

            var gravity = 9.81; // m/s2
            ini.Values["gravity"] = gravity.ToString(CultureInfo.InvariantCulture);
            ini.Values["UnitLength"] = "ft";
            ini.Values["UnitForce"] = "MN";
            ini.Values["UnitMass"] = "t";

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
                null);

            var b = 6; // (cm)
            var t = 3; // (cm)

            // input is scaled to base units
            var unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);

            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unit_crosec), };
            elems[0].bending_stiff = false;

            var l = uc["cm"].toBase(10.0);
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };

            var supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportFixedConditions),
                k3d.Support.Support(1, new List<bool>() { false, true, true, true, true, true }),
            };

            var model = k3d.Model.AssembleModel(
                elems,
                supports,
                null,
                out var info,
                out var mass,
                out var cog,
                out var msg,
                out var runtimeWarning,
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
                out var nat_frequencies,
                out var modal_masses,
                out var participation_facs,
                out var participation_facs_disp,
                out model);

            var e_ = e; // N/cm2
            var a_ = unit_crosec.A; // cm2
            var l_ = l; // cm;
            var gamma_ = unit_material.gamma(); // N/cm3
            var c = e_ * a_ / l; // N / m
            var m = a_ * l_ * gamma_ / gravity * 0.5; // kg
            var omega0 = Math.Sqrt(c / m); // rad / sec
            var f0 = omega0 / 2.0 / Math.PI; // 1 / sec = Hz

            Assert.That(f0, Is.EqualTo(nat_frequencies[0]).Within(1e-2));

            // clear temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();
            ini = INIReader.Instance();

            // switch back to SI units
            ini.Values["UnitsSystem"] = "SI";
        }

        [Test]
        public void PointMassOnSpring_SI()
        {
            // make temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();

            var ini = INIReader.Instance();
            ini.Values["UnitsSystem"] = "SI";
            ini.Values["UnitsSystem"] = "SI";

            var gravity = 9.81; // m/s2
            ini.Values["gravity"] = gravity.ToString(CultureInfo.InvariantCulture);

            ini.Values["UnitLength"] = "cm";
            ini.Values["UnitForce"] = "N";
            ini.Values["UnitMass"] = "t";

            var k3d = new Toolkit();

            var uc = UnitsConversionFactory.Conv();
            var c_trans = uc["N/cm"].toBase(50);
            var c_rot = uc["Ncm/rad"].toBase(50);
            var unitCrosec = k3d.CroSec.Spring(new double[] { c_trans, c_trans, c_trans, c_rot, c_rot, c_rot });
            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unitCrosec), };
            elems[0].bending_stiff = false;

            var l = uc["cm"].toBase(10.0);
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };

            var supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportFixedConditions),
                k3d.Support.Support(1, new List<bool>() { false, true, true, true, true, true }),
            };

            var m = uc["kg"].toBase(50); // kg
            var point_mass = new PointMass(1, m, 1);

            var model = k3d.Model.AssembleModel(
                elems,
                supports,
                new List<Load>() { point_mass },
                out var info,
                out var mass,
                out var cog,
                out var msg,
                out var runtimeWarning,
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
                out var nat_frequencies,
                out var modal_masses,
                out var participation_facs,
                out var participation_facs_disp,
                out model);

            var omega0 = Math.Sqrt(c_trans / m); // rad / sec
            var f0 = omega0 / 2.0 / Math.PI; // 1 / sec = Hz

            Assert.That(f0, Is.EqualTo(nat_frequencies[0]).Within(1e-2));

            // clear temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();
            ini = INIReader.Instance();

            // switch back to SI units
            ini.Values["UnitsSystem"] = "SI";
        }
    }
}

#endif