using System.ComponentModel;
using System.Reflection;
using Arcanum.API.Attributes;

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
   [CustomResetMethod(nameof(ResetToDefaults))]
   public Enum Title { get; set; } = title;

   /// <summary>
   /// The fields to be displayed in the main view of the object.
   /// </summary>
   [CustomResetMethod(nameof(ResetToDefaults))]
   public Enum[] ViewFields { get; set; } = viewFields;

   /// <summary>
   /// The fields to be displayed when the object is embedded within another object's view.
   /// </summary>
   [CustomResetMethod(nameof(ResetToDefaults))]
   public Enum[] EmbeddedFields { get; set; } = embeddedFields;

   /// <summary>
   /// The fields to be displayed in a short info panel or tooltip.
   /// </summary>
   [CustomResetMethod(nameof(ResetToDefaults))]
   public Enum[] ShortInfoFields { get; set; } = shortInfoFields;

   /// <summary>
   /// If <c>true</c>, the system will attempt to infer actions for embedded objects based on their context.
   /// </summary>
   [DefaultValue(true)]
   public bool InferActionsInEmbedded { get; set; } = true;
   
   public object ResetToDefaults(PropertyInfo propertyInfo)
   {
      switch (propertyInfo.Name)
      {
         case nameof(Title):
            return (Enum)Activator.CreateInstance(Title.GetType(), 0)!;
         case nameof(ViewFields):
            case nameof(EmbeddedFields):
            case nameof(ShortInfoFields):
               return Enum.GetValues(Title.GetType()).Cast<Enum>().ToArray();
         default:
            throw new ArgumentOutOfRangeException();
      }
   }
}