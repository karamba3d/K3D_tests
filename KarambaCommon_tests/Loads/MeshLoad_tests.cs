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

namespace KarambaCommon.Tests.Loads
{
    [TestFixture]
    public class MeshLoad_tests
    {
// #if ALL_TESTS
        [Test]
        public void MeshLoadProfiling()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();

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

            var builderElements = k3d.Part.LineToBeam(lines, new List<string>(), new List<CroSec>(), logger, out List<Point3> outPoints);

            // create a MeshLoad
            var mesh = new Mesh3((nFaces+1)*2, nFaces);
            mesh.AddVertex(new Point3(0, -0.5, 0));
            mesh.AddVertex(new Point3(0, 0.5, 0));
            for (var faceInd = 0; faceInd < nFaces; ++faceInd)
            {
                mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, -0.5, 0));
                mesh.AddVertex(new Point3((faceInd + 1) * xIncMesh, 0.5, 0));
                var nV = mesh.Vertices.Count;
                mesh.AddFace(nV - 4, nV - 3, nV - 1, nV - 2);
            }
            UnitsConversionFactory ucf = UnitsConversionFactories.Conv();
            UnitConversion m = ucf.m();
            var baseMesh = m.toBaseMesh(mesh);

            // create a mesh load
            var load = k3d.Load.MeshLoad(new List<Vector3>() { new Vector3(0, 0, -1) }, baseMesh);

            // create a support
            var support = k3d.Support.Support(new Point3(0, 0, 0), k3d.Support.SupportFixedConditions);

            // assemble the model
            var model = k3d.Model.AssembleModel(builderElements, new List<Support>() { support }, new List<Load>() { load }, 
                out var info, out var mass, out var cog, out var message, out var runtimeWarning);
            
            // calculate the model
            model = k3d.Algorithms.AnalyzeThI(model, out var outMaxDisp, out var outG, out var outComp, out var warning);
            Assert.AreEqual(outMaxDisp[0], 2.8232103119228276, 1E-5);
        }
// #endif
    }
}
