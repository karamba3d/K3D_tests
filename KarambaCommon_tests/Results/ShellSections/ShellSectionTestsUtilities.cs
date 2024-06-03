namespace KarambaCommon.Tests.Result.ShellSection
{
    using System;
    using System.Collections.Generic;
    using feb;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Results.ShellSection;
    using Model = Karamba.Models.Model;

    internal static class ShellSectionTestsUtilities
    {
        public static ShellSectionState MakeState_ElementBased()
        {
            // Create a state
            var state = new ShellSectionState
            {
                Polylines = new List<PolyLine3>()
                {
                    new PolyLine3(new Point3(0, 0, 0), new Point3(0.5, 0, 0), new Point3(1, 0, 0)),
                },
                Normals = new List<IList<Vector3>>()
                {
                    new List<Vector3>() { new Vector3(0, 0, 1), new Vector3(0, 0, 1), },
                },
            };

            // Define a crossed section.

            // Define mesh normals for crossed faces

            // Define some values for the result.
            var list3 = new List<List<List<double>>>
            {
                new List<List<double>> { new List<double> { 1.0 }, new List<double> { 2.0 } },
            };
            state.Results.Add(ShellSecResult.M_nn, list3);

            return state;
        }

        public static ShellSectionState MakeState_VertexBased()
        {
            // Create a state
            var state = new ShellSectionState
            {
                Polylines = new List<PolyLine3>()
                {
                    new PolyLine3(new Point3(0, 0, 0), new Point3(0.5, 0, 0), new Point3(1, 0, 0)),
                },

                Normals = new List<IList<Vector3>>()
                {
                    new List<Vector3>() { new Vector3(0, 0, 1), new Vector3(0, 0, 1), },
                },
            };

            // Define some values for the result.
            var list2 = new List<List<double>>
            {
                new List<double> { 1.0 },
                new List<double> { 2.0 },
                new List<double> { 3.0 },
            };
            var list3 = new List<List<List<double>>> { list2 };
            state.Results.Add(ShellSecResult.X, list3);

            return state;
        }

        public static AABBTree BuildAabbTree(ShellMesh meshGroup)
        {
            if (meshGroup is null)
                throw new ArgumentNullException(nameof(meshGroup));

            var aabbTree = new AABBTree();
            aabbTree.add(meshGroup.mesh());
            aabbTree.build();
            return aabbTree;
        }

        public static ShellMesh BuildMeshGroup(IEnumerable<ModelMembrane> surfaceElements, Model model)
        {
            var meshGroup = new ShellMesh();
            foreach (var surfaceElement in surfaceElements)
            {
                ShellMesh shellMesh = surfaceElement.feMesh(model);
                shellMesh.swigCMemOwn = false;
                _ = meshGroup.add(shellMesh);
            }

            meshGroup.finalizeConstruction();
            return meshGroup;
        }
    }
}
