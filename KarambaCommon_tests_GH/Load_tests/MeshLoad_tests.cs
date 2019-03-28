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

namespace KarambaCommon.Tests.Loads
{
    [TestFixture]
    public class MeshLoad_tests
    {
        [Test]
        public void MeshLoadProfiling()
        {
            var k3d = new Toolkit();

            int nBeams = 20;
            int nFaces = 1500;
            double lengthBeams = 10.0;
            double xIncBeam = lengthBeams / nBeams;
            double xIncMesh = lengthBeams / nFaces;
            double limit_dist = xIncBeam / 100.0;

            // create beams
            var lines = new List<Line3>();
            var nodeI = new Point3(0,0,0);
            for (int beamInd = 0; beamInd < nBeams; ++beamInd)
            {
                var nodeK = new Point3(nodeI.X + xIncBeam, 0, 0);
                lines.Add(new Line3(nodeI, nodeK));
                nodeI = nodeK;
            }
            LineToBeam.solve(new List<Point3>(), lines, true, true, limit_dist, new List<Vector3>(), new List<string>(), 
                new List<Color>(), new List<CroSec>(), true, 
                out List<Point3> outPoints, out List<BuilderBeam> builderElements, out string info);

            // create a MeshLoad
            var meshUnitLoadsBuffer = new Dictionary<MeshUnitLoad, MeshUnitLoad>();
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
            UnitsConversionFactory ucf = UnitsConversionFactories.Conv();
            UnitConversion m = ucf.m();
            var baseMesh = m.toBaseMesh(new RhinoMesh(mesh));

            var load = new MeshLoad(new List<Vector3>() {new Vector3(0, 0, -1)});
            load.InitUnitLoads(meshUnitLoadsBuffer, baseMesh, LoadOrientation.global, new List<bool>() {true, true},
                new List<Point3>(), new List<string>());

            // create a Support
            var support = k3d.Support.Support(new Point3(0, 0, 0), k3d.Support.SupportFixedConditions);

            // assemble the model
            var model = k3d.Model.AssembleModel(builderElements, new List<Support>() { support }, new List<Load>() { load },
                out info, out var mass, out var cog, out var msg, out var runtimeWarning);         

            ThIAnalyze.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning, out model);

            Assert.AreEqual(outMaxDisp[0], 2.8223745365824779, 1E-5);
            // Assert.AreEqual(1, 1, 1E-8);
        }
    }
}
