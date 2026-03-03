using System.Reflection;

namespace Arcanum.Core.Utils;

public static class ArcResources
{
    public static bool GetResource(string resourceName, Assembly assembly, out string resource)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            resource = reader.ReadToEnd();
        }
        else
        {
            resource = string.Empty;
            return false;
        }

        return true;
    }
    
    public static bool GetResource(string resourceName, Type type, out string resource)
    {
        var assembly = Assembly.GetAssembly(type);
        if (assembly != null) return GetResource(resourceName, assembly, out resource);
        resource = string.Empty;
        return false;
    }

    public static bool GetResourceBytes(string resourceName, Assembly assembly, out byte[] resource)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var memoryStream = new MemoryStream();
            resource = memoryStream.ToArray();
        }
        else
        {
            resource = [];
            return false;
        }

        return true;
    }
}