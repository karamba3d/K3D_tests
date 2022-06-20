namespace KarambaCommon.Tests.Algorithms
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
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

        /// <summary>
        /// Helper to produce a mesh from a rectangle in the XY-plane. The normal is pointing ion the global Z-direction.
        /// </summary>
        /// <param name="fromP">1st point of rectangle</param>
        /// <param name="lx">Length of rectangle in X-direction</param>
        /// <param name="ly">Length of rectangle in Y-direction</param>
        /// <param name="nX">Number of faces in X-direction</param>
        /// <param name="nY">Number of faces in Y-direction</param>
        /// <returns></returns>
        protected Mesh3 RectangularMeshXy(Point3 fromP, double lx, double ly, int nX, int nY)
        {
            var m = new Mesh3();
            var dx = lx / nX;
            var dy = ly / nY;

            for (int i = 0; i <= nX; ++i)
            {
                for (int j = 0; j <= nY; ++j)
                {
                    m.AddVertex(new Point3(fromP.X + i * dx, fromP.Y + j * dy, fromP.Z));
                }
            }

            var nY1 = nY + 1;
            for (int i = 0; i < nX; ++i)
            {
                for (int j = 0; j < nY; ++j)
                {
                    m.AddFace(new Face3(i * nY1 + j, (i + 1) * nY1 + j, (i + 1) * nY1 + j + 1, i * nY1 + j + 1));
                }
            }

            return m;
        }

        /// <summary>
        /// Helper to produce a mesh from a rectangle in the YZ-plane. The normal is pointing ion the global X-direction.
        /// </summary>
        /// <param name="fromP">1st point of rectangle</param>
        /// <param name="ly">Length of rectangle in Y-direction</param>
        /// <param name="lz">Length of rectangle in Z-direction</param>
        /// <param name="nY">Number of faces in Y-direction</param>
        /// <param name="nZ">Number of faces in Z-direction</param>
        /// <returns></returns>
        protected Mesh3 RectangularMeshYz(Point3 fromP, double ly, double lz, int nY, int nZ)
        {
            var m = new Mesh3();
            var dy = ly / nY;
            var dz = lz / nZ;

            for (int i = 0; i <= nY; ++i)
            {
                for (int j = 0; j <= nZ; ++j)
                {
                    m.AddVertex(new Point3(fromP.X, fromP.Y + i * dy, fromP.Z + j * dz));
                }
            }

            var nZ1 = nZ + 1;
            for (int i = 0; i < nY; ++i)
            {
                for (int j = 0; j < nZ; ++j)
                {
                    m.AddFace(new Face3(i * nZ1 + j, (i + 1) * nZ1 + j, (i + 1) * nZ1 + j + 1, i * nZ1 + j + 1));
                }
            }

            return m;
        }

        /// <summary>
        /// Multi-story frame under wind- and live-load. See test examples 'TestExamples\06_Algorithms\OptiCroSec\OptiCroSec_Frame.gh'
        /// </summary>
        [Test]
        public void Frame()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            const int nFloors = 2; // number of stories
            const int nBays = 1; // number of bays
            const double span = 10.0; // span of the frame in meter
            const double depth = 10.0; // depth of the frame in meter
            const double floorHeight = 4.0; // height of a story

            const double totalLength = span * nBays; // total length
            const double totalHeight = floorHeight * nFloors; // total height
            const double loadMeshSize = 0.25; // in meter

            const double w = 1.0; // kN/m2 (wind load, characteristic)
            const double q = 3.0; // kN/m2 (life load, characteristic)
            const double g2 = 5.0; // kN/m2 (additional dead weight, characteristic)

            // get a material from the material table in the folder 'Resources'
            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var materialPath = Path.Combine(resourcePath, "MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = Path.Combine(resourcePath, "CrossSectionValues.bin");
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out var info);
            var crosecFamily = inCroSecs.crosecs.FindAll(x => x.family == "HEA");
            var crosecInitial = crosecFamily.Find(x => x.name == "HEA100");

            var beams = new List<BuilderBeam>();

            // create the columns
            for (var i = 0; i <= nBays; ++i)
            {
                for (var j = 0; j < nFloors; ++j)
                {
                    var p0 = new Point3(i * span, 0, j * floorHeight);
                    var p1 = new Point3(i * span, 0, (j + 1) * floorHeight);
                    var beam = k3d.Part.LineToBeam(new Line3(p0, p1), "column", crosecInitial, logger, out _)[0];
                    var length = p0.DistanceTo(p1) * 2; // to have same length as in definition where columns are a polyline of length 8
                    beam.BucklingLength_SetEstimate(new Vector3(length, length, length));
                    beams.Add(beam);
                }
            }

            // create the floors
            for (var i = 0; i < nBays; ++i)
            {
                for (var j = 1; j <= nFloors; ++j)
                {
                    var p0 = new Point3(i * span, 0, j * floorHeight);
                    var p1 = new Point3((i + 1) * span, 0, j * floorHeight);
                    var beam = k3d.Part.LineToBeam(new Line3(p0, p1), "floor", crosecInitial, logger, out _)[0];
                    var length = p0.DistanceTo(p1);
                    beam.BucklingLength_SetEstimate(new Vector3(length, length, length));
                    beams.Add(beam);
                }
            }

            // create supports
            var supports = new List<Support>();
            for (var i = 0; i <= nBays; ++i)
            {
                supports.Add(k3d.Support.Support(new Point3(i * span, 0, 0), new List<bool>() { true, true, true, true, false, true }));
            }

            // create meshes for loads on floors
            var floorLoadMeshes = new List<Mesh3>();
            for (var j = 1; j <= nFloors; ++j)
            {
                floorLoadMeshes.Add(RectangularMeshXy(
                    new Point3(0, -depth * 0.5, j * floorHeight),
                    totalLength,
                    depth,
                    (int)(totalLength / loadMeshSize),
                    (int)(depth / loadMeshSize)));
            }

            // create mesh for load on facade
            var facadeLoadMeshes = new List<Mesh3>();
            facadeLoadMeshes.Add(RectangularMeshYz(
                    new Point3(0, -depth * 0.5, 0),
                    depth,
                    totalHeight,
                    (int)(depth / loadMeshSize),
                    (int)(totalHeight / loadMeshSize)));
            facadeLoadMeshes[0].ComputeNormals();

            var loads = new List<Load>();

            // create mesh-loads for q and g2
            foreach (var m in floorLoadMeshes)
            {
                loads.Add(
                    k3d.Load.MeshLoad(
                        new List<Vector3>() { new Vector3(0, 0, -g2 - q) },
                        m,
                        LoadOrientation.global,
                        false,
                        true,
                        null,
                        new List<string>() { "floor" }));
            }

            // create mesh-loads for wind
            foreach (var m in facadeLoadMeshes)
            {
                loads.Add(
                    k3d.Load.MeshLoad(
                        new List<Vector3>() { new Vector3(0, 0, w) },
                        m,
                        LoadOrientation.local,
                        false,
                        true,
                        null,
                        new List<string>() { "column" }));
            }

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            // check the initial mass of the structure
            var massTarget = (totalLength * nFloors + totalHeight * (nBays + 1)) * ((CroSec_Beam)crosecInitial).A * material.gamma() / 10; // tons
            Assert.That(mass, Is.EqualTo(massTarget).Within(1E-5));

            // calculate the maximum displacement
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out var outMaxDisp,
                out var outG,
                out var outComp,
                out message);

            double targetUtil = 0.7;
            model = k3d.Algorithms.OptiCroSec(
                model,
                crosecFamily,
                out outMaxDisp,
                out outComp,
                out message,
                5,
                targetUtil,
                5,
                -1.0,
                3,
                true,
                false,
                null,
                null,
                1.0,
                1.1,
                true);
            Assert.That(outMaxDisp[0], Is.EqualTo(0.00760845).Within(1E-5));
            mass = model.mass();
            Assert.That(mass, Is.EqualTo(7.956603).Within(1E-5));

            var bklLens = new List<double>();
            foreach (var beam in beams)
            {
                var elemSl = (BuilderElementStraightLine)beam;
                bklLens.Add(elemSl.BucklingLength(BuilderElementStraightLine.BucklingDir.bklY));
            }

            Assert.That(bklLens[0], Is.EqualTo(-8).Within(1E-5));
        }

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
            var reinfThick = crosec_shell.elem_crosecs[0].layers[1].height;

            // ~ 2 / 1.21 / 43.5 * 2 (result at cantilever mid)
            Assert.That(reinfThick, Is.EqualTo(2.1828168088280552E-06).Within(1E-5));
        }
#endif
    }
}