namespace Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;

/// <summary>
/// 
/// </summary>
/// <param name="typeName">A descriptive name for the <see cref="FileTypeInformation"/></param>
/// <param name="fileEnding">Must be without the <c>.</c></param>
/// <param name="commentPrefix">If this value is <see cref="string.Empty"/> no comments will be written in those files</param>
public readonly struct FileTypeInformation(string typeName, string fileEnding, string commentPrefix)
{
   public static FileTypeInformation Default = new("EUV-JSON", "txt", "#");

   public readonly string TypeName = typeName;

   /// <summary>
   /// Should be in the format of a file ending, e.g. "txt", "json", "xml", etc.
   /// </summary>
   public readonly string FileEnding = fileEnding;

   /// <summary>
   /// Prefix for comments in this file type, e.g. <c>#</c> for default EU5 files.
   /// </summary>
   public readonly string CommentPrefix = commentPrefix;

   /// <summary>
   /// Returns <c>true</c> if this file type can have comments, i.e. if the <see cref="CommentPrefix"/> is not empty or whitespace.
   /// </summary>
   public bool CanHaveComments => !string.IsNullOrWhiteSpace(CommentPrefix);
}