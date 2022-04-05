#if ALL_TESTS

// using System.Text.Json;
// using System.Text.Json.Serialization;

namespace KarambaCommon.Tests.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Materials;
    using Karamba.Supports;
    using Karamba.Utilities;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Joint = Karamba.Joints.Joint;
    using Load = Karamba.Loads.Load;

    [TestFixture]
    public class Json_Tests
    {
        [Test]
        public void JsonSerialization_MiscClasses()
        {
            var extListDouble = new ExtendedList<double>(0);
            var extListDoubleJson = JsonConvert.SerializeObject(extListDouble);
            var extListDoubleDser = JsonConvert.DeserializeObject<ExtendedList<double>>(extListDoubleJson);

            var vec = new Vector3(1, 2, 3);
            var vecJson = JsonConvert.SerializeObject(vec);
            var vecDser = JsonConvert.DeserializeObject<Vector3>(vecJson);
            Assert.That(vecDser.Length, Is.EqualTo(vec.Length).Within(1e-10));
        }

        [Test]
        public void JsonSerialization_Model()
        {
            var k3d = new Toolkit();
            var e = 70000;
            var gamma = 1.0;
            var unit_material = new FemMaterial_Isotrop(
                "unit",
                "unit",
                e,
                0.5 * e,
                0.5 * e,
                gamma,
                1.0,
                -1.0,
                FemMaterial.FlowHypothesis.mises,
                1.0,
                null);
            var b = 6; // cm
            var t = 3; // cm
            var unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);
            unit_crosec.Az = 1e10; // make cross section rigid in shear

            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unit_crosec), };

            var l = 10.0; // m
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };
            var supports = new List<Support> { k3d.Support.Support(0, k3d.Support.SupportFixedConditions) };
            var fz = 1; // kN
            var loads = new List<Load> { k3d.Load.PointLoad(points[1], new Vector3(0, 0, -fz)), };

            var model = k3d.Model.AssembleModel(
                elems,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var msg,
                out var runtimeWarning,
                new List<Joint>(),
                points);

            // json serialization/de-serialization
            //---
            var json_settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            };

            // node serialization
            //---
            var node_json = JsonConvert.SerializeObject(model.nodes[0]);

            // File.WriteAllText("node.json", node_json);
            var node_dser = JsonConvert.DeserializeObject<Karamba.Nodes.Node>(node_json);
            Assert.IsTrue(model.nodes[0].ToString() == node_dser.ToString());

            // element serialization
            //---
            // var elements_output = JsonConvert.SerializeObject(model.elems);
            var elements_json = JsonConvert.SerializeObject(model.elems, Formatting.Indented, json_settings);

            // File.WriteAllText("elements.json", elements_json);
            var elements_dser = JsonConvert.DeserializeObject<List<ModelElement>>(elements_json, json_settings);
            Assert.IsTrue(model.elems[0].ToString() == elements_dser[0].ToString());

            // model serialization
            //---
            var model_json = JsonConvert.SerializeObject(model, Formatting.Indented, json_settings);

            // File.WriteAllText("model.json", model_json);
            var model_dser = JsonConvert.DeserializeObject<Karamba.Models.Model>(model_json, json_settings);

            // calculate Th.I response
            var model_calc = k3d.Algorithms.AnalyzeThI(
                model_dser,
                out var max_disp,
                out var out_g,
                out var out_comp,
                out var message);
            var max_disp_expected = fz * l * l * l / (3 * e * unit_crosec.Iyy);
            Assert.That(max_disp_expected, Is.EqualTo(max_disp[0]).Within(1E-6));
        }

        [Test]
        public void BinarySerialization_Model()
        {
            var model = new Karamba.Models.Model();
            model.in_materials.Add(
                new FemMaterial_Isotrop(
                    "test",
                    string.Empty,
                    1,
                    2,
                    3,
                    4,
                    5,
                    -6,
                    FemMaterial.FlowHypothesis.mises,
                    8,
                    null));
            model.initMaterialCroSecLists();

            var formatter = ModelSerializer.GetInstance().Formatter;
            var stream = new MemoryStream();
            formatter.Serialize(stream, model);
            var data = Convert.ToBase64String(stream.ToArray());
            var streamDser = new MemoryStream(Convert.FromBase64String(data));
            var modelDser = (Karamba.Models.Model)formatter.Deserialize(streamDser);
        }

        [Test]
        public void JsonSerialization_ReferencePreservingModel()
        {
            var k3d = new Toolkit();
            var e = 70000;
            var gamma = 1.0;
            var unit_material = new FemMaterial_Isotrop(
                "unit",
                "unit",
                e,
                0.5 * e,
                0.5 * e,
                gamma,
                1.0,
                -1.0,
                FemMaterial.FlowHypothesis.mises,
                1.0,
                null);
            var b = 6; // cm
            var t = 3; // cm
            var unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);
            unit_crosec.Az = 1e10; // make cross section rigid in shear

            var elems = new List<BuilderBeam>()
            {
                k3d.Part.IndexToBeam(0, 1, "A", unit_crosec),
                k3d.Part.IndexToBeam(1, 2, "A", unit_crosec),
            };

            var l = 10.0; // m
            var points = new List<Point3> { new Point3(), new Point3(0.5 * l, 0, 0), new Point3(l, 0, 0) };
            var supports = new List<Support> { k3d.Support.Support(0, k3d.Support.SupportFixedConditions) };
            var fz = 1; // kN
            var loads = new List<Load> { k3d.Load.PointLoad(points[1], new Vector3(0, 0, -fz)), };

            var model = k3d.Model.AssembleModel(
                elems,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var msg,
                out var runtimeWarning,
                new List<Joint>(),
                points);

            // json serialization/de-serialization
            //---
            var jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceResolverProvider = () => new IdReferenceResolver(),
            };

            // element serialization
            //---
            var elements_json = JsonConvert.SerializeObject(model.elems, Formatting.Indented, jsonSettings);
            var elements_dser = JsonConvert.DeserializeObject<List<ModelElement>>(elements_json, jsonSettings);
            var material1 = elements_dser[0].crosec.material;
            var material2 = elements_dser[1].crosec.material;
            Assert.IsTrue(material1 == material2);
        }
    }
}

#endif