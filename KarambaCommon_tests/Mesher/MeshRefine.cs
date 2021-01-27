#if WithMesher

using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace TriangulationFtester
{
    public partial class Meshing_tests
    {
        [Test]
        public static void MeshRefine()
        {
    
            var e = new TriangulationFCsharp();
            e.setBoundary(new double[] { 0, 0, 0, 1, 1, 1, 1, 0 }, new int[] { 10, 20, 30, 40 });
            int errorCode = e.Triangulate(.05);
            Assert.AreEqual(0, errorCode);

            // refine the triangulation with a density
            // in mmgtools' terms, one does not prescribe a density but a solution instead,
            // because the main usage of this functionality is to adapt the vertex density to an FEA solution of a differential equation

            int nv = e.nvertices();
            double[] solution = new double[nv];
            double[] vertsresult = e.vertices();
            // caution: some of these might be inside holes, if we had holes

            for (int i = 0; i < nv; i++)
            {
                double x = vertsresult[2 * i];
                double y = vertsresult[2 * i + 1];
                double dist0 = Math.Sqrt((x) * (x) + (y) * (y));
                double dist1 = Math.Sqrt((x - 1) * (x - 1) + (y) * (y));
                double dist2 = Math.Sqrt((x) * (x) + (y - 1) * (y - 1));
                double dist3 = Math.Sqrt((x - 1) * (x - 1) + (y - 1) * (y - 1));

                solution[i] = 0.001 + dist1 * dist2 * dist3 * dist0;
            }

            // so far, only scalar solutions are supported, hence one for their dimension
            errorCode = e.refineWithMetric(solution, 1, 1);

            Assert.AreEqual(0, errorCode);

            var vc = vertexConns(e.triangles(), out int neb, out int nei, out int nvb, out int nvi, out int ec);

            // Is the Euler characteristic of the mesh correct?
            int ne = neb + nei;
            int nt = e.ntriangles();
            nv = nvb + nvi;

            Assert.AreEqual(1, e.EulerCharacteristic());
            Assert.AreEqual(1, ec);
            Assert.AreEqual(1, nt - ne + nv);

            e.WriteObj("MeshRefine.obj");

            var loc = System.Reflection.Assembly.GetExecutingAssembly().Location;

        }
    }
}
#endif