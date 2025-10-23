using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Arcanum.API.Attributes;
using Arcanum.API.UtilServices.Search.SearchableSetting;
using Arcanum.Core.CoreSystems.Queastor;
using Common.UI;
using Common.Utils.PropertyUtils;

namespace Arcanum.Core.Settings.BaseClasses;

/// <summary>
/// An implementation of <see cref="SearchableSettings"/> that is used for settings that can be searched. <br/><br/>
/// Preferably use this as it brings along automated search functionality
/// and relevance scoring based on the Levenshtein distance to the search terms.<br/><br/>
/// It also defaults the topLevelType to <see cref="MainSettingsObj"/> which is the main settings object of the Arcanum project.
/// </summary>
/// <param name="searchTerms"></param>
public abstract class InternalSearchableSetting(object parent, List<string>? searchTerms = null)
   : SearchableSettings(Config.Settings, typeof(MainSettingsObj), searchTerms), ISettingsNotify
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

   public void SettingChanged(string key, object? oldValue, object? newValue)
   {
      if (oldValue == null && newValue == null || Equals(oldValue, newValue))
         return;

      SettingsEventManager.TriggerSettingsChanged(this,
                                                  new($"{GetNamespace}.{key}",
                                                      key,
                                                      oldValue,
                                                      newValue,
                                                      false));
   }

   /// <summary>
   /// Sets a property, checking for changes and raising the SettingChanged event if necessary.
   /// </summary>
   /// <typeparam name="T">The type of the property.</typeparam>
   /// <param name="field">A reference to the backing field.</param>
   /// <param name="newValue">The new value for the property.</param>
   /// <param name="propertyName">The name of the property (automatically supplied by the compiler).</param>
   /// <returns>True if the value changed, false otherwise.</returns>
   protected bool SetNotifyProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null!)
   {
      if (EqualityComparer<T>.Default.Equals(field, newValue))
         return false;

      var oldValue = field;
      field = newValue;
      SettingChanged(propertyName, oldValue, newValue);
      return true;
   }
}