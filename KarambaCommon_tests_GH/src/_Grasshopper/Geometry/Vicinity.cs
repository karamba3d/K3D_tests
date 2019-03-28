using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Rhino.Geometry;

namespace Karamba.GHopper.Geometry
{
    using Karamba.Geometry;

    [Serializable, DataContract]
    public class Vicinity: IVicinity
    {
        /// <summary>
        /// positions of nodes to which the joint connects
        /// </summary>
        [DataMember]
        protected List<Point3d> to_points_;

        /// <summary>
        /// curves to which the joint connects
        /// </summary>
        [DataMember]
        protected List<Curve> to_curves_;

        /// <summary>
        /// lines to which the joint connects
        /// </summary>
        [DataMember]
        protected List<Line> to_lines_;

        /// <summary>
        /// planes to which the joint connects
        /// </summary>
        [DataMember]
        protected List<Plane> to_planes_;

        /// <summary>
        /// surfaces to which the joint connects
        /// </summary>
        [DataMember]
        protected List<Brep> to_breps_;

        /// <summary>
        /// meshes to which the joint connects
        /// </summary>
        [DataMember]
        protected List<Mesh> to_meshes_;

        protected double limit_dist_;

        public Vicinity(List<Point3d> to_points, List<Curve> to_curves, List<Line> to_lines, List<Plane> to_planes, List<Brep> to_breps, List<Mesh> to_meshes, double limit_dist = 1E-10) {
            to_points_ = to_points;
            to_curves_ = to_curves;
            to_lines_ = to_lines;
            to_planes_ = to_planes;
            to_breps_ = to_breps;
            to_meshes_ = to_meshes;
            limit_dist_ = limit_dist;
            foreach (var plane in to_planes)
            {
                if (!plane.IsValid)
                {
                    throw new Exception("Given to-plane is not valid: " + plane.ToString());
                }
            }
        }

        public bool IsNear(Point3 p) {
            Point3d p3d = p.Convert();

            foreach (Point3d to_point in to_points_)
            {
                if (to_point.DistanceTo(p3d) < limit_dist_)
                {
                    return true;
                }
            }

            foreach (Curve to_curve in to_curves_)
            {
#if UnitTest
                throw new NotImplementedException();
#else
                double t;
                if (to_curve.ClosestPoint(p3d, out t, limit_dist_))
                {
                    return true;
                }
#endif
            }

            foreach (var to_line in to_lines_)
            {
                if (to_line.DistanceTo(p3d, true)  <= limit_dist_)
                {
                    return true;
                }
            }

            foreach (Plane to_plane in to_planes_)
            {
                if (Math.Abs(to_plane.DistanceTo(p3d)) <= limit_dist_)
                {
                    return true;
                }

            }

            foreach (Brep to_brep in to_breps_)
            {
#if UnitTest
                throw new NotImplementedException();
#else
                Point3d closest_point;
                ComponentIndex ci;
                double s, t;
                Vector3d normal;
                if (to_brep.ClosestPoint(p3d, out closest_point, out ci, out s, out t,
                    limit_dist_, out normal))
                {
                    return true;
                }
#endif
            }

            foreach (Mesh to_mesh in to_meshes_)
            {
#if UnitTest
            throw new NotImplementedException();
#else
            if (to_mesh.ClosestPoint(p3d).DistanceTo(p3d) <= limit_dist_)
            {
                return true;
            }
#endif

            }
            return false;
        }
    }
}
