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
using Common.UI;

namespace Arcanum.Core.GameObjects.Court.State;

[ObjectSaveAs]
#pragma warning disable ARC002
public partial class ParliamentDefinition : IEu5Object<ParliamentDefinition>
#pragma warning restore ARC002
{
   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [Description("The type of this parliament definition.")]
   [ParseAs("parliament_type")]
   public ParliamentType Type { get; set; } = ParliamentType.Empty;

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ParliamentDefinitionSettings;
   public INUINavigation[] Navigations => throw new NotImplementedException();
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ParliamentDefinitionAgsSettings;
   public string UniqueId
   {
      get => Type.UniqueId;
      set { }
   }
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string SavingKey => string.Empty;
   public static ParliamentDefinition Empty { get; } = new() { Type = ParliamentType.Empty };
   public string GetNamespace => "Court.parliament_definition";
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, Type.UniqueId, string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;

   public static Dictionary<string, ParliamentDefinition> GetGlobalItems() => [];
}