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
    public class Mesh3_tests
    {
#if ALL_TESTS
        [Test]
        public void NakedEdges()
        {
            var mesh = new Mesh3(
                new List<Point3>{new Point3(0,0,0), new Point3(1, 0, 0), new Point3(1, 1, 0), new Point3(0, 1, 0) },
                new List<Face3>{new Face3(0,1,2), new Face3(0,2,3)});
            var ne = mesh.NakedEdges();
            Assert.AreEqual(ne[0].A, 0);
            Assert.AreEqual(ne[0].B, 1);
            Assert.AreEqual(ne[1].A, 1);
            Assert.AreEqual(ne[1].B, 2);
            Assert.AreEqual(ne[2].A, 2);
            Assert.AreEqual(ne[2].B, 3);
            Assert.AreEqual(ne[3].A, 3);
            Assert.AreEqual(ne[3].B, 0);
        }
#endif

    }
}