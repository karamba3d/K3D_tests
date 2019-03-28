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
using Karamba.Joints;

namespace KarambaCommon.Tests.Result
{
    [TestFixture]
    public class ReinforcementStressTests
    {
#if ALL_TESTS
        [Test]
        public void DeformationTestIsotropicShellInPlane()
        {
            var nFaces = 1;
            var length = 1.0;
            var xIncMesh = length / nFaces;
            var limit_dist = xIncMesh / 100.0;

            var y0 = 0.0;
            var y1 = 1.0;

            // create the mesh
            var mesh = new Mesh3((nFaces + 1) * 2, nFaces);
            mesh.AddVertex(new Point3(0, y0, 0));
            mesh.AddVertex(new Point3(0, y1, 0));
            for (var faceInd = 0; faceInd < nFaces; ++faceInd)
            {
                mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, y0, 0));
                mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, y1, 0));
                var nV = mesh.Vertices.Count;
                mesh.AddFace(nV - 4, nV - 3, nV - 1, nV - 2);
            }

            // create a shell
            MeshToShell.solve(new List<Point3>(), new List<Mesh3>() { mesh }, limit_dist, new List<string>(), new List<Color>(), new List<CroSec>(),
                out List<Point3> outPoints, out List<BuilderShell> outBuilderShells, out MessageLogger outLogger);

            // create two supports
            var support1 = new Support(new Point3(0, y0, 0), new List<bool>() { true, true , true, true, true, true }, Plane3.Default);
            var support2 = new Support(new Point3(0, y1, 0), new List<bool>() { true, false, true, true, true, true }, Plane3.Default);

            // create two point loads
            var pl1 = new PointLoad(mesh.Vertices.Count - 2, new Vector3(25, 0, 0), new Vector3(), false);
            var pl2 = new PointLoad(mesh.Vertices.Count - 1, new Vector3(25, 0, 0), new Vector3(), false);
            
            // assemble the model
            var modelBuilder = new ModelBuilder(limit_dist);
            var model = modelBuilder.build(new List<Point3>(), new List<FemMaterial>(), new List<CroSec>(),
                new List<Support>() { support1, support2 }, new List<Load>() { pl1, pl2 }, outBuilderShells, new List<ElemSet>(), 
                new List<Joint>(), new MessageLogger()
            );

            ThIAnalyze.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning, out model);

            Assert.AreEqual(outMaxDisp[0], 2.4858889456113236E-05, 1E-5);
        }
#endif
    }
}
