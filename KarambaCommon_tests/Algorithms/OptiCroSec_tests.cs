#if ALL_TESTS
namespace KarambaCommon.Tests.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using MeshFactory = KarambaCommon.Tests.Helpers.MeshFactory;

#pragma warning disable CS1591 // don't enforce documentation in test files.

#pragma warning disable CS1591 // don't enforce documentation in test files.

    [TestFixture]
    public class OptiCroSec_tests
    {
        public OptiCroSec_tests()
        {
            IniConfig.UnitSystem = UnitSystem.SI;
            IniConfig.DefaultUnits();
        }

        [Test]
        public void Insufficient_Truss_ULS()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            double fc = -10 * 10000; // kN/m²
            FemMaterial concrete = k3d.Material.IsotropicMaterial(
                "concrete",
                "concrete",
                21000 * 10000,
                8900 * 10000,
                8900 * 10000,
                0,
                -fc,
                fc,
                FemMaterial.FlowHypothesis.mises,
                0);

            var croSecs = new List<CroSec>();
            for (int i = 1; i < 20; ++i)
            {
                croSecs.Add(new CroSec_Trapezoid("F", "F_" + i, string.Empty, Color.Aqua, concrete, i, 1, 1));
            }

            double L = 10; // m
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(L, 0, 0);
            var trussElements = new List<BuilderElement>();
            var trussElement = k3d.Part.LineToBeam(new Line3(p0, p1), "column", croSecs[0], logger, out _, false)[0];
            trussElements.Add(trussElement);

            // create the support
            var supports = new List<Support>();
            supports.Add(
                k3d.Support.Support(
                    p0,
                    new List<bool>()
                    {
                        true,
                        true,
                        true,
                    }));
            supports.Add(
                    k3d.Support.Support(
                    p1,
                    new List<bool>()
                    {
                        false,
                        true,
                        true,
                    }));

            double f1_uls = ((CroSec_Trapezoid)croSecs[18]).A * fc * 1.5;
            double f2_sls = ((CroSec_Trapezoid)croSecs[6]).A * fc;
            double dispTarget_sls = f2_sls / ((CroSec_Trapezoid)croSecs[10]).A / concrete.E() * L * 1.00001;

            // create a Point-load
            var loads = new List<Load>()
            {
                k3d.Load.PointLoad(p1, new Vector3(f1_uls, 0, 0), new Vector3(), "ULS"),
                k3d.Load.PointLoad(p1, new Vector3(f2_sls, 0, 0), new Vector3(), "SLS"),
            };
            var ULSLcNames = new List<string> { "ULS" };
            var SLSLcNames = new List<string> { "SLS" };

            // create the model
            var model = k3d.Model.AssembleModel(
                trussElements,
                supports,
                loads,
                out var _, // info
                out var _, // mass
                out var _, // cog
                out var _, // message
                out var _); // warning

            double targetUtil = 1.0;

            model = k3d.Algorithms.OptiCroSec(
                model,
                croSecs,
                out var _, // outMaxDisp,
                out var _, // outComp,
                out var _, // message
                out IReadOnlyList<string> insufficientCSOptiElemsSLS,
                out IReadOnlyList<string> insufficientCSOptiElemsULS,
                ULSLcNames,
                SLSLcNames,
                5,
                targetUtil,
                5,
                5,
                dispTarget_sls,
                0.01,
                3,
                true,
                false,
                null,
                null,
                1.0,
                1.1,
                true);

            var optiCroSec = model.elems[0].crosec;
            Assert.That(optiCroSec.name, Is.EqualTo("F_19"));

            Assert.That(insufficientCSOptiElemsULS.Count, Is.EqualTo(1));
            Assert.That(insufficientCSOptiElemsULS[0], Is.EqualTo("0"));
        }

        [Test]
        public void Cantilever_ULS_SLS()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();

            double fc = -10 * 10000; // kN/m²
            FemMaterial concrete = k3d.Material.IsotropicMaterial(
                "concrete",
                "concrete",
                21000 * 10000,
                8900 * 10000,
                8900 * 10000,
                0,
                -fc,
                fc,
                FemMaterial.FlowHypothesis.mises,
                0);

            var croSecs = new List<CroSec>();
            for (int i = 1; i < 20; ++i)
            {
                croSecs.Add(new CroSec_Trapezoid("F", "F_" + i, string.Empty, Color.Aqua, concrete, i, 1, 1));
            }

            double L = 10; // m
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(L, 0, 0);
            var beams = new List<BuilderElement>();
            var beam = k3d.Part.LineToBeam(new Line3(p0, p1), "column", croSecs[0], logger, out _)[0];
            beams.Add(beam);

            // create the support
            var supports = new List<Support>();
            supports.Add(
                k3d.Support.Support(
                    p0,
                    new List<bool>()
                    {
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                    }));

            double f1_uls = -((CroSec_Trapezoid)croSecs[4]).A * fc;
            double f2_sls = -((CroSec_Trapezoid)croSecs[6]).A * fc;
            double dispTarget_sls = f2_sls / ((CroSec_Trapezoid)croSecs[10]).A / concrete.E() * L * 1.00001;

            // create a Point-load
            var loads = new List<Load>()
            {
                k3d.Load.PointLoad(p1, new Vector3(f1_uls, 0, 0), new Vector3(), "ULS"),
                k3d.Load.PointLoad(p1, new Vector3(f2_sls, 0, 0), new Vector3(), "SLS"),
            };
            var ULSLcNames = new List<string> { "ULS" };
            var SLSLcNames = new List<string> { "SLS" };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var _, // info
                out var _, // mass
                out var _, // cog
                out var _, // message
                out var _); // warning

            double targetUtil = 1.0;

            model = k3d.Algorithms.OptiCroSec(
                model,
                croSecs,
                out var outMaxDisp,
                out var _, // outComp
                out var _, // message
                out IReadOnlyList<string> insufficientCSOptiElemsSLS,
                out IReadOnlyList<string> insufficientCSOptiElemsULS,
                ULSLcNames,
                SLSLcNames,
                5,
                targetUtil,
                5,
                5,
                dispTarget_sls,
                0.01,
                3,
                true,
                false,
                null,
                null,
                1.0,
                1.1,
                true);

            var optiCroSec = model.elems[0].crosec;
            Assert.That(optiCroSec.name, Is.EqualTo("F_11"));

            var maxDisp = f2_sls * L / concrete.E() / ((CroSec_Trapezoid)optiCroSec).A;
            Assert.That(maxDisp, Is.EqualTo(outMaxDisp[1]).Within(1E-5));
        }

        /// <summary>
        /// Multi-story frame under wind- and live-load. See test examples 'TestExamples\06_Algorithms\OptiCroSec\OptiCroSec_Frame.gh'
        /// </summary>
        [Test]
        public void Frame()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

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
            var materialPath = PathUtil.MaterialPropertiesFile();
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = PathUtil.CrossSectionValuesFile();
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
                    var length =
                        p0.DistanceTo(p1) *
                        2; // to have same length as in definition where columns are a polyline of length 8
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
                supports.Add(
                    k3d.Support.Support(
                        new Point3(i * span, 0, 0),
                        new List<bool>()
                        {
                            true,
                            true,
                            true,
                            true,
                            false,
                            true,
                        }));
            }

            // create meshes for loads on floors
            var floorLoadMeshes = new List<Mesh3>();
            for (var j = 1; j <= nFloors; ++j)
            {
                floorLoadMeshes.Add(
                    MeshFactory.RectangularMeshXy(
                        new Point3(0, -depth * 0.5, j * floorHeight),
                        totalLength,
                        depth,
                        (int)(totalLength / loadMeshSize),
                        (int)(depth / loadMeshSize)));
            }

            // create mesh for load on facade
            var facadeLoadMeshes = new List<Mesh3>();
            facadeLoadMeshes.Add(
                MeshFactory.RectangularMeshYz(
                    new Point3(0, -depth * 0.5, 0),
                    depth,
                    totalHeight,
                    (int)(depth / loadMeshSize),
                    (int)(totalHeight / loadMeshSize)));
            _ = facadeLoadMeshes[0].ComputeNormals();

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
            var massTarget = (totalLength * nFloors + totalHeight * (nBays + 1)) *
                             ((CroSec_Beam)crosecInitial).A *
                             material.gamma() /
                             10; // tons
            Assert.That(mass, Is.EqualTo(massTarget).Within(1E-5));

            // calculate the maximum displacement
            model = k3d.Algorithms.AnalyzeThI(model, out var outMaxDisp, out var outG, out var outComp, out message);

            var ULSLcNames = new List<string> { "LC0" };
            var SLSLcNames = new List<string> { "LC0" };
            double targetUtil = 0.7;
            model = k3d.Algorithms.OptiCroSec(
                model,
                crosecFamily,
                out outMaxDisp,
                out outComp,
                out message,
                out var insufficientElementIdsSls,
                out var insufficientElementIdsULS,
                ULSLcNames,
                SLSLcNames,
                20,
                targetUtil,
                0,
                0,
                -1.0,
                0.01,
                3,
                true,
                false,
                null,
                null,
                1.0,
                1.1,
                true);

            Assert.That(outMaxDisp[0], Is.EqualTo(0.0076084532546641269).Within(1E-5)); // NII from each load-case
            // Assert.That(outMaxDisp[0], Is.EqualTo(0.00679759362726422).Within(1E-5)); // NII from each load-case

            mass = model.mass();
            Assert.That(mass, Is.EqualTo(7.9566030000000012).Within(1E-5));
            // Assert.That(mass, Is.EqualTo(8.385056).Within(1E-5));

            var bklLens = new List<double>();
            foreach (var beam in beams)
            {
                var elemSl = (BuilderElementStraightLine)beam;
                bklLens.Add(elemSl.BucklingLength(BuilderElementStraightLine.BucklingDir.bklY));
            }

            Assert.That(bklLens[0], Is.EqualTo(-8).Within(1E-5));
        }

        [Test]
        public void OptiCroSecPlateBendingGroupedElements()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();

            double length = 1.0;
            var mesh = new Mesh3();
            _ = mesh.AddVertex(new Point3(0, length, 0));
            _ = mesh.AddVertex(new Point3(0, 0, 0));
            _ = mesh.AddVertex(new Point3(length, 0, 0));
            _ = mesh.AddVertex(new Point3(length, length, 0));
            _ = mesh.AddFace(new Face3(0, 1, 3));
            _ = mesh.AddFace(new Face3(1, 2, 3));

            double fc0 = -10; // kN/m²
            double gamma0 = 20; // kN/m²
            var concrete = k3d.Material.IsotropicMaterial(
                "concrete",
                "concrete",
                21000,
                8900,
                8900,
                gamma0,
                -fc0,
                fc0,
                FemMaterial.FlowHypothesis.mises,
                0);

            var fc1 = -1; // kN/m²
            double gamma1 = 6; // kN/m²
            var wood = k3d.Material.IsotropicMaterial(
                "wood",
                "wood",
                2100,
                890,
                890,
                gamma1,
                -fc1,
                fc1,
                FemMaterial.FlowHypothesis.mises,
                0);

            // set up family of cross sections
            var croSecFamily = new List<CroSec>();
            var thick0 = 0.01;
            croSecFamily.Add(k3d.CroSec.ShellConst(thick0, 0, concrete, "test", "concrete"));
            var thick1 = 0.50;
            croSecFamily.Add(k3d.CroSec.ShellConst(thick1, 0, wood, "test", "wood"));

            var crosec = croSecFamily[1];
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                new List<string> { "shell" },
                new List<CroSec> { crosec },
                logger,
                out var _); // nodes

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(
                    0,
                    new List<bool>()
                    {
                        true,
                        false,
                        true,
                        true,
                        true,
                        true,
                    }),
                k3d.Support.Support(
                    1,
                    new List<bool>()
                    {
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                    }),
            };

            // create a Point-load
            double force = 0.0; // kN
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
                out var _, // info
                out var _, // mass
                out var _, // cog
                out var _, // message
                out var _); // warning);

            double targetUtil = 0.2;
            model = k3d.Algorithms.OptiCroSec(
                model,
                croSecFamily,
                out var maxDisplacements,
                out var compliances,
                out var _, // message
                5,
                targetUtil,
                5,
                -1,
                3,
                true,
                true,
                null,
                new List<string> { "shell" });

            var crosec_shell = model.elems[0].crosec as CroSec_Shell;
            var height = crosec_shell.elem_crosecs[0].layers[0].height;
            var material = crosec_shell.elem_crosecs[0].layers[0].material;

            Assert.That(height, Is.EqualTo(0.01).Within(1E-5));
            // Assert.That(Equals(material, concrete));
            Assert.That(material, Is.EqualTo(concrete));

            var mass = model.mass(); // in tons
            var area = length * length;
            var mass0 = area * thick0 * gamma0 * 0.1; // in tons
            // var mass1 = area * thick1 * gamma1 * 0.1; // in tons (unused)
            Assert.That(mass, Is.EqualTo(mass0).Within(1E-5));
        }

        [Test]
        public void Rope_SLS()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            FemMaterial steel = Material_Default.Instance().steel;

            var croSecs = new List<CroSec>();
            for (int i = 1; i < 20; ++i)
            {
                croSecs.Add(new CroSec_Trapezoid("F", "F_" + i, string.Empty, Color.Aqua, steel, i, 1, 1));
            }

            double L = 200; // m
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(L, 0, 0);
            var beams = new List<BuilderElement>();
            var beam = k3d.Part.LineToBeam(new Line3(p0, p1), "rope", croSecs[0], logger, out _, false)[0];
            beams.Add(beam);

            // create the support
            var supports = new List<Support>
            {
                k3d.Support.Support(
                    p0,
                    new List<bool>()
                    {
                        true,
                        true,
                        true,
                        false,
                        false,
                        false,
                    }),
                k3d.Support.Support(
                    p1,
                    new List<bool>()
                    {
                        false,
                        true,
                        true,
                        false,
                        false,
                        false,
                    }),
            };

            double f1Uls = ((CroSec_Trapezoid)croSecs[5]).A * steel.ft();
            double dispTarget = f1Uls * L / (steel.E() * ((CroSec_Trapezoid)croSecs[15]).A * 0.999);

            // create a Point-load
            var loads = new List<Load>()
            {
                k3d.Load.PointLoad(p1, new Vector3(f1Uls, 0, 0), new Vector3(), "ULS"),
            };
            var ULSLcNames = new List<string> { "ULS" };
            var SLSLcNames = new List<string> { "ULS" };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var _, // info
                out var _, // mass
                out var _, // cog
                out var _, // message
                out var _); // warning

            double targetUtil = 1.0;

            model = k3d.Algorithms.OptiCroSec(
                model,
                croSecs,
                out var outMaxDisp,
                out var _, // outComp,
                out var _, // message
                out var insufficientElementIdsSls,
                out var insufficientElementIdsULS,
                ULSLcNames,
                SLSLcNames,
                5,
                targetUtil,
                5,
                5,
                dispTarget,
                0.01,
                3,
                true,
                false,
                null,
                null,
                1.0,
                1.1,
                true);

            var optiCroSec = model.elems[0].crosec;
            // Assert.That(optiCroSec.name, Is.EqualTo("F_15"));

            // var maxDisp = f1Uls * L / steel.E() / ((CroSec_Trapezoid)optiCroSec).A;
            // Assert.That(maxDisp, Is.EqualTo(outMaxDisp[1]).Within(1E-5));
        }

        [Test]
        public void OptiCroSecPlateInPlane()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();

            double length = 1.0;
            var mesh = new Mesh3();
            _ = mesh.AddVertex(new Point3(0, length, 0));
            _ = mesh.AddVertex(new Point3(0, 0, 0));
            _ = mesh.AddVertex(new Point3(length, 0, 0));
            _ = mesh.AddVertex(new Point3(length, length, 0));
            _ = mesh.AddFace(new Face3(0, 1, 3));
            _ = mesh.AddFace(new Face3(1, 2, 3));

            double fc = -10; // kN/m²
            FemMaterial concrete = k3d.Material.IsotropicMaterial(
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
            var croSecFamily = new List<CroSec>();
            for (int t = 1; t < 200; t += 5)
            {
                croSecFamily.Add(
                    k3d.CroSec.ReinforcedConcreteStandardShellConst(
                        t,
                        0,
                        null,
                        new List<double>
                        {
                            4,
                            4,
                            -4,
                            -4,
                        },
                        0,
                        concrete,
                        null,
                        "Family",
                        "Name" + t));
            }

            CroSec crosec = croSecFamily[0];
            List<BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out List<Point3> _); // nodes

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(
                    0,
                    new List<bool>()
                    {
                        true,
                        false,
                        true,
                        true,
                        true,
                        true,
                    }),
                k3d.Support.Support(
                    1,
                    new List<bool>()
                    {
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                    }),
            };

            // create a Point-load
            double force = -0.4; // kN
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(2, new Vector3( force, 0, 0)), k3d.Load.PointLoad(3, new Vector3(force, 0, 0)),
            };

            // create the model
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out string _, // info
                out double _, // mass
                out Point3 _, // cog
                out string _, // message
                out bool _); // warning

            var ULSLcNames = new List<string> { "LC0" };
            var SLSLcNames = new List<string>(ULSLcNames); // clone
            double targetUtil = 0.2;
            model = k3d.Algorithms.OptiCroSec(
                model,
                croSecFamily,
                out IReadOnlyList<double> maxDisplacements,
                out IReadOnlyList<double> compliances,
                out var _, // message
                out var insufficientElementIdsSls,
                out var insufficientElementIdsULS,
                ULSLcNames,
                SLSLcNames,
                5,
                targetUtil);

            var crosec_shell = model.elems[0].crosec as CroSec_Shell;
            double height = crosec_shell.elem_crosecs[0].layers[0].height;
            double heightTarg = 2 * force / length / fc / targetUtil;
            Assert.That(height, Is.EqualTo(0.41).Within(0.01));

            model = k3d.Algorithms.OptiReinf(
                model,
                out maxDisplacements,
                out compliances,
                out _, // message
                out double reinfMass);

            crosec_shell = model.elems[0].crosec as CroSec_Shell;
            double reinf_thick = crosec_shell.elem_crosecs[0].layers[1].height;
            Assert.That(reinf_thick, Is.EqualTo(1.84E-06).Within(1E-5));
        }

        [Test]
        public void OptiCroSecPlateBending()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();

            double length = 1.0;
            var mesh = new Mesh3();
            _ = mesh.AddVertex(new Point3(0, length, 0));
            _ = mesh.AddVertex(new Point3(0, 0, 0));
            _ = mesh.AddVertex(new Point3(length, 0, 0));
            _ = mesh.AddVertex(new Point3(length, length, 0));
            _ = mesh.AddFace(new Face3(0, 1, 3));
            _ = mesh.AddFace(new Face3(1, 2, 3));

            double fc = -10; // kN/m²
            FemMaterial concrete = k3d.Material.IsotropicMaterial(
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
            var croSecFamily = new List<CroSec>();
            for (int t = 1; t < 200; t += 5)
            {
                croSecFamily.Add(
                    k3d.CroSec.ReinforcedConcreteStandardShellConst(
                        t,
                        0,
                        null,
                        new List<double>
                        {
                            4,
                            4,
                            -4,
                            -4,
                        },
                        0,
                        concrete,
                        null,
                        "Family",
                        "Name" + t));
            }

            CroSec crosec = croSecFamily[0];
            List<BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out List<Point3> _); // nodes

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(
                    0,
                    new List<bool>()
                    {
                        true,
                        false,
                        true,
                        true,
                        true,
                        true,
                    }),
                k3d.Support.Support(
                    1,
                    new List<bool>()
                    {
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                    }),
            };

            // create a Point-load
            double force = -0.4; // kN
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(2, new Vector3(0, 0, force)),
                k3d.Load.PointLoad(3, new Vector3(0, 0, force)),
            };

            // create the model
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out string _, // info
                out double _, // mass
                out Point3 _, // cog
                out string _, // message
                out bool _); //  warning

            var ULSLcNames = new List<string> { "LC0" };
            var SLSLcNames = new List<string> { "LC0" };
            double targetUtil = 0.2;
            model = k3d.Algorithms.OptiCroSec(
                model,
                croSecFamily,
                out IReadOnlyList<double> maxDisplacements,
                out IReadOnlyList<double> compliances,
                out var message,
                out var insufficientElementIdsSls,
                out var insufficientElementIdsULS,
                ULSLcNames,
                SLSLcNames,
                5,
                targetUtil);

            var crosec_shell = model.elems[0].crosec as CroSec_Shell;
            double height0 = crosec_shell.elem_crosecs[0].layers[0].height;
            double height1 = crosec_shell.elem_crosecs[1].layers[0].height;
            double height_mean = 0.5 * (height0 + height1);
            // double heightTarg = Math.Sqrt(2 * force * 6 / fc / targetUtil); // at the support
            double heightTarg = Math.Sqrt(1.05 * force * 6 / fc / targetUtil); // at the element center
            Assert.That(height_mean, Is.EqualTo(1.06).Within(1E-5));

            model = k3d.Algorithms.OptiReinf(
                model,
                out _, // maxDisplacements
                out _, // compliances
                out _, // message
                out double _); // reinfMass

            crosec_shell = model.elems[0].crosec as CroSec_Shell;
            var reinfThick = crosec_shell.elem_crosecs[0].layers[1].height;

            // ~ 2 / 1.21 / 43.5 * 2 (result at cantilever mid)
            Assert.That(reinfThick, Is.EqualTo(2.1828168088280552E-06).Within(1E-5));
        }
    }
}
#endif
