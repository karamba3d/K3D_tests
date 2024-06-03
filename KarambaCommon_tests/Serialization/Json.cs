#if ALL_TESTS

namespace KarambaCommon.Tests.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Materials;
    using Karamba.Models;
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
            string extListDoubleJson = JsonConvert.SerializeObject(extListDouble);
            ExtendedList<double> extListDoubleDser =
                JsonConvert.DeserializeObject<ExtendedList<double>>(extListDoubleJson);

            var vec = new Vector3(1, 2, 3);
            string vecJson = JsonConvert.SerializeObject(vec);
            Vector3 vecDser = JsonConvert.DeserializeObject<Vector3>(vecJson);
            Assert.That(vecDser.Length, Is.EqualTo(vec.Length).Within(1e-10));
        }

        [Test]
        public void JsonSerialization_Interval3l()
        {
            var json_settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            };

            var interval = new Interval3(1, 2);
            string intervalJson = JsonConvert.SerializeObject(interval, Formatting.Indented, json_settings);

            // File.WriteAllText("model.json", model_json);
            var intervalDser = JsonConvert.DeserializeObject<Interval3>(intervalJson, json_settings);

            Assert.That(intervalDser.T0, Is.EqualTo(interval.T0));
        }

        [Test]
        public void JsonSerialization_Model()
        {
            var k3d = new Toolkit();
            int e = 70000;
            double gamma = 1.0;
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
                null,
                out string _);
            int b = 6; // cm
            int t = 3; // cm
            Karamba.CrossSections.CroSec_Box unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);
            unit_crosec.Az = 1e10; // make cross section rigid in shear

            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A", unit_crosec), };

            double l = 10.0; // m
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };
            var supports = new List<Support> { k3d.Support.Support(0, k3d.Support.SupportFixedConditions) };
            int fz = 1; // kN
            var loads = new List<Load> { k3d.Load.PointLoad(points[1], new Vector3(0, 0, -fz)), };

            Karamba.Models.Model model = k3d.Model.AssembleModel(
                elems,
                supports,
                loads,
                out string _,    // info
                out double _,    // mass
                out Point3 _,    // cog
                out string _,    // msg
                out bool _,      // runtimeWarning
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
            string node_json = JsonConvert.SerializeObject(model.nodes[0]);

            // File.WriteAllText("node.json", node_json);
            Karamba.Nodes.Node node_dser = JsonConvert.DeserializeObject<Karamba.Nodes.Node>(node_json);
            Assert.That(node_dser.ToString(), Is.EqualTo(model.nodes[0].ToString()));

            // element serialization
            //---
            // var elements_output = JsonConvert.SerializeObject(model.elems);
            string elements_json = JsonConvert.SerializeObject(model.elems, Formatting.Indented, json_settings);

            // File.WriteAllText("elements.json", elements_json);
            List<ModelElement> elements_dser = JsonConvert.DeserializeObject<List<ModelElement>>(elements_json, json_settings);
            Assert.That(elements_dser[0].ToString(), Is.EqualTo(model.elems[0].ToString()));

            // model serialization
            //---
            string model_json = JsonConvert.SerializeObject(model, Formatting.Indented, json_settings);

            // File.WriteAllText("model.json", model_json);
            Model model_dser = JsonConvert.DeserializeObject<Model>(model_json, json_settings);

            // calculate Th.I response
            Model _ = k3d.Algorithms.AnalyzeThI(  // model_calc
                model_dser,
                out IReadOnlyList<double> max_disp,
                out IReadOnlyList<double> _, // out_g
                out IReadOnlyList<double> _, // out_comp
                out string _);               // message
            double max_disp_expected = fz * l * l * l / (3 * e * unit_crosec.Iyy);
            Assert.That(max_disp_expected, Is.EqualTo(max_disp[0]).Within(1E-6));
        }

        [Test]
        public void BinarySerialization_Model()
        {
            var model = new Karamba.Models.Model();
            model.materials.Add(
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
                    null,
                    out string warning));
            model.initMaterialCroSecLists();

            System.Runtime.Serialization.IFormatter formatter = ModelSerializer.GetInstance().Formatter;
            var stream = new MemoryStream();
            formatter.Serialize(stream, model);
            string data = Convert.ToBase64String(stream.ToArray());
            var streamDser = new MemoryStream(Convert.FromBase64String(data));
            var modelDser = (Model)formatter.Deserialize(streamDser);
        }

        [Test]
        public void JsonSerialization_ReferencePreservingModel()
        {
            var k3d = new Toolkit();
            int e = 70000;
            double gamma = 1.0;
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
                null,
                out string _);
            int b = 6; // cm
            int t = 3; // cm
            Karamba.CrossSections.CroSec_Box unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);
            unit_crosec.Az = 1e10; // make cross section rigid in shear

            var elems = new List<BuilderBeam>()
            {
                k3d.Part.IndexToBeam(0, 1, "A", unit_crosec),
                k3d.Part.IndexToBeam(1, 2, "A", unit_crosec),
            };

            double l = 10.0; // m
            var points = new List<Point3> { new Point3(), new Point3(0.5 * l, 0, 0), new Point3(l, 0, 0) };
            var supports = new List<Support> { k3d.Support.Support(0, k3d.Support.SupportFixedConditions) };
            int fz = 1; // kN
            var loads = new List<Load> { k3d.Load.PointLoad(points[1], new Vector3(0, 0, -fz)), };

            Karamba.Models.Model model = k3d.Model.AssembleModel(
                elems,
                supports,
                loads,
                out string info,
                out double mass,
                out Point3 cog,
                out string msg,
                out bool runtimeWarning,
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
            string elements_json = JsonConvert.SerializeObject(model.elems, Formatting.Indented, jsonSettings);
            List<ModelElement> elements_dser = JsonConvert.DeserializeObject<List<ModelElement>>(elements_json, jsonSettings);
            FemMaterial material1 = elements_dser[0].crosec.material;
            FemMaterial material2 = elements_dser[1].crosec.material;
            Assert.That(material1, Is.EqualTo(material2));
        }

        [Test]
        public void JsonAssemblyTest()
        {
            var test = new JsonSerializerSettings();

            var assembly = test.GetType().Assembly;

            var test1 = assembly.CodeBase;
            var test2 = assembly.FullName;
        }
    }
}

#endif
