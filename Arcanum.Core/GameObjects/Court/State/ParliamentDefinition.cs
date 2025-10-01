using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;

namespace Arcanum.Core.GameObjects.Court.State;

[ObjectSaveAs]
#pragma warning disable ARC002
public partial class ParliamentDefinition : IEu5Object<ParliamentDefinition>
#pragma warning restore ARC002
{
   [SaveAs]
   [DefaultValue("")]
   [Description("The type of this parliament definition.")]
   [ParseAs("parliament_type")]
   public string Type { get; set; } = string.Empty;

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ParliamentDefinitionSettings;
   public INUINavigation[] Navigations => throw new NotImplementedException();
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ParliamentDefinitionAgsSettings;
   public string UniqueId
   {
      get => Type;
      set { }
   }
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public string SavingKey => string.Empty;
   public static ParliamentDefinition Empty { get; } = new() { Type = "Arcanum_empty_parliament_definition" };
   public string GetNamespace => "Court.parliament_definition";

   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, Type, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.GameObjects;

   public static Dictionary<string, ParliamentDefinition> GetGlobalItems() => [];
}