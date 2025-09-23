using System.Text.Json.Serialization;
using Arcanum.API.Attributes;
using Arcanum.API.UtilServices.Search.SearchableSetting;
using Arcanum.Core.CoreSystems.Queastor;
using Common.UI;
using Common.Utils.PropertyUtils;

namespace Arcanum.Core.Settings;

/// <summary>
/// An implementation of <see cref="SearchableSettings"/> that is used for settings that can be searched. <br/><br/>
/// Preferably use this as it brings along automated search functionality
/// and relevance scoring based on the Levenshtein distance to the search terms.<br/><br/>
/// It also defaults the topLevelType to <see cref="MainSettingsObj"/> which is the main settings object of the Arcanum project.
/// </summary>
/// <param name="searchTerms"></param>
public abstract class InternalSearchableSetting(object parent, List<string>? searchTerms = null)
   : SearchableSettings(Config.Settings, typeof(MainSettingsObj), searchTerms)
{
   public override float GetRelevanceScore(string query)
      => Queastor.GlobalInstance.MinLevinsteinDistanceToTerms(this, query);

   [IgnoreInPropertyGrid]
   [JsonIgnore]
   public override string ResultName => GetType().Name;

   public override void OnSearchSelected()
   {
      var info = parent.GetType().GetProperty(GetType().Name);
      if (info == null)
         throw new
            InvalidOperationException($"Property '{GetType().Name}' not found in type '{parent.GetType().Name}'.");

      var path = PropertyPathBuilder.GetPathToProperty(parent, info);
      UIHandle.Instance.PopUpHandle.NavigateToSetting(path);
   }
}