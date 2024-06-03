#if ALL_TESTS

namespace KarambaCommon.Tests.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Combination;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using PathUtil = Karamba.Utilities.PathUtil;

    [TestFixture]
    public class CalculationFlow_tests
    {
        [Test]
        public void Cantilever()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            // get a cross section from the cross section table in the folder 'Resources'
            double height = 20;
            double width = 10;
            var crosec = new CroSec_Trapezoid(
                "family",
                "name",
                "country",
                null,
                Material_Default.Instance().steel,
                height,
                width,
                width);
            crosec.Az = 1E10; // make it stiff in shear

            // create the beam
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out List<Point3> out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create two Point-loads in load-case-combinations 'LCA' and 'LCB'
            int f = 10;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(p1, new Vector3(0, 0, -f), new Vector3(), "LCA"),
                k3d.Load.PointLoad(p1, new Vector3(-f * 100, 0, -f), new Vector3(), "LCB"),
                k3d.Load.PointLoad(p1, new Vector3(f, 0, 0), new Vector3(), "LCC"),
                k3d.Load.LoadCaseOptions("LCA", false),
                k3d.Load.LoadCaseOptions("LCB", false, true, false, 1e-7, 50),
                k3d.Load.LoadCaseOptions("LCC", false),
            };

            // create the model
            var inModel = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            var loadCaseCombinations = inModel.lcCombinationCollection
                .SelectLoadCaseCombinations(new List<string>() { "&LC." }).ToList();
            inModel.lcActivation = new LoadCaseActivation(loadCaseCombinations);
            var calculationFlow = inModel.lcActivation.CalculationFlow;

            Assert.That(calculationFlow.CalculationSteps.Count, Is.EqualTo(2));

            calculationFlow.Execute(inModel, inModel.lcActivation, out Model outModel, out var warnings);

            var e = crosec.material.E();
            var i = crosec.Iyy;
            var maxDispTarg0 = f * Math.Pow(length, 3) / 3 / e / i;

            var area = height * width / 10000;
            var maxDispTarg1 = f / area / e * length;

            // the system buckles
            var nbkl = Math.PI * Math.PI * e * crosec.Izz / length / length / 4;
            Assert.That(warnings, Has.Count.EqualTo(1));

            var maxDispTarg2 = 0.028318114896055912;

            Assert.That(calculationFlow.LCMaxDisp[0], Is.EqualTo(maxDispTarg0).Within(1E-10));
            Assert.That(calculationFlow.LCMaxDisp[1], Is.EqualTo(maxDispTarg1).Within(1E-10));
            Assert.That(calculationFlow.LCMaxDisp[2], Is.EqualTo(maxDispTarg2).Within(1E-10));

            var maxDisps = calculationFlow.LCMaxDisp;
        }

        [Test]
        public void AnalyzeAfterEigenmodes()
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
            var fz = -100;
            var loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(0, 0, fz)) };

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

            Assert.Multiple(() => {
                Assert.That(eigenValues[0], Is.EqualTo(0.12267429652500436).Within(1e-8));
                double uy0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 1.0, true).disp_local((int)feb.Node.DOF.y_t);
                Assert.That(uy0, Is.EqualTo(-0.082955011956547739).Within(1e-8));
                double uy1 = model.febmodel.element(0).interiorState(model.febmodel, 1, 1.0, true).disp_local((int)feb.Node.DOF.y_t);
                Assert.That(uy1, Is.EqualTo(0).Within(1e-8));
            });
            var activeLCCs = model.lcActivation.LoadCaseCombinationNames.ToArray();

            // if calculated, load case results come before eigen-modes results
            // analyze clears all existing load-case results
            model = k3d.Algorithms.Analyze(
                model,
                new List<string> { "LC0" },
                out _,
                out _,
                out _,
                out _);

            double i = (model.elems[0].crosec as CroSec_Beam).Iyy;
            double e = model.elems[0].crosec.material.E();
            double uz2Target = -Math.Abs(fz) * length * length * length / (3 * e * i);
            double uz2 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).disp_local((int)feb.Node.DOF.z_t);
            Assert.That(uz2, Is.EqualTo(uz2Target).Within(1e-1));
            activeLCCs = model.lcActivation.LoadCaseCombinationNames.ToArray();
        }
    }
}

#endif
