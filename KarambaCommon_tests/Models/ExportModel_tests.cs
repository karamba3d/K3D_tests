#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Exporters;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Loads.Combinations;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class ExportModel_tests
    {
        [Test]
        public void ExportToDAStV()
        {
            var k3d = new Toolkit();

            var crosec = k3d.CroSec.CircularHollow();
            crosec.Az = 1E20; // make the cross section very stiff in shear
            crosec.setMaterial(
                k3d.Material.IsotropicMaterial(
                    "family",
                    "name",
                    21000.1111,
                    8750.123,
                    8750.123,
                    234.234567,
                    2.34566,
                    -10.3,
                    FemMaterial.FlowHypothesis.mises,
                    1.23456e-6));

            var elems = new List<BuilderBeam>()
            {
                k3d.Part.IndexToBeam(0, 1, "A", crosec), k3d.Part.IndexToBeam(1, 2, "B", crosec),
            };

            var l = 5.55555; // in meter
            var points = new List<Point3> { new Point3(), new Point3(l * 0.5, 0, 0), new Point3(l, 0, 0) };

            var model = k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out var info,
                out var mass,
                out var cog,
                out var msg,
                out var runtimeWarning,
                new List<Joint>(),
                points);

            var lc_indexer = new LCCombiCollector();
            lc_indexer.AddLoadCase("LC", 1);

            var builder = new BuilderDSTV_EnglishRStab("RSTAB8", lc_indexer);
            ExportDirector director = new ExportDirector();
            director.ConstructExport(model, builder);
            var res = builder.getProduct().ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var comp = res[11] ==
                       "#5=MATERIAL($,'name',21.0001111,8.750123,0.000234234567,1.23456E-06,0.00234566,$,$,$);";
            Assert.IsTrue(comp);
            comp = res[16] == "#10=VERTEX($,2777.775,0,0,$);";
            Assert.IsTrue(comp);
        }
    }
}

#endif