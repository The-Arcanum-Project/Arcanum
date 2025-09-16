namespace Nexus.Core;

public interface INexus
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

   /// <summary>
   /// Adds a value to a collection property by its enum key. <br/>
   /// Only to be used internally by Nexus only! <br/>
   /// Use <see cref="Nx.AddToCollection{T}(INexus, Enum, T)"/> to add values from outside!
   /// </summary>
   /// <param name="property"></param>
   /// <param name="item"></param>
   void _addToCollection(Enum property, object item);

   /// <summary>
   /// Removes a value from a collection property by its enum key. <br/>
   /// Only to be used internally by Nexus only! <br/>
   /// Use <see cref="Nx.RemoveFromCollection{T}(INexus, Enum, T)"/> to remove values from outside!
   /// </summary>
   /// <param name="property"></param>
   /// <param name="item"></param>
   void _removeFromCollection(Enum property, object item);

   /// <summary>
   /// Clears all values from a collection property by its enum key. <br/>
   /// Only to be used internally by Nexus only! <br/>
   /// Use <see cref="Nx.ClearCollection(INexus, Enum)"/> to clear collections from outside!
   /// </summary>
   /// <param name="property"></param>
   void _clearCollection(Enum property);

   /// <summary>
   /// Accessor to get or set a property by its enum key.
   /// </summary>
   /// <param name="key"></param>
   object this[Enum key] { get; set; }

   /// <summary>
   /// Returns whether the given property is read-only and cannot be modified.
   /// </summary>
   /// <param name="property"></param>
   /// <returns></returns>
   bool IsPropertyReadOnly(Enum property);

   /// <summary>
   /// Returns whether the given property allows an empty value (e.g., null or default) in it's embedded view.
   /// </summary>
   /// <param name="property"></param>
   /// <returns></returns>
   bool AllowsEmptyValue(Enum property);

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
}