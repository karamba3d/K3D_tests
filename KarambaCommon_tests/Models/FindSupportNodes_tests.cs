#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using UnitSystem = Karamba.Utilities.UnitSystem;

    [TestFixture]
    public class FindSupportNodesTests
    {
        [Test]
        // [Category("QuickTests")]
        public void FindSupportNode()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            // XXX something other global ...

            int nFaces = 1;
            double length = 10.0;
            double xIncMesh = length / nFaces;
            double limit_dist = xIncMesh / 100.0;

            // create the mesh
            var mesh = new Mesh3((nFaces + 1) * 2, nFaces);
            _ = mesh.AddVertex(new Point3(0, -0.5, 0));
            _ = mesh.AddVertex(new Point3(0, 0.5, 0));
            for (int faceInd = 0; faceInd < nFaces; ++faceInd)
            {
                _ = mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, -0.5, 0));
                _ = mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, 0.5, 0));
                int nV = mesh.Vertices.Count;
                _ = mesh.AddFace(nV - 4, nV - 3, nV - 1, nV - 2);
            }

            // create a shell
            MeshToShell.solve(
                new List<Point3>(),
                new List<Mesh3>() { mesh },
                limit_dist,
                new List<string>(),
                new List<Color>(),
                new List<CroSec>(),
                true,
                out _,
                out List<BuilderShell> outBuilderShells,
                out _);

            // create two supports
            var support1 = new Support(
                new Point3(0, -0.5, 0),
                new List<bool>() { true, true, true, true, true, true },
                Plane3.Default);
            var support2 = new Support(
                new Point3(0, 0.5, 0),
                new List<bool>() { true, true, true, true, true, true },
                Plane3.Default);

            // create a gravity load
            var gravityLoad = new GravityLoad(new Vector3(0, 0, -1));

            // assemble the model
            var modelBuilder = new ModelBuilder(limit_dist);
            Model model = modelBuilder.build(
                new List<Point3>(),
                new List<FemMaterial>(),
                new List<CroSec>(),
                new List<Support>() { support1, support2 },
                new List<Load>() { gravityLoad },
                outBuilderShells,
                new List<ElemSet>(),
                new List<Joint>(),
                out var _);    // logger

            AnalyzeThI.solve(model, out IReadOnlyList<double> outMaxDisp, out _, out _, out _, out _);

            int nodeInd = model.NodeInd(new Point3(0, -0.5, 0), 1e-8);
            Support support3 = model.Support(nodeInd);
            Support support4 = model.Support(nodeInd + 3);

            Assert.Multiple(() => {
                Assert.That(outMaxDisp[0], Is.EqualTo(53.62193486094136).Within(1E-5));
                Assert.That(support3, Is.Not.Null);
                Assert.That(support4, Is.Null);
                // Debug.WriteLine("test");
            });
        }
    }
}

#endif
