#if ALL_TESTS

namespace KarambaCommon.Tests.Geometry
{
    using System;
    using System.Collections.Generic;
    using Karamba.Geometry;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class Vector3_tests
    {
        [Test]
        public void PerpendicularTo()
        {
            Vector3 v1 = new Vector3(1, 0, 0);
            Vector3 v2 = new Vector3(1, 1, 0);
            bool success = v2.MakePerpendicularTo(v1);
            Assert.That(success, Is.True);
            Assert.That(new Vector3(0, 1, 0), Is.EqualTo(v2));

            Vector3 v3 = new Vector3(1, 1e-16, 0);
            success = v3.MakePerpendicularTo(v1);
            Assert.That(success, Is.False);
            Assert.That(new Vector3(0, 1e-16, 0), Is.EqualTo(v3));

            Vector3 v4 = new Vector3(0.75, 1e-16, 0.75);
            Vector3 v5 = Vector3.UnitX;
            if (!v5.MakePerpendicularTo(v4))
            {
                Assert.Fail();
            }

            Assert.That(v5.X, Is.EqualTo(0.5).Within(1e-8));
            Assert.That(v5.Y, Is.EqualTo(0.0).Within(1e-8));
            Assert.That(v5.Z, Is.EqualTo(-0.5).Within(1e-8));
        }

        [Test]
        public void Rotate()
        {
            Vector3 v0 = new Vector3(0, 0, -1);
            double angle = 45.0.ToRad();
            v0.Rotate(angle, new Vector3(1, 0, 0));
            double targ_y = 1.0 * Math.Sin(angle);
            double targ_z = -1.0 * Math.Cos(angle);

            Assert.That(targ_y, Is.EqualTo(v0.Y).Within(1e-8));
            Assert.That(targ_z, Is.EqualTo(v0.Z).Within(1e-8));
        }

        [Test]
        public void Point3_HashCode()
        {
            Point3 p0 = new Point3(0, 1, 2);
            Point3 p1 = new Point3(3,4,5);
            Point3 p2 = new Point3(0,1,2);

            HashSet<Point3> hset = new HashSet<Point3>();
            hset.Add(p0);
            hset.Add(p1);
            hset.Add(p2);
            int count = hset.Count;

            Assert.That(count == 2);
        }
    }
}

#endif
