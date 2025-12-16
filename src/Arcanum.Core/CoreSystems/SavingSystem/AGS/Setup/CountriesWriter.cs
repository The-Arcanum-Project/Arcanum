using System.Text;
using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class CountriesWriter() : SetupFileWriter([], "") //: SetupFileWriter(SetupParsingManager.NestedSubTypes(Country.Empty), "10_countries.txt")
{
   // windows-1252 encoding
   public override Encoding FileEncoding { get; } = Encoding.GetEncoding(1252);

   public override IndentedStringBuilder WriteFile()
   {
      var builder = new IndentedStringBuilder();
      using (builder.BlockWithName("countries"))
      {
         using (builder.BlockWithName("countries"))
         {
            var countries = Globals.Countries.Values.ToList();
            countries = countries.OrderBy(x => x.UniqueId).ToList();
            foreach (var country in countries)
            {
               ((IAgs)country).ToAgsContext().BuildContext(builder);
               builder.AppendLine();
            }
         }
      }

      return builder;
   }
}