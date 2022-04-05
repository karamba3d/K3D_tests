#if ALL_TESTS

namespace KarambaCommon.Tests.Geometry
{
    using Karamba.Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class InfiniteLine_Tests
    {
        [Test]
        public void ConstructorFrom2Pt_SlopeAndIntercept_AreComputed()
        {
            // Arrange
            var p1 = new Point2(2, 10);
            var p2 = new Point2(5, 19);

            // Act
            var infLine = new InfiniteLine2(p1, p2);

            // Assert
            Assert.That(infLine.Intercept, Is.EqualTo(4).Within(double.Epsilon));
            Assert.That(infLine.Slope, Is.EqualTo(3).Within(double.Epsilon));
        }

        [Test]
        public void ConstructorFromCartesianCoord_SlopeAndIntercept_AreComputed()
        {
            // Act
            var infLine = new InfiniteLine2(2, 10, 5, 19);

            // Assert
            Assert.That(infLine.Intercept, Is.EqualTo(4).Within(double.Epsilon));
            Assert.That(infLine.Slope, Is.EqualTo(3).Within(double.Epsilon));
        }
    }
}

#endif