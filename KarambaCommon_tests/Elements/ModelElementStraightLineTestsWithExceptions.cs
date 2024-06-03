#if ALL_TESTS
#if TEST_CAN_THROW_EXCEPTION
namespace KarambaCommon.Tests.Elements
{
    using System;
    using System.Linq;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Models;
    using KarambaCommon_tests.Utilities;
    using NUnit.Framework;

    public partial class ModelElementStraightLineTests
    {
        [Test]
        public void GetState1D_InvalidLoadCases_ThrowsException()
        {
            // Arrange
            var beam = (ModelElementStraightLine)ModelFactory.CreateOneBeamModel(
                                        Point3.Zero,
                                        new Point3(0, 0, 10),
                                        TestableBeamProperties.HingedBeamDefaultProperties())
                                    .elems.First();
            var emptyModel = new Model();

            // Act
            void GetState() => beam.GetState1D(0.0, emptyModel, null);

            // Assert
            Assert.That(emptyModel.numLC, Is.Zero);
            var exception = Assert.Throws<ArgumentException>(GetState);
            Assert.That(exception?.Message, Is.EqualTo("Can't superimpose load cases because there are none\r\nParameter name: model"));
        }

        [Test]
        public void GetState1D_InvalidPositions_ThrowException()
        {
            // Arrange
            var model = ModelFactory.CreateOneBeamModel(
                Point3.Zero,
                new Point3(0, 0, 10),
                TestableBeamProperties.HingedBeamDefaultProperties());
            var beam = (ModelElementStraightLine)model.elems.First();

            // Act
            void CreateState1() => beam.GetState1D(-1, model, null);
            void CreateState2() => beam.GetState1D(1.1, model, null, isCurveReparametrized: true);
            void CreateState3() => beam.GetState1D(10.1, model, null, isCurveReparametrized: false);

            // Act + Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(CreateState1);
            Assert.That(exception?.Message, Is.EqualTo($"The position is outside the valid domain: {new Interval3(0, 10)}\r\nParameter name: position"));

            exception = Assert.Throws<ArgumentOutOfRangeException>(CreateState2);
            Assert.That(exception?.Message, Is.EqualTo($"The position is outside the valid domain: {new Interval3(0, 1)}\r\nParameter name: position"));

            exception = Assert.Throws<ArgumentOutOfRangeException>(CreateState3);
            Assert.That(exception?.Message, Is.EqualTo($"The position is outside the valid domain: {new Interval3(0, 10)}\r\nParameter name: position"));
        }
    }
}
#endif
#endif