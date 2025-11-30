using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.GameObjects.Economy;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public class MarketWriter() : SetupFileWriter([typeof(Market)], "03_markets.txt")
{
   public override IndentedStringBuilder WriteFile()
   {
      var isb = new IndentedStringBuilder();
      using (isb.BlockWithName("market_manager"))
         foreach (var market in Globals.Markets.Values)
            SavingUtil.FormatValue(SavingValueType.Identifier, market, Market.Field.Location);

      return isb;
   }
}