using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;

/// <summary>
/// All <see cref="IEu5Object"/> schould be activated here to ensure all properties are set correctly.
/// </summary>
public static class Eu5Activator
{
   private static int _unNamedObjectCounter;

   private static string GenerateUniqueIdForUnnamedObject<T>() where T : IEu5Object => $"<unnamed_{typeof(T).Name}_{_unNamedObjectCounter++}>";

   public static T CreateEmbeddedInstance<T>(string? uniqueId, StatementNode node) where T : IEu5Object<T>, new() => new()
   {
      UniqueId = uniqueId ?? GenerateUniqueIdForUnnamedObject<T>(),
      Source = Eu5FileObj.Embedded,
      FileLocation = node.GetLocationData(),
   };

   public static T CreateInstance<T>(string uniqueId, Eu5FileObj source, StatementNode node)
      where T : IEu5Object<T>, new()
   {
      var t = IEu5Object<T>.CreateInstance(uniqueId, source);
      t.FileLocation = node.GetLocationData();
      return t;
   }

   private static Eu5ObjectLocation GetLocationData(this StatementNode node)
   {
      var (_, charPos) = node.GetEndLocation();
      var length = charPos - node.KeyNode.Start;
      return new(node.KeyNode.Column,
                 node.KeyNode.Line,
                 length,
                 node.KeyNode.Start);
   }
}