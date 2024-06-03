#if ALL_TESTS

namespace KarambaCommon.Tests.Utilities // .Mappings
{
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using Karamba.Utilities.Mappings;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture]
    public class Mappings
    {
        [Test]
        public void DisassembleAssemble()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            List<BuilderBeam> elems = k3d.Part.LineToBeam(
                new List<Line3>()
                {
                    new Line3(new Point3(0, 0, 0), new Point3(5, 0, 0)),
                    new Line3(new Point3(0, 0, 1), new Point3(5, 0, 1)),
                },
                new List<string> { "beam1", "beam2" },
                null,
                logger,
                out List<Point3> _); // out_points

            var beamsets = new List<ElemSet> { new ElemSet("set1"), new ElemSet("set2"), };
            beamsets[0].add("beam1");
            beamsets[1].add("beam2");

            Model model = k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out string _, // info
                out double _, // mass
                out Point3 _, // cog
                out string _, // msg
                out bool _,   // runtimeWarning
                null,
                null,
                beamsets);

            var stitch = new SimpleStitch(1, beamsets, "beam3");

            model = Mapper.mappedModel(model, new List<Mapping> { stitch }, new List<double> { 0.5, 0.5 });

            // disassemble model
            model.Disassemble(
                out var _, // out_points
                out List<Line3> lines,
                out List<IMesh> _,       // meshes,
                out List<BuilderBeam> beams,
                out List<BuilderShell> _, // shells
                out List<Support> _,     // supports
                out List<Load> _,        // loads
                out List<FemMaterial> _, // materials
                out List<CroSec> _,      // crosecs
                out List<Joint> _,       // joints
                out var _); // logger
            Assert.That(lines, Has.Count.EqualTo(5));

            // assemble model
            Model model2 = k3d.Model.AssembleModel(
                beams,
                null,
                null,
                out string _, // info
                out double _, // mass
                out Point3 _, // cog
                out string _, // msg
                out bool _);  // runtimeWarning

            // disassemble model
            model2.Disassemble(
                out List<Point3> _,       // out_points
                out lines,
                out List<IMesh> _,        // meshes
                out List<BuilderBeam> _,  // beams,
                out List<BuilderShell> _, // shells,
                out List<Support> _,      // supports,
                out List<Load> _,         // loads,
                out List<FemMaterial> _,  // materials,
                out List<CroSec> _,       // crosecs
                out List<Joint> _,        // joints
                out var _);               // logger

            Assert.That(lines, Has.Count.EqualTo(5));
            Assert.That(lines[0].PointAtEnd.X, Is.EqualTo(2.5).Within(1e-5));
        }

        [Test]
        public void InterpolateModels()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            var elems = k3d.Part.LineToBeam(
                new List<Line3>()
                {
                    new Line3(new Point3(0, 0, 0), new Point3(3, 0, 0)),
                    new Line3(new Point3(3, 0, 0), new Point3(6, 0, 0)),
                },
                new List<string> { "beam1", "beam2" },
                null,
                logger,
                out var _); // out_points

            var model = k3d.Model.AssembleModel(
                elems,
                null,
                null,
                out string _, // info
                out double _, // mass
                out Point3 _, // cog
                out string _, // msg
                out bool _);  // runtimeWarning

            var deformedPositions = new List<Point3>
            {
                new Point3(),
                new Point3(3, 0, 3),
                new Point3(6, 0, 0),
            };

            var deformedModel = model.updateDeformation(deformedPositions);

            var feModels = new List<feb.Model> { deformedModel.febmodel };
            var interShape = new InterShape(feModels);

            model = Mapper.mappedModel(model, new List<Mapping> { interShape }, new List<double> { 0.5 });

            // disassemble model
            model.Disassemble(
                out var points,
                out var _, // lines,
                out var _,  // meshes,
                out var _,  // beams
                out var _,  // shells
                out var _,  // supports
                out var _,  // loads
                out var _,  // materials
                out var _,  // crosecs
                out var _,  // joints
                out var _); // logger

            Assert.That(points, Has.Count.EqualTo(3));
            Assert.That(points[1].Z, Is.EqualTo(1.5).Within(1e-5));
        }
    }
}

#endif
