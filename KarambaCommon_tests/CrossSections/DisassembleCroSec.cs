using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Supports;
using Karamba.Models;
using Karamba.Utilities;
using Karamba.Algorithms;


namespace KarambaCommon.Tests.CrossSections
{
    [TestFixture]
    public class Disassemble_CroSec_tests
    {
#if ALL_TESTS
        [Test]
        public void DisassembleShellCroSec()
        {
            var k3d = new Toolkit();
            var crosecs = new List<CroSec_Shell>()
            {
                k3d.CroSec.ShellConst(1.0),
                k3d.CroSec.ShellConst(2.0)
            };

            var heights = crosecs[0].heights();
            Assert.AreEqual(heights.Count, 1, 1E-5);

            var logger = new MessageLogger();
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 1);
            var p2 = new Point3(1, 1, 1);
            var p3 = new Point3(0, 1, 0);
            var mesh = new Mesh3(new List<Point3>() { p0, p1, p2, p3 }, new List<Face3>() { new Face3(0, 1, 2), new Face3(0, 2, 3) });

            var shells = k3d.Part.MeshToShell(new List<Mesh3>() { mesh }, new List<string>() { "s1" }, new List<CroSec>(){crosecs[0]}, logger, out var outPoints);

            var supports = new List<Support> {
                k3d.Support.Support(0, k3d.Support.SupportHingedConditions),
                k3d.Support.Support(1, k3d.Support.SupportHingedConditions)
            };

            // create a Point-load
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(2, new Vector3(), new Vector3(0, 25, 0)),
                k3d.Load.PointLoad(3, new Vector3(), new Vector3(0, 25, 0))
            };

            // create the model
            var model = k3d.Model.AssembleModel(shells, supports, loads,
                out var info, out var mass, out var cog, out var message, out var warning);

            var model_opti = k3d.Algorithms.OptiCroSec(model, crosecs, out List<double> maxDisplacements,
                out List<double> compliances, out message);

            heights = model_opti.elems[0].crosec.heights();
            Assert.AreEqual(heights.Count, 2, 1E-5);
        }
#endif
    }
}
