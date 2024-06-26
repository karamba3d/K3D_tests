﻿#if ALL_TESTS

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
    public class MarchingAlgorithmRetrieverTestOnOpenMeshes
    {
        private Karamba.Models.Model _model;
        private Vector3 _projectionVector;
        private double _tol;
        private double _delta;

        [OneTimeSetUp]
        public void Initialize()
        {
            // Create Mesh
            Mesh3 mesh = new Mesh3();
            mesh.AddVertex(0, 0, 0);
            mesh.AddVertex(0, 1, 0);
            mesh.AddVertex(0, 2, 0);
            mesh.AddVertex(1, 0, 0);
            mesh.AddVertex(1, 1, 0);
            mesh.AddVertex(1, 2, 0);
            mesh.AddVertex(2, 0, 0);
            mesh.AddVertex(2, 1, 0);
            mesh.AddVertex(2, 2, 0);

            mesh.AddFace(0, 3, 4);
            mesh.AddFace(1, 4, 5);
            mesh.AddFace(3, 6, 7);
            mesh.AddFace(4, 7, 8);
            mesh.AddFace(0, 4, 1);
            mesh.AddFace(1, 5, 2);
            mesh.AddFace(3, 7, 4);
            mesh.AddFace(4, 8, 5);

            // Create Model
            Toolkit k3d = new Toolkit();
            List<bool> supportConditions = new List<bool>() { true, true, true, true, true, true };
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(new Point3(0, 0, 0), supportConditions),
                k3d.Support.Support(new Point3(0, 1, 0), supportConditions),
                k3d.Support.Support(new Point3(0, 2, 0), supportConditions),
            };

            List<Load> loads = new List<Load>
            {
                k3d.Load.PointLoad(new Point3(2, 2, 0), new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(new Point3(2, 1, 0), new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(new Point3(2, 0, 0), new Vector3(0, 0, -5)),
            };

            MessageLogger logger = new MessageLogger();
            CroSec_Shell crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(
                25,
                0,
                null,
                new List<double> { 4, 4, -4, -4 });
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

            // Create projection Vector
            Vector3 projectionVector = new Vector3(0, 0, -1);

            // Create other definition
            double tol = 1E-12;
            double delta = 0.02;

            _model = model;
            _projectionVector = projectionVector;
            _tol = tol;
            _delta = delta;
        }

        private bool EqualityWithTol(PolyLine3 poly1, PolyLine3 poly2)
        {
            if (poly1.PointCount != poly2.PointCount)
                return false;
            for (int i = 0; i < poly1.PointCount; i++)
            {
                bool success = Math.Abs(poly1[i].X - poly2[i].X) < _tol && Math.Abs(poly1[i].Y - poly2[i].Y) < _tol &&
                              Math.Abs(poly1[i].Z - poly2[i].Z) < _tol;
                if (success == false)
                    return false;
            }

            return true;
        }

        private void MakeComparison(PolyLine3 polylineToProject, PolyLine3 expectedPolyline, List<int> expectedFaceIndices)
        {
            var info = new MarchingInfo(
                new ShellSectionGeometry(_model, _model.elems.OfType<ModelMembrane>()),
                polylineToProject,
                _projectionVector,
                _tol,
                _delta);
            var retriever = new MarchingAlgorithmRetriever(info);

            retriever.FindSection(out var allOutputPoly, out var allOutputFaces, out _);

            PolyLine3 outPolyline = allOutputPoly[0];

            Assert.That(outPolyline, Is.EqualTo(expectedPolyline).Using<PolyLine3>(EqualityWithTol));
            Assert.That(allOutputFaces[0], Is.EqualTo(expectedFaceIndices).Within(_tol));
        }

        [Test]
        [Description("Test_0: 2pt polyline")]
        public void FindSection_for_2pt_polyline_with_start_and_end_point_not_projecting_on_mesh()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(-1, 0.5, 1),
                new Point3(2.1, 0.5, 1),
            };

            PolyLine3 expectedPolyline = new PolyLine3()
            {
                new Point3(0, 0.5, 0),
                new Point3(0.5, 0.5, 0),
                new Point3(1, 0.5, 0),
                new Point3(1.5, 0.5, 0),
                new Point3(2, 0.5, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 4, 0, 6, 2 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_1: 2pt polyline: start project, end project")]
        public void FindSection_for_2pt_polyline_with_start_and_end_point_projecting_on_mesh()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(0.7, 0.5, 1),
                new Point3(1.3, 0.5, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0.7, 0.5, 0),
                new Point3(1, 0.5, 0),
                new Point3(1.3, 0.5, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0, 6 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_2: 2pt polyline: start project, end outside")]
        public void FindSection_for_2pt_polyline_with_just_start_point_projecting_on_mesh()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(0.7, 0.5, 1),
                new Point3(2.1, 0.5, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0.7, 0.5, 0),
                new Point3(1, 0.5, 0),
                new Point3(1.5, 0.5, 0),
                new Point3(2, 0.5, 0),
            };

            List<int> expectedFaceIndices = new List<int>()
            {
                0,
                6,
                2,
            };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_3: 2pt polyline: start outside, end project")]
        public void FindSection_for_2pt_polyline_with_just_end_point_projecting_on_mesh()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(2.1, 0.5, 1),
                new Point3(0.7, 0.5, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(2, 0.5, 0),
                new Point3(1.5, 0.5, 0),
                new Point3(1, 0.5, 0),
                new Point3(0.7, 0.5, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 2, 6, 0 };
            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_4: 2pt polyline: start on vertex, end project in the same face as start")]
        public void FindSection_for_2pt_polyline_projecting_on_same_mesh_face_with_start_project_on_vertex()
        {
            PolyLine3 polylineToProject = new PolyLine3()
            {
                new Point3(1, 1, 1),
                new Point3(0.8, 0.5, 1),
            };

            PolyLine3 expectedPolyline = new PolyLine3()
            {
                new Point3(1, 1, 0),
                new Point3(0.8, 0.5, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_5: 2pt polyline: start on vertex, end project in different face compared to start")]
        public void FindSection_for_2pt_polyline_projecting_on_different_mesh_faces_with_start_project_on_vertex()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(1, 1, 1),
                new Point3(1.8, 1.5, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(1, 1, 0),
                new Point3(1, 1, 0),
                new Point3(1.8, 1.5, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0, 3 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_6: 2pt polyline: start on edge, end project in the same face as start")]
        public void FindSection_for_2pt_polyline_projecting_on_same_mesh_face_with_start_project_on_edge()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(1, 0.5, 1),
                new Point3(0.5, 0.2, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(1, 0.5, 0),
                new Point3(0.5, 0.2, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_7: 2pt polyline: start on edge, end project in different face compared to start")]
        public void FindSection_for_2pt_polyline_projecting_on_different_mesh_faces_with_start_project_on_edge()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(1, 0.5, 1),
                new Point3(1.5, 0.9, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(1, 0.5, 0),
                new Point3(1, 0.5, 0),
                new Point3(1.5, 0.9, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0, 6 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_8: 2pt polyline: start on vertex, end on vertex in the same face as start")]
        public void FindSection_for_2pt_polyline_with_ending_points_projecting_on_vertices_of_same_mesh_face()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(1, 1, 1),
                new Point3(0, 0, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(1, 1, 0),
                new Point3(0, 0, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_9: 2pt polyline: start on vertex, end on vertex in different face compared to start")]
        public void FindSection_for_2pt_polyline_with_ending_points_projecting_on_vertices_of_different_mesh_faces()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(1, 1, 1),
                new Point3(2, 2, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(1, 1, 0),
                new Point3(1, 1, 0),
                new Point3(2, 2, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0, 3 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_10: 3pt polyline: start project, middle project, end project")]
        public void FindSection_for_3pt_polyline_all_points_projecting_on_mesh()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(0.1, 0.6, 1),
                new Point3(0.8, 0.4, 1),
                new Point3(1.2, 0.6, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0.1, 0.6, 0),
                new Point3(0.488888888888889, 0.488888888888889, 0),
                new Point3(0.8, 0.4, 0),
                new Point3(1, 0.5, 0),
                new Point3(1.2, 0.6, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 4, 0, 0, 6 };

            // Assert.Fail();
            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_11: 3pt polyline: start outside, middle project, end outside")]
        public void FindSection_for_3pt_polyline_start_and_end_projecting_outside_mesh()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(-0.1, 0.6, 1),
                new Point3(0.8, 0.4, 1),
                new Point3(2.1, 0.6, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0, 0.577777777777778, 0),
                new Point3(0.472727272727273, 0.472727272727273, 0),
                new Point3(0.8, 0.4, 0),
                new Point3(1, 0.430769230769231, 0),
                new Point3(1.50909090909091, 0.509090909090909, 0),
                new Point3(2, 0.584615384615385, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 4, 0, 0, 6, 2 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_12: 3pt polyline: middle on vertex, start project in same face, end project in same face")]
        public void
            FindSection_for_3pt_polyline_middle_pt_project_on_mesh_vertex_start_and_end_projecting_into_same_face()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(0, -0.5, 1),
                new Point3(1, 1, 1),
                new Point3(0.5, -0.5, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0.333333333333333, 0, 0),
                new Point3(1, 1, 0),
                new Point3(0.666666666666667, 0, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0, 0 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description(
            "Test_13: 3pt polyline: middle on vertex, start project on different face, end project on different face")]
        public void
            FindSection_for_3pt_polyline_middle_pt_project_on_mesh_vertex_start_end_and_middle_projecting_into_different_faces()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(-0.2, 0.5, 1),
                new Point3(1, 1, 1),
                new Point3(2.5, 1.5, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0, 0.583333333333333, 0),
                new Point3(1, 1, 0),
                new Point3(1, 1, 0),
                new Point3(1, 1, 0),
                new Point3(2, 1.33333333333333, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 4, 0, 0, 3 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_14: 3pt polyline: middle on vertex, start project in same face, end project in different")]
        public void
            FindSection_for_3pt_polyline_middle_pt_project_on_mesh_vertex_start_and_middle_projecting_into_same_faces()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(0, -0.5, 1),
                new Point3(1, 1, 1),
                new Point3(2.5, 1.5, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0.333333333333333, 0, 0),
                new Point3(1, 1, 0),
                new Point3(1, 1, 0),
                new Point3(2, 1.33333333333333, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0, 0, 3 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_15: 3pt polyline: middle on edge, start project in same face, end project in same face")]
        public void
            FindSection_for_3pt_polyline_middle_pt_project_on_mesh_edge_start_end_and_middle_projecting_into_same_face()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(0, -0.5, 1),
                new Point3(0.5, 0.5, 1),
                new Point3(0.5, -0.5, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0.25, 0, 0),
                new Point3(0.5, 0.5, 0),
                new Point3(0.5, 0, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0, 0 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description(
            "Test_16: 3pt polyline: middle on edge, start project on different face, end project on different face")]
        public void
            FindSection_for_3pt_polyline_middle_pt_project_on_mesh_edge_start_end_and_middle_projecting_into_different_faces()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(-0.2, 0.5, 1),
                new Point3(0.5, 0.5, 1),
                new Point3(-0.4, 0.8, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0, 0.5, 0),
                new Point3(0.5, 0.5, 0),
                new Point3(0.5, 0.5, 0),
                new Point3(0.5, 0.5, 0),
                new Point3(0, 0.666666666666667, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 4, 0, 0, 4 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        [Test]
        [Description("Test_17: 3pt polyline: middle on edge, start project in same face, end project in different")]
        public void FindSection_for_3pt_polyline_middle_pt_project_on_mesh_edge_just_start_and_middle_projecting_into_same_face()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(0, -0.5, 1),
                new Point3(0.5, 0.5, 1),
                new Point3(-0.4, 0.8, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0.25, 0, 0),
                new Point3(0.5, 0.5, 0),
                new Point3(0.5, 0.5, 0),
                new Point3(0, 0.666666666666667, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0, 0, 4 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        // [Test, Description("Test_18: 3pt polyline: middle on vertex, next direction march = edges direction")]
        private void FindSection_for_3pt_polyline_middle_pt_project_on_vertex_and_next_direction_marching_equal_edges_direction()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(-1, 3, 1),
                new Point3(1, 1, 1),
                new Point3(1, -1, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(0, 2, 0),
                new Point3(0.5, 1.5, 0),
                new Point3(1, 1, 0),
                new Point3(1, 1, 0),
                new Point3(1, 0, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 5, 1, 0, 0 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        // [Test, Description("Test_19: 3pt polyline: middle on vertex, prev direction march = edges direction")]
        private void FindSection_for_3pt_polyline_middle_pt_project_on_vertex_and_prev_direction_marching_equal_edges_direction()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(1, -1, 1),
                new Point3(1, 1, 1),
                new Point3(-1, 3, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(1, 0, 0),
                new Point3(1, 1, 0),
                new Point3(1, 1, 0),
                new Point3(0.5, 1.5, 0),
                new Point3(0, 2, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0, 0, 1, 5 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        // [Test, Description("Test_20: 3pt polyline: middle on vertex, next and prev direction march = edges direction ")]
        private void FindSection_for_3pt_polyline_middle_pt_project_on_vertex_and_both_next_and_prev_direction_marching_equal_edges_direction()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(1, -1, 1),
                new Point3(1, 1, 1),
                new Point3(1, 3, 1),
            };

            var expectedPolyline = new PolyLine3()
            {
                new Point3(1, 0, 0),
                new Point3(1, 1, 0),
                new Point3(1, 1, 0),
                new Point3(1, 2, 0),
            };

            List<int> expectedFaceIndices = new List<int>() { 0, 0, 1 };

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        // [Test, Description("Test_21: 3pt polyline: middle on edge, next direction march = edges direction")]
        private void FindSection_for_3pt_polyline_middle_pt_project_on_edge_and_next_direction_marching_equal_edges_direction()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(-1, 3.5, 1),
                new Point3(1, 0.5, 1),
                new Point3(1, -1, 1),
            };

            var expectedPolyline = new PolyLine3();
            List<int> expectedFaceIndices = new List<int>();

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        // [Test, Description("Test_22: 3pt polyline: middle on edge, prev direction march = edges direction")]
        private void FindSection_for_3pt_polyline_middle_pt_project_on_edge_and_prev_direction_marching_equal_edges_direction()
        {
            var polylineToProject = new PolyLine3()
            {
                new Point3(1, -1, 1),
                new Point3(1, 0.5, 1),
                new Point3(-1, 3.5, 1),
            };

            var expectedPolyline = new PolyLine3();
            List<int> expectedFaceIndices = new List<int>();

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }

        // [Test, Description("Test_23: 3pt polyline: middle on edge, next and prev direction march = edges direction")]
        private void FindSection_for_3pt_polyline_middle_pt_project_on_edge_and_both_next_and_prev_direction_marching_equal_edges_direction()
        {
            PolyLine3 polylineToProject = new PolyLine3()
            {
                new Point3(1, -1, 1),
                new Point3(1, 0.5, 1),
                new Point3(1, 3, 1),
            };

            var expectedPolyline = new PolyLine3();
            List<int> expectedFaceIndices = new List<int>();

            MakeComparison(polylineToProject, expectedPolyline, expectedFaceIndices);
        }
    }
}

#endif
