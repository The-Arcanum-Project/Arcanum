namespace Arcanum.Core.CoreSystems.NUI;

public class NUISetting
{
   public NUISetting(Enum title,
                     Enum[] viewFields,
                     Enum[] embeddedFields,
                     Enum[] shortInfoFields)
   {
      Title = title;
      ViewFields = viewFields;
      EmbeddedFields = embeddedFields;
      ShortInfoFields = shortInfoFields;
   }

   public Enum Title { get; set; }

   public Enum[] ViewFields { get; set; }

   public Enum[] EmbeddedFields { get; set; }

   public Enum[] ShortInfoFields { get; set; }
}