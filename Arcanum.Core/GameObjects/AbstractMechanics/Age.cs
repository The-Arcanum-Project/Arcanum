using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;
using ModValInstance = Arcanum.Core.CoreSystems.Jomini.Modifiers.ModValInstance;

namespace Arcanum.Core.GameObjects.AbstractMechanics;

[ObjectSaveAs]
public partial class Age : IEu5Object<Age>
{
#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key for ages. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;
#pragma warning restore AGS004

   # region Nexus Properties

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("year")]
   [Description("What year the age starts in.")]
   public int Year { get; set; }

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("max_price")]
   [Description("The maximum price for goods during this age.")]
   public int MaxPrice { get; set; }

   [SaveAs]
   [DefaultValue(1f)]
   [ParseAs("price_stability")]
   [Description("A modifier to price stability during this age.")]
   public float PriceStability { get; set; }

   [SaveAs]
   [DefaultValue(1f)]
   [ParseAs("mercenaries")]
   [Description("The factor for the size of mercenary armies during this age.")]
   public float Mercenaries { get; set; } = 1f;

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("hegemons_allowed")]
   [Description("Whether hegemons are allowed to be formed during this age.")]
   public bool AllowHegemons { get; set; }

   [SaveAs]
   [DefaultValue(1f)]
   [ParseAs("efficiency")]
   [Description("Some economic efficiency modifier during this age.")]
   public float Efficiency { get; set; } = 1f;

   [SaveAs]
   [DefaultValue(1f)]
   [ParseAs("war_score_from_battles")]
   [Description("The factor for war score gained from battles during this age.")]
   public float WarScoreFromBattles { get; set; } = 1f;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("modifier", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied during this age.")]
   public ObservableRangeCollection<ModValInstance> Modifiers { get; set; } = [];

   # endregion

   #region Interface Properties

   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.AgeSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Age Empty { get; } = new() { UniqueId = "Arcanum_Empty_Age" };
   public static Dictionary<string, Age> GetGlobalItems() => Globals.Ages;

   #endregion

   #region ISearchable

   public string GetNamespace => $"AbstractMechanics.{nameof(Age)}";
   public string ResultName => UniqueId;
   public List<string> SearchTerms => [UniqueId];

   public void OnSearchSelected()
   {
      UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   }

   public ISearchResult VisualRepresentation { get; } = new SearchResultItem(null, "Age", string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;

   #endregion

   public Eu5FileObj Source { get; set; } = null!;

   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.AgeAgsSettings;
   public string SavingKey => UniqueId;

   public override string ToString() => UniqueId;

   protected bool Equals(Age other) => UniqueId == other.UniqueId;

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((Age)obj);
   }

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => UniqueId.GetHashCode();
}