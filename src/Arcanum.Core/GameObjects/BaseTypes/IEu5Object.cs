using System.Collections;
using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.Registry;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.BaseTypes;

/// <summary>
/// Every object implemented in Arcanum should implement this interface. <br/>
/// Can be inserted via <c>eu5BaseObject</c> live template. <br/>
/// <see cref="IEu5Object{T}"/> extends multiple interfaces: <br/>
/// <see cref="ISearchable"/> - to be searchable in the Queastor search system <br/>
/// <see cref="INUI"/> - to be displayable in the NUI system <br/>
/// <see cref="IAgs"/> - to be saveable and loadable using AGS <br/>
/// <see cref="ICollectionProvider{T}"/> - to provide a collection of its type <br/>
/// <see cref="IEmpty{T}"/> - to provide an empty instance of itself <br/>
/// This ensures that every object has a unique key, can be searched for, can be displayed
/// in the NUI, can be saved/loaded using AGS, can provide a collection of its type,
/// and has an empty instance.
/// 
/// </summary>
/// <typeparam name="T">The type of the object itself</typeparam>
public interface IEu5Object<T> : IEu5Object, IEu5ObjectProvider<T>, IEmpty<T>
   where T : IEu5Object<T>, new()
{
   /// <summary>
   /// Provides the default implementation for the non-generic gateway method from IEu5Object.
   /// It calls the static abstract method guaranteed by the <see cref="IEu5ObjectProvider{T}"/> contract.
   /// </summary>
   IDictionary IEu5Object.GetGlobalItemsNonGeneric() => T.GetGlobalItems();

   static T CreateInstance(string uniqueId, Eu5FileObj source)
   {
      var instance = new T
      {
         UniqueId = uniqueId, Source = source,
      };
      source.ObjectsInFile.Add(instance);
      return instance;
   }
}

public interface IEu5Object : ISearchable, INUI, IAgs
{
   [AddModifiable, SuppressAgs]
   [PropertyConfig(isReadonly: true, isRequired: true)]
   [Description("Unique key of this SuperRegion. Must be unique among all objects of this type.")]
   [DefaultValue("")]
   public string UniqueId { get; set; }

   [SuppressAgs]
   public Eu5FileObj Source { get; set; }
   public Eu5ObjectLocation FileLocation { get; set; }

   string ISearchable.ResultName => UniqueId;
   List<string> ISearchable.SearchTerms => [UniqueId];
   string IAgs.SavingKey => UniqueId;
   /// <summary>
   /// If true this object can not be saved individually and has to have a reference to it's parent
   /// </summary>
   public bool IsEmbeddedObject => false;

   /// <summary>
   /// Is True if this object is initialized as a <code>REPLACE/TRY_REPLACE/REPLACE_OR_CREATE:UniqueID</code> object.
   /// </summary>
   public InjRepType InjRepType { get; set; }
   /// <summary>
   /// Only exists if this object is an embedded object.
   /// </summary>
   public IEu5Object? Parent => null;

   /// <summary>
   /// Provides the default implementation for the non-generic gateway method from IEu5Object.
   /// It calls the static abstract method guaranteed by the <see cref="IEu5ObjectProvider{T}"/> contract.
   /// </summary>
   public IDictionary GetGlobalItemsNonGeneric();

   /// <summary>
   /// Returns an array of all properties that have non-default values.
   /// </summary>
   /// <returns></returns>
   public KeyValuePair<Enum, object>[] GetNonDefaultProperties()
   {
      // Is not as bad as it looks as GetAllProperties() is cached.
      var nonDefaultProps = new KeyValuePair<Enum, object>[GetAllProperties().Length];

      var index = 0;
      foreach (var prop in GetAllProperties())
      {
         var currentValue = _getValue(prop);
         var defaultValue = GetDefaultValue(prop);

         if (IsCollection(prop) && InjectManager.AreCollectionsLogicallyEqual(currentValue, defaultValue))
            continue;

         if (currentValue.Equals(defaultValue))
            continue;

         // Skip UniqueId property
         if (prop.ToString() == "UniqueId")
            continue;

         nonDefaultProps[index++] = new(prop, currentValue);
      }

      return nonDefaultProps[..index];
   }

   public static bool IsEmpty(IEu5Object obj)
   {
      var empty = (IEu5Object)EmptyRegistry.Empties[obj.GetType()];
      return ReferenceEquals(obj, empty);
   }

   public void ResetToDefault()
   {
      foreach (var prop in GetAllProperties())
         _setValue(prop, GetDefaultValue(prop));
   }
}