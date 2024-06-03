#if ALL_TESTS
namespace KarambaCommon.Tests.CrossSections
{
    using System.Collections.Generic;
    using Karamba.Geometry;
    using Karamba.Utilities;
    using NUnit.Framework;

#pragma warning disable CS1591 // don't enforce documentation in test files.

    [TestFixture]
    public class CroSecProperties_tests
    {
        [Test]
        public void RectangleValues()
        {
            // define outline of triangle
            double b = 0.5;
            double h = 1.0;

            PolyLine3 pl = new PolyLine3(new List<Point3>
            {
                new Point3(0, 0, h),
                new Point3(0, 0, 0),
                new Point3(0, b, 0),
                new Point3(0, b, h),
            });

            CrossSectionProperties props = CroSecProperties.solve(new List<PolyLine3> { pl });
            double area_targ = b * h;
            Assert.That(area_targ, Is.EqualTo(props.A).Within(1E-10));
            double area_iy = b * h * h * h / 12.0;
            Assert.That(area_iy, Is.EqualTo(props.inertia.X).Within(1E-10));
            Assert.That(area_iy, Is.EqualTo(props.inertia_princ.X).Within(1E-10));
            double area_iz = h * b * b * b / 12.0;
            Assert.That(area_iz, Is.EqualTo(props.inertia.Y).Within(1E-10));
            Assert.That(area_iz, Is.EqualTo(props.inertia_princ.Y).Within(1E-10));
            double cog_z = h * 0.5;
            double cog_y = b * 0.5;
            Assert.That(cog_y, Is.EqualTo(props.cog.Y).Within(1E-10));
            Assert.That(cog_z, Is.EqualTo(props.cog.Z).Within(1E-10));
            double wel_y = b * h * h / 6.0;
            double wel_z = h * b * b / 6.0;
            Assert.That(-wel_z, Is.EqualTo(props.Welz_y_pos).Within(1E-10));
            Assert.That(wel_y, Is.EqualTo(props.Wely_z_pos).Within(1E-10));
            Assert.That(wel_z, Is.EqualTo(props.Welz_y_neg).Within(1E-10));
            Assert.That(-wel_y, Is.EqualTo(props.Wely_z_neg).Within(1E-10));
            double wpl_y = b * h * h / 4.0;
            double wpl_z = h * b * b / 4.0;
            Assert.That(wpl_y, Is.EqualTo(props.Wply).Within(1E-10));
            Assert.That(wpl_z, Is.EqualTo(props.Wplz).Within(1E-10));
        }

        [Test]
        public void HollowRectangleValues()
        {
            // define outline of triangle
            double b = 0.5;
            double h = 1.0;
            double t = 0.1;

            PolyLine3 pl1 = new PolyLine3(new List<Point3>
            {
                new Point3(0, 0, h),
                new Point3(0, 0, 0),
                new Point3(0, b, 0),
                new Point3(0, b, h),
            });

            PolyLine3 pl2 = new PolyLine3(new List<Point3>
            {
                new Point3(0, b - t, h - t),
                new Point3(0, b - t, 0 + t),
                new Point3(0, t, 0 + t),
                new Point3(0, t, h - t),
            });

            double bi = 0.5 - 2 * t;
            double hi = 1.0 - 2 * t;

            CrossSectionProperties props = CroSecProperties.solve(new List<PolyLine3> { pl1, pl2 });
            double area_targ = b * h - bi * hi;
            Assert.That(area_targ, Is.EqualTo(props.A).Within(1E-10));
        }
    }
}
#endif