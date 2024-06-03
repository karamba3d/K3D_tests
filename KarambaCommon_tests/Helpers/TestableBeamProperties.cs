namespace KarambaCommon.Tests.Helpers
{
    using System.Collections.Generic;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Supports;
    using KarambaCommon;

    /// <summary>
    /// Class that collects all the properties to be used to create
    /// a <see cref="ModelBeam"/> to be used for testing.
    /// </summary>
    public class TestableBeamProperties
    {
        private static readonly Toolkit Toolkit = new Toolkit();

        public IList<Load> Loads { get; set; } = new List<Load>();

        public CroSec CrossSection { get; set; }

        public FemMaterial Material { get; set; }

        public IList<Support> Supports { get; set; }

        public string ID { get; set; } = "HingedBeam";

        private TestableBeamProperties()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestableBeamProperties"/>.<br/>
        /// Supports[0] => { tx = 1, ty = 1, tz = 1, rx = 0, ry = 0, rz = 0 }<br/>
        /// Supports[1] => { tx = 1, ty = 1, tz = 1, rx = 1, ry = 0, rz = 0 }<br/>
        /// Material => Steel S235<br/>
        /// Cross Section => Germany RO114.3/4<br/>
        /// </summary>
        /// <returns>
        /// A new <see cref="TestableBeamProperties"/>.
        /// </returns>
        public static TestableBeamProperties HingedBeamDefaultProperties()
        {
            return new TestableBeamProperties()
            {
                CrossSection = Toolkit.CroSec.CircularHollow(),
                Material = Toolkit.Material.IsotropicMaterial(
                    "Steel",
                    "S235",
                    210000,
                    8076,
                    8076,
                    78.5,
                    23.5,
                    -23.5,
                    FemMaterial.FlowHypothesis.mises,
                    1.2E-5),
                Supports = new[]
                {
                    Toolkit.Support.Support(0, new[] { true, true, true, false, false, false }),
                    Toolkit.Support.Support(1, new[] { true, true, true, true, false, false }),
                },
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestableBeamProperties"/>.<br/>
        /// Supports[0] => { tx = 1, ty = 1, tz = 1, rx = 1, ry = 1, rz = 1 }<br/>
        /// Material => Steel S235<br/>
        /// Cross Section => Germany RO114.3/4<br/>
        /// </summary>
        /// <returns>
        /// A new <see cref="TestableBeamProperties"/>.
        /// </returns>
        public static TestableBeamProperties CantileverBeamDefaultProperties()
        {
            return new TestableBeamProperties()
            {
                CrossSection = Toolkit.CroSec.CircularHollow(),
                Material = Toolkit.Material.IsotropicMaterial(
                    "Steel",
                    "S235",
                    210000,
                    8076,
                    8076,
                    78.5,
                    23.5,
                    -23.5,
                    FemMaterial.FlowHypothesis.mises,
                    1.2E-5),
                Supports = new[]
                {
                    Toolkit.Support.Support(0, new[] { true, true, true, true, true, true }),
                },
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestableBeamProperties"/>.<br/>
        /// Supports[0] => { tx = 1, ty = 1, tz = 1, rx = 1, ry = 1, rz = 1 }<br/>
        /// Supports[1] => { tx = 1, ty = 1, tz = 1, rx = 1, ry = 1, rz = 1 }<br/>
        /// Material => Steel S235<br/>
        /// Cross Section => Germany RO114.3/4<br/>
        /// </summary>
        /// <returns>
        /// A new <see cref="TestableBeamProperties"/>.
        /// </returns>
        public static TestableBeamProperties FixedFixedBeamDefaultProperties()
        {
            return new TestableBeamProperties()
            {
                CrossSection = Toolkit.CroSec.CircularHollow(),
                Material = Toolkit.Material.IsotropicMaterial(
                    "Steel",
                    "S235",
                    210000,
                    8076,
                    8076,
                    78.5,
                    23.5,
                    -23.5,
                    FemMaterial.FlowHypothesis.mises,
                    1.2E-5),
                Supports = new[]
                {
                    Toolkit.Support.Support(0, new[] { true, true, true, true, true, true }),
                    Toolkit.Support.Support(1, new[] { true, true, true, true, true, true }),
                },
            };
        }
    }
}
