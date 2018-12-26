namespace NBWTA.Utils
{
    using System.Reflection;

    public static class AssemblyVersion
    {
        public static string Get() =>
            AssemblyName.GetAssemblyName(typeof(AssemblyVersion).Assembly.Location).Version.ToString();
    }
}