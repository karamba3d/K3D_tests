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


namespace KarambaCommon.Tests.CrossSections
{
    [TestFixture]
    public class CroSecValues_tests
    {
#if ALL_TESTS
        [Test]
        public void BoxValues()
        {
            var k3d = new Toolkit();
            var cs = k3d.CroSec.Box(390, 60, 60, 23.6, 0.1, 2.7, 0, -1);
            var A = cs._height * cs.uf_width - (cs._height - cs.uf_thick - cs.lf_thick) * (cs.uf_width - 2*cs.w_thick);
            var ufA = cs.uf_thick * cs.uf_width;
            var lfA = cs.lf_thick * cs.lf_width;
            var wh = cs._height - cs.uf_thick - cs.lf_thick;
            var wA = wh * cs.w_thick * 2;
            var Sy = ufA * cs.uf_thick * 0.5 + lfA * (cs._height - 0.5 * cs.lf_thick) + wA * (cs.uf_thick + wh * 0.5);
            var zs = Sy / A;
            Assert.AreEqual(A, cs.A, 1E-10);
            Assert.AreEqual(zs, cs.zs, 1E-10);
        }
#endif
    }
}
