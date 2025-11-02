using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.LocationCollections;
using Common.UI;

namespace Arcanum.Core.GameObjects.Court;

[ObjectSaveAs]
public partial class Dynasty : IEu5Object<Dynasty>
{
   public enum DynastyNameType
   {
      [EnumAgsData("default")]
      Default,

      [EnumAgsData("location")]
      Location,

      [EnumAgsData("descendant")]
      Decendant,

      [EnumAgsData("patronym")]
      Patronym,

      [EnumAgsData("location_ancient")]
      LocationAncient,
   }

   #region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("name", AstNodeType.StatementNode)]
   [Description("The naming conventions for members of this dynasty.")]
   public CharacterNameDeclaration Name { get; set; } = CharacterNameDeclaration.Empty;

   [SaveAs]
   [DefaultValue(DynastyNameType.Default)]
   [ParseAs("dynasty_name_type")]
   [Description("The type of this dynasty.")]
   public DynastyNameType NameType { get; set; } = DynastyNameType.Default;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("home")]
   [Description("The home location of this dynasty.")]
   public Location Home { get; set; } = Location.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("male_names")]
   [Description("The list of possible male names for members of this dynasty.")]
   public ObservableRangeCollection<string> MaleNames { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("female_names")]
   [Description("The list of possible female names for members of this dynasty.")]
   public ObservableRangeCollection<string> FemaleNames { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this Dynasty. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Court.{nameof(Dynasty)}";
   public void OnSearchSelected() => UIHandle.Instance.MainWindowsHandle.SetToNui(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.DynastySettings;
   public INUINavigation[] Navigations => [];
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.DynastyAgsSettings;
   public static Dictionary<string, Dynasty> GetGlobalItems() => Globals.Dynasties;

   public static Dynasty Empty { get; } = new() { UniqueId = "Arcanum_Empty_Dynasty" };

   public override string ToString() => UniqueId;

   #endregion
}