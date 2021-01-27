using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Supports;
using Karamba.Utilities;
using Karamba.Algorithms;

namespace KarambaCommon.Tests.Algorithms
{
    [TestFixture]
    public class MultipleLoadCases_tests
    {
#if ALL_TESTS
        [Test]
        public void TwoPointLoads()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            var resourcePath = @"";

            // get a material from the material table in the folder 'Resources'
            var materialPath = Path.Combine(resourcePath, "Materials/MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = Path.Combine(resourcePath, "CrossSections/CrossSectionValues.bin");
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out var info);
            var crosec_family = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            var crosec_initial = crosec_family.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            crosec_initial.setMaterial(material);

            // create the column
            var beams = k3d.Part.LineToBeam(new List<Line3> { axis }, new List<string>() { "B1" }, new List<CroSec>() { crosec_initial }, logger,
                out var out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() {true, true, true, true, true, true}),
            };

            // create a Point-load
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(p1, new Vector3(0, 100, 0), new Vector3(), "LC0"),
                k3d.Load.PointLoad(p1, new Vector3(0, 0, 50), new Vector3(), "LC1")
            };

            // create the model
            var model = k3d.Model.AssembleModel(beams, supports, loads,
                out info, out var mass, out var cog, out var message, out var is_warning);

            // calculate the displacements
            ThIAnalyze.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning, out model);
            Assert.AreEqual(54.338219302231252, outMaxDisp[0], 1e-5);
            Assert.AreEqual(27.169109651115626, outMaxDisp[1], 1e-5);
        }
#endif
    }
}
