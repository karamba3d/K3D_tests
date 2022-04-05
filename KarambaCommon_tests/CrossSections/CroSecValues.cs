#if ALL_TESTS

namespace KarambaCommon.Tests.CrossSections
{
    using System;
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class CroSecValues_tests
    {
        [Test]
        public void BoxValues()
        {
            var k3d = new Toolkit();
            var cs = k3d.CroSec.Box(390, 60, 60, 23.6, 0.1, 2.7, 0, -1);
            var a = cs._height * cs.uf_width -
                    (cs._height - cs.uf_thick - cs.lf_thick) * (cs.uf_width - 2 * cs.w_thick);
            var ufA = cs.uf_thick * cs.uf_width;
            var lfA = cs.lf_thick * cs.lf_width;
            var wh = cs._height - cs.uf_thick - cs.lf_thick;
            var wA = wh * cs.w_thick * 2;
            var sy = ufA * cs.uf_thick * 0.5 + lfA * (cs._height - 0.5 * cs.lf_thick) + wA * (cs.uf_thick + wh * 0.5);
            var zs = sy / a;
            Assert.That(cs.A, Is.EqualTo(a).Within(1E-10));
            Assert.That(cs.zs, Is.EqualTo(zs).Within(1E-10));
        }

        [Test]
        public void ChannelValues()
        {
            var k3d = new Toolkit();
            var cs = k3d.CroSec.Box(15, 20, 20, 0, 1.5, 2.0, 0, -1);
            var a = cs._height * cs.uf_width -
                    (cs._height - cs.uf_thick - cs.lf_thick) * (cs.uf_width - 2 * cs.w_thick);
            var ufA = cs.uf_thick * cs.uf_width;
            var lfA = cs.lf_thick * cs.lf_width;
            var wh = cs._height - cs.uf_thick - cs.lf_thick;
            var wA = wh * cs.w_thick * 2;
            var sy = ufA * cs.uf_thick * 0.5 + lfA * (cs._height - 0.5 * cs.lf_thick) + wA * (cs.uf_thick + wh * 0.5);
            var zs = sy / a;
            var iyy = 1.9105714285714284e-5;
            var ipp = 94.5 * 1E-8;

            Assert.That(cs.A, Is.EqualTo(a).Within(1E-10));
            Assert.That(cs.zs, Is.EqualTo(zs).Within(1E-10));
            Assert.That(cs.Iyy, Is.EqualTo(iyy).Within(1E-10));
            Assert.That(cs.Ipp, Is.EqualTo(ipp).Within(1E-10));
        }

        // # values for mext tests
        private readonly double _w_thick = 0.4;
        private readonly double _uf_thick = 0.3;
        private readonly double _lf_thick = 0.2;
        private static readonly double FilletR = 0.2;
        private readonly double _height = 15;
        private readonly double _uf_width = 10;
        private readonly double _lf_width = 5;
        private readonly double _fillet_l = 0.25 * 2 * Math.PI * FilletR;

        // /*
        [Test]
        public void CroSec_ExteriorPerimeter()
        {
            var ucf = UnitsConversionFactory.Conv();
            var cm = ucf.cm();

            // # CroSec_I
            var cs_i = new CroSec_I(
                "test",
                "testI",
                null,
                null,
                null,
                _height,
                _uf_width,
                _lf_width,
                _uf_thick,
                _lf_thick,
                _w_thick,
                FilletR,
                0,
                0);
            var cs_i_expected = cm.toBase(2 * _height + 2 * _uf_width + 2 * _lf_width + 4 * _fillet_l - 8 * FilletR);

            // # CroSec_Box
            var cs_box = new CroSec_Box(
                "test",
                "testBox",
                null,
                null,
                null,
                _height,
                _uf_width,
                _lf_width,
                _uf_thick,
                _lf_thick,
                _w_thick,
                FilletR,
                0);
            var cs_box_expected = cm.toBase(2 * _height + _uf_width + _lf_width + 4 * _fillet_l - 8 * FilletR);

            // # CroSec_Circle
            var cs_circle = new CroSec_Circle("test", "testCirc", null, null, null, _height, _w_thick);
            var cs_circle_expected = cm.toBase(Math.PI * _height);

            // # CroSec_T
            var cs_t = new CroSec_T(
                "test",
                "testT",
                null,
                null,
                null,
                _height,
                _uf_width,
                _uf_thick,
                _w_thick,
                FilletR,
                0,
                0);
            var cs_t_expected = cm.toBase(2 * _height + 2 * _uf_width + 2 * _fillet_l - 4 * FilletR);

            // # CroSec_Trapezoid
            var cs_trapezoid = new CroSec_Trapezoid(
                "test",
                "testTrapezoid",
                null,
                null,
                null,
                _height,
                _lf_width,
                _uf_width);
            var diff = Math.Abs(_uf_width - _lf_width);
            var cs_trapezoid_expected = cm.toBase(
                _uf_width + _lf_width + 2 * Math.Sqrt(Math.Pow(_height, 2) + Math.Pow(diff, 2)));

            Assert.That(cs_box.exteriorPerimeter, Is.EqualTo(cs_box_expected).Within(1E-3));
            Assert.That(cs_circle.exteriorPerimeter, Is.EqualTo(cs_circle_expected).Within(1E-3));
            Assert.That(cs_i.exteriorPerimeter, Is.EqualTo(cs_i_expected).Within(1E-3));
            Assert.That(cs_t.exteriorPerimeter, Is.EqualTo(cs_t_expected).Within(1E-3));
            Assert.That(cs_trapezoid.exteriorPerimeter, Is.EqualTo(cs_trapezoid_expected).Within(1E-3));
        }

        // */

        // /*
        [Test]
        public void CroSec_SurfaceArea()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            var ucf = UnitsConversionFactory.Conv();
            var cm = ucf.cm();

            var l = 10;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(l, 0, 0);
            var line = new Line3(p0, p1);

            var croSecI = new CroSec_I(
                "test",
                "testI",
                null,
                null,
                null,
                _height,
                _uf_width,
                _lf_width,
                _uf_thick,
                _lf_thick,
                _w_thick,
                FilletR,
                0,
                0);
            var croSecBox = new CroSec_Box(
                "test",
                "testBox",
                null,
                null,
                null,
                _height,
                _uf_width,
                _lf_width,
                _uf_thick,
                _lf_thick,
                _w_thick,
                FilletR,
                0);
            var croSecCircle = new CroSec_Circle("test", "testCirc", null, null, null, _height, _w_thick);
            var croSecT = new CroSec_T(
                "test",
                "testT",
                null,
                null,
                null,
                _height,
                _uf_width,
                _uf_thick,
                _w_thick,
                FilletR,
                0,
                0);
            var croSecTrapezoid = new CroSec_Trapezoid(
                "test",
                "testTrapezoid",
                null,
                null,
                null,
                _height,
                _lf_width,
                _uf_width);

            // # build elements
            var elems = new List<BuilderElement>();
            elems.AddRange(
                k3d.Part.LineToBeam(new List<Line3> { line }, null, new List<CroSec> { croSecI }, logger, out _));
            elems.AddRange(
                k3d.Part.LineToBeam(new List<Line3> { line }, null, new List<CroSec> { croSecBox }, logger, out _));
            elems.AddRange(
                k3d.Part.LineToBeam(
                    new List<Line3> { line },
                    null,
                    new List<CroSec> { croSecCircle },
                    logger,
                    out _));
            elems.AddRange(
                k3d.Part.LineToBeam(new List<Line3> { line }, null, new List<CroSec> { croSecT }, logger, out _));
            elems.AddRange(
                k3d.Part.LineToBeam(
                    new List<Line3> { line },
                    null,
                    new List<CroSec> { croSecTrapezoid },
                    logger,
                    out _));

            // # assemble model
            var points = new List<Point3>() { p0, p1 };
            var supports = new List<Support> { k3d.Support.Support(0, k3d.Support.SupportFixedConditions) };

            var loads = new List<Load> { k3d.Load.PointLoad(points[1], new Vector3(0, 0, -1)), };

            var model = k3d.Model.AssembleModel(
                elems,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _,
                new List<Joint>(),
                points);

            // # expected Area
            var areaI = cm.toBase(2 * _height + 2 * _uf_width + 2 * _lf_width + 4 * _fillet_l - 8 * FilletR) * l;
            var areaBox = cm.toBase(2 * _height + _uf_width + _lf_width + 4 * _fillet_l - 8 * FilletR) * l;
            var areaCircle = cm.toBase(Math.PI * _height) * l;
            var areaT = cm.toBase(2 * _height + 2 * _uf_width + 2 * _fillet_l - 4 * FilletR) * l;
            var diff = Math.Abs(_uf_width - _lf_width);
            var areaTrapezoid =
                cm.toBase(_uf_width + _lf_width + 2 * Math.Sqrt(Math.Pow(_height, 2) + Math.Pow(diff, 2))) * l;

            Assert.That(model.elems[0].SurfaceArea(model.nodes), Is.EqualTo(areaI).Within(1E-3));
            Assert.That(model.elems[1].SurfaceArea(model.nodes), Is.EqualTo(areaBox).Within(1E-3));
            Assert.That(model.elems[2].SurfaceArea(model.nodes), Is.EqualTo(areaCircle).Within(1E-3));
            Assert.That(model.elems[3].SurfaceArea(model.nodes), Is.EqualTo(areaT).Within(1E-3));
            Assert.That(model.elems[4].SurfaceArea(model.nodes), Is.EqualTo(areaTrapezoid).Within(1E-3));
        }

        // */

        // /*
        [Test]
        public void CroSec_Volume()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            var ucf = UnitsConversionFactory.Conv();
            ucf.cm();

            var l = 10;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(l, 0, 0);
            var line = new Line3(p0, p1);

            var croSecI = new CroSec_I(
                "test",
                "testI",
                null,
                null,
                null,
                _height,
                _uf_width,
                _lf_width,
                _uf_thick,
                _lf_thick,
                _w_thick,
                FilletR,
                0,
                0);
            var croSecBox = new CroSec_Box(
                "test",
                "testBox",
                null,
                null,
                null,
                _height,
                _uf_width,
                _lf_width,
                _uf_thick,
                _lf_thick,
                _w_thick,
                FilletR,
                0);
            var croSecCircle = new CroSec_Circle("test", "testCirc", null, null, null, _height, _w_thick);
            var croSecT = new CroSec_T(
                "test",
                "testT",
                null,
                null,
                null,
                _height,
                _uf_width,
                _uf_thick,
                _w_thick,
                FilletR,
                0,
                0);
            var croSecTrapezoid = new CroSec_Trapezoid(
                "test",
                "testTrapezoid",
                null,
                null,
                null,
                _height,
                _lf_width,
                _uf_width);

            // # build elements
            var elems = new List<BuilderElement>();
            elems.AddRange(
                k3d.Part.LineToBeam(new List<Line3> { line }, null, new List<CroSec> { croSecI }, logger, out _));
            elems.AddRange(
                k3d.Part.LineToBeam(new List<Line3> { line }, null, new List<CroSec> { croSecBox }, logger, out _));
            elems.AddRange(
                k3d.Part.LineToBeam(
                    new List<Line3> { line },
                    null,
                    new List<CroSec> { croSecCircle },
                    logger,
                    out _));
            elems.AddRange(
                k3d.Part.LineToBeam(new List<Line3> { line }, null, new List<CroSec> { croSecT }, logger, out _));
            elems.AddRange(
                k3d.Part.LineToBeam(
                    new List<Line3> { line },
                    null,
                    new List<CroSec> { croSecTrapezoid },
                    logger,
                    out _));

            // # assemble model
            var points = new List<Point3>() { p0, p1 };
            var supports = new List<Support> { k3d.Support.Support(0, k3d.Support.SupportFixedConditions) };

            var loads = new List<Load> { k3d.Load.PointLoad(points[1], new Vector3(0, 0, -1)), };

            var model = k3d.Model.AssembleModel(
                elems,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _,
                new List<Joint>(),
                points);

            // # expected Area
            var volumeI = ((CroSec_Beam)model.elems[0].crosec).A * l;
            var volumeBox = ((CroSec_Beam)model.elems[1].crosec).A * l;
            var volumeCircle = ((CroSec_Beam)model.elems[2].crosec).A * l;
            var volumeT = ((CroSec_Beam)model.elems[3].crosec).A * l;
            var volumeTrapezoid = ((CroSec_Beam)model.elems[4].crosec).A * l;

            Assert.That(model.elems[0].Volume(model.nodes), Is.EqualTo(volumeI).Within(1E-3));
            Assert.That(model.elems[1].Volume(model.nodes), Is.EqualTo(volumeBox).Within(1E-3));
            Assert.That(model.elems[2].Volume(model.nodes), Is.EqualTo(volumeCircle).Within(1E-3));
            Assert.That(model.elems[3].Volume(model.nodes), Is.EqualTo(volumeT).Within(1E-3));
            Assert.That(model.elems[4].Volume(model.nodes), Is.EqualTo(volumeTrapezoid).Within(1E-3));
        }

        // */
    }
}

#endif