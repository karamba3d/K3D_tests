#if WithMesher

using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace TriangulationFtester
{
    public partial class Meshing_tests
    {
        [Test]
        public static void MeshSurfaceStatic()
        {
            var result = MMGStatic.MMGSLIB(new double[] { 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0.5, 0.5, 0 }, new int[] { 0, 1, 4, 1, 2, 4, 2, 3, 4, 0, 3, 4 }, 0.5);
            var vertices = result.Item1;
            var triangles = result.Item2;
        }
    }
}
#endif