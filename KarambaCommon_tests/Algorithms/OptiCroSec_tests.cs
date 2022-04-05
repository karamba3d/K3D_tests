namespace KarambaCommon.Tests.Algorithms
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class OptiCroSec_tests
    {
#if ALL_TESTS
        [Test]
        public void OptiCroSecPlateInPlane()
        {
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

            double fc = -10; // kN/m²
            var concrete = k3d.Material.IsotropicMaterial(
                "concrete",
                "concrete",
                21000,
                8900,
                8900,
                0,
                -fc,
                fc,
                FemMaterial.FlowHypothesis.mises,
                0);

            // set up family of cross sections
            List<CroSec> croSecFamily = new List<CroSec>();
            for (int t = 1; t < 200; t += 5)
            {
                croSecFamily.Add(k3d.CroSec.ReinforcedConcreteStandardShellConst(
                    t,
                    0,
                    null,
                    new List<double> { 4, 4, -4, -4 },
                    0,
                    concrete,
                    null,
                    "Family",
                    "Name" + t));
            }

            var crosec = croSecFamily[0];
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out var nodes);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(0, new List<bool>() { true, false, true, true, true, true }), 
                k3d.Support.Support(1, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-load
            double force = -0.4; // kN
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(2,  new Vector3(0, force, 0)),
                k3d.Load.PointLoad(3,  new Vector3(0, force, 0)),
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

            double targetUtil = 0.2;
            model = k3d.Algorithms.OptiCroSec(
                model,
                croSecFamily,
                out var maxDisplacements,
                out var compliances,
                out message,
                5,
                targetUtil);

            var crosec_shell = model.elems[0].crosec as CroSec_Shell;
            var height = crosec_shell.elem_crosecs[0].layers[0].height;
            var heightTarg = 2 * force / length / fc / targetUtil;
            Assert.That(height, Is.EqualTo(1.16).Within(1E-5));

            model = k3d.Algorithms.OptiReinf(
                model,
                out maxDisplacements,
                out compliances,
                out message,
                out var reinfMass);

            crosec_shell = model.elems[0].crosec as CroSec_Shell;
            var reinf_thick = crosec_shell.elem_crosecs[0].layers[1].height;
            Assert.That(reinf_thick, Is.EqualTo(1.84E-06).Within(1E-5));
        }

        [Test]
        public void OptiCroSecPlateBending()
        {
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

            double fc = -10; // kN/m²
            var concrete = k3d.Material.IsotropicMaterial(
                "concrete",
                "concrete",
                21000,
                8900,
                8900,
                0,
                -fc,
                fc,
                FemMaterial.FlowHypothesis.mises,
                0);

            // set up family of cross sections
            List<CroSec> croSecFamily = new List<CroSec>();
            for (int t = 1; t < 200; t += 5)
            {
                croSecFamily.Add(k3d.CroSec.ReinforcedConcreteStandardShellConst(
                    t,
                    0,
                    null,
                    new List<double> { 4, 4, -4, -4 },
                    0,
                    concrete,
                    null,
                    "Family",
                    "Name" + t));
            }

            var crosec = croSecFamily[0];
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out var nodes);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(0, new List<bool>() { true, false, true, true, true, true }),
                k3d.Support.Support(1, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-load
            double force = -0.4; // kN
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(2,  new Vector3(0, 0, force)),
                k3d.Load.PointLoad(3,  new Vector3(0, 0, force)),
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

            double targetUtil = 0.2;
            model = k3d.Algorithms.OptiCroSec(
                model,
                croSecFamily,
                out var maxDisplacements,
                out var compliances,
                out message,
                5,
                targetUtil);

            var crosec_shell = model.elems[0].crosec as CroSec_Shell;
            var height = crosec_shell.elem_crosecs[0].layers[0].height;
            var heightTarg = 2 * force / length / fc / targetUtil;
            Assert.That(height, Is.EqualTo(1.21).Within(1E-5));

            model = k3d.Algorithms.OptiReinf(
                model,
                out maxDisplacements,
                out compliances,
                out message,
                out var reinfMass);

            crosec_shell = model.elems[0].crosec as CroSec_Shell;
            var reinf_thick = crosec_shell.elem_crosecs[0].layers[1].height;

            // ~ 2 / 1.21 / 43.5 * 2 (result at cantilever mid)
            Assert.That(reinf_thick, Is.EqualTo(2.1828168088280552E-06).Within(1E-5));
        }
#endif
    }
}