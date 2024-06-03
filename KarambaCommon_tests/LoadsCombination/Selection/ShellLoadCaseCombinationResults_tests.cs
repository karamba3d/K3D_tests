#if ALL_TESTS

namespace KarambaCommon.Tests.Loads
{
    using System;
    using System.Collections.Generic;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Results;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Load = Karamba.Loads.Load;

    [TestFixture]
    public class ShellLoadCaseCombinationResults_tests
    {
        [Test]
        public void SingleElement_PrincipalCrossSectionForces()
        {
            var k3d = new Toolkit();

            var logger = new MessageLogger();

            const double l = 5.0;
            var points = new List<Point3> { new Point3(0, -0.5 * l, 0), new Point3(0, 0.5 * l, 0), new Point3(l, 0, 0) };

            var mesh = new Mesh3();
            foreach (Point3 point in points)
                _ = mesh.AddVertex(point);

            _ = mesh.AddFace(new Face3(0, 1, 2));

            Karamba.Materials.FemMaterial material = k3d.Material.IsotropicMaterial(string.Empty, string.Empty, 200000, 100000, 100000, 0, 0, 0, 0, 0);
            double height = 0.01;
            CroSec_Shell crosec = k3d.CroSec.ShellConst(height, 0, material);
            List<BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out List<Point3> _); // nodes

            // create supports
            var supportConditions_1 = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(points[0], supportConditions_1),
                k3d.Support.Support(points[1], supportConditions_1),
            };

            // create a Point-load
            double f0 = -1.5;
            double f1 = -1.0;
            double f2 = 2.0;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(points[2], new Vector3(f0, 0, 0), new Vector3(), "LC0"),
                k3d.Load.PointLoad(points[2], new Vector3(f1, 0, 0), new Vector3(), "LC1"),
                k3d.Load.PointLoad(points[2], new Vector3(f2, 0, 0), new Vector3(), "LC2"),
                new BuilderLoadCaseCombination(new List<string>() { "LCC = (LC0|LC1|LC2)", }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out var _, // info
                out var _, // mass
                out var _, // cog
                out var _, // message
                out var _); // warning

            Analyze.solve(
                model,
                new List<string>() { "LCC" },
                out IReadOnlyList<double> _,  // maxDisp
                out IReadOnlyList<Vector3> _, // force
                out IReadOnlyList<double> _,  // energy
                out var outModel,
                out string _);                // warningMessage

            ShellForcesPrincipal.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/min",
                out var _, // n1
                out var _, // n2
                out var _, // v1
                out var _, // v2
                out var _, // m1
                out var _, // m2
                out var lcInds);

            Assert.That(lcInds[0][0][0], Is.EqualTo(-1));

            ShellForcesPrincipal.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC",
                out var _, // n1
                out var _, // n2
                out var _, // v1
                out var _, // v2
                out var _, // m1
                out var _, // m2
                out lcInds);

            Assert.That(lcInds[0][0][0], Is.EqualTo(0));
            Assert.That(lcInds[0][0][1], Is.EqualTo(1));
            Assert.That(lcInds[0][0][2], Is.EqualTo(2));
            // XXX use collection comparing
        }
    }
}

#endif
