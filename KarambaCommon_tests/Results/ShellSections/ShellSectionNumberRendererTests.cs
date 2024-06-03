#if ALL_TESTS

namespace KarambaCommon.Tests.Result.ShellSection
{
    using Karamba.Geometry;
    using Karamba.Results.ShellSection;
    using NSubstitute;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using UnitSystem = Karamba.Utilities.UnitSystem;

    [TestFixture]
    public class ShellSectionNumberRendererTests
    {
        [Test]
        public void Render_DrawStepwiseNumber()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            ShellSectionState state = ShellSectionTestsUtilities.MakeState_ElementBased();

            ShellSectionRendererInfo info = new ShellSectionRendererInfoForce();
            info.DisplayResults[ShellSecResult.M_nn] = true;
            info.ScaleResults[ShellSecResult.M_nn] = 1.0;
            info.DisplaySmooth = false;
            info.TextFormat = "{0:f}";

            IDrawViewBehaviour mock = Substitute.For<IDrawViewBehaviour>();

            var expectedPosition = new Point3[] { new Point3(0.25, 0, 1), new Point3(0.75, 0, 2) };
            var expectedText = new string[] { string.Format(info.TextFormat, 1), string.Format(info.TextFormat, 2), };

            var sut = new ShellSectionNumberRenderer(mock);
            sut.Render(state, info);

            Assert.That(sut.Positions.ToArray(), Is.EqualTo(expectedPosition));
            Assert.That(sut.Texts.ToArray(), Is.EqualTo(expectedText));
            mock.Received().DrawView();
        }

        [Test]
        public void Render_DrawSmoothNumber_ForVertexBased()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            ShellSectionState state = ShellSectionTestsUtilities.MakeState_VertexBased();

            ShellSectionRendererInfo info = new ShellSectionRendererInfoDisplacement();
            info.DisplayResults[ShellSecResult.X] = true;
            info.ScaleResults[ShellSecResult.X] = 1.0;
            info.DisplaySmooth = true;
            info.TextFormat = "{0:f}";

            IDrawViewBehaviour mock = Substitute.For<IDrawViewBehaviour>();

            var expectedPosition = new Point3[] { new Point3(0, 0, 1), new Point3(0.5, 0, 2), new Point3(1, 0, 3) };
            var expectedText = new string[]
            {
                string.Format(info.TextFormat, 100), string.Format(info.TextFormat, 200),
                string.Format(info.TextFormat, 300),
            };

            var sut = new ShellSectionNumberRenderer(mock);
            sut.Render(state, info);

            Assert.That(sut.Positions, Is.EqualTo(expectedPosition));
            Assert.That(sut.Texts, Is.EqualTo(expectedText));
            mock.Received().DrawView();
        }

        [Test]
        public void Render_DrawSmoothNumber_ForElementBased()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            ShellSectionState state = ShellSectionTestsUtilities.MakeState_ElementBased();

            ShellSectionRendererInfo info = new ShellSectionRendererInfoForce();
            info.DisplayResults[ShellSecResult.M_nn] = true;
            info.ScaleResults[ShellSecResult.M_nn] = 1.0;
            info.DisplaySmooth = true;
            info.TextFormat = "{0:f}";

            IDrawViewBehaviour mock = Substitute.For<IDrawViewBehaviour>();

            var expectedPosition = new Point3[] { new Point3(0, 0, 1), new Point3(0.5, 0, 1.5), new Point3(1, 0, 2) };
            var expectedText = new string[]
            {
                string.Format(info.TextFormat, 1), string.Format(info.TextFormat, 1.5),
                string.Format(info.TextFormat, 2),
            };

            var sut = new ShellSectionNumberRenderer(mock);
            sut.Render(state, info);

            Assert.That(sut.Positions, Is.EqualTo(expectedPosition));
            Assert.That(sut.Texts, Is.EqualTo(expectedText));
            mock.Received().DrawView();
        }
    }
}

#endif
