#if ALL_TESTS
namespace KarambaCommon.Tests.Loads
{
    using System;
    using System.Collections.Generic;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Beam;
    using Karamba.Models;
    using KarambaCommon.Tests.Helpers;
    using KarambaCommon.Tests.Loads;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture(LoadTypes.Force)]
    [TestFixture(LoadTypes.Moment)]
    public class ConstantLoad_Tests_T // was: ConstantLoad_Tests<T>
    // why <T>, T not used anyway?
    {
        private readonly LoadTypes _loadType;

        public ConstantLoad_Tests_T(LoadTypes type)
        {
            _loadType = type;
        }

        [Test]
        public void Constructor_AnyDirectionVector_WillBeUnitized()
        {
            // Arrange
            var args = new ConstLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId" },
                LcName = "anyLcName",
                Value = 2.0,
                Direction = new Vector3(1, 1, 1),
            };

            // Act
            DistributedLoad load = CreateTestLoad(args);

            // Assert
            Assert.That(args.Direction.Length, Is.Not.EqualTo(1));
            Assert.That(load.Direction.Length, Is.EqualTo(1).Within(double.Epsilon));
        }

        [Test]
        public void Constructor_IfStartGreaterThenEnd_StartAndEndWillBeSwapped()
        {
            // Arrange
            var args = new ConstLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId" },
                LcName = "anyLcName",
                Value = 2.0,
                Direction = new Vector3(0, 0, 1),
                Start = 1.0,
                End = 0.0,
            };

            // Act
            DistributedLoad load = CreateTestLoad(args);

            // Assert
            Assert.That(load.Positions[0], Is.EqualTo(0.0));
            Assert.That(load.Positions[1], Is.EqualTo(1.0));
        }

#if TEST_CAN_THROW_EXCEPTION

        [Test]
        [TestCase(-1.0, 1.0)]
        [TestCase(2.0, 1.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(0.0, 2.0)]
        public void Constructor_InvalidPosition_WillThrowException(double start, double end)
        {
            // Arrange
            var args = new ConstLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId" },
                LcName = "anyLcName",
                Value = double.NaN,
                Direction = new Vector3(1, 1, 1),
                Start = start,
                End = end,
            };

            // Act + Assert
            try
            {
                CreateTestLoad(args);

                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.That(e, Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

#endif

        [Test]
        public void TryComputeValueFromPosition_AValidValue_WillThrowValue()
        {
            // Arrange
            var args = new ConstLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId" },
                LcName = "anyLcName",
                Value = 1.0,
                Direction = new Vector3(1, 1, 1),
                Start = 0.0,
                End = 1.0,
            };

            // Act
            DistributedLoad load = CreateTestLoad(args);
            bool boolRes = load.TryGetValue(0.5, out double outValue, out bool _);

            // Assert
            Assert.That(boolRes, Is.True);
            Assert.That(outValue, Is.EqualTo(1.0).Within(double.Epsilon));
        }

        [Test]
        public void TryComputeValueFromPosition_NonValidValue_ReturnsFalse()
        {
            // Arrange
            var args = new ConstLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId" },
                LcName = "anyLcName",
                Value = 1.0,
                Direction = new Vector3(1, 0, 0),
                Start = 0.2,
                End = 1.0,
                LoadOrientation = LoadOrientation.local,
            };

            // Act
            DistributedLoad load = CreateTestLoad(args);
            bool boolRes = load.TryGetValue(0.1, out double outValue, out bool _);

            // Assert
            Assert.That(boolRes, Is.False);
            Assert.That(outValue, Is.EqualTo(double.NaN));
        }

        [Test]
        public void Constructor_CreationOfNewLoad_InstantiateNewFebLoads()
        {
            // Arrange
            var args = new ConstLoadArgs
            {
                BeamIds = new List<string> { string.Empty },
                LcName = string.Empty,
                Value = 1.0,
                Direction = new Vector3(0, 0, 1),
                Start = 0.0,
                End = 1.0,
            };
            DistributedLoad load = CreateTestLoad(args);
            var k3d = new Toolkit();

            // Act
            double length = 10;
            Karamba.Models.Model model = BeamFactory.CreateHingedBeam(length, load, k3d.CroSec.CircularHollow());
            ModelBuilderFEB.AddLoadsAndSupports(model, model.lcActivation);

            feb.LoadCaseElement febLoads = model.febmodel.element(0).load_case(0);

            // Assert
            Assert.That(febLoads.size(), Is.EqualTo(1));

            feb.LoadTranslationalLine febLoad1 = _loadType is LoadTypes.Force
                ? SwigHelper.CastTo<feb.LoadTranslationalLine>(febLoads.load(0), false)
                : SwigHelper.CastTo<feb.LoadRotationalLine>(febLoads.load(0), false);
            Assert.That(febLoad1.swigCMemOwn, Is.EqualTo(false));
            Assert.That(febLoad1.pos(), Is.EqualTo(0.0 * length));

            // Assert.That(febLoad1.vals().ToArray(), Is.EqualTo(new double[] { 1.0 }));
            Assert.That(febLoad1.vals()[0], Is.EqualTo(1.0));
        }

        private DistributedLoad CreateTestLoad(ConstLoadArgs args)
        {
            switch (_loadType)
            {
                case LoadTypes.Force:
                    return new DistributedForce(
                        beamIds: args.BeamIds,
                        beamGuids: new List<Guid>(),
                        lcName: args.LcName,
                        loadOrientation: args.LoadOrientation,
                        direction: args.Direction,
                        new List<DistributedLoad.LoadPositionValue>
                        {
                            new DistributedLoad.LoadPositionValue(args.Start, args.Value),
                            new DistributedLoad.LoadPositionValue(args.End, args.Value),
                        });

                case LoadTypes.Moment:
                    return new DistributedMoment(
                        beamIds: args.BeamIds,
                        beamGuids: new List<Guid>(),
                        lcName: args.LcName,
                        loadOrientation: args.LoadOrientation,
                        direction: args.Direction,
                        new List<DistributedLoad.LoadPositionValue>
                        {
                            new DistributedLoad.LoadPositionValue(args.Start, args.Value),
                            new DistributedLoad.LoadPositionValue(args.End, args.Value),
                        });
                default:
                    throw new NotSupportedException();
            }
        }

        private class ConstLoadArgs
        {
            public List<string> BeamIds { get; set; }

            public string LcName { get; set; }

            public Vector3 Direction { get; set; }

            public LoadOrientation LoadOrientation { get; set; } = LoadOrientation.global;

            public double Value { get; set; }

            public double Start { get; set; } = 0;

            public double End { get; set; } = 1;
        }
    }
}
#endif
