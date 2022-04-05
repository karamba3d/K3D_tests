#if ALL_TESTS
namespace KarambaCommon.Tests.Loads
{
    using System;
    using System.Collections.Generic;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Beam;
    using NUnit.Framework;
    using NUnitLite.Tests.Helpers;

    public class ConcentratedLoad_Tests
    {
        [Test]
        public void Cantilever_PointLoadAtMid()
        {
            var k3d = new Toolkit();
            var fz = 1;
            var pos = 0.5;
            var load = k3d.Load.ConcentratedForceLoad(0.5, new Vector3(0, 0, fz), LoadOrientation.global);

            // Act
            var l = 10;
            var model = BeamFactory.CreateCantileverBeam(l, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out var out_max_disp,
                out var out_g,
                out var out_comp,
                out var message);

            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double my0_target = -fz * l * pos;
            Assert.That(my0_target, Is.EqualTo(my0).Within(1e-5));

            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, l, true).force((int)feb.Node.DOF.y_r);
            double my1_target = 0;
            Assert.That(my1_target, Is.EqualTo(my1).Within(1e-5));

            // Assert
            var febLoads = model.febmodel.element(0).loadCase(0);
            Assert.That(febLoads.size(), Is.EqualTo(1));
            Assert.That(febLoads.load(0).swigCMemOwn, Is.EqualTo(false));
        }

        [Test]
        public void Cantilever_PointMomentAtMid()
        {
            var k3d = new Toolkit();
            var me = 2;
            var load = k3d.Load.ConcentratedMomentLoad(0.5, new Vector3(0, me, 0), LoadOrientation.global);

            // Act
            var l = 10;
            var model = BeamFactory.CreateCantileverBeam(l, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out var out_max_disp,
                out var out_g,
                out var out_comp,
                out var message);

            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double my0_target = me;
            Assert.That(my0_target, Is.EqualTo(my0).Within(1e-5));

            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, l, true).force((int)feb.Node.DOF.y_r);
            double my1_target = 0;
            Assert.That(my1_target, Is.EqualTo(my1).Within(1e-5));

            // Assert
            var febLoads = model.febmodel.element(0).loadCase(0);
            Assert.That(febLoads.size(), Is.EqualTo(1));
            Assert.That(febLoads.load(0).swigCMemOwn, Is.EqualTo(false));
        }
    }
}
#endif