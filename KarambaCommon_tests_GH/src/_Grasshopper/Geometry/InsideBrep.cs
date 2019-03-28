using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Karamba.GHopper.Geometry
{
    using Karamba.Models;
    using Karamba.Elements;
    using Karamba.Geometry;

    [Serializable]
    public class InsideBrep: IInsideBrep
    {
        private IReadOnlyList<Brep> breps_;
        private Model model_;

        public InsideBrep(IReadOnlyList<Brep> breps, Model model) {
            breps_ = breps;
            model_ = model;
        }

        /// <summary>
        /// tests whether the given nodes of a given model are inside the brep
        /// </summary>
        /// <param name="node_inds"></param>
        /// <returns></returns>
        public bool IsInside(IReadOnlyList<int> node_inds) {
            if (breps_.Count == 0) return true;
            foreach (var brep in breps_)
            {
                foreach (var node_ind in node_inds)
                {
#if UnitTest
                    throw new NotImplementedException();
#else
                    if (brep.IsPointInside(model_.nodes[node_ind].pos.Convert(), 1E-8, false))
                    {
                        return true;
                    }
#endif
                }
            }
            return false;
        }

        /// <summary>
        /// tests whether an element is inside the breps
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public bool IsInside(ModelElement elem) {
            if (breps_.Count == 0) return true;
            if (IsInside(elem.node_inds)) return true;

            // check whether the element axis intersects the brep               
            Curve axis = new LineCurve(model_.nodes[elem.node_inds[0]].pos.Convert(), model_.nodes[elem.node_inds[1]].pos.Convert());

            foreach (var brep in breps_)
            {
#if UnitTest
                throw new NotImplementedException();
#else
                Rhino.Geometry.Intersect.Intersection.CurveBrep(axis, brep, 1E-8,
                out Curve[] intersection_curves, out Point3d[] intersection_points);
                if (intersection_points.Length != 0)
                    return true;
#endif
            }
            return false;
        }

        /// <summary>
        /// tests whether an element is inside the breps
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public bool IsInside(ModelElementStraightLine elem) {
            if (breps_.Count == 0) return true;
            return IsInside(elem.node_inds);
        }
    }
}
