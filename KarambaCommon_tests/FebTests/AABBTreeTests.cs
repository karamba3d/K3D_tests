#if ALL_TESTS
namespace KarambaCommon.Tests.Algorithms
{
    using System.Collections.Generic;
    using feb;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;
    using Load = Karamba.Loads.Load;
    using Helper = KarambaCommon.Tests.Helpers.Helper;

    [TestFixture]
    public class AabbTreeTests
    {
        [Test]
        public void TwoTriangles()
        {
            IniConfig.DefaultUnits(); // X move to a better place.
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();

            double length = 1.0;
            var mesh = new Mesh3();
            mesh.AddVertex(new Point3(0, length, 0));
            mesh.AddVertex(new Point3(0, 0, 0));
            mesh.AddVertex(new Point3(length, 0, 0));
            mesh.AddVertex(new Point3(length, length, 0));
            mesh.AddFace(new Face3(0, 1, 3));
            mesh.AddFace(new Face3(1, 2, 3));

            CroSec_Shell crosec0 = k3d.CroSec.ShellVariable(new List<double> { 0.1, 0.2 });
            string shellName = "s1";
            List<BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                new List<string>() { shellName },
                new List<CroSec> { crosec0 },
                logger,
                out List<Point3> _);

            // create supports
            var supportConditions = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(0, supportConditions), k3d.Support.Support(1, supportConditions),
            };

            // create a Point-load
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(2, new Vector3(), new Vector3(0, 25, 0)),
                k3d.Load.PointLoad(3, new Vector3(), new Vector3(0, 25, 0)),
            };

            // create the model
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out string _,
                out double _,
                out Point3 _,
                out string _,
                out bool _);

            var meshGrp = new feb.ShellMesh();
            var elemInds = model.elemId2elemInd.Select(shellName);
            foreach (var elemInd in elemInds)
            {
                switch (model.elems[elemInd])
                {
                    case ModelMembrane membrane:
                    {
                        meshGrp.add(model.febmodel.triMesh(membrane.fe_id));
                        break;
                    }
                }
            }

            meshGrp.finalizeConstruction();

            var aabbTree = new feb.AABBTree();
            aabbTree.add(meshGrp.mesh());
            aabbTree.build();

            double tolerance = 1E-8;
            VectIndexes inds = new VectIndexes();

            //---
            var p0 = new feb.Point3d(mesh.Vertices[0].vec3d, tolerance);
            aabbTree.intersect(p0, inds);
            Assert.That(inds.Count, Is.EqualTo(1));
            Assert.That(inds[0], Is.EqualTo(0));

            // access element in mesh_grp
            var t = meshGrp.elem(inds[0]).crosec().asSurface3DCroSec().h();
            Assert.That(t, Is.EqualTo(0.1));
            inds.Clear(); // resize to 0 again

            //---
            var p1 = new feb.Point3d(mesh.Vertices[1].vec3d, tolerance);
            aabbTree.intersect(p1, inds);
            Assert.That(inds.Count, Is.EqualTo(2));

            // the order is not guaranteed
            Assert.That(inds[0], Is.EqualTo(1));
            Assert.That(inds[1], Is.EqualTo(0));

            // access element in mesh_grp
            t = meshGrp.elem(inds[0]).crosec().asSurface3DCroSec().h();
            Assert.That(t, Is.EqualTo(0.2));
            t = meshGrp.elem(inds[1]).crosec().asSurface3DCroSec().h();
            Assert.That(t, Is.EqualTo(0.1));

            inds.Clear(); // resize to 0 again

            var p2 = new feb.Point3d(mesh.Vertices[2].vec3d, tolerance);
            aabbTree.intersect(p2, inds);
            Assert.That(inds.Count, Is.EqualTo(1));
            Assert.That(inds[0], Is.EqualTo(1));
            inds.Clear(); // resize to 0 again

            var p3 = new feb.Point3d(mesh.Vertices[1].vec3d, tolerance);
            aabbTree.intersect(p3, inds);
            Assert.That(inds.Count, Is.EqualTo(2));

            // the order is not guaranteed
            Assert.That(inds[0], Is.EqualTo(1));
            Assert.That(inds[1], Is.EqualTo(0));
            inds.Clear(); // resize to 0 again

            var p4 = new feb.Point3d(new Point3(length * 0.5, length * 0.75, 0).vec3d, tolerance);
            aabbTree.intersect(p4, inds);
            Assert.That(inds.Count, Is.EqualTo(1));
            Assert.That(inds[0], Is.EqualTo(0));
            inds.Clear(); // resize to 0 again

            var p5 = new feb.Point3d(new Point3(length * 0.5, length * 0.5, 0).vec3d, tolerance);
            aabbTree.intersect(p5, inds);
            Assert.That(inds.Count, Is.EqualTo(2));

            // the order is not guaranteed
            Assert.That(inds[0], Is.EqualTo(1));
            Assert.That(inds[1], Is.EqualTo(0));
            inds.Clear(); // resize to 0 again

            var p6 = new feb.Point3d(new Point3(length * 0.5, length * 0.25, 0).vec3d, tolerance);
            aabbTree.intersect(p6, inds);
            Assert.That(inds.Count, Is.EqualTo(1));
            Assert.That(inds[0], Is.EqualTo(1));
            inds.Clear(); // resize to 0 again

            var p7 = new feb.Point3d(new Point3(length * 2.0, length * 2.0, 0).vec3d, tolerance);
            aabbTree.intersect(p7, inds);
            Assert.That(inds.Count, Is.EqualTo(0));
        }
    }
}
#endif
