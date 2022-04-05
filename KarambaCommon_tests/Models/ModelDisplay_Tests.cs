#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    public class ModelDisplay_Tests
    {
        private Karamba.Models.Model CreateModel()
        {
            var k3d = new Toolkit();

            var line = new Line3
            {
                PointAtStart = new Point3(0, 0, 0),
                PointAtEnd = new Point3(10, 0, 0),
            };

            var logger = new MessageLogger();
            var crossSec = new CroSec_Circle();
            var beam = k3d.Part.LineToBeam(line, "anyId", crossSec, logger, out _);

            var supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportHingedConditions),
                k3d.Support.Support(1, k3d.Support.SupportHingedConditions),
            };

            List<Load> loads = new List<Load>()
            {
                k3d.Load.GravityLoad(new Vector3(0, 0, 1)),
            };

            var model = k3d.Model.AssembleModel(
                beam,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _);

            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out _);

            return model;
        }

        [Test]
        public void Element1DCrossSectionStates_ReturnDeformedStatesCollectionGroup()
        {
            // Arrange
            var model = CreateModel();
            model.dp.basic.maxSectLength = 3;

            // Act
            var collection = model.dp.Element1DCrossSectionStates[0];

            // Assert
            CollectionAssert.AreEqual(collection.Positions, new List<double> { 0.0, 0.25, 0.5, 0.75, 1.0 });

            // All z coordinates except first and last are not zero
            var zCoordinates = collection.BasePoints.Select(x => x.Z).ToList();
            zCoordinates = zCoordinates.GetRange(1, zCoordinates.Count - 2);

            CollectionAssert.AreNotEqual(zCoordinates, Enumerable.Repeat(0.0, zCoordinates.Count));
        }
    }
}

#endif