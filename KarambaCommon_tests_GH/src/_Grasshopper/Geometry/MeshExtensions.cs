using System;
using Karamba.Geometry;
using Rhino.Geometry;

namespace Karamba.GHopper.Geometry
{
    public static class MeshExtensions
    {
        public static Karamba.Geometry.Face3 Convert(this Rhino.Geometry.MeshFace face)
        {
            return new Face3(face.A, face.B, face.C, face.D);
        }

        public static Karamba.Geometry.Mesh3 Convert(this Rhino.Geometry.Mesh mesh)
        {
            var res = new Karamba.Geometry.Mesh3(mesh.Vertices.Count, mesh.Faces.Count);

            foreach (Point3f vertex in mesh.Vertices)
            {
                res.AddVertex(vertex.Convert());
            }

            foreach (MeshFace face in mesh.Faces)
            {
                res.AddFace(face.A, face.B, face.C, face.D);
            }
            return res;
        }

        /// <summary>
        /// Convert to mesh to rhino mesh. If the mesh is already of type 
        /// <see cref="RhinoMesh"/>, the underlying mesh <see cref="RhinoMesh.Mesh"/>
        /// is returned without creating a copy of it. Otherwise, a new rhino 
        /// mesh instance is created from the supplied mesh.
        /// 
        /// The following attributes/properties are taken into account.
        /// 
        ///     (1) Mesh topology (vertices, faces)
        ///     (2) Vertex normals.
        ///     (3) Vertex colors.
        ///     
        /// </summary>
        /// 
        /// <param name="mesh">Mesh.</param>
        /// 
        /// <returns>Rhino mesh.</returns>
        /// 
        /// <exception cref="ArgumentNullException">Is thrown when the mesh is null.
        /// </exception>
        public static Rhino.Geometry.Mesh Convert(
            this IReadonlyMesh mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }
            var rmesh = mesh as RhinoMesh;
            if (rmesh != null)
            {
                return rmesh.Mesh;
            }
            else
            {
                var out_mesh = new Rhino.Geometry.Mesh();
                foreach (var v in mesh.Vertices)
                {
                    out_mesh.Vertices.Add(v.Convert());
                }
                foreach (var c in mesh.VertexColors)
                {
                    out_mesh.VertexColors.Add(c);
                }
                foreach (var c in mesh.Faces)
                {
                    out_mesh.Faces.AddFace(c.A, c.B, c.C, c.D);
                }
                return out_mesh;
            }
        }
    }
}
