using System.Collections.Concurrent;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// Provides a thread-safe, cached mechanism for getting the sorted list of saveable properties for a given IAgs type and settings.
/// This avoids re-calculating the sort order for every single object instance.
/// </summary>
public static class PropertyOrderCache
{
   // The key for our cache. It uniquely identifies a sorting configuration.
   private readonly struct CacheKey(Type type, AgsSettings settings) : IEquatable<CacheKey>
   {
      private Type Type { get; } = type;
      private bool CustomSaveOrder { get; } = settings.CustomSaveOrder;
      private bool SortCollectionsSeparately { get; } = settings.SortCollectionsAndPropertiesSeparately;

      // Convert the list of enums to a stable string representation for the key.
      private string? CustomOrderKey { get; } = settings.CustomSaveOrder
                                                   ? string.Join(";", settings.SaveOrder.Select(e => e.ToString()))
                                                   : null;

      public bool Equals(CacheKey other) => Type == other.Type &&
                                            CustomSaveOrder == other.CustomSaveOrder &&
                                            SortCollectionsSeparately == other.SortCollectionsSeparately &&
                                            CustomOrderKey == other.CustomOrderKey;

      public override bool Equals(object? obj) => obj is CacheKey other && Equals(other);

      public override int GetHashCode() => HashCode.Combine(Type, CustomSaveOrder, SortCollectionsSeparately, CustomOrderKey);
   }

   private static readonly ConcurrentDictionary<CacheKey, List<PropertySavingMetadata>> Cache = new();

   /// <summary>
   /// Gets the sorted list of properties for the given IAgs object, using a cache to avoid re-sorting.
   /// </summary>
   public static List<PropertySavingMetadata> GetOrCreateSortedProperties(IAgs ags)
   {
      return Cache.GetOrAdd(new(ags.GetType(), ags.AgsSettings), SortSaveableProperties(ags));
   }

   private static List<PropertySavingMetadata> SortSaveableProperties(IAgs ags)
   {
      var settings = ags.AgsSettings;
      if (settings.CustomSaveOrder)
         return SortBySettings(ags.SaveableProps, settings.SaveOrder);

      if (settings.SortCollectionsAndPropertiesSeparately)
      {
         List<PropertySavingMetadata> collections = [];
         List<PropertySavingMetadata> properties = [];

         foreach (var property in ags.SaveableProps)
            if (property.IsCollection)
               collections.Add(property);
            else
               properties.Add(property);

         collections.Sort((a, b) => string.Compare(a.Keyword, b.Keyword, StringComparison.Ordinal));
         properties.Sort((a, b) => string.Compare(a.Keyword, b.Keyword, StringComparison.Ordinal));

         properties.AddRange(collections);
         return properties;
      }

      var sorted = ags.SaveableProps.ToList();
      sorted.Sort((a, b) => string.Compare(a.Keyword, b.Keyword, StringComparison.Ordinal));
      return sorted;
   }

   /// <summary>
   /// Clears the entire property sort order cache. This should be called
   /// whenever global settings that might affect sorting are changed,
   /// such as at the beginning of a new loading cycle.
   /// </summary>
   public static void Clear()
   {
      Cache.Clear();
   }

   private static List<PropertySavingMetadata> SortBySettings(IReadOnlyList<PropertySavingMetadata> propertiesToSave,
                                                              List<Enum> saveOrder)
   {
      var propertyMap = propertiesToSave.ToDictionary(p => p.NxProp);
      var sortedList = new List<PropertySavingMetadata>(saveOrder.Count);

      foreach (var enumValue in saveOrder)
         if (propertyMap.TryGetValue(enumValue, out var prop))
            sortedList.Add(prop);

      return sortedList;
   }
}