namespace NUnitLite.Tests.Helpers
{
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Factories;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Beam;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;

    public static class BeamFactory
    {
        /// <summary>
        /// Create a simple hinged beam model on the x-axis.
        /// </summary>
        /// <param name="beamLength">Length of the beam.</param>
        /// <param name="loads">Loads applied to the beam.</param>
        /// <param name="croSec">Cross section of the beam.</param>
        /// <returns></returns>
        public static Model CreateHingedBeam(double beamLength, List<Load> loads, CroSec croSec)
        {
            var logger = new MessageLogger();
            var k3d = new Toolkit();

            var beam = k3d.Part.LineToBeam(
                    line: new Line3(
                            new Point3(0, 0, 0),
                            new Point3(beamLength, 0, 0)),
                    id: string.Empty,
                    crosec: croSec,
                    info: logger,
                    out _);

            var hc0 = k3d.Support.SupportHingedConditions;
            var hc1 = new List<bool> { false, true, true, true, false, false };
            var supports = new List<Support>
            {
                k3d.Support.Support(0, hc0),
                k3d.Support.Support(1, hc1),
            };

            var model = k3d.Model.AssembleModel(
                    beam,
                    supports,
                    loads,
                    out _,
                    out _,
                    out _,
                    out _,
                    out _);

            return model;
        }

        /// <summary>
        /// Shorthand method for creating a hinged beam with one load only.
        /// </summary>
        /// <param name="beamLength">length of the beam.</param>
        /// <param name="load">load to be applied.</param>
        /// <param name="croSec">cross section to be used.</param>
        /// <returns> a model of a hinged beam</returns>
        public static Model CreateHingedBeam(double beamLength, Load load, CroSec croSec) => CreateHingedBeam(beamLength, new List<Load>(1) { load }, croSec);

        /// <summary>
        /// Create a cantilever beam model on the x-axis.
        /// </summary>
        /// <param name="beamLength">Length of the beam.</param>
        /// <param name="loads">Loads applied to the beam.</param>
        /// <param name="croSec">Cross section of the beam.</param>
        /// <returns>Model of a cantilever beam.</returns>
        public static Model CreateCantileverBeam(double beamLength, List<Load> loads, CroSec croSec)
        {
            var logger = new MessageLogger();
            var k3d = new Toolkit();

            var beam = k3d.Part.LineToBeam(
                line: new Line3(
                    new Point3(0, 0, 0),
                    new Point3(beamLength, 0, 0)),
                id: string.Empty,
                crosec: croSec,
                info: logger,
                out _);

            var supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportFixedConditions),
            };

            var model = k3d.Model.AssembleModel(
                beam,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _);

            return model;
        }

        /// <summary>
        /// Shorthand method for creating a cantilever beam with one load only.
        /// </summary>
        /// <param name="beamLength">length of the beam.</param>
        /// <param name="load">load to be applied.</param>
        /// <param name="croSec">cross section to be used.</param>
        /// <returns>Model of a cantilever beam.</returns>
        public static Model CreateCantileverBeam(double beamLength, Load load, CroSec croSec) => CreateCantileverBeam(beamLength, new List<Load>(1) { load }, croSec);

        /// <summary>
        /// Create a beam model on the x-axis which is fully fixed at both sides.
        /// </summary>
        /// <param name="beamLength">Length of the beam.</param>
        /// <param name="loads">Loads applied to the beam.</param>
        /// <param name="croSec">Cross section of the beam.</param>
        /// <returns>Model of a cantilever beam.</returns>
        public static Model CreateFixedFixedBeam(double beamLength, List<Load> loads, CroSec croSec)
        {
            var logger = new MessageLogger();
            var k3d = new Toolkit();

            var beam = k3d.Part.LineToBeam(
                line: new Line3(
                    new Point3(0, 0, 0),
                    new Point3(beamLength, 0, 0)),
                id: string.Empty,
                crosec: croSec,
                info: logger,
                out _);

            var supports = new List<Support>
            {
                k3d.Support.Support(0, k3d.Support.SupportFixedConditions),
                k3d.Support.Support(1, k3d.Support.SupportFixedConditions),
            };

            var model = k3d.Model.AssembleModel(
                beam,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _);

            return model;
        }

        /// <summary>
        /// Shorthand method for creating a cantilever beam with one load only.
        /// </summary>
        /// <param name="beamLength">length of the beam.</param>
        /// <param name="load">load to be applied.</param>
        /// <param name="croSec">cross section to be used.</param>
        /// <returns>Model of a cantilever beam.</returns>
        public static Model CreateFixedFixedBeam(double beamLength, Load load, CroSec croSec) => CreateFixedFixedBeam(beamLength, new List<Load>(1) { load }, croSec);
    }
}