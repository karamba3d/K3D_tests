using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Karamba.Results.ShellSection;
using Karamba.Geometry;
using NSubstitute;

namespace KarambaCommon.Tests.Result.ShellSection
{
    static class Utilities
    {
        public  static ShellSecState MakeState_ElementBased()
        {
            //Create a state
            var state = new ShellSecState();


            //Define a crossed section.
            state.Polylines = new List<PolyLine3>()
            {
                new PolyLine3
                (
                    new Point3(0,0,0),
                    new Point3(0.5,0,0),
                    new Point3(1,0,0)
                )
            };

            //Define mesh normals for crossed faces
            state.Normals = new List<List<Vector3>>()
            {
                new List<Vector3>()
                {
                    new Vector3(0,0,1),
                    new Vector3(0,0,1)
                }
            };



            //Define some values for the result.
            var list1 = new List<double>(2) { 1.0, 2.0 };
            var list2 = new List<List<double>>(1) { list1 };
            var list3 = new List<List<List<double>>>(1) { list2 };
            state.Results.Add(ShellSecResult.M_nn, list3);


            return state;
        }

        public static ShellSecState MakeState_VertexBased()
        {
            //Create a state
            var state = new ShellSecState();

            //Define a crossed section.
            state.Polylines = new List<PolyLine3>()
            {
                new PolyLine3
                (
                    new Point3(0,0,0),
                    new Point3(0.5,0,0),
                    new Point3(1,0,0)
                )
            };

            //Define mesh normals for crossed faces
            state.Normals = new List<List<Vector3>>()
            {
                new List<Vector3>()
                {
                    new Vector3(0,0,1),
                    new Vector3(0,0,1)
                }
            };

            //Define some values for the result.
            var list1 = new List<double>(2) { 1.0, 2.0, 3.0 };
            var list2 = new List<List<double>>(1) { list1 };
            var list3 = new List<List<List<double>>>(1) { list2 };
            state.Results.Add(ShellSecResult.X, list3);


            return state;
        }
    }

    [TestFixture]
    public class ShellSec_MeshRenderer_Tests
    {
        


        [Test]
        public void Render_DrawStepwiseMesh()
        {
            ShellSecState state = Utilities.MakeState_ElementBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force();
            info.DisplayResults = new Dictionary<ShellSecResult, bool> { { ShellSecResult.M_nn, true } };
            info.ScaleResults = new Dictionary<ShellSecResult, double> { { ShellSecResult.M_nn, 1.0 } };
            info.DisplaySmooth = false;
            info.DisplayMesh = true;
            

            var expectedVertices = new List<Point3>()
            {
                new Point3 (0,0,1),
                new Point3 (0,0,0),
                new Point3 (0.5,0,1),
                new Point3 (0.5,0,0),
                new Point3 (0.5,0,2),
                new Point3 (0.5,0,0),
                new Point3 (1,0,2),
                new Point3 (1,0,0),
            };
            var expectedFaces = new Face3[]
            {
                new Face3(0,1,3,2),
                new Face3(4,5,7,6)
            };




            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<IMesh> output = sut.RenderedMeshes;




            Assert.AreEqual(output.Count, 1);            
            Assert.That(output[0].Vertices, Is.EqualTo(expectedVertices));            
            Assert.That(output[0].Faces, Is.EqualTo(expectedFaces));
            
            
        }

        [Test]
        public void Render_DrawSmoothMesh_ForVertexBasedResult()
        {
            ShellSecState state = Utilities.MakeState_VertexBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Displacement();
            info.DisplayResults = new Dictionary<ShellSecResult, bool> { { ShellSecResult.X, true } };
            info.ScaleResults = new Dictionary<ShellSecResult, double> { { ShellSecResult.X, 1.0 } };
            info.DisplaySmooth = true;
            info.DisplayMesh = true;
            info.BaseMeshObject = ShellSec_BaseObj.Vertex;

            var expectedVertices = new Point3[]
            {
                new Point3 (0,0,1),
                new Point3 (0,0,0),
                new Point3 (0.5,0,2),
                new Point3 (0.5,0,0),                
                new Point3 (1,0,3),
                new Point3 (1,0,0),
            };
            var expectedFaces = new List<Face3>()
            {
                new Face3(0,1,3,2),
                new Face3(2,3,5,4)
            };




            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<IMesh> output = sut.RenderedMeshes;



            
            Assert.AreEqual(output.Count, 1);
            Assert.That(output[0].Vertices.ToArray, Is.EqualTo(expectedVertices));
            Assert.That(output[0].Faces, Is.EqualTo(expectedFaces));
        }

        [Test]
        public void Render_DrawSmoothMesh_ForElementBasedResult()
        {
            ShellSecState state = Utilities.MakeState_ElementBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force();
            info.DisplayResults = new Dictionary<ShellSecResult, bool> { { ShellSecResult.M_nn, true } };
            info.ScaleResults = new Dictionary<ShellSecResult, double> { { ShellSecResult.M_nn, 1.0 } };
            info.DisplaySmooth = true;
            info.DisplayMesh = true;
            info.BaseMeshObject = ShellSec_BaseObj.Element;

            var expectedVertices = new Point3[]
            {
                new Point3 (0,0,1),
                new Point3 (0,0,0),
                new Point3 (0.5,0,1.5),
                new Point3 (0.5,0,0),
                new Point3 (1,0,2),
                new Point3 (1,0,0),
            };
            var expectedFaces = new List<Face3>()
            {
                new Face3(0,1,3,2),
                new Face3(2,3,5,4)
            };




            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<IMesh> output = sut.RenderedMeshes;




            Assert.AreEqual(output.Count, 1);
            Assert.That(output[0].Vertices.ToArray, Is.EqualTo(expectedVertices));
            Assert.That(output[0].Faces, Is.EqualTo(expectedFaces));
        }

        [Test]
        public void Render_DrawStepwiseCurve()
        {
            ShellSecState state = Utilities.MakeState_ElementBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force();
            info.DisplayResults = new Dictionary<ShellSecResult, bool> { { ShellSecResult.M_nn, true } };
            info.ScaleResults = new Dictionary<ShellSecResult, double> { { ShellSecResult.M_nn, 1.0 } };
            info.DisplaySmooth = false;

            var expectedCurves = new List<PolyLine3>();
            expectedCurves.Add(new PolyLine3(new Point3(0, 0, 1), new Point3(0.5, 0, 1)));
            expectedCurves.Add(new PolyLine3(new Point3(0.5, 0, 2), new Point3(1, 0, 2)));




            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<List<PolyLine3>> output = sut.RenderedCurves;
            var outputCurves = output.SelectMany(poly => poly);

           
                   

            Assert.That(outputCurves, Is.EqualTo(expectedCurves));
        }

        [Test]
        public void Render_DrawSmoothCurve_ForVertexBasedResult()
        {
            ShellSecState state = Utilities.MakeState_VertexBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Displacement();
            info.DisplayResults[ShellSecResult.X] = true;
            info.ScaleResults[ShellSecResult.X] = 1.0;
            info.DisplaySmooth = true;
            info.BaseMeshObject = ShellSec_BaseObj.Vertex;

            var expectedCurves = new List<PolyLine3>();
            expectedCurves.Add(new PolyLine3(new Point3(0, 0, 1), new Point3(0.5, 0, 2), new Point3(1,0,3)));
           



            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<List<PolyLine3>> output = sut.RenderedCurves;
            var outputCurves = output.SelectMany(poly => poly);




            Assert.That(outputCurves, Is.EqualTo(expectedCurves));
        }

        [Test]
        public void Render_DrawSmoothCurve_ForElementBasedResult()
        {
            ShellSecState state = Utilities.MakeState_ElementBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force();
            info.DisplayResults[ShellSecResult.M_nn] = true;
            info.ScaleResults[ShellSecResult.M_nn] = 1.0;
            info.DisplaySmooth = true;
            info.BaseMeshObject = ShellSec_BaseObj.Element;

            var expectedCurves = new List<PolyLine3>();
            expectedCurves.Add(new PolyLine3(new Point3(0, 0, 1), new Point3(0.5, 0, 1.5), new Point3(1, 0, 2)));




            var sut = new ShellSec_MeshRenderer();
            sut.Render(state, info);
            List<List<PolyLine3>> output = sut.RenderedCurves;
            var outputCurves = output.SelectMany(poly => poly);




            Assert.That(outputCurves, Is.EqualTo(expectedCurves));
        }
    }

    [TestFixture]
    public class ShellSec_NumberRenderer_Tests
    {
        [Test]
        public void Render_DrawStepwiseNumber() 
        {
            ShellSecState state = Utilities.MakeState_ElementBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force();
            info.DisplayResults[ShellSecResult.M_nn] = true;
            info.ScaleResults[ShellSecResult.M_nn] = 1.0;
            info.DisplaySmooth = false;
            info.TextFormat = "{0:f}";

            var mock = Substitute.For<IDrawViewBehaviour>();

            var expectedPosition = new Point3[] { new Point3(0.25, 0, 1), new Point3(0.75, 0, 2) };
            var expectedText = new string[] { "1.00", "2.00" };




            var sut = new ShellSec_NumberRenderer(mock);
            sut.Render(state, info);




            Assert.That(sut.Positions, Is.EqualTo(expectedPosition));
            Assert.That(sut.Texts, Is.EqualTo(expectedText));
            mock.Received().DrawView();
        }

        [Test]
        public void Render_DrawSmoothNumber_ForVertexBased()
        {
            ShellSecState state = Utilities.MakeState_VertexBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Displacement();
            info.DisplayResults[ShellSecResult.X] = true;
            info.ScaleResults[ShellSecResult.X] = 1.0;
            info.DisplaySmooth = true;
            info.TextFormat = "{0:f}";

            var mock = Substitute.For<IDrawViewBehaviour>();

            var expectedPosition = new Point3[] { new Point3(0, 0, 1), new Point3(0.5, 0, 2), new Point3(1, 0, 3) };
            var expectedText = new string[] { "1.00", "2.00", "3.00" };




            var sut = new ShellSec_NumberRenderer(mock);
            sut.Render(state, info);

            




            Assert.That(sut.Positions, Is.EqualTo(expectedPosition));
            Assert.That(sut.Texts, Is.EqualTo(expectedText));
            mock.Received().DrawView();
        }

        [Test]
        public void Render_DrawSmoothNumber_ForElementBased()
        {
            ShellSecState state = Utilities.MakeState_ElementBased();

            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force();
            info.DisplayResults[ShellSecResult.M_nn] = true;
            info.ScaleResults[ShellSecResult.M_nn] = 1.0;
            info.DisplaySmooth = true;
            info.TextFormat = "{0:f}";

            var mock = Substitute.For<IDrawViewBehaviour>();

            var expectedPosition = new Point3[] { new Point3(0, 0, 1), new Point3(0.5, 0, 1.5), new Point3(1, 0, 2) };
            var expectedText = new string[] { "1.00", "1.50", "2.00" };




            var sut = new ShellSec_NumberRenderer(mock);
            sut.Render(state, info);




            Assert.That(sut.Positions, Is.EqualTo(expectedPosition));
            Assert.That(sut.Texts, Is.EqualTo(expectedText));
            mock.Received().DrawView();
        }
    }


    [TestFixture]
    public class ShellSec_LocalAxesRenderer_Test
    {
        [Test]
        public void TestMethod()
        {
            ShellSecState state = Utilities.MakeState_ElementBased();
            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force();
            var mock = Substitute.For<IDrawViewBehaviour>();            

            var expectedOrigins = new Point3[] { new Point3(0.25, 0, 0), new Point3(0.75, 0, 0)};
            var expectedAxes = new List<Vector3[]> 
            {   
                new Vector3[] 
                { 
                    new Vector3(1, 0, 0), 
                    new Vector3(0, -1, 0),
                    new Vector3(0, 0, 1)
                },
                 new Vector3[]
                {
                    new Vector3(1, 0, 0),
                    new Vector3(0, -1, 0),
                    new Vector3(0, 0, 1)
                }
            };




            var sut = new ShellSec_LocalAxesRenderer(mock);
            sut.Render(state, info);




            Assert.That(sut.Origins, Is.EqualTo(expectedOrigins));
            Assert.That(sut.AxesList, Is.EqualTo(expectedAxes));
            mock.Received().DrawView();
        }
    }
}
