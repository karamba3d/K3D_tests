#if ALL_TESTS

namespace KarambaCommon.Tests.Joints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;

    [TestFixture]
    public class BeamJointTests
    {
        public BeamJointTests()
        {
            // IniConfig.UnitSystem = UnitSystem.SI;
            IniConfig.DefaultUnits();
        }

        [Test]
        public void JointAgent_II()
        {
            var k3d = new Toolkit();
            var messageLogger = new MessageLogger();

            var lines = new List<Line3>()
            {
                new Line3(new Point3(), new Point3(5, 0, 0)),
                new Line3(new Point3(), new Point3(0, 1, 0)),
            };

            List<BuilderBeam> elems = k3d.Part.LineToBeam(
                lines, new List<string> {"A", "B"}, null, messageLogger, out var outNodes);

            var jointAgent = new JointAgent(
                null,
                new List<string> { "A" },
                new List<Guid>(),
                new List<string> { "B" },
                new List<int> { },
                null);

            Model model = k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out string info,
                out double mass,
                out Point3 cog,
                out string msg,
                out bool runtimeWarning,
                new List<Joint>() { jointAgent },
                null);

            var beams = model.elems.Select(i => i as ModelBeam).ToList();
            int nJoints = beams[0].joint != null ? 1 : 0;
            nJoints += beams[1].joint != null ? 1 : 0;
            Assert.That(nJoints, Is.EqualTo(1));
        }

        [Test]
        public void JointAgent_I()
        {
            var k3d = new Toolkit();
            var messageLogger = new MessageLogger();

            var lines = new List<Line3>()
            {
                new Line3(new Point3(), new Point3(5, 0, 0)),
                new Line3(new Point3(), new Point3(0, 1, 0)),
            };

            List<BuilderBeam> elems = k3d.Part.LineToBeam(
                lines, new List<string> {"A", "B"}, null, messageLogger, out var outNodes);

            var jointAgent = new JointAgent(
                null,
                new List<string> { "A" },
                new List<Guid>(),
                new List<string> { "C" },
                new List<int> { },
                null);

            Model model = k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out string info,
                out double mass,
                out Point3 cog,
                out string msg,
                out bool runtimeWarning,
                new List<Joint>() { jointAgent },
                null);

            var beams = model.elems.Select(i => i as ModelBeam).ToList();
            int nJoints = beams[0].joint != null ? 1 : 0;
            nJoints += beams[1].joint != null ? 1 : 0;
            Assert.That(nJoints, Is.EqualTo(0));
        }

        [Test]
        public void AxialSpringOnBothSides()
        {
            var k3d = new Toolkit();

            CroSec_Circle crosec = k3d.CroSec.CircularHollow();
            crosec.Az = 1E20; // make the cross section very stiff in shear

            var elems = new List<BuilderBeam>()
            {
                k3d.Part.IndexToBeam(0, 1, string.Empty, crosec), k3d.Part.IndexToBeam(1, 2, string.Empty, crosec),
            };

            double cx = 100;
            var joint0 = new Joint(
                new double?[] { cx, null, null, null, null, null, null, null, null, null, null, null });
            var joint1 = new Joint(
                new double?[] { null, null, null, null, null, null, cx, null, null, null, null, null });
            elems[0].joint = joint0;
            elems[1].joint = joint1;

            double l = 10.0; // in meter
            var points = new List<Point3> { new Point3(), new Point3(l * 0.5, 0, 0), new Point3(l, 0, 0) };

            var supports = new List<Support>
            {
                k3d.Support.Support(0, new List<bool> { true, true, true, true, false, false }),
                k3d.Support.Support(2, new List<bool> { false, true, true, false, false, false }),
            };

            double fx = 1.0; // in kN
            var loads = new List<Load> { k3d.Load.PointLoad(2, new Vector3(fx, 0, 0)), };

            Model model = k3d.Model.AssembleModel(
                elems,
                supports,
                loads,
                out string _, // info
                out double _, // mass
                out Point3 _, // cog
                out string _, // msg
                out bool _, // runtimeWarning
                new List<Joint>(),
                points);

            AnalyzeThI.solve(model,
                             out IReadOnlyList<double> outMaxDisp,
                             out IReadOnlyList<double> _, // outG
                             out IReadOnlyList<double> _, // outComp
                             out string _, // warning
                             out model);

            double a = (model.elems[0].crosec as CroSec_Beam).A;
            double e = model.elems[0].crosec.material.E();
            double c_inv_tot = l / (e * a) + 2 / cx;
            double dispTarget = Math.Abs(fx) * c_inv_tot;

            Assert.That(dispTarget, Is.EqualTo(outMaxDisp[0]).Within(1E-10));
        }
    }
}

#endif
