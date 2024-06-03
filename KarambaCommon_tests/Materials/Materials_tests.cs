#if ALL_TESTS

namespace KarambaCommon.Tests.Materials
{
    using System.Collections.Generic;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Materials;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;

    [TestFixture]
    public class Materials_tests
    {
        [Test]
        public void Assemble_Material()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();

            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(1, 0, 0);
            Point3 p2 = new Point3(1, 1, 0);
            Point3 p3 = new Point3(0, 1, 0);
            Mesh3 mesh = new Mesh3(new List<Point3>() { p0, p1, p2, p3 }, new List<Face3>() { new Face3(0, 1, 2, 3) });

            // create a shell
            List<BuilderShell> shells = k3d.Part.MeshToShell(new List<Mesh3>() { mesh }, null, null, logger, out _);

            // create a material
            double gamma = 0.0;
            List<FemMaterial> materials = new List<FemMaterial>
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
                    null,
                    out _),
            };
            materials[0].AddBeamId(string.Empty); // make material reference all elements

            // assemble the model
            k3d.Model.AssembleModel(
                shells,
                null,
                null,
                out _,
                out double mass,
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
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();

            // get a material from the material table in the folder 'Resources'
            string materialPath = PathUtil.MaterialPropertiesFile();
            List<FemMaterial> inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            FemMaterial material = inMaterials.Find(x => x.name == "Steel");

            Assert.That(material.alphaT(), Is.EqualTo(1.2e-5).Within(1e-8));
        }
    }
}

#endif
