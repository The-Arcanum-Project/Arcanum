using System.Drawing.Imaging;

namespace Arcanum.Core.OldAndDebug;

using System.Collections.Concurrent;

public static class MapDrawing
{
   /// <summary>
   /// 32 bpp
   /// </summary>
   /// <param name="points"></param>
   /// <param name="color"></param>
   /// <param name="bmp"></param>
   public static void DrawPixelsParallel(Memory<Point> points, int color, Bitmap bmp)
   {
      var bmpData = bmp.LockBits(new(0, 0, bmp.Width, bmp.Height),
                                 ImageLockMode.ReadWrite,
                                 PixelFormat.Format32bppArgb);

      var stride = bmpData.Stride / 4;

      unsafe
      {
         var scan0 = (int*)bmpData.Scan0;
         Parallel.ForEach(Partitioner.Create(0, points.Length),
                          range =>
                          {
                             var sliceSpan = points.Span.Slice(range.Item1, range.Item2 - range.Item1);
                             for (var i = 0; i < sliceSpan.Length; i++)
                             {
                                var pt = sliceSpan[i];
                                scan0[pt.Y * stride + pt.X] = color;
                             }
                          });
      }

      bmp.UnlockBits(bmpData);
   }
   
   public static void DrawPixelsParallel(Memory<Point> points, int color, BitmapData bmp)
   {
      var stride = bmp.Stride / 4;

      unsafe
      {
         var scan0 = (int*)bmp.Scan0;
         Parallel.ForEach(Partitioner.Create(0, points.Length),
                          range =>
                          {
                             var sliceSpan = points.Span.Slice(range.Item1, range.Item2 - range.Item1);
                             for (var i = 0; i < sliceSpan.Length; i++)
                             {
                                var pt = sliceSpan[i];
                                scan0[pt.Y * stride + pt.X] = color;
                             }
                          });
      }
   }
}