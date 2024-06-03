#if ALL_TESTS

namespace KarambaCommon.Tests.Elements
{
    using System;
    using System.Collections.Generic;
    using Karamba.CrossSections;
    // using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.ExceptUtil;

    [TestFixture]
    public class Shell_tests
    {
        [Test]
        public void CreateShellPatch()
        {
            // Create Mesh
            var mesh = new Mesh3();
            foreach (var v1 in new[] { 0, 1, 2 })
                foreach (var v2 in new[] { 0, 1, 2 })
                   _ = mesh.AddVertex(v1, v2, 0);

            RepeatCall(mesh.AddFace, (0, 3, 4), (1, 4, 5), (3, 6, 7), (4, 7, 8),
                                     (0, 4, 1), (1, 5, 2), (3, 7, 4), (4, 8, 5));
            /*mesh.AddFace(0, 3, 4);
            mesh.AddFace(1, 4, 5);
            mesh.AddFace(3, 6, 7);
            mesh.AddFace(4, 7, 8);
            mesh.AddFace(0, 4, 1);
            mesh.AddFace(1, 5, 2);
            mesh.AddFace(3, 7, 4);
            mesh.AddFace(4, 8, 5);
            */
            // Create Model
            var k3d = new Toolkit();
            var supportConditions = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(new Point3(0, 0, 0), supportConditions),
                k3d.Support.Support(new Point3(0, 1, 0), supportConditions),
                k3d.Support.Support(new Point3(0, 2, 0), supportConditions),
            };

            var loads = new List<Load>
            {
                k3d.Load.PointLoad(new Point3(2, 2, 0), new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(new Point3(2, 1, 0), new Vector3(0, 0, -5)),
                k3d.Load.PointLoad(new Point3(2, 0, 0), new Vector3(0, 0, -5)),
            };

            var logger = new MessageLogger();
            CroSec_Shell crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(
                25,
                0,
                null,
                new List<double> { 4, 4, -4, -4 });
            List<Karamba.Elements.BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out _);
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _);
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out _);

            // Create projection Vector
            var projectionVector = new Vector3(0, 0, -1);

            // Create other definition
            double tol = 1E-12;
            double delta = 0.02;
        }
    }
}

#endif
