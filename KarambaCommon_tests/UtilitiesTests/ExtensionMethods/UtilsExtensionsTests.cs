#if ALL_TESTS

namespace KarambaCommon.Tests.Utilities
{
    using System.Collections.Generic;
    using Karamba.Geometry;
    using Karamba.Utilities;
    using NUnit.Framework;

    public class UtilsExtensionsTests
    {
        [Test]
        public void Split()
        {
            // Arrange
            var interval = Interval3.Create(0, 10);
            double maxDistance = 3;

            // Act
            var splitInterval = interval.Split(maxDistance);

            // Assert
            Assert.That(splitInterval, Is.EquivalentTo(new[] { 0, 2.5, 5, 7.5, 10 }));
        }

        [Test]
        public void Split_WithMinIntervalEnabled()
        {
            // Arrange
            var interval = Interval3.Create(0, 10);
            double maxDistance = 5;
            int minInterval = 4;

            // Act
            var splitInterval = interval.Split(maxDistance, minInterval);

            // Assert
            Assert.That(splitInterval, Is.EquivalentTo(new[] { 0, 2.5, 5, 7.5, 10 }));
        }

        [Test]
        public void Split_SpecialCases_EmptyAndElementCollections()
        {
            // Arrange
            IEnumerable<double> emptyInterval = new List<double>();
            IEnumerable<double> oneValueInterval = new[] { 1.0 };
            double maxDistance = 3;

            // Act + Assert
            Assert.Multiple(() => {
                Assert.That(emptyInterval.Split(maxDistance), Is.Empty);
                Assert.That(oneValueInterval.Split(maxDistance), Is.EquivalentTo(new[] { 1.0 }));
            });
        }

        [Test]
        public void Split_MultiValuesSource()
        {
            // Arrange
            var source = new[] { 0.0, 10.0, 20.0 };
            double maxDistance = 3;

            // Act + Assert
            Assert.That(source.Split(maxDistance),
                Is.EquivalentTo(new[] { 0, 2.5, 5, 7.5, 10, 12.5, 15, 17.5, 20 }));
        }

        [Test]
        public void Split_ByUsingFunctions()
        {
            // Arrange
            var source = new[] { Point3.Zero, new Point3(1, 1, 1) };
            double FromPointToDouble(Point3 point) => point.X;
            Point3 FromDoubleToPoint(double value) => new Point3(value, 0, 0);
            double maxDistance = 0.5;

            // Act
            var points = source.Split(FromPointToDouble, FromDoubleToPoint, maxDistance);

            // Assert
            var expectedPoints = new[] { Point3.Zero, new Point3(0.5, 0, 0), new Point3(1, 0, 0) };
            Assert.That(points, Is.EquivalentTo(expectedPoints));
        }
    }
}

#endif
