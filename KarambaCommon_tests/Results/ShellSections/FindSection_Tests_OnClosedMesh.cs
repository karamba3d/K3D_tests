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
    public class RetrieverFindSection_Tests_OnClosedMesh
    {
        [OneTimeSetUp]
        public void Initialize()
        {
            // Create Mesh
            var mesh = new Mesh3();
            mesh.AddVertex(0, 0, 0);
            mesh.AddVertex(0, 1, 0);
            mesh.AddVertex(1, 0, 0);
            mesh.AddVertex(1, 1, 0);
            mesh.AddVertex(2, 0, 0);
            mesh.AddVertex(2, 1, 0);
            mesh.AddVertex(0, 0, 0);
            mesh.AddVertex(1, 0, 0);
            mesh.AddVertex(2, 0, 0);
            mesh.AddVertex(0, 0, 1);
            mesh.AddVertex(1, 0, 1);
            mesh.AddVertex(2, 0, 1);
            mesh.AddVertex(0, 0, 2);
            mesh.AddVertex(1, 0, 2);
            mesh.AddVertex(2, 0, 2);
            mesh.AddVertex(2, 0, 0);
            mesh.AddVertex(2, 1, 0);
            mesh.AddVertex(2, 0, 1);
            mesh.AddVertex(2, 1, 1);
            mesh.AddVertex(2, 0, 2);
            mesh.AddVertex(2, 1, 2);
            mesh.AddVertex(2, 1, 0);
            mesh.AddVertex(1, 1, 0);
            mesh.AddVertex(0, 1, 0);
            mesh.AddVertex(2, 1, 1);
            mesh.AddVertex(1, 1, 1);
            mesh.AddVertex(0, 1, 1);
            mesh.AddVertex(2, 1, 2);
            mesh.AddVertex(1, 1, 2);
            mesh.AddVertex(0, 1, 2);
            mesh.AddVertex(0, 1, 0);
            mesh.AddVertex(0, 0, 0);
            mesh.AddVertex(0, 1, 1);
            mesh.AddVertex(0, 0, 1);
            mesh.AddVertex(0, 1, 2);
            mesh.AddVertex(0, 0, 2);
            mesh.AddVertex(0, 0, 2);
            mesh.AddVertex(1, 0, 2);
            mesh.AddVertex(2, 0, 2);
            mesh.AddVertex(0, 1, 2);
            mesh.AddVertex(1, 1, 2);
            mesh.AddVertex(2, 1, 2);
            mesh.AddFace(0, 1, 3);
            mesh.AddFace(2, 3, 5);
            mesh.AddFace(6, 7, 10);
            mesh.AddFace(7, 8, 11);
            mesh.AddFace(9, 10, 13);
            mesh.AddFace(10, 11, 14);
            mesh.AddFace(15, 16, 18);
            mesh.AddFace(17, 18, 20);
            mesh.AddFace(21, 22, 25);
            mesh.AddFace(22, 23, 26);
            mesh.AddFace(24, 25, 28);
            mesh.AddFace(25, 26, 29);
            mesh.AddFace(30, 31, 33);
            mesh.AddFace(32, 33, 35);
            mesh.AddFace(36, 37, 40);
            mesh.AddFace(37, 38, 41);
            mesh.AddFace(0, 3, 2);
            mesh.AddFace(2, 5, 4);
            mesh.AddFace(6, 10, 9);
            mesh.AddFace(7, 11, 10);
            mesh.AddFace(9, 13, 12);
            mesh.AddFace(10, 14, 13);
            mesh.AddFace(15, 18, 17);
            mesh.AddFace(17, 20, 19);
            mesh.AddFace(21, 25, 24);
            mesh.AddFace(22, 26, 25);
            mesh.AddFace(24, 28, 27);
            mesh.AddFace(25, 29, 28);
            mesh.AddFace(30, 33, 32);
            mesh.AddFace(32, 35, 34);
            mesh.AddFace(36, 40, 39);
            mesh.AddFace(37, 41, 40);

            // Create Model
            var k3d = new Toolkit();
            var supportConditions = new List<bool>() { true, true, true, true, true, true, };
            var supports = new List<Support>
            {
                k3d.Support.Support(new Point3(0, 0, 0), supportConditions),
                k3d.Support.Support(new Point3(0, 1, 0), supportConditions),
                k3d.Support.Support(new Point3(1, 0, 0), supportConditions),
                k3d.Support.Support(new Point3(1, 1, 0), supportConditions),
                k3d.Support.Support(new Point3(2, 0, 0), supportConditions),
                k3d.Support.Support(new Point3(2, 1, 0), supportConditions),
            };
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(new Point3(0, 0, 2), new Vector3(1000, 0, 0)),
                k3d.Load.PointLoad(new Point3(1, 0, 2), new Vector3(1000, 0, 0)),
                k3d.Load.PointLoad(new Point3(2, 0, 2), new Vector3(1000, 0, 0)),
                k3d.Load.PointLoad(new Point3(2, 1, 2), new Vector3(1000, 0, 0)),
                k3d.Load.PointLoad(new Point3(1, 1, 2), new Vector3(1000, 0, 0)),
                k3d.Load.PointLoad(new Point3(0, 1, 2), new Vector3(1000, 0, 0)),
            };

            var logger = new MessageLogger();
            var crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(
                25,
                0,
                null,
                new List<double> { 4, 4, -4, -4, },
                0);
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh, },
                null,
                new List<CroSec> { crosec, },
                logger,
                out _);
            var model = k3d.Model.AssembleModel(shells, supports, loads, out _, out _, out _, out _, out _);
            model = k3d.Algorithms.AnalyzeThI(model, out _, out _, out _, out _);

            // Create projection Vector
            Vector3 proj_vect = new Vector3(0, 1, 0);

            // Create other definition
            var tol = 1E-12;
            var delta = 0.02;

            _model = model;
            _projectionVector = proj_vect;
            _tol = tol;
            _delta = delta;
        }

        private Karamba.Models.Model _model;
        private Vector3 _projectionVector;
        private double _tol;
        private double _delta;

        private static PolyLine3 MakeNewPolyline(params Point3[] pts)
        {
            return new PolyLine3(pts);
        }

        private Karamba.Results.ShellSection.Retriever GetNewRetriever(PolyLine3 poly)
        {
            var info = new RetrieverInfo_Force(_model, poly, _projectionVector, _tol, _delta, "0", new List<string> { string.Empty }, new List<Guid>());
            var strategy = new RetrieverStrategy_Force(1, 1);
            return new Karamba.Results.ShellSection.Retriever(strategy, info);
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

        private void MakeComparison(
            PolyLine3 polylineToProject,
            PolyLine3 expectedPolyline,
            List<int> expectedfaceIndxs)
        {
            var sut = GetNewRetriever(polylineToProject);

            sut.FindSection(out var all_output_poly, out var all_output_faces);

            Assert.That(all_output_poly[0], Is.EqualTo(expectedPolyline).Using<PolyLine3>(EqualityWithTol));
            Assert.That(all_output_faces[0], Is.EqualTo(expectedfaceIndxs).Within(_tol));
        }

        [Test]
        [Description("Test_0: 2pt polyline")]
        public void FindSection_2pt_polyline_start_and_end_not_project_on_mesh()
        {
            var in_poly = MakeNewPolyline(new Point3(-1, -1, 0.5), new Point3(3, -1, 0.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 0.5),
                new Point3(1.5, 0, 0.5),
                new Point3(2, 0, 0.5),
                new Point3(2, 0.5, 0.5),
                new Point3(2, 1, 0.5),
                new Point3(1.5, 1, 0.5),
                new Point3(1, 1, 0.5),
                new Point3(0.5, 1, 0.5),
                new Point3(0, 1, 0.5),
                new Point3(0, 0.5, 0.5),
                new Point3(0, 0, 0.5),
                new Point3(0.5, 0, 0.5),
                new Point3(1, 0, 0.5));
            var exp_faces = new List<int>()
            {
                19,
                3,
                22,
                6,
                24,
                8,
                25,
                9,
                28,
                12,
                18,
                2,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_1: 2pt polyline: start project, end project")]
        public void FindSection_2pt_polyline_start_and_end_project_on_mesh()
        {
            var in_poly = MakeNewPolyline(new Point3(0.3, -1, 0.5), new Point3(1.8, -1, 0.5));
            var exp_poly = MakeNewPolyline(
                new Point3(0.3, 0, 0.5),
                new Point3(0.5, 0, 0.5),
                new Point3(1, 0, 0.5),
                new Point3(1.5, 0, 0.5),
                new Point3(1.8, 0, 0.5));
            var exp_faces = new List<int>() { 18, 2, 19, 3, };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_2: 2pt polyline: start project, end outside")]
        public void FindSection_2pt_polyline_just_start_project_on_mesh()
        {
            var in_poly = MakeNewPolyline(new Point3(0.3, -1, 0.5), new Point3(3, -1, 0.5));
            var exp_poly = MakeNewPolyline(
                new Point3(0.3, 1, 0.5),
                new Point3(0.5, 1, 0.5),
                new Point3(1, 1, 0.5),
                new Point3(1.5, 1, 0.5),
                new Point3(2, 1, 0.5),
                new Point3(2, 0.5, 0.5),
                new Point3(2, 0, 0.5),
                new Point3(1.5, 0, 0.5),
                new Point3(1, 0, 0.5),
                new Point3(0.5, 0, 0.5),
                new Point3(0.3, 0, 0.5));

            var exp_faces = new List<int>()
            {
                9, 25, 8, 24, 6, 22, 3, 19, 2, 18,
            };
            exp_poly.Reverse();
            exp_faces.Reverse();

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_3: 2pt polyline: start outside, end project")]
        public void FindSection_2pt_polyline_just_end_project_on_mesh()
        {
            var in_poly = MakeNewPolyline(new Point3(-1, -1, 0.5), new Point3(1.8, -1, 0.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1.8, 1, 0.5),
                new Point3(1.5, 1, 0.5),
                new Point3(1, 1, 0.5),
                new Point3(0.5, 1, 0.5),
                new Point3(0, 1, 0.5),
                new Point3(0, 0.5, 0.5),
                new Point3(0, 0, 0.5),
                new Point3(0.5, 0, 0.5),
                new Point3(1, 0, 0.5),
                new Point3(1.5, 0, 0.5),
                new Point3(1.8, 0, 0.5));
            var exp_faces = new List<int>()
            {
                24, 8, 25, 9, 28, 12, 18, 2, 19, 3,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_4: 2pt polyline: starting point on edge")]
        public void FindSection_2pt_polyline_start_project_on_edge()
        {
            var in_poly = MakeNewPolyline(new Point3(-1, -1, 0.5), new Point3(3, -1, 0.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 0.5),
                new Point3(1.5, 0, 0.5),
                new Point3(2, 0, 0.5),
                new Point3(2, 0.5, 0.5),
                new Point3(2, 1, 0.5),
                new Point3(1.5, 1, 0.5),
                new Point3(1, 1, 0.5),
                new Point3(0.5, 1, 0.5),
                new Point3(0, 1, 0.5),
                new Point3(0, 0.5, 0.5),
                new Point3(0, 0, 0.5),
                new Point3(0.5, 0, 0.5),
                new Point3(1, 0, 0.5));
            var exp_faces = new List<int>()
            {
                19,
                3,
                22,
                6,
                24,
                8,
                25,
                9,
                28,
                12,
                18,
                2,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        // [Test, Description("Test_5: 2pt polyline: starting point on vertex, next and prev direction march = edges direction")]
        public void FindSection_2pt_polyline_start_project_on_vertex_with_next_prev_march_direction_equal_edges()
        {
            var in_poly = MakeNewPolyline(new Point3(-1, -1, 1), new Point3(3, -1, 1));
            var exp_poly = MakeNewPolyline();
            var exp_faces = new List<int>() { };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_6: 2pt polyline: starting point on vertex, start outside, end outside n")]
        public void FindSection_2pt_polyline_startPt_endPt_project_outside_and_startingPt_project_on_vertex()
        {
            var in_poly = MakeNewPolyline(new Point3(-1, -1, 1.5), new Point3(3, -1, 0.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 1, 1),
                new Point3(0, 1, 1.25),
                new Point3(0, 0.75, 1.25),
                new Point3(0, 0, 1.25),
                new Point3(0.2, 0, 1.2),
                new Point3(1, 0, 1),
                new Point3(1, 0, 1),
                new Point3(1.8, 0, 0.8),
                new Point3(2, 0, 0.75),
                new Point3(2, 0.75, 0.75),
                new Point3(2, 1, 0.75),
                new Point3(1, 1, 1));
            var exp_faces = new List<int>()
            {
                11,
                29,
                13,
                20,
                4,
                2,
                19,
                3,
                22,
                6,
                24,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_7: 2pt polyline: start on edge, end outside")]
        public void FindSection_2pt_polyline_startPtProjectOnEdge_endPtProjectOutside()
        {
            var in_poly = MakeNewPolyline(new Point3(1, -1, 0.5), new Point3(3, -1, 0.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 0.5),
                new Point3(1, 0, 0.5),
                new Point3(1.5, 0, 0.5),
                new Point3(2, 0, 0.5),
                new Point3(2, 0.5, 0.5),
                new Point3(2, 1, 0.5),
                new Point3(1.5, 1, 0.5),
                new Point3(1, 1, 0.5));
            var exp_faces = new List<int>() { 2, 19, 3, 22, 6, 24, 8, };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_8: 2pt polyline: start on vertex, end outside")]
        public void FindSection_2pt_polyline_startPtProjectOnVertex_endPtProjectOutside()
        {
            var in_poly = MakeNewPolyline(new Point3(1, -1, 1), new Point3(3, -1, 1.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 1),
                new Point3(1, 0, 1),
                new Point3(2, 0, 1.25),
                new Point3(2, 0.25, 1.25),
                new Point3(2, 1, 1.25),
                new Point3(1.8, 1, 1.2),
                new Point3(1, 1, 1));
            var exp_faces = new List<int>() { 2, 5, 23, 7, 26, 10, };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_9: 2pt polyline: start on edge, next direction march = edges direction")]
        public void FindSection_2pt_polyline_startPtProjectOnEdge_NextMarchDirectionEqualEdgeDirection()
        {
            var in_poly = MakeNewPolyline(new Point3(1, -1, 0.5), new Point3(1, -1, 3.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 0.5),
                new Point3(1, 0, 1),
                new Point3(1, 0, 2),
                new Point3(1, 1, 2),
                new Point3(1, 1, 1),
                new Point3(1, 1, 0.5));
            var exp_faces = new List<int>() { 2, 4, 14, 10, 8, };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_10: 2pt polyline: start on vertex, next direction march = edges direction")]
        public void FindSection_2pt_polyline_startPtProjectOnVertex_NextMarchDirectionEqualEdgeDirection()
        {
            var in_poly = MakeNewPolyline(new Point3(1, -1, 1), new Point3(2.5, -1, 1));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 1),
                new Point3(1, 0, 1),
                new Point3(2, 0, 1),
                new Point3(2, 1, 1),
                new Point3(1, 1, 1));
            var exp_faces = new List<int>() { 2, 5, 7, 10, };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_11: 3pt polyline: start outside, middle project, end outside")]
        public void FindSection_3PtPolyline_startPtOutside_middlePtProject_endPtOutside()
        {
            var in_poly = MakeNewPolyline(new Point3(-1, -1, 0.5), new Point3(0.8, -1, 0.5), new Point3(1, -1, -1));
            var exp_poly = MakeNewPolyline(
                new Point3(0.8, 0, 0.5),
                new Point3(0.866666666666667, 0, 0),
                new Point3(0.866666666666667, 0.866666666666667, 0),
                new Point3(0.866666666666667, 1, 0),
                new Point3(0.846153846153846, 1, 0.153846153846154),
                new Point3(0.8, 1, 0.5),
                new Point3(0.5, 1, 0.5),
                new Point3(0, 1, 0.5),
                new Point3(0, 0.5, 0.5),
                new Point3(0, 0, 0.5),
                new Point3(0.5, 0, 0.5),
                new Point3(0.8, 0, 0.5));
            var exp_faces = new List<int>()
            {
                2,
                16,
                0,
                9,
                25,
                25,
                9,
                28,
                12,
                18,
                2,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_12: 3pt polyline: middle on edge, next and prev march no cross projection face")]
        public void FindSection_3PtPolyline_middlePtProjectOnEdge_projectionFaceNotCrossed()
        {
            var in_poly = MakeNewPolyline(new Point3(2.5, -1, 0.8), new Point3(1, -1, 0.5), new Point3(2, -1, -1));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 0.5),
                new Point3(1, 0, 0.5),
                new Point3(1.2, 0, 0.2),
                new Point3(1.33333333333333, 0, 0),
                new Point3(1.33333333333333, 0.333333333333333, 0),
                new Point3(1.33333333333333, 1, 0),
                new Point3(1, 1, 0.5),
                new Point3(1.41666666666667, 1, 0.583333333333333),
                new Point3(2, 1, 0.7),
                new Point3(2, 0.7, 0.7),
                new Point3(2, 0, 0.7),
                new Point3(1.625, 0, 0.625),
                new Point3(1, 0, 0.5));
            var exp_faces = new List<int>()
            {
                2,
                19,
                3,
                17,
                1,
                8,
                8,
                24,
                6,
                22,
                3,
                19,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description(
            "Test_13: 3pt polyline: middle on edge, next march cross proj face, prev march no cross projection face")]
        public void FindSection_3PtPolyline_middlePtProjectOnEdge_projectionFaceCrossedInNextDirection()
        {
            var in_poly = MakeNewPolyline(new Point3(2.5, -1, 0.8), new Point3(1, -1, 0.5), new Point3(0, -1, -1));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 0.5),
                new Point3(0.666666666666667, 0, 0),
                new Point3(0.666666666666667, 0.666666666666667, 0),
                new Point3(0.666666666666667, 1, 0),
                new Point3(0.8, 1, 0.2),
                new Point3(1, 1, 0.5),
                new Point3(1, 1, 0.5),
                new Point3(1.41666666666667, 1, 0.583333333333333),
                new Point3(2, 1, 0.7),
                new Point3(2, 0.7, 0.7),
                new Point3(2, 0, 0.7),
                new Point3(1.625, 0, 0.625),
                new Point3(1, 0, 0.5));
            var exp_faces = new List<int>()
            {
                2,
                16,
                0,
                9,
                25,
                25,
                8,
                24,
                6,
                22,
                3,
                19,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_14: 3pt polyline: middle on edge, next and prev march cross projection face")]
        public void FindSection_3PtPolyline_middlePtProjectOnEdge_projectionFaceCrossedBothDirection()
        {
            var in_poly = MakeNewPolyline(
                new Point3(0.7, -1, -0.5),
                new Point3(1, -1, 0.5),
                new Point3(-0.15, -1, -0.3));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 0.5),
                new Point3(0.28125, 0, 0),
                new Point3(0.28125, 0.28125, 0),
                new Point3(0.28125, 1, 0),
                new Point3(0.705128205128205, 1, 0.294871794871795),
                new Point3(1, 1, 0.5),
                new Point3(0.884615384615385, 1, 0.115384615384615),
                new Point3(0.85, 1, 0),
                new Point3(0.85, 0.85, 0),
                new Point3(0.85, 0, 0),
                new Point3(1, 0, 0.5));
            var exp_faces = new List<int>()
            {
                2, 16, 0, 9, 25, 25, 9, 0, 16, 2,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_15: 3pt polyline: middle on vertex, next and prev march no cross projection face")]
        public void FindSection_3PtPolyline_middlePtProjectOnVertex_projectionFaceNotCrossed()
        {
            var in_poly = MakeNewPolyline(new Point3(2.5, -1, 0.8), new Point3(1, -1, 1), new Point3(2, -1, -1));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 1),
                new Point3(1, 0, 1),
                new Point3(1.33333333333333, 0, 0.333333333333333),
                new Point3(1.5, 0, 0),
                new Point3(1.5, 0.5, 0),
                new Point3(1.5, 1, 0),
                new Point3(1, 1, 1),
                new Point3(1, 1, 1),
                new Point3(2, 1, 0.866666666666667),
                new Point3(2, 0.866666666666667, 0.866666666666667),
                new Point3(2, 0, 0.866666666666667),
                new Point3(1.88235294117647, 0, 0.882352941176471),
                new Point3(1, 0, 1));
            var exp_faces = new List<int>()
            {
                2,
                19,
                3,
                17,
                1,
                8,
                8,
                24,
                6,
                22,
                3,
                19,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description(
            "Test_16: 3pt polyline: middle on vertex, next march cross proj face, prev march no cross projection face")]
        public void FindSection_3PtPolyline_middlePtProjectOnVertex_projectionFaceCrossedInNextDirection()
        {
            var in_poly = MakeNewPolyline(new Point3(2.5, -1, 0.8), new Point3(1, -1, 1), new Point3(0, -1, -1));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 1),
                new Point3(0.5, 0, 0),
                new Point3(0.5, 0.5, 0),
                new Point3(0.5, 1, 0),
                new Point3(0.666666666666667, 1, 0.333333333333333),
                new Point3(1, 1, 1),
                new Point3(1, 1, 1),
                new Point3(2, 1, 0.866666666666667),
                new Point3(2, 0.866666666666667, 0.866666666666667),
                new Point3(2, 0, 0.866666666666667),
                new Point3(1.88235294117647, 0, 0.882352941176471),
                new Point3(1, 0, 1));
            var exp_faces = new List<int>()
            {
                2,
                16,
                0,
                9,
                25,
                25,
                24,
                6,
                22,
                3,
                19,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_17: 3pt polyline: middle on vertex, next and prev march cross projection face")]
        public void FindSection_3PtPolyline_middlePtProjectOnVertex_projectionFaceCrossedBothDirection()
        {
            var in_poly = MakeNewPolyline(new Point3(0.7, -1, -0.5), new Point3(1, -1, 1), new Point3(-0.15, -1, -0.3));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 1),
                new Point3(0.115384615384615, 0, 0),
                new Point3(0.115384615384615, 0.115384615384615, 0),
                new Point3(0.115384615384615, 1, 0),
                new Point3(0.530612244897959, 1, 0.469387755102041),
                new Point3(1, 1, 1),
                new Point3(0.833333333333333, 1, 0.166666666666667),
                new Point3(0.8, 1, 0),
                new Point3(0.8, 0.8, 0),
                new Point3(0.8, 0, 0),
                new Point3(1, 0, 1));
            var exp_faces = new List<int>()
            {
                2, 16, 0, 9, 25, 25, 9, 0, 16, 2,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_18: 3pt polyline: middle on edge, next march = edges direction")]
        public void FindSection_3PtPolyline_middlePtProjectOnEdge_NextMarchDirectionEqualsEdgeDirection()
        {
            var in_poly = MakeNewPolyline(new Point3(0.7, -1, -0.5), new Point3(1, -1, 0.5), new Point3(1, -1, 3.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 0.5),
                new Point3(1, 0, 1),
                new Point3(1, 0, 2),
                new Point3(1, 1, 2),
                new Point3(1, 1, 1),
                new Point3(1, 1, 0.5),
                new Point3(1, 1, 0.5),
                new Point3(0.884615384615385, 1, 0.115384615384615),
                new Point3(0.85, 1, 0),
                new Point3(0.85, 0.85, 0),
                new Point3(0.85, 0, 0),
                new Point3(1, 0, 0.5));
            var exp_faces = new List<int>()
            {
                2,
                4,
                14,
                10,
                8,
                8,
                25,
                9,
                0,
                16,
                2,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_19: 3pt polyline: middle on edge, prev march = edges direction")]
        public void FindSection_3PtPolyline_middlePtProjectOnEdge_PrevMarchDirectionEqualsEdgeDirection()
        {
            var in_poly = MakeNewPolyline(new Point3(1, -1, 3.5), new Point3(1, -1, 0.5), new Point3(0.7, -1, -0.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 0.5),
                new Point3(0.85, 0, 0),
                new Point3(0.85, 0.85, 0),
                new Point3(0.85, 1, 0),
                new Point3(0.884615384615384, 1, 0.115384615384616),
                new Point3(1, 1, 0.5),
                new Point3(1, 1, 1),
                new Point3(1, 1, 2),
                new Point3(1, 0, 2),
                new Point3(1, 0, 1),
                new Point3(1, 0, 0.5));
            var exp_faces = new List<int>()
            {
                2, 16, 0, 9, 25, 25, 10, 14, 4, 2,
            };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description("Test_20: 3pt polyline: middle on edge, next and prev march = edges direction")]
        public void FindSection_3PtPolyline_middlePtProjectOnEdge_PrevNextMarchDirectionEqualsEdgeDirection()
        {
            var in_poly = MakeNewPolyline(new Point3(1, -1, -0.5), new Point3(1, -1, 0.5), new Point3(1, -1, 3.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 0.5),
                new Point3(1, 0, 1),
                new Point3(1, 0, 2),
                new Point3(1, 1, 2),
                new Point3(1, 1, 1),
                new Point3(1, 1, 0.5),
                new Point3(1, 1, 0),
                new Point3(1, 0, 0),
                new Point3(1, 0, 0.5));
            var exp_faces = new List<int>() { 2, 4, 14, 10, 8, 8, 1, 2, };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description(
            "Test_21: 3pt polyline: middle on vertex, next and prev march = edges direction, next cross projection face")]
        public void
            FindSection_3PtPolyline_middlePtProjectOnEdge_PrevNextMarchDirectionEqualsEdgeDirection_ProjectionFaceCrossedInNextDir()
        {
            var in_poly = MakeNewPolyline(new Point3(1, -1, 3.5), new Point3(1, -1, 1), new Point3(1, -1, -0.5));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 1),
                new Point3(1, 0, 0),
                new Point3(1, 1, 0),
                new Point3(1, 1, 1),
                new Point3(1, 1, 1),
                new Point3(1, 1, 2),
                new Point3(1, 0, 2),
                new Point3(1, 0, 1));
            var exp_faces = new List<int>() { 2, 1, 8, 8, 10, 14, 4, };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }

        [Test]
        [Description(
            "Test_22: 3pt polyline: middle on vertex, next and prev march = edges direction, no cross projection face")]
        public void
            FindSection_3PtPolyline_middlePtProjectOnEdge_PrevNextMarchDirectionEqualsEdgeDirection_ProjectionFaceNotCrossed()
        {
            var in_poly = MakeNewPolyline(new Point3(3, -1, 3), new Point3(1, -1, 1), new Point3(3, -1, 1));
            var exp_poly = MakeNewPolyline(
                new Point3(1, 0, 1),
                new Point3(1, 0, 1),
                new Point3(2, 0, 1),
                new Point3(2, 1, 1),
                new Point3(1, 1, 1),
                new Point3(1.5, 1, 1.5),
                new Point3(2, 1, 2),
                new Point3(2, 0, 2),
                new Point3(1, 0, 1));
            var exp_faces = new List<int>() { 2, 5, 7, 10, 10, 26, 15, 5, };

            MakeComparison(in_poly, exp_poly, exp_faces);
        }
    }
}

#endif