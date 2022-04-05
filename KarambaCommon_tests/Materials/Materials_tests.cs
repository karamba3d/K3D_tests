#if ALL_TESTS

namespace KarambaCommon.Tests.Materials
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class Materials_tests
    {
        [Test]
        public void Assemble_Material()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 0);
            var p2 = new Point3(1, 1, 0);
            var p3 = new Point3(0, 1, 0);
            var mesh = new Mesh3(new List<Point3>() { p0, p1, p2, p3 }, new List<Face3>() { new Face3(0, 1, 2, 3) });

            // create a shell
            var shells = k3d.Part.MeshToShell(new List<Mesh3>() { mesh }, null, null, logger, out _);

            // create a material
            var gamma = 0.0;
            var materials = new List<FemMaterial>
            {
                new Karamba.Materials.FemMaterial_Isotrop(
                    "family",
                    "name",
                    210000000,
                    10000,
                    10000,
                    gamma,
                    10,
                    -10,
                    FemMaterial.FlowHypothesis.mises,
                    1e-4,
                    null),
            };
            materials[0].AddBeamId(string.Empty); // make material reference all elements

            // assemble the model
            k3d.Model.AssembleModel(
                shells,
                null,
                null,
                out _,
                out var mass,
                out _,
                out _,
                out _,
                null,
                null,
                null,
                0.005,
                null,
                materials);

            Assert.That(mass, Is.EqualTo(0).Within(1e-5));
        }

        [Test]
        public void Read_MaterialTable()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            // get a material from the material table in the folder 'Resources'
            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var materialPath = Path.Combine(resourcePath, "MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            Assert.That(material.alphaT(), Is.EqualTo(1.2e-5).Within(1e-8));
        }
    }
}

#endif