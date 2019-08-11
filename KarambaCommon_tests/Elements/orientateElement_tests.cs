using NUnit.Framework;
using System.Collections.Generic;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Utilities;
using Karamba.Elements;

namespace KarambaCommon.Tests.Elements
{
    [TestFixture]
    public class OrientateElement_tests
    {
#if ALL_TESTS
        [Test]
        public void orientBeam()
        {
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 1, 0);
            var ln = new Line3(p0, p1);
            var beam_id = "b1";
            var yOriNew = new Vector3(0, 1, 1);

            var k3d = new Toolkit();
            var beamBuilders = k3d.Part.LineToBeam(new List<Line3>() { ln }, new List<string>() { beam_id }, new List<CroSec>(), logger,
                out var out_points);
            beamBuilders.Add(k3d.Part.OrientateBeam("b1", null, yOriNew));
            var model = k3d.Model.AssembleModel(beamBuilders, null, null, out var info, out var mass, out var cog, out var msg, out var warning);

            k3d.Model.Disassemble(model, logger, out var points, out var lines, out var meshes, out var beams, out var shells, 
                out var supports, out var loads, out var materials, out var crosecs, out var joints);

            var yOri = beams[0].Ori.YOri;
            var diff = (yOri.Value - yOriNew).Length;
            Assert.AreEqual(diff, 0, 1E-10);
        }
#endif

#if ALL_TESTS
        [Test]
        public void orientShell()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 1);
            var p2 = new Point3(1, 1, 1);
            var mesh = new Mesh3(new List<Point3>() { p0, p1, p2}, new List<Face3>() { new Face3(0, 1, 2) });

            var shells = k3d.Part.MeshToShell(new List<Mesh3>() { mesh }, null, null, logger, out var outPoints);
            var old_shell = shells[0];
            var new_shell = (BuilderShell)old_shell.Clone();
            var ori = new_shell.Ori.Writer;
            ori.XOriList = new List<Vector3> { new Vector3(1, 1, 1) };
            new_shell.Ori = ori.Reader;

            Assert.True(old_shell.Ori.XOriGiven == false);
            Assert.True(new_shell.Ori.XOriGiven == true);
        }
#endif

#if ALL_TESTS
        [Test]
        public void modifyBeam()
        {
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 1, 0);
            var ln = new Line3(p0, p1);
            var beam_id = "b1";

            var k3d = new Toolkit();
            var beamBuilders = k3d.Part.LineToBeam(new List<Line3>() { ln }, new List<string>() { beam_id }, new List<CroSec>(), logger,
                out var out_points);

            var beam = beamBuilders[0];
            {
                var ori = beam.Ori.Writer;
                ori.XOri = new Vector3(1, 1, 1);
                ori.ZOri = new Vector3(2, 2, 2);
                ori.Alpha = 0.1;
                beam.Ori = ori.Reader;
            }
            Assert.AreEqual(beam.Ori.Alpha, 0.1, 1e-8);

        }
#endif
    }
}
