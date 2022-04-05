#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Loads;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class ModifyModelTests
    {
        [Test]
        public void ModifyModel()
        {
            var k3d = new Toolkit();

            var crosec = k3d.CroSec.CircularHollow();
            crosec.Az = 1E20; // make the cross section very stiff in shear

            var elems = new List<BuilderBeam>()
            {
                k3d.Part.IndexToBeam(0, 1, string.Empty, crosec), k3d.Part.IndexToBeam(1, 2, string.Empty, crosec),
            };

            var l = 10.0; // in meter
            var points = new List<Point3> { new Point3(), new Point3(l * 0.5, 0, 0), new Point3(l, 0, 0) };

            var supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportHingedConditions),
                k3d.Support.Support(2, k3d.Support.SupportHingedConditions),
            };

            var fz = -0.1; // in kN
            var loads = new List<Load> { k3d.Load.PointLoad(1, new Vector3(0, 0, fz)), };

            var model = k3d.Model.AssembleModel(
                elems,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var msg,
                out var runtimeWarning,
                new List<Joint>(),
                points);

            AnalyzeThI.solve(model, out var outMaxDisp, out var outG, out var outComp, out var warning, out model);

            var i = (model.elems[0].crosec as CroSec_Beam).Iyy;
            var e = model.elems[0].crosec.material.E();
            var dispTarget = Math.Abs(fz) * l * l * l / (48 * e * i);

            Assert.That(dispTarget, Is.EqualTo(outMaxDisp[0]).Within(1E-10));
        }
    }
}

#endif