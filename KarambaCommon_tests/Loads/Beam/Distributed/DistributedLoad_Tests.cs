#if ALL_TESTS
namespace KarambaCommon.Tests.Loads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Beam;
    using NSubstitute;
    using NUnit.Framework;
    using NUnitLite.Tests.Helpers;

    [TestFixture]
    public class DistributedLoad_Tests
    {
        [Test]
        public void Constructor_ValuesAndPositions_WillBeSorted()
        {
            // Arrange
            var args = new DistLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId1", "anyBeamId2" },
                LcName = "anyLcName",
                Direction = new Vector3 { X = 1, Y = 1, Z = 1 },
                Positions = new List<double> { 0.5, 0.0, 1.0 },
                Values = new List<double> { 3.0, 2.0, 1.0 },
            };

            // Act
            var load = CreateLoadInstance(args);

            // Assert
            Assert.That(load.Positions.ToArray(), Is.EqualTo(new[] { 0.0, 0.5, 1.0, 1.0 }));
            Assert.That(load.Values.ToArray(), Is.EqualTo(new[] { 2.0, 3.0, 1.0, 0.0 }));
        }

        [Test]
        public void TryComputeValueFromPosition_AValidValue_WillBeLinearlyInterpolated()
        {
            // Arrange
            var args = new DistLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId1", "anyBeamId2" },
                LcName = "anyLcName",
                Direction = new Vector3 { X = 1, Y = 1, Z = 1 },
                Positions = new List<double> { 0, 1 },
                Values = new List<double> { 1, 2 },
            };

            // Act
            var distributedLoad = CreateLoadInstance(args);
            var boolRes = distributedLoad.TryGetValue(0.5, out var outValue, out var _);

            // Assert
            Assert.That(boolRes, Is.True);
            Assert.That(outValue, Is.EqualTo(1.5).Within(double.Epsilon));
        }

        [Test]
        public void TryComputeValueFromPosition_NonValidValue_ReturnsFalse()
        {
            // Arrange
            var args = new DistLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId1", "anyBeamId2" },
                LcName = "anyLcName",
                Direction = new Vector3 { X = 1, Y = 1, Z = 1 },
                Positions = new List<double> { 0, 0.5 },
                Values = new List<double> { 1, 2 },
            };

            // Act
            var distributedLoad = CreateLoadInstance(args);
            var boolRes = distributedLoad.TryGetValue(1, out var outValue, out var _);

            // Assert
            Assert.That(boolRes, Is.False);
            Assert.That(outValue, Is.EqualTo(double.NaN));
        }

#if TEST_CAN_THROW_EXCEPTION
        [Test]
        public void Constructor_DifferentValuesAndPositionsCount_WillThrowException()
        {
            // Arrange
            var args = new DistLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId1", "anyBeamId2" },
                LcName = "anyLcName",
                Direction = new Vector3 { X = 1, Y = 1, Z = 1 },
                Values = new List<double> { 0.0, 1.0 },
                Positions = new List<double> { 3.0, 2.0, 1.0 },
            };

            // Act + Assert
            try
            {
                CreateLoadInstance(args);

                Assert.Fail();
            }
            catch (Exception e)
            {
                // This error exception is due to the use of a mock
                Assert.That(e, Is.TypeOf<TargetInvocationException>());

                // This check that the exception throw is as expected
                Assert.That(e.InnerException, Is.TypeOf<ArgumentException>());
            }
        }

        [Test]
        public void Constructor_NullValueOrPosition_WillThrowException()
        {
            // Arrange
            var args = new DistLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId1", "anyBeamId2" },
                LcName = "anyLcName",
                Direction = new Vector3 { X = 1, Y = 1, Z = 1 },
                Values = null,
                Positions = null,
            };

            // Act + Assert
            try
            {
                CreateLoadInstance(args);

                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.That(e, Is.TypeOf<TargetInvocationException>());
                Assert.That(e.InnerException, Is.TypeOf<ArgumentNullException>());
            }
        }

        [Test]
        [TestCase(-1.0)]
        [TestCase(2.0)]
        public void Constructor_InvalidPosition_WillThrowException(double position)
        {
            // Arrange
            var args = new DistLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId1", "anyBeamId2" },
                LcName = "anyLcName",
                Direction = new Vector3 { X = 1, Y = 1, Z = 1 },
                Positions = new List<double> { position },
                Values = new List<double> { 0 },
            };

            // Act + Assert
            try
            {
                CreateLoadInstance(args);

                Assert.Fail();
            }
            catch (Exception e)
            {
                // This error exception is due to the use of a mock
                Assert.That(e, Is.TypeOf<TargetInvocationException>());

                // This check that the exception throw is as expected
                Assert.That(e.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }
#endif

        private DistributedLoad CreateLoadInstance(DistLoadArgs args)
        {
            var constructObjects = new object[]
            {
                args.BeamIds, new List<Guid>(), args.LcName, args.LoadOrientation, args.Direction, args.Positions, args.Values,
            };
            return Substitute.For<DistributedLoad>(constructObjects);
        }

        private class DistLoadArgs
        {
            public List<string> BeamIds { get; set; }

            public string LcName { get; set; }

            public Vector3 Direction { get; set; }

            public LoadOrientation LoadOrientation { get; set; } = LoadOrientation.global;

            public List<double> Positions { get; set; }

            public List<double> Values { get; set; }
        }

        [Test]
        public void CantileverBeam_TorsionalMoment_TriangleLoad()
        {
            var k3d = new Toolkit();
            var m = 1.0;
            var load = k3d.Load.DistributedMomentLoad(
                Vector3.XAxis,
                new List<double> { 0, 1, 0 },
                new List<double> { 0.25, 0.5, 0.75 },
                LoadOrientation.local);

            // Act
            const double length = 1;
            var model = BeamFactory.CreateCantileverBeam(length, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out var message);

            const double aLoad = 0.25 * length;
            var mxTarget = m * aLoad;

            double mx1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.x_r);
            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.y_r);
            double mz1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx1, Is.EqualTo(0).Within(1e-5));
            Assert.That(my1, Is.EqualTo(0).Within(1e-5));
            Assert.That(mz1, Is.EqualTo(0).Within(1e-5));

            double mx0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.x_r);
            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double mz0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mxTarget, Is.EqualTo(mx0).Within(1e-5));
            Assert.That(my0, Is.EqualTo(0).Within(1e-5));
            Assert.That(mz0, Is.EqualTo(0).Within(1e-5));
        }

        [Test]
        public void CantileverBeam_TorsionalMoment_BlockLoad()
        {
            var k3d = new Toolkit();
            var m = 1.0;
            var load = k3d.Load.DistributedMomentLoad(
                Vector3.XAxis,
                new List<double> { 1, 1 },
                new List<double> { 0.25, 0.75 },
                LoadOrientation.local);

            // Act
            const double length = 1;
            var model = BeamFactory.CreateCantileverBeam(length, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out var message);

            const double aLoad = 0.50 * length;
            var mxTarget = m * aLoad;

            double mx1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.x_r);
            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.y_r);
            double mz1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx1, Is.EqualTo(0).Within(1e-5));
            Assert.That(my1, Is.EqualTo(0).Within(1e-5));
            Assert.That(mz1, Is.EqualTo(0).Within(1e-5));

            double mx0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.x_r);
            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double mz0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mxTarget, Is.EqualTo(mx0).Within(1e-5));
            Assert.That(my0, Is.EqualTo(0).Within(1e-5));
            Assert.That(mz0, Is.EqualTo(0).Within(1e-5));
        }

        [Test]
        public void CantileverBeam_TorsionalMoment_TrapezoidLoad()
        {
            var k3d = new Toolkit();
            var m = 2.0;
            var load = k3d.Load.DistributedMomentLoad(
                Vector3.XAxis,
                new List<double> { 0, m, m, 0 },
                new List<double> { 0.5, 0.6, 0.9, 1.0 },
                LoadOrientation.local);

            // Act
            const double length = 1;
            var model = BeamFactory.CreateCantileverBeam(length, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out var message);

            const double areaLoad = (0.50 + 0.3) * 0.5 * length;
            var mxTarget = m * areaLoad;

            double mx1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.x_r);
            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.y_r);
            double mz1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx1, Is.EqualTo(0).Within(1e-5));
            Assert.That(my1, Is.EqualTo(0).Within(1e-5));
            Assert.That(mz1, Is.EqualTo(0).Within(1e-5));

            double mx0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.x_r);
            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double mz0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mxTarget, Is.EqualTo(mx0).Within(1e-5));
            Assert.That(my0, Is.EqualTo(0).Within(1e-5));
            Assert.That(mz0, Is.EqualTo(0).Within(1e-5));
        }

        [Test]
        public void CantileverBeam_BendingMomentMy_ForceBlockLoad()
        {
            var k3d = new Toolkit();
            var m = 2.0;
            var load = k3d.Load.DistributedForceLoad(
                Vector3.ZAxis,
                new List<double> { m, m },
                new List<double> { 0.5, 1.0 },
                LoadOrientation.local);

            // Act
            const double length = 1;
            var model = BeamFactory.CreateCantileverBeam(length, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out var message);

            const double areaLoad = 0.5 * length;
            var myTarget = -m * areaLoad * length * 0.75;

            double mx1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.x_r);
            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.y_r);
            double mz1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx1, Is.EqualTo(0).Within(1e-5));
            Assert.That(my1, Is.EqualTo(0).Within(1e-5));
            Assert.That(mz1, Is.EqualTo(0).Within(1e-5));

            double mx0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.x_r);
            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double mz0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx0, Is.EqualTo(0).Within(1e-5));
            Assert.That(my0, Is.EqualTo(myTarget).Within(1e-5));
            Assert.That(mz0, Is.EqualTo(0).Within(1e-5));
        }

        [Test]
        public void CantileverBeam_BendingMomentMy_MomentBlockLoad()
        {
            var k3d = new Toolkit();
            var m = 2.0;
            var load = k3d.Load.DistributedMomentLoad(
                Vector3.YAxis,
                new List<double> { m, m },
                new List<double> { 0.0, 1.0 },
                LoadOrientation.local);

            // Act
            const double length = 1;
            var model = BeamFactory.CreateCantileverBeam(length, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out var message);

            const double areaLoad = length;
            var myTarget = m * areaLoad;

            double mx0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.x_r);
            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double mz0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx0, Is.EqualTo(0).Within(1e-5));
            Assert.That(my0, Is.EqualTo(myTarget).Within(1e-5));
            Assert.That(mz0, Is.EqualTo(0).Within(1e-5));

            double mx1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.x_r);
            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.y_r);
            double mz1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx1, Is.EqualTo(0).Within(1e-5));
            Assert.That(my1, Is.EqualTo(0).Within(1e-5));
            Assert.That(mz1, Is.EqualTo(0).Within(1e-5));
        }

        [Test]
        public void CantileverBeam_BendingMomentMz_MomentBlockLoad()
        {
            var k3d = new Toolkit();
            var m = 2.0;
            var load = k3d.Load.DistributedMomentLoad(
                Vector3.ZAxis,
                new List<double> { m, m },
                new List<double> { 0.0, 1.0 },
                LoadOrientation.local);

            // Act
            const double length = 1;
            var model = BeamFactory.CreateCantileverBeam(length, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out var message);

            const double areaLoad = length;
            var mzTarget = m * areaLoad;

            double mx0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.x_r);
            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double mz0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx0, Is.EqualTo(0).Within(1e-5));
            Assert.That(my0, Is.EqualTo(0).Within(1e-5));
            Assert.That(mzTarget, Is.EqualTo(mz0).Within(1e-5));

            double mx1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.x_r);
            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.y_r);
            double mz1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx1, Is.EqualTo(0).Within(1e-5));
            Assert.That(my1, Is.EqualTo(0).Within(1e-5));
            Assert.That(mz1, Is.EqualTo(0).Within(1e-5));
        }
    }
}
#endif