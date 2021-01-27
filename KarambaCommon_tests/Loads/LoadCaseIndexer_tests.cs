using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Supports;
using Karamba.Models;
using Karamba.Utilities;
using Karamba.Algorithms;
using Karamba.Loads.Combinations;

namespace KarambaCommon.Tests.Loads
{
    [TestFixture]
    public class LoadCaseIndexer_tests
    {
#if ALL_TESTS
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
#endif
    }
}
