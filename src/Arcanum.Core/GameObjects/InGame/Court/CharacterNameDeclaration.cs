using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.GameObjects.InGame.Court;

[ObjectSaveAs(savingMethod: "SaveNameDeclaration")]
#pragma warning disable ARC002
public partial class CharacterNameDeclaration : IEu5Object<CharacterNameDeclaration>
#pragma warning restore ARC002
{
   [SaveAs]
   [DefaultValue("")]
   [Description("The name declaration of the character.")]
   [ParseAs("name")]
   public string Name { get; set; } = string.Empty;

   [SuppressAgs]
   [DefaultValue(false)]
   [Description("Is this name randomly generated?")]
   public bool IsRandom { get; set; }

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.CharacterNameDeclarationNUISettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.CharacterNameDeclarationAgsSettings;

   [Description("Unique key of this SuperRegion. Must be unique among all objects of this type.")]
   [DefaultValue("")]
   public string UniqueId
   {
      get => Name;
      set { }
   }
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string SavingKey { get; set; } = string.Empty;
   public static CharacterNameDeclaration Empty { get; } = new() { Name = "Arcanum_CharacterNameDeclaration_Empty", };

   public string GetNamespace => $"Characters.{nameof(CharacterNameDeclaration)}";
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, Name, string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public static Dictionary<string, CharacterNameDeclaration> GetGlobalItems() => [];
}