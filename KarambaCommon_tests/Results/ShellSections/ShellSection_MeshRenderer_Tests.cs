#if ALL_TESTS

namespace KarambaCommon.Tests.Result.ShellSection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.Geometry;
    using Karamba.Results.ShellSection;
    using NUnit.Framework;

    [TestFixture]
    public class ShellSection_MeshRenderer_Tests
    {
        [Test]
        public void Render_DrawStepwiseMesh()
        {
            ShellSectionState state = Utilities.MakeState_ElementBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force
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

            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<IMesh> output = sut.RenderedMeshes;

            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0].Vertices, Is.EqualTo(expectedVertices));
            Assert.That(output[0].Faces, Is.EqualTo(expectedFaces));
        }

        [Test]
        public void Render_DrawSmoothMesh_ForVertexBasedResult()
        {
            ShellSectionState state = Utilities.MakeState_VertexBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Displacement
            {
                DisplayResults = new Dictionary<ShellSecResult, bool> { { ShellSecResult.X, true } },
                ScaleResults = new Dictionary<ShellSecResult, double> { { ShellSecResult.X, 1.0 } },
                DisplaySmooth = true,
                DisplayMesh = true,
                BaseMeshObject = ShellSec_BaseObj.Vertex,
            };

            var expectedVertices = new Point3[]
            {
                new Point3(0, 0, 1), new Point3(0, 0, 0), new Point3(0.5, 0, 2), new Point3(0.5, 0, 0),
                new Point3(1, 0, 3), new Point3(1, 0, 0),
            };
            var expectedFaces = new List<Face3>() { new Face3(0, 1, 3, 2), new Face3(2, 3, 5, 4), };

            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<IMesh> output = sut.RenderedMeshes;

            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0].Vertices.ToArray, Is.EqualTo(expectedVertices));
            Assert.That(output[0].Faces, Is.EqualTo(expectedFaces));
        }

        [Test]
        public void Render_DrawSmoothMesh_ForElementBasedResult()
        {
            ShellSectionState state = Utilities.MakeState_ElementBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force
            {
                DisplayResults = new Dictionary<ShellSecResult, bool> { { ShellSecResult.M_nn, true } },
                ScaleResults = new Dictionary<ShellSecResult, double> { { ShellSecResult.M_nn, 1.0 } },
                DisplaySmooth = true,
                DisplayMesh = true,
                BaseMeshObject = ShellSec_BaseObj.Element,
            };

            var expectedVertices = new Point3[]
            {
                new Point3(0, 0, 1), new Point3(0, 0, 0), new Point3(0.5, 0, 1.5), new Point3(0.5, 0, 0),
                new Point3(1, 0, 2), new Point3(1, 0, 0),
            };
            var expectedFaces = new List<Face3>() { new Face3(0, 1, 3, 2), new Face3(2, 3, 5, 4), };

            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<IMesh> output = sut.RenderedMeshes;

            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0].Vertices.ToArray, Is.EqualTo(expectedVertices));
            Assert.That(output[0].Faces, Is.EqualTo(expectedFaces));
        }

        [Test]
        public void Render_DrawStepwiseCurve()
        {
            ShellSectionState state = Utilities.MakeState_ElementBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force
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

            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<List<PolyLine3>> output = sut.RenderedCurves;
            var outputCurves = output.SelectMany(poly => poly);

            Assert.That(outputCurves, Is.EqualTo(expectedCurves));
        }

        [Test]
        public void Render_DrawSmoothCurve_ForVertexBasedResult()
        {
            ShellSectionState state = Utilities.MakeState_VertexBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Displacement();
            info.DisplayResults[ShellSecResult.X] = true;
            info.ScaleResults[ShellSecResult.X] = 1.0;
            info.DisplaySmooth = true;
            info.BaseMeshObject = ShellSec_BaseObj.Vertex;

            var expectedCurves = new List<PolyLine3>
            {
                new PolyLine3(new Point3(0, 0, 1), new Point3(0.5, 0, 2), new Point3(1, 0, 3)),
            };

            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<List<PolyLine3>> output = sut.RenderedCurves;
            var outputCurves = output.SelectMany(poly => poly);

            Assert.That(outputCurves, Is.EqualTo(expectedCurves));
        }

        [Test]
        public void Render_DrawSmoothCurve_ForElementBasedResult()
        {
            ShellSectionState state = Utilities.MakeState_ElementBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force();
            info.DisplayResults[ShellSecResult.M_nn] = true;
            info.ScaleResults[ShellSecResult.M_nn] = 1.0;
            info.DisplaySmooth = true;
            info.BaseMeshObject = ShellSec_BaseObj.Element;

            var expectedCurves = new List<PolyLine3>
            {
                new PolyLine3(new Point3(0, 0, 1), new Point3(0.5, 0, 1.5), new Point3(1, 0, 2)),
            };

            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<List<PolyLine3>> output = sut.RenderedCurves;
            var outputCurves = output.SelectMany(poly => poly);

            Assert.That(outputCurves, Is.EqualTo(expectedCurves));
        }
    }
}

#endif