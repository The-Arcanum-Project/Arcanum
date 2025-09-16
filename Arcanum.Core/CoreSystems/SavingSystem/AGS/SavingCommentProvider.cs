namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// Provides methods to generate comments during the saving process. <br/>
/// Custom methods can be define here and referenced in <see cref="PropertySavingMetadata.SavingMethod"/>.
/// </summary>
public static class SavingCommentProvider
{
   /// <summary>
   /// The default comment provider method. <br/>
   /// It generates a comment in the format: "# Saving property: {PropertyName}"
   /// </summary>
   /// <param name="value"></param>
   /// <param name="nxProp"></param>
   /// <returns></returns>
   public static string DefaultCommentProvider(object value, Enum nxProp)
   {
      return $"# Saving property: {nxProp}";
   }
}