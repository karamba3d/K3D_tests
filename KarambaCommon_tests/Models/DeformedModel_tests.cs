#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Models;
    using Karamba.Supports;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture]
    public class DeformedModelTests
    {
        [Test]
        public void DeformedModel_FromLoad()
        {
            Toolkit k3d = new Toolkit();

            CroSec_Circle crosec = k3d.CroSec.CircularHollow();
            crosec.Az = 1E20; // make the cross section very stiff in shear

            List<BuilderBeam> elems = new List<BuilderBeam>()
            {
                k3d.Part.IndexToBeam(0, 1, string.Empty, crosec),
            };

            double l = 10.0; // in meter
            List<Point3> points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };

            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportFixedConditions),
            };

            double fz = -0.1; // in kN
            List<Load> loads = new List<Load>
            {
                k3d.Load.PointLoad(1, new Vector3(0, 0, 2 * fz), new Vector3(0, 0, 0), "LC0"),
                k3d.Load.PointLoad(1, new Vector3(0, 0, fz), new Vector3(0, 0, 0), "LC1"),
            };

            Model model = k3d.Model.AssembleModel(
                elems,
                supports,
                loads,
                out string info,
                out double mass,
                out Point3 cog,
                out string msg,
                out bool runtimeWarning,
                new List<Joint>(),
                points);

            int febLCNum0 = model.febmodel.numberOfLoadCases();
            Analyze.solve(model, new List<string> { "LC1" }, out var outMaxDisp, out var outF, out var outComp, out model, out string warning);
            int febLCNum1 = model.febmodel.numberOfLoadCases();

            double i = (model.elems[0].crosec as CroSec_Beam).Iyy;
            double e = model.elems[0].crosec.material.E();
            double dispTarget = Math.Abs(fz) * l * l * l / (3 * e * i);
            Assert.That(dispTarget, Is.EqualTo(outMaxDisp[0]).Within(1E-10));

            // Create deformed model
            model.dp.SetDisplayLoadCase("LC1/0");

            var curves = new List<PolyLine3>();
            var meshes = new List<IMesh>();

            Model deformedModel = null;
            model.dp.basic.DisplacementScale = 1.0;
            ModelResult.CollectDeformedGeometry(model, curves, meshes, out deformedModel, out var updatedModel);

            var pos0 = model.nodes[1].pos;
            var pos1 = deformedModel.nodes[1].pos;
            double disp = pos1.Z - pos0.Z;
            Assert.That(dispTarget, Is.EqualTo(Math.Abs(disp)).Within(1E-10));
        }

        [Test]
        public void DeformedModel_FromEigenmodes()
        {
            Toolkit k3d = new Toolkit();

            CroSec_Circle crosec = k3d.CroSec.CircularHollow();
            crosec.Az = 1E20; // make the cross section very stiff in shear

            List<BuilderBeam> elems = new List<BuilderBeam>()
            {
                k3d.Part.IndexToBeam(0, 1, string.Empty, crosec),
            };

            double l = 10.0; // in meter
            List<Point3> points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };

            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportFixedConditions),
            };

            double fz = -0.1; // in kN
            List<Load> loads = new List<Load> { k3d.Load.PointLoad(1, new Vector3(0, 0, fz)), };

            Model model = k3d.Model.AssembleModel(
                elems,
                supports,
                loads,
                out string info,
                out double mass,
                out Point3 cog,
                out string msg,
                out bool runtimeWarning,
                new List<Joint>(),
                points);

            int febLCNum0 = model.febmodel.numberOfLoadCases();
            EigenModes.solve(model, 1, 2, 10, 1e-8, 1, out List<double> eigenVals, out model, out msg);
            int febLCNum1 = model.febmodel.numberOfLoadCases();
        }
    }
}

#endif
