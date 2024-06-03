#if ALL_TESTS

namespace KarambaCommon.Tests.Loads
{
    using System.Collections.Generic;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture]
    public class SelectiveCalculation_tests
    {
        [Test]
        public void CalculateOneOfTwoLoadCases()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            double length = 4.0;
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(length, 0, 0);
            Line3 axis = new Line3(p0, p1);

            // get a cross section from the cross section table in the folder 'Resources'
            CroSec_Trapezoid crosec = new CroSec_Trapezoid(
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
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create two Point-loads in load-case-combinations 'LCA' and 'LCB'
            int f = 10;
            List<Load> loads = new List<Load>
            {
                k3d.Load.PointLoad(p1, new Vector3(10 * f, 0, 0), new Vector3(), "A"),
                k3d.Load.PointLoad(p1, new Vector3(f, 0, 0), new Vector3(), "B"),
                new BuilderLoadCaseCombination(new List<string>() { "C = (A|B)", }),
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
                new List<string>() { "B" },
                out IReadOnlyList<double> maxDisp,
                out IReadOnlyList<Vector3> force,
                out IReadOnlyList<double> energy,
                out var warningMessage,
                out var outModel);

            var maxDispTarget = f / crosec.A / crosec.material.E(0) * length;
            Assert.That(maxDisp[0], Is.EqualTo(maxDispTarget).Within(1e-8));
        }
    }
}

#endif
