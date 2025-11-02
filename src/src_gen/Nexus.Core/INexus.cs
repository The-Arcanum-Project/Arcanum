using System.Collections;
using System.ComponentModel;

namespace Nexus.Core;

public interface INexus : INotifyPropertyChanged
{
   /// <summary>
   /// Gets the value of a property by its enum key. <br/>
   /// Only to be used internally by Nexus only! <br/>
   /// Use <see cref="Nx.Get{T}(INexus, Enum)"/> to get values from outside!
   /// </summary>
   /// <param name="property"></param>
   /// <returns></returns>
   object _getValue(Enum property);

   /// <summary>
   /// Sets the value of a property by its enum key. <br/>
   /// Only to be used internally by Nexus only! <br/>
   /// Use <see cref="Nx.Set{T}(INexus, Enum, T)"/> to set values from outside!
   /// </summary>
   /// <param name="property"></param>
   /// <param name="value"></param>
   void _setValue(Enum property, object value);

   #region Collection Manipulation

   /// <summary>
   /// Adds a value to a collection property by its enum key. <br/>
   /// Only to be used internally by Nexus only! <br/>
   /// Use <see cref="Nx.AddToCollection{T}(INexus, Enum, T)"/> to add values from outside!
   /// </summary>
   void _addToCollection(Enum property, object item);

   /// <summary>
   /// Adds a range of values to a collection property by its enum key. <br/>
   /// Only to be used internally by Nexus only!
   /// </summary>
   void _addRangeToCollection(Enum property, IEnumerable items);

   /// <summary>
   /// Removes a value from a collection property by its enum key. <br/>
   /// Only to be used internally by Nexus only! <br/>
   /// Use <see cref="Nx.RemoveFromCollection{T}(INexus, Enum, T)"/> to remove values from outside!
   /// </summary>
   void _removeFromCollection(Enum property, object item);

   /// <summary>
   /// Inserts an item into a collection property at a specific index. <br/>
   /// Only to be used internally by Nexus only!
   /// </summary>
   void _insertIntoCollection(Enum property, int index, object item);

   /// <summary>
   /// Removes an item from a collection property at a specific index. <br/>
   /// Only to be used internally by Nexus only!
   /// </summary>
   void _removeFromCollectionAt(Enum property, int index);

   /// <summary>
   /// Clears all values from a collection property by its enum key. <br/>
   /// Only to be used internally by Nexus only! <br/>
   /// Use <see cref="Nx.ClearCollection(INexus, Enum)"/> to clear collections from outside!
   /// </summary>
   void _clearCollection(Enum property);

   #endregion

   /// <summary>
   /// Returns a description of the given property, if any.
   /// </summary>
   /// <param name="property"></param>
   /// <returns></returns>
   string? GetDescription(Enum property);

   /// <summary>
   /// Returns all properties defined in the Nexus as a list of enum values.
   /// </summary>
   /// <returns></returns>
   List<Enum> GetAllProperties();

   /// <summary>
   /// Returns the type of the given property.
   /// </summary>
   /// <param name="property"></param>
   Type GetNxPropType(Enum property);

   /// <summary>
   /// Returns the item type of the given collection property. <br/>
   /// Returns null if the property is not a collection.
   /// </summary>
   Type? GetNxItemType(Enum property);

   #region Bool Accessors

   /// <summary>
   /// Returns whether the given property is a collection.
   /// </summary>
   bool IsCollection(Enum property);

   /// <summary>
   /// Returns whether the given property is required to have a non-empty value.
   /// </summary>
   /// <param name="property"></param>
   /// <returns></returns>
   bool IsRequired(Enum property);

   /// <summary>
   /// Returns whether the given property is inlined (i.e., shown directly instead of in a sub-view).
   /// </summary>
   bool IsPropertyInlined(Enum property);

   /// <summary>
   /// Returns whether the given property is read-only and cannot be modified.
   /// </summary>
   /// <param name="property"></param>
   /// <returns></returns>
   bool IsPropertyReadOnly(Enum property);

   /// <summary>
   /// Returns whether the given property allows an empty value (e.g., null or default) in it's embedded view.
   /// </summary>
   bool AllowsEmptyValue(Enum property);

   /// <summary>
   /// Returns whether the "Map Infer" buttons for the given property are disabled.
   /// </summary>
   bool IsMapInferButtonsDisabled(Enum property);

   #endregion

   /// <summary>
   /// Gets the default value for the given property.
   /// </summary>
   /// <param name="property"></param>
   /// <typeparam name="T"></typeparam>
   /// <returns></returns>
   T GetDefaultValue<T>(Enum property);

   /// <summary>
   /// Gets the default value for the given property as an object. <br/>
   /// Use <see cref="GetDefaultValue{T}(Enum)"/> to get the typed version.
   /// </summary>
   /// <param name="property"></param>
   /// <returns></returns>
   object GetDefaultValue(Enum property);
}