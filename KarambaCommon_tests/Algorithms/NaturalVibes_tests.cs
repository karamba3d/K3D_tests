#if ALL_TESTS

namespace KarambaCommon.Tests.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class NaturalVibes_tests
    {
        [Test]
        public void Beam()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            // get a material from the material table in the folder 'Resources'
            var materialPath = Path.Combine(resourcePath, "MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = Path.Combine(resourcePath, "CrossSectionValues.bin");
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out var info);
            var crosec_family = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            var crosec_initial = crosec_family.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            crosec_initial.setMaterial(material);

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec_initial },
                logger,
                out var out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, false, false }),
                k3d.Support.Support(p1, new List<bool>() { false, true, true, false, false, false }),
            };

            // create a Point-load
            var loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(0, 0, -100)), };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

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

            Assert.That(nat_frequencies[0], Is.EqualTo(8.9828263788644716).Within(1e-8));
        }

        [Test]
        public void Shell()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 1.0;
            var mesh = new Mesh3();
            var p0 = new Point3(0, length, 0);
            var p1 = new Point3(0, 0, 0);
            mesh.AddVertex(p0);
            mesh.AddVertex(p1);
            mesh.AddVertex(new Point3(length, 0, 0));
            mesh.AddVertex(new Point3(length, length, 0));
            mesh.AddFace(new Face3(0, 1, 3));
            mesh.AddFace(new Face3(1, 2, 3));

            var crosec = k3d.CroSec.ShellConst(25, 0);
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out _);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
                k3d.Support.Support(p1, new List<bool>() { true, true, true, true, true, true }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                shells,
                supports,
                new List<Load>(),
                out _,
                out _,
                out _,
                out _,
                out _);

            // calculate the natural vibrations
            int from_shape_ind = 1;
            int shapes_num = 1;
            int max_iter = 100;
            double eps = 1e-8;
            var disp_dummy = new List<double>();
            var scaling = EigenShapesScalingType.matrix;
            _ = k3d.Algorithms.NaturalVibes(
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
                out List<double> _,
                out _);

            Assert.That(nat_frequencies[0], Is.EqualTo(624.44918408731951).Within(1e-8));
        }

        [Test]
        public void BeamWithTwoPointMassesAtOneNode()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            double kNCm2ToKNM2 = 10000;
            var material = k3d.Material.IsotropicMaterial(
                "MaterialFamily",
                "MaterialName",
                21000 * kNCm2ToKNM2,
                10500 * kNCm2ToKNM2,
                10500 * kNCm2ToKNM2,
                0,
                10,
                10,
                FemMaterial.FlowHypothesis.mises,
                0);

            var crosec = k3d.CroSec.CircularHollow(10, 5, material, "CroSecFamily", "CroSecName", string.Empty);

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out var out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-mass
            double m = 100; // [kg]
            var loads = new List<Load>
            {
                k3d.Load.PointMass(p1, m * 0.5, 0.1),
                k3d.Load.PointMass(p1, m * 0.5, 0.1),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

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

            double c = 1 / (Math.Pow(length, 3) / (3.0 * material.E() * crosec.Iyy));
            double omega = Math.Sqrt(c / m);
            double f_tar = omega / 2 / Math.PI;
            double f_res = nat_frequencies[0];
            Assert.That(f_tar, Is.EqualTo(f_res).Within(1e-4));
        }

        [Test]
        public void SpringWithPointMassesSI()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 1.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            var c = 100; // [kN/m]
            var crosec = k3d.CroSec.Spring(new double[] { c, c, c, 0, 0, 0 });

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out var outPoints);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
                k3d.Support.Support(p1, new List<bool>() { false, true, true, true, true, true }),
            };

            // create a Point-mass
            double m = 100000; // [kg]
            var ucf = UnitsConversionFactory.Conv();
            var kg = ucf.kg();
            double mConverted = kg.toBase(m);
            var loads = new List<Load>
            {
                k3d.Load.PointMass(p1, mConverted, 0.1),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

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

            double omega = Math.Sqrt(c / mConverted);
            double fTar = omega / 2 / Math.PI;
            double fRes = nat_frequencies[0];
            Assert.That(fTar, Is.EqualTo(fRes).Within(1e-4));
        }

        // [Test]
        public void SpringWithPointMassesImperial()
        {
            var ini_reader = INIReader.Instance();
            ini_reader.Values["UnitsSystem"] = "imperial";
            ini_reader.Values["UnitLength"] = "ft";
            ini_reader.Values["UnitForce"] = "kipf";
            ini_reader.Values["UnitMass"] = "kipm";
            ini_reader.Values["gravity"] = "9.80665";
            UnitsConversionFactory.ClearSingleton();

            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 1.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            var c = 100; // [kip/ft]
            var crosec = k3d.CroSec.Spring(new double[] { c, c, c, 0, 0, 0 });

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out var outPoints);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
                k3d.Support.Support(p1, new List<bool>() { false, true, true, true, true, true }),
            };

            // create a Point-mass
            double mass = 100; // [lb]
            var ucf = UnitsConversionFactory.Conv();
            var kg = ucf.kg();
            double massConverted = kg.toBase(mass);
            var loads = new List<Load>
            {
                k3d.Load.PointMass(p1, massConverted, 0.1),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var info,
                out var modelMass,
                out var cog,
                out var message,
                out var warning);

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

            double omega = Math.Sqrt(c / massConverted);
            double fTar = omega / 2 / Math.PI;
            double fRes = nat_frequencies[0];
            Assert.That(fTar, Is.EqualTo(fRes).Within(1e-4));

            ini_reader.Values["UnitsSystem"] = "SI";
            ini_reader.Values["UnitLength"] = "m";
            ini_reader.Values["UnitForce"] = "kN";
            ini_reader.Values["UnitMass"] = "t";
            ini_reader.Values["gravity"] = "10";
            UnitsConversionFactory.ClearSingleton();
        }

        // [Test]
        public void BeamWithPointMassesImperial()
        {
            var ini_reader = INIReader.Instance();
            ini_reader.Values["UnitsSystem"] = "imperial";
            ini_reader.Values["UnitLength"] = "ft";
            ini_reader.Values["UnitForce"] = "kipf";
            ini_reader.Values["UnitMass"] = "kipm";
            ini_reader.Values["gravity"] = "9.80665";
            UnitsConversionFactory.ClearSingleton();

            var k3d = new Toolkit();
            var logger = new MessageLogger();

            double E = 21000; // kip/ft2
            double gamma = 10; // kip/ft3
            var material = k3d.Material.IsotropicMaterial(
                "MaterialFamily",
                "MaterialName",
                21000,
                10500,
                10500,
                gamma,
                10,
                10,
                FemMaterial.FlowHypothesis.mises,
                0);

            double diameter = 10; // in;
            var crosec = k3d.CroSec.CircularHollow(diameter, diameter * 0.5, material, "CroSecFamily", "CroSecName", string.Empty);
            crosec.Iyy = 1E2;
            crosec.Izz = 1E2;
            crosec.Ipp = 1E2;

            double length = 1.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out var outPoints);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
                k3d.Support.Support(p1, new List<bool>() { false, true, true, true, true, true }),
            };

            // create a Point-mass
            double mass = 100; // [lb]
            var ucf = UnitsConversionFactory.Conv();
            var kg = ucf.kg();
            double massConverted = kg.toBase(mass);
            var loads = new List<Load>
            {
                k3d.Load.PointMass(p1, massConverted, 0.1),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var info,
                out var modelMass,
                out var cog,
                out var message,
                out var warning);

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

            double A = diameter * diameter * Math.PI / 4.0 / 144; // ft^2
            double c = E * A / length;
            double totalMass = massConverted + A * length * gamma * 140.0 / 420.0;
            double omega = Math.Sqrt(c / totalMass);
            double fTar = omega / 2 / Math.PI;
            double fRes = nat_frequencies[0];
            Assert.That(fTar, Is.EqualTo(fRes).Within(1e-4));

            ini_reader.Values["UnitsSystem"] = "SI";
            ini_reader.Values["UnitLength"] = "m";
            ini_reader.Values["UnitForce"] = "kN";
            ini_reader.Values["UnitMass"] = "t";
            ini_reader.Values["gravity"] = "10";
            UnitsConversionFactory.ClearSingleton();
        }
    }
}

#endif