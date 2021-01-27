#if WithMesher

using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace TriangulationFtester
{
    public partial class Meshing_tests
    {
        [Test]
        public static void MeshHoles()
        {
            var e = new TriangulationFCsharp();

            // instead of setting vertices and constraints directly, one can also simply set 
            // set boundary
            // this is the way the API is used from the mesher.
            e.setBoundary(new double[] { 0, 0, 0, 1, 1, 1, 1, 0 }, new int[] { 10, 20, 30, 40});
            //int errorcode = e.Triangulate(-1);
            //Assert.AreEqual(0, errorcode);

            //var v = e.vertices();
            //Assert.Greater(v.Length, 4);

            //var t = e.triangles();
            //Assert.Greater(t.Length, 0);
            double[] hole1 = {
        1.0/3,1.0/3,
        2.0/3,1.0/3,
        2.0/3,2.0/3,
        1.0/3,2.0/3};

            e.addHole(hole1, new int[4] { 1, 1, 1, 1 });


            // do it, triangulate!
            int errorCode = e.Triangulate(.8);

            // which are the hole vertices?
            var vertexConnections = vertexConns(e.triangles(), out int neb, out int nei, out int nvb, out int nvi, out int ec);

            Assert.AreEqual(0, errorCode);
            int[] ids = e.getVertexIds();
            e.WriteObj("MeshHoles.obj");

            // Is the Euler characteristic of the mesh correct?
            int ne = neb + nei;
            int nt = e.ntriangles();
            int nv = nvb + nvi;

            // It is a planar connected mesh with one hole, so it has to be: 0 - 1 + 1 = 0

            Assert.AreEqual(0, ec);
            Assert.AreEqual(0, nt - ne + nv);

            Assert.AreEqual(0, e.EulerCharacteristic());


            // check vertex ids:
            var v = e.vertices();


        }
    }
}
#endif