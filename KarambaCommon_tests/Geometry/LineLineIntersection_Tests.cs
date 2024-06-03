#if ALL_TESTS

namespace KarambaCommon.Tests.Geometry
{
    using System;
    using System.Collections.Generic;
    using Karamba.Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class LineLineIntersection_Tests
    {
        [Test]
        public void IntersectThreeLines()
        {
            // Arrange
            Line3 l0 = new Line3(new Point3(0, 2, 0), new Point3(0, -2, 0));
            Line3 l1 = new Line3(new Point3(-1, 1, 0), new Point3(1, 1, 0));
            Line3 l2 = new Line3(new Point3(-1, -1, 0), new Point3(1, -1, 0));

            var (resLines, resPoints) = Line3Line3Intersection.solve(
                new List<List<Line3>>
                {
                    new List<Line3> { l0 }, new List<Line3> { l1, l2 },
                },
                1e-5);

            // Assert
            Assert.That(resLines.Count == 2);
            Assert.That(resPoints.Count == 2);
            Assert.That(resPoints[0].DistanceTo(new Point3(0, 1, 0)), Is.EqualTo(0).Within(double.Epsilon));
            Assert.That(resPoints[1].DistanceTo(new Point3(0, -1, 0)), Is.EqualTo(0).Within(double.Epsilon));
        }

        [Test]
        public void IntersectTwoInclinedLines()
        {
            // Arrange
            Line3 l0 = new Line3(new Point3(-2, 5, 0), new Point3(-3, 4, 0));
            Line3 l1 = new Line3(new Point3(-1, 6, 0), new Point3(-2, 5, 0));
            var (resLines, resPoints) = Line3Line3Intersection.solve(
                new List<List<Line3>>
                {
                    new List<Line3> { l0, l1 }, new List<Line3> { l0, l1 },
                },
                1e-12);

            // Assert
            Assert.That(resLines.Count == 2);
        }

        [Test]
        public void LineLineIntersectionTValues()
        {
            var line = new Line3(new Point3(-2, 5, 0), new Point3(-3, 4, 0));
            var ldist = 0.005;
            var eps = 0.005;
            var res = Line3Line3Intersection.lineLineIntersectionTValues(line, line, ldist, eps);
            Assert.That(res.Count == 2);
            Assert.That(res[0], Is.EqualTo(0).Within(double.Epsilon));
            Assert.That(res[1], Is.EqualTo(1).Within(double.Epsilon));
        }
    }
}

#endif
