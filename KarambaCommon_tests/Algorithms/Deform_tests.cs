#if ALL_TESTS

namespace KarambaCommon.Tests.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class Deform_tests
    {
        [Test]
        public void Cantilever()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            // get a cross section from the cross section table in the folder 'Resources'
            var crosec = new CroSec_Trapezoid(
                "family",
                "name",
                "country",
                null,
                Material_Default.Instance().steel,
                20,
                10,
                10);
            crosec.Az = 1E10; // make it stiff in shear

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out var out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-load
            const int fz = 10;
            var loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(0, 0, -fz)), };
        }
    }
}

#endif