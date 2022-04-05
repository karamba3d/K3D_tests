#if ALL_TESTS

namespace KarambaCommon.Tests.Elements.ElementStates
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Karamba.Elements;
    using Karamba.Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class ElementStateCollectionGroup_Tests
    {
        private static Element1DStatesCollection BuildCollectionFromPositions(List<double> positons)
        {
            var collection = new Element1DStatesCollection();
            foreach (var position in positons)
            {
                collection.TryAddState(new Element1DState(position) { BasePoint = Point3.Unset, Coosys = null, });
            }

            return collection;
        }

        [Test]
        public void UnionWith_GroupsWithSameElementId_DuplicatedValuesWillNotBeAdded()
        {
            // Arrange
            var factory1 = new GroupFactory()
            {
                { 1, new List<double> { 0.0, 1.0 } }, { 2, new List<double> { 0.0, 1.0 } },
            };

            var factory2 = new GroupFactory()
            {
                { 1, new List<double> { 0.5, 0.8 } }, { 3, new List<double> { 0.0, 1.0 } },
            };

            var group1 = factory1.BuildCollectionGroup();
            var group2 = factory2.BuildCollectionGroup();

            // Act
            group1.UnionWith(group2);

            var ids = new List<int>();
            var positions = new List<double>();
            foreach (var keyValuePair in group1)
            {
                ids.Add(keyValuePair.Key);
                foreach (var state in keyValuePair.Value)
                {
                    positions.Add(state.Position);
                }
            }

            // Assert
            var expectedPositions = new[] { 0.0, 0.5, 0.8, 1.0, 0.0, 1.0, 0.0, 1.0, };
            var expectedIds = new[] { 1, 2, 3 };

            Assert.That(ids, Is.EqualTo(expectedIds));
            Assert.That(positions, Is.EqualTo(expectedPositions));
        }

        private class GroupFactory : IEnumerable<KeyValuePair<int, double>>
        {
            public GroupFactory()
            {
                PositionsById = new Dictionary<int, List<double>>();
            }

            public Dictionary<int, List<double>> PositionsById { get; set; }

            public void Add(int id, List<double> positions)
            {
                PositionsById.Add(id, positions);
            }

            public Element1DStateCollectionGroup BuildCollectionGroup()
            {
                var group = new Element1DStateCollectionGroup();
                foreach (var keyValuePair in PositionsById)
                {
                    group.Add(keyValuePair.Key, BuildCollectionFromPositions(keyValuePair.Value));
                }

                return group;
            }

            public IEnumerator<KeyValuePair<int, double>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}

#endif