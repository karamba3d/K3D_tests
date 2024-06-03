#if ALL_TESTS

namespace KarambaCommon.Tests.Model
{
    using System.Collections.Generic;
    using Karamba.Loads.Combination;
    using NUnit.Framework;

    [TestFixture]
    public class LoadCombiParser_tests
    {
        [Test]
        public void ParseLoadCombis_simple()
        {
            var parser = new RulesParser();

            List<string> input;
            List<CombinationSentence> res;
            CombinationExpression combis;

            //---
            input = new List<string> { @"X=(A|B)+C" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as CombinationAND, Is.Not.Null);
            combis = combis.CombinationExpressions[0];
            Assert.That(combis as CombinationOR, Is.Not.Null);
            combis = combis.CombinationExpressions[1];
            Assert.That(combis as CombinationFactored, Is.Not.Null);
            Assert.That((combis as CombinationFactored).Name, Is.EqualTo("B"));
            Assert.That((combis as CombinationFactored).Facs.Upper, Is.EqualTo(1));

            //---
            input = new List<string> { @"X=A|B" };
            res = parser.Parse(input);
            Assert.That(res[0].exprName, Is.EqualTo("X"));
            combis = res[0].expr;
            Assert.That(combis as CombinationOR, Is.Not.Null);

            //---
            input = new List<string> { @"X=A+B" };
            res = parser.Parse(input);
            Assert.That(res[0].exprName, Is.EqualTo("X"));
            combis = res[0].expr;
            Assert.That(combis as CombinationAND, Is.Not.Null);

            //---
            input = new List<string> { @"X=A-B" };
            res = parser.Parse(input);
            Assert.That(res[0].exprName, Is.EqualTo("X"));
            combis = res[0].expr;
            Assert.That(combis as CombinationAND, Is.Not.Null);
            combis = combis.CombinationExpressions[1];
            Assert.That(combis as CombinationFactored, Is.Not.Null);
            Assert.That((combis as CombinationFactored).Name, Is.EqualTo("B"));
            Assert.That((combis as CombinationFactored).Facs.Upper, Is.EqualTo(-1));

            //---
            // '|' has lower priority than '+': A|B+C == A|(B+C)
            //---
            input = new List<string> { @"X=A|B+C" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as CombinationOR, Is.Not.Null);
            combis = combis.CombinationExpressions[1];
            Assert.That(combis as CombinationAND, Is.Not.Null);
            combis = combis.CombinationExpressions[1];
            Assert.That(combis as CombinationFactored, Is.Not.Null);
            Assert.That((combis as CombinationFactored).Name, Is.EqualTo("C"));
            Assert.That((combis as CombinationFactored).Facs.Upper, Is.EqualTo(1));

            //---
            input = new List<string> { @"X=(A|B)+C" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as CombinationAND, Is.Not.Null);
            combis = combis.CombinationExpressions[0];
            Assert.That(combis as CombinationOR, Is.Not.Null);
            combis = combis.CombinationExpressions[1];
            Assert.That(combis as CombinationFactored, Is.Not.Null);
            Assert.That((combis as CombinationFactored).Name, Is.EqualTo("B"));
            Assert.That((combis as CombinationFactored).Facs.Upper, Is.EqualTo(1));

            //---
            input = new List<string> { @"X=(A|B|C)+D" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as CombinationAND, Is.Not.Null);
            combis = combis.CombinationExpressions[0];
            Assert.That(combis as CombinationOR, Is.Not.Null);
            combis = combis.CombinationExpressions[2];
            Assert.That(combis as CombinationFactored, Is.Not.Null);
            Assert.That((combis as CombinationFactored).Name, Is.EqualTo("C"));
            Assert.That((combis as CombinationFactored).Facs.Upper, Is.EqualTo(1));

            //---
            input = new List<string> { @"X=A|B|C+D" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as CombinationOR, Is.Not.Null);
            combis = combis.CombinationExpressions[2];
            Assert.That(combis as CombinationAND, Is.Not.Null);
            combis = combis.CombinationExpressions[1];
            Assert.That(combis as CombinationFactored, Is.Not.Null);
            Assert.That((combis as CombinationFactored).Name, Is.EqualTo("D"));
            Assert.That((combis as CombinationFactored).Facs.Upper, Is.EqualTo(1));

            //---
            input = new List<string> { @"X=1.5*A" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as CombinationFactored, Is.Not.Null);
            Assert.That((combis as CombinationFactored).Name, Is.EqualTo("A"));
            Assert.That((combis as CombinationFactored).Facs.Upper, Is.EqualTo(1.5));
            Assert.That((combis as CombinationFactored).Facs.Lower, Is.EqualTo(1.5));

            //---
            input = new List<string> { @"X=(1.5|2.5)*A" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That((CombinationFactored)combis, Is.Not.Null);
            Assert.That(((CombinationFactored)combis).Name, Is.EqualTo("A"));
            Assert.That(((CombinationFactored)combis).Facs.Upper, Is.EqualTo(2.5));
            Assert.That(((CombinationFactored)combis).Facs.Lower, Is.EqualTo(1.5));

            //---
            input = new List<string> { @"X=(1.5|2.5)*A & (3.5|4.5)*B" };
            res = parser.Parse(input);
            combis = res[0].expr;
            Assert.That(combis as CombinationANDLF, Is.Not.Null);
            combis = combis.CombinationExpressions[1];
            Assert.That(combis as CombinationFactored, Is.Not.Null);
            Assert.That((combis as CombinationFactored).Name, Is.EqualTo("B"));
            Assert.That((combis as CombinationFactored).Facs.Upper, Is.EqualTo(4.5));
            Assert.That((combis as CombinationFactored).Facs.Lower, Is.EqualTo(3.5));

            //---
        }
    }
}

#endif
