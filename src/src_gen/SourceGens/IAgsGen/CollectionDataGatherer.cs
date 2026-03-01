using Microsoft.CodeAnalysis;

namespace ParserGenerator.IAgsGen;

public static class CollectionDataGatherer
{
   public static CollectionFormatProfile ParseCollectionProfile(AttributeData? attributeData)
   {
      var profile = new CollectionFormatProfile();
      if (attributeData == null)
         return profile;

      foreach (var arg in attributeData.NamedArguments)
      {
         // arg.Key is the property name (string)
         // arg.Value is a TypedConstant
         // arg.Value.Value is the actual boxed value

         if (arg.Value.Value is null)
            continue;

         switch (arg.Key)
         {
            case nameof(CollectionFormatProfile.LayoutMode):
               if (arg.Value.Value is int layoutModeVal)
                  profile.LayoutMode = (CollectionLayoutMode)layoutModeVal;

               break;

            case nameof(CollectionFormatProfile.ItemsPerRow):
               if (arg.Value.Value is int itemsPerRowVal)
                  profile.ItemsPerRow = itemsPerRowVal;

               break;

            case nameof(CollectionFormatProfile.AlignColumns):
               if (arg.Value.Value is bool alignColumnsVal)
                  profile.AlignColumns = alignColumnsVal;

               break;

            case nameof(CollectionFormatProfile.ColumnWidth):
               if (arg.Value.Value is int columnWidthVal)
                  profile.ColumnWidth = columnWidthVal;

               break;

            case nameof(CollectionFormatProfile.SortMode):
               if (arg.Value.Value is int sortModeVal)
                  profile.SortMode = (CollectionSortMode)sortModeVal;

               break;

            case nameof(CollectionFormatProfile.WriteEmpty):
               if (arg.Value.Value is bool writeEmptyVal)
                  profile.WriteEmpty = writeEmptyVal;

               break;
         }
      }

      return profile;
   }
}