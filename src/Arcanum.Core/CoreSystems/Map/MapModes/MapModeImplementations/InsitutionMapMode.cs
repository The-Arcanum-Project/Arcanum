#region

using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects;

#endregion

namespace Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;

public sealed class InsitutionMapMode : IVariableMapMode<Institution>
{
   public override string Name => "Institutions";
   public override string Description => "Displays the whether the selected institution is present in the location.";
   public override MapModeManager.MapModeType Type => MapModeManager.MapModeType.Institutions;
   public override Type[] DisplayTypes { get; } = [typeof(Institution), typeof(InstitutionPresence)];

   public override string[] GetTooltip(Location location)
   {
      var tooltip = new string[location.InstitutionPresences.Count(x => x.IsPresent) + 1];
      tooltip[0] = $"Present Institutions in '{location.UniqueId}': ";

      var index = 1;
      foreach (var institutionPresence in location.InstitutionPresences)
         if (institutionPresence.IsPresent)
            tooltip[index++] = $"- {institutionPresence.Institution.UniqueId}";

      return tooltip;
   }

   public override string? GetLocationText(Location location) => null;

   public override object?[]? GetVisualObject(Location location) => null;

   public override void OnActivateMode()
   {
      SelectedTarget ??= AvailableTargets.FirstOrDefault();
   }

   public override void OnDeactivateMode()
   {
   }

   public override object GetLocationRelatedData(Location location)
      => location.InstitutionPresences.Where(x => x.IsPresent).Select(x => x.Institution.UniqueId);

   public override IEnumerable<Institution> AvailableTargets => Globals.Institutions.Values;

   protected override bool IsTargetPresent(Location location, Institution target)
   {
      for (var i = 0; i < location.InstitutionPresences.Count; i++)
      {
         var institutionPresence = location.InstitutionPresences[i];
         if (institutionPresence.IsPresent && institutionPresence.Institution == target)
            return true;
      }

      return false;
   }
}