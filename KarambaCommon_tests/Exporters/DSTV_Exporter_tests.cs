#if ALL_TESTS

namespace KarambaCommon.Tests.Exporter
{
    using System;
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Exporters;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads.Combination;
    using Karamba.Materials;
    using Karamba.Models;
    using KarambaCommon;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using UnitSystem = Karamba.Utilities.UnitSystem;

    [TestFixture]
    public class DSTV_Exporter_tests
    {
        [Test]
        public void ExportToDAStV()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();

            CroSec_Circle crosec = k3d.CroSec.CircularHollow();
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

            double l = 5.55555; // in meter
            var points = new List<Point3> { new Point3(), new Point3(l * 0.5, 0, 0), new Point3(l, 0, 0) };

            Model model = k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out string _, // info
                out double _, // mass
                out Point3 _, // cog
                out string _, // msg
                out bool _,   // runtimeWarning
                new List<Joint>(),
                points);

            var loadCaseCombiCollection = new LoadCaseCombinationCollection();
            loadCaseCombiCollection.AddLoadCase("LC", 1);
            var lc_activation = new LoadCaseActivation();

            var builder = new BuilderDSTV_EnglishRStab("RSTAB8", lc_activation);
            var director = new ExportDirector();
            director.ConstructExport(model, builder);
            string[] res = builder.getProduct().ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // value.ToString(CultureInfo.InvariantCulture); gives different results in .NET and Core
            bool compNet48 = res[11] == "#5=MATERIAL($,'name',21.0001111,8.750123,0.000234234567,1.23456E-06,0.00234566,$,$,$);";
            bool compNet70 = res[11] == "#5=MATERIAL($,'name',21.000111099999998,8.750123,0.00023423456699999998,1.23456E-06,0.0023456600000000003,$,$,$);";
            bool comp = compNet48 || compNet70;

            Assert.Multiple(() => {
                Assert.That(comp, Is.True);
                comp = res[16] == "#10=VERTEX($,2777.775,0,0,$);";
                Assert.That(comp, Is.True);
            });
        }
    }
}

#endif
