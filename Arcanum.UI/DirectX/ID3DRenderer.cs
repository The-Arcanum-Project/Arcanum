using System.IO;
using System.Reflection;
using Vortice.D3DCompiler;

namespace Arcanum.UI.DirectX;

public interface ID3DRenderer : IDisposable
{
    void Resize(int newWidth, int newHeight);
    void Initialize(IntPtr hwnd, int newWidth, int newHeight);
    void Render();
    
    protected static ReadOnlyMemory<byte> CompileBytecode(string shaderName, string entryPoint, string profile)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Arcanum.UI.DirectX.Shaders." + shaderName;
        string shaderSource;
        using (var stream = assembly.GetManifestResourceStream(resourceName))
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                shaderSource = reader.ReadToEnd();
            }
            else
                throw new FileNotFoundException("Shader file not found: " + resourceName);
            

        return Compiler.Compile(shaderSource, entryPoint, shaderName, profile);
    }
}