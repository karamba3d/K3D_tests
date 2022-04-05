#if ALL_TESTS

namespace KarambaCommon.Tests.Loads
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
    using Karamba.Loads;
    using Karamba.Loads.Combinations;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class LoadCaseIndexer_tests
    {
        /*
                /// <summary>
                /// shows how load-case identifiers are ordered by default
                /// </summary>
                [Test]
                public void LoadCaseIdentifierOrdering()
                {
                    var lc_combinator = new LCCombinator();
                    lc_combinator.AddLoadCase("LC1");
                    lc_combinator.AddLoadCase("LC2");
                    lc_combinator.AddLoadCase("LC10");
                    lc_combinator.AddLoadCase("_LC10");
                    lc_combinator.AddLoadCase("0");
                    lc_combinator.AddLoadCase("1");
                    lc_combinator.AddLoadCase("2");
                    lc_combinator.AddLoadCase("10");

                    var lcIds = new List<string>(lc_combinator.loadCaseAndCombinationIdsSorted());

                    Assert.AreEqual(lcIds[0], "0");
                    Assert.AreEqual(lcIds[1], "1");
                    Assert.AreEqual(lcIds[2], "2");
                    Assert.AreEqual(lcIds[3], "10");
                    Assert.AreEqual(lcIds[4], "_LC10");
                    Assert.AreEqual(lcIds[5], "LC1");
                    Assert.AreEqual(lcIds[6], "LC10");
                    Assert.AreEqual(lcIds[7], "LC2");
                }
                    */
    }
}

#endif
