namespace KarambaCommon.Tests.Helpers
{
    using Karamba.Geometry;

    public static class MeshFactory
    {
        /// <summary>
        /// Helper to produce a mesh from a rectangle in the XY-plane. The normal is pointing ion the global Z-direction.
        /// </summary>
        /// <param name="fromP">1st point of rectangle</param>
        /// <param name="lx">Length of rectangle in X-direction</param>
        /// <param name="ly">Length of rectangle in Y-direction</param>
        /// <param name="nX">Number of faces in X-direction</param>
        /// <param name="nY">Number of faces in Y-direction</param>
        /// <returns></returns>
        public static Mesh3 RectangularMeshXy(Point3 fromP, double lx, double ly, int nX, int nY)
        {
            var m = new Mesh3();
            var dx = lx / nX;
            var dy = ly / nY;

            for (int i = 0; i <= nX; ++i)
            {
                for (int j = 0; j <= nY; ++j)
                {
                    _ = m.AddVertex(new Point3(fromP.X + i * dx, fromP.Y + j * dy, fromP.Z));
                }
            }

            var nY1 = nY + 1;
            for (int i = 0; i < nX; ++i)
            {
                for (int j = 0; j < nY; ++j)
                {
                    int a = i * nY1 + j;
                    int b = (i + 1) * nY1 + j;
                    _ = m.AddFace(new Face3(a, b, b + 1, a + 1));
                    // was: i * nY1 + j, (i + 1) * nY1 + j, (i + 1) * nY1 + j + 1, i * nY1 + j + 1
                }
            }

            return m;
        }

        /// <summary>
        /// Helper to produce a mesh from a rectangle in the YZ-plane. The normal is pointing ion the global X-direction.
        /// </summary>
        /// <param name="fromP">1st point of rectangle</param>
        /// <param name="ly">Length of rectangle in Y-direction</param>
        /// <param name="lz">Length of rectangle in Z-direction</param>
        /// <param name="nY">Number of faces in Y-direction</param>
        /// <param name="nZ">Number of faces in Z-direction</param>
        /// <returns></returns>
        // XXX DRY!!! just different order
        public static Mesh3 RectangularMeshYz(Point3 fromP, double ly, double lz, int nY, int nZ)
        {
            var m = new Mesh3();
            var dy = ly / nY;
            var dz = lz / nZ;

            for (int i = 0; i <= nY; ++i)
            {
                for (int j = 0; j <= nZ; ++j)
                {
                    _ = m.AddVertex(new Point3(fromP.X, fromP.Y + i * dy, fromP.Z + j * dz));
                }
            }

            var nZ1 = nZ + 1;
            for (int i = 0; i < nY; ++i)
            {
                for (int j = 0; j < nZ; ++j)
                {
                    int a = i * nZ1 + j;
                    int b = (i + 1) * nZ1 + j;
                    _ = m.AddFace(new Face3(a, b, b + 1, a + 1));
                }
            }

            return m;
        }
    }
}
