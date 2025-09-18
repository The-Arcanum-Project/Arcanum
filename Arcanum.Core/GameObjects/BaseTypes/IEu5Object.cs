using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
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
public interface IEu5Object<out T> : ISearchable, INUI, IAgs, ICollectionProvider<T>, IEmpty<T>
   where T : IEu5Object<T>, new()
{
   [AddModifiable]
   public string UniqueKey { get; set; }

   [SuppressAgs]
   public FileObj Source { get; set; }
}