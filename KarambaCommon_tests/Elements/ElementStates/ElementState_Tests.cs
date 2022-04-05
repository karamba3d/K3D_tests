#if ALL_TESTS
namespace KarambaCommon.Tests.Elements.ElementStates
{
    using System;
    using System.ComponentModel;
    using feb;
    using Karamba.Elements;
    using Karamba.Geometry;
    using NUnit.Framework;

#if TEST_CAN_THROW_EXCEPTION
    [TestFixture]
    public class ElementState_Tests
    {
        [Test]
        [TestCase(-0.1)]
        [TestCase(1.1)]
        public void Constructor_InvalidPositions_WillThrowError(double position)
        {
            // Act
            void TestDelegate()
            {
                var elementState = new Element1DState(position) { BasePoint = Point3.NaP, Coosys = null, };
            }

            // Assert
            try
            {
                TestDelegate();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.That(e, Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }
    }
#endif
}
#endif