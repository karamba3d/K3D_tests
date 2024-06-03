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
    public class ModifyModelTests
    {
        [Test]
        public void ModifyModel()
        {
            Toolkit k3d = new Toolkit();

            CroSec_Circle crosec = k3d.CroSec.CircularHollow();
            crosec.Az = 1E20; // make the cross section very stiff in shear

            List<BuilderBeam> elems = new List<BuilderBeam>()
            {
                k3d.Part.IndexToBeam(0, 1, string.Empty, crosec), k3d.Part.IndexToBeam(1, 2, string.Empty, crosec),
            };

            double l = 10.0; // in meter
            List<Point3> points = new List<Point3> { new Point3(), new Point3(l * 0.5, 0, 0), new Point3(l, 0, 0) };

            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportHingedConditions),
                k3d.Support.Support(2, k3d.Support.SupportHingedConditions),
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

            AnalyzeThI.solve(model, out IReadOnlyList<double> outMaxDisp, out IReadOnlyList<double> outG, out IReadOnlyList<double> outComp, out string warning, out model);

            double i = (model.elems[0].crosec as CroSec_Beam).Iyy;
            double e = model.elems[0].crosec.material.E();
            double dispTarget = Math.Abs(fz) * l * l * l / (48 * e * i);

            Assert.That(dispTarget, Is.EqualTo(outMaxDisp[0]).Within(1E-10));
        }
    }
}

#endif
