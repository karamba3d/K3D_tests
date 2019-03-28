using System.Collections.Generic;
using Karamba.Geometry;
using Rhino.Geometry;

namespace Karamba.GHopper.Geometry
{
    public static class GeometryExtensions
    {
        public static Vector3 Convert(this Vector3d vec) {
            return new Vector3(vec.X, vec.Y, vec.Z);
        }

        public static Vector3 Convert(this Vector3f vec)
        {
            return new Vector3(vec.X, vec.Y, vec.Z);
        }

        /// <summary>
        /// Convert karamba vector to rhino vector.
        /// </summary>
        /// <param name="v">Karamba vector.</param>
        /// <returns>Rhino vector.</returns>
        public static Vector3d Convert(this Vector3 v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static Point3 Convert(this Point3d poi)
        {
            return new Point3(poi.X, poi.Y, poi.Z);
        }

        public static Point3 Convert(this Point3f poi)
        {
            return new Point3(poi.X, poi.Y, poi.Z);
        }

        /// <summary>
        /// Convert karamba point to rhino <see cref="Point3d"/>.
        /// </summary>
        /// 
        /// <param name="v">Karamba point.</param>
        /// 
        /// <returns>Rhino point.</returns>
        public static Point3d Convert(this Point3 v)
        {
            return new Point3d(v.X, v.Y, v.Z);
        }

        /// <summary>
        /// Convert karamba point to rhino <see cref="Point3d"/>.
        /// </summary>
        /// 
        /// <param name="v">Karamba point.</param>
        /// 
        /// <returns>Rhino point.</returns>
        public static IEnumerable<Point3d> Convert(this IEnumerable<Point3> v)
        {
            var res = new List<Point3d>();
            foreach (var item in v) res.Add(item.Convert());
            return res;
        }

        /// <summary>
        /// Convert rhino transform to karamba transformation.
        /// </summary>
        /// 
        /// <param name="xform">Rhino transform.</param>
        /// <returns>Karamba transformation.</returns>
        public static Transform3 Convert(this Transform xform)
        {
            // row-major order.
            return new Transform3(
                xform.ToFloatArray(true));
        }

        public static Karamba.Geometry.BoundingBox3 Convert(
           this Rhino.Geometry.BoundingBox box)
        {
            return new Karamba.Geometry.BoundingBox3(
                box.Min.Convert(),
                box.Max.Convert());
        }

        public static Rhino.Geometry.BoundingBox Convert(
            this Karamba.Geometry.BoundingBox3 box)
        {
            return new Rhino.Geometry.BoundingBox(
                box.Min.Convert(),
                box.Max.Convert());
        }

        /// <summary>
        /// Convert kamaba3D segment to rhino <see cref="Line"/>.
        /// </summary>
        /// 
        /// <param name="line">Karamba line.</param>
        /// 
        /// <returns>Rhino line.</returns>
        public static Line Convert(this Line3 line)
        {
            return new Line(line.PointAtStart.Convert(), line.PointAtEnd.Convert());
        }

        /// <summary>
        /// Convert rhino line to karamba segment.
        /// </summary>
        /// 
        /// <param name="line">Rhino line.</param>
        /// <returns>Karamba segment.</returns>
        public static Line3 Convert(this Line line)
        {
            return new Line3(line.From.Convert(), line.To.Convert());
        }

        public static Plane3 Convert(this Plane plane)
        {
            return new Plane3(plane.Origin.Convert(), plane.XAxis.Convert(), plane.YAxis.Convert());
        }

        /// <summary>
        /// Create rhino plane from karamba plane.
        /// </summary>
        /// 
        /// <param name="p"></param>
        /// <returns></returns>
        public static Plane Convert(this Plane3 p)
        {
            return new Plane(p.Origin.Convert(), p.XAxis.Convert(), p.YAxis.Convert());
        }

        /// <summary>
        /// Create rhino <see cref="PolyCurve"/> from karamba polyline.
        /// </summary>
        /// 
        /// <param name="p">Polyline.</param>
        /// <returns></returns>
        public static PolylineCurve Convert(this PolyLine3 p)
        {
            var ps = new List<Point3d>();
            foreach (var pt in p.Points)
            {
                ps.Add(pt.Convert());
            }
            return new PolylineCurve(ps);
        }
    }
}
