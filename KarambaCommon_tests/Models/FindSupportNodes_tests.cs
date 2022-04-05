#if ALL_TESTS

namespace KarambaCommon.Tests.Model
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
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class FindSupportNodesTests
    {
        [Test]

        // [Category("QuickTests")]
        public void FindSupportNode()
        {
            var nFaces = 1;
            var length = 10.0;
            var xIncMesh = length / nFaces;
            var limit_dist = xIncMesh / 100.0;

            // create the mesh
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
            var model = modelBuilder.build(
                new List<Point3>(),
                new List<FemMaterial>(),
                new List<CroSec>(),
                new List<Support>() { support1, support2 },
                new List<Load>() { gravityLoad },
                outBuilderShells,
                new List<ElemSet>(),
                new List<Joint>(),
                new MessageLogger());

            AnalyzeThI.solve(model, out var outMaxDisp, out _, out _, out _, out _);

            Assert.That(outMaxDisp[0], Is.EqualTo(53.62193486094136).Within(1E-5));

            var nodeInd = model.NodeInd(new Point3(0, -0.5, 0), 1e-8);
            var support3 = model.Support(nodeInd);
            Assert.True(support3 != null);
            var support4 = model.Support(nodeInd + 3);
            Assert.True(support4 == null);
        }
    }
}

#endif