#if ALL_TESTS

namespace KarambaCommon.Tests.Algorithms
{
    using System.Collections.Generic;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using PathUtil = Karamba.Utilities.PathUtil;

    [TestFixture]
    public class Eigenmodes_tests
    {
        [Test]
        public void BeamWithLoadsCalculated()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            // get a material from the material table in the folder 'Resources'
            string materialPath = PathUtil.MaterialPropertiesFile();
            List<FemMaterial> inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            FemMaterial material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            string crosecPath = PathUtil.CrossSectionValuesFile();
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out string info);
            List<CroSec> crosec_family = inCroSecs.crosecs.FindAll(x => x.family == "RRO(EN10219-2)");
            CroSec crosec_initial = crosec_family.Find(x => x.name == "RHS 40x20x2");

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
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-load
            // List<Load> loads = new List<Load> { };
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

            // if calculated, load case results come before eigen-modes results
            // analyze clears all existing load-case results
            model = k3d.Algorithms.Analyze(
                model,
                new List<string> { "LC0" },
                out _,
                out _,
                out _,
                out _);

            // calculate the natural vibrations
            int from_shape_ind = 1;
            int shapes_num = 2;
            int max_iter = 100;
            double eps = 1e-8;

            // determine smallest positive and negative eigenvalues
            int sign = 0;
            var disp_dummy = new List<double>();
            EigenShapesScalingType scaling = EigenShapesScalingType.matrix;
            model = k3d.Algorithms.Eigenmodes(
                model,
                from_shape_ind,
                shapes_num,
                max_iter,
                eps,
                sign,
                scaling,
                out List<double> eigenValues,
                out model);

            Assert.That(eigenValues[0], Is.EqualTo(0.12267429652500436).Within(1e-8));
            double uy0 = model.febmodel.element(0).interiorState(model.febmodel, 1, 1.0, true).disp_local((int)feb.Node.DOF.y_t);
            Assert.That(uy0, Is.EqualTo(-0.082955011956547739).Within(1e-8));
            double uy1 = model.febmodel.element(0).interiorState(model.febmodel, 2, 1.0, true).disp_local((int)feb.Node.DOF.y_t);
            Assert.That(uy1, Is.EqualTo(0).Within(1e-8));
        }

        [Test]
        public void BeamWithoutLoadsCalculated()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            // get a material from the material table in the folder 'Resources'
            string materialPath = PathUtil.MaterialPropertiesFile();
            List<FemMaterial> inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            FemMaterial material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            string crosecPath = PathUtil.CrossSectionValuesFile();
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out string info);
            List<CroSec> crosec_family = inCroSecs.crosecs.FindAll(x => x.family == "RRO(EN10219-2)");
            CroSec crosec_initial = crosec_family.Find(x => x.name == "RHS 40x20x2");

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
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-load
            // List<Load> loads = new List<Load> { };
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

            // without calculated load-case results  eigen-modes results come first

            // calculate the natural vibrations
            int from_shape_ind = 1;
            int shapes_num = 2;
            int max_iter = 100;
            double eps = 1e-8;

            // determine smallest positive and negative eigenvalues
            int sign = 0;
            var disp_dummy = new List<double>();
            EigenShapesScalingType scaling = EigenShapesScalingType.matrix;

            model = k3d.Algorithms.Eigenmodes(
                model,
                from_shape_ind,
                shapes_num,
                max_iter,
                eps,
                sign,
                scaling,
                out List<double> eigenValues,
                out model);

            Assert.That(eigenValues[0], Is.EqualTo(0.12267429652500436).Within(1e-8));
            double uy0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 1.0, true).disp_local((int)feb.Node.DOF.y_t);
            Assert.That(uy0, Is.EqualTo(-0.082955011956547739).Within(1e-8));
            double uy1 = model.febmodel.element(0).interiorState(model.febmodel, 1, 1.0, true).disp_local((int)feb.Node.DOF.y_t);
            Assert.That(uy1, Is.EqualTo(0).Within(1e-8));
        }
    }
}

#endif
