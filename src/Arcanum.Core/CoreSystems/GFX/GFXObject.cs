namespace Arcanum.Core.CoreSystems.GFX;

public class GFXObject(IconType iconType, string filePath)
{
   public IconType IconType { get; } = iconType;
   public string FilePath { get; } = filePath;

   public Bitmap? Icon
   {
      get
      {
         if (field != null)
            return field;

         switch (IconType)
         {
            case IconType.DDS:
            case IconType.TGA:
               field = ImageReader.ReadImage(FilePath);
               break;
            case IconType.PNG:
            case IconType.TIFF:
               return new(FilePath);
            default:
               throw new ArgumentOutOfRangeException();
         }

         return field;
      }
   }

   public static GFXObject CreateFromPath(string filePath)
   {
      if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
         throw new FileNotFoundException("The specified file does not exist.", filePath);

      var extension = Path.GetExtension(filePath).ToLowerInvariant();
      var iconType = extension switch
      {
         ".dds" => IconType.DDS,
         ".png" => IconType.PNG,
         ".Png" => IconType.PNG,
         ".tiff" or ".tif" => IconType.TIFF,
         ".tga" => IconType.TGA,
         _ => throw new NotSupportedException($"Unsupported file type: {extension}")
      };

      return new(iconType, filePath);
   }
}