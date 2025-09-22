using System.Collections;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
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
}

public interface IEu5Object : ISearchable, INUI, IAgs
{
   [SuppressAgs]
   [AddModifiable]
   public string UniqueId { get; set; }

   [SuppressAgs]
   public Eu5FileObj Source { get; set; }

   string ISearchable.ResultName => UniqueId;
   List<string> ISearchable.SearchTerms => [UniqueId];

   string IAgs.SavingKey => UniqueId;

   /// <summary>
   /// Provides the default implementation for the non-generic gateway method from IEu5Object.
   /// It calls the static abstract method guaranteed by the <see cref="IEu5ObjectProvider{T}"/> contract.
   /// </summary>
   public IDictionary GetGlobalItemsNonGeneric();
}