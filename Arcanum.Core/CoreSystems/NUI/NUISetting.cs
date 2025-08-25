namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// Defines how an object should be presented in the NUI (Navigation User Interface).
/// </summary>
/// <param name="title"></param>
/// <param name="viewFields"></param>
/// <param name="embeddedFields"></param>
/// <param name="shortInfoFields"></param>
public class NUISetting(Enum title,
                        Enum[] viewFields,
                        Enum[] embeddedFields,
                        Enum[] shortInfoFields)
{
   /// <summary>
   /// The field to be used as the title in the UI (e.g., in lists or headers).
   /// </summary>
   public Enum Title { get; set; } = title;

   /// <summary>
   /// The fields to be displayed in the main view of the object.
   /// </summary>
   public Enum[] ViewFields { get; set; } = viewFields;

   /// <summary>
   /// The fields to be displayed when the object is embedded within another object's view.
   /// </summary>
   public Enum[] EmbeddedFields { get; set; } = embeddedFields;

   /// <summary>
   /// The fields to be displayed in a short info panel or tooltip.
   /// </summary>
   public Enum[] ShortInfoFields { get; set; } = shortInfoFields;
}