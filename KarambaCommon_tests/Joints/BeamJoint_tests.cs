using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Joints;
using Karamba.Supports;
using Karamba.Models;
using Karamba.Utilities;
using Karamba.Algorithms;

namespace KarambaCommon.Tests.Joints
{
    [TestFixture]
    public class BeamJointTests
    {
#if ALL_TESTS
        [Test]
        public void AxialSpringOnBothSides()
        {
            var k3d = new Toolkit();

            var crosec = k3d.CroSec.CircularHollow();
            crosec.Az = 1E20; // make the cross section very stiff in shear

            var elems = new List<BuilderBeam>() {
                k3d.Part.IndexToBeam(0, 1, "", crosec),
                k3d.Part.IndexToBeam(1, 2, "", crosec),
            };

            double cx = 100;
            var joint0 = new Joint(new double?[] { cx, null, null, null, null, null, null, null, null, null, null, null });
            var joint1 = new Joint(new double?[] { null, null, null, null, null, null, cx, null, null, null, null, null });
            elems[0].joint = joint0;
            elems[1].joint = joint1;
            
            var L = 10.0; // in meter
            var points = new List<Point3> { new Point3(), new Point3(L * 0.5, 0, 0), new Point3(L, 0, 0) };

            var supports = new List<Support> {
                k3d.Support.Support(0,new List<bool> { true, true, true, true, false, false}),
                k3d.Support.Support(2, new List<bool> { false, true, true, false, false,  false})
            };

            var fx = 1.0; // in kN
            var loads = new List<Load> {
                k3d.Load.PointLoad(2, new Vector3(fx,0,0))
            };

            var model = k3d.Model.AssembleModel(elems, supports, loads, out var info, out var mass, out var cog, out var msg,
                out var runtimeWarning, new List<Joint>(), points);

            ThIAnalyze.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning, out model);

            var A = (model.elems[0].crosec as CroSec_Beam).A;
            var E = model.elems[0].crosec.material.E();
            var c_inv_tot = L / (E * A) + 2 / cx;
            var dispTarget = Math.Abs(fx) * c_inv_tot;

            Assert.AreEqual(outMaxDisp[0], dispTarget, 1E-10);
        }
#endif
    }
}
