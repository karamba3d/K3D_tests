using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Supports;
using Karamba.Models;
using Karamba.Utilities;
using Karamba.Algorithms;
using KarambaCommon.Utilities;


namespace KarambaCommon.Tests.CrossSections
{
    [TestFixture]
    public class CroSecProperties_tests
    {
#if ALL_TESTS
        [Test]
        public void RectangleValues()
        {
            // define outline of triangle
            double b = 0.5;
            double h = 1.0;

            var pl = new PolyLine3(new List<Point3>
            {
                new Point3(0,0, h),
                new Point3(0,0, 0),
                new Point3(0,b, 0),
                new Point3(0,b, h)
            });

            var props = CroSecProperties.solve(new List<PolyLine3>{pl});
            double area_targ = b * h;
            Assert.AreEqual(props.A, area_targ, 1E-10);
            double area_iy = b * h * h * h / 12.0;
            Assert.AreEqual(props.inertia.X, area_iy, 1E-10);
            Assert.AreEqual(props.inertia_princ.X, area_iy, 1E-10);
            double area_iz = h * b * b * b / 12.0;
            Assert.AreEqual(props.inertia.Y, area_iz, 1E-10);
            Assert.AreEqual(props.inertia_princ.Y, area_iz, 1E-10);
            double cog_z = h * 0.5;
            double cog_y = b * 0.5;
            Assert.AreEqual(props.cog.Y, cog_y, 1E-10);
            Assert.AreEqual(props.cog.Z, cog_z, 1E-10);
            double wel_y = b * h * h / 6.0;
            double wel_z = h * b * b / 6.0;
            Assert.AreEqual(props.Welz_y_pos, -wel_z, 1E-10);
            Assert.AreEqual(props.Wely_z_pos, wel_y, 1E-10);
            Assert.AreEqual(props.Welz_y_neg, wel_z, 1E-10);
            Assert.AreEqual(props.Wely_z_neg, -wel_y, 1E-10);
            double wpl_y = b * h * h / 4.0;
            double wpl_z = h * b * b / 4.0;
            Assert.AreEqual(props.Wply, wpl_y, 1E-10);
            Assert.AreEqual(props.Wplz, wpl_z, 1E-10);
        }

        [Test]
        public void HollowRectangleValues()
        {
            // define outline of triangle
            double b = 0.5;
            double h = 1.0;
            double t = 0.1;

            var pl1 = new PolyLine3(new List<Point3>
            {
                new Point3(0,0, h),
                new Point3(0,0, 0),
                new Point3(0,b, 0),
                new Point3(0,b, h)
            });

            var pl2 = new PolyLine3(new List<Point3>
            {
                new Point3(0,b-t, h-t),
                new Point3(0,b-t, 0+t),
                new Point3(0,t, 0+t),
                new Point3(0,t, h-t)
            });

            double bi = 0.5 - 2 * t;
            double hi = 1.0 - 2 * t;

            var props = CroSecProperties.solve(new List<PolyLine3> { pl1, pl2 });
            double area_targ = b * h - bi*hi;
            Assert.AreEqual(props.A, area_targ, 1E-10);
        }
#endif
    }
}