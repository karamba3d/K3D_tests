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

namespace KarambaCommon.Tests.Materials
{
    [TestFixture]
    public class Materials_tests
    {
        #if ALL_TESTS
        [Test]
        public void Assemble_Material()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(1, 0, 0);
            var p2 = new Point3(1, 1, 0);
            var p3 = new Point3(0, 1, 0);
            var mesh = new Mesh3(new List<Point3>() { p0, p1, p2, p3 },
                new List<Face3>() { new Face3(0, 1, 2, 3) });


            // create a shell
            var shells = k3d.Part.MeshToShell(new List<Mesh3>() { mesh },
                null, null, logger, out var outPoints);

            // create a material
            var gamma = 0.0;
            var materials = new List<FemMaterial> { new Karamba.Materials.FemMaterial_Isotrop("family", "name", 21000, 10000, 10000, gamma, 10, 0, null) };
            materials[0].AddBeamId(""); // make material reference all elements

            // assemble the model
            var model = k3d.Model.AssembleModel(shells, null, null,
            out var info, out var mass, out var cog, out var message, out var warning, 
            null, null, null, 0.005, null, materials);

            Assert.AreEqual(mass, 0, 1e-5);
        }
#endif
    }
}
