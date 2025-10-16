using System.Numerics;
using System.Runtime.InteropServices;

namespace Arcanum.UI.DirectX.ComputeShaderClasses;

[StructLayout(LayoutKind.Sequential)]
public struct TopTwoColors
{
   public Float4 MostFrequent;
   public Float4 SecondMostFrequent;
}

[StructLayout(LayoutKind.Sequential)]
public struct GradientInstanceData // Renamed for clarity
{
   public Vector2 Offset;
   public Vector2 Size;
   public Float4 Color1; // Top-left color
   public Float4 Color2; // Bottom-right color
}