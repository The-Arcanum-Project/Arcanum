using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.BaseTypes;

public abstract class OldNameKeyDefined(string name) : ISearchable
{
   [ReadonlyNexus]
   [AddModifiable]
   public string Name { get; set; } = name;

   public override string ToString() => Name;

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();

   public override bool Equals(object? obj)
   {
      if (obj is not OldNameKeyDefined other)
         return false;

      return string.Equals(Name, other.Name, StringComparison.Ordinal);
   }

   public abstract string GetNamespace { get; }
   public string ResultName => Name;
   public List<string> SearchTerms { get; set; } = [name];

   public virtual void OnSearchSelected()
   {
   }

   public ISearchResult VisualRepresentation { get; } = new SearchResultItem(null, name, string.Empty);
   public virtual IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.GameObjects;
}