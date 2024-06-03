namespace KarambaCommon.Tests.Utilities
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using NUnit.Framework;
    using DynUtil = Karamba.Utilities.DynUtil;
    // using Helper = Karamba.Tests.Helpers.Helper;
    using OD = ObjectDumper;
    // using UnitSystem = Karamba.Utilities.UnitSystem;

    /// <summary>
    /// Enable output for tests.
    /// </summary>
    [SetUpFixture]
    public class SetupTrace
    {
        /// <inheritdoc/>
        [OneTimeSetUp]
        public void StartTest()
        { if (!Trace.Listeners.OfType<ConsoleTraceListener>().Any())
             _ = Trace.Listeners.Add(new ConsoleTraceListener());
        }

        /// <inheritdoc/>
        [OneTimeTearDown]
        public void EndTest() => Trace.Flush();
    }

    [TestFixture]
    internal class Dyn_tests
    {
        [Test]
        public static void GetFrame()
        {
            var ver = Environment.Version;
            var fw = RuntimeInformation.FrameworkDescription;
            Debug.WriteLine(fw);
            Debug.WriteLine(OD.Dump(ver));
            string lookAt = ver.Major >= 6 ? "RunOnCurrentThread" : "ThreadStart";
            var sf = DynUtil.InsideOf(lookAt);
            Assert.That(sf, Is.Not.Null);
        }

        [Test]
        public static void GetAssembly()
        {
            var assm = DynUtil.PerLoadedAssemblies("KarambaCommon_tests");
            Assert.That(assm, Is.Not.Null);
        }

        [Test]
        public static void TestFindAssembly()
        {
            var all = DynUtil.FindLoadedAssemblies("System", true)
                          .Select(a => a.GetName().Name);
            Debug.WriteLine(OD.Dump(all));

            var assemblies1 = DynUtil.FindLoadedAssemblies("System"); // net4.8
            var assemblies2 = DynUtil.FindLoadedAssemblies("System.Runtime"); // net7
            int cnt = Math.Max(assemblies1.Count(), assemblies2.Count());
            Assert.Multiple(() => {
                Assert.That(assemblies1, Is.Not.Null);
                Assert.That(assemblies2, Is.Not.Null);
                Assert.That(cnt, Is.EqualTo(1));
            });

            var assm = DynUtil.FindLoadedAssembly("System") ??
                       DynUtil.FindLoadedAssembly("System.Runtime");
            Assert.That(assm, Is.Not.Null);
        }

        [Test]
        public static void NoGrasshopper()
        {
            var doc = DynUtil.ActiveGrassHopperDoc();
            Assert.That(doc, Is.Null);
            // doc = DynUtil.ActiveGrassHopperDoc(true);
            // Assert.That(doc, Is.Null);
        }

        [Test]
        public static void NoValueTable()
        {
            var vt = DynUtil.ActiveGrassHopperDocValueTable();
            Assert.That(vt, Is.Null);
            // vt = DynUtil.ActiveGrassHopperDocValueTable(true);
            // Assert.That(vt, Is.Null);
        }
    }
}
