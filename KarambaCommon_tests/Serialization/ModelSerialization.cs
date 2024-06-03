#if ALL_TESTS

namespace KarambaCommon.Tests.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Runtime.Serialization;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Combination;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using MeshFactory = KarambaCommon.Tests.Helpers.MeshFactory;

    [TestFixture]
    public class Serialization_Tests
    {
        /// <summary>
        /// Multi-story frame under wind- and live-load. See test examples 'TestExamples\06_Algorithms\OptiCroSec\OptiCroSec_Frame.gh'
        /// </summary>
        [Test]
        public void MeshLoad()
        {
            var logger = new MessageLogger();
            var k3d = new Toolkit();

            var beamLength = 5.0;
            var beam = k3d.Part.LineToBeam(
                line: new Line3(
                    new Point3(0, 0, 0),
                    new Point3(beamLength, 0, 0)),
                id: string.Empty,
                crosec: null,
                info: logger,
                out _);

            var hc0 = k3d.Support.SupportHingedConditions;
            var hc1 = new List<bool> { false, true, true, true, false, false };
            var supports = new List<Support>
            {
                k3d.Support.Support(0, hc0),
                k3d.Support.Support(1, hc1),
            };

            var loadMesh = MeshFactory.RectangularMeshXy(
                    new Point3(0, -beamLength * 0.5, 0),
                    beamLength,
                    beamLength,
                    1,
                    1);

            var loads = new List<Load>
            {
                k3d.Load.MeshLoad(
                    new List<Vector3>() { new Vector3(0, 0, 1) },
                    loadMesh,
                    LoadOrientation.local,
                    false,
                    true,
                    null,
                    new List<string>() { "column" }),
            };

            var model = k3d.Model.AssembleModel(
                beam,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _);

            // serialize model using json
            var serModel = ModelSerializer.ToJson(model);

            Assert.That(serModel, !Is.EqualTo(string.Empty));
        }

        [Test]
        public void MultiMap1()
        {
            var multiMap = new MultiMap<int, int>
            {   { 1, new List<int>() { 1, 2, 3, 4 } },
                { 5, new List<int>() { 5, 6, 7, 8 } }
            };

            string multiMapJson = JsonConvert.SerializeObject(multiMap);
            MultiMap<int, int> multiMapDser = JsonConvert.DeserializeObject<MultiMap<int, int>>(multiMapJson);

            List<int> item1 = multiMapDser[1];
            Assert.That(item1, Has.Count.EqualTo(4));
            Assert.That(item1[0], Is.EqualTo(1));
            List<int> item2 = multiMapDser[5];
            Assert.That(item2, Has.Count.EqualTo(4));
            Assert.That(item2[1], Is.EqualTo(6));
        }

        [Test]
        public void MultiMap2()
        {
            var multiMap = new MultiMap<int, List<int>>
            {   { 1, new List<List<int>>() { new List<int> { 1, 2 }, new List<int> { 3, 4 } } },
                { 5, new List<List<int>>() { new List<int> { 5, 6 }, new List<int> { 7, 8 } } }
            };

            string multiMapJson = JsonConvert.SerializeObject(multiMap);
            MultiMap<int, List<int>> multiMapDser = JsonConvert.DeserializeObject<MultiMap<int, List<int>>>(multiMapJson);

            List<List<int>> item1 = multiMapDser[1];
            Assert.That(item1, Has.Count.EqualTo(2));
            Assert.That(item1[1][0], Is.EqualTo(3));
            List<List<int>> item2 = multiMapDser[5];
            Assert.That(item2, Has.Count.EqualTo(2));
            Assert.That(item2[1][0], Is.EqualTo(7));
        }

        [Test]
        public void BeamModelBinarySerialization()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(0, 0, length);
            var axis = new Line3(p0, p1);

            // get a material from the material table in the folder 'Resources'
            string materialPath = PathUtil.MaterialPropertiesFile();
            List<Karamba.Materials.FemMaterial> inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            Karamba.Materials.FemMaterial material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            string crosecPath = PathUtil.CrossSectionValuesFile();
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out string info);
            List<CroSec> crosec_family = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            CroSec crosec_initial = crosec_family.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            crosec_initial.setMaterial(material);

            // create the column
            List<Karamba.Elements.BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec_initial },
                logger,
                out List<Point3> out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, false, false, true }),
                k3d.Support.Support(p1, new List<bool>() { true, true, false, false, false, false }),
            };

            // create a Point-load
            var loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(0, 0, -100)), };

            // create the model
            Model model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out info,
                out double mass,
                out Point3 cog,
                out string message,
                out bool warning);

            IFormatter formatter = ModelSerializer.GetInstance().Formatter;
            var stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream, model);
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }

            stream.Position = 0;
            try
            {
                Model modelDeser = (Model)formatter.Deserialize(stream);
                Assert.That(modelDeser.elems, Has.Count.EqualTo(model.elems.Count));
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        [Test]
        public void LoadCaseBinarySerialization()
        {
            var lc = new LoadCase(string.Empty);
            lc.Add("LC1", 1.35);

            IFormatter formatter = ModelSerializer.GetInstance().Formatter;
            var stream = new MemoryStream();
            formatter.Serialize(stream, lc);
            stream.Position = 0;
            var lcDeser = (LoadCase)formatter.Deserialize(stream);
            Assert.That(lcDeser.ToString(), Is.EqualTo(lc.ToString()));
        }

        [Test]
        public void LoadCaseCombinationBinarySerialization()
        {
            var lc1 = new LoadCase(string.Empty);
            lc1.Add("LC1", 1.35);
            var lc2 = new LoadCase(string.Empty);
            lc2.Add("LC2", 1.50);
            var loadCases = new List<LoadCase>() { lc1, lc2 };

            var lcCombi = new LoadCaseCombination("lcc1", loadCases);
            var lcCombiTuple =
                new List<Tuple<int, LoadCaseCombination>>() { new Tuple<int, LoadCaseCombination>(1, lcCombi) };
            IFormatter formatter = ModelSerializer.GetInstance().Formatter;
            var stream = new MemoryStream();
            formatter.Serialize(stream, lcCombiTuple);
            stream.Position = 0;

            var lcCombiDeserTuple =
                (List<Tuple<int, LoadCaseCombination>>)formatter.Deserialize(stream);
            LoadCaseCombination lcCombiDeser = lcCombiDeserTuple[0].Item2;
            Assert.That(lcCombiDeser.ToString(), Is.EqualTo(lcCombi.ToString()));
        }

        [Test]
        public void LoadCaseCombinationCollectionBinarySerialization()
        {
            var loadCaseNames = new List<string>() { "LC0", "LC1" };
            var loadCombinationRules = new List<string>();

            var lcCombinationCollection = new LoadCaseCombinationCollection(loadCaseNames, loadCombinationRules);

            IFormatter formatter = ModelSerializer.GetInstance().Formatter;
            var stream = new MemoryStream();
            formatter.Serialize(stream, lcCombinationCollection);
            stream.Position = 0;

            var lcCombinationCollectionDeser = (LoadCaseCombinationCollection)formatter.Deserialize(stream);
            var s1 = lcCombinationCollection.OrderedLoadCaseCombinations[0].ToString();
            var s2 = lcCombinationCollectionDeser.OrderedLoadCaseCombinations[0].ToString();
            Assert.That(s2, Is.EqualTo(s1));
        }

        [Test]
        public void ShellModelBinarySerialization()
        {
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

            CroSec_Shell crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(
                25,
                0,
                null,
                new List<double> { 4, 4, -4, -4 },
                0);
            List<Karamba.Elements.BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out List<Point3> _); // nodes

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
            Model model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out string _, // info
                out double _, // mass
                out Point3 _, // cog
                out string _, // message
                out bool _);  // warning

            IFormatter formatter = ModelSerializer.GetInstance().Formatter;
            var stream = new MemoryStream();
            formatter.Serialize(stream, model);

            stream.Position = 0;
            Model modelDeser = (Model)formatter.Deserialize(stream);
            Assert.That(modelDeser.elems, Has.Count.EqualTo(model.elems.Count));
        }

        [Test]
        public void MeshLoadSerialization()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();

            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 0);
            var p2 = new Point3(1, 1, 0);
            var p3 = new Point3(0, 1, 0);
            var mesh = new Mesh3(new List<Point3>() { p0, p1, p2, p3 }, new List<Face3>() { new Face3(0, 1, 2, 3), });

            // create a mesh load
            MeshLoad load = k3d.Load.MeshLoad(
                new List<Vector3>() { new Vector3(0, 0, 1) },
                mesh, LoadOrientation.global,
                generatePointLoads: false,
                generateLineLoads: true,
                loadPoints: null,
                elemIDs: new List<string> { "" });

            // create a beam
            FemMaterial steel = Material_Default.Instance().steel;
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new Line3(new Point3(), new Point3(1, 1, 0)),
                "b0",
                new CroSec_Trapezoid("F", "F_" + 10, string.Empty, Color.Aqua, steel, 10, 1, 1),
                logger,
                out _);

            // create the model
            Model model = k3d.Model.AssembleModel(
                beams,
                null,
                new List<Load>() { load },
                out string _, // info
                out double _, // mass
                out Point3 _, // cog
                out string _, // message
                out bool _);  // warning

            IFormatter formatter = ModelSerializer.GetInstance().Formatter;
            var stream = new MemoryStream();
            formatter.Serialize(stream, model);

            stream.Position = 0;
            Model modelDeser = (Model)formatter.Deserialize(stream);
            Assert.That(modelDeser.elems, Has.Count.EqualTo(model.elems.Count));
        }
    }
}

#endif
