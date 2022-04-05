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
    using Karamba.Results;
    using Karamba.Supports;
    using Karamba.Utilities;
    using NUnit.Framework;
    /*
    [TestFixture]
    public class LoadCaseCombinator_tests
    {

        [Test]
        public void LoadCaseCombinations_From_Rules_With_Warning()
        {
            var rules = new List<string>
            {
                "ULS = 1.5*P + (W1|W2)"
            };
            var lc_combinator = new LCCombinator(RecoveryMode.warn);
            lc_combinator.AddRules(rules);
            lc_combinator.AddLoadCombination("ULS");

            var uls_combis = lc_combinator.load_combinations["ULS"];
            var uls_combis_str = new List<string>();
            foreach (var uls_combi in uls_combis)
            {
                uls_combis_str.Add(uls_combi.ToString());
            }

            Assert.True(uls_combis.Count == 2);
            Assert.True(uls_combis_str[0] == "1,5*P+W1");
            Assert.True(uls_combis_str[1] == "1,5*P+W2");
        }

        [Test]
        public void LoadCaseCombinations_From_Rules()
        {
            var rules = new List<string>
            {
                "w = w1 | w2 | w3",
                "g = g1 + (1|0)*g2",
                "ULS = 1.35*g + 1.5*w"
            };
            var lc_combinator = new LCCombinator();
            lc_combinator.AddLoadCase("w0");
            lc_combinator.AddLoadCase("w1");
            lc_combinator.AddLoadCase("w2");
            lc_combinator.AddLoadCase("w3");
            lc_combinator.AddLoadCase("g1");
            lc_combinator.AddLoadCase("g2");
            lc_combinator.AddRules(rules);
            lc_combinator.AddLoadCombination("ULS");

            var uls_combis = lc_combinator.load_combinations["ULS"];
            var uls_combis_str = new List<string>();
            foreach (var uls_combi in uls_combis) {
                uls_combis_str.Add(uls_combi.ToString());
            }

            Assert.True(uls_combis.Count == 6);
            Assert.True(uls_combis_str[0] == "1,35*g1+1,35*g2+1,5*w1");
            Assert.True(uls_combis_str[1] == "1,35*g1+1,35*g2+1,5*w2");
            Assert.True(uls_combis_str[2] == "1,35*g1+1,35*g2+1,5*w3");
            Assert.True(uls_combis_str[3] == "1,35*g1+1,5*w1");
            Assert.True(uls_combis_str[4] == "1,35*g1+1,5*w2");
            Assert.True(uls_combis_str[5] == "1,35*g1+1,5*w3");
        }

        [Test]
        public void LoadCaseCombinations_ANDLF()
        {
            var lc_combinator = new LCCombinator();
            lc_combinator.AddLoadCase("A");
            lc_combinator.AddLoadCase("B");
            lc_combinator.AddLoadCase("C");


            var rules = new List<string> { @"X1=(1|0.5)A & (2|2.5)B" };
            lc_combinator.AddRules(rules);
            lc_combinator.AddLoadCombination("X1");
            var combis = lc_combinator.load_combinations["X1"];
            var combis_str = new List<string>();
            foreach (var combi in combis)
            {
                combis_str.Add(combi.ToString());
            }
            Assert.True(combis.Count == 2);
            Assert.True(combis_str[0] == "A+2,5*B");
            Assert.True(combis_str[1] == "2*B+0,5*A");

            rules = new List<string> { @"X2=(1|0.5)A & (2|1.5)B & (3|2.5)C" };
            lc_combinator.AddRules(rules);
            lc_combinator.AddLoadCombination("X2");
            combis = lc_combinator.load_combinations["X2"];
            combis_str = new List<string>();
            foreach (var combi in combis)
            {
                combis_str.Add(combi.ToString());
            }
            Assert.True(combis.Count == 3);
            Assert.True(combis_str[0] == "A+1,5*B+2,5*C");
            Assert.True(combis_str[1] == "2*B+2,5*C+0,5*A");
            Assert.True(combis_str[2] == "3*C+0,5*A+1,5*B");
        }

        [Test]
        public void LoadCaseCombinations_AND()
        {
            var lc_combinator = new LCCombinator();
            lc_combinator.AddLoadCase("A");
            lc_combinator.AddLoadCase("B");
            lc_combinator.AddLoadCase("C");
            var rules = new List<string>();
            rules.Add("X1=(0|0)A+B");
            rules.Add("X2=(0|1)A+B");
            rules.Add("X3=(0|1)*A+B");
            rules.Add("X4=A+B");
            rules.Add("X5=A+B+C");
            lc_combinator.AddRules(rules);

            lc_combinator.AddLoadCombination("X1");
            var combis = lc_combinator.load_combinations["X1"];
            var combis_str = new List<string>();
            foreach (var combi in combis)
            {
                combis_str.Add(combi.ToString());
            }
            Assert.True(combis.Count == 2);
            Assert.True(combis_str[0] == "B");
            Assert.True(combis_str[1] == "B");

            lc_combinator.AddLoadCombination("X2");
            combis = lc_combinator.load_combinations["X2"];
            combis_str = new List<string>();
            foreach (var combi in combis)
            {
                combis_str.Add(combi.ToString());
            }
            Assert.True(combis.Count == 2);
            Assert.True(combis_str[0] == "B");
            Assert.True(combis_str[1] == "A+B");

            lc_combinator.AddLoadCombination("X3");
            combis = lc_combinator.load_combinations["X3"];
            combis_str = new List<string>();
            foreach (var combi in combis)
            {
                combis_str.Add(combi.ToString());
            }
            Assert.True(combis.Count == 2);
            Assert.True(combis_str[0] == "B");
            Assert.True(combis_str[1] == "A+B");

            lc_combinator.AddLoadCombination("X4");
            combis = lc_combinator.load_combinations["X4"];
            combis_str = new List<string>();
            foreach (var combi in combis)
            {
                combis_str.Add(combi.ToString());
            }
            Assert.True(combis.Count == 1);
            Assert.True(combis_str[0] == "A+B");

            lc_combinator.AddLoadCombination("X5");
            combis = lc_combinator.load_combinations["X5"];
            combis_str = new List<string>();
            foreach (var combi in combis)
            {
                combis_str.Add(combi.ToString());
            }
            Assert.True(combis.Count == 1);
            Assert.True(combis_str[0] == "A+B+C");
        }

        [Test]
        public void LoadCaseCombinations_OR()
        {
            var lc_combinator = new LCCombinator();
            lc_combinator.AddLoadCase("A");
            lc_combinator.AddLoadCase("B");
            lc_combinator.AddLoadCase("C");
            var rules = new List<string>();
            rules.Add("X1=A|B");
            rules.Add("X2=A|B|C");
            lc_combinator.AddRules(rules);

            lc_combinator.AddLoadCombination("X1");
            var combis = lc_combinator.load_combinations["X1"];
            var combis_str = new List<string>();
            foreach (var combi in combis)
            {
                combis_str.Add(combi.ToString());
            }
            Assert.True(combis.Count == 2);
            Assert.True(combis_str[0] == "A");
            Assert.True(combis_str[1] == "B");

            lc_combinator.AddLoadCombination("X2");
            combis = lc_combinator.load_combinations["X2"];
            combis_str = new List<string>();
            foreach (var combi in combis)
            {
                combis_str.Add(combi.ToString());
            }
            Assert.True(combis.Count == 3);
            Assert.True(combis_str[0] == "A");
            Assert.True(combis_str[1] == "B");
            Assert.True(combis_str[2] == "C");
        }
    }
    */
}

#endif