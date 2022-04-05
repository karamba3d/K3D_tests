#if ALL_TESTS

namespace KarambaCommon.Tests.Result
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Loads.Combinations;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Results;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class NodeForces_tests
    {
        [Test]
        public void FrameCorner()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double l = 4.0;
            var p0 = new Point3(-l, 0, 0);
            var p1 = new Point3(0, 0, l);
            var p2 = new Point3(l, 0, 0);
            var line1 = new Line3(p0, p1);
            var line2 = new Line3(p1, p2);

            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            // get a material from the material table in the folder 'Resources'
            var materialPath = Path.Combine(resourcePath, "MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = Path.Combine(resourcePath, "CrossSectionValues.bin");
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out var info);
            var croSecFamily = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            var croSecInitial = croSecFamily.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            croSecInitial.setMaterial(material);

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { line1, line2 },
                new List<string>() { "B1" },
                new List<CroSec>() { croSecInitial },
                logger,
                out var out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, false, true }),
                k3d.Support.Support(p2, new List<bool>() { true, true, true, true, false, true }),
            };

            // create a Point-load
            double fx = 10;
            var loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(fx, 0, 0), new Vector3()) };

            // create the model
            var model = k3d.Model.AssembleModel(beams, supports, loads, out info, out var mass, out var cog, out var message, out var warning);
            var calcModel = k3d.Algorithms.AnalyzeThI(model, out var displacement, out var gravities, out var energies, out var warnings);

            NodeForces.solve(calcModel, 1, new Plane3(), "-1", false, false, out var nodeForcesResults);
            var sumX = nodeForcesResults[0].Forces[0].X + nodeForcesResults[1].Forces[0].X + fx;
            var sumZ = nodeForcesResults[0].Forces[0].Z + nodeForcesResults[1].Forces[0].Z;
            Assert.That(sumX, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumZ, Is.EqualTo(0.0).Within(1E-8));
        }

        [Test]
        public void FrameCornerOriented()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double l = 4.0;
            var p0 = new Point3(-l, 0, 0);
            var p1 = new Point3(0, 0, l);
            var p2 = new Point3(l, 0, 0);
            var line1 = new Line3(p0, p1);
            var line2 = new Line3(p1, p2);

            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            // get a material from the material table in the folder 'Resources'
            var materialPath = Path.Combine(resourcePath, "MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = Path.Combine(resourcePath, "CrossSectionValues.bin");
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out var info);
            var croSecFamily = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            var croSecInitial = croSecFamily.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            croSecInitial.setMaterial(material);

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { line1, line2 },
                new List<string>() { "B1" },
                new List<CroSec>() { croSecInitial },
                logger,
                out var out_points);

            // change the default orientation of beam #0
            var ori = beams[0].Ori.Writer;
            ori.XOri = line1.PointAtStart - line1.PointAtEnd;
            beams[0].Ori = ori.Reader;

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, false, true }),
                k3d.Support.Support(p2, new List<bool>() { true, true, true, true, false, true }),
            };

            // create a Point-load
            double fx = 10;
            var loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(fx, 0, 0), new Vector3()) };

            // create the model
            var model = k3d.Model.AssembleModel(beams, supports, loads, out info, out var mass, out var cog, out var message, out var warning);
            var calcModel = k3d.Algorithms.AnalyzeThI(model, out var displacement, out var gravities, out var energies, out var warnings);

            NodeForces.solve(calcModel, 1, new Plane3(), "-1", false, false, out var nodeForcesResults);
            var sumX = nodeForcesResults[0].Forces[0].X + nodeForcesResults[1].Forces[0].X + fx;
            var sumZ = nodeForcesResults[0].Forces[0].Z + nodeForcesResults[1].Forces[0].Z;
            Assert.That(sumX, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumZ, Is.EqualTo(0.0).Within(1E-8));
        }

        [Test]
        public void FrameCornerOrientedEccentric()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double l = 4.0;
            var p0 = new Point3(-l, 0, 0);
            var p1 = new Point3(0, 0, 0);
            var p2 = new Point3(l, 0, 0);
            var line1 = new Line3(p0, p1);
            var line2 = new Line3(p1, p2);

            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            // get a material from the material table in the folder 'Resources'
            var materialPath = Path.Combine(resourcePath, "MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = Path.Combine(resourcePath, "CrossSectionValues.bin");
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out var info);
            var croSecFamily = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            var croSecInitial = croSecFamily.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            croSecInitial.setMaterial(material);

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { line1, line2 },
                new List<string>() { "B1" },
                new List<CroSec>() { croSecInitial },
                logger,
                out var out_points);

            // change the default orientation of beam #0
            var ori = beams[0].Ori.Writer;
            ori.XOri = line1.PointAtStart - line1.PointAtEnd;
            beams[0].Ori = ori.Reader;

            // change the eccentricity of beam #0
            double ecc = 0.5;
            beams[1].ecce_loc = new Vector3(ecc, ecc, ecc);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-load
            double fx = 1;
            double fy = 2;
            double fz = 3;
            var loads = new List<Load> { k3d.Load.PointLoad(p2, new Vector3(fx, fy, fz), new Vector3()) };

            // create the model
            var model = k3d.Model.AssembleModel(beams, supports, loads, out info, out var mass, out var cog, out var message, out var warning);
            var calcModel = k3d.Algorithms.AnalyzeThI(model, out var displacement, out var gravities, out var energies, out var warnings);

            NodeForces.solve(calcModel, 1, new Plane3(), "-1", false, false, out var nodeForcesResults);
            var sumFx = nodeForcesResults[0].Forces[0].X + nodeForcesResults[1].Forces[0].X;
            var sumFz = nodeForcesResults[0].Forces[0].Z + nodeForcesResults[1].Forces[0].Z;
            var sumMx = nodeForcesResults[0].Moments[0].X + nodeForcesResults[1].Moments[0].X;
            var sumMy = nodeForcesResults[0].Moments[0].Y + nodeForcesResults[1].Moments[0].Y;
            var sumMz = nodeForcesResults[0].Moments[0].Z + nodeForcesResults[1].Moments[0].Z;
            Assert.That(sumFx, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumFz, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumMx, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumMy, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(sumMz, Is.EqualTo(0.0).Within(1E-8));

            var statesCollection0 = ((ModelElementStraightLine)model.elems[0]).GetElementStates(model, new LCSuperPosition(0, model), new List<double> { 0.5 });
            var my0 = statesCollection0.First().GetForce(BeamDofs.r_y);
            var statesCollection1 = ((ModelElementStraightLine)model.elems[0]).GetElementStates(model, new LCSuperPosition(0, model), new List<double> { 0.5 });
            var my1 = statesCollection1.First().GetForce(BeamDofs.r_y);
            Assert.That(my0, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(my1, Is.EqualTo(0.0).Within(1E-8));
        }

        [Test]
        public void CrossEccentric()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double l = 4.0;
            var p0 = new Point3(-l, 0, 0);
            var p1 = new Point3(0, 0, 0);
            var p2 = new Point3(l, 0, 0);
            var p3 = new Point3(0, 0, -l);
            var p4 = new Point3(0, 0, l);
            var line1 = new Line3(p0, p1);
            var line2 = new Line3(p1, p2);
            var line3 = new Line3(p3, p1);
            var line4 = new Line3(p1, p4);

            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            // get a material from the material table in the folder 'Resources'
            var materialPath = Path.Combine(resourcePath, "MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = Path.Combine(resourcePath, "CrossSectionValues.bin");
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out var info);
            var croSecFamily = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            var croSecInitial = croSecFamily.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            croSecInitial.setMaterial(material);

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { line1, line2, line3, line4 },
                new List<string>() { "B1" },
                new List<CroSec>() { croSecInitial },
                logger,
                out var out_points);

            // change the eccentricity of beam #2, #3
            double ecc = 0.5;
            beams[2].ecce_glo = new Vector3(0, ecc, 0);
            beams[3].ecce_glo = new Vector3(0, ecc, 0);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-load
            double fx = 1;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(p3, new Vector3(fx, 0, 0), new Vector3()),
                k3d.Load.PointLoad(p4, new Vector3(-fx, 0, 0), new Vector3()),
            };

            // create the model
            var model = k3d.Model.AssembleModel(beams, supports, loads, out info, out var mass, out var cog, out var message, out var warning);
            var calcModel = k3d.Algorithms.AnalyzeThI(model, out var displacement, out var gravities, out var energies, out var warnings);

            NodeForces.solve(calcModel, 1, new Plane3(), "-1", false, false, out var nodeForcesResults);
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

            var statesCollection0 = ((ModelElementStraightLine)model.elems[0]).GetElementStates(model, new LCSuperPosition(0, model), new List<double> { 0.5 });
            var my0 = statesCollection0.First().GetForce(BeamDofs.r_y);
            var statesCollection1 = ((ModelElementStraightLine)model.elems[0]).GetElementStates(model, new LCSuperPosition(0, model), new List<double> { 0.5 });
            var my1 = statesCollection1.First().GetForce(BeamDofs.r_y);
            Assert.That(my0, Is.EqualTo(0.0).Within(1E-8));
            Assert.That(my1, Is.EqualTo(0.0).Within(1E-8));
        }
    }
}
#endif