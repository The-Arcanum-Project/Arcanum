using System.Text;
using Arcanum.Core.CoreSystems.Common;
using InstitutionPresence = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects.InstitutionPresence;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class InstitutionWriter() : SetupFileWriter([typeof(Location)], "08_institutions.txt")
{
   // windows-1252 encoding
   public override Encoding FileEncoding { get; } = Encoding.GetEncoding(1252);

   public override IndentedStringBuilder WriteFile()
   {
      var sb = new IndentedStringBuilder();
      using (sb.BlockWithName("locations"))
         foreach (var location in Globals.Locations.Values)
            using (sb.BlockWithName(location.UniqueId))
            {
               Enum iField = InstitutionPresence.Field.Institution;
               Enum isPresent = InstitutionPresence.Field.IsPresent;
               for (var i = 0; i < location.InstitutionPresences.Count; i++)
                  sb.Append(SavingUtil.FormatValue(SavingValueType.Identifier,
                                                   location.InstitutionPresences[i],
                                                   iField))
                    .Append(" = ")
                    .AppendLine(SavingUtil.FormatValue(SavingValueType.Bool,
                                                       location.InstitutionPresences[i],
                                                       isPresent));
            }

      return sb;
   }
}