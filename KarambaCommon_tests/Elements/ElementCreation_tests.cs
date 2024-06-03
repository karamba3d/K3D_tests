namespace KarambaCommon.Tests.Elements
{
    using System;
    using System.Collections.Generic;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Joints;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class ElementCreation_tests
    {
#if ALL_TESTS
        [Test]
        public void IndexToBeam()
        {
            var logger = new MessageLogger();

            var k3d = new Toolkit();
            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "B1") };

            var crosec = k3d.CroSec.CircularHollow();
            var points = new List<Point3> { new Point3(0, 0, 0) };
            try
            {
                var model = k3d.Model.AssembleModel(
                    elems,
                    null,
                    null,
                    out var info,
                    out var mass,
                    out var cog,
                    out var msg,
                    out var runtimeWarning,
                    new List<Joint>(),
                    points);
            }
            catch (Exception e)
            {
                var msg = e.Message;
                Assert.That(msg, Is.EqualTo("Element 0: node-index 1 is out of permissible range."));
            }
        }
#endif
    }
}
