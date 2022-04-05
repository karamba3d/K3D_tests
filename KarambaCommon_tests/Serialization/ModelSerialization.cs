#if ALL_TESTS

// using System.Text.Json;
// using System.Text.Json.Serialization;

namespace KarambaCommon.Tests.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Karamba.CrossSections;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Supports;
    using Karamba.Utilities;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class Serialization_Tests
    {
        // [Test]
        public void MultiMap()
        {
            var multiMap = new MultiMap<int, List<int>>();
            multiMap.Add(1, new List<List<int>>() { new List<int> { 1, 2 }, new List<int> { 3, 4 } });
            multiMap.Add(5, new List<List<int>>() { new List<int> { 5, 6 }, new List<int> { 7, 8 } });

            var multiMapJson = JsonConvert.SerializeObject(multiMap);
            MultiMap<int, List<int>> multiMapDser = JsonConvert.DeserializeObject<MultiMap<int, List<int>>>(multiMapJson);

            List<List<int>> item1 = multiMapDser[1];
            Assert.That(item1.Count, Is.EqualTo(2));
            Assert.That(item1[1][0], Is.EqualTo(3));
            List<List<int>> item2 = multiMapDser[5];
            Assert.That(item2.Count, Is.EqualTo(2));
            Assert.That(item2[1][0], Is.EqualTo(7));
        }

        // [Test]
        public void BeamModelBinarySerialization()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(0, 0, length);
            var axis = new Line3(p0, p1);

            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            // get a material from the material table in the folder 'Resources'
            var materialPath = Path.Combine(resourcePath, "MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = Path.Combine(resourcePath, "CrossSectionValues.bin");
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out var info);
            var crosec_family = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            var crosec_initial = crosec_family.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            crosec_initial.setMaterial(material);

            // create the column
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec_initial },
                logger,
                out var out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, false, false, true }),
                k3d.Support.Support(p1, new List<bool>() { true, true, false, false, false, false }),
            };

            // create a Point-load
            var loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(0, 0, -100)), };

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

            IFormatter formatter = ModelSerializer.GetInstance().Formatter;
            MemoryStream stream = new MemoryStream();
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
                Karamba.Models.Model modelDeser = (Karamba.Models.Model)formatter.Deserialize(stream);
                Assert.True(modelDeser.elems.Count == model.elems.Count);
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        [Test]
        public void ShellModelBinarySerialization()
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

            var crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(
                25,
                0,
                null,
                new List<double> { 4, 4, -4, -4 },
                0);
            var shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out var nodes);

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
            var model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            IFormatter formatter = ModelSerializer.GetInstance().Formatter;
            MemoryStream stream = new MemoryStream();
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
                Karamba.Models.Model modelDeser = (Karamba.Models.Model)formatter.Deserialize(stream);
                Assert.True(modelDeser.elems.Count == model.elems.Count);
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}

#endif