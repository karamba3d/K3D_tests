using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Supports;
using Karamba.Models;
using Karamba.Utilities;
using Karamba.Algorithms;

namespace KarambaCommon.Tests.Elements
{
    [TestFixture]
    public class LocalCoordinateSystems_tests
    {
#if ALL_TESTS
        [Test]
        public void InclinedBeam()
        {
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 1, 0);
            var XDir = (p1 - p0).Unitized;
            var ln = new Line3(p0, p1);
            var beam_id = "b1";

            var k3d = new Toolkit();
            var beams = k3d.Part.LineToBeam(new List<Line3>() { ln }, new List<string>() { beam_id }, new List<CroSec>(), logger,
                out var out_points);

            var points = beams[0].Pos.Points();
            Assert.That((p0 - points[0]).Length < 1E-9);
            Assert.That((p1 - points[1]).Length < 1E-9);

            var coosys = beams[0].Ori.CooSys(beams[0].Pos);
            var n0 = coosys[0];
            var n1 = coosys[1];
            Assert.That((n0 - XDir).Length < 1E-9);
        }
#endif

#if ALL_TESTS
        [Test]
        public void BeamWithNodeIndexes()
        {
            var k3d = new Toolkit();
            var beam = k3d.Part.IndexToBeam(0, 1, "b1", new CroSec_Circle());

            var points = beam.Pos.Points();
            Assert.That(points.Count == 0);

            var coosys = beam.Ori.CooSys(beam.Pos);
            var n0 = coosys[0];
            var n1 = coosys[1];

            var d0 = new Vector3(1, 0, 0);
            var d1 = new Vector3(0, 1, 0);
            Assert.That((n0 - d0).Length < 1E-9);
            Assert.That((n1 - d1).Length < 1E-9);
        }
#endif

#if ALL_TESTS
        [Test]
        public void InclinedOrientedBeam()
        {
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 1, 0);
            var XDir = (p1 - p0).Unitized;
            var ln = new Line3(p0, p1);
            var beam_id = "b1";

            var k3d = new Toolkit();
            var beams = k3d.Part.LineToBeam(new List<Line3>() { ln }, new List<string>() { beam_id }, new List<CroSec>(), logger,
                out var out_points);
                        
            var alpha = 45.0.ToRad();
            beams[0].Ori.XOri = new Vector3(-1,0,0);
            beams[0].Ori.ZOri = new Vector3( 0, 0,-1);
            beams[0].Ori.Alpha = alpha;
            
            var coosys = beams[0].Ori.CooSys(beams[0].Pos);
            var n0 = coosys[0];
            var n1 = coosys[1];
            var n2 = coosys[2];

            var n0_targ = -XDir;
            Assert.That((n0 - n0_targ).Length < 1E-9);

            var n2_targ = new Vector3(0,0,-1);
            n2_targ.Rotate(alpha, n0);
            Assert.That((n2 - n2_targ).Length < 1E-9);
        }
#endif

#if ALL_TESTS
        [Test]
        public void InclinedShell()
        {
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 1);
            var p2 = new Point3(1, 1, 1);
            var p3 = new Point3(0, 1, 0);
            var mesh = new Mesh3(new List<Point3>(){p0, p1, p2, p3}, new List<Face3>(){new Face3(0,1,2), new Face3(0,2,3)});
            var id = "s1";

            var k3d = new Toolkit();
            var outShells = k3d.Part.ShellToMesh(new List<Mesh3>() { mesh }, new List<string>() { id }, new List<CroSec>(), logger, out var outPoints);

            var coosys0 = outShells[0].Ori.CooSys(outShells[0].Pos, 0);
            var n0_0 = coosys0[0];
            var n0_1 = coosys0[1];
            var c = Math.Cos(45.0.ToRad());
            var xDir0 = new Vector3(c, 0, c);
            var yDir0 = new Vector3(0, 1, 0);
            Assert.That((n0_0 - xDir0).Length < 1E-9);
            Assert.That((n0_1 - yDir0).Length < 1E-9);

            var coosys1 = outShells[0].Ori.CooSys(outShells[0].Pos, 1);
            var n1_0 = coosys1[0];
            var n1_1 = coosys1[1];
            var xDir1 = new Vector3(c, 0, c);
            var yDir1 = new Vector3(0, 1, 0);
            Assert.That((n1_0 - xDir1).Length < 1E-9);
            Assert.That((n1_1 - yDir1).Length < 1E-9);
        }
#endif

#if ALL_TESTS
        [Test]
        public void InclinedOrientedShell()
        {
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 0);
            var p2 = new Point3(1, 1, 0);
            var p3 = new Point3(0, 1, 0);
            var mesh = new Mesh3(new List<Point3>() { p0, p1, p2, p3 }, new List<Face3>() { new Face3(0, 1, 2), new Face3(0, 2, 3) });
            var id = "s1";

            var k3d = new Toolkit();
            var outShells = k3d.Part.ShellToMesh(new List<Mesh3>() { mesh }, new List<string>() { id }, new List<CroSec>(), logger, out var outPoints);

            var alpha_rad = 20.0.ToRad();
            var c20 = Math.Cos(alpha_rad);
            var s20 = Math.Cos(alpha_rad);

            outShells[0].Ori.XOriList = new List<Vector3> { new Vector3(c20, s20, 0), new Vector3(-c20, -s20, 0) };

            // at the moment angles alpha take no effect in case of shells
            outShells[0].Ori.AlphaList = new List<double>() {alpha_rad, 0};

            var coosys0 = outShells[0].Ori.CooSys(outShells[0].Pos, 0);
            var n0_0 = coosys0[0];
            var c = Math.Cos(45.0.ToRad());
            var xDir0 = new Vector3(c, c, 0);
            Assert.That((n0_0 - xDir0).Length < 1E-9);

            var coosys1 = outShells[0].Ori.CooSys(outShells[0].Pos, 1);
            var n1_0 = coosys1[0];
            var xDir1 = new Vector3(-c, -c, 0);
            Assert.That((n1_0 - xDir1).Length < 1E-9);
        }
#endif

#if ALL_TESTS
        [Test]
        public void YZPlaneShell()
        {
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(0, 1, 0);
            var p2 = new Point3(0, 1, 1);
            var p3 = new Point3(0, 0, 1);
            var mesh = new Mesh3(new List<Point3>() { p0, p1, p2, p3 }, new List<Face3>() { new Face3(0, 1, 2), new Face3(0, 2, 3) });
            var id = "s1";

            var k3d = new Toolkit();
            var outShells = k3d.Part.ShellToMesh(new List<Mesh3>() { mesh }, new List<string>() { id }, new List<CroSec>(), logger, out var outPoints);

            var coosys0 = outShells[0].Ori.CooSys(outShells[0].Pos, 0);
            var n0_0 = coosys0[0];
            var n0_1 = coosys0[1];
            var c = Math.Cos(45.0.ToRad());
            var xDir0 = new Vector3(0, 1, 0);
            var yDir0 = new Vector3(0, 0, 1);
            Assert.That((n0_0 - xDir0).Length < 1E-9);
            Assert.That((n0_1 - yDir0).Length < 1E-9);

            var coosys1 = outShells[0].Ori.CooSys(outShells[0].Pos, 1);
            var n1_0 = coosys1[0];
            var n1_1 = coosys1[1];
            var xDir1 = new Vector3(0, 1, 0);
            var yDir1 = new Vector3(0, 0, 1);
            Assert.That((n1_0 - xDir1).Length < 1E-9);
            Assert.That((n1_1 - yDir1).Length < 1E-9);
        }
#endif
    }
}
