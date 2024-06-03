#if ALL_TESTS

namespace KarambaCommon.Tests.Result.ShellSection
{
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Models;
    using Karamba.Results.ShellSection;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;

    public class MarchingAlgorithmRetrieverTestOnTMesh
    {
        public Model Model { get; set; }

        public double Tolerance => 1E-12;

        public double Delta => 0.02;

        [OneTimeSetUp]
        public void InitializeModel()
        {
            // Create mesh
            var mesh = new Mesh3();
            _ = mesh.AddVertex(0, 0, 0);
            _ = mesh.AddVertex(1, -1, 0);
            _ = mesh.AddVertex(1, 0, 0);
            _ = mesh.AddVertex(1, 0, 1);
            _ = mesh.AddVertex(1, -1, -1);

            _ = mesh.AddFace(0, 1, 2);
            _ = mesh.AddFace(1, 2, 3);
            _ = mesh.AddFace(1, 2, 4);

            // Create Model
            var k3d = new Toolkit();
            var supportConditions = new List<bool>()
            {
                true,
                true,
                true,
                true,
                true,
                true,
            };

            var supports = new List<Support>
            {
                k3d.Support.Support(new Point3(0, 0, 0), supportConditions),
                k3d.Support.Support(new Point3(1, -1, 0), supportConditions),
                k3d.Support.Support(new Point3(1, 0, 0), supportConditions),
                k3d.Support.Support(new Point3(1, 0, 1), supportConditions),
                k3d.Support.Support(new Point3(1, -1, -1), supportConditions),
            };

            var loads = new List<Load>
            {
                k3d.Load.PointLoad(new Point3(0, 0, 0), new Vector3(0, 0, -5)),
            };

            var logger = new MessageLogger();
            var crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(
                25,
                0,
                null,
                new List<double>
                {
                    4,
                    4,
                    -4,
                    -4,
                });

            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out _);

            var model = k3d.Model.AssembleModel(shells, supports, loads, out _, out _, out _, out _, out _);
            Model = k3d.Algorithms.AnalyzeThI(model, out _, out _, out _, out _);
        }

        [Test]
        public void FindSection_2PtPolyline()
        {
            // Arrange
            var polyline = new PolyLine3
            {
                new Point3(0.7, -0.5, 1),
                new Point3(1.5, -0.5, 1),
            };

            var vector = -Vector3.UnitZ;

            // Act
            var intersection = ComputeIntersection(Model, polyline, vector);

            // Assert
            var points = intersection.Polylines.SelectMany(p => p.Points);
            var expectedPoints = new Point3[] { new Point3(0.7, -0.5, 0), new Point3(1, -0.5, 0) };
            // Assert.That(points, Is.EqualTo(expectedPoints).AsCollection.Within(Tolerance));
            // -> not supported exception
            foreach (var (p, idx) in points.WithIndex())
               Assert.That(p.ToArray(), Is.EqualTo(expectedPoints[idx].ToArray()).Within(Tolerance));

            var indices = intersection.CrossSectionFaceIndices.SelectMany(x => x);
            var expectedIndices = new int[] { 0 };
            Assert.That(indices, Is.EqualTo(expectedIndices).AsCollection);
        }

        private ShellSectionIntersection ComputeIntersection(Model model, PolyLine3 polyline, Vector3 vector)
        {
            var info = new MarchingInfo(
                new ShellSectionGeometry(model, model.elems.OfType<ModelMembrane>()),
                polyline,
                vector,
                Tolerance,
                Delta);
            var retriever = new MarchingAlgorithmRetriever(info);

            retriever.FindSection(out var allOutputPoly, out var allOutputFaces, out _);

            return new ShellSectionIntersection(allOutputPoly, allOutputFaces);
        }

        private class ShellSectionIntersection
        {
            public ShellSectionIntersection(
                IList<PolyLine3> polylines,
                IList<IList<int>> crossSectionFaceIndices)
            {
                Polylines = polylines;
                CrossSectionFaceIndices = crossSectionFaceIndices;
            }

            public IList<PolyLine3> Polylines { get; }

            public IList<IList<int>> CrossSectionFaceIndices { get; }
        }
    }
}

#endif
