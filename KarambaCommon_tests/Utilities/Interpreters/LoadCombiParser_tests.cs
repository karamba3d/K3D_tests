using System;
using System.Collections.Generic;
using Karamba.Utilities;
using Karamba.Algorithms;
using Karamba.Loads;
using Karamba.Loads.Combinations;
using Karamba.Materials;
using NUnit.Common;
using NUnit.Framework;

namespace KarambaCommon.Tests.Model
{
    [TestFixture]
    public class LoadCombiParser_tests
    {
#if ALL_TESTS
        [Test]
        public void ParseLoadCombis_simple()
        {
            var parser = new CombiParser();

            List<string> input;
            List<CombinationSentence> res;
            CombinationExpression combis;
            
            //---
            input = new List<string> {@"X=A|B"};
            res = parser.Parse(input);
            Assert.AreEqual(res[0].exprName, "X");
            combis = res[0].expr;
            Assert.IsNotNull(combis as ORCombination);
            //---
            input = new List<string> { @"X=A+B" };
            res = parser.Parse(input);
            Assert.AreEqual(res[0].exprName, "X");
            combis = res[0].expr;
            Assert.IsNotNull(combis as ANDCombination);
            //---
            input = new List<string> { @"X=A-B" };
            res = parser.Parse(input);
            Assert.AreEqual(res[0].exprName, "X");
            combis = res[0].expr;
            Assert.IsNotNull(combis as ANDCombination);
            combis = combis.loadCombis[1];
            Assert.IsNotNull(combis as FactoredCombi);
            Assert.AreEqual((combis as FactoredCombi).Name, "B");
            Assert.AreEqual((combis as FactoredCombi).Facs.Upper, -1);
            //---
            // '|' has lower priority than '+': A|B+C == A|(B+C)
            //---
            input = new List<string> { @"X=A|B+C" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.IsNotNull(combis as ORCombination);
            combis = combis.loadCombis[1];
            Assert.IsNotNull(combis as ANDCombination);
            combis = combis.loadCombis[1];
            Assert.IsNotNull(combis as FactoredCombi);
            Assert.AreEqual((combis as FactoredCombi).Name, "C");
            Assert.AreEqual((combis as FactoredCombi).Facs.Upper, 1);
            //---
            input = new List<string> { @"X=(A|B)+C" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.IsNotNull(combis as ANDCombination);
            combis = combis.loadCombis[0];
            Assert.IsNotNull(combis as ORCombination);
            combis = combis.loadCombis[1];
            Assert.IsNotNull(combis as FactoredCombi);
            Assert.AreEqual((combis as FactoredCombi).Name, "B");
            Assert.AreEqual((combis as FactoredCombi).Facs.Upper, 1);
            //---
            input = new List<string> { @"X=(A|B|C)+D" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.IsNotNull(combis as ANDCombination);
            combis = combis.loadCombis[0];
            Assert.IsNotNull(combis as ORCombination);
            combis = combis.loadCombis[2];
            Assert.IsNotNull(combis as FactoredCombi);
            Assert.AreEqual((combis as FactoredCombi).Name, "C");
            Assert.AreEqual((combis as FactoredCombi).Facs.Upper, 1);
            //---
            input = new List<string> { @"X=A|B|C+D" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.IsNotNull(combis as ORCombination);
            combis = combis.loadCombis[2];
            Assert.IsNotNull(combis as ANDCombination);
            combis = combis.loadCombis[1];
            Assert.IsNotNull(combis as FactoredCombi);
            Assert.AreEqual((combis as FactoredCombi).Name, "D");
            Assert.AreEqual((combis as FactoredCombi).Facs.Upper, 1);
            //---
            input = new List<string> { @"X=1.5*A" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.IsNotNull(combis as FactoredCombi);
            Assert.AreEqual((combis as FactoredCombi).Name, "A");
            Assert.AreEqual((combis as FactoredCombi).Facs.Upper, 1.5);
            Assert.IsNull((combis as FactoredCombi).Facs.Lower);
            //---
            input = new List<string> { @"X=(1.5|2.5)*A" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.IsNotNull((FactoredCombi) combis);
            Assert.AreEqual(((FactoredCombi) combis).Name, "A");
            Assert.AreEqual(((FactoredCombi) combis).Facs.Upper, 1.5);
            Assert.AreEqual(((FactoredCombi) combis).Facs.Lower, 2.5);
            //---
            input = new List<string> { @"X=(1.5|2.5)*A & (3.5|4.5)*B" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.IsNotNull(combis as ANDLFCombination);
            combis = combis.loadCombis[1];
            Assert.IsNotNull(combis as FactoredCombi);
            Assert.AreEqual((combis as FactoredCombi).Name, "B");
            Assert.AreEqual((combis as FactoredCombi).Facs.Upper, 3.5);
            Assert.AreEqual((combis as FactoredCombi).Facs.Lower, 4.5);
            //---
        }
#endif
    }
}
