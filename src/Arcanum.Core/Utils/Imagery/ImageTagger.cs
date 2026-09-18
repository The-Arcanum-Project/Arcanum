using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Text;
using Arcanum.Core.CoreSystems.IO;

namespace Arcanum.Core.Utils.Imagery;

public static class ImageTagger
{
   /// <summary>
   ///    Embeds a string into the Blue Channel LSB of the first N pixels
   ///    and adds standard PNG metadata.
   /// </summary>
   /// <param name = "bitmap" >The source bitmap (Must be writable).</param>
   /// <param name = "tag" >The short ASCII string to embed.</param>
   /// <param name = "addMetadataChunk" >If true, adds standard PNG text chunk.</param>
   public static void TagImage(Bitmap bitmap, string tag, bool addMetadataChunk)
   {
      if (string.IsNullOrEmpty(tag))
         return;

      // Convert string to ASCII bytes and append Null Terminator (0x00)
      var data = Encoding.ASCII.GetBytes(tag + "\0");

      // Ensure image is large enough (8 pixels needed per byte)
      if (bitmap.Width * bitmap.Height < data.Length * 8)
         throw new InvalidOperationException("Image is too small to hold this tag.");

      // LSB Pixel Embedding
      unsafe
      {
         // Lock the bitmap in memory so we can access raw bytes
         // We force Format32bppArgb for consistent B-G-R-A memory layout
         var bmpData = bitmap.LockBits(new(0, 0, bitmap.Width, bitmap.Height),
                                       ImageLockMode.ReadWrite,
                                       PixelFormat.Format32bppArgb);

         try
         {
            var ptr = (byte*)bmpData.Scan0;
            var dataIndex = 0;
            var bitIndex = 0;

            while (dataIndex < data.Length)
            {
               var bit = (byte)((data[dataIndex] >> bitIndex) & 1);

               // Clear LSB of Blue channel and set it to our bit
               ptr[0] = (byte)((ptr[0] & 0xFE) | bit);
               ptr += 4;
               bitIndex++;
               if (bitIndex == 8)
               {
                  bitIndex = 0;
                  dataIndex++;
               }
            }
         }
         finally
         {
            bitmap.UnlockBits(bmpData);
         }
      }

      // Standard PNG Metadata (iTXt/tEXt)
      // Setting a PropertyItem 
      // with ID 0x0131 (Software) is the standard way GDI+ writes text chunks.
      if (addMetadataChunk)
         SetMetadata(bitmap, 0x0131, tag); // 0x0131 = 'Software' tag in EXIF/TIFF/PNG
   }

   private static string GetWatermarkTag() => $"Created with {AppData.FullTitle}";

   public static void ExportTaggedTexture(string filePath, Bitmap texture, ImageFormat format, bool forceWaterMark = false)
   {
      if (forceWaterMark || Config.Settings.SavingConfig.WriteInvisibleWaterMarkInImages)
         TagImage(texture, GetWatermarkTag(), true);

      IO.SaveBitmap(filePath, texture, format);
   }

   /// <summary>
   ///    Reads the tag back from the image to verify provenance.
   /// </summary>
   public static string ReadTag(Bitmap bitmap)
   {
      const int maxBytes = 1024;
      unsafe
      {
         var bmpData = bitmap.LockBits(new(0, 0, bitmap.Width, bitmap.Height),
                                       ImageLockMode.ReadOnly,
                                       PixelFormat.Format32bppArgb);

         try
         {
            var ptr = (byte*)bmpData.Scan0;
            var bytes = new List<byte>();

            // We don't know the length, so we read until we hit the NULL terminator or run out of pixels.
            for (var i = 0; i < maxBytes; i++)
            {
               byte currentByte = 0;

               // Reconstruct byte from 8 pixels
               for (var bit = 0; bit < 8; bit++)
               {
                  // Extract LSB from Blue channel
                  var val = ptr[0] & 1;

                  // Shift it into position
                  currentByte |= (byte)(val << bit);

                  ptr += 4;
               }

               // Null terminator found
               if (currentByte == 0)
                  break;

               bytes.Add(currentByte);
            }

            return Encoding.ASCII.GetString(bytes.ToArray());
         }
         finally
         {
            bitmap.UnlockBits(bmpData);
         }
      }
   }

   private static void SetMetadata(Bitmap bmp, int id, string text)
   {
      // PropertyItem has no public constructor, so we clone an existing one or hack it via reflection
      // Reflection is cleanest for creating a new instance.
#pragma warning disable SYSLIB0050
      var propItem = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));
#pragma warning restore SYSLIB0050

      propItem.Id = id;
      propItem.Type = 2; // ASCII
      propItem.Value = Encoding.ASCII.GetBytes(text + "\0");
      propItem.Len = propItem.Value.Length;

      bmp.SetPropertyItem(propItem);
   }
}