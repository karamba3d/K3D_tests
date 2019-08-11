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
using Karamba.GHopper.Geometry;
using Rhino.Geometry;

namespace KarambaCommon.Tests.Model
{
    [TestFixture]
    public class FindSupportNodesTests
    {
        [Test]
        [Category("QuickTests")]
        public void FindSupportNode()
        {
            var k3d = new Toolkit();

            var nFaces = 1;
            var length = 10.0;
            var xIncMesh = length / nFaces;
            var limit_dist = xIncMesh / 100.0;

            // create the mesh
            var mesh = new Mesh();
            mesh.Vertices.Add(new Point3d(0, -0.5, 0));
            mesh.Vertices.Add(new Point3d(0, 0.5, 0));
            for (var faceInd = 0; faceInd < nFaces; ++faceInd)
            {
                mesh.Vertices.Add(new Point3d((faceInd + 1) * xIncMesh, -0.5, 0));
                mesh.Vertices.Add(new Point3d((faceInd + 1) * xIncMesh, 0.5, 0));
                var nV = mesh.Vertices.Count;
                mesh.Faces.AddFace(new MeshFace(nV - 4, nV - 3, nV - 1, nV - 2));
            }

            // create a shell
            var outLogger = new MessageLogger();
            var outBuilderShells = k3d.Part.MeshToShell(new List<Mesh3>() { mesh.Convert() }, new List<string>(), new List<CroSec>(),
                 outLogger, out var outPoints);

            // create two supports
            var support1 = k3d.Support.Support(new Point3(0, -0.5, 0), k3d.Support.SupportFixedConditions);
            var support2 = k3d.Support.Support(new Point3(0,  0.5, 0), k3d.Support.SupportFixedConditions);

            // create a gravity load
            var gravityLoad = new GravityLoad(new Vector3(0,0,-1));

            // assemble the model
            var model = k3d.Model.AssembleModel(outBuilderShells, new List<Support>() { support1, support2 },
                new List<Load>() { gravityLoad }, out var info, out var mass, out var cog, out var warning, out var runtimeWarning);

            var targetMass = length * 0.01 * 7850;
            Assert.AreEqual(mass, targetMass, 1E-5);
            
            ThIAnalyze.solve(model, out var outMaxDisp, out var outG, out var outComp, out warning, out model);

            // Assert.AreEqual(outMaxDisp[0], 34.137028934059728, 1E-5);
            Assert.AreEqual(outMaxDisp[0], 53.621934860941359, 1E-5);

            
        }
    }
}
