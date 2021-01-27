using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Supports;
using Karamba.Utilities;
using Karamba.Algorithms;

namespace KarambaCommon.Tests.Algorithms
{
    [TestFixture]
    public class NaturalVibes_tests
    {
#if ALL_TESTS
        [Test]
        public void Beam()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            var resourcePath = @"";

            // get a material from the material table in the folder 'Resources'
            var materialPath = Path.Combine(resourcePath, "Materials/MaterialProperties.csv");
            var inMaterials = k3d.Material.ReadMaterialTable(materialPath);
            var material = inMaterials.Find(x => x.name == "Steel");

            // get a cross section from the cross section table in the folder 'Resources'
            var crosecPath = Path.Combine(resourcePath, "CrossSections/CrossSectionValues.bin");
            CroSecTable inCroSecs = k3d.CroSec.ReadCrossSectionTable(crosecPath, out var info);
            var crosec_family = inCroSecs.crosecs.FindAll(x => x.family == "FRQ");
            var crosec_initial = crosec_family.Find(x => x.name == "FRQ45/5");

            // attach the material to the cross section
            crosec_initial.setMaterial(material);

            // create the column
            var beams = k3d.Part.LineToBeam(new List<Line3> { axis }, new List<string>() { "B1" }, new List<CroSec>() { crosec_initial }, logger,
                out var out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() {true, true, true, true, false, false}),
                k3d.Support.Support(p1, new List<bool>() {false, true, true, false, false, false})
            };

            // create a Point-load
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(p1, new Vector3(0, 0, -100))
            };

            // create the model
            var model = k3d.Model.AssembleModel(beams, supports, loads,
                out info, out var mass, out var cog, out var message, out var warning);

            // calculate the natural vibrations
            int from_shape_ind = 1;
            int shapes_num = 1;
            int max_iter = 100;
            double eps = 1e-8;
            var disp_dummy = new List<double>();
            var scaling = EigenShapesScalingType.matrix;
            model = k3d.Algorithms.NaturalVibes(model, from_shape_ind, shapes_num, max_iter, eps, disp_dummy, scaling,
                out List<double> nat_frequencies, out List<double> modal_masses, out List<Vector3> participation_facs,
                out List<double> participation_facs_disp, out model);

            Assert.AreEqual(nat_frequencies[0], 8.9828263788644716, 1e-8);
        }
#endif
#if ALL_TESTS
        [Test]
        public void Shell()
        {
            var k3d = new Toolkit();
            var logger = new MessageLogger();
            double length = 1.0;
            var mesh = new Mesh3();
            var p0 = new Point3(0, length, 0);
            var p1 = new Point3(0, 0, 0);
            mesh.AddVertex(p0);
            mesh.AddVertex(p1);
            mesh.AddVertex(new Point3(length, 0, 0));
            mesh.AddVertex(new Point3(length, length, 0));
            mesh.AddFace(new Face3(0, 1, 3));
            mesh.AddFace(new Face3(1, 2, 3));

            var crosec = k3d.CroSec.ShellConst(25, 0);
            var shells = k3d.Part.MeshToShell(new List<Mesh3> { mesh }, null, new List<CroSec> { crosec }, logger, out var nodes);
            
            
            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() {true, true, true, true, true, true}),
                k3d.Support.Support(p1, new List<bool>() {true, true, true, true, true, true}),
            };


            // create the model
            var model = k3d.Model.AssembleModel(shells, supports, new List<Load>(),
                out var info, out var mass, out var cog, out var message, out var warning);

            // calculate the natural vibrations
            int from_shape_ind = 1;
            int shapes_num = 1;
            int max_iter = 100;
            double eps = 1e-8;
            var disp_dummy = new List<double>();
            var scaling = EigenShapesScalingType.matrix;
            model = k3d.Algorithms.NaturalVibes(model, from_shape_ind, shapes_num, max_iter, eps, disp_dummy, scaling,
                out List<double> nat_frequencies, out List<double> modal_masses, out List<Vector3> participation_facs,
                out List<double> participation_facs_disp, out model);

            Assert.AreEqual(nat_frequencies[0], 624.44918408731951, 1e-8);
        }
#endif
    }
}
