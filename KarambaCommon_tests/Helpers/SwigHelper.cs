namespace NUnitLite.Tests.Helpers
{
    using System.Runtime.InteropServices;

    public static class SwigHelper
    {
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