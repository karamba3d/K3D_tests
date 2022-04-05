#if ALL_TESTS

namespace KarambaCommon.Tests.Elements.ElementStates
{
    using Karamba.Elements;
    using Karamba.Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class ElementStateCollection_Tests
    {
        private static Element1DStatesCollection BuildCollectionFromPositions(params double[] positons)
        {
            var collection = new Element1DStatesCollection();
            foreach (var position in positons)
            {
                collection.TryAddState(new Element1DState(position) { BasePoint = Point3.Unset, Coosys = null, });
            }

            return collection;
        }

        [Test]
        public void TryAddState_StatesWithAlreadyCollectedPosition_WillNotBeAdded()
        {
            // Arrange
            var collection = BuildCollectionFromPositions(0.0, 1.0);
            var existingState = new Element1DState(1.0) { BasePoint = Point3.Unset, Coosys = null, };

            // Act
            var stateAdded = collection.TryAddState(existingState);

            // Assert
            var expectedPos = new double[] { 0.0, 1.0 };
            int i = -1;
            Assert.That(stateAdded, Is.False);
            foreach (var state in collection)
            {
                Assert.That(state.Position, Is.EqualTo(expectedPos[++i]));
            }
        }

        [Test]
        public void TryGetAtPosition_ValidPosition_ReturnsTrue()
        {
            // Arrange
            var collection = BuildCollectionFromPositions(0.0, 1.0);

            // Act
            var validBool = collection.TryGetStateFromPosition(1.0, out var validState);
            var invalidBool = collection.TryGetStateFromPosition(0.5, out var invalidState);

            // Assert
            Assert.That(validBool, Is.True);
            Assert.That(validState.Position, Is.EqualTo(1.0));

            Assert.That(invalidBool, Is.False);
            Assert.That(invalidState, Is.EqualTo(null));
        }

        [Test]
        public void TryGetRange_UsingKeyPositions_WillReturnTheRange()
        {
            // Arrange
            var collection = BuildCollectionFromPositions(0.0, 0.2, 0.5, 1.0);

            // Act
            var boolean = collection.TryGetRange(0.0, 0.5, out var states);

            // Assert
            Assert.That(boolean, Is.True);
            Assert.That(states[0].Position, Is.EqualTo(0.0));
            Assert.That(states[1].Position, Is.EqualTo(0.2));
            Assert.That(states[2].Position, Is.EqualTo(0.5));
        }

        [Test]
        public void TryGetRange_UsingNonKeyPositions_WillNotReturnTheRange()
        {
            // Arrange
            var collection = BuildCollectionFromPositions(0.0, 0.2, 0.5, 1.0);

            // Act
            var boolean1 = collection.TryGetRange(0.0, 0.8, out var states1);
            var boolean2 = collection.TryGetRange(0.3, 1.0, out var states2);
            var boolean3 = collection.TryGetRange(0.3, 0.8, out var states3);

            // Assert
            Assert.That(boolean1, Is.False);
            Assert.That(states1, Is.Null);

            Assert.That(boolean2, Is.False);
            Assert.That(states2, Is.Null);

            Assert.That(boolean3, Is.False);
            Assert.That(states3, Is.Null);
        }

        [Test]
        public void TryGetRange_IfFromIsGreaterThenTo_WillReturnTheSwitchedRange()
        {
            // Arrange
            var collection = BuildCollectionFromPositions(0.0, 0.2, 0.5);

            // Act
            var boolean1 = collection.TryGetRange(0.5, 0.0, out var states);

            // Assert
            Assert.That(boolean1, Is.True);
            Assert.That(states[0].Position, Is.EqualTo(0.0));
            Assert.That(states[1].Position, Is.EqualTo(0.2));
            Assert.That(states[2].Position, Is.EqualTo(0.5));
        }

        [Test]
        public void GetEnumerator_WillReturnStates_SortedByPositions()
        {
            // Arrange
            var collection = BuildCollectionFromPositions(0.0, 1.0, 0.5, 0.2);

            // Act
            var enumerator = collection.GetEnumerator();

            // Assert
            var expected = new[] { 0.0, 0.2, 0.5, 1.0 };
            int i = 0;
            while (enumerator.MoveNext())
            {
                Assert.That(enumerator.Current.Position, Is.EqualTo(expected[i++]));
            }
        }

        [Test]
        public void UnionWith_DuplicatedValues_WillBeDiscarded()
        {
            // Arrange
            var firstCollection = BuildCollectionFromPositions(0.0, 0.3, 1.0);
            var secondCollection = BuildCollectionFromPositions(0.0, 0.9, 1.0);

            // Act
            firstCollection.UnionWith(secondCollection);
            var enumerator = firstCollection.GetEnumerator();

            var expected = new[] { 0.0, 0.3, 0.9, 1.0 };
            int i = 0;
            while (enumerator.MoveNext())
            {
                Assert.That(enumerator.Current.Position, Is.EqualTo(expected[i++]));
            }
        }
    }
}

#endif