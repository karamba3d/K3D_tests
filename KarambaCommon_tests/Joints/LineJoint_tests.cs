#if ALL_TESTS

namespace KarambaCommon.Tests.Joints
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class LineJointTests
    {
        [Test]
        public void Membrane_Triangle_InPlaneSpringOnOneSide()
        {
            var k3d = new Toolkit();

            var logger = new MessageLogger();

            const double l = 1.0;
            var points = new List<Point3> { new Point3(0, 0.5 * l, 0), new Point3(0, -0.5 * l, 0), new Point3(l, 0, 0) };

            var mesh = new Mesh3();
            foreach (var point in points)
            {
                mesh.AddVertex(point);
            }

            mesh.AddFace(new Face3(0, 1, 2));

            var material = k3d.Material.IsotropicMaterial(string.Empty, string.Empty, 200000, 100000, 100000, 0, 0, 0, 0, 0);
            var crosec = k3d.CroSec.ShellConst(0.01, 0, material);
            var shells = k3d.Part.MeshToShell(new List<Mesh3> { mesh }, null, new List<CroSec> { crosec }, false, logger, out var out_nodes);

            // create supports
            var supportConditions_1 = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(points[0], supportConditions_1),
                k3d.Support.Support(points[1], supportConditions_1),
            };

            // create a Point-load
            double fx = 100.0;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(points[2], new Vector3(fx, 0, 0), new Vector3()),
            };

            double cx = 2.0;
            var joints = new List<Joint>()
            {
                new JointLine(
                    new List<string> { string.Empty },
                    new PolyLine3(new List<Point3> { points[1], points[0] }),
                    Vector3.XAxis,
                    Vector3.ZAxis,
                    Math.PI * 0.5,
                    new double?[] { null, cx }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(shells, supports, loads, out var info, out var mass, out var cog, out var message, out var warning, joints);

            AnalyzeThI.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning_msg, out model);

            var h = (model.elems[0].crosec as CroSec_Shell).getHeight();
            var e = model.elems[0].crosec.material.E();
            var c_inv_tot = 2 * l / (e * h) + 1 / cx / l;
            var dispTarget = Math.Abs(fx) / l * c_inv_tot;
            var dispCalc = outMaxDisp[0];
            Assert.That(dispTarget, Is.EqualTo(dispCalc).Within(1));
        }

        [Test]
        public void Plate_Quad_InPlaneSpringOnOneSide()
        {
            var k3d = new Toolkit();

            var logger = new MessageLogger();

            const double l = 1.0;
            var points = new List<Point3> { new Point3(0, l, 0), new Point3(0, 0, 0), new Point3(l, 0, 0), new Point3(l, l, 0) };

            var mesh = new Mesh3();
            foreach (var point in points)
            {
                mesh.AddVertex(point);
            }

            mesh.AddFace(new Face3(0, 1, 3));
            mesh.AddFace(new Face3(1, 2, 3));

            var material = k3d.Material.IsotropicMaterial(string.Empty, string.Empty, 200000, 100000, 100000, 0, 0, 0, 0, 0);
            var crosec = k3d.CroSec.ShellConst(0.01, 0, material);
            var shells = k3d.Part.MeshToShell(new List<Mesh3> { mesh }, null, new List<CroSec> { crosec }, logger, out var nodes);

            // create supports
            var supportConditions_1 = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(points[0], supportConditions_1),
                k3d.Support.Support(points[1], supportConditions_1),
            };

            // create a Point-load
            double fx = 100.0;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(points[2], new Vector3(fx, 0, 0), new Vector3()),
                k3d.Load.PointLoad(points[3], new Vector3(fx, 0, 0), new Vector3()),
            };

            double cx = 1e-4;
            var joints = new List<Joint>()
            {
                new JointLine(
                    new List<string> { string.Empty },
                    new PolyLine3(new List<Point3> { new Point3(l, 0, 0), new Point3(l, l, 0) }),
                    -Vector3.XAxis,
                    z_dir: -Vector3.ZAxis,
                    Math.PI * 0.5,
                    new double?[] { null, cx }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out _,
                out var mass,
                out var cog,
                out var message,
                out var warning,
                joints);

            AnalyzeThI.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning_msg, out model);

            var h = (model.elems[0].crosec as CroSec_Shell).getHeight();
            var e = model.elems[0].crosec.material.E();
            var c_inv_tot = l / (e * h) + 1 / cx;
            var dispTarget = 2.0 * Math.Abs(fx) / l * c_inv_tot;
            var dispCalc = outMaxDisp[0];
            Assert.That(dispTarget, Is.EqualTo(dispCalc).Within(1));
        }

        [Test]
        public void Membrane_Quad_InPlaneSpringOnOneSide()
        {
            var k3d = new Toolkit();

            var logger = new MessageLogger();

            const double lx = 1.0;
            const double ly = 2.0;
            var points = new List<Point3> { new Point3(0, ly, 0), new Point3(0, 0, 0), new Point3(lx, 0, 0), new Point3(lx, ly, 0) };

            var mesh = new Mesh3();
            foreach (var point in points)
            {
                mesh.AddVertex(point);
            }

            mesh.AddFace(new Face3(2, 0, 1));
            mesh.AddFace(new Face3(0, 2, 3));

            var material = k3d.Material.IsotropicMaterial(string.Empty, string.Empty, 200000, 100000, 100000, 0, 0, 0, 0, 0);
            var crosec = k3d.CroSec.ShellConst(0.01, 0, material);
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                false,
                logger,
                out var nodes,
                0.005,
                null,
                points);

            // create supports
            var supportConditions_1 = new List<bool>() { true, true, true };
            var supportConditions_2 = new List<bool>() { true, true, true };
            var supportConditions_3 = new List<bool>() { false, false, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(points[0], supportConditions_1),
                k3d.Support.Support(points[1], supportConditions_2),
                k3d.Support.Support(points[2], supportConditions_3),
                k3d.Support.Support(points[3], supportConditions_3),
            };

            // create a Point-load
            const double fx = 100.0;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(points[2], new Vector3(fx, 0, 0), new Vector3()),
                k3d.Load.PointLoad(points[3], new Vector3(fx, 0, 0), new Vector3()),
            };

            double cy = 10;
            var joints = new List<Joint>()
            {
                new JointLine(
                    new List<string> { string.Empty },
                    new PolyLine3(new List<Point3> { points[2], points[3] }),
                    -Vector3.XAxis,
                    -Vector3.ZAxis,
                    Math.PI * 0.5,
                    new double?[] { null, cy }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(shells, supports, loads, out var info, out var mass, out var cog, out var message, out var warning, joints);

            AnalyzeThI.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning_msg, out model);

            var h = (model.elems[0].crosec as CroSec_Shell).getHeight();
            var e = model.elems[0].crosec.material.E();

            // var c_inv_tot = L / (E * h);
            var c_inv_tot = lx / (e * h) + 1 / cy;
            var dispTarget = 2.0 * Math.Abs(fx) / ly * c_inv_tot;
            var dispCalc = outMaxDisp[0];
            Assert.That(dispTarget, Is.EqualTo(dispCalc).Within(1));
        }

        [Test]
        public void TSection_6Elems()
        {
            var k3d = new Toolkit();

            var logger = new MessageLogger();

            const double lx = 1.0;
            const double ly = 2.0;
            const double lz = 3.0;
            var points = new List<Point3>
            {
                new Point3(0, ly, 0), new Point3(0, 0, 0), new Point3(0, -ly, 0),
                new Point3(lx, ly, 0), new Point3(lx, 0, 0), new Point3(lx, -ly, 0),
                new Point3(0, 0, lz), new Point3(lx, 0, lz),
            };

            var mesh = new Mesh3();
            foreach (var point in points)
            {
                mesh.AddVertex(point);
            }

            mesh.AddFace(new Face3(0, 4, 3));
            mesh.AddFace(new Face3(0, 1, 4));
            mesh.AddFace(new Face3(1, 5, 4));
            mesh.AddFace(new Face3(1, 2, 5));
            mesh.AddFace(new Face3(6, 7, 4));
            mesh.AddFace(new Face3(1, 6, 4));

            var material = k3d.Material.IsotropicMaterial(string.Empty, string.Empty, 200000, 100000, 100000, 0, 0, 0, 0, 0);
            var crosec = k3d.CroSec.ShellConst(0.1, 0, material);
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                true,
                logger,
                out var nodes,
                0.005,
                null,
                points);

            // create supports
            var supportConditions_1 = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(points[0], supportConditions_1),
                k3d.Support.Support(points[1], supportConditions_1),
                k3d.Support.Support(points[2], supportConditions_1),
                k3d.Support.Support(points[4], supportConditions_1),
            };

            // create a Point-load
            const double fx = 100.0;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(points[6], new Vector3(fx * 0.5, 0, 0), new Vector3()),
                k3d.Load.PointLoad(points[7], new Vector3(fx * 0.5, 0, 0), new Vector3()),
            };

            double c = 0.1;
            var joints = new List<Joint>()
            {
                new JointLine(
                    new List<string> { string.Empty },
                    new PolyLine3(new List<Point3> { points[1], points[4] }),
                    Vector3.ZAxis,
                    null,
                    Math.PI * 0.25,
                    new double?[] { c }),
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
                out var warning,
                joints);

            AnalyzeThI.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning_msg, out model);

            var dispTarget = fx / lx / c;
            var dispCalc = outMaxDisp[0];
            Assert.That(dispTarget, Is.EqualTo(dispCalc).Within(1));
        }

        [Test]
        public void TSection_3Elems()
        {
            var k3d = new Toolkit();

            var logger = new MessageLogger();

            const double lx = 1.0;
            const double ly = 2.0;
            const double lz = 3.0;
            var points = new List<Point3>
            {
                new Point3(0, ly, 0), new Point3(0, 0, 0), new Point3(0, -ly, 0),
                new Point3(lx, 0, 0),
                new Point3(0, 0, lz),
            };

            var mesh = new Mesh3();
            foreach (var point in points)
            {
                mesh.AddVertex(point);
            }

            mesh.AddFace(new Face3(0, 1, 3));
            mesh.AddFace(new Face3(1, 2, 3));
            mesh.AddFace(new Face3(1, 4, 3));

            var material = k3d.Material.IsotropicMaterial(string.Empty, string.Empty, 200000, 100000, 100000, 0, 0, 0, 0, 0);
            var crosec = k3d.CroSec.ShellConst(0.1, 0, material);
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                true,
                logger,
                out var nodes,
                0.005,
                null,
                points);

            // create supports
            var supportConditions_1 = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(points[0], supportConditions_1),
                k3d.Support.Support(points[1], supportConditions_1),
                k3d.Support.Support(points[2], supportConditions_1),
                k3d.Support.Support(points[3], supportConditions_1),
            };

            // create a Point-load
            double fx = 100.0;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(points[4], new Vector3(fx, 0, 0), new Vector3()),
            };

            double c = 0.1;
            var joints = new List<Joint>()
            {
                new JointLine(
                    new List<string> { string.Empty },
                    new PolyLine3(new List<Point3> { points[1], points[3] }),
                    Vector3.ZAxis,
                    null,
                    Math.PI * 0.25,
                    new double?[] { c }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out _,
                out var mass,
                out var cog,
                out var message,
                out var warning,
                joints);

            AnalyzeThI.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning_msg, out model);

            var dispTarget = fx / lx / c;
            var dispCalc = outMaxDisp[0];
            Assert.That(dispTarget, Is.EqualTo(dispCalc).Within(1));
        }

        [Test]
        public void TSection_2Elems()
        {
            var k3d = new Toolkit();

            var logger = new MessageLogger();

            double lx = 1.0;
            double ly = 2.0;
            double lz = 3.0;
            var points = new List<Point3>
            {
                new Point3(0, ly, 0), new Point3(0, 0, 0),
                new Point3(lx, 0, 0),
                new Point3(0, 0, lz),
            };

            var mesh = new Mesh3();
            foreach (var point in points)
            {
                mesh.AddVertex(point);
            }

            mesh.AddFace(new Face3(0, 1, 2));
            mesh.AddFace(new Face3(1, 2, 3));

            var material = k3d.Material.IsotropicMaterial(string.Empty, string.Empty, 200000, 100000, 100000, 0, 0, 0, 0, 0);
            var crosec = k3d.CroSec.ShellConst(0.1, 0, material);
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                true,
                logger,
                out var nodes,
                0.005,
                null,
                points);

            // create supports
            var supportConditions_1 = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(points[0], supportConditions_1),
                k3d.Support.Support(points[1], supportConditions_1),
                k3d.Support.Support(points[2], supportConditions_1),
            };

            // create a Point-load
            double fx = 100.0;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(points[3], new Vector3(fx, 0, 0), new Vector3()),
            };

            double c = 0.1;
            var joints = new List<Joint>()
            {
                new JointLine(
                    new List<string> { string.Empty },
                    new PolyLine3(new List<Point3> { points[1], points[2] }),
                    Vector3.ZAxis,
                    null,
                    Math.PI * 0.25,
                    new double?[] { c }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out _,
                out double mass,
                out var cog,
                out var message,
                out var warning,
                joints);

            AnalyzeThI.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning_msg, out model);

            var dispTarget = fx / lx / c;
            var dispCalc = outMaxDisp[0];
            Assert.That(dispTarget, Is.EqualTo(dispCalc).Within(1));
        }
    }
}

#endif
