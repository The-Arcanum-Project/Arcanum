using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

public interface ILocationCollection<T> : ILocationCollection where T : ILocation
{
   [SuppressAgs]
   [ParseAs("THIS_SHOULD_NEVER_BE_USED")]
   [AddModifiable]
   [Description("The child locations of this location.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<T> LocationChildren { get; set; }

   List<ILocation> ILocationCollection.LocationChildrenGeneral => LocationChildren.Cast<ILocation>().ToList();
}

public interface ILocationCollection : ILocation
{
   public List<ILocation> LocationChildrenGeneral { get; }
}

public interface ILocation : IEu5Object
{
   public List<Location> GetLocations();

   public LocationCollectionType LcType { get; }

   [SuppressAgs]
   [AddModifiable]
   [Description("The parent locations of this location.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<ILocation> Parents { get; set; }
}