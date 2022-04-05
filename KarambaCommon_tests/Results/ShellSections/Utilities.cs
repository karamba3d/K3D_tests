namespace KarambaCommon.Tests.Result.ShellSection
{
    using System.Collections.Generic;
    using Karamba.Geometry;
    using Karamba.Results.ShellSection;

    internal static class Utilities
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
                Normals = new List<List<Vector3>>()
                {
                    new List<Vector3>() { new Vector3(0, 0, 1), new Vector3(0, 0, 1), },
                },
            };

            // Define a crossed section.

            // Define mesh normals for crossed faces

            // Define some values for the result.
            var list1 = new List<double>(2) { 1.0, 2.0 };
            var list2 = new List<List<double>>(1) { list1 };
            var list3 = new List<List<List<double>>>(1) { list2 };
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

                Normals = new List<List<Vector3>>()
                {
                    new List<Vector3>() { new Vector3(0, 0, 1), new Vector3(0, 0, 1), },
                },
            };

            // Define some values for the result.
            var list1 = new List<double>(2) { 1.0, 2.0, 3.0 };
            var list2 = new List<List<double>>(1) { list1 };
            var list3 = new List<List<List<double>>>(1) { list2 };
            state.Results.Add(ShellSecResult.X, list3);

            return state;
        }
    }
}