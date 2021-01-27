using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using feb;
using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Supports;
using Karamba.Models;
using Karamba.Utilities;
using Karamba.Algorithms;

namespace KarambaCommon.Tests.Loads
{

#if __ALL_TESTS
    [TestFixture]
    public class LoadsOnBeam_tests
    {

        /// <summary>
        /// this tests creates feb-beam loads
        /// </summary>
        [Test]
        public void create_feb_loads()
        {
            // a point load on a beam or truss
            var P = new Vec3d(1, 2, 3); // load vector;
            var orientation = feb.Load.loadOrientation.global; // definition of P with respect to the global coordinate system
            // var orientation = feb.Load.loadOrientation.local;// definition of P with respect to the beam's local  global coordinate system
            // var orientation = feb.Load.loadOrientation.proj; // projected to the coordinate planes of the global coordinate system
            double pos = 1.0; // starting position of load
            var point_load_translational_on_beam = new feb.LoadTranslationalPoint(P, orientation, pos);

            // a point moment on a beam or truss
            // not yet implemented to give results when added to a model
            var point_load_rotational_on_beam = new feb.LoadRotationalPoint(P, orientation, pos);

            // a constant, linear and parabolic translational load on a beam or truss
            // in order to e.g. define a block load one needs to add two loads which start at different positions:
            // a positive load at the beginning of the block-load, a negative load at the end which neutralizes the first one.
            var n = new Vec3d(0, 0, 1); // direction vector of the load (length shoudl be 1)
            var p = new VectReal(3);
            double p_const = 1.0;
            double p_linear = 2.0;
            double p_quadratic = 3.0;
            p[0] = p_const;
            p[1] = p_linear;
            p[3] = p_quadratic;
            var linear_load_translational_on_beam = new feb.LoadTranslationalLine(n, p, orientation, pos);

            // a constant, linear and parabolic rotational load (= moment) on a beam or truss
            // not yet implemented to give results when added to a model
            var linear_load_rotational_on_beam = new feb.LoadRotationalLine(n, p, orientation, pos);

            // opens a translational gap along a beam
            // not yet implemented to give results when added to a model
            var gap = new Vec3d(0, 0, 1); // load vector;
            var dispt_translational_point_on_beam = new feb.DispTranslationalPoint(gap, orientation, pos);

            // creates a rotational kink (rotation about the axes of the given vector) along a beam
            // not yet implemented to give results when added to a model
            var kink = new Vec3d(0, 1, 0); // load vector;
            var dispt_rotational_point_on_beam = new feb.DispRotationalPoint(gap, orientation, pos);

        }
    }
#endif
}
