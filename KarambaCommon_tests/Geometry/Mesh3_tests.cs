#if ALL_TESTS

namespace KarambaCommon.Tests.Geometry
{
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class Mesh3_tests
    {
        [Test]
        public void NakedEdges()
        {
            Mesh3 mesh = new Mesh3(
                new List<Point3> { new Point3(0, 0, 0), new Point3(1, 0, 0), new Point3(1, 1, 0), new Point3(0, 1, 0) },
                new List<Face3> { new Face3(0, 1, 2), new Face3(0, 2, 3) });
            var ne = mesh.NakedEdges().ToList();
            Assert.That(ne.Count == 4);
            Assert.That(ne.Contains(new Edge3(0, 1)));
            Assert.That(ne.Contains(new Edge3(0, 3)));
            Assert.That(ne.Contains(new Edge3(2, 3)));
            Assert.That(ne.Contains(new Edge3(1, 2)));
            /*
             * These tests do not work since the order is not specified
            Assert.That(ne[0].A, Is.EqualTo(0));
            Assert.That(ne[0].B, Is.EqualTo(1));
            Assert.That(ne[1].A, Is.EqualTo(0));
            Assert.That(ne[1].B, Is.EqualTo(3));
            Assert.That(ne[2].A, Is.EqualTo(2));
            Assert.That(ne[2].B, Is.EqualTo(3));
            Assert.That(ne[3].A, Is.EqualTo(1));
            Assert.That(ne[3].B, Is.EqualTo(2));
            */
        }
    }
}

#endif
