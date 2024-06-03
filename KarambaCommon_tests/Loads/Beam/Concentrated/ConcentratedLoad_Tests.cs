#if ALL_TESTS
namespace KarambaCommon.Tests.Loads
{
    using System.Collections.Generic;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Beam;
    using KarambaCommon.Tests.Helpers;
    using KarambaCommon;
    using NUnit.Framework;

    public class ConcentratedLoad_Tests
    {
        [Test]
        public void Cantilever_PointLoadAtMid()
        {
            Toolkit k3d = new Toolkit();
            int fz = 1;
            double pos = 0.5;
            ConcentratedForce load = k3d.Load.ConcentratedForceLoad(0.5, new Vector3(0, 0, fz), LoadOrientation.global);

            // Act
            int l = 10;
            Karamba.Models.Model model = BeamFactory.CreateCantileverBeam(l, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out IReadOnlyList<double> out_max_disp,
                out IReadOnlyList<double> out_g,
                out IReadOnlyList<double> out_comp,
                out string message);

            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double my0_target = -fz * l * pos;
            Assert.That(my0_target, Is.EqualTo(my0).Within(1e-5));

            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, l, true).force((int)feb.Node.DOF.y_r);
            double my1_target = 0;
            Assert.That(my1_target, Is.EqualTo(my1).Within(1e-5));

            // Assert
            feb.LoadCaseElement febLoads = model.febmodel.element(0).load_case(0);
            Assert.That(febLoads.size(), Is.EqualTo(1));
            Assert.That(febLoads.load(0).swigCMemOwn, Is.EqualTo(false));
        }

        [Test]
        public void Cantilever_PointMomentAtMid()
        {
            Toolkit k3d = new Toolkit();
            int me = 2;
            ConcentratedMoment load = k3d.Load.ConcentratedMomentLoad(0.5, new Vector3(0, me, 0), LoadOrientation.global);

            // Act
            int l = 10;
            Karamba.Models.Model model = BeamFactory.CreateCantileverBeam(l, load, k3d.CroSec.CircularHollow());
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out IReadOnlyList<double> out_max_disp,
                out IReadOnlyList<double> out_g,
                out IReadOnlyList<double> out_comp,
                out string message);

            double my0 = model.febmodel.element(0).interiorState(model.febmodel, 0, 0, true).force((int)feb.Node.DOF.y_r);
            double my0_target = me;
            Assert.That(my0_target, Is.EqualTo(my0).Within(1e-5));

            double my1 = model.febmodel.element(0).interiorState(model.febmodel, 0, l, true).force((int)feb.Node.DOF.y_r);
            double my1_target = 0;
            Assert.That(my1_target, Is.EqualTo(my1).Within(1e-5));

            // Assert
            feb.LoadCaseElement febLoads = model.febmodel.element(0).load_case(0);
            Assert.That(febLoads.size(), Is.EqualTo(1));
            Assert.That(febLoads.load(0).swigCMemOwn, Is.EqualTo(false));
        }
    }
}
#endif
