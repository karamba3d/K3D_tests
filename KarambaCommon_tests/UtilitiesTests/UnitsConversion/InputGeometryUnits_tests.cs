#if ALL_TESTS

namespace KarambaCommon.Tests.UnitConversion
{
    using System;
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Models;
    using Karamba.Utilities;
    // using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;
    using Helper = KarambaCommon.Tests.Helpers.Helper;

    [TestFixture]
    public class InputGeometryUnitsTests
    {
        [Test]
        public void ComputeMass()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();

            CroSec_Circle crosec = k3d.CroSec.CircularHollow();

            IniConfig.ReSet();
            IniConfig.SetValue("UnitLength", "cm");
            UnitsConversionFactory.ClearSingleton();
            var ucf = UnitsConversionFactory.Conv();

            crosec = k3d.CroSec.CircularHollow();
            double a = 1.5; // centimeter2
            crosec.A = a;

            double l = 10.0; // centimeter

            var elems = k3d.Part.LineToBeam(
                new List<Line3>() { new Line3(new Point3(0, 0, 0), new Point3(l, 0, 0)) },
                new List<string>(),
                new List<CroSec>() { crosec },
                new MessageLogger(),
                out List<Point3> outNodes);

            Model model = k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out var info,
                out var massStructure_base_units,
                out var cog,
                out var msg,
                out var runtimeWarning,
                new List<Joint>());

            double gamma_steel = 78.5; // kN/m3
            double mass_tar = a / 10000 * l / 100 * gamma_steel * 0.1; // in tons

            var mass = ucf.kg().toUnit(massStructure_base_units) / 1000;
            Assert.That(mass_tar, Is.EqualTo(mass).Within(1e-6));

            var logger = new MessageLogger();
            k3d.Model.Disassemble(model, logger, out outNodes, out var outLines, out var outMeshes, out var outBeams, out var outShells, out var outSupports, out var outLoads, out var outMaterials, out var outCroSecs, out var outJoints);
            var bklLength = outBeams[0].BucklingLength(BuilderElementStraightLine.BucklingDir.bklY);

            // -4.0 because there are two loose ends
            Assert.That(-4.0 * l, Is.EqualTo(bklLength).Within(1e-6));

            IniConfig.ReSet();
            IniConfig.SetValue("UnitLength", "m");
            UnitsConversionFactory.ClearSingleton();
        }

        [Test]
        public void ReadMaterialPropsFromFile()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();

            IniConfig.SetValue("UnitLength", "mm");
            UnitsConversionFactory.ClearSingleton();
            var ucf = UnitsConversionFactory.Conv();

            // get a material from the material table in the folder 'Resources'
            var materialPath = PathUtil.MaterialPropertiesFile();
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "C30/37");

            IniConfig.SetValue("UnitLength", "m");
            UnitsConversionFactory.ClearSingleton();

            Assert.That(material.E(0), Is.EqualTo(33).Within(1e-6));
        }
    }
}

#endif
