using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;

namespace Arcanum.Core.CoreSystems.SavingSystem;

public class AgsFileContext
{
   /// <summary>
   /// The StringBuilder used to construct the AGS file content.
   /// </summary>
   public IndentedStringBuilder Sb { get; }
   /// <summary>
   /// The type of AGS objects being saved (e.g., Item, NPC, Quest).
   /// </summary>
   public Type AgsType { get; }
   /// <summary>
   /// A list of AGS objects to be saved. All must be of the same type as specified in <see cref="AgsType"/>.
   /// </summary>
   public List<IAgs> AgsObjects { get; }
   /// <summary>
   /// The character(s) used to denote comments in the AGS format (e.g., "#").
   /// </summary>
   public string CommentChar { get; }

   private AgsFileContext(Type agsType, List<IAgs> agsObjects, string commentChar = "#")
   {
      Sb = new();
      AgsType = agsType;
      AgsObjects = agsObjects;
      CommentChar = commentChar;
   }

   /// <summary>
   /// Creates a new AgsFileContext for the specified type of AGS objects.
   /// </summary>
   /// <param name="agsObjects"></param>
   /// <typeparam name="T"></typeparam>
   /// <returns></returns>
   public static AgsFileContext Create<T>(List<IAgs> agsObjects) where T : IAgs
   {
      return Create(typeof(T), agsObjects);
   }

   public static AgsFileContext Create(Type agsType, List<IAgs> agsObjects)
   {
      return !typeof(IAgs).IsAssignableFrom(agsType)
                ? throw new ArgumentException($"Type {agsType.FullName} does not implement IAgs.")
                : new(agsType, agsObjects);
   }

   public string BuildContext()
   {
      foreach (var ags in AgsObjects)
      {
         // TODO generate the object header

         if (ags.GetType() != AgsType)
            throw new
               ArgumentException($"All objects must be of type {AgsType.FullName}. Found {ags.GetType().FullName}.");

         using (Sb.Indent())
         {
            var agsContext = ags.ToAgsContext(CommentChar);
            agsContext.BuildContext(Sb);
            Sb.AppendLine();
         }
      }

      return Sb.ToString();
   }
}