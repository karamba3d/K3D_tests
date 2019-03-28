using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Rhino.Geometry;

namespace Karamba.GHopper.Geometry
{
    using Karamba.Geometry;
    using Karamba.Utilities;

    /// <summary>
    /// Rhino mesh wrapper.
    /// </summary>
    [Serializable]
    public class RhinoMesh
        : IMesh
    {

        /// <summary>
        /// Underlying rhino mesh.
        /// </summary>
        [NonSerialized]
        private Rhino.Geometry.Mesh
            mesh;

        /// represents the mesh in a serializable form
        private SerializableMesh serializable_mesh; 

        /// <summary>
        /// Get underlying mesh.
        /// </summary>
        public Rhino.Geometry.Mesh Mesh
        {
            get { return this.mesh; }
        }

        /// <summary>
        /// Create empty mesh.
        /// </summary>
        public RhinoMesh()
        {
            this.mesh = new Rhino.Geometry.Mesh();
        }

        /// <summary>
        /// Create rhino mesh wrapper. 
        /// 
        /// The rhino mesh must not be accessed from outside while the 
        /// </summary>
        /// 
        /// <param name="mesh">Rhino mesh.</param>
        /// 
        /// <exception cref="ArgumentNullException">Is thrown if the supplied
        /// rhino mesh is null.</exception>
        public RhinoMesh(
            Rhino.Geometry.Mesh mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }
            this.mesh = mesh;
        }

        public IReadOnlyList<Color> VertexColors
        {
            get { return (IReadOnlyList<Color>)mesh.VertexColors; }
        }

        public IReadOnlyList<Point3> Vertices
        {
            get {
                var vertices = new List<Point3>(mesh.Vertices.Count);
                foreach (var vertex in mesh.Vertices) {
                    vertices.Add(new Point3(vertex.X, vertex.Y, vertex.Z));
                }
                return vertices;
            }
        }

        public IReadOnlyList<Face3> Faces
        {
            get
            {
                var faces = new List<Face3>(mesh.Faces.Count);
                foreach (var face in mesh.Faces)
                {
                    faces.Add(new Face3(face.A, face.B, face.C, face.D));
                }
                return faces;
            }
        }

        public IReadOnlyList<Vector3> Normals
        {
            get
            {
                var normals = new List<Vector3>(mesh.Normals.Count);
                foreach (var normal in mesh.Normals)
                {
                    normals.Add(new Vector3(normal.X, normal.Y, normal.Z));
                }
                return normals;
            }
        }

        public bool AddFace(int v1, int v2, int v3, int v4)
        {
            mesh.Faces.AddFace(v1, v2, v3, v4);
            return true;
        }

        public bool AddFace(int v1, int v2, int v3)
        {
            mesh.Faces.AddFace(v1, v2, v3);
            return true;
        }

        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="x">X component of new vertex coordinate.</param>
        /// <param name="y">Y component of new vertex coordinate.</param>
        /// <param name="z">Z component of new vertex coordinate.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public int AddVertex(double x, double y, double z)
        {
            var res = mesh.Vertices.Count;
            mesh.Vertices.Add(x, y, z);
            return res;
        }

        public void ComputeVertexNormals()
        {
            throw new NotImplementedException();
        }

        public Point3 GetVertex(int idx)
        {
            return mesh.Vertices[idx].Convert();
        }

        public bool SetVertex(int index, Point3 position)
        {
            mesh.Vertices[index] = new Point3f((float)position.X, (float)position.Y, (float)position.Z);
            return true;
        }

        public bool SetVertex(int index, double x, double y, double z)
        {
            mesh.Vertices[index] = new Point3f((float)x, (float)y, (float)z);
            return true;
        }

        public bool ContainsQuads {
            get
            {
                foreach (var face in mesh.Faces)
                {
                    if (face.IsQuad)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public IMesh Triangulated()
        {
            // test whether the mesh contains triangles only
            if (!ContainsQuads) return this;

            var nmesh = new Rhino.Geometry.Mesh();
            nmesh.CopyFrom(mesh);
            for (int face_ind = 0; face_ind < nmesh.Faces.Count; ++face_ind) {
                var face = nmesh.Faces[face_ind];
                if (face.IsQuad) {
                    var l_ac = nmesh.Vertices[face.A].DistanceTo(nmesh.Vertices[face.C]);
                    var l_bd = nmesh.Vertices[face.B].DistanceTo(nmesh.Vertices[face.D]);
                    Rhino.Geometry.MeshFace face1, face2;
                    if (l_ac < l_bd)
                    {
                        face1 = new Rhino.Geometry.MeshFace(face.A, face.B, face.C);
                        face2 = new Rhino.Geometry.MeshFace(face.A, face.C, face.D);
                    }
                    else {
                        face1 = new Rhino.Geometry.MeshFace(face.A, face.B, face.D);
                        face2 = new Rhino.Geometry.MeshFace(face.B, face.C, face.D);
                    }
                    nmesh.Faces.SetFace(face_ind, face1);
                    nmesh.Faces.AddFace(face2);
                }
            }
            return new RhinoMesh(nmesh);
        }

        /// <summary>
        /// determine the center of gravity of a mesh face
        /// </summary>
        /// <param name="faceInd">index of mesh face for which to calculate the area</param>
        /// <returns>center of gravity of mesh face</returns>
        public Point3 faceCOG(int faceInd)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// make a deep copy of the mesh
        /// </summary>
        /// <returns></returns>
        public IMesh DuplicateMesh()
        {
            return new RhinoMesh(mesh.DuplicateMesh());
        }

        /// <summary>Reverses the direction of the mesh.</summary>
        /// <param name="vertexNormals">If true, vertex normals will be reversed.</param>
        /// <param name="faceNormals">If true, face normals will be reversed.</param>
        /// <param name="faceOrientation">If true, face orientations will be reversed.</param>
        public void Flip(bool vertexNormals, bool faceNormals, bool faceOrientation)
        {
            mesh.Flip(vertexNormals, faceNormals, faceOrientation);
        }


        /// <summary>
        /// Create a new mesh based on another mesh doing a sanity check and removing faces with a smaller area than lim_area
        /// </summary>
        /// <param name="lim_area">limit area for culling faces</param>
        /// <param name="point_tree"> kd-tree for input points</param>
        /// <param name="points"> list of points of global model</param>
        /// <param name="cleaned_global_indexes"> list of new global indexes of all vertices</param>
        /// <param name="logger"> gets information about cleaning process</param>
        public IMesh Cleaned(double lim_area, feb.NKDTreeDupli point_tree, IReadOnlyList<Point3> points,
            out List<int> cleaned_global_indexes, MessageLogger logger)
        {
            throw new NotImplementedException();
        }

        public IMesh Copy()
        {
            // Create copy of rhino mesh.
            var nmesh = new Rhino.Geometry.Mesh();
            nmesh.CopyFrom(mesh);
            return new RhinoMesh(nmesh);
        }

        public int AddVertex(Point3 vertex)
        {
            var res = mesh.Vertices.Count;
            mesh.Vertices.Add(vertex.Convert());
            return res;
        }

        public bool AddFace(Face3 face)
        {
            mesh.Faces.AddFace(face.A, face.B, face.C, face.D);
            return true;
        }

        public void AddVertexColor(Color color)
        {
            mesh.VertexColors.Add(color);
        }

        public void ClearVertexColors()
        {
            mesh.VertexColors.Clear();
        }

        /// <summary>
        /// True in case the mesh is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid { get { return mesh.IsValid; } }

        /// <summary>
        /// /// Gets the point on the mesh that is closest to a given test point. Similar to the 
        /// ClosestPoint function except this returns a MeshPoint class which includes
        /// extra information beyond just the location of the closest point.
        /// </summary>
        /// <param name="testPoint"></param>
        /// <param name="maximumDistance"></param>
        /// <returns></returns>
        public Point3 ClosestMeshPoint(Point3 testPoint, double maximumDistance)
        {
#if UnitTest 
            throw new NotImplementedException();
#else
            return (mesh.ClosestMeshPoint(testPoint.Convert(), maximumDistance)).Point.Convert();
#endif
        }
        
        /// <summary>
        /// calculate the normal of the face. 
        /// works for triangles and quads?
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public Vector3 faceNormal(Face3 face)
        {
            var p1 = Vertices[face.A];
            var p2 = Vertices[face.B];
            var p3 = Vertices[face.C];
            var v1 = p2 - p1;
            var v2 = p3 - p1;
            var res = Vector3.CrossProduct(v1, v2);
            double l = res.Length;
            if (l != 0.0) res /= l;
            return res;
        }

        /// <summary>
        /// compute the normals of all faces
        /// </summary>
        public bool ComputeNormals()
        {
            return mesh.Normals.ComputeNormals();
        }


        /// <summary>
        /// determine the area of a mesh face
        /// </summary>
        /// <param name="faceInd">index of mesh face for which to calculate the area</param>
        /// <param name="n">normal unit vector of face</param>
        /// <returns>area of mesh face</returns>
        public double faceArea(int faceInd, out Vector3 n)
        {
            return faceArea(mesh.Faces[faceInd].Convert(), out n);
        }

        public double faceArea(Face3 face, out Vector3 n)
        {
            n = doubleFaceArea(face);
            var area = n.Length / 2.0;
            n.Unitize();
            return area;
        }

        /// <summary>
        /// determine the raw face normal whose length is double the area of the mesh face
        /// <param name="face">mesh face for which to calculate the area</param>
        /// <returns>normal vector to face whose length is double the face area</returns>
        /// </summary>
        public Vector3 doubleFaceArea(Face3 face)
        {
            var res = DoubleTriangleArea(face.A, face.B, face.C);
            if (face.IsQuad)
            {
                res += DoubleTriangleArea(face.A, face.C, face.D);
            }
            return res.Convert();
        }
        
        /// <summary>
        /// calculate the normal of a triangle. Is not scaled to unity.
        /// </summary>
        /// <param name="ind1">index of the first vertex</param>
        /// <param name="ind2">index of the second vertex</param>
        /// <param name="ind3">index of the third vertex</param>
        /// <returns></returns>
        private Vector3d DoubleTriangleArea(int ind1, int ind2, int ind3)
        {
            var p1 = mesh.Vertices[ind1];
            var p2 = mesh.Vertices[ind2];
            var p3 = mesh.Vertices[ind3];
            var v1 = p2 - p1;
            var v2 = p3 - p1;
            var res = Vector3d.CrossProduct(v1, v2);
            return res;
        }

        /// <summary>
        /// determine the characteristic mesh face size
        /// </summary>
        /// <returns>mean distance of mesh vertices in all faces</returns>
        public double characteristicFaceSize()
        {
            double res = 0;
            foreach (MeshFace face in mesh.Faces)
            {
                res += mesh.Vertices[face.A].DistanceTo(mesh.Vertices[face.C]);
                res += mesh.Vertices[face.B].DistanceTo(mesh.Vertices[face.D]);
            }
            if (mesh.Faces.Count != 0)
            {
                return res / (2 * mesh.Faces.Count);
            }
            else
            {
                return 1;
            }
        }
                
        [OnSerializing]
        void prepareForSerialization(StreamingContext sc)
        {
            serializable_mesh = new SerializableMesh(mesh);
        }

        [OnSerialized]
        void cleanupAfterSerialization(StreamingContext sc)
        {
            serializable_mesh = null;
        }

        [OnDeserialized]
        void afterDeserialization(StreamingContext sc)
        {
            mesh = serializable_mesh.getMesh();
        }
    }

    [Serializable]
    public class SerializableMesh
    {
        List<MeshFace> faces;
        List<Point3f> vertices;

        /// <summary>
        /// construct serializable mesh from normal mesh
        /// </summary>
        /// <param name="mesh"></param>
        public SerializableMesh(Mesh mesh)
        {
            faces = new List<MeshFace>(mesh.Faces.Count);
            faces.AddRange(mesh.Faces);
            vertices = new List<Point3f>(mesh.Vertices.Count);
            vertices.AddRange(mesh.Vertices);
        }

        public Mesh getMesh()
        {
            Mesh res = new Mesh();
            res.Faces.AddFaces(faces);
            res.Vertices.AddVertices(vertices);
            faces.Clear();
            vertices.Clear();
            return res;
        }
    }
}
