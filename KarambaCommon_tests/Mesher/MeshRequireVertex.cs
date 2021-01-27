#if WithMesher

using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace TriangulationFtester
{
    public partial class Meshing_tests
    {
        [Test]
        public static void MeshRequireVertex()
        {
            var e = new TriangulationFCsharp();
            e.setBoundary(new double[] { 0, 0, 0, 1, 1, 1, 1, 0 }, new int[] { 10, 20, 30, 40 });
            double[] hole1 = {
        1.0/3,1.0/3,
        2.0/3,1.0/3,
        2.0/3,2.0/3,
        1.0/3,2.0/3};

            e.addHole(hole1, new int[4] { 1, 1, 1, 1 });
            e.requireVertex(0.1, 0.1, 1000);
            int errorCode = e.Triangulate(1.1);

            var vertexConnections = vertexConns(
                e.triangles(), out int neb, out int nei, out int nvb, out int nvi, out int ec);

            Assert.AreEqual(0, errorCode);

            e.WriteObj("MeshRequireVertex.obj");

            // Is the Euler characteristic of the mesh correct?
            int ne = neb + nei;
            int nt = e.ntriangles();
            int nv = nvb + nvi;

            // It is a planar connected mesh with one hole, so it has to be: 0 - 1 + 1 = 0

            Assert.AreEqual(0, ec);
            Assert.AreEqual(0, nt - ne + nv);

            Assert.AreEqual(0, e.EulerCharacteristic());
            var v = e.vertices();
            var ids = e.getVertexIds();
        }
    }
}
#endif