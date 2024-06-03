#if ALL_TESTS

namespace KarambaCommon.Tests.Loads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.Loads.Combination;
    using NUnit.Framework;

    [TestFixture]
    public class LoadCaseCombinator_tests
    {
        [Test]
        public void LoadCaseCombnination_Hash()
        {
            // test whether two load case combinations can be identified as being equal
            //
            // 1. create two load case combinations
            // 2. check whether they are equal
            // 3. check whether they have the same hash
            // 4. change one of the load case combinations
            // 5. check whether they are not equal
            // 6. check whether they have different hashes
            // 7. check whether the hash of the first combination is still the same
            // 8. check whether the hash of the second combination is still the same
            // 9. check whether the hash of the first combination is different from the hash of the second combination

            var loadCaseCombination1 = new LoadCaseCombination("LC1", new List<LoadCase> { new LoadCase("LC1", 1.0) });
            var loadCaseCombination2 = new LoadCaseCombination("LC1", new List<LoadCase> { new LoadCase("LC1", 1.0) });

            var loadCaseCombination1Hash = loadCaseCombination1.GetHashCode();
            var loadCaseCombination1NameHash = loadCaseCombination1.Name.GetHashCode();
            var loadCaseCombination1LoadCasesHash = loadCaseCombination1.LoadCases.GetHashCode();
            var loadCaseCombination1OptionsHash = loadCaseCombination1.Options.GetHashCode();

            var loadCaseCombination2Hash = loadCaseCombination2.GetHashCode();
            var loadCaseCombination2NameHash = loadCaseCombination2.Name.GetHashCode();
            var loadCaseCombination2LoadCasesHash = loadCaseCombination2.LoadCases.GetHashCode();
            var loadCaseCombination2OptionsHash = loadCaseCombination2.Options.GetHashCode();

            List<LoadCaseCombination> lcs1 = new List<LoadCaseCombination>() { loadCaseCombination1 };

            List<LoadCaseCombination> lcs2 = new List<LoadCaseCombination>() { loadCaseCombination2 };
            HashSet<LoadCaseCombination> loadCaseCombinations = new HashSet<LoadCaseCombination>(lcs1);
            loadCaseCombinations.UnionWith(lcs2);
            Assert.That(loadCaseCombinations.Count, Is.EqualTo(1));
        }

        [Test]
        public void LoadCaseCombinations_LoadFactorShortcut()
        {
            var rules = new List<string>
            {
                "W1 = A",
                "W2 = B",
                "ULS = W$",
            };
            var loadCaseCollection = new LoadCaseCombinationCollection(new List<string>(), rules);

            var ulsLoadCaseCombination = loadCaseCollection.LoadCaseCombination("ULS");
            var ulsCombisStr = new List<string>();
            foreach (var loadCase in ulsLoadCaseCombination.LoadCases)
                ulsCombisStr.Add(loadCase.ToString());

            Assert.Multiple(() => {
                Assert.That(ulsLoadCaseCombination.LoadCases, Has.Count.EqualTo(2));
                Assert.That(ulsCombisStr[0], Is.EqualTo("A"));
                Assert.That(ulsCombisStr[1], Is.EqualTo("B"));
            });
        }

        [Test]
        public void LoadCaseCombinations_LoadFactorCalculation()
        {
            var rules = new List<string>
            {
                "ULS = -2*((1.5*2/0.3+3+4)*P)",
            };
            var loadCaseCollection = new LoadCaseCombinationCollection(new List<string>(), rules);

            var ulsLoadCaseCombination = loadCaseCollection.LoadCaseCombination("ULS");
            var ulsCombisStr = new List<string>();
            foreach (var loadCase in ulsLoadCaseCombination.LoadCases)
            {
                ulsCombisStr.Add(loadCase.ToString());
            }

            Assert.Multiple(() =>
            {   Assert.That(ulsLoadCaseCombination.LoadCases, Has.Count.EqualTo(1));
                Assert.That(ulsCombisStr[0], Is.EqualTo("-34*P"));
            });
        }

        [Test]
        public void LoadCaseCombinations_From_Rules_With_Warning()
        {
            var rules = new List<string>
            {
                "ULS = 1.5*P + (W1|W2)",
            };
            var loadCaseCollection = new LoadCaseCombinationCollection(new List<string>(), rules);

            var ulsLoadCaseCombination = loadCaseCollection.LoadCaseCombination("ULS");
            var ulsCombisStr = new List<string>();
            foreach (var loadCase in ulsLoadCaseCombination.LoadCases)
            {
                ulsCombisStr.Add(loadCase.ToString());
            }

            Assert.Multiple(() =>
            {   Assert.That(ulsLoadCaseCombination.LoadCases, Has.Count.EqualTo(2));
                Assert.That(ulsCombisStr[0], Is.EqualTo("1.5*P+W1"));
                Assert.That(ulsCombisStr[1], Is.EqualTo("1.5*P+W2"));
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
            var loadCaseCollection = new LoadCaseCombinationCollection(new List<string>(), rules);

            var ulsLoadCaseCombination = loadCaseCollection.LoadCaseCombination("ULS");
            var ulsCombisStr = new List<string>();
            foreach (var loadCase in ulsLoadCaseCombination.LoadCases)
            {
                ulsCombisStr.Add(loadCase.ToString());
            }

            Assert.Multiple(() =>
            {   Assert.That(ulsLoadCaseCombination.LoadCases, Has.Count.EqualTo(6));
                Assert.That(ulsCombisStr[0], Is.EqualTo("1.35*g1+1.35*g2+1.5*w1"));
                Assert.That(ulsCombisStr[1], Is.EqualTo("1.35*g1+1.35*g2+1.5*w2"));
                Assert.That(ulsCombisStr[2], Is.EqualTo("1.35*g1+1.35*g2+1.5*w3"));
                Assert.That(ulsCombisStr[3], Is.EqualTo("1.35*g1+1.5*w1"));
                Assert.That(ulsCombisStr[4], Is.EqualTo("1.35*g1+1.5*w2"));
                Assert.That(ulsCombisStr[5], Is.EqualTo("1.35*g1+1.5*w3"));
            });
        }

        [Test]
        public void LoadCaseCombinations_ANDLF()
        {
            var rules = new List<string> { "X1=(1|0.5)*A & (2|2.5)*B" };
            var loadCaseCollection = new LoadCaseCombinationCollection(new List<string>(), rules);

            var loadCaseCombination = loadCaseCollection.LoadCaseCombination("X1");

            Assert.Multiple(() =>
            {   var combisStr = loadCaseCombination.LoadCases.Select(loadCase => loadCase.ToString()).ToList();

                Assert.That(loadCaseCombination.LoadCases, Has.Count.EqualTo(2));
                Assert.That(combisStr[0], Is.EqualTo("A+2*B"));
                Assert.That(combisStr[1], Is.EqualTo("2.5*B+0.5*A"));

                rules = new List<string> { @"X2=(1|0.5)*A & (2|1.5)*B & (3|2.5)*C" };
                loadCaseCollection = new LoadCaseCombinationCollection(new List<string>(), rules);

                loadCaseCombination = loadCaseCollection.LoadCaseCombination("X2");
                combisStr = loadCaseCombination.LoadCases.Select(loadCase => loadCase.ToString()).ToList();

                Assert.That(loadCaseCombination.LoadCases, Has.Count.EqualTo(3));
                Assert.That(combisStr[0], Is.EqualTo("A+1.5*B+2.5*C"));
                Assert.That(combisStr[1], Is.EqualTo("2*B+2.5*C+0.5*A"));
                Assert.That(combisStr[2], Is.EqualTo("3*C+0.5*A+1.5*B"));
            });
        }

        [Test]
        public void LoadCaseCombinations_AND()
        {
            var rules = new List<string>
            {
                "X1=(0|0)*A+B",
                "X2=(0|1)*A+B",
                "X3=(0|1)*A+B",
                "X4=A+B",
                "X5=A+B+C",
            };
            var loadCaseCollection = new LoadCaseCombinationCollection(new List<string>(), rules);

            var loadCaseCombination = loadCaseCollection.LoadCaseCombination("X1");
            Assert.Multiple(() => {
            var combisStr = loadCaseCombination.LoadCases.Select(loadCase => loadCase.ToString()).ToList();
            Assert.That(combisStr, Has.Count.EqualTo(1));
            Assert.That(combisStr[0], Is.EqualTo("B"));

            loadCaseCombination = loadCaseCollection.LoadCaseCombination("X2");
            combisStr = loadCaseCombination.LoadCases.Select(loadCase => loadCase.ToString()).ToList();
            Assert.That(combisStr, Has.Count.EqualTo(2));
            Assert.That(combisStr[0], Is.EqualTo("A+B"));
            Assert.That(combisStr[1], Is.EqualTo("B"));

            loadCaseCombination = loadCaseCollection.LoadCaseCombination("X3");
            combisStr = loadCaseCombination.LoadCases.Select(loadCase => loadCase.ToString()).ToList();
            Assert.That(combisStr, Has.Count.EqualTo(2));
            Assert.That(combisStr[0], Is.EqualTo("A+B"));
            Assert.That(combisStr[1], Is.EqualTo("B"));

            loadCaseCombination = loadCaseCollection.LoadCaseCombination("X4");
            combisStr = loadCaseCombination.LoadCases.Select(loadCase => loadCase.ToString()).ToList();
            Assert.That(combisStr, Has.Count.EqualTo(1));
            Assert.That(combisStr[0], Is.EqualTo("A+B"));

            loadCaseCombination = loadCaseCollection.LoadCaseCombination("X5");
            combisStr = loadCaseCombination.LoadCases.Select(loadCase => loadCase.ToString()).ToList();
            Assert.That(combisStr, Has.Count.EqualTo(1));
            Assert.That(combisStr[0], Is.EqualTo("A+B+C"));
            });
        }

        [Test]
        public void LoadCaseCombinations_OR()
        {
            var rules = new List<string>
            {
                "X1=A|B",
                "X2=A|B|C",
            };
            var loadCaseCollection = new LoadCaseCombinationCollection(new List<string>(), rules);

            var loadCaseCombination = loadCaseCollection.LoadCaseCombination("X1");

            Assert.Multiple(() =>
            {   var combisStr = loadCaseCombination.LoadCases.Select(loadCase => loadCase.ToString()).ToList();
                Assert.That(combisStr, Has.Count.EqualTo(2));
                Assert.That(combisStr[0], Is.EqualTo("A"));
                Assert.That(combisStr[1], Is.EqualTo("B"));

                loadCaseCombination = loadCaseCollection.LoadCaseCombination("X2");
                combisStr = loadCaseCombination.LoadCases.Select(loadCase => loadCase.ToString()).ToList();
                Assert.That(combisStr, Has.Count.EqualTo(3));
                Assert.That(combisStr[0], Is.EqualTo("A"));
                Assert.That(combisStr[1], Is.EqualTo("B"));
                Assert.That(combisStr[2], Is.EqualTo("C"));
            });
        }

        [Test]
        public void LoadCaseCombinations_CircularReference()
        {
            var rules = new List<string> { "X1=X1" };

            _ = Assert.Throws<ArgumentException>(
                () => new LoadCaseCombinationCollection(new List<string>(), rules));
        }

        // */
    }
}

#endif
