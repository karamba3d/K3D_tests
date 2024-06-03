#if ALL_TESTS

namespace KarambaCommon.Tests.Elements
{
    using System;
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture]
    public class DisassembleElement_tests
    {
        [Test]
        public void InclinedBeam()
        {
            MessageLogger logger = new MessageLogger();
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(1, 1, 0);
            Line3 ln = new Line3(p0, p1);
            string beam_id = "b1";

            Toolkit k3d = new Toolkit();
            k3d.Part.LineToBeam(
                new List<Line3>() { ln },
                new List<string>() { beam_id },
                new List<CroSec>(),
                logger,
                out _);
        }

        [Test]
        public void BeamWithNodeIndexes()
        {
            Toolkit k3d = new Toolkit();
            BuilderBeam beam = k3d.Part.IndexToBeam(0, 1, "b1", new CroSec_Circle());

            IReadOnlyList<Point3> points = beam.Pos.Points();
            Assert.That(points.Count == 0);

            Vector3[] coosys = beam.Ori.CooSys(beam.Pos);
            Vector3 n0 = coosys[0];
            Vector3 n1 = coosys[1];

            Vector3 d0 = new Vector3(1, 0, 0);
            Vector3 d1 = new Vector3(0, 1, 0);
            Assert.That((n0 - d0).Length < 1E-9);
            Assert.That((n1 - d1).Length < 1E-9);
        }

        [Test]
        public void InclinedOrientedBeam()
        {
            MessageLogger logger = new MessageLogger();
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(1, 1, 0);
            Vector3 xDir = (p1 - p0).Unitized;
            Line3 ln = new Line3(p0, p1);
            string beam_id = "b1";

            Toolkit k3d = new Toolkit();
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3>() { ln },
                new List<string>() { beam_id },
                new List<CroSec>(),
                logger,
                out _);

            double alpha = 45.0.ToRad();
            IBuilderElementOrientationWriter ori = beams[0].Ori.Writer;
            ori.XOri = new Vector3(-1, 0, 0);
            ori.ZOri = new Vector3(0, 0, -1);
            ori.Alpha = alpha;
            beams[0].Ori = ori.Reader;

            Vector3[] coosys = beams[0].Ori.CooSys(beams[0].Pos);
            Vector3 n0 = coosys[0];
            Vector3 n2 = coosys[2];

            Vector3 n0_targ = -xDir;
            Assert.That((n0 - n0_targ).Length < 1E-9);

            Vector3 n2_targ = new Vector3(0, 0, -1);
            n2_targ.Rotate(alpha, n0);
            Assert.That((n2 - n2_targ).Length < 1E-9);
        }

        [Test]
        public void InclinedShell()
        {
            MessageLogger logger = new MessageLogger();
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(1, 0, 1);
            Point3 p2 = new Point3(1, 1, 1);
            Point3 p3 = new Point3(0, 1, 0);
            Mesh3 mesh = new Mesh3(
                new List<Point3>() { p0, p1, p2, p3 },
                new List<Face3>() { new Face3(0, 1, 2), new Face3(0, 2, 3) });
            string id = "s1";

            Toolkit k3d = new Toolkit();
            List<BuilderShell> outShells = k3d.Part.MeshToShell(
                new List<Mesh3>() { mesh },
                new List<string>() { id },
                new List<CroSec>(),
                logger,
                out _);

            Vector3[] coosys0 = outShells[0].Ori.CooSys(outShells[0].Pos, 0);
            Vector3 n0_0 = coosys0[0];
            Vector3 n0_1 = coosys0[1];
            double c = Math.Cos(45.0.ToRad());
            Vector3 xDir0 = new Vector3(c, 0, c);
            Vector3 yDir0 = new Vector3(0, 1, 0);
            Assert.That((n0_0 - xDir0).Length < 1E-9);
            Assert.That((n0_1 - yDir0).Length < 1E-9);

            Vector3[] coosys1 = outShells[0].Ori.CooSys(outShells[0].Pos, 1);
            Vector3 n1_0 = coosys1[0];
            Vector3 n1_1 = coosys1[1];
            Vector3 xDir1 = new Vector3(c, 0, c);
            Vector3 yDir1 = new Vector3(0, 1, 0);
            Assert.That((n1_0 - xDir1).Length < 1E-9);
            Assert.That((n1_1 - yDir1).Length < 1E-9);
        }

        [Test]
        public void InclinedOrientedShell()
        {
            MessageLogger logger = new MessageLogger();
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(1, 0, 0);
            Point3 p2 = new Point3(1, 1, 0);
            Point3 p3 = new Point3(0, 1, 0);
            Mesh3 mesh = new Mesh3(
                new List<Point3>() { p0, p1, p2, p3 },
                new List<Face3>() { new Face3(0, 1, 2), new Face3(0, 2, 3) });
            string id = "s1";

            Toolkit k3d = new Toolkit();
            List<BuilderShell> outShells = k3d.Part.MeshToShell(
                new List<Mesh3>() { mesh },
                new List<string>() { id },
                new List<CroSec>(),
                logger,
                out _);

            double alpha_rad = 20.0.ToRad();
            double c20 = Math.Cos(alpha_rad);
            double s20 = Math.Cos(alpha_rad);

            IBuilderElementOrientationWriter ori = outShells[0].Ori.Writer;
            ori.XOriList = new List<Vector3> { new Vector3(c20, s20, 0), new Vector3(-c20, -s20, 0) };

            // at the moment angles alpha take no effect in case of shells
            ori.AlphaList = new List<double>() { alpha_rad, 0 };
            outShells[0].Ori = ori.Reader;

            Vector3[] coosys0 = outShells[0].Ori.CooSys(outShells[0].Pos, 0);
            Vector3 n0_0 = coosys0[0];
            double c = Math.Cos(45.0.ToRad());
            Vector3 xDir0 = new Vector3(c, c, 0);
            Assert.That((n0_0 - xDir0).Length < 1E-9);

            Vector3[] coosys1 = outShells[0].Ori.CooSys(outShells[0].Pos, 1);
            Vector3 n1_0 = coosys1[0];
            Vector3 xDir1 = new Vector3(-c, -c, 0);
            Assert.That((n1_0 - xDir1).Length < 1E-9);
        }

        [Test]
        public void YZPlaneShell()
        {
            MessageLogger logger = new MessageLogger();
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(0, 1, 0);
            Point3 p2 = new Point3(0, 1, 1);
            Point3 p3 = new Point3(0, 0, 1);
            Mesh3 mesh = new Mesh3(
                new List<Point3>() { p0, p1, p2, p3 },
                new List<Face3>() { new Face3(0, 1, 2), new Face3(0, 2, 3) });
            string id = "s1";

            Toolkit k3d = new Toolkit();
            List<BuilderShell> outShells = k3d.Part.MeshToShell(
                new List<Mesh3>() { mesh },
                new List<string>() { id },
                new List<CroSec>(),
                logger,
                out _);

            Vector3[] coosys0 = outShells[0].Ori.CooSys(outShells[0].Pos, 0);
            Vector3 n0_0 = coosys0[0];
            Vector3 n0_1 = coosys0[1];
            Vector3 xDir0 = new Vector3(0, 1, 0);
            Vector3 yDir0 = new Vector3(0, 0, 1);
            Assert.That((n0_0 - xDir0).Length < 1E-9);
            Assert.That((n0_1 - yDir0).Length < 1E-9);

            Vector3[] coosys1 = outShells[0].Ori.CooSys(outShells[0].Pos, 1);
            Vector3 n1_0 = coosys1[0];
            Vector3 n1_1 = coosys1[1];
            Vector3 xDir1 = new Vector3(0, 1, 0);
            Vector3 yDir1 = new Vector3(0, 0, 1);
            Assert.That((n1_0 - xDir1).Length < 1E-9);
            Assert.That((n1_1 - yDir1).Length < 1E-9);
        }
    }
}

#endif