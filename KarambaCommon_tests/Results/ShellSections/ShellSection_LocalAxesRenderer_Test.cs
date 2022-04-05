﻿#if ALL_TESTS

namespace KarambaCommon.Tests.Result.ShellSection
{
    using System.Collections.Generic;
    using Karamba.Geometry;
    using Karamba.Results.ShellSection;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class ShellSection_LocalAxesRenderer_Test
    {
        [Test]
        public void TestMethod()
        {
            ShellSectionState state = Utilities.MakeState_ElementBased();
            ShellSec_RendererInfo info = new ShellSec_RendererInfo_Force();
            var mock = Substitute.For<IDrawViewBehaviour>();

            var expectedOrigins = new Point3[] { new Point3(0.25, 0, 0), new Point3(0.75, 0, 0) };
            var expectedAxes = new List<Vector3[]>
            {
                new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 0, 1), },
                new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 0, 1), },
            };

            var sut = new ShellSec_LocalAxesRenderer(mock);
            sut.Render(state, info);

            Assert.That(sut.Origins, Is.EqualTo(expectedOrigins));
            Assert.That(sut.AxesList, Is.EqualTo(expectedAxes));
            mock.Received().DrawView();
        }
    }
}

#endif