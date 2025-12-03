namespace ParserGenerator.ParserGen;

public static class DispatchDataHandler
{
   public static List<DispatchData> GroupByLength(List<PropertyData> properties)
   {
      var lengthGroups = new Dictionary<int, DispatchData>();

      foreach (var prop in properties)
      {
         var length = prop.PropertyMetadata.Keyword.Length;

         if (!lengthGroups.ContainsKey(length))
         {
            lengthGroups[length] = new()
            {
               Length = length, Properties = [],
            };
         }

         lengthGroups[length].Properties.Add(prop);
      }

      var result = lengthGroups.Values.ToList();
      result.Sort((a, b) => a.Properties[0].PropertyMetadata.Keyword.Length.CompareTo(b.Properties[0].PropertyMetadata.Keyword.Length));
      return result;
   }
}

public class DispatchData
{
   public int Length { get; set; }
   public List<PropertyData> Properties { get; set; } = [];
}