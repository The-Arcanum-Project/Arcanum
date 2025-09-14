namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

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