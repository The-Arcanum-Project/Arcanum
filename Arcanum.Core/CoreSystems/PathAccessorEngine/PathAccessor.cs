using System.Collections;
using System.Diagnostics;
using Arcanum.Core.CoreSystems.CommandSystem;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.PathAccessorEngine;

public class PathAccessor
{
   public static readonly Dictionary<string, Type> MasterSuggestionList =
      Eu5ObjectsRegistry.Eu5Objects.ToDictionary(o => o.Name, o => o);

   public string RawPath { get; set; }
   public string[] Segments => RawPath.Split('.',
                                             StringSplitOptions.TrimEntries |
                                             StringSplitOptions.RemoveEmptyEntries);

   public PathAccessor(string path)
   {
      RawPath = path;
   }

   /// <summary>
   /// If the object is not null, the empty object of the same type is used and the example value is calculated.
   /// </summary>
   /// <param name="obj"></param>
   public string CalculateExampleValue(IEu5Object? obj)
   {
      const string typeNullIdNull = "Type: null -:- Id: null";
      var parts = Segments;

      if (obj == null && (parts.Length <= 0 || !RawPath.Contains('.')))
         return typeNullIdNull;

      if (MasterSuggestionList.TryGetValue(parts[0], out var type) &&
          EmptyRegistry.Empties.TryGetValue(type, out var emptyObj))
         obj = (IEu5Object)emptyObj;
      else
         return "Invalid Accessor Path";

      var currentObj = obj!;
      // Traverse the path to get the final property type
      for (var i = 0; i < parts.Length; i++)
      {
         if (i == 0)
         {
            if (i == parts.Length - 1)
               return $"Type: {currentObj.GetType().Name} -:- Id: {currentObj.UniqueId}";

            continue;
         }

         var enums = currentObj.GetAllProperties();
         var nxProp =
            enums.FirstOrDefault(p => string.Equals(p.ToString(), parts[i], StringComparison.OrdinalIgnoreCase));
         if (nxProp == null)
            return "Invalid Accessor Path";

         var propType = currentObj.GetNxPropType(nxProp);

         if (typeof(IEu5Object).IsAssignableFrom(propType))
         {
            Nx.ForceGet(currentObj, nxProp, ref currentObj);
         }
         else if (currentObj.IsCollection(nxProp))
         {
            object collection = null!;
            Nx.ForceGet(currentObj, nxProp, ref collection);
            var itemType = currentObj.GetNxItemType(nxProp);
            if (typeof(IEu5Object).IsAssignableFrom(itemType))
            {
               currentObj = (IEu5Object)EmptyRegistry.Empties[itemType];
               continue;
            }

            if (collection is not ICollection coll)
               continue;

            return $"Collection of {itemType!.Name} with {coll.Count} items";
         }
         else
         {
            object? value = null;
            Nx.ForceGet(currentObj, nxProp, ref value);
            return value?.ToString() ?? "null";
         }
      }

      if (currentObj == null)
         return typeNullIdNull;

      return $"Type: {currentObj.GetType().Name} -:- Id: {currentObj.UniqueId}";
   }

   public object GetValueForSorting(IEu5Object target)
   {
      const string typeNullIdNull = "null";
      var parts = Segments;

      if (parts.Length <= 0)
         return typeNullIdNull;

      if (parts.Length == 1)
         return target.UniqueId;

      var currentObj = target;
      // Traverse the path to get the final property type
      for (var i = 1; i < parts.Length; i++)
      {
         var enums = currentObj.GetAllProperties();
         var nxProp =
            enums.FirstOrDefault(p => string.Equals(p.ToString(), parts[i], StringComparison.OrdinalIgnoreCase));
         if (nxProp == null)
            return "null";

         var propType = currentObj.GetNxPropType(nxProp);

         if (typeof(IEu5Object).IsAssignableFrom(propType))
         {
            Nx.ForceGet(currentObj, nxProp, ref currentObj);
         }
         else if (currentObj.IsCollection(nxProp))
         {
            // Collections are not supported for sortingValueRetrieval
            return "null";
         }
         else
         {
            object value = null!;
            Nx.ForceGet(currentObj, nxProp, ref value);
            Debug.Assert(value != null, nameof(value) + " != null");
            return value;
         }
      }

      return $"Type: {currentObj.GetType().Name} -:- Id: {currentObj.UniqueId}";
   }

   public static List<Enum> GetPathOptions(PathAccessor accessor, bool suggestionSource)
   {
      return GetPathOptions(accessor.RawPath, suggestionSource);
   }

   public static List<Enum> GetPathOptions(string path, bool includeCollections)
   {
      if (string.IsNullOrWhiteSpace(path))
         return [];

      var parts = path.Split('.');

      if (!MasterSuggestionList.TryGetValue(parts[0], out var currentType) ||
          !EmptyRegistry.Empties.TryGetValue(currentType, out var emptyInstance))
         return [];

      var currentInstance = (IEu5Object)emptyInstance;
      for (var i = 1; i < parts.Length; i++)
      {
         var propertyName = parts[i];

         var properties = currentInstance.GetAllProperties();
         var propInfo = properties.FirstOrDefault(p => p.ToString() == propertyName);

         if (propInfo == null)
            return [];

         var propertyType = Nx.TypeOf(currentInstance, propInfo);

         if (EmptyRegistry.Empties.TryGetValue(propertyType, out var empty))
         {
            // Check if we have an observable collection or HashSet
            if (!currentInstance.IsCollection(propInfo))
            {
               currentInstance = (IEu5Object)empty!;
               if (i == parts.Length - 1)
               {
                  var props = currentInstance.GetAllProperties()
                                             .OrderBy(name => name)
                                             .ToList();

                  if (!includeCollections)
                     props = props.Where(p => !currentInstance.IsCollection(p)).ToList();
                  return props;
               }

               continue;
            }

            var itemType = currentInstance.GetNxItemType(propInfo);
            if (EmptyRegistry.Empties.TryGetValue(itemType!, out empty))
            {
               currentInstance = (IEu5Object)empty!;
               continue;
            }

            return [];
         }
      }

      object value = null!;
      Nx.ForceGet(currentInstance, currentInstance.GetAllProperties().First(), ref value);
      var prop = currentInstance.GetAllProperties()
                                .OrderBy(name => name)
                                .ToList();

      if (!includeCollections)
         prop = prop.Where(p => !currentInstance.IsCollection(p)).ToList();
      return prop;
   }

   public static List<string> GetSuggestions(string text,
                                             out bool isOpen,
                                             bool forceShowAll = false,
                                             bool includeCollections = true)
   {
      List<string> suggestions = [];
      if (!forceShowAll && string.IsNullOrWhiteSpace(text))
      {
         isOpen = false;
         return suggestions;
      }

      List<string> suggestionSource;
      string filterText;

      var lastDotIndex = text.LastIndexOf('.');

      if (lastDotIndex == -1)
      {
         suggestionSource = MasterSuggestionList.Keys.ToList();
         filterText = text;
      }
      else
      {
         var path = text[..lastDotIndex];
         filterText = text[(lastDotIndex + 1)..];

         suggestionSource = GetPathOptions(path, includeCollections).Select(e => e.ToString()).ToList();
      }

      var filteredSuggestions = suggestionSource
                               .Where(s => s.StartsWith(filterText, StringComparison.OrdinalIgnoreCase))
                               .ToList();

      foreach (var suggestion in filteredSuggestions)
         if (suggestion != filterText)
            suggestions.Add(suggestion);

      isOpen = suggestions.Count != 0 || (forceShowAll && suggestionSource.Count != 0);

      if (!isOpen || !forceShowAll || suggestions.Count != 0)
         return suggestions;

      foreach (var suggestion in suggestionSource)
         suggestions.Add(suggestion);

      return suggestions;
   }
}