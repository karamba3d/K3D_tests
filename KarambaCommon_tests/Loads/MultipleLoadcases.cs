#if ALL_TESTS

namespace KarambaCommon.Tests.Loads
{
    using System.Collections.Generic;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;

    [TestFixture]
    public class MultipleLoadCases_tests
    {
        [Test]
        public void TwoPointLoads()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
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
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec_initial },
                logger,
                out List<Point3> out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-load
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(p1, new Vector3(0, 100, 0), new Vector3(), "LC0"),
                k3d.Load.PointLoad(p1, new Vector3(0, 0, 50), new Vector3(), "LC1"),
            };

            // create the model
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out info,
                out double mass,
                out Point3 cog,
                out string message,
                out bool is_warning);

            // calculate the displacements
            AnalyzeThI.solve(model, out var outMaxDisp, out var outG, out var outComp,
                             out string warning, out model);
            Assert.Multiple(() => {
                Assert.That(outMaxDisp[0], Is.EqualTo(54.338219302231252).Within(1e-5));
                Assert.That(outMaxDisp[1], Is.EqualTo(27.169109651115626).Within(1e-5));
            });
        }
    }
}

#endif
