using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Common.UI;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

public abstract class LocationComposite : ISaveable, ISearchable // TODO: @Melco @Minnator implement ISaveable here
{
   protected LocationComposite(string name, FileInformation information)
   {
      Name = name.Trim();
      SearchTerms = [Name];
      FileInformation = information;

      Queastor.GlobalInstance.AddToIndex(this);
   }

   [AddModifiable]
   public string Name { get; set; }
   [AddModifiable]
   public ObservableRangeCollection<LocationComposite> Parents { get; set; } = [];
   public abstract ICollection<Location> GetLocations();
   public abstract LocationCollectionType LCType { get; }

   public virtual LocationComposite GetFirstParentOfType(LocationCollectionType type)
   {
      foreach (var parent in Parents)
      {
         if (parent.LCType == type)
            return parent;

         var recursiveParent = parent.GetFirstParentOfType(type);
         if (recursiveParent != Empty)
            return recursiveParent;
      }

      return Empty;
   }

   public override bool Equals(object? obj)
   {
      if (obj is LocationComposite other)
         return Name == other.Name;

      return false;
   }

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();
   public override string ToString() => Name;

   public static bool operator ==(LocationComposite? left, LocationComposite? right)
   {
      if (left is null)
         return right is null;

      return left.Equals(right);
   }

   public static bool operator !=(LocationComposite? left, LocationComposite? right) => !(left == right);

   public static LocationComposite Empty { get; } = Location.Empty;

   public FileInformation FileInformation { get; }
   public SaveableType SaveType { get; } = SaveableType.Location;

   #region ISearchable Implementation

   public string GetNamespace => BuildNamespace();
   public string ResultName
      => $"{Name} - ({string.Join(',', Parents)})"; // TODO: @Minnator replace this with the localisation of the objects once localisation is implemented
   public List<string> SearchTerms { get; set; }
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, Name, GetNamespace);
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.MapObjects;

   protected string BuildNamespace()
   {
      foreach (var parent in Parents)
         if (parent.LCType == LCType + 1)
            return $"{parent.GetNamespace}{((ISearchable)this).NamespaceSeparator}{LCType}";

      return $"{LCType}";
   }

   public float GetRelevanceScore(string query) => Queastor.GlobalInstance.MinLevinsteinDistanceToTerms(this, query);

   #endregion
}