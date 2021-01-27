#if WithMesher

using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace TriangulationFtester
{
    public partial class Meshing_tests
    {
        [Test]
        public static void MeshSurface()
        {
            var loc = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var e = new TriangulationFCsharp();
            e.setBoundary(new double[] { 0, 0, 0, 1, 1, 1, 1, 0 }, new int[] { 10, 20, 30, 40 });
            int errorCode = e.Triangulate(.1);
            Assert.AreEqual(0, errorCode);
            var v = e.vertices();
            double[] coords = new double[3 * v.Length / 2];
            for (int i = 0; i < v.Length / 2; i++)
            {
                double x = v[2 * i];
                double y = v[2 * i + 1];
                double z = 1 - 5 * (x - 0.5) * (x - 0.5) - 5 * (y - 0.5) * (y - 0.5);
                coords[3 * i] = x;
                coords[3 * i + 1] = y;
                coords[3 * i + 2] = z;
            }



            // PROMOTE
            e.promoteVerticesToSurface(coords);
            
            
            
            // REMESH the surface
            Assert.AreEqual(0, e.RemeshS(0.1));


            double[] vertsresultr = e.vertices3d();
            double[] solution = new double[e.nvertices3d()];
            for (int i = 0; i < e.nvertices3d(); i++)
            {
                double x = vertsresultr[3 * i];
                double y = vertsresultr[3 * i + 1];
                double z = vertsresultr[3 * i + 2];
                double zrefined = 1 - (0.3 - 0.5) * (0.3 - 0.5) - (0.3 - 0.5) * (0.3 - 0.5);
                double dist1 = Math.Sqrt((x - 0.3) * (x - 0.3) + (y - 0.3) * (y - 0.3) + (z - zrefined) * (z - zrefined));
                solution[i] = 0.001 + dist1;
            }

            // REFINE
            Assert.AreEqual(0, e.RefineWithMetricS(solution, 1));
            vertsresultr = e.vertices3d();
            double[] normalsresult = e.normals3d();
            var triangulesresults = e.triangles3d();
            e.WriteObjS("C:\\Users\\Z77Pro\\surfacefromcsharp.obj");
            e.WriteMsh("C:\\Users\\Z77Pro\\surfacefromcsharpmsh");
        }
    }
}
#endif