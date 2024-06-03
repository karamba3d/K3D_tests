#if ALL_TESTS

namespace KarambaCommon.Tests.Loads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using feb;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Elements.States;
    using Karamba.Elements.States.Selectors;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Results;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Load = Karamba.Loads.Load;

    [TestFixture]
    public class WallLoadCaseCombinationResults_tests
    {
        [Test]
        public void WallLoadCaseCombinationresults()
        {
            Toolkit k3d = new Toolkit();

            MessageLogger logger = new MessageLogger();

            const double l = 1.0;
            List<Point3> points = new List<Point3> { new Point3(0, l, 0), new Point3(0, 0, 0), new Point3(l, 0, 0), new Point3(l, l, 0) };

            Mesh3 mesh = new Mesh3();
            foreach (Point3 point in points)
            {
                mesh.AddVertex(point);
            }

            mesh.AddFace(new Face3(0, 1, 3));
            mesh.AddFace(new Face3(1, 2, 3));

            Karamba.Materials.FemMaterial material = k3d.Material.IsotropicMaterial(string.Empty, string.Empty, 200000, 100000, 100000, 0, 0, 0, 0, 0);
            double height = 0.01;
            CroSec_Shell crosec = k3d.CroSec.ShellConst(height, 0, material);
            List<BuilderShell> shells = k3d.Part.MeshToShell(new List<Mesh3> { mesh }, null, new List<CroSec> { crosec }, logger, out List<Point3> nodes);

            // create supports
            List<bool> supportConditions_1 = new List<bool>() { true, true, true, true, true, true };
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(points[0], supportConditions_1),
                k3d.Support.Support(points[1], supportConditions_1),
            };

            // create a Point-load
            double fA = 100;
            double fB = 50;
            List<Load> loads = new List<Load>
            {
                k3d.Load.PointLoad(points[2], new Vector3(fA, 0, 0), new Vector3(), "A"),
                k3d.Load.PointLoad(points[3], new Vector3(fA, 0, 0), new Vector3(), "A"),
                k3d.Load.PointLoad(points[2], new Vector3(fB, 0, 0), new Vector3(), "B"),
                k3d.Load.PointLoad(points[3], new Vector3(fB, 0, 0), new Vector3(), "B"),
                new BuilderLoadCaseCombination(new List<string>() { "C = (A|B)", }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            Analyze.solve(
                model,
                new List<string>() { "C" },
                out IReadOnlyList<double> maxDisp,
                out IReadOnlyList<Vector3> force,
                out IReadOnlyList<double> energy,
                out var outModel,
                out var warningMessage);

            // Support reactions
            //---
            Reaction.solve(
                outModel,
                "C",
                new List<int> { 0, 1 },
                out var orientations,
                out var forces_all,
                out var moments_all,
                out var sum_forces_all,
                out var sum_moments_all,
                out var governingLoadCases,
                out var governingLoadCaseInds);

            Assert.That(forces_all[0][0].X, Is.EqualTo(-fA).Within(1e-8));
            Assert.That(forces_all[0][1].X, Is.EqualTo(-fB).Within(1e-8));
            var loadCase = governingLoadCases[0][0].ToString();
            Assert.That(loadCase == "A");
            loadCase = governingLoadCases[0][1].ToString();
            Assert.That(loadCase == "B");

            var selectorSet = StateSelectorFactory.Instance().Selectors("C", model);
            var resultSelectors = selectorSet.StateElement2DSelector;
            var stateNodal = resultSelectors.GetStateNodal(outModel.elems[0] as ModelMembrane, outModel, out _, ValueDistributionType.average);
            var results = stateNodal.GetResultsMinMaxAll(new StateElement2DResultsPosition(Element2DResultOption.Sig1)).ToList();

            var sig1CalcA = results[0].ToList()[0];
            var sig1TargA = 2 * fA / height / l;
            Assert.That(sig1CalcA, Is.EqualTo(sig1TargA).Within(1e-8));
            var sig1CalcB = results[0].ToList()[1];
            var sig1TargB = 2 * fB / height / l;
            Assert.That(sig1CalcB, Is.EqualTo(sig1TargB).Within(1e-8));

            ShellForcesPrincipal.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "C",
                out var n1,
                out var n2,
                out var v1,
                out var v2,
                out var m1,
                out var m2,
                out _);

            var n1TargA = 2 * fA / l;
            Assert.That(n1[0][0][0], Is.EqualTo(n1TargA).Within(1e-8));
            var n1TargB = 2 * fB / l;
            Assert.That(n1[0][0][1], Is.EqualTo(n1TargB).Within(1e-8));

            ShellForcesLocal.Solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "C",
                out var nxx,
                out var nyy,
                out var nxy,
                out var mxx,
                out var myy,
                out var mxy,
                out var vx,
                out var vy,
                out _);

            var nxxTargA = 2 * fA / l;
            Assert.That(nxx[0][0][0], Is.EqualTo(nxxTargA).Within(1e-8));
            var nxxTargB = 2 * fB / l;
            Assert.That(nxx[0][0][1], Is.EqualTo(nxxTargB).Within(1e-8));
        }

        [Test]
        public void Wall()
        {
            Toolkit k3d = new Toolkit();

            MessageLogger logger = new MessageLogger();

            const double l = 1.0;
            List<Point3> points = new List<Point3> { new Point3(0, l, 0), new Point3(0, 0, 0), new Point3(l, 0, 0), new Point3(l, l, 0) };

            Mesh3 mesh = new Mesh3();
            foreach (Point3 point in points)
            {
                mesh.AddVertex(point);
            }

            mesh.AddFace(new Face3(0, 1, 3));
            mesh.AddFace(new Face3(1, 2, 3));

            Karamba.Materials.FemMaterial material = k3d.Material.IsotropicMaterial(string.Empty, string.Empty, 200000, 100000, 100000, 0, 0, 0, 0, 0);
            double height = 0.01;
            CroSec_Shell crosec = k3d.CroSec.ShellConst(height, 0, material);
            List<BuilderShell> shells = k3d.Part.MeshToShell(new List<Mesh3> { mesh }, null, new List<CroSec> { crosec }, logger, out List<Point3> nodes);

            // create supports
            List<bool> supportConditions_1 = new List<bool>() { true, true, true, true, true, true };
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(points[0], supportConditions_1),
                k3d.Support.Support(points[1], supportConditions_1),
            };

            // create a Point-load
            double fA = 100;
            double fB = -50;
            List<Load> loads = new List<Load>
            {
                k3d.Load.PointLoad(points[2], new Vector3(fA, 0, 0), new Vector3(), "A"),
                k3d.Load.PointLoad(points[3], new Vector3(fA, 0, 0), new Vector3(), "A"),
                k3d.Load.PointLoad(points[2], new Vector3(fB, 0, 0), new Vector3(), "B"),
                k3d.Load.PointLoad(points[3], new Vector3(fB, 0, 0), new Vector3(), "B"),
                new BuilderLoadCaseCombination(new List<string>() { "C = (A|B)", }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            Analyze.solve(
                model,
                new List<string>() { "C" },
                out IReadOnlyList<double> maxDisp,
                out IReadOnlyList<Vector3> force,
                out IReadOnlyList<double> energy,
                out var outModel,
                out var warningMessage);

            // Support reactions
            //---
            Reaction.solve(
                outModel,
                "C/S-Fx/min",
                new List<int> { 0, 1 },
                out var orientations,
                out var forces_all,
                out var moments_all,
                out var sum_forces_all,
                out var sum_moments_all,
                out var governingLoadCases,
                out var governingLoadCaseInds);

            var rxMin0 = forces_all[0][0].X;
            Assert.That(rxMin0, Is.EqualTo(-fA).Within(1e-8));
            var rxMin1 = forces_all[1][0].X;
            Assert.That(rxMin1, Is.EqualTo(-fA).Within(1e-8));
            var loadCase = governingLoadCases[0][0].ToString();
            Assert.That(loadCase == "A");

            var selectorSet = StateSelectorFactory.Instance().Selectors("C/Sig1/max", model);
            var resultSelectors = selectorSet.StateElement2DSelector;
            var stateNodal = resultSelectors.GetStateNodal(outModel.elems[0] as ModelMembrane, outModel, out _, ValueDistributionType.average);
            var results = stateNodal.GetResultsMinMax(new StateElement2DResultsPosition(Element2DResultOption.Sig1)).ToList();

            var sig1Calc = results[0];
            var sig1Targ = 2 * fA / height / l;
            Assert.That(sig1Calc, Is.EqualTo(sig1Targ).Within(1e-8));

            ShellForcesPrincipal.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "C/Sig2/min",
                out var n1,
                out var n2,
                out var v1,
                out var v2,
                out var m1,
                out var m2,
                out _);

            var n2Calc = n2[0][0][0];
            var n2Targ = 2 * fB / l;
            Assert.That(n2Calc, Is.EqualTo(n2Targ).Within(1e-8));

            ShellForcesLocal.Solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "C/Sig1/max",
                out List<List<List<double>>> nxx,
                out List<List<List<double>>> nyy,
                out List<List<List<double>>> nxy,
                out List<List<List<double>>> mxx,
                out List<List<List<double>>> myy,
                out List<List<List<double>>> mxy,
                out List<List<List<double>>> vx,
                out List<List<List<double>>> vy,
                out _);

            var nxxCalc = nxx[0][0][0];
            var nxxTarg = 2 * fA / l;
            Assert.That(nxxCalc, Is.EqualTo(nxxTarg).Within(1e-8));

            ShellForcesLocal.Solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "C/max",
                out nxx,
                out nyy,
                out nxy,
                out mxx,
                out myy,
                out mxy,
                out vx,
                out vy,
                out _);

            nxxCalc = nxx[0][0][0];
            nxxTarg = 2 * fA / l;
            Assert.That(nxxCalc, Is.EqualTo(nxxTarg).Within(1e-8));
        }
    }
}

#endif
