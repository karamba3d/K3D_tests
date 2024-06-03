namespace KarambaCommon.Tests.Helpers
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// SWIG related helpers for tests.
    /// </summary>
    public static class SwigHelper
    {
        /// <summary>
        /// Make parts of the C++ model accessible from C# for testing
        /// purposes, or such.
        /// </summary>
        /// <typeparam name="T">result type</typeparam>
        /// <param name="from">model part</param>
        /// <param name="cMemoryOwn">...</param>
        /// <returns></returns>
        public static T CastTo<T>(object from, bool cMemoryOwn)
        {
            System.Reflection.MethodInfo cPtrGetter = from.GetType().GetMethod("getCPtr", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return cPtrGetter == null ? default : (T)System.Activator.CreateInstance(
                typeof(T),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new object[] { ((HandleRef)cPtrGetter.Invoke(null, new object[] { from })).Handle, cMemoryOwn },
                null);
        }
    }
}
