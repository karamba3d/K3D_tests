#if WithMesher

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace TriangulationFtester
{
    public partial class Meshing_tests
    {
        [Test]
        public static void MeshSquare()
        {
            var e = new TriangulationFCsharp();
            string commit = e.commitHash();
            // give 5 vertices, the interior one first, then the boundary. The 1 means there is no interior vertex not applying Fortran counting.
            // e.setVerticesConstraints(new double[] { 0, 0, 0, 1, 1, 1, 1, 0 }, new int[] { 0 });
            e.setBoundary(new double[] { 0, 0, 0,0, 0, 1, 1, 1, 1, 0 }, new int[] { 10, 11, 20, 30, 40 });
            // set max edge length to 0.8
            int errorCode = e.Triangulate(0.8);
            Assert.AreEqual(0, errorCode);

            // get the result
            var vv = e.vertices();
            var tt = e.triangles();
            int nt = e.ntriangles();
            int nv = e.nvertices();

            // Is the Euler characteristic of the mesh correct?

            // get the boundary vertices.
            List<int> boundaryVertices = new List<int>();
            var vertexConnections = vertexConns(tt, out int neb, out int nei, out int nvb, out int nvi, out int ec);
           

            // two ways to check that the Euler characteristic is ok.
            Assert.AreEqual(1, e.EulerCharacteristic());
            Assert.AreEqual(1, ec);

            var loc = System.Reflection.Assembly.GetExecutingAssembly().Location;
            e.WriteObj("MeshSquare.obj");
            var ids = e.getVertexIds();
        }


        // a dictionary mapping edges (coded as the integer A*nv + B) to their valences, and the number of connected boundary and interior edges, and the Euler characteristic
        static Dictionary<int, int> vertexConns(int[] tt, out int neb, out int nei, out int nvb, out int nvi, out int ec)
        {
            int nv = tt.AsEnumerable().Max() + 1; // just a number that is higher than all entries of the triangles

            var vertexConnections = new Dictionary<int, int>();
            for (int i = 0; i < tt.Length; i += 3)
            {
                int A = tt[i];
                int B = tt[i + 1];
                int C = tt[i + 2];

                List<int> s = new List<int> { A, B, C };
                s.Sort();
                A = s[0]; B = s[1]; C = s[2];
                int s1 = A * nv + B; // just to code two ints into one uniquely
                if (vertexConnections.ContainsKey(s1)) vertexConnections[s1] += 1;
                else vertexConnections[s1] = 1;

                s1 = B * nv + C;
                if (vertexConnections.ContainsKey(s1)) vertexConnections[s1] += 1;
                else vertexConnections[s1] = 1;

                s1 = A * nv + C;
                if (vertexConnections.ContainsKey(s1)) vertexConnections[s1] += 1;
                else vertexConnections[s1] = 1;
            }
            int ne = vertexConnections.Count;
            neb = 0;

            var connectedVertices = new Dictionary<int, bool>();
            var boundaryVertices = new Dictionary<int, bool>();

            foreach (var x in vertexConnections)
            {
                int A = x.Key % nv;
                int B = (int)(x.Key / nv);
                if (!connectedVertices.ContainsKey(A)) connectedVertices[A] = true;
                if (!connectedVertices.ContainsKey(B)) connectedVertices[B] = true;

                if (x.Value == 1)
                {
                    // this edge is boundary
                    neb++;
                    if (!boundaryVertices.ContainsKey(A)) boundaryVertices[A] = true;
                    if (!boundaryVertices.ContainsKey(B)) boundaryVertices[B] = true;

                }
            }

            nei = ne - neb;
            nvb = boundaryVertices.Count;
            nvi = connectedVertices.Count - boundaryVertices.Count;
            
            ec = tt.Length / 3 - ne + connectedVertices.Count;
            return vertexConnections;
        }
    }
}
#endif