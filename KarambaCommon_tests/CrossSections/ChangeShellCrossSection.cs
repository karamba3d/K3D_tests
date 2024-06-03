#if ALL_TESTS
namespace KarambaCommon.Tests.CrossSections
{
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture]
    public class ChangeShellCrossSections
    {
        // [Test]
        public void RectangleValues()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();

            // create a material
            double gamma = 1.0;
            List<FemMaterial> materials = new List<FemMaterial>
            {
                new Karamba.Materials.FemMaterial_Isotrop(
                    "family",
                    "name",
                    210000000,
                    10000,
                    10000,
                    gamma,
                    10,
                    -10,
                    FemMaterial.FlowHypothesis.mises,
                    1e-4,
                    null,
                    out _),
            };

            // create a cross section
            double height = 1;
            CroSec_Shell crosec = k3d.CroSec.ShellConst(height, 0, materials[0]);

            // create an element
            double length = 1.0;
            double area = length * length;
            Mesh3 mesh = new Mesh3();
            mesh.AddVertex(new Point3(0, -length, 0));
            mesh.AddVertex(new Point3(length, 0, 0));
            mesh.AddVertex(new Point3(0, length, 0));
            mesh.AddFace(new Face3(0, 1, 2));

            List<BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out List<Point3> nodes);

            // create supports
            List<bool> supportConditions = new List<bool>() { true, true, true, true, true, true };
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(0, supportConditions), k3d.Support.Support(2, supportConditions),
            };

            // create a gravity-load
            List<Load> loads = new List<Load>() { k3d.Load.GravityLoad(new Vector3(0, 0, 1)), };

            // create the model
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out string info,
                out double mass,
                out Point3 cog,
                out string message,
                out bool warning);

            // calculate Th.I response
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out var outMaxDisp,
                out var outG,
                out var outComp,
                out message);

            var maxDispTarget = 0.87483333333405078;
            Assert.That(outMaxDisp[0], Is.EqualTo(maxDispTarget).Within(1E-10));

            // get the weight of the element
            var shell = model.elems[0] as Karamba.Elements.ModelMembrane;
            feb.ShellMesh triMesh = shell.feMesh(model);
            var feShellElement = triMesh.elem(0);
            var weight0 = feShellElement.weight();

            // clone the model to avoid side-effects
            model = model.Clone();
            model.deepCloneFEModel();

            // set the analysis mode
            var analysis = new feb.Deform(model.febmodel);
            var response = new feb.Response(analysis);

            // change cross section in the C# model
            shell = model.elems[0] as Karamba.Elements.ModelMembrane;
            var shellCrosec = (Karamba.CrossSections.CroSec_Shell)shell.crosec.Clone();
            crosec.clearAndSetDefault();
            shell.crosec = crosec;

            // change the cross section in the feb model
            triMesh = shell.feMesh(model);
            const double factor = 0.5;
            for (int elemInd = 0; elemInd < triMesh.numberOfElems(); elemInd++)
            {
                feShellElement = triMesh.elem(elemInd);
                var newHeight = feShellElement.crosec().asSurface3DCroSec().h(0) * factor;
                crosec.setHeight(elemInd, newHeight);
            }

            model.febmodel.touch();

            // update the model response
            response.updateMemberForces();

            // get the new weight of the element
            feShellElement = triMesh.elem(0);
            var weight1 = feShellElement.weight();

            Assert.That(weight0, Is.EqualTo(weight1 * factor).Within(1E-10));
        }
    }
}
#endif
