using Vortice.Mathematics;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.CoreSystems.Map.MapModes;

public abstract class LocationBasedMapMode : IMapMode
{
   public virtual bool DarkenWastelands => true;
   public abstract string Name { get; }
   public abstract string Description { get; }
   public abstract MapModeManager.MapModeType Type { get; }
   public abstract Type[] DisplayTypes { get; }
   public virtual bool IsLandOnly => true;

   public void Render(Color4[] buffer)
   {
      var array = MapModeManager.LocationsArray;

      if (IsLandOnly)
      {
         var waterProvinces = new HashSet<Location>(Globals.DefaultMapDefinition.SeaZones);
         waterProvinces.UnionWith(Globals.DefaultMapDefinition.Lakes);
         var useLocWater = Config.Settings.MapSettings.UseShadeOfColorOnWater;
         Parallel.For(0, array.Length, ProcessLocation);
         waterProvinces.Clear();

         void ProcessLocation(int i)
         {
            var location = array[i];
            if (waterProvinces.Contains(location))
            {
               if (useLocWater)
                  buffer[i] = MapModeManager.GetWaterColorForLocation(location);
               else
                  buffer[i] = new(location.Color.AsInt());
            }
            else
               buffer[i] = new(GetColorForLocation(location));
         }
      }
      else
         Parallel.For(0,
                      array.Length,
                      i => { buffer[i] = new(GetColorForLocation(array[i])); });

      if (DarkenWastelands)
         MapModeManager.DarkenWastelandColors(buffer);

      PostRender(buffer);
   }

   public abstract string[] GetTooltip(Location location);
   public abstract string? GetLocationText(Location location);
   public abstract object?[]? GetVisualObject(Location location);
   public abstract void OnActivateMode();
   public abstract void OnDeactivateMode();
   public abstract object GetLocationRelatedData(Location location);
   public virtual MapContexMenuConfig[]? GetContextMenuOptions() => null;

   protected internal virtual void PostRender(Color4[] colorBuffer)
   {
   }

   public abstract int GetColorForLocation(Location location);
}