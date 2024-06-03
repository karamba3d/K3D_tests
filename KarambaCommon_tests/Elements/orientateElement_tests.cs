#if ALL_TESTS

namespace KarambaCommon.Tests.Elements
{
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;

    [TestFixture]
    public class OrientateElement_tests
    {
        public OrientateElement_tests()
        {
            // IniConfig.UnitSystem = UnitSystem.SI;
            IniConfig.DefaultUnits();
        }

        [Test]
        public void OrientBeam()
        {
            MessageLogger logger = new MessageLogger();
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(1, 1, 0);
            Line3 ln = new Line3(p0, p1);
            string beam_id = "b1";
            Vector3 yOriNew = new Vector3(0, 1, 1);

            Toolkit k3d = new Toolkit();
            List<BuilderBeam> beamBuilders = k3d.Part.LineToBeam(
                new List<Line3>() { ln },
                new List<string>() { beam_id },
                new List<CroSec>(),
                logger,
                out _);
            beamBuilders.Add(k3d.Part.OrientateBeam("b1", null, yOriNew));
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                beamBuilders,
                null,
                null,
                out _,
                out _,
                out _,
                out _,
                out _);
            k3d.Model.Disassemble(
                model,
                logger,
                out _,
                out _,
                out _,
                out List<BuilderBeam> beams,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _);

            Vector3? yOri = beams[0].Ori.YOri;
            double diff = (yOri.Value - yOriNew).Length;
            Assert.That(diff, Is.EqualTo(0).Within(1E-10));
        }

        [Test]
        public void OrientShell()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(1, 0, 1);
            Point3 p2 = new Point3(1, 1, 1);
            Mesh3 mesh = new Mesh3(new List<Point3>() { p0, p1, p2 }, new List<Face3>() { new Face3(0, 1, 2) });
            List<BuilderShell> shells = k3d.Part.MeshToShell(new List<Mesh3>() { mesh }, null, null, logger, out _);
            BuilderShell old_shell = shells[0];
            BuilderShell new_shell = (BuilderShell)old_shell.Clone();
            IBuilderElementOrientationWriter ori = new_shell.Ori.Writer;
            ori.XOriList = new List<Vector3> { new Vector3(1, 1, 1) };
            new_shell.Ori = ori.Reader;

            Assert.That(old_shell.Ori.XOriGiven, Is.EqualTo(false));
            Assert.That(new_shell.Ori.XOriGiven, Is.EqualTo(true));
        }

        [Test]
        public void ModifyBeam()
        {
            MessageLogger logger = new MessageLogger();
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(1, 1, 0);
            Line3 ln = new Line3(p0, p1);
            string beam_id = "b1";

            Toolkit k3d = new Toolkit();
            List<BuilderBeam> beamBuilders = k3d.Part.LineToBeam(
                new List<Line3>() { ln },
                new List<string>() { beam_id },
                new List<CroSec>(),
                logger,
                out _);

            BuilderBeam beam = beamBuilders[0];
            {
                IBuilderElementOrientationWriter ori = beam.Ori.Writer;
                ori.XOri = new Vector3(1, 1, 1);
                ori.ZOri = new Vector3(2, 2, 2);
                ori.Alpha = 0.1;
                beam.Ori = ori.Reader;
            }

            Assert.That(beam.Ori.Alpha, Is.EqualTo(0.1).Within(1e-8));
        }
    }
}

#endif
