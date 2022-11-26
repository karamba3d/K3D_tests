#if ALL_TESTS

namespace KarambaCommon.Tests.Result.ShellSection
{
    using System;
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Results.ShellSection;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class RetrieverFindSection_Tests_NonPlanarMesh
    {
        private double _tol;

        [Test]
        public void FindSection_3ptpolyline_NonPlanarMesh()
        {
            PolyLine3 inputPolyline = new PolyLine3(new Point3(0.5, -0.5, 1), new Point3(0.5, 1.5, 1));
            Vector3 inputProjectionVector = new Vector3(0, 0, -1);
            _tol = 1E-12;
            var delta = 0.02;
            var inputModel = MakeNonConvexModel();

            var info = new RetrieverInfo_Force(inputModel, inputPolyline, inputProjectionVector, _tol, delta, "0", new List<string> { string.Empty }, new List<Guid>());
            var strategy = new RetrieverStrategy_Force(1, 1);
            var sut = new Karamba.Results.ShellSection.Retriever(strategy, info);

            sut.FindSection(out var outputPolylines, out var outputFaceIndxs);

            var expectedPolylines = new PolyLine3(
                new Point3(0.5, 0, 0.25),
                new Point3(0.5, 0.5, 0.75),
                new Point3(0.5, 1, 0.5));
            var expectedFaceIndxs = new List<int>() { 0, 1 };

            Assert.That(outputPolylines[0], Is.EqualTo(expectedPolylines).Using<PolyLine3>(EqualityWithTol));
            Assert.That(outputFaceIndxs[0], Is.EqualTo(expectedFaceIndxs));
        }

        private bool EqualityWithTol(PolyLine3 poly1, PolyLine3 poly2)
        {
            if (poly1.Count != poly2.Count) return false;
            for (int i = 0; i < poly1.Count; i++)
            {
                var success = Math.Abs(poly1[i].X - poly2[i].X) < _tol && Math.Abs(poly1[i].Y - poly2[i].Y) < _tol &&
                              Math.Abs(poly1[i].Z - poly2[i].Z) < _tol;
                if (success == false) return false;
            }

            return true;
        }

        private static Karamba.Models.Model MakeNonConvexModel()
        {
            var k3d = new Toolkit();
            var mesh = new Mesh3();
            mesh.AddVertex(0, 0, 0.5);
            mesh.AddVertex(1, 0, 0);
            mesh.AddVertex(1, 1, 1);
            mesh.AddVertex(0, 1, 0);
            mesh.AddFace(0, 1, 2);
            mesh.AddFace(0, 2, 3);

            var supportConditions = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(0, supportConditions),
                k3d.Support.Support(1, supportConditions),
                k3d.Support.Support(2, supportConditions),
                k3d.Support.Support(3, supportConditions),
            };

            var loads = new List<Load>
            {
                k3d.Load.PointLoad(0, new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(1, new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(2, new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(3, new Vector3(0, 0, -5)),
            };

            var logger = new MessageLogger();
            var crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(
                25,
                0,
                null,
                new List<double> { 4, 4, -4, -4 },
                0);
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out _);
            var model = k3d.Model.AssembleModel(
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