#if ALL_TESTS

namespace KarambaCommon.Tests.Loads
{
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Combination;
    using Karamba.Materials;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture]
    public class LoadCaseOptions_tests
    {
        [Test]
        public void TwoLoadCases_AttachOptions_via_Name()
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
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create two Point-loads in load-case-combinations 'LCA' and 'LCB'
            int f = 10;
            List<Load> loads = new List<Load>
            {
                k3d.Load.PointLoad(p1, new Vector3(0, 0, -f), new Vector3(), "LCA"),
                k3d.Load.PointLoad(p1, new Vector3(f, 0, 0), new Vector3(), "LCB"),
                k3d.Load.LoadCaseOptions("LCA", false),
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

            var lca = model.lcActivation.LoadCaseCombinations.First(i => i.Name == "LCA");
            var lcb = model.lcActivation.LoadCaseCombinations.First(i => i.Name == "LCB");
            Assert.That(((LoadCaseCombinationOptionsThI)lca.Options).InitialNII == false);
            Assert.That(((LoadCaseCombinationOptionsThI)lcb.Options).InitialNII);
        }

        [Test]
        public void TwoLoadCases_AttachOptions_via_RegularExpression()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            double length = 4.0;
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(0, 0, length);
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
                k3d.Load.PointLoad(p1, new Vector3(0, 0, -f), new Vector3(), "LCA"),
                k3d.Load.PointLoad(p1, new Vector3(f, 0, 0), new Vector3(), "LCB"),
                k3d.Load.LoadCaseOptions("&LC.", false),
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

            var lca = model.lcActivation.LoadCaseCombinations.First(i => i.Name == "LCA");
            var lcb = model.lcActivation.LoadCaseCombinations.First(i => i.Name == "LCB");
            Assert.That(((LoadCaseCombinationOptionsThI)lca.Options).InitialNII == false);
            Assert.That(((LoadCaseCombinationOptionsThI)lcb.Options).InitialNII == false);
        }
    }
}

#endif
