﻿#if ALL_TESTS

namespace KarambaCommon.Tests.Algorithms
{
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using PathUtil = Karamba.Utilities.PathUtil;

    [TestFixture]
    public class Buckling_tests
    {
        [Test]
        public void BucklingLength()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(0, 0, 0.5 * length);
            var p2 = new Point3(0, 0, length);
            var axis0 = new Line3(p0, p1);
            var axis1 = new Line3(p1, p2);

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { axis0, axis1 },
                new List<string>() { "B1" },
                new List<CroSec>() { },
                logger,
                out var out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, false, false, true }),
                k3d.Support.Support(p2, new List<bool>() { true, true, false, false, false, false }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                null,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            // disassemble the model
            k3d.Model.Disassemble(
                model,
                logger,
                out var points,
                out var lines,
                out var meshes,
                out beams,
                out var shells,
                out supports,
                out var loads,
                out var materials,
                out var crosecs,
                out var joints);

            // check the buckling length,; a negative value means that the length was autogenerated
            Assert.That(-length, Is.EqualTo(beams[0].BucklingLength(BuilderElementStraightLine.BucklingDir.bklY)).Within(1E-10));
            Assert.That(-length, Is.EqualTo(beams[1].BucklingLength(BuilderElementStraightLine.BucklingDir.bklY)).Within(1E-10));
        }

        [Test]
        [NonParallelizable] // don't help
        [System.Obsolete]
        public void Column()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(0, 0, length);
            var axis = new Line3(p0, p1);

            // get a material from the material table in the folder 'Resources'
            string materialPath = PathUtil.MaterialPropertiesFile();
            List<Karamba.Materials.FemMaterial> inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            Karamba.Materials.FemMaterial material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            string crosecPath = PathUtil.CrossSectionValuesFile();
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out string info);
            List<CroSec> crosec_family = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            CroSec crosec_initial = crosec_family.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            crosec_initial.setMaterial(material);

            // create the column
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec_initial },
                logger,
                out List<Point3> out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, false, false, true }),
                k3d.Support.Support(p1, new List<bool>() { true, true, false, false, false, false }),
            };

            // create a Point-load
            var loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(0, 0, -100)), };

            // create the model
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out info,
                out double mass,
                out Point3 cog,
                out string message,
                out bool warning);

            // calculate Th.I response
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out IReadOnlyList<double> out_max_disp,
                out IReadOnlyList<double> out_g,
                out IReadOnlyList<double> out_comp,
                out message);

            // optimize the cross section
            model = k3d.Algorithms.OptiCroSec(
                model,
                crosec_family,
                out IReadOnlyList<double> maxDisplacements,
                out IReadOnlyList<double> compliances,
                out message);

            // disassemble the model
            k3d.Model.Disassemble(
                model,
                logger,
                out List<Point3> points,
                out List<Line3> lines,
                out List<IMesh> meshes,
                out beams,
                out List<BuilderShell> shells,
                out supports,
                out loads,
                out List<Karamba.Materials.FemMaterial> materials,
                out List<CroSec> crosecs,
                out List<Karamba.Joints.Joint> joints);

            // check the buckling length,; a negative value means that the length was autogenerated
            Assert.That(-length, Is.EqualTo(beams[0].BucklingLength(BuilderElementStraightLine.BucklingDir.bklY)).Within(1E-10));

            // check the resulting cross section
            Assert.That(-length, Is.EqualTo(beams[0].BucklingLength(BuilderElementStraightLine.BucklingDir.bklY)).Within(1E-10));
            Assert.That(beams[0].crosec.name, Is.EqualTo("FRQ70/6"));
        }
    }
}

#endif
