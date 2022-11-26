#if ALL_TESTS

namespace KarambaCommon.Tests.UnitConversion
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class InputGeometryUnitsTests
    {
        [Test]
        public void ComputeMass()
        {
            var k3d = new Toolkit();

            var crosec = k3d.CroSec.CircularHollow();

            var ini_reader = INIReader.Instance();
            ini_reader.Values["UnitLength"] = "cm";
            UnitsConversionFactory.ClearSingleton();
            var ucf = UnitsConversionFactory.Conv();

            crosec = k3d.CroSec.CircularHollow();
            var a = 1.5; // centimeter2
            crosec.A = a;

            var l = 10.0; // centimeter

            var elems = k3d.Part.LineToBeam(
                new List<Line3>() { new Line3(new Point3(0, 0, 0), new Point3(l, 0, 0)) },
                new List<string>(),
                new List<CroSec>() { crosec },
                new MessageLogger(),
                out var outNodes);

            var model = k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out var info,
                out var massStructure_base_units,
                out var cog,
                out var msg,
                out var runtimeWarning,
                new List<Joint>());

            var gamma_steel = 78.5; // kN/m3
            var mass_tar = a / 10000 * l / 100 * gamma_steel * 0.1; // in tons

            var mass = ucf.kg().toUnit(massStructure_base_units) / 1000;
            Assert.That(mass_tar, Is.EqualTo(mass).Within(1e-6));

            MessageLogger logger = new MessageLogger();
            k3d.Model.Disassemble(model, logger, out outNodes, out var outLines, out var outMeshes, out var outBeams, out var outShells, out var outSupports, out var outLoads, out var outMaterials, out var outCroSecs, out var outJoints);
            var bklLength = outBeams[0].BucklingLength(BuilderElementStraightLine.BucklingDir.bklY);
            // -4.0 because there are two loose ends
            Assert.That(-4.0 * l, Is.EqualTo(bklLength).Within(1e-6));

            ini_reader.Values["UnitLength"] = "m";
            UnitsConversionFactory.ClearSingleton();
        }

        [Test]
        public void ReadMaterialPropsFromFile()
        {
            var k3d = new Toolkit();

            var ini_reader = INIReader.Instance();
            ini_reader.Values["UnitLength"] = "mm";
            UnitsConversionFactory.ClearSingleton();
            var ucf = UnitsConversionFactory.Conv();

            // get a material from the material table in the folder 'Resources'
            var resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var materialPath = Path.Combine(resourcePath, "MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "C30/37");

            ini_reader.Values["UnitLength"] = "m";
            UnitsConversionFactory.ClearSingleton();

            Assert.That(material.E(0), Is.EqualTo(33).Within(1e-6));
        }
    }
}

#endif