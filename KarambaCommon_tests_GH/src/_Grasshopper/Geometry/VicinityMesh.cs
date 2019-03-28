using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Karamba.GHopper.Geometry
{
    using Karamba.Geometry;
    using Karamba.Models;

    /// <summary>
    /// GH-wrapper for beam-sets
    /// </summary>
    [Serializable]
    public class VivinityMesh
    {
        /// <summary>
        /// Rhino meshes.
        /// </summary>
        private List<Rhino.Geometry.Mesh>  meshes_;

        public VivinityMesh(
            IEnumerable<IReadonlyMesh> meshes)
        {
            meshes_ = new List<Rhino.Geometry.Mesh>();
            foreach (var mesh in meshes)
            {
                meshes_.Add(mesh.Convert());
            }
        }

        public VivinityMesh(Model model)
        {
            meshes_ = new List<Rhino.Geometry.Mesh>();
            foreach (var mesh in model.Meshes)
            {
                meshes_.Add(mesh.Convert());
            }
        }

        /// <summary>
        /// constructs a line that intersects the shell mesh of a model from a 
        /// point close to the model.
        /// </summary>
        /// <param name="test_point">point close to the model</param>
        /// <param name="intLine">line that intersects the model</param>
        /// <returns>true if an intersection could be found</returns>
        public bool IntersectionLine(Point3 test_point, out Line3 intLine)
        {
            var pointOnModel = new Point3();
            var normalOnModel = new Vector3();
            var res = ClosestPoint(test_point, out pointOnModel, out normalOnModel);
            if (res)
            {
                double tol = 0.001;
                intLine = new Line3(
                    pointOnModel - tol * normalOnModel,
                    pointOnModel + tol * normalOnModel);
            }
            else
            {
                intLine = new Line3();
            }
            return res;
        }

        /// <summary>
        /// Determine whether there exists a closest point to any mesh whose distance
        /// to the test_point is smaller or equal than limit_dist.
        /// 
        /// </summary>
        /// <param name="testPoint">Test point.</param>
        /// <param name="limit_dist"></param>
        /// <returns>True if such a point exists, false otherwise.</returns>
        public bool HasClosestPoint(
            Point3 testPoint, double limit_dist)
        {
#if !UnitTest
            var min_dist = double.MaxValue;
            double dist;
#endif
            var tp = testPoint.Convert();
            foreach (var mesh in meshes_)
            {
#if UnitTest
                throw new NotImplementedException();
#else
                var mpoint = mesh.ClosestMeshPoint(
                    tp, double.MaxValue);
                dist = mpoint.Point.DistanceTo(tp);
                if (dist <= limit_dist && dist < min_dist)
                {
                    return true;
                }
#endif
            }
            return false;
        }

        /// <summary>
        /// Search for closest point on model-mesh and corresponding normal vector 
        /// when given an arbitrary point. A fe-model needs to exist before this 
        /// function gives correct results.
        /// </summary>
        /// <param name="test_point">point for which closest point on the mesh 
        /// of a model is desired</param>
        /// <param name="pointOnModel">point on model mesh which is closest to 
        /// given point</param>
        /// <param name="normalOnModel"> normal vector at model point closest to
        /// given point</param>
        /// 
        /// <returns>true if a closest model mesh-point exists</returns>
        public bool ClosestPoint(
            Point3 test_point, out Point3 pointOnModel, out Vector3 normalOnModel)
        {
            var res = false;
#if !UnitTest
            var min_dist = double.MaxValue;
            double dist;
#endif
            pointOnModel = new Point3();
            normalOnModel = new Vector3();
            var tp = test_point.Convert();
            foreach (var mesh in meshes_)
            {
#if UnitTest
                throw new NotImplementedException();
#else
                var mpoint = mesh.ClosestMeshPoint(
                    tp, double.MaxValue);
                dist = mpoint.Point.DistanceTo(tp);
                if (dist < min_dist)
                {
                    res = true;
                    pointOnModel = mpoint.Point.Convert();
                    // calculate face normal
                    var face = mesh.Faces[mpoint.FaceIndex];
                    normalOnModel = FaceNormal(mesh, face);
                    min_dist = dist;
                }
#endif
            }
            return res;
        }

        /// <summary>
        /// calculate face normal
        /// </summary>
        /// <param name="mesh">mesh in which face is contained</param>
        /// <param name="face">face for which normal is to be calculated</param>
        /// <returns>face normal</returns>
        private static Vector3 FaceNormal(
            Rhino.Geometry.Mesh mesh,
            MeshFace face)
        {
            var p1 = mesh.Vertices[face.A];
            var p2 = mesh.Vertices[face.B];
            var p3 = mesh.Vertices[face.C];
            var v1 = p2 - p1;
            var v2 = p3 - p1;
            var res = Vector3d.CrossProduct(v1, v2);
            double l = res.Length;
            if (l != 0.0)
            {
                res /= l;
            }
            return res.Convert();
        }

    }
}
