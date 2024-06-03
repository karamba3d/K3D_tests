#if ALL_TESTS

namespace KarambaCommon.Tests.Elements
{
    using System;
    using System.Collections.Generic;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Supports;
    using KarambaCommon;
    using NUnit.Framework;
    using Joint = Karamba.Joints.Joint;
    using Load = Karamba.Loads.Load;

    /// <summary>
    /// identify BuilderElement-objects before and after model assembly.
    /// </summary>
    [TestFixture]
    public class IdentifyBuilderElement_tests
    {
        [Test]
        public void JsonSerialization_Model()
        {
            var k3d = new Toolkit();
            var elems = new List<BuilderBeam>() { k3d.Part.IndexToBeam(0, 1, "A"), };

            double l = 10.0; // m
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };
            var supports = new List<Support> { k3d.Support.Support(0, k3d.Support.SupportFixedConditions) };
            var loads = new List<Load>();

            Karamba.Models.Model model = k3d.Model.AssembleModel(
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

            BuilderElement modelBuilderElement = model.elems[0].BuilderElement();
            Guid guid1 = elems[0].guid;
            Guid guid2 = modelBuilderElement.guid;
            Assert.That(guid1, Is.EqualTo(guid2));
        }
    }
}

#endif
