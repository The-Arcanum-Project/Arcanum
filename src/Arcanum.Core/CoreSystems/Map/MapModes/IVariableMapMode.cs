using System.Collections;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes;

public interface IVariableSelectionMapMode : IMapMode
{
   IEnumerable AvailableTargets { get; }
   object? SelectedTarget { get; set; }
}

public abstract class IVariableMapMode<TTarget> : LocationBasedMapMode, IVariableSelectionMapMode
   where TTarget : class
{
   /// <summary>
   /// All posisble options the user can choose from. Should be a manageable number of not more than ~30 items! 
   /// </summary>
   public abstract IEnumerable<TTarget> AvailableTargets { get; }

   IEnumerable IVariableSelectionMapMode.AvailableTargets => AvailableTargets;

   // The currently selected option. If null, then the map mode shows only the default color / nothing present.
   public TTarget? SelectedTarget { get; set; }

   object? IVariableSelectionMapMode.SelectedTarget
   {
      get => SelectedTarget;
      set => SelectedTarget = value as TTarget;
   }

   private readonly int _absentColor = JominiColor.Empty.AsInt();
   private readonly int _presentColor = new JominiColor.Rgb(29, 163, 65).AsInt();

   /// <summary>
   /// Return true if the given target is present in the given location, false otherwise. This is used to determine the color of the location on the map.
   /// </summary>
   /// <returns></returns>
   protected abstract bool IsTargetPresent(Location location, TTarget target);

   /// <summary>
   /// Returns the color to render for the given location on the map, based on whether the selected target is present or not. <br/>
   /// If no target is selected, all locations should return the same color (e.g. the "absent" color).
   /// </summary>
   /// <param name="location"></param>
   /// <returns></returns>
   public override int GetColorForLocation(Location location)
   {
      if (SelectedTarget == null)
         return _absentColor;

      return IsTargetPresent(location, SelectedTarget)
                ? _presentColor
                : _absentColor;
   }
}