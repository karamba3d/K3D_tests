#if ALL_TESTS

namespace KarambaCommon.Tests.Result
{
    using System.Collections.Generic;
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
    public class ReinforcementStressTests
    {
        [Test]
        public void DeformationTestIsotropicShellInPlane()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            int nFaces = 1;
            double length = 1.0;
            double xIncMesh = length / nFaces;
            double limit_dist = xIncMesh / 100.0;

            double y0 = 0.0;
            double y1 = 1.0;

            // create the mesh
            Mesh3 mesh = new Mesh3((nFaces + 1) * 2, nFaces);
            mesh.AddVertex(new Point3(0, y0, 0));
            mesh.AddVertex(new Point3(0, y1, 0));
            for (int faceInd = 0; faceInd < nFaces; ++faceInd)
            {
                mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, y0, 0));
                mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, y1, 0));
                int nV = mesh.Vertices.Count;
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
            Support support1 = new Support(
                new Point3(0, y0, 0),
                new List<bool>() { true, true, true, true, true, true },
                Plane3.Default);
            Support support2 = new Support(
                new Point3(0, y1, 0),
                new List<bool>() { true, false, true, true, true, true },
                Plane3.Default);

            // create two point loads
            PointLoad pl1 = new PointLoad(mesh.Vertices.Count - 2, new Vector3(25, 0, 0), new Vector3(), false);
            PointLoad pl2 = new PointLoad(mesh.Vertices.Count - 1, new Vector3(25, 0, 0), new Vector3(), false);

            // assemble the model
            ModelBuilder modelBuilder = new ModelBuilder(limit_dist);
            Model model = modelBuilder.build(
                new List<Point3>(),
                new List<FemMaterial>(),
                new List<CroSec>(),
                new List<Support>() { support1, support2 },
                new List<Load>() { pl1, pl2 },
                outBuilderShells,
                new List<ElemSet>(),
                new List<Joint>(),
                out var logger);
            AnalyzeThI.solve(model, out IReadOnlyList<double> outMaxDisp, out _, out _, out _, out _);

            Assert.That(outMaxDisp[0], Is.EqualTo(2.4858889456113236E-05).Within(1E-5));
        }
    }
}

#endif
