using System.Text;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GlobalStates;
using Adjacency = Arcanum.Core.GameObjects.Map.Adjacency;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

public class AdjacencyFileLoading : FileLoadingService
{
   public override List<Type> ParsedObjects => [typeof(Adjacency)];

   public override string GetFileDataDebugInfo()
   {
      return $"Loaded '{Globals.Adjacencies.Count}' adjacencies.\n" +
             $"\tLongest: \t{Globals.Adjacencies.Values.Max(adj => adj.GetLength())}\n" +
             $"\tShortest: \t{Globals.Adjacencies.Values.Min(adj => adj.GetLength())}\n";
   }

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      if (!IO.IO.CreateStreamReader(fileObj.Path.FullPath, Encoding.UTF8, out var sr))
      {
         DiagnosticException.CreateAndHandle(new(0, 0, fileObj.Path.FullPath),
                                             IOError.Instance.FileReadingError,
                                             GetActionName(),
                                             args: [fileObj.Path.FullPath]);
         return false;
      }

      var lineNumber = 0;
      while (sr!.ReadLine() is { } line)
      {
         if (lineNumber == 0)
         {
            // skip the header
            lineNumber++;
            continue;
         }

         if (string.IsNullOrWhiteSpace(line))
            continue;

         var parts = line.Split(';');
         if (parts.Length != 9)
         {
            DiagnosticException.LogWarning(new(lineNumber, 0, fileObj.Path.FullPath),
                                           ParsingError.Instance.InvalidAdjacencyLine,
                                           GetActionName(),
                                           lineNumber,
                                           line,
                                           parts.Length);
         }
         else
         {
            var isValid = true;
            if (!Globals.Locations.TryGetValue(parts[0], out var from))
            {
               DiagnosticException.LogWarning(new(lineNumber, 0, fileObj.Path.FullPath),
                                              ParsingError.Instance.InvalidLocationKey,
                                              GetActionName(),
                                              parts[0]);
               isValid = false;
            }

            if (!Globals.Locations.TryGetValue(parts[1], out var to))
            {
               DiagnosticException.LogWarning(new(lineNumber, 0, fileObj.Path.FullPath),
                                              ParsingError.Instance.InvalidLocationKey,
                                              GetActionName(),
                                              parts[1]);
               isValid = false;
            }

            if (!Enum.TryParse<AdjacencyType>(parts[2], true, out var type))
            {
               DiagnosticException.LogWarning(new(lineNumber, 0, fileObj.Path.FullPath),
                                              ParsingError.Instance.InvalidAdjacencyType,
                                              GetActionName(),
                                              parts[2]);
               isValid = false;
            }

            if (!int.TryParse(parts[4], out var startX))
            {
               DiagnosticException.LogWarning(new(lineNumber, 0, fileObj.Path.FullPath),
                                              ParsingError.Instance.InvalidIntMarkup,
                                              GetActionName(),
                                              parts[4]);
               isValid = false;
            }

            if (!int.TryParse(parts[5], out var startY))
            {
               DiagnosticException.LogWarning(new(lineNumber, 0, fileObj.Path.FullPath),
                                              ParsingError.Instance.InvalidIntMarkup,
                                              GetActionName(),
                                              parts[5]);
               isValid = false;
            }

            if (!int.TryParse(parts[6], out var endX))
            {
               DiagnosticException.LogWarning(new(lineNumber, 0, fileObj.Path.FullPath),
                                              ParsingError.Instance.InvalidIntMarkup,
                                              GetActionName(),
                                              parts[6]);
               isValid = false;
            }

            if (!int.TryParse(parts[7], out var endY))
            {
               DiagnosticException.LogWarning(new(lineNumber, 0, fileObj.Path.FullPath),
                                              ParsingError.Instance.InvalidIntMarkup,
                                              GetActionName(),
                                              parts[7]);
               isValid = false;
            }

            if (!isValid)
               continue;

            Globals.Adjacencies.Add($"{from}-{to}",
                                    new(from!,
                                        to!,
                                        type,
                                        parts[3],
                                        parts[8],
                                        startX,
                                        startY,
                                        endX,
                                        endY));
         }

         lineNumber++;
      }

      return true;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      Globals.Adjacencies.Clear();
      Globals.Adjacencies.TrimExcess();
      return true;
   }
}