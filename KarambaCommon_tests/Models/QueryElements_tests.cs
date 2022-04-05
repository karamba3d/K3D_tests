namespace KarambaCommon.Tests.Model
{
    /*
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class QueryElements_tests
    {
#if ALL_TESTS

        // /*
        [Test]
        public void QueryTest()
        {
            var k3d = new Toolkit();

            var beam_id_A = "A";
            var beam_id_B = "B";
            var beam_id_C = "C";
            var matA = new FemMaterial_Isotrop("test", "matA", 1, 1, 1, 1, 1, 1, FemMaterial.FlowHypothesis.mises, 1,
                null);
            var matB = new FemMaterial_Isotrop("test", "matB", 1, 1, 1, 1, 1, 1, FemMaterial.FlowHypothesis.mises, 1,
                null);
            var croSecA = new CroSec_I("test", "croSecA", null, null, matA);
            var croSecB = new CroSec_I("test", "croSecB", null, null, matB);
            var croSecC = new CroSec_I("test", "croSecC", null, null, matB);

            //# build elements
            var elems = new List<BuilderBeam>()
            {
                //# ID, CroSec, Mat
                k3d.Part.IndexToBeam(0, 1, beam_id_A, croSecA), // A,A,A
                k3d.Part.IndexToBeam(0, 1, beam_id_A, croSecC), // A,C,B
                k3d.Part.IndexToBeam(0, 1, beam_id_B, croSecA), // B,A,A
                k3d.Part.IndexToBeam(0, 1, beam_id_B, croSecA), // B,A,A
                k3d.Part.IndexToBeam(0, 1, beam_id_A, croSecB), // A,B,B
                k3d.Part.IndexToBeam(0, 1, beam_id_C, croSecB), // C,B,B
                k3d.Part.IndexToBeam(0, 1, beam_id_A, croSecB), // A,B,B
            };

            //# assemble model
            var points = new List<Point3> {new Point3(), new Point3(10, 0, 0)};
            var supports = new List<Support> {k3d.Support.Support(0, k3d.Support.SupportFixedConditions)};

            var loads = new List<Load>
            {
                k3d.Load.PointLoad(points[1], new Vector3(0, 0, -1))
            };

            var model = k3d.Model.AssembleModel(elems, supports, loads, out var info, out var mass, out var cog,
                out var msg,
                out var runtimeWarning, new List<Joint>(), points);

            //# arguments
            var args1 = new IteratorArgs(new List<string> {beam_id_A}, null, null);
            var args2 = new IteratorArgs(new List<string> {beam_id_A}, croSecA, matA);
            var args3 = new IteratorArgs(new List<string> {beam_id_A}, null, matB);
            var args4 = new IteratorArgs(new List<string> {beam_id_C}, null, matA);
            var args5 = new IteratorArgs(new List<string>(), null, null);
            var args6 = new IteratorArgs(new List<string>(), null, matB);
            var args7 = new IteratorArgs(new List<string>(), croSecC, null);

            //# query for arguments
            var res1 = model.ElemIter(args1);
            var res2 = model.ElemIter(args2);
            var res3 = model.ElemIter(args3);
            var res4 = model.ElemIter(args4);
            var res5 = model.ElemIter(args5);
            var res6 = model.ElemIter(args6);
            var res7 = model.ElemIter(args7);

            //# results
            Assert.That(res1.Count() == 4);
            Assert.That(res2.Count() == 1);
            Assert.That(res3.Count() == 3);
            Assert.That(res4.Count() == 0);
            Assert.That(res5.Count() == 7);
            Assert.That(res6.Count() == 4);
            Assert.That(res7.Count() == 1);
        }
        //
#endif
    }
*/
}
