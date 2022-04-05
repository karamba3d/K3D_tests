#if ALL_TESTS

// using System.Text.Json;
// using System.Text.Json.Serialization;

namespace KarambaCommon.Tests.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using feb;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using Newtonsoft.Json;
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

            var l = 10.0; // m
            var points = new List<Point3> { new Point3(), new Point3(l, 0, 0) };
            var supports = new List<Support> { k3d.Support.Support(0, k3d.Support.SupportFixedConditions) };
            var loads = new List<Load>();

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

            var modelBuilderElement = model.elems[0].BuilderElement();
            var guid1 = elems[0].guid;
            var guid2 = modelBuilderElement.guid;
            Assert.True(guid1 == guid2);
        }
    }
}

#endif