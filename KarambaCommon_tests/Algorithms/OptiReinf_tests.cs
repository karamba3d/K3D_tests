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

namespace KarambaCommon.Tests.Algorithms
{
    [TestFixture]
    public class OptiReinf_tests
    {
#if ALL_TESTS
        [Test]
        public void ReinforcedPlate()
        {
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

            var crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(25, 0, null, new List<double> { 4, 4, -4, -4 }, 0);
            var shells = k3d.Part.MeshToShell(new List<Mesh3> { mesh }, null, new List<CroSec> { crosec }, logger, out var nodes);

            // create supports
            var supportConditions = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(0, supportConditions),
                k3d.Support.Support(1, supportConditions),
            };

            // create a Point-load
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(2, new Vector3(), new Vector3(0, 25, 0)),
                k3d.Load.PointLoad(3, new Vector3(), new Vector3(0, 25, 0))
            };

            // create the model
            var model = k3d.Model.AssembleModel(shells, supports, loads,
                out var info, out var mass, out var cog, out var message, out var warning);

            model = k3d.Algorithms.OptiReinf(model,
            out List<double> maxDisplacements, out List<double> compliances, out message, out double reinfMass);

            k3d.Results.ShellForcesLocal(model, null, 0,
                out var nxx, out var nyy, out var nxy, 
                out var mxx, out var myy, out var mxy, 
                out var vx, out var vy);
            Assert.AreEqual(mxx[0][0], 50.082245640312429, 1E-5);
            Assert.AreEqual(mxx[0][1], 49.91775435968767, 1E-5);
            
            var crosec_shell = model.elems[0].crosec as CroSec_Shell;
            var reinf_thick = crosec_shell.elem_crosecs[0].layers[1].height;
            Assert.AreEqual(reinf_thick, 0.00046824411288599481, 1E-5);
        }
#endif
    }
}
