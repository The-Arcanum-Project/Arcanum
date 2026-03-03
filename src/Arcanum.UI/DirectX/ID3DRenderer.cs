using System.IO;
using System.Reflection;
using Arcanum.Core.Utils;
using Vortice.D3DCompiler;

namespace Arcanum.UI.DirectX;

public interface ID3DRenderer : IDisposable
{
   void EndResize(int newWidth, int newHeight);

   void Resize(int newWidth, int newHeight);

   void Initialize(IntPtr hwnd, int newWidth, int newHeight);
   void Render();

   protected static ReadOnlyMemory<byte> CompileBytecode(string shaderName, string entryPoint, string profile)
   {
      var resourceName = "Arcanum.UI.DirectX.Shaders." + shaderName;
      
      // ReSharper disable once ConvertIfStatementToReturnStatement
      if (!ArcResources.GetResource(resourceName, Assembly.GetExecutingAssembly(), out var shaderSource))
         throw new FileNotFoundException("Shader file not found: " + resourceName);

      return Compiler.Compile(shaderSource, entryPoint, shaderName, profile);
   }
}