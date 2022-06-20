#if ALL_TESTS

namespace KarambaCommon.Tests.Loads
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
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class MeshLoad_tests
    {
        [Test]
        public void MeshLoadProfiling()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            int nBeams = 20;
            int nFaces = 1500;
            double lengthBeams = 10.0;
            double xIncBeam = lengthBeams / nBeams;
            double xIncMesh = lengthBeams / nFaces;
            _ = xIncBeam / 100.0;

            // create beams
            var lines = new List<Line3>();
            var nodeI = new Point3(0, 0, 0);
            for (int beamInd = 0; beamInd < nBeams; ++beamInd)
            {
                var nodeK = new Point3(nodeI.X + xIncBeam, 0, 0);
                lines.Add(new Line3(nodeI, nodeK));
                nodeI = nodeK;
            }

            var builderElements = k3d.Part.LineToBeam(
                lines,
                new List<string>(),
                new List<CroSec>(),
                logger,
                out List<Point3> _);

            // create a MeshLoad
            var mesh = new Mesh3((nFaces + 1) * 2, nFaces);
            mesh.AddVertex(new Point3(0, -0.5, 0));
            mesh.AddVertex(new Point3(0, 0.5, 0));
            for (var faceInd = 0; faceInd < nFaces; ++faceInd)
            {
                mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, -0.5, 0));
                mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, 0.5, 0));
                var nV = mesh.Vertices.Count;
                mesh.AddFace(nV - 4, nV - 3, nV - 1, nV - 2);
            }

            var ucf = UnitsConversionFactory.Conv();
            var m = UnitsConversionFactory.Conv().base_length;
            var baseMesh = m.toBaseMesh(mesh);

            // create a mesh load
            var load = k3d.Load.MeshLoad(new List<Vector3>() { new Vector3(0, 0, -1) }, baseMesh);

            // create a support
            var support = k3d.Support.Support(new Point3(0, 0, 0), k3d.Support.SupportFixedConditions);

            // assemble the model
            var model = k3d.Model.AssembleModel(
                builderElements,
                new List<Support>() { support },
                new List<Load>() { load },
                out var _,
                out _,
                out var _,
                out var _,
                out var _);

            // calculate the model
            _ = k3d.Algorithms.AnalyzeThI(
                model,
                out var outMaxDisp,
                out var _,
                out var _,
                out var _);
            Assert.That(outMaxDisp[0], Is.EqualTo(2.8232103119228276).Within(1E-5));
        }

        [Test]
        public void MeshLoad_on_QuadMesh()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 0);
            var p2 = new Point3(1, 1, 0);
            var p3 = new Point3(0, 1, 0);
            var mesh = new Mesh3(new List<Point3>() { p0, p1, p2, p3 }, new List<Face3>() { new Face3(0, 1, 2, 3), });

            // create a mesh load
            var load = k3d.Load.MeshLoad(new List<Vector3>() { new Vector3(0, 0, 1) }, mesh);

            // create a shell
            var shells = k3d.Part.MeshToShell(new List<Mesh3>() { mesh }, null, null, logger, out _);

            // assemble the model
            var model = k3d.Model.AssembleModel(
                shells,
                null,
                new List<Load> { load },
                out _,
                out _,
                out _,
                out _,
                out _);

            Assert.That(model.mloads[0].model_unit_loads.max_vertex_load, Is.EqualTo(0.25).Within(1e-5));
        }

        [Test]
        public void MeshLoad_on_Named_QuadMesh()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 0);
            var p2 = new Point3(1, 1, 0);
            var p3 = new Point3(0, 1, 0);
            var mesh = new Mesh3(new List<Point3>() { p0, p1, p2, p3 }, new List<Face3>() { new Face3(0, 1, 2, 3), });

            // create a mesh load
            var load = k3d.Load.MeshLoad(
                new List<Vector3>() { new Vector3(0, 0, 1) },
                mesh,
                LoadOrientation.global,
                true,
                true,
                null,
                new List<string>() { "A" });

            // create a shell
            List<BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3>() { mesh },
                new List<string>() { "A" },
                null,
                logger,
                out _);

            // assemble the model
            var model = k3d.Model.AssembleModel(
                shells,
                null,
                new List<Load> { load },
                out _,
                out _,
                out _,
                out _,
                out _);

            Assert.That(model.mloads[0].model_unit_loads.max_vertex_load, Is.EqualTo(0.25).Within(1e-5));
        }

        [Test]
        public void UnitMeshLoads_Detection()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 0);
            var p2 = new Point3(1, 1, 0);
            var p3 = new Point3(0, 1, 0);
            var elemPoints = new List<Point3>()
            {
                p0,
                p1,
                p2,
                p3,
            };
            var elemMesh = new Mesh3(new List<Point3>() { p0, p1, p2, p3 }, new List<Face3>() { new Face3(0, 1, 2, 3), });

            // create a shell
            List<BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3>() { elemMesh },
                new List<string>() { "A" },
                null,
                logger,
                out _);

            var p4 = new Point3(0, 0, 0);
            var p5 = new Point3(1, 1, 0);
            var p6 = new Point3(1, -1, 0);
            var points = new List<Point3>()
            {
                p4,
                p5,
                p6,
            };

            var loadMesh = new Mesh3(points, new List<Face3>() { new Face3(0, 1, 2), });

            // create a mesh loads
            var meshUnitLoadsCache = new Dictionary<MeshUnitLoad, MeshUnitLoad>();

            var load1 = k3d.Load.MeshLoad(
                new List<Vector3>() { new Vector3(0, 0, 1) },
                loadMesh,
                LoadOrientation.global,
                true,
                false,
                elemPoints,
                new List<string> { "A" },
                meshUnitLoadsCache);

            var load2 = k3d.Load.MeshLoad(
                new List<Vector3>() { new Vector3(0, 0, 2) },
                loadMesh,
                LoadOrientation.global,
                true,
                false,
                elemPoints,
                new List<string> { "A" },
                meshUnitLoadsCache);

            var load3 = k3d.Load.MeshLoad(
                new List<Vector3>() { new Vector3(0, 0, -2) },
                loadMesh,
                LoadOrientation.global,
                true,
                false,
                elemPoints,
                new List<string> { "A" },
                meshUnitLoadsCache);

            Assert.That(meshUnitLoadsCache.Count == 1);

            // assemble the model
            var model = k3d.Model.AssembleModel(
                shells,
                null,
                new List<Load> { load1, load2, load3 },
                out _,
                out _,
                out _,
                out _,
                out _);
        }
    }
}

#endif