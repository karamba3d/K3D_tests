#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using UnitSystem = Karamba.Utilities.UnitSystem;

    [TestFixture]
    public class DisassembleModelTests
    {
        [Test]
        public void DisassembleModelAfterModelView()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();

            CroSec_Circle crosec = k3d.CroSec.CircularHollow();
            crosec.Az = 1E20; // make the cross section very stiff in shear

            var elems = new List<BuilderBeam>()
            {
                k3d.Part.IndexToBeam(0, 1, "A", crosec), k3d.Part.IndexToBeam(1, 2, "B", crosec),
            };

            double l = 10.0; // in meter
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

            // limit visibility to one element
            model.dp.basic.visible_elem_ids.Add("A");
            model.dp.setVisibility(model);

            // disassemble model
            model.Disassemble(
                out List<Point3> _,       // out_points
                out List<Line3> lines,
                out List<IMesh> _,        // meshes
                out List<BuilderBeam> _,  // beams
                out List<BuilderShell> _, // shells
                out List<Support> _,      // supports
                out List<Load> _,         // loads
                out List<Karamba.Materials.FemMaterial> _, // materials
                out List<CroSec> _,       // crosecs
                out List<Joint> _,        // joints
                out MessageLogger _);     // logger

            Assert.That(lines, Has.Count.EqualTo(2));
        }
    }
}

#endif
