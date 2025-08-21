using System.Text.Json.Serialization;

public class NUISetting
{
   public NUISetting(Enum title,
                     Enum description,
                     Enum[] viewFields,
                     Enum[] embeddedFields,
                     Enum[] shortInfoFields,
                     Enum[] navigations)
   {
      Title = title;
      Description = description;
      ViewFields = viewFields;
      EmbeddedFields = embeddedFields;
      ShortInfoFields = shortInfoFields;
      Navigations = navigations;
   }

   public Enum Title { get; set; }

   public Enum Description { get; set; }

   public Enum[] ViewFields { get; set; } = [];

   public Enum[] EmbeddedFields { get; set; } = [];

   public Enum[] ShortInfoFields { get; set; } = [];

   public Enum[] Navigations { get; set; } = [];
}