#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;

    public class ModelDisplay_Tests
    {
        private Karamba.Models.Model CreateModel()
        {
            Toolkit k3d = new Toolkit();

            Line3 line = new Line3
            {
                PointAtStart = new Point3(0, 0, 0),
                PointAtEnd = new Point3(10, 0, 0),
            };

            MessageLogger logger = new MessageLogger();
            CroSec_Circle crossSec = new CroSec_Circle();
            List<Karamba.Elements.BuilderBeam> beam = k3d.Part.LineToBeam(line, "anyId", crossSec, logger, out _);

            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportHingedConditions),
                k3d.Support.Support(1, k3d.Support.SupportHingedConditions),
            };

            List<Load> loads = new List<Load>()
            {
                k3d.Load.GravityLoad(new Vector3(0, 0, 1)),
            };

            Karamba.Models.Model model = k3d.Model.AssembleModel(
                beam,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _);

            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out _);

            return model;
        }
    }
}

#endif