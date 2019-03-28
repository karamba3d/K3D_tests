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
using Karamba.Materials;
using Karamba.Supports;
using Karamba.Models;
using Karamba.Utilities;
using Karamba.Algorithms;

namespace KarambaCommon.Tests.Geometry
{
    [TestFixture]
    public class Vector3_tests
    {
#if ALL_TESTS
        [Test]
        public void PerpendicularTo()
        {
            var v1 = new Vector3(1,0,0);
            var v2 = new Vector3(1,1,0);
            var success = v2.MakePerpendicularTo(v1);
            Assert.IsTrue(success);
            Assert.AreEqual(v2, new Vector3(0,1,0));

            var v3 = new Vector3(1, 1e-16, 0);
            success = v3.MakePerpendicularTo(v1);
            Assert.IsFalse(success);
            Assert.AreEqual(v3, new Vector3(0, 1e-16, 0));

            var v4 = new Vector3(0.75, 1e-16, 0.75);
            var v5 = Vector3.UnitX;
            if (!v5.MakePerpendicularTo(v4))
            {
                Assert.Fail();
            }
            Assert.AreEqual(v5.X, 0.5, 1e-8);
            Assert.AreEqual(v5.Y, 0.0, 1e-8);
            Assert.AreEqual(v5.Z,-0.5, 1e-8);
        }
#endif

#if ALL_TESTS
        [Test]
        public void Rotate()
        {
            var v0 = new Vector3(0, 0, -1);
            var angle = 45.0.ToRad();
            v0.Rotate(angle, new Vector3(1,0,0));
            var targ_y = 1.0 * Math.Sin(angle);
            var targ_z = -1.0 * Math.Cos(angle);

            Assert.AreEqual(v0.Y, targ_y, 1e-8);
            Assert.AreEqual(v0.Z, targ_z, 1e-8);

        }
#endif
    }
}
