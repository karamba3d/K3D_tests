using feb;
using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Loads.Combinations;
//using KarambaCommon.src.Results.Features;
using Karamba.Results;
using Karamba.Supports;
using Karamba.Utilities;
using KarambaCommon;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Load = Karamba.Loads.Load;
using Karamba.Results.ShellSection;
using System;

namespace KarambaCommon.Tests.Result.ShellSection
{
    [TestFixture]
    public class ShellSecAlgorithmsUtilities_Tests
    {

        private Mesh3 MakeMesh()
        {
            Mesh3 mesh = new Mesh3();
            mesh.AddVertex(0, 0, 0);
            mesh.AddVertex(1, 0, 0);
            mesh.AddVertex(0, 1, 0.5);
            mesh.AddFace(0, 1, 2);
            return mesh;
        }

        private AABBTree MakeAABBTree(Mesh3 mesh)
        {
            //define karamba model
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            var crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(25, 0, null, new List<double> { 4, 4, -4, -4 }, 0);
            var shells = k3d.Part.MeshToShell(new List<Mesh3> { mesh }, null, new List<CroSec> { crosec }, logger, out var nodes);

            var supportConditions = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
            k3d.Support.Support(0, supportConditions),
            k3d.Support.Support(1, supportConditions),
            };

            var loads = new List<Load>
            {
            k3d.Load.PointLoad(2, new Vector3(), new Vector3(0, 25, 0))
            };

            var model = k3d.Model.AssembleModel(shells, supports, loads,
                out var info, out var mass, out var cog, out var message, out var any_warning);

            model = k3d.Algorithms.AnalyzeThI(model, out var maxDisplacements,
                out var gravityForce, out var elasticEnergy, out var warning);

            //get ShellMesh from karamba model                      
            feb.ShellMesh mesh_grp = new feb.ShellMesh();
            for (int ind = 0; ind < model.febmodel.numberOfTriMeshes(); ind++)
            {
                mesh_grp.add(model.febmodel.triMesh(ind));
            }
            mesh_grp.finalizeConstruction();


            //Build the AABB tree for the mesh
            var _mesh = mesh_grp.mesh();
            var aabb_tree = new feb.AABBTree();
            aabb_tree.add(_mesh);
            aabb_tree.build();
            return aabb_tree;
        }

        [Test]
        public void AABBTreeRayIntersection_FindIntersection()
        {

            Vector3 projVect = new Vector3(0, 0, -1);
            var mesh = MakeMesh();
            var aabbTree = MakeAABBTree(mesh);
            double tolerance = 1E-10;

            Point3[] inputPoints =
            {
                new Point3(0,1,0.5), //the vertex of the mesh           
                0.5 * new Point3(0, 1, 0.5) + 0.5 * new Point3(0, 0, 1), //point on mesh edge
                new Point3(0.4, 0.4, 0.4),  //point inside mesh 
                new Point3(1, 0, 2.5), //point above the vertex of the mesh  
                //new Point3(1, 0.6, 0.6),  //point outside mesh
            };
            Point3[] expectedPoints = new Point3[]
            {
                new Point3(0, 1, 0.5),
                new Point3(0, 0.5, 0.25),
                new Point3(0.4, 0.4, 0.2),
                new Point3(1, 0, 0),
            };
            Point3[] outputPoints = new Point3[4];



            for (int i = 0; i < inputPoints.Count(); i++)
            {
                Intersection intersection = ShellSecAlgorithms.RayAABBTreeIntersections(inputPoints[i], projVect, aabbTree);
                var pt = intersection.Values.ToList()[0];

                Assert.That(pt.Equals(expectedPoints[i], tolerance));
            }
        }

        [Test]
        public void AABBTreeRayIntersection_FindIntersection_NoValidIntersection()
        {
            Vector3 projVect = new Vector3(0, 0, -1);
            var mesh = MakeMesh();
            var aabbTree = MakeAABBTree(mesh);
            var inputPoints = new Point3(1, 0.6, 0.6);  //point outside mesh

            Intersection intersection = ShellSecAlgorithms.RayAABBTreeIntersections(inputPoints, projVect, aabbTree);

            Assert.That(intersection is null);
        }


        [Test]
        public void PlaneEdgeIntersection_Gives_Expected_Intersection_Points()
        {
            int size = 4;
            double tol = 1E-6;
            Vector3 vertex1 = new Vector3(0, 0, 0);
            Vector3 vertex2 = new Vector3(0, 1, 0.5);
            Vector3 planeNormal = new Vector3(0.301511, -0.904534, 0);

            Vector3[] pointsOnPlane = new Vector3[]
            {
                new Vector3(-0.2, 0.933333,0.2),
                new Vector3(-0.2, 0.2, 0.2),
                new Vector3(-0.2, -0.066667, 0.2),
                new Vector3(-0.2, -0.423389, 0.2),
            };

            Vector3[] expectedResVct =
            {
                new Vector3(0, 1, 0.5),
                new Vector3(0, 0.266667, 0.133333),
                new Vector3(0, 0, 0),
                new Vector3(0, -0.356722, -0.178361),
            };

            int[] expectedRes = new int[] { 2, 3, 1, -3 };

            for (int i = 0; i < size; i++)
            {
                int res = ShellSecAlgorithms.PlaneEdgeIntersection(vertex1, vertex2, planeNormal, pointsOnPlane[i], tol, out var x);
                Assert.AreEqual(expectedResVct[i].X, x.X, tol);
                Assert.AreEqual(expectedResVct[i].Y, x.Y, tol);
                Assert.AreEqual(expectedResVct[i].Z, x.Z, tol);
                Assert.AreEqual(expectedRes[i], res);
            }
        }
    }
}
