#if ALL_TESTS

namespace KarambaCommon.Tests.Geometry
{
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class PolyLine3_tests
    {
        [Test]
        public void OverlapsesOpen()
        {
            PolyLine3 pl = new PolyLine3(new List<Point3>()
            {
                new Point3(0, 0, 0),
                new Point3(2, 0, 0),
                new Point3(1, 0, 0),
                new Point3(3, 0, 0),
            });

            pl.RemoveOverlaps();

            Assert.That(pl[0].X, Is.EqualTo(0));
            Assert.That(pl[1].X, Is.EqualTo(1));
            Assert.That(pl[2].X, Is.EqualTo(2));
            Assert.That(pl[3].X, Is.EqualTo(3));
        }

        [Test]
        public void OverlapsesClosed()
        {
            PolyLine3 pl = new PolyLine3(new List<Point3>()
            {
                new Point3(0, 0, 0),
                new Point3(2, 0, 0),
                new Point3(1, 0, 0),
                new Point3(3, 0, 0),
                new Point3(0, 0, 0),
            }) { IsClosed = true};

            pl.RemoveOverlaps();

            Assert.That(pl[0].X, Is.EqualTo(0));
            Assert.That(pl[1].X, Is.EqualTo(1));
            Assert.That(pl[2].X, Is.EqualTo(2));
            Assert.That(pl[3].X, Is.EqualTo(3));
            Assert.That(pl[4].X, Is.EqualTo(0));
        }
    }
}

#endif
