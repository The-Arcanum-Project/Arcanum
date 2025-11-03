using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;

namespace Arcanum.Core.GameObjects.Court.State;

[ObjectSaveAs]
public partial class RulerTerm : IEu5Object<RulerTerm>
{
   [SaveAs]
   [DefaultValue("")]
   [Description("The ID of the character serving this ruler term.")]
   [ParseAs("character")]
   public string CharacterId { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue("")]
   [Description("The regnal name used by the ruler during this term.")]
   [ParseAs("regnal_name")]
   public string RegnalName { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [Description("The starting year of this ruler term.")]
   [ParseAs("start_date")]
   public JominiDate StartDate { get; set; } = JominiDate.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [Description("The ending year of this ruler term.")]
   [ParseAs("end_date")]
   public JominiDate EndDate { get; set; } = JominiDate.Empty;

   [SaveAs]
   [DefaultValue(1)]
   [Description("The regnal number of the ruler during this term.")]
   [ParseAs("regnal_number")]
   public int RegnalNumber { get; set; }

   #region IEu5Object Implementation

   public string GetNamespace => $"Court.{nameof(RulerTerm)}";
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.RulerTermSettings;
   public INUINavigation[] Navigations { get; } = [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.RulerTermAgsSettings;

   [Description("Unique key of this SuperRegion. Must be unique among all objects of this type.")]
   [DefaultValue("")]
   public string UniqueId
   {
      get => CharacterId;
      set => CharacterId = value;
   }
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static Dictionary<string, RulerTerm> GetGlobalItems() => [];

   public static RulerTerm Empty { get; } = new() { UniqueId = "Arcanum_Empty_RulerTerm" };

   #endregion

   #region Equality Members

   protected bool Equals(RulerTerm other) => CharacterId == other.CharacterId &&
                                             StartDate.Equals(other.StartDate) &&
                                             EndDate.Equals(other.EndDate) &&
                                             RegnalNumber == other.RegnalNumber;

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((RulerTerm)obj);
   }

   [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
   public override int GetHashCode() => HashCode.Combine(CharacterId, StartDate, EndDate, RegnalNumber);

   public override string ToString() => $"{CharacterId} ({StartDate} - {EndDate})";

   #endregion
}