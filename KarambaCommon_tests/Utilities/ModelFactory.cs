namespace KarambaCommon.Tests.Utilities
{
    using System.Collections.Generic;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using TestableBeamProperties = KarambaCommon.Tests.Helpers.TestableBeamProperties;

    public static class ModelFactory
    {
        /// <summary>
        /// Creates a model with one single <see cref="ModelBeam"/>.
        /// </summary>
        /// <param name="start">Beam's starting point.</param>
        /// <param name="end">Beam's ending point.</param>
        /// <param name="properties">Beam's properties.</param>
        /// <returns>
        /// <see cref="Model"/> with one <see cref="ModelBeam"/>.
        /// </returns>
        public static Model CreateOneBeamModel(Point3 start, Point3 end, TestableBeamProperties properties)
        {
            var logger = new MessageLogger();
            var k3d = new Toolkit();

            var beam = k3d.Part.LineToBeam(
                new Line3(new[] { start, end }),
                properties.ID,
                properties.CrossSection,
                info: logger,
                outNodes: out _);

            var model = k3d.Model.AssembleModel(
                beam,
                (IReadOnlyList<Support>)properties.Supports,
                (List<Load>)properties.Loads,
                info: out _,
                mass: out _,
                cog: out _,
                msg: out _,
                runtimeWarning: out _);

            return model;
        }
    }
}
