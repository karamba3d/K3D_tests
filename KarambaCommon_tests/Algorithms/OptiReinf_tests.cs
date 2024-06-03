#if ALL_TESTS
namespace KarambaCommon.Tests.Algorithms
{
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Combination;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using KarambaCommon.Tests.Utilities;
    using Karamba.Models;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;
    using Helper = KarambaCommon.Tests.Helpers.Helper;

#pragma warning disable CS1591 // don't enforce documentation in test files.

#pragma warning disable CS1591 // don't enforce documentation in test files.

    [TestFixture]
    public class OptiReinf_tests
    {
        [Test]
        public void ReinforcedPlate()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();

            double length = 1.0;
            var mesh = new Mesh3();
            mesh.AddVertex(new Point3(0, length, 0));
            mesh.AddVertex(new Point3(0, 0, 0));
            mesh.AddVertex(new Point3(length, 0, 0));
            mesh.AddVertex(new Point3(length, length, 0));
            mesh.AddFace(new Face3(0, 1, 3));
            mesh.AddFace(new Face3(1, 2, 3));

            CroSec_Shell crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(
                25,
                0,
                null,
                new List<double> { 4, 4, -4, -4 },
                0);
            List<BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out List<Point3> nodes);

            // create supports
            var supportConditions = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(0, supportConditions), k3d.Support.Support(1, supportConditions),
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
                out string info,
                out double mass,
                out Point3 cog,
                out string message,
                out bool warning);

            // activate all load-cases
            var lc_combinations = model.lcActivation = new LoadCaseActivation(model.lcCombinationCollection.OrderedLoadCaseCombinations.ToList());

            model.buildFEModel();
            ModelBuilderFEB.AddLoadsAndSupports(model, model.lcActivation);

            model = k3d.Algorithms.OptiReinf(
                model,
                out IReadOnlyList<double> maxDisplacements,
                out IReadOnlyList<double> compliances,
                out message,
                out double reinfMass);

            k3d.Results.ShellForcesLocal(
                model,
                null,
                "0",
                out List<List<List<double>>> nxx,
                out List<List<List<double>>> nyy,
                out List<List<List<double>>> nxy,
                out List<List<List<double>>> mxx,
                out List<List<List<double>>> myy,
                out List<List<List<double>>> mxy,
                out List<List<List<double>>> vx,
                out List<List<List<double>>> vy,
                out _);
            Assert.That(mxx[0][0][0], Is.EqualTo(50.082245640312429).Within(1E-5));
            Assert.That(mxx[0][1][0], Is.EqualTo(49.91775435968767).Within(1E-5));

            CroSec_Shell crosec_shell = model.elems[0].crosec as CroSec_Shell;
            double reinf_thick = crosec_shell.elem_crosecs[0].layers[1].height;
            Assert.That(reinf_thick, Is.EqualTo(0.00046824411288599481).Within(1E-5));
        }
    }
}
#endif
