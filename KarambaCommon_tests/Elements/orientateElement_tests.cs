using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Supports;
using Karamba.Models;
using Karamba.Utilities;
using Karamba.Algorithms;

namespace KarambaCommon.Tests.Elements
{
    [TestFixture]
    public class OrientateElement_tests
    {
#if ALL_TESTS
        [Test]
        public void orientBeam()
        {
            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 1, 0);
            var ln = new Line3(p0, p1);
            var beam_id = "b1";
            var yOriNew = new Vector3(0, 1, 1);

            var k3d = new Toolkit();
            var beamBuilders = k3d.Part.LineToBeam(new List<Line3>() { ln }, new List<string>() { beam_id }, new List<CroSec>(), logger,
                out var out_points);
            beamBuilders.Add(k3d.Part.OrientateBeam("b1", null, yOriNew));
            var model = k3d.Model.AssembleModel(beamBuilders, null, null, out var info, out var mass, out var cog, out var msg, out var warning);

            k3d.Model.Disassemble(model, logger, out var points, out var lines, out var meshes, out var beams, out var shells, 
                out var supports, out var loads, out var materials, out var crosecs, out var joints);

            var yOri = beams[0].Ori.YOri;
            var diff = (yOri.Value - yOriNew).Length;
            Assert.AreEqual(diff, 0, 1E-10);
        }
#endif
    }
}
