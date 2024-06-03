#if ALL_TESTS
namespace KarambaCommon.Tests.Result.ShellSection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Results.ShellSection;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Load = Karamba.Loads.Load;

    [TestFixture]
    public class MarchingAlgorithmRetrieverTestsOnNonConvexMesh
    {
        [Test]
        public void FindSection_3ptPolyline_NonConvexMesh()
        {
            // Arrange
            var inputPolyline = new PolyLine3
            {
                new Point3(0.5, -0.5, 1),
                new Point3(0.5, 0.8, 1),
                new Point3(0.5, 3.5, 1),
            };

            var inputProjectionVector = new Vector3(0, 0, -1);
            Karamba.Models.Model inputModel = MakeNonConvexModel();
            double tol = 1E-12;
            double delta = 0.02;
            var meshGroup = ShellSectionTestsUtilities.BuildMeshGroup(inputModel.elems.OfType<ModelMembrane>(), inputModel);

            // Act
            var info = new MarchingInfo(
                new ShellSectionGeometry(inputModel, inputModel.elems.OfType<ModelMembrane>()),
                inputPolyline,
                inputProjectionVector,
                tol,
                delta);
            var retriever = new MarchingAlgorithmRetriever(info);
            retriever.FindSection(out var outputPolylines, out var outputFaceIndxs, out _);

            // Assert
            var expectedPolyline = new PolyLine3(new Point3(0.5, 0, 0), new Point3(0.5, 0.5, 0));
            Assert.That(outputPolylines[0], Is.EqualTo(expectedPolyline).Using<List<PolyLine3>>(EqualityWithTol));

            expectedPolyline = new PolyLine3(new Point3(0.5, 1, 0), new Point3(0.5, 1.5, 0), new Point3(0.5, 2, 0));
            Assert.That(outputPolylines[1], Is.EqualTo(expectedPolyline).Using<List<PolyLine3>>(EqualityWithTol));

            var expectedFaceIndices = new List<List<int>>() { new List<int>() { 0 }, new List<int>() { 1, 2 }, };
            Assert.That(outputFaceIndxs, Is.EqualTo(expectedFaceIndices));
        }

        private bool EqualityWithTol(IList<PolyLine3> listPoly1, IList<PolyLine3> listPoly2)
        {
            double tolerance = 1E-12;

            if (listPoly1.Count != listPoly2.Count)
                return false;
            for (int j = 0; j < listPoly1.Count; j++)
            {
                PolyLine3 poly1 = listPoly1[j];
                PolyLine3 poly2 = listPoly2[j];

                if (poly1.PointCount != poly2.PointCount)
                    return false;
                for (int i = 0; i < poly1.PointCount; i++)
                {
                    bool success = Math.Abs(poly1[i].X - poly2[i].X) < tolerance &&
                                  Math.Abs(poly1[i].Y - poly2[i].Y) < tolerance && Math.Abs(poly1[i].Z - poly2[i].Z) < tolerance;
                    if (success == false)
                        return false;
                }
            }

            return true;
        }

        private static Karamba.Models.Model MakeNonConvexModel()
        {
            Toolkit k3d = new Toolkit();
            Mesh3 mesh = new Mesh3();
            mesh.AddVertex(0, 0, 0);
            mesh.AddVertex(1, 0, 0);
            mesh.AddVertex(1, 1, 0);
            mesh.AddVertex(0, 1, 0);
            mesh.AddVertex(1, 2, 0);
            mesh.AddVertex(0, 2, 0);
            mesh.AddFace(0, 1, 2);
            mesh.AddFace(3, 2, 4);
            mesh.AddFace(3, 4, 5);

            List<bool> supportConditions = new List<bool>() { true, true, true, true, true, true };
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(0, supportConditions),
                k3d.Support.Support(3, supportConditions),
                k3d.Support.Support(5, supportConditions),
            };

            List<Load> loads = new List<Load>
            {
                k3d.Load.PointLoad(4, new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(2, new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(1, new Vector3(0, 0, -5)),
            };

            MessageLogger logger = new MessageLogger();
            CroSec_Shell crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(
                25,
                0,
                null,
                new List<double> { 4, 4, -4, -4 },
                0);
            List<Karamba.Elements.BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out _);
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _);
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out _);

            return model;
        }
    }
}

#endif
