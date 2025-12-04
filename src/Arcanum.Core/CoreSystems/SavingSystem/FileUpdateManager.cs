using System.Diagnostics;
using System.Text;
using System.Windows;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;
using Common.UI.MBox;

namespace Arcanum.Core.CoreSystems.SavingSystem;

public static class FileUpdateManager
{
   public static IndentedStringBuilder? UpdateEu5ObjectsInFile(List<IEu5Object> objectsToUpdate)
   {
      if (objectsToUpdate.Count == 0)
      {
         Log("No objects to update in file. We should have returned earlier.");
         return null;
      }

      Debug.Assert(objectsToUpdate.All(o => o.Source == objectsToUpdate[0].Source), "All objects must belong to the same source file.");

      HashSet<IEu5Object> existingToUpdate = [];
      List<IEu5Object> newObjects = [];

      foreach (var obj in objectsToUpdate)
         if (obj.FileLocation == Eu5ObjectLocation.Empty)
            newObjects.Add(obj);
         else
            existingToUpdate.Add(obj);

      // get the original file content depending on whether we have existing or new objects
      var original = existingToUpdate.Count > 0
                        ? IO.IO.ReadAllTextUtf8(objectsToUpdate[0].Source.Path.FullPath)!
                        : newObjects.Count > 0
                           ? IO.IO.ReadAllTextUtf8(newObjects[0].Source.Path.FullPath)!
                           : string.Empty;

      Debug.Assert(original != null, nameof(original) + " != null");

      // Clear the cache before updating to ensure we get fresh data.
      PropertyOrderCache.Clear();

      // Get all objects that are already in the file. This does not contain newObjects.
      var objectsInFile = objectsToUpdate[0].Source.ObjectsInFile.OrderBy(x => x.FileLocation.CharPos).ToArray();

      if (!VerifyFileIntegrity(original.Length, objectsInFile))
      {
         Log("File integrity verification failed. Aborting update.", LogLevel.CRT);
         UIHandle.Instance.PopUpHandle
                 .ShowMBox("The file integrity verification failed. Aborting the update to prevent data corruption.\nPlease restart the application and provide your steps to the developers.",
                           "File Update Error",
                           MBoxButton.OK,
                           MessageBoxImage.Error);
         return null;
      }

      var sb = new IndentedStringBuilder(original.Length + existingToUpdate.Count * 20);
      var isb = new IndentedStringBuilder();
      var deltaLines = 0;
      var deltaChars = 0;
      var currentPos = 0;
      var spaces = Config.Settings.SavingConfig.SpacesPerIndent;

      for (var i = 0; i < objectsInFile.Length; i++)
      {
         isb.Clear();
         var iterator = objectsInFile[i];

         // We are an object that needs to be updated
         if (existingToUpdate.Contains(iterator))
         {
            // Append the text before the object
            sb.InnerBuilder.Append(original, currentPos, iterator.FileLocation.CharPos - currentPos);
            // Update the current position in the original file
            currentPos = iterator.FileLocation.End;

            // Estimate the indentation level based on the column
            var indentLevel = Math.DivRem(iterator.FileLocation.Column, spaces, out var remainder);

            // We have an object defined in one line, disgusting we can not really deal with it,
            // so we just append a new line and dump our formatted obj there :P
            if (remainder != 0 && i > 0)
               isb.InnerBuilder.AppendLine();

            isb.SetIndentLevel(indentLevel);

            // Format the object into the isb
            if (iterator.InjRepType != InjRepType.None)
               iterator.ToAgsContext().BuildContext(isb, [.. iterator.SaveableProps], iterator.InjRepType, true);
            else
               iterator.ToAgsContext().BuildContext(isb);

            var lineOffset = CountNewLinesInStringBuilder(isb.InnerBuilder) + 1;
            var itLength = isb.InnerBuilder.Length;
            var oldLineCount = CountLinesInOriginal(original, iterator.FileLocation);
            var oldCharCount = Math.Max(0, iterator.FileLocation.Length);

            iterator.FileLocation.Line += deltaLines;
            iterator.FileLocation.CharPos = sb.InnerBuilder.Length;
            iterator.FileLocation.Length = itLength;
            iterator.FileLocation.Column = indentLevel * spaces;

            deltaLines += lineOffset - oldLineCount;
            deltaChars += itLength - oldCharCount;
            isb.Merge(sb);
         }
         // We are an object which already exists in the file and just have to be copied over and have it's position adjusted
         else
         {
            // We update the line and char pos offsets based on the changes made so far
            iterator.FileLocation.CharPos += deltaChars;
            iterator.FileLocation.Line += deltaLines;
         }
      }

      sb.InnerBuilder.Append(original, currentPos, original.Length - currentPos);

      // Append the new objects at the end of the file
      foreach (var obj in newObjects)
      {
         isb.Clear();
         obj.ToAgsContext().BuildContext(isb);
         obj.FileLocation = new(0,
                                CountNewLinesInStringBuilder(sb.InnerBuilder),
                                isb.InnerBuilder.Length,
                                sb.InnerBuilder.Length);
         obj.Source.ObjectsInFile.Add(obj);
         isb.Merge(sb);
      }

      if (!VerifyFileIntegrity(sb.InnerBuilder.Length, objectsInFile))
      {
         Log("File integrity verification failed. Aborting update.", LogLevel.CRT);
         UIHandle.Instance.PopUpHandle
                 .ShowMBox("The file integrity verification failed. Aborting the update to prevent data corruption.\nPlease restart the application and provide your steps to the developers.",
                           "File Update Error",
                           MBoxButton.OK,
                           MessageBoxImage.Error);
         return null;
      }

      foreach (var obj in objectsToUpdate)
         SaveMaster.RemoveObjectFromChanges(obj);

      return sb;
   }

   private static int CountLinesInOriginal(string originalText, Eu5ObjectLocation location)
   {
      var lineCount = 0;
      var endPos = Math.Min(location.CharPos + location.Length, originalText.Length);
      for (var i = location.CharPos; i < endPos; i++)
         if (originalText[i] == '\n')
            lineCount++;

      return location.Length > 0 ? lineCount + 1 : 0;
   }

   private static int CountNewLinesInStringBuilder(StringBuilder sb)
   {
      ArgumentNullException.ThrowIfNull(sb);

      var newlineCount = 0;

      for (var i = 0; i < sb.Length; i++)
         if (sb[i] == '\n')
            newlineCount++;

      return newlineCount;
   }

   private static bool VerifyFileIntegrity(int fileLength, ICollection<IEu5Object> objectsInFile)
   {
      var lastEndPos = -1;
      var sortedObjs = objectsInFile.OrderBy(x => x.FileLocation.CharPos).ToArray();
      foreach (var obj in sortedObjs)
      {
#if DEBUG
         Debug.Assert(obj.FileLocation is { CharPos: >= 0, Length: >= 0 } &&
                      obj.FileLocation.CharPos + obj.FileLocation.Length <= fileLength,
                      "All modified objects must have a valid FileLocation within the bounds of the new file.");

         if (lastEndPos != -1)
            if (obj.FileLocation.CharPos <= lastEndPos)
               Debug.Fail("Modified objects must not overlap in the new file.");
#else
         if (obj.FileLocation is not { CharPos: >= 0, Length: >= 0 } ||
             obj.FileLocation.CharPos + obj.FileLocation.Length > fileLength)
            return false;

         if (lastEndPos != -1)
            if (obj.FileLocation.CharPos <= lastEndPos)
               return false;
#endif
         lastEndPos = obj.FileLocation.CharPos + obj.FileLocation.Length - 1;
      }

      return true;
   }

   private static void Log(string message, LogLevel level = LogLevel.DBG) => ArcLog.WriteLine(CommonLogSource.FUM, level, message);
}