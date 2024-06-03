#if ALL_TESTS
namespace KarambaCommon.Tests.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Elements.States;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture]
    public class ModelElementStraightLineTests
    {
        [OneTimeSetUp]
        public void CreateTestModel()
        {
            var k3d = new Toolkit();

            var line = new Line3 { PointAtStart = new Point3(0, 0, 0), PointAtEnd = new Point3(10, 0, 0), };

            var logger = new MessageLogger();
            var crossSec = new CroSec_Circle();
            List<BuilderBeam> beam = k3d.Part.LineToBeam(line, "anyId", crossSec, logger, out _);

            var supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportHingedConditions),
                k3d.Support.Support(1, k3d.Support.SupportHingedConditions),
            };

            var loads = new List<Load>() { k3d.Load.GravityLoad(new Vector3(0, 0, 1)), };

            _model = k3d.Model.AssembleModel(
                beam,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _);

            _model = k3d.Algorithms.AnalyzeThI(
                _model,
                out IReadOnlyList<double> maxDispls,
                out _,
                out _,
                out _);

            _maxDisplacement = maxDispls[0];
        }

        private Karamba.Models.Model _model;
        private double _maxDisplacement;

        private static void ChangeMaxSectLength(Karamba.Models.Model model, double max)
        {
            model.dp.basic.maxSectLength = max;
        }

        [Test]
        public void NCrossSec_ChangeMaxSectLength_WillChangeSectionNumber()
        {
            // Arrange
            Model model = _model.Clone();
            var elementStraightLine = _model.elems[0] as ModelElementStraightLine;
            double length = elementStraightLine.elementLength(model);
            var r = new Random();
            ChangeMaxSectLength(model, r.NextDouble());

            // Act
            int crossSectionCount = elementStraightLine.nCroSec(model);
            double segmentLength = length / (crossSectionCount - 1);
            double segmentLength2 = length / (crossSectionCount - 2);

            // Assert
            Assert.Multiple(() => {
                Assert.That(segmentLength, Is.Not.GreaterThan(model.dp.basic.maxSectLength).Within(double.Epsilon));
                Assert.That(segmentLength2, Is.GreaterThanOrEqualTo(model.dp.basic.maxSectLength).Within(double.Epsilon));
            });
        }

        [Test]
        public void GetPositionOfCrossSections_ReturnValues_InParametricSpace()
        {
            // Arrange
            Model model = _model.Clone();
            var elementStraightLine = model.elems[0] as ModelElementStraightLine;
            double max = 3;
            ChangeMaxSectLength(model, max);

            // Act
            int crossSecsCount = elementStraightLine.nCroSec(model);
            List<double> positions = elementStraightLine.GetCrossSectionsPositions(model);

            // Assert
            Assert.Multiple(() =>
            {   Assert.That(crossSecsCount, Is.EqualTo(5));
                Assert.That(positions, Is.EquivalentTo(new List<double> { 0.0, 0.25, 0.5, 0.75, 1.0 }));
            });
        }

        [Test]
        public void GetElementState_ValidPositions_ReturnDeformedStates()
        {
            var elementStraightLine = _model.elems[0] as ModelElementStraightLine;
            double position = 0.5;

            var state = elementStraightLine.GetState1D(position, _model,
                new StateElement1DSelectorIndex(_model,
                    _model.lcActivation.LoadCaseCombinations.First()), true, out _, out _);

            Assert.Multiple(() => {
                Assert.That(state.Position.X, Is.EqualTo(5));
                Assert.That(state.Position.Y, Is.EqualTo(0));
                Assert.That(state.Position.Z, Is.EqualTo(_maxDisplacement).Within(double.Epsilon));
            });
        }
    }
}
#endif
