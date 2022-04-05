#if ALL_TESTS

namespace KarambaCommon.Tests.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class Vector3_tests
    {
        [Test]
        public void PerpendicularTo()
        {
            var v1 = new Vector3(1, 0, 0);
            var v2 = new Vector3(1, 1, 0);
            var success = v2.MakePerpendicularTo(v1);
            Assert.That(success, Is.True);
            Assert.That(new Vector3(0, 1, 0), Is.EqualTo(v2));

            var v3 = new Vector3(1, 1e-16, 0);
            success = v3.MakePerpendicularTo(v1);
            Assert.That(success, Is.False);
            Assert.That(new Vector3(0, 1e-16, 0), Is.EqualTo(v3));

            var v4 = new Vector3(0.75, 1e-16, 0.75);
            var v5 = Vector3.UnitX;
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
            var v0 = new Vector3(0, 0, -1);
            var angle = 45.0.ToRad();
            v0.Rotate(angle, new Vector3(1, 0, 0));
            var targ_y = 1.0 * Math.Sin(angle);
            var targ_z = -1.0 * Math.Cos(angle);

            Assert.That(targ_y, Is.EqualTo(v0.Y).Within(1e-8));
            Assert.That(targ_z, Is.EqualTo(v0.Z).Within(1e-8));
        }
    }
}

#endif
