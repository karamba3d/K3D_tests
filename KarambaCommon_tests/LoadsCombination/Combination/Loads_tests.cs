#if ALL_TESTS
namespace KarambaCommon.Tests.Loads
{
    using System;
    using System.Collections.Generic;
    using feb;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Beam;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using KarambaCommon.Tests.Helpers;
    using KarambaCommon.Tests.Loads;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Load = Karamba.Loads.Load;

    [TestFixture]
    public class Loads_tests
    {
        private LoadTypes _loadType;

        [Test]
        public void PrescribedDisplacementCombination()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(0, 0, length);
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
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out List<Point3> out_points);

            // create supports
            var supports = new List<Support>
            {
                new Support(
                    p0,
                    new List<bool>() { true, true, true, true, true, true },
                    Plane3.WorldXY,
                    new Vector3(1, 0, 0),
                    new Vector3(),
                    "LC0"),
            };

            var loads = new List<Load>
            {
                new BuilderLoadCaseCombination(new List<string>() { "LCC = 2*LC0", }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            Analyze.solve(
                model,
                new List<string>() { "LC0", "LCC" },
                out IReadOnlyList<double> maxDisp,
                out IReadOnlyList<Vector3> force,
                out IReadOnlyList<double> energy,
                out _, // outModel
                out _); // warningMessage

            // Assert.That(maxDisp, Has.Count.EqualTo(2));
            // Assert.That(maxDisp[0], Is.EqualTo(1.0));
            // Assert.That(maxDisp[1], Is.EqualTo(2.0));
            Assert.That(maxDisp, Is.EquivalentTo(new[] { 1.0, 2.0 }));
        }
    }
}
#endif
