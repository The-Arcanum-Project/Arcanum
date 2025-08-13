using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Pfim;
using ImageFormat = Pfim.ImageFormat;

namespace Arcanum.Core.CoreSystems.GFX;

public static class ImageReader
{
   /// <summary>
   /// Loads a DDS image from the specified file path and returns it as a Bitmap
   /// </summary>
   /// <param name="filePath"></param>
   /// <returns></returns>
   public static Bitmap ReadImage(string filePath)
   {
      if (!File.Exists(filePath))
         return new (1, 1);
      using var image = Pfimage.FromFile(filePath);
      PixelFormat format;

      switch (image.Format)
      {
         case ImageFormat.Rgba32:
            format = PixelFormat.Format32bppArgb;
            break;
         case ImageFormat.Rgb24:
            format = PixelFormat.Format24bppRgb;
            break;
         case ImageFormat.R5g5b5:
            format = PixelFormat.Format16bppRgb555;
            break;
         case ImageFormat.Rgb8:
         case ImageFormat.R5g6b5:
         case ImageFormat.R5g5b5a1:
         case ImageFormat.Rgba16:
         default:
            throw new InvalidOperationException("Unsupported pixel format.");
      }

      var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
      try
      {
         var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
         var bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, data);
         return new (bitmap);
      }
      finally
      {
         handle.Free();
      }
   }
}