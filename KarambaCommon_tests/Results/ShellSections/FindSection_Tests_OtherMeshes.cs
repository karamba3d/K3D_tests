#if ALL_TESTS

namespace KarambaCommon.Tests.Result.ShellSection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using feb;
    using Karamba.CrossSections;
    using Karamba.Geometry;
    using Karamba.Loads.Combinations;

    // using KarambaCommon.src.Results.Features;
    using Karamba.Results;
    using Karamba.Results.ShellSection;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Load = Karamba.Loads.Load;

    [TestFixture]
    public class RetrieverFindSection_Tests_NonConvexMesh
    {
        private double _tol;

        [Test]
        public void FindSection_3ptPolyline_NonConvexMesh()
        {
            PolyLine3 inputPolyline = new PolyLine3(
                new Point3(0.5, -0.5, 1),
                new Point3(0.5, 0.8, 1),
                new Point3(0.5, 3.5, 1));
            Vector3 inputProjectionVector = new Vector3(0, 0, -1);
            var inputModel = MakeNonConvexModel();
            var tol = 1E-12;
            var delta = 0.02;
            var info = new RetrieverInfo_Force(inputModel, inputPolyline, inputProjectionVector, tol, delta, "0", new List<string> { string.Empty }, new List<Guid>());
            var strategy = new RetrieverStrategy_Force(1, 1);
            var sut = new Karamba.Results.ShellSection.Retriever(strategy, info);
            _tol = tol;

            sut.FindSection(out var outputPolylines, out var outputFaceIndxs);

            var expectedPolylines = new List<PolyLine3>()
            {
                new PolyLine3(new Point3(0.5, 0, 0), new Point3(0.5, 0.5, 0)),
                new PolyLine3(new Point3(0.5, 1, 0), new Point3(0.5, 1.5, 0), new Point3(0.5, 2, 0)),
            };
            var expectedFaceIndxs = new List<List<int>>() { new List<int>() { 0 }, new List<int>() { 1, 2 }, };

            Assert.That(outputPolylines, Is.EqualTo(expectedPolylines).Using<List<PolyLine3>>(EqualityWithTol));
            Assert.That(outputFaceIndxs, Is.EqualTo(expectedFaceIndxs));
        }

        private bool EqualityWithTol(List<PolyLine3> listPoly1, List<PolyLine3> listPoly2)
        {
            if (listPoly1.Count != listPoly2.Count) return false;
            for (int j = 0; j < listPoly1.Count; j++)
            {
                var poly1 = listPoly1[j];
                var poly2 = listPoly2[j];

                if (poly1.Count != poly2.Count) return false;
                for (int i = 0; i < poly1.Count; i++)
                {
                    var success = Math.Abs(poly1[i].X - poly2[i].X) < _tol &&
                                  Math.Abs(poly1[i].Y - poly2[i].Y) < _tol && Math.Abs(poly1[i].Z - poly2[i].Z) < _tol;
                    if (success == false) return false;
                }
            }

            return true;
        }

        private static Karamba.Models.Model MakeNonConvexModel()
        {
            var k3d = new Toolkit();
            var mesh = new Mesh3();
            mesh.AddVertex(0, 0, 0);
            mesh.AddVertex(1, 0, 0);
            mesh.AddVertex(1, 1, 0);
            mesh.AddVertex(0, 1, 0);
            mesh.AddVertex(1, 2, 0);
            mesh.AddVertex(0, 2, 0);
            mesh.AddFace(0, 1, 2);
            mesh.AddFace(3, 2, 4);
            mesh.AddFace(3, 4, 5);

            var supportConditions = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(0, supportConditions),
                k3d.Support.Support(3, supportConditions),
                k3d.Support.Support(5, supportConditions),
            };

            var loads = new List<Load>
            {
                k3d.Load.PointLoad(4, new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(2, new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(1, new Vector3(0, 0, -5)),
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