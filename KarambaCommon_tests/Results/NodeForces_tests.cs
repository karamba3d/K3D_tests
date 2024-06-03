#if ALL_TESTS

namespace KarambaCommon.Tests.Result
{
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Elements.States;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Results;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;

    [TestFixture]
    public class NodeForces_tests
    {
        [Test]
        public void FrameCorner()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            double l = 4.0;
            Point3 p0 = new Point3(-l, 0, 0);
            Point3 p1 = new Point3(0, 0, l);
            Point3 p2 = new Point3(l, 0, 0);
            Line3 line1 = new Line3(p0, p1);
            Line3 line2 = new Line3(p1, p2);

            // get a material from the material table in the folder 'Resources'
            string materialPath = PathUtil.MaterialPropertiesFile();
            List<FemMaterial> inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            FemMaterial material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            string crosecPath = PathUtil.CrossSectionValuesFile();
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out string info);
            List<CroSec> croSecFamily = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            CroSec croSecInitial = croSecFamily.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            croSecInitial.setMaterial(material);

            // create the column
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { line1, line2 },
                new List<string>() { "B1" },
                new List<CroSec>() { croSecInitial },
                logger,
                out List<Point3> out_points);

            // create supports
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, false, true }),
                k3d.Support.Support(p2, new List<bool>() { true, true, true, true, false, true }),
            };

            // create a Point-load
            double fx = 10;
            List<Load> loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(fx, 0, 0), new Vector3()) };

            // create the model
            Model model = k3d.Model.AssembleModel(beams, supports, loads, out info, out double mass, out Point3 cog, out string message, out bool warning);
            Model calcModel = k3d.Algorithms.AnalyzeThI(model, out IReadOnlyList<double> displacement, out IReadOnlyList<double> gravities, out IReadOnlyList<double> energies, out string warnings);

            NodeForces.solve(calcModel, 1, new Plane3(), "LC0", false, false, out List<NodeForces.NodeForcesResult> nodeForcesResults);
            double sumX = nodeForcesResults[0].Forces[0].X + nodeForcesResults[1].Forces[0].X + fx;
            double sumZ = nodeForcesResults[0].Forces[0].Z + nodeForcesResults[1].Forces[0].Z;
            Assert.That(sumX, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumZ, Is.EqualTo(0.0).Within(1E-8));
        }

        [Test]
        public void FrameCornerOriented()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            double l = 4.0;
            Point3 p0 = new Point3(-l, 0, 0);
            Point3 p1 = new Point3(0, 0, l);
            Point3 p2 = new Point3(l, 0, 0);
            Line3 line1 = new Line3(p0, p1);
            Line3 line2 = new Line3(p1, p2);

            // get a material from the material table in the folder 'Resources'
            string materialPath = PathUtil.MaterialPropertiesFile();
            List<FemMaterial> inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            FemMaterial material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            string crosecPath = PathUtil.CrossSectionValuesFile();
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out string info);
            List<CroSec> croSecFamily = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            CroSec croSecInitial = croSecFamily.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            croSecInitial.setMaterial(material);

            // create the column
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { line1, line2 },
                new List<string>() { "B1" },
                new List<CroSec>() { croSecInitial },
                logger,
                out List<Point3> out_points);

            // change the default orientation of beam #0
            IBuilderElementOrientationWriter ori = beams[0].Ori.Writer;
            ori.XOri = line1.PointAtStart - line1.PointAtEnd;
            beams[0].Ori = ori.Reader;

            // create supports
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, false, true }),
                k3d.Support.Support(p2, new List<bool>() { true, true, true, true, false, true }),
            };

            // create a Point-load
            double fx = 10;
            List<Load> loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(fx, 0, 0), new Vector3()) };

            // create the model
            Model model = k3d.Model.AssembleModel(beams, supports, loads, out info, out double mass, out Point3 cog, out string message, out bool warning);
            Model calcModel = k3d.Algorithms.AnalyzeThI(model, out IReadOnlyList<double> displacement, out IReadOnlyList<double> gravities, out IReadOnlyList<double> energies, out string warnings);

            NodeForces.solve(calcModel, 1, new Plane3(), "LC0", false, false, out List<NodeForces.NodeForcesResult> nodeForcesResults);
            double sumX = nodeForcesResults[0].Forces[0].X + nodeForcesResults[1].Forces[0].X + fx;
            double sumZ = nodeForcesResults[0].Forces[0].Z + nodeForcesResults[1].Forces[0].Z;
            Assert.That(sumX, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumZ, Is.EqualTo(0.0).Within(1E-8));
        }

        [Test]
        public void FrameCornerOrientedEccentric()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            double l = 4.0;
            Point3 p0 = new Point3(-l, 0, 0);
            Point3 p1 = new Point3(0, 0, 0);
            Point3 p2 = new Point3(l, 0, 0);
            Line3 line1 = new Line3(p0, p1);
            Line3 line2 = new Line3(p1, p2);

            // get a material from the material table in the folder 'Resources'
            string materialPath = PathUtil.MaterialPropertiesFile();
            List<FemMaterial> inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            FemMaterial material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            string crosecPath = PathUtil.CrossSectionValuesFile();
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out string info);
            List<CroSec> croSecFamily = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            CroSec croSecInitial = croSecFamily.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            croSecInitial.setMaterial(material);

            // create the column
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { line1, line2 },
                new List<string>() { "B1" },
                new List<CroSec>() { croSecInitial },
                logger,
                out List<Point3> out_points);

            // change the default orientation of beam #0
            IBuilderElementOrientationWriter ori = beams[0].Ori.Writer;
            ori.XOri = line1.PointAtStart - line1.PointAtEnd;
            beams[0].Ori = ori.Reader;

            // change the eccentricity of beam #0
            double ecc = 0.5;
            beams[0].ecce_loc = new Vector3(ecc, ecc, ecc);

            // create supports
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-load
            double fx = 1;
            double fy = 2;
            double fz = 3;
            List<Load> loads = new List<Load> { k3d.Load.PointLoad(p2, new Vector3(fx, fy, fz), new Vector3()) };

            // create the model
            Model model = k3d.Model.AssembleModel(beams, supports, loads, out info, out double mass, out Point3 cog, out string message, out bool warning);
            Model calcModel = k3d.Algorithms.AnalyzeThI(model, out IReadOnlyList<double> displacement, out IReadOnlyList<double> gravities, out IReadOnlyList<double> energies, out string warnings);

            NodeForces.solve(calcModel, 1, new Plane3(), "LC0", false, false, out List<NodeForces.NodeForcesResult> nodeForcesResults);
            double sumFx = nodeForcesResults[0].Forces[0].X + nodeForcesResults[1].Forces[0].X;
            double sumFz = nodeForcesResults[0].Forces[0].Z + nodeForcesResults[1].Forces[0].Z;
            double sumMx = nodeForcesResults[0].Moments[0].X + nodeForcesResults[1].Moments[0].X;
            double sumMy = nodeForcesResults[0].Moments[0].Y + nodeForcesResults[1].Moments[0].Y;
            double sumMz = nodeForcesResults[0].Moments[0].Z + nodeForcesResults[1].Moments[0].Z;
            Assert.That(sumFx, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumFz, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumMx, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumMy, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumMz, Is.EqualTo(0.0).Within(1E-8));

            var state1 = ((ModelElementStraightLine)model.elems[0]).GetState1D(
                0.5,
                model,
                new StateElement1DSelectorIndex(model, model.lcActivation.LoadCaseCombinations.First()),
                isCurveReparametrized: true,
                out _,
                out _);
            var my0 = state1.GetResult(Element1DOption.My, out _);

            var lcc = model.lcActivation.LoadCaseCombinations.First();
            var state2 = ((ModelElementStraightLine)model.elems[0]).GetState1D(
                0.5,
                model,
                new StateElement1DSelectorIndex(model, lcc),
                isCurveReparametrized: true,
                out _,
                out _);
            var my1 = state2.GetResult(Element1DOption.My, out _);
            Assert.That(my0.First(), Is.EqualTo(0.0).Within(1E-8));
            Assert.That(my1.First(), Is.EqualTo(0.0).Within(1E-8));
        }

        [Test]
        public void CrossEccentric()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            double l = 4.0;
            Point3 p0 = new Point3(-l, 0, 0);
            Point3 p1 = new Point3(0, 0, 0);
            Point3 p2 = new Point3(l, 0, 0);
            Point3 p3 = new Point3(0, 0, -l);
            Point3 p4 = new Point3(0, 0, l);
            Line3 line1 = new Line3(p0, p1);
            Line3 line2 = new Line3(p1, p2);
            Line3 line3 = new Line3(p3, p1);
            Line3 line4 = new Line3(p1, p4);

            // get a material from the material table in the folder 'Resources'
            string materialPath = PathUtil.MaterialPropertiesFile();
            List<FemMaterial> inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            FemMaterial material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            string crosecPath = PathUtil.CrossSectionValuesFile();
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out string info);
            List<CroSec> croSecFamily = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            CroSec croSecInitial = croSecFamily.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            croSecInitial.setMaterial(material);

            // create the column
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { line1, line2, line3, line4 },
                new List<string>() { "B1" },
                new List<CroSec>() { croSecInitial },
                logger,
                out List<Point3> out_points);

            // change the eccentricity of beam #2, #3
            double ecc = 0.5;
            beams[2].ecce_glo = new Vector3(0, ecc, 0);
            beams[3].ecce_glo = new Vector3(0, ecc, 0);

            // create supports
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-load
            double fx = 1;
            List<Load> loads = new List<Load>
            {
                k3d.Load.PointLoad(p3, new Vector3(fx, 0, 0), new Vector3()),
                k3d.Load.PointLoad(p4, new Vector3(-fx, 0, 0), new Vector3()),
            };

            // create the model
            Model model = k3d.Model.AssembleModel(beams, supports, loads, out info, out double mass, out Point3 cog, out string message, out bool warning);
            Model calcModel = k3d.Algorithms.AnalyzeThI(model, out IReadOnlyList<double> displacement, out IReadOnlyList<double> gravities, out IReadOnlyList<double> energies, out string warnings);
            NodeForces.solve(calcModel, 1, new Plane3(), "LC0", false, false, out List<NodeForces.NodeForcesResult> nodeForcesResults);
            double sumFx = 0;
            for (int i = 0; i < 4; ++i)
            {
                sumFx += nodeForcesResults[i].Forces[0].X;
            }

            double sumFz = 0;
            for (int i = 0; i < 4; ++i)
            {
                sumFz += nodeForcesResults[i].Forces[0].Z;
            }

            double sumMx = 0;
            for (int i = 0; i < 4; ++i)
            {
                sumMx += nodeForcesResults[i].Moments[0].X;
            }

            double sumMy = 0;
            for (int i = 0; i < 4; ++i)
            {
                sumMy += nodeForcesResults[i].Moments[0].Y;
            }

            double sumMz = 0;
            for (int i = 0; i < 4; ++i)
            {
                sumMz += nodeForcesResults[i].Moments[0].Z;
            }

            Assert.That(sumFx, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumFz, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumMx, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumMy, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumMz, Is.EqualTo(0.0).Within(1E-8));

            var lcc = model.lcActivation.LoadCaseCombinations.First();
            var state0 = ((ModelElementStraightLine)model.elems[0]).GetState1D(0.5, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: true, out _, out _);
            var my0 = state0.GetResult(Element1DOption.RotY, out _).First();
            var state1 = ((ModelElementStraightLine)model.elems[0]).GetState1D(0.5, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: true, out _, out _);
            var my1 = state1.GetResult(Element1DOption.RotY, out _).First();
            Assert.That(my0, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(my1, Is.EqualTo(0.0).Within(1E-8));
        }
    }
}
#endif
