#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using Karamba.Algorithms;
    using Karamba.Loads;
    using Karamba.Loads.Combinations;
    using Karamba.Materials;
    using Karamba.Utilities;
    using NUnit.Common;
    using NUnit.Framework;

    [TestFixture]
    public class LoadCombiParser_tests
    {
        [Test]
        public void ParseLoadCombis_simple()
        {
            var parser = new CombiParser();

            List<string> input;
            List<CombinationSentence> res;
            CombinationExpression combis;

            //---
            input = new List<string> { @"X=A|B" };
            res = parser.Parse(input);
            Assert.That(res[0].exprName, Is.EqualTo("X"));
            combis = res[0].expr;
            Assert.That(combis as ORCombination, Is.Not.Null);

            //---
            input = new List<string> { @"X=A+B" };
            res = parser.Parse(input);
            Assert.That(res[0].exprName, Is.EqualTo("X"));
            combis = res[0].expr;
            Assert.That(combis as ANDCombination, Is.Not.Null);

            //---
            input = new List<string> { @"X=A-B" };
            res = parser.Parse(input);
            Assert.That(res[0].exprName, Is.EqualTo("X"));
            combis = res[0].expr;
            Assert.That(combis as ANDCombination, Is.Not.Null);
            combis = combis.loadCombis[1];
            Assert.That(combis as FactoredCombi, Is.Not.Null);
            Assert.That((combis as FactoredCombi).Name, Is.EqualTo("B"));
            Assert.That((combis as FactoredCombi).Facs.Upper, Is.EqualTo(-1));

            //---
            // '|' has lower priority than '+': A|B+C == A|(B+C)
            //---
            input = new List<string> { @"X=A|B+C" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as ORCombination, Is.Not.Null);
            combis = combis.loadCombis[1];
            Assert.That(combis as ANDCombination, Is.Not.Null);
            combis = combis.loadCombis[1];
            Assert.That(combis as FactoredCombi, Is.Not.Null);
            Assert.That((combis as FactoredCombi).Name, Is.EqualTo("C"));
            Assert.That((combis as FactoredCombi).Facs.Upper, Is.EqualTo(1));

            //---
            input = new List<string> { @"X=(A|B)+C" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as ANDCombination, Is.Not.Null);
            combis = combis.loadCombis[0];
            Assert.That(combis as ORCombination, Is.Not.Null);
            combis = combis.loadCombis[1];
            Assert.That(combis as FactoredCombi, Is.Not.Null);
            Assert.That((combis as FactoredCombi).Name, Is.EqualTo("B"));
            Assert.That((combis as FactoredCombi).Facs.Upper, Is.EqualTo(1));

            //---
            input = new List<string> { @"X=(A|B|C)+D" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as ANDCombination, Is.Not.Null);
            combis = combis.loadCombis[0];
            Assert.That(combis as ORCombination, Is.Not.Null);
            combis = combis.loadCombis[2];
            Assert.That(combis as FactoredCombi, Is.Not.Null);
            Assert.That((combis as FactoredCombi).Name, Is.EqualTo("C"));
            Assert.That((combis as FactoredCombi).Facs.Upper, Is.EqualTo(1));

            //---
            input = new List<string> { @"X=A|B|C+D" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as ORCombination, Is.Not.Null);
            combis = combis.loadCombis[2];
            Assert.That(combis as ANDCombination, Is.Not.Null);
            combis = combis.loadCombis[1];
            Assert.That(combis as FactoredCombi, Is.Not.Null);
            Assert.That((combis as FactoredCombi).Name, Is.EqualTo("D"));
            Assert.That((combis as FactoredCombi).Facs.Upper, Is.EqualTo(1));

            //---
            input = new List<string> { @"X=1.5*A" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as FactoredCombi, Is.Not.Null);
            Assert.That((combis as FactoredCombi).Name, Is.EqualTo("A"));
            Assert.That((combis as FactoredCombi).Facs.Upper, Is.EqualTo(1.5));
            Assert.That((combis as FactoredCombi).Facs.Lower, Is.Null);

            //---
            input = new List<string> { @"X=(1.5|2.5)*A" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That((FactoredCombi)combis, Is.Not.Null);
            Assert.That(((FactoredCombi)combis).Name, Is.EqualTo("A"));
            Assert.That(((FactoredCombi)combis).Facs.Upper, Is.EqualTo(1.5));
            Assert.That(((FactoredCombi)combis).Facs.Lower, Is.EqualTo(2.5));

            //---
            input = new List<string> { @"X=(1.5|2.5)*A & (3.5|4.5)*B" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as ANDLFCombination, Is.Not.Null);
            combis = combis.loadCombis[1];
            Assert.That(combis as FactoredCombi, Is.Not.Null);
            Assert.That((combis as FactoredCombi).Name, Is.EqualTo("B"));
            Assert.That((combis as FactoredCombi).Facs.Upper, Is.EqualTo(3.5));
            Assert.That((combis as FactoredCombi).Facs.Lower, Is.EqualTo(4.5));

            //---
        }
    }
}

#endif
