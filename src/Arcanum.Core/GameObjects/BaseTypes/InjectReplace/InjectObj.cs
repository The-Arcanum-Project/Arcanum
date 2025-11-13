using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.CommandSystem;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Common.UI;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

[ObjectSaveAs]
public partial class InjectObj : IEu5Object<InjectObj>
{
   [IgnoreModifiable]
   [SuppressAgs]
   public IEu5Object Target { get; init; } = null!;

   [IgnoreModifiable]
   [SuppressAgs]
   public KeyValuePair<Enum, object>[] InjectedProperties { get; init; } = [];

#pragma warning disable AGS004
   [Description("Unique key of this InjectObj. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Injection.{nameof(InjectObj)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Target.NUISettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Target.AgsSettings;
   public static Dictionary<string, InjectObj> GetGlobalItems() => [];
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static InjectObj Empty { get; } = new() { UniqueId = "Arcanum_Empty_InjectObj" };

   public override string ToString() => $"{InjRepType}: {Target.UniqueId}";

   #endregion
}