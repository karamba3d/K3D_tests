#if ALL_TESTS
namespace KarambaCommon.Tests.Loads
{
    using System;
    using System.Collections.Generic;
    using feb;
    using Karamba.CrossSections;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Beam;
    using Karamba.Models;
    using KarambaCommon.Tests.Helpers;
    using KarambaCommon.Tests.Loads;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture(LoadTypes.Translational)]
    [TestFixture(LoadTypes.Rotational)]
    public class GapLoads_Tests_T
    {
        private LoadTypes _loadType;

        public GapLoads_Tests_T(LoadTypes type) => _loadType = type;

        private class TestLoadArgs
        {
            public List<string> BeamIds { get; set; }

            public string LcName { get; set; }

            public double Position { get; set; }

            public LoadOrientation LoadOrientation { get; set; } = LoadOrientation.global;

            public Vector3 Values { get; set; }
        }

        private ConcentratedLoad CreateTestLoad(TestLoadArgs args)
        {
            switch (_loadType)
            {
                case LoadTypes.Translational:
                    return new TranslationalGap(
                        beamIds: args.BeamIds,
                        beamGuids: new List<Guid>(),
                        lcName: args.LcName,
                        loadOrientation: args.LoadOrientation,
                        gaps: args.Values,
                        position: args.Position);

                case LoadTypes.Rotational:
                    return new RotationalGap(
                        beamIds: args.BeamIds,
                        beamGuids: new List<Guid>(),
                        lcName: args.LcName,
                        loadOrientation: args.LoadOrientation,
                        gaps: args.Values,
                        position: args.Position);
                default:
                    throw new NotSupportedException();
            }
        }

#if TEST_CAN_THROW_EXCEPTION
        [Test]
        [TestCase(-1.0)]
        [TestCase(2.0)]
        public void Constructor_InvalidPosition_WillThrowException(double position)
        {
            // Arrange
            var args = new TestLoadArgs
            {
                BeamIds = new List<string> { "anyBeamId" },
                LcName = "anyLcName",
                Values = new Vector3(0, 0, 1),
                Position = position,
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
        public void Constructor_NewLoad_WillInstantiateNewFebLoads()
        {
            // Arrange
            TestLoadArgs args = new TestLoadArgs
            {
                BeamIds = new List<string> { string.Empty },
                LcName = string.Empty,
                Values = new Vector3(0, 0, 1),
                Position = 0.5,
            };
            ConcentratedLoad load = CreateTestLoad(args);
            Toolkit k3d = new Toolkit();

            // Act
            Karamba.Models.Model model = BeamFactory.CreateHingedBeam(10, load, k3d.CroSec.CircularHollow());
            ModelBuilderFEB.AddLoadsAndSupports(model, model.lcActivation);

            LoadCaseElement febLoads = model.febmodel.element(0).load_case(0);

            // Assert
            Assert.That(febLoads.size(), Is.EqualTo(1));
            Assert.That(febLoads.load(0).swigCMemOwn, Is.EqualTo(false));
        }

        [Test]
        public void FixedBeam_Rotational_GapAtMidPoint()
        {
            Toolkit k3d = new Toolkit();
            Vector3 theta = new Vector3(0.1, 0.2, 0.3);
            RotationalGap load = k3d.Load.RotationalGapLoad(0.5, theta);

            // Act
            const double length = 10.0;
            Karamba.Models.Model model = BeamFactory.CreateFixedFixedBeam(length, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out IReadOnlyList<double> outMaxDisp,
                out IReadOnlyList<double> out_g,
                out IReadOnlyList<double> out_comp,
                out string message);

            CroSec_Beam crosec = model.elems[0].crosec as CroSec_Beam;
            double mx_targ = -theta.X * crosec.material.G12() * crosec.Ipp / length;
            double my_targ = -theta.Y * crosec.material.E() * crosec.Iyy / length;
            double mz_targ = -theta.Z * crosec.material.E() * crosec.Izz / length;

            double mx0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.x_r);
            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double mz0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx_targ, Is.EqualTo(mx0).Within(1e-5));
            Assert.That(my_targ, Is.EqualTo(my0).Within(1e-5));
            Assert.That(mz_targ, Is.EqualTo(mz0).Within(1e-5));

            double mx1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.x_r);
            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.y_r);
            double mz1 = model.febmodel.element(0).interiorState(model.febmodel, 0, length, true).force((int)feb.Node.DOF.z_r);
            Assert.That(mx_targ, Is.EqualTo(mx1).Within(1e-5));
            Assert.That(my_targ, Is.EqualTo(my1).Within(1e-5));
            Assert.That(mz_targ, Is.EqualTo(mz1).Within(1e-5));
        }
    }
}
#endif
