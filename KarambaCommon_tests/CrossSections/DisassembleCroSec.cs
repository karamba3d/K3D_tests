#if ALL_TESTS

namespace KarambaCommon.Tests.CrossSections
{
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Combination;
    using Karamba.Supports;
    using Karamba.Utilities;
    using Karamba.Models;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture]
    public class Disassemble_CroSec_tests
    {
        [Test]
        public void DisassembleShellCroSec()
        {
            var k3d = new Toolkit();
            var crosecs = new List<CroSec_Shell>() { k3d.CroSec.ShellConst(1.0), k3d.CroSec.ShellConst(2.0), };

            List<double> heights = crosecs[0].heights();
            Assert.That(heights, Has.Count.EqualTo(1).Within(1E-5));

            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 1);
            var p2 = new Point3(1, 1, 1);
            var p3 = new Point3(0, 1, 0);
            Mesh3 mesh = new Mesh3(
                new List<Point3>() { p0, p1, p2, p3 },
                new List<Face3>() { new Face3(0, 1, 2), new Face3(0, 2, 3) });
            List<Karamba.Elements.BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3>() { mesh },
                new List<string>() { "s1" },
                new List<CroSec>() { crosecs[0] },
                logger,
                out _);

            var supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportHingedConditions),
                k3d.Support.Support(1, k3d.Support.SupportHingedConditions),
            };

            // create a Point-load
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(2, new Vector3(), new Vector3(0, 25, 0)),
                k3d.Load.PointLoad(3, new Vector3(), new Vector3(0, 25, 0)),
            };

            // create the model
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _);

            // activate all load-cases
            model.lcActivation = new LoadCaseActivation(model.lcCombinationCollection.OrderedLoadCaseCombinations.ToList());

            // optimize cross sections for activated load-cases
            Model model_opti = k3d.Algorithms.OptiCroSec(
                model,
                crosecs,
                out _,
                out _,
                out _);

            heights = model_opti.elems[0].crosec.heights();
            Assert.That(heights, Has.Count.EqualTo(2).Within(1E-5));
        }
    }
}

#endif
