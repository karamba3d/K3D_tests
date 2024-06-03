#if ALL_TESTS

namespace KarambaCommon.Tests.Result.ShellSection
{
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.Geometry;
    using Karamba.Results.ShellSection;
    using NUnit.Framework;

    [TestFixture]
    public class ShellSectionMeshRendererTests
    {
        [Test]
        public void Render_DrawStepwiseMesh()
        {
            ShellSectionState state = ShellSectionTestsUtilities.MakeState_ElementBased();

            var info = new ShellSectionRendererInfoForce
            {
                DisplayResults = new Dictionary<ShellSecResult, bool> { { ShellSecResult.M_nn, true } },
                ScaleResults = new Dictionary<ShellSecResult, double> { { ShellSecResult.M_nn, 1.0 } },
                DisplaySmooth = false,
                DisplayMesh = true,
            };

            var expectedVertices = new List<Point3>()
            {
                new Point3(0, 0, 1),
                new Point3(0, 0, 0),
                new Point3(0.5, 0, 1),
                new Point3(0.5, 0, 0),
                new Point3(0.5, 0, 2),
                new Point3(0.5, 0, 0),
                new Point3(1, 0, 2),
                new Point3(1, 0, 0),
            };
            var expectedFaces = new Face3[] { new Face3(0, 1, 3, 2), new Face3(4, 5, 7, 6), };

            var sut = new ShellSectionMeshRenderer();
            sut.Render(state, info);
            List<IMesh> output = sut.RenderedMeshes;

            Assert.Multiple(() => {
                Assert.That(output, Has.Count.EqualTo(1));
                Assert.That(output[0].Vertices, Is.EqualTo(expectedVertices));
                // Assert.That(output[0].Faces, Is.EqualTo(expectedFaces));
                // ^- NUnit4 bug: TargetParameterCountException
                Assert.That(output[0].Faces.Where((e, idx) => e != expectedFaces[idx]).Any(), Is.False);
                // ^- workaround
            });
        }

        [Test]
        public void Render_DrawSmoothMesh_ForVertexBasedResult()
        {
            ShellSectionState state = ShellSectionTestsUtilities.MakeState_VertexBased();

            ShellSectionRendererInfo info = new ShellSectionRendererInfoDisplacement
            {
                DisplayResults = new Dictionary<ShellSecResult, bool> { { ShellSecResult.X, true } },
                ScaleResults = new Dictionary<ShellSecResult, double> { { ShellSecResult.X, 1.0 } },
                DisplaySmooth = true,
                DisplayMesh = true,
                BaseMeshObject = ShellSectionBaseObject.Vertex,
            };

            var expectedVertices = new Point3[]
            {
                new Point3(0, 0, 1), new Point3(0, 0, 0), new Point3(0.5, 0, 2), new Point3(0.5, 0, 0),
                new Point3(1, 0, 3), new Point3(1, 0, 0),
            };
            var expectedFaces = new List<Face3>() { new Face3(0, 1, 3, 2), new Face3(2, 3, 5, 4), };

            var sut = new ShellSectionMeshRenderer();
            sut.Render(state, info);
            List<IMesh> output = sut.RenderedMeshes;

            Assert.That(output, Has.Count.EqualTo(1));
            Assert.That(output[0].Vertices.ToArray, Is.EqualTo(expectedVertices));
            // Assert.That(output[0].Faces, Is.EqualTo(expectedFaces));
            // ^- NUnit4 bug: TargetParameterCountException
            Assert.That(output[0].Faces.Where((e, idx) => e != expectedFaces[idx]).Any(), Is.False);
            // ^- workaround
        }

        [Test]
        public void Render_DrawSmoothMesh_ForElementBasedResult()
        {
            ShellSectionState state = ShellSectionTestsUtilities.MakeState_ElementBased();

            ShellSectionRendererInfo info = new ShellSectionRendererInfoForce
            {
                DisplayResults = new Dictionary<ShellSecResult, bool> { { ShellSecResult.M_nn, true } },
                ScaleResults = new Dictionary<ShellSecResult, double> { { ShellSecResult.M_nn, 1.0 } },
                DisplaySmooth = true,
                DisplayMesh = true,
                BaseMeshObject = ShellSectionBaseObject.Element,
            };

            var expectedVertices = new Point3[]
            {
                new Point3(0, 0, 1), new Point3(0, 0, 0), new Point3(0.5, 0, 1.5), new Point3(0.5, 0, 0),
                new Point3(1, 0, 2), new Point3(1, 0, 0),
            };
            var expectedFaces = new List<Face3>() { new Face3(0, 1, 3, 2), new Face3(2, 3, 5, 4), };

            var sut = new ShellSectionMeshRenderer();
            sut.Render(state, info);
            List<IMesh> output = sut.RenderedMeshes;

            Assert.That(output, Has.Count.EqualTo(1));
            Assert.That(output[0].Vertices.ToArray(), Is.EqualTo(expectedVertices));
            // Assert.That(output[0].Faces, Is.EqualTo(expectedFaces));
            // ^- NUnit4 bug: TargetParameterCountException
            Assert.That(output[0].Faces.Where((e, idx) => e != expectedFaces[idx]).Any(), Is.False);
            // ^- workaround
        }

        [Test]
        public void Render_DrawStepwiseCurve()
        {
            ShellSectionState state = ShellSectionTestsUtilities.MakeState_ElementBased();

            ShellSectionRendererInfo info = new ShellSectionRendererInfoForce
            {
                DisplayResults = new Dictionary<ShellSecResult, bool> { { ShellSecResult.M_nn, true } },
                ScaleResults = new Dictionary<ShellSecResult, double> { { ShellSecResult.M_nn, 1.0 } },
                DisplaySmooth = false,
            };

            var expectedCurves = new List<PolyLine3>
            {
                new PolyLine3(new Point3(0, 0, 1), new Point3(0.5, 0, 1)),
                new PolyLine3(new Point3(0.5, 0, 2), new Point3(1, 0, 2)),
            };

            var sut = new ShellSectionMeshRenderer();
            sut.Render(state, info);
            List<List<PolyLine3>> output = sut.RenderedCurves;
            IEnumerable<PolyLine3> outputCurves = output.SelectMany(poly => poly);

            Assert.That(outputCurves, Is.EqualTo(expectedCurves));
        }

        [Test]
        public void Render_DrawSmoothCurve_ForVertexBasedResult()
        {
            ShellSectionState state = ShellSectionTestsUtilities.MakeState_VertexBased();

            ShellSectionRendererInfo info = new ShellSectionRendererInfoDisplacement();
            info.DisplayResults[ShellSecResult.X] = true;
            info.ScaleResults[ShellSecResult.X] = 1.0;
            info.DisplaySmooth = true;
            info.BaseMeshObject = ShellSectionBaseObject.Vertex;

            var expectedCurves = new List<PolyLine3>
            {
                new PolyLine3(new Point3(0, 0, 1), new Point3(0.5, 0, 2), new Point3(1, 0, 3)),
            };

            var sut = new ShellSectionMeshRenderer();
            sut.Render(state, info);
            List<List<PolyLine3>> output = sut.RenderedCurves;
            IEnumerable<PolyLine3> outputCurves = output.SelectMany(poly => poly);

            Assert.That(outputCurves, Is.EqualTo(expectedCurves));
        }

        [Test]
        public void Render_DrawSmoothCurve_ForElementBasedResult()
        {
            ShellSectionState state = ShellSectionTestsUtilities.MakeState_ElementBased();

            ShellSectionRendererInfo info = new ShellSectionRendererInfoForce();
            info.DisplayResults[ShellSecResult.M_nn] = true;
            info.ScaleResults[ShellSecResult.M_nn] = 1.0;
            info.DisplaySmooth = true;
            info.BaseMeshObject = ShellSectionBaseObject.Element;

            var expectedCurves = new List<PolyLine3>
            {
                new PolyLine3(new Point3(0, 0, 1), new Point3(0.5, 0, 1.5), new Point3(1, 0, 2)),
            };

            var sut = new ShellSectionMeshRenderer();
            sut.Render(state, info);
            List<List<PolyLine3>> output = sut.RenderedCurves;
            IEnumerable<PolyLine3> outputCurves = output.SelectMany(poly => poly);

            Assert.That(outputCurves, Is.EqualTo(expectedCurves));
        }
    }
}

#endif
