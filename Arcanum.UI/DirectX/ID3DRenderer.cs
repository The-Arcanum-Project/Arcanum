using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using SharpGen.Runtime;
using Vortice.D3DCompiler;
using Vortice.Direct3D;

namespace Arcanum.UI.DirectX;

public interface ID3DRenderer : IDisposable
{
   void EndResize(int newWidth, int newHeight);

   void Resize(int newWidth, int newHeight);

   void Initialize(IntPtr hwnd, int newWidth, int newHeight);
   void Render();

   protected internal static ReadOnlyMemory<byte> CompileBytecode(string shaderName, string entryPoint, string profile)
   {
      var shaderSource = FindShader(shaderName);

      return Compiler.Compile(shaderSource, entryPoint, shaderName, profile);
   }

   private static string FindShader(string shaderName)
   {
      var assembly = Assembly.GetExecutingAssembly();
      var resourceName = "Arcanum.UI.DirectX.Shaders." + shaderName;
      string shaderSource;
      using var stream = assembly.GetManifestResourceStream(resourceName);
      if (stream != null)
      {
         using var reader = new StreamReader(stream);
         shaderSource = reader.ReadToEnd();
      }
      else
         throw new FileNotFoundException("Shader file not found: " + resourceName);

      return shaderSource;
   }

   [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
   protected internal static Blob CompileBytecodeBlob(string shaderName, string entryPoint, string profile)
   {
      var shaderSource = FindShader(shaderName);

      // Use the overload that gives you the result and errors as Blob objects.
      var result = Compiler.Compile(shaderSource,
                                    entryPoint,
                                    shaderName,
                                    profile,
                                    out var shaderBlob,
                                    out var errorBlob);

      // ALWAYS check for compilation errors.
      if (result.Failure)
      {
         // If there's an error, get the message from the errorBlob.
         var errorMessage = errorBlob?.AsString() ?? "Unknown compilation error.";
         errorBlob?.Dispose();
         shaderBlob?.Dispose();
         throw new($"Shader compilation failed: {errorMessage}");
      }

      // The compilation was successful, but the error blob might still be allocated.
      // It's good practice to dispose of it.
      errorBlob?.Dispose();

      // Return the valid, compiled shader blob.
      // The calling code is now responsible for disposing this blob.
      return shaderBlob;
   }
}