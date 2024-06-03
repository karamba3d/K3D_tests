namespace KarambaCommon.Tests.Utilities
{
    using System;
    using NUnit.Framework;
    using static Karamba.Utilities.StringUtil;

    public class StringUtil_tests
    {
        [Test]
        public void WhenEmptyTest()
        {
            Assert.Multiple(() => {
                Assert.That(WhenEmpty("a", "b"), Is.EqualTo("a"));
                Assert.That(WhenEmpty(null, "b"), Is.EqualTo("b"));
                Assert.That(WhenEmpty(string.Empty, "b"), Is.EqualTo("b"));
                Assert.That(WhenEmpty(" ", "b", true), Is.EqualTo("b"));
                Assert.That(WhenEmpty(" ", "b", false), Is.EqualTo(" "));
            });
        }

        [Test]
        public void AddSeparatorTest()
        {
            Assert.Multiple(() => {
                Assert.That(AddSeparator(string.Empty, ","), Is.EqualTo(string.Empty));
                Assert.That(AddSeparator(" ", ","), Is.EqualTo(" "));
                Assert.That(AddSeparator(",", ","), Is.EqualTo(","));
                Assert.That(AddSeparator("a,", ","), Is.EqualTo("a,"));
                Assert.That(AddSeparator("a", ","), Is.EqualTo("a,"));
                Assert.That(AddSeparator("a, ", ","), Is.EqualTo("a, "));
                Assert.That(AddSeparator("a,b", ","), Is.EqualTo("a,b,"));
                Assert.That(AddSeparator("a,b,", ","), Is.EqualTo("a,b,"));
            });
        }

        [Test]
        public void DeQuoteTest()
        {
            Assert.Multiple(() => {
                Assert.That("\"\"".DeQuote(), Is.EqualTo(string.Empty));
                Assert.That("\"abc\"".DeQuote(), Is.EqualTo("abc"));
                Assert.That("abc".DeQuote(), Is.EqualTo("abc"));
                Assert.That("\"abc\"def\"".DeQuote(), Is.EqualTo("abc\"def"));
            });
        }

        [Test]
        public void LineTrimTest()
        {
            Assert.Multiple(() => {
                Assert.That(string.Empty.LineTrim(), Is.EqualTo(string.Empty));
                Assert.That("\n".LineTrim(), Is.EqualTo("\n"));
                Assert.That("  \n".LineTrim(), Is.EqualTo("\n"));
                Assert.That("abc  \n".LineTrim(), Is.EqualTo("abc\n"));
                Assert.That("a b c  \n".LineTrim(), Is.EqualTo("a b c\n"));
                Assert.That("abc  \n def  \n".LineTrim(), Is.EqualTo("abc\ndef\n"));
                Assert.That("abc  \n def  \n".LineTrim(true), Is.EqualTo("abc\n def\n"));
            });
        }

        [Test]
        public void AsHexTest()
        {
            var t1 = new byte[] { 1, 2, 3, 4, 0xff };
            Assert.That(t1.AsHex(), Is.EqualTo("01020304ff"));
        }

        [Test]
        public void ShortenTest()
        {
            Assert.Multiple(() => {
                Assert.That("0123456789".Shorten(20), Is.EqualTo("0123456789"));
                Assert.That("0123456789".Shorten(10), Is.EqualTo("0123456789"));
                Assert.That("0123456789".Shorten(4), Is.EqualTo("012…"));
            });
        }

        // TODO: FromColor, ToColor

        [Test]
        public void ToFormatTest()
        {
            Assert.Multiple(() => {
                Assert.That("f".ToFormat(), Is.EqualTo("{0:f}"));
                Assert.That(":f".ToFormat(), Is.EqualTo("{0:f}"));
                Assert.That("{0:f}".ToFormat(), Is.EqualTo("{0:f}"));
            });
        }

        [Test]
        public void ParseAsTest()
        {
            string s0 = null;
            Assert.Multiple(() => {
                Assert.That("123".ParseAs<int>(), Is.EqualTo(123));
                Assert.That("123".ParseAs(0), Is.EqualTo(123));
                Assert.That("123".ParseAs<int>("0"), Is.EqualTo(123));
                Assert.That(s0.ParseAs<int>("0"), Is.EqualTo(0));
                Assert.That(s0.ParseAs<int>(), Is.EqualTo(0));
                Assert.That(s0.ParseAs(0), Is.EqualTo(0));
                // TODO: other types
                // TODO: on lists of strings
            });
        }

        [Test]
        public void ParseAsDynTest()
        {
            string s0 = null;
            Assert.Multiple(() => {
                Assert.That((int)"123".ParseAs(typeof(int)), Is.EqualTo(123));
                Assert.That((int)"123".ParseAs(typeof(int), 0), Is.EqualTo(123));
                Assert.That((int)s0.ParseAs(typeof(int), 0), Is.EqualTo(0));
                Assert.That((int)s0.ParseAs(typeof(int), "0"), Is.EqualTo(0));
                Assert.That((int)"123".ParseAs("System.Int32"), Is.EqualTo(123));
                Assert.That((int)"123".ParseAs("System.Int32", 0), Is.EqualTo(123));
                Assert.That((int)"123".ParseAs("System.Int32", "0"), Is.EqualTo(123));
                Assert.That((int)"123".ParseAs("int"), Is.EqualTo(123));
                Assert.That((int)"123".ParseAs("int", 0), Is.EqualTo(123));
                Assert.That((int)"123".ParseAs("int", "0"), Is.EqualTo(123));
            });
        }

        // NormalizeUnit()s test in Licenses/unit_tests.cs
    }
}
