#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class DisassembleModelTests
    {
        [Test]
        public void DisassembleModelAfterModelView()
        {
            var k3d = new Toolkit();

            var crosec = k3d.CroSec.CircularHollow();
            crosec.Az = 1E20; // make the cross section very stiff in shear

            var elems = new List<BuilderBeam>()
            {
                k3d.Part.IndexToBeam(0, 1, "A", crosec), k3d.Part.IndexToBeam(1, 2, "B", crosec),
            };

            var l = 10.0; // in meter
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

            // limit visibility to one element
            model.dp.basic.visible_elem_ids.Add("A");
            model.dp.setVisibility(model);

            // disassemble model
            model.Disassemble(
                out var out_points,
                out var lines,
                out var meshes,
                out var beams,
                out var shells,
                out var supports,
                out var loads,
                out var materials,
                out var crosecs,
                out var joints,
                out var logger);

            Assert.True(lines.Count == 2);
        }
    }
}

#endif