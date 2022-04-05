#if ALL_TESTS

namespace KarambaCommon.Tests.Utilities.Mappings
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
    using Karamba.Utilities.Mappings;
    using NUnit.Framework;

    [TestFixture]
    public class Mappings
    {
        [Test]
        public void DisassembleAssemble()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            var elems = k3d.Part.LineToBeam(
                new List<Line3>()
                {
                    new Line3(new Point3(0, 0, 0), new Point3(5, 0, 0)),
                    new Line3(new Point3(0, 0, 1), new Point3(5, 0, 1)),
                },
                new List<string> { "beam1", "beam2" },
                null,
                logger,
                out var out_points);

            var beamsets = new List<ElemSet> { new ElemSet("set1"), new ElemSet("set2"), };
            beamsets[0].add("beam1");
            beamsets[1].add("beam2");

            var model = k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out var info,
                out var mass,
                out var cog,
                out var msg,
                out var runtimeWarning,
                null,
                null,
                beamsets);

            var stitch = new SimpleStitch(1, beamsets, "beam3");

            model = Mapper.mappedModel(model, new List<Mapping> { stitch }, new List<double> { 0.5, 0.5 });

            // disassemble model
            model.Disassemble(
                out out_points,
                out var lines,
                out var meshes,
                out var beams,
                out var shells,
                out var supports,
                out var loads,
                out var materials,
                out var crosecs,
                out var joints,
                out logger);
            Assert.True(lines.Count == 5);

            // assemble model
            var model2 = k3d.Model.AssembleModel(
                beams,
                null,
                null,
                out info,
                out mass,
                out cog,
                out msg,
                out runtimeWarning);

            // disassemble model
            model2.Disassemble(
                out out_points,
                out lines,
                out meshes,
                out beams,
                out shells,
                out supports,
                out loads,
                out materials,
                out crosecs,
                out joints,
                out logger);

            Assert.True(lines.Count == 5);
            Assert.That(lines[0].PointAtEnd.X, Is.EqualTo(2.5).Within(1e-5));
        }
    }
}

#endif