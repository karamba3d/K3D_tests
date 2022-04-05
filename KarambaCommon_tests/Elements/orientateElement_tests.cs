#if ALL_TESTS

namespace KarambaCommon.Tests.Elements
{
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class OrientateElement_tests
    {
        [Test]
        public void OrientBeam()
        {
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 1, 0);
            var ln = new Line3(p0, p1);
            var beam_id = "b1";
            var yOriNew = new Vector3(0, 1, 1);

            var k3d = new Toolkit();
            var beamBuilders = k3d.Part.LineToBeam(
                new List<Line3>() { ln },
                new List<string>() { beam_id },
                new List<CroSec>(),
                logger,
                out _);
            beamBuilders.Add(k3d.Part.OrientateBeam("b1", null, yOriNew));
            var model = k3d.Model.AssembleModel(
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
                out var beams,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _);

            var yOri = beams[0].Ori.YOri;
            var diff = (yOri.Value - yOriNew).Length;
            Assert.That(diff, Is.EqualTo(0).Within(1E-10));
        }

        [Test]
        public void OrientShell()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 1);
            var p2 = new Point3(1, 1, 1);
            var mesh = new Mesh3(new List<Point3>() { p0, p1, p2 }, new List<Face3>() { new Face3(0, 1, 2) });
            var shells = k3d.Part.MeshToShell(new List<Mesh3>() { mesh }, null, null, logger, out _);
            var old_shell = shells[0];
            var new_shell = (BuilderShell)old_shell.Clone();
            var ori = new_shell.Ori.Writer;
            ori.XOriList = new List<Vector3> { new Vector3(1, 1, 1) };
            new_shell.Ori = ori.Reader;

            Assert.That(old_shell.Ori.XOriGiven, Is.EqualTo(false));
            Assert.That(new_shell.Ori.XOriGiven, Is.EqualTo(true));
        }

        [Test]
        public void ModifyBeam()
        {
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 1, 0);
            var ln = new Line3(p0, p1);
            var beam_id = "b1";

            var k3d = new Toolkit();
            var beamBuilders = k3d.Part.LineToBeam(
                new List<Line3>() { ln },
                new List<string>() { beam_id },
                new List<CroSec>(),
                logger,
                out _);

            var beam = beamBuilders[0];
            {
                var ori = beam.Ori.Writer;
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