using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

public abstract class LocationCollection<T>(string name) : LocationComposite(name)
   where T : LocationComposite
// Province, Area, Region, SuperRegion, Continent
{
   public LocationCollection(string name, ICollection<T> provinces) : this(name)
   {
      SubCollection = provinces;
   }

   private readonly ICollection<T> _subCollection = [];
   internal ICollection<T> SubCollection
   {
      get => _subCollection;
      init => AddRange(value);
   }

   /// <summary>
   /// Recursively gets all provinces in the SubCollections
   /// </summary>
   /// <returns></returns>
   public override ICollection<Location> GetLocations()
   {
      var provinces = new List<Location>();
      foreach (var subCollection in SubCollection)
         provinces.AddRange(subCollection.GetLocations());
      return provinces;
   }

   public void Add(T composite)
   {
      foreach (var parent in composite.Parents)
         if (parent.LCType == LCType)
         {
            ((LocationCollection<T>)parent).Remove(composite);
            break;
         }

      composite.Parents.Add(this);
      _subCollection.Add(composite);
   }

   public void Remove(T composite)
   {
      composite.Parents.Remove(this);
      _subCollection.Remove(composite);
   }

   public void RemoveRange(ICollection<T> composites)
   {
      foreach (var composite in composites)
         Remove(composite);
   }

   public void AddRange(ICollection<T> composites)
   {
      foreach (var composite in composites)
         Add(composite);
   }

   public virtual ICommand GetAddCommand(LocationCollection<T> collection,
                                         bool addToGlobal,
                                         List<LocationComposite> toAdd)
   {
      // return new CAddProvinceCollection<T>(this, addToGlobal, toAdd);
      throw new NotImplementedException("GetAddCommand must be implemented in derived classes.");
   }

   public virtual ICommand GetRemoveCommand(LocationCollection<T> collection,
                                            bool removeFromGlobal,
                                            List<LocationComposite> toRemove)
   {
      // return new CRemoveProvinceCollection<T>(this, removeFromGlobal, toRemove);
      throw new NotImplementedException("GetRemoveCommand must be implemented in derived classes.");
   }

   public void CreateNew(T composite, bool addToGlobal = false, bool tryAddEventToHistory = true)
   {
      if (_subCollection.Contains(composite))
         return;

      var command = GetAddCommand(this, addToGlobal, [composite]);
      ExecuteAndAdd(command, tryAddEventToHistory);
   }

   public void CreateNewRange(ICollection<T> composites, bool addToGlobal = false, bool tryAddEventToHistory = true)
   {
      List<LocationComposite> toAdd = [];
      var count = composites.Count;
      foreach (var composite in composites)
      {
         if (_subCollection.Contains(composite))
            count--;
         else
            toAdd.Add(composite);
      }

      var command = GetAddCommand(this, addToGlobal, toAdd);
      if (count != 0)
         ExecuteAndAdd(command, tryAddEventToHistory);
   }

   public void Delete(T composite, bool removeFromGlobal = false, bool tryAddEventToHistory = true)
   {
      if (!_subCollection.Contains(composite))
         return;

      var command = GetRemoveCommand(this, removeFromGlobal, [composite]);
      ExecuteAndAdd(command, tryAddEventToHistory);
   }

   public void DeleteRange(ICollection<T> composites,
                           bool removeFromGlobal = false,
                           bool tryAddEventToHistory = true)
   {
      List<LocationComposite> toRemove = [];
      foreach (var composite in composites)
      {
         if (!_subCollection.Contains(composite))
            continue;

         toRemove.Add(composite);
      }

      var command = GetRemoveCommand(this, removeFromGlobal, toRemove);
      ExecuteAndAdd(command, tryAddEventToHistory);
   }

   /// <summary>
   /// Executes the command and adds it to the history if the state is allowed.
   /// </summary>
   /// <param name="command"></param>
   /// <param name="tryAddEventToHistory"></param>
   public void ExecuteAndAdd(ICommand command, bool tryAddEventToHistory)
   {
      command.Execute();
      if (tryAddEventToHistory && Globals.AppState == AppState.EditingAllowed)
         Globals.HistoryManager.AddCommand(command);
   }

   public virtual void InternalAdd(T composite) => _subCollection.Add(composite);
   public virtual void InternalRemove(T composite) => _subCollection.Remove(composite);
   public abstract void RemoveGlobal();
   public abstract void AddGlobal();
}