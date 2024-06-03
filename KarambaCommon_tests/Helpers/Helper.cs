namespace KarambaCommon.Tests.Helpers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using NUnit.Framework;
    using static Karamba.Utilities.IniConfigData;
    using UnitSystem = Karamba.Utilities.UnitSystem;

    // using System.Runtime.InteropServices;

    public static class Helper
    {
        /*public static T CastTo<T>(object from, bool cMemoryOwn) // not used
        {
            MethodInfo cPtrGetter =
                from.GetType().GetMethod("getCPtr", BindingFlags.NonPublic | BindingFlags.Static);
            return cPtrGetter == null ? default : (T)System.Activator.CreateInstance(
                typeof(T),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object[] { ((HandleRef)cPtrGetter.Invoke(null, new object[] { from })).Handle, cMemoryOwn },
                null);
        }*/

        /*public static string GetMaterialPropertiesPath()
        {
            string resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            return Path.Combine(resourcePath, "Materials", "MaterialProperties.csv");
        }*/

        /*public static string GetCrossSectionValuesPath()
        {
            string resourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            return Path.Combine(resourcePath, "CrossSections", "CrossSectionValues.bin");
        }*/

        // --- init stuff ---

        public static void InitIniConfigTest(UnitSystem us = UnitSystem.SI, bool show = true) // XXX DRY
        {
            IniConfig.DeleteValuesFromDoc();
            // IniConfig.ReReadWoUserCfg();
            IniConfig.ReReadDefaultsOnly();

            // IniConfig.DefaultUnits();
            var asText = IniConfig.AsText(true, true); // .ToString()
            // Console.WriteLine($"Ini: {asText}");
            Assert.Multiple(() => {
                Assert.That(IniConfig, Has.Count.AtLeast(1));
                Assert.That(IniConfig.UnitSystem, Is.EqualTo(UnitSystem.SI));
                Assert.That(IniConfig.UnitLength, Is.EqualTo("m"));
                Assert.That(IniConfig.UnitLength, Is.Not.EqualTo("cm"));
                Assert.That(asText, Is.Not.Null.And.Not.Empty);
                Assert.That(IniConfig.ToString(), Is.Not.Null.And.Not.Empty);
            });

            IniConfig.UnitSystem = us;
            asText = IniConfig.AsText(true, true); // .ToString()
            Assert.Multiple(() => {
                Assert.That(asText, Is.Not.Null.And.Not.Empty);
                Assert.That(IniConfig.ToString(), Is.Not.Null.And.Not.Empty);
            });
            if (show) Console.WriteLine($"Ini: {asText}");
        }
    }
}
