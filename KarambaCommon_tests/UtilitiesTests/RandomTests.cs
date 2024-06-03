#if ALL_TESTS

namespace KarambaCommon.Tests.Utilities
{
    using System.Linq;
    using Karamba.Loads.Combination;
    using NUnit.Framework;

    public class RandomTests
    {
        [Test]
        public void Test()
        {
            LoadCaseCombinationCollection test = null;
            var test2 = test?.OrderedLoadCaseIds.ToList();
        }
    }
}

#endif
