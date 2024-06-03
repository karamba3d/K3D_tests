#if ALL_TESTS

namespace KarambaCommon.Tests.Loads
{
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.Loads.Combination;
    using NUnit.Framework;

    [TestFixture]
    public class RulesExpansion_tests
    {
        [Test]
        public void Expand_Regular_Expression_AndLF_Single_Factor()
        {
            var rules = new List<string>
            {
                "ULS = (1|2)*w&(3)*s",
            };
            var lcCombinator = new LoadCaseCombinationCollection(new List<string>(), rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            var ulsCombis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "ULS" }).ToList();
            var ulsCombisStr = ulsCombis[0].LoadCases.Select(i => i.ToString()).ToList();

            Assert.That(ulsCombisStr, Has.Count.EqualTo(2));
        }

        [Test]
        public void Expand_Regular_Expression_Reduction()
        {
            var l1 = new LoadCase(string.Empty);
            var l2 = new LoadCase(string.Empty);
            var s1 = new HashSet<LoadCase>() { l1 };
            var s2 = new HashSet<LoadCase>() { l2 };
            var hc1 = l1.GetHashCode();
            var hc2 = l2.GetHashCode();
            var equal = s1 == s2;
            s1.UnionWith(s2);
            Assert.That(s1, Has.Count.EqualTo(1));

            var rules = new List<string>
            {
                "w = (0|1)*w1",
                "s = (0|1)*s1",
                "ULS = (2|3)*w+(4|5)*s",
            };
            var lcCombinator = new LoadCaseCombinationCollection(new List<string>(), rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            var ulsCombis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "ULS" }).ToList();
            var ulsCombisStr = ulsCombis[0].LoadCases.Select(i => i.ToString()).ToList();

            Assert.That(ulsCombisStr, Has.Count.EqualTo(9));
        }

        [Test]
        public void Expand_Regular_Expression_0()
        {
            var rules = new List<string>
            {
                "w1 = w11",
                "w2 = w22",
                "ULS = w$",
            };
            var lcCombinator = new LoadCaseCombinationCollection(new List<string>(), rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            var ulsCombis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "ULS" }).ToList();
            var ulsCombisStr = ulsCombis[0].LoadCases.Select(i => i.ToString()).ToList();

            Assert.That(ulsCombisStr, Is.EquivalentTo(new List<string> { "w11", "w22" }));
            // Assert.That(ulsCombisStr, Has.Count.EqualTo(2));
            // Assert.That(ulsCombisStr[0], Is.EqualTo("w11"));
            // Assert.That(ulsCombisStr[1], Is.EqualTo("w22"));
        }

        [Test]
        public void Expand_Regular_Expression_1()
        {
            var rules = new List<string>
            {
                "ULS = w$",
                "w1 = w11",
                "w2 = w22",
            };
            var lcCombinator = new LoadCaseCombinationCollection(new List<string>(), rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            var ulsCombis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "ULS" }).ToList();
            var ulsCombisStr = ulsCombis[0].LoadCases.Select(i => i.ToString()).ToList();

            Assert.That(ulsCombisStr, Is.EquivalentTo(new List<string> { "w11", "w22" }));
            // Assert.That(ulsCombisStr, Has.Count.EqualTo(2));
            // Assert.That(ulsCombisStr[0], Is.EqualTo("w11"));
            // Assert.That(ulsCombisStr[1], Is.EqualTo("w22"));
        }

        [Test]
        public void Expand_Rules_0()
        {
            var rules = new List<string>
            {
                "ULS = 1.5*W1",
            };
            var lc_combinator = new LoadCaseCombinationCollection(new List<string>(), rules);
            IReadOnlyList<LoadCaseCombination> res = lc_combinator.OrderedLoadCaseCombinations;

            string s1 = res[0].ToString();
            Assert.That(s1, Is.EqualTo("ULS = 1.5*W1;"));
        }

        [Test]
        public void Expand_Rules_1()
        {
            var rules = new List<string>
            {
                "ULS = 1.5*P + (W1|W2)",
            };
            var lcCombinator = new LoadCaseCombinationCollection(new List<string>(), rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            Assert.That(res[0].LoadCases, Has.Count.EqualTo(2));
            string s1 = res[0].LoadCases[0].ToString();
            Assert.That(s1, Is.EqualTo("1.5*P+W1"));
            string s2 = res[0].LoadCases[1].ToString();
            Assert.That(s2, Is.EqualTo("1.5*P+W2"));
        }

        [Test]
        public void Expand_Rules_2()
        {
            var rules = new List<string>
            {
                "W = 1.5*W1$",
            };
            var lc_combinator = new LoadCaseCombinationCollection(new List<string>(), rules);
            IReadOnlyList<LoadCaseCombination> res = lc_combinator.OrderedLoadCaseCombinations;

            string s1 = res[0].ToString();
            Assert.That(s1, Is.EqualTo("W = 1.5*W1$;"));
        }

        [Test]
        public void Expand_Rules_3()
        {
            var rules = new List<string>
            {
                "ULS = 1.5*P",
                "SLS = 1.0*P",
            };
            var lcCombinator = new LoadCaseCombinationCollection(new List<string>(), rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            Assert.Multiple(() => {
                Assert.That(res[0].Name, Is.EqualTo("SLS"));
                Assert.That(res[0].LoadCases, Has.Count.EqualTo(1));
                Assert.That(res[1].Name, Is.EqualTo("ULS"));
                Assert.That(res[1].LoadCases, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void LoadCaseCombinations_From_Rules()
        {
            var rules = new List<string>
            {
                "w = w1 | w2 | w3",
                "g = g1 + (1|0)*g2",
                "ULS = 1.35*g + 1.5*w",
            };
            var lcCombinator = new LoadCaseCombinationCollection(new List<string>(), rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            var ulsCombis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "ULS" }).ToList();
            var ulsCombisStr = ulsCombis[0].LoadCases.Select(i => i.ToString()).ToList();

            Assert.That(ulsCombisStr, Is.EquivalentTo(
                new List<string> { "1.35*g1+1.35*g2+1.5*w1", "1.35*g1+1.35*g2+1.5*w2",
                                   "1.35*g1+1.35*g2+1.5*w3", "1.35*g1+1.5*w1",
                                   "1.35*g1+1.5*w2", "1.35*g1+1.5*w3" }));

            // Assert.That(ulsCombisStr, Has.Count.EqualTo(6));
            // Assert.That(ulsCombisStr[0], Is.EqualTo("1.35*g1+1.35*g2+1.5*w1"));
            // Assert.That(ulsCombisStr[1], Is.EqualTo("1.35*g1+1.35*g2+1.5*w2"));
            // Assert.That(ulsCombisStr[2], Is.EqualTo("1.35*g1+1.35*g2+1.5*w3"));
            // Assert.That(ulsCombisStr[3], Is.EqualTo("1.35*g1+1.5*w1"));
            // Assert.That(ulsCombisStr[4], Is.EqualTo("1.35*g1+1.5*w2"));
            // Assert.That(ulsCombisStr[5], Is.EqualTo("1.35*g1+1.5*w3"));
            // was ^ before using collection comparing
        }

        [Test]
        public void Remark_in_Rules()
        {
            var rules = new List<string>
            {
                "# remark",
            };
            var lcCombinator = new LoadCaseCombinationCollection(new List<string>(), rules);
            // IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations; // needed?

            var ulsCombis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "ULS" }).ToList();

            Assert.That(ulsCombis, Has.Count.EqualTo(0));
        }

        [Test]
        public void LoadCaseCombinations_ANDLF()
        {
            var rules = new List<string> { @"X1=(1|0.5)*A & (2|2.5)*B" };
            var lc_combinator = new LoadCaseCombinationCollection(new List<string> { "A", "B" }, rules);
            IReadOnlyList<LoadCaseCombination> res = lc_combinator.OrderedLoadCaseCombinations;

            var x1Combis = lc_combinator.SelectLoadCaseCombinations(new List<string> { "X1" }).ToList();
            var x1CombisStr = x1Combis[0].LoadCases.Select(i => i.ToString()).ToList();

            Assert.That(x1CombisStr, Is.EquivalentTo(new List<string> { "A+2*B", "2.5*B+0.5*A" })); // Is.SupersetOf(...) ???
            // Assert.That(x1CombisStr[0], Is.EqualTo("A+2*B"));
            // Assert.That(x1CombisStr[1], Is.EqualTo("2.5*B+0.5*A"));

            rules = new List<string> { @"X2=(1|0.5)*A & (2|1.5)*B & (3|2.5)*C" };
            lc_combinator = new LoadCaseCombinationCollection(new List<string> { }, rules);
            var x2Combis = lc_combinator.SelectLoadCaseCombinations(new List<string> { "X2" }).ToList();
            var x2CombisStr = x2Combis[0].LoadCases.Select(i => i.ToString()).ToList();

            Assert.Multiple(() => {
                Assert.That(x2Combis, Has.Count.EqualTo(1));
                Assert.That(x2CombisStr[0], Is.EqualTo("A+1.5*B+2.5*C"));
                Assert.That(x2CombisStr[1], Is.EqualTo("2*B+2.5*C+0.5*A"));
                Assert.That(x2CombisStr[2], Is.EqualTo("3*C+0.5*A+1.5*B"));
            });
        }

        [Test]
        public void LoadCaseCombinations_AND()
        {
            var loadCases = new List<string> { "A", "B", "C" };

            var rules = new List<string>()
            {
                "X1=(0|0)*A+B",
                "X2=(0|1)*A+B",
                "X3=(0|1)*A+B",
                "X4=A+B",
                "X5=A+B+C",
            };

            var lcCombinator = new LoadCaseCombinationCollection(loadCases, rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            var x1Combis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "X1" }).ToList();
            var x1CombisStr = x1Combis[0].LoadCases.Select(i => i.ToString()).ToList();
            Assert.That(x1CombisStr[0], Is.EqualTo("B"));

            var x2Combis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "X2" }).ToList();
            var x2CombisStr = x2Combis[0].LoadCases.Select(i => i.ToString()).ToList();
            Assert.That(x2CombisStr, Is.EquivalentTo(new List<string> { "A+B", "B" }));
            // Assert.That(x2CombisStr[0], Is.EqualTo("A+B"));
            // Assert.That(x2CombisStr[1], Is.EqualTo("B"));

            var x3Combis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "X3" }).ToList();
            var x3CombisStr = x3Combis[0].LoadCases.Select(i => i.ToString()).ToList();
            Assert.That(x3CombisStr, Is.EquivalentTo(new List<string> { "A+B", "B" }));
            // Assert.That(x3CombisStr[0], Is.EqualTo("A+B"));
            // Assert.That(x3CombisStr[1], Is.EqualTo("B"));

            var x4Combis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "X4" }).ToList();
            var x4CombisStr = x4Combis[0].LoadCases.Select(i => i.ToString()).ToList();
            Assert.That(x4CombisStr[0], Is.EqualTo("A+B"));

            var x5Combis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "X5" }).ToList();
            var x5CombisStr = x5Combis[0].LoadCases.Select(i => i.ToString()).ToList();
            Assert.That(x5CombisStr[0], Is.EqualTo("A+B+C"));
        }

        [Test]
        public void LoadCaseCombinations_OR()
        {
            var loadCases = new List<string> { "A", "B", "C" };

            var rules = new List<string>()
            {
                "X1=A|B",
                "X2=A|B|C",
            };

            var lcCombinator = new LoadCaseCombinationCollection(loadCases, rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            var x1Combis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "X1" }).ToList();
            var x1CombisStr = x1Combis[0].LoadCases.Select(i => i.ToString()).ToList();
            Assert.That(x1CombisStr, Is.EquivalentTo(new List<string> { "A", "B" }));
            // Assert.That(x1CombisStr[0], Is.EqualTo("A"));
            // Assert.That(x1CombisStr[1], Is.EqualTo("B"));

            var x2Combis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "X2" }).ToList();
            var x2CombisStr = x2Combis[0].LoadCases.Select(i => i.ToString()).ToList();
            Assert.That(x2CombisStr, Is.EquivalentTo(new List<string> { "A", "B", "C" }));
            // Assert.That(x2CombisStr[0], Is.EqualTo("A"));
            // Assert.That(x2CombisStr[1], Is.EqualTo("B"));
            // Assert.That(x2CombisStr[2], Is.EqualTo("C"));
        }

        [Test]
        public void LoadCaseCombinations_multiple()
        {
            var loadCases = new List<string> { };

            var rules = new List<string>()
            {
                "C=D|B",
                "B=A",
            };

            var lcCombinator = new LoadCaseCombinationCollection(loadCases, rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            var BCombis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "B" }).ToList();
            var BCombisStr = BCombis[0].LoadCases.Select(i => i.ToString()).ToList();
            Assert.Multiple(() => {
                Assert.That(BCombis[0].LoadCases, Has.Count.EqualTo(1));
                Assert.That(BCombisStr[0], Is.EqualTo("A"));
            });

            var CCombis = lcCombinator.SelectLoadCaseCombinations(new List<string> { "C" }).ToList();
            var CCombisStr = CCombis[0].LoadCases.Select(i => i.ToString()).ToList();
            Assert.Multiple(() => {
                Assert.That(CCombis[0].LoadCases, Has.Count.EqualTo(2));
                Assert.That(CCombisStr[0], Is.EqualTo("D"));
                Assert.That(CCombisStr[1], Is.EqualTo("A"));
            });
        }

        // */
        [Test]
        public void Expand_Rules_4()
        {
            var rules = new List<string>
            {
                "A = 1.5*(B|C)",
            };
            var lcCombinator = new LoadCaseCombinationCollection(new List<string> { "B", "C" }, rules);
            IReadOnlyList<LoadCaseCombination> res = lcCombinator.OrderedLoadCaseCombinations;

            Assert.Multiple(() => {
                Assert.That(res[0].Name, Is.EqualTo("A"));
                Assert.That(res[0].LoadCases, Has.Count.EqualTo(2));
                var CCombisStr = res[0].LoadCases.Select(i => i.ToString()).ToList();
                Assert.That(CCombisStr[0], Is.EqualTo("1.5*B"));
                Assert.That(CCombisStr[1], Is.EqualTo("1.5*C"));
            });
        }
    }
}

#endif
