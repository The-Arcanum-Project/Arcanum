using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;
using Nexus.Core.Attributes;

namespace Arcanum.Core.Utils.DataStructures;

[NexusConfig]
[ObjectSaveAs]
public partial class Test2 : IEu5Object<Test2>
{
#pragma warning disable ARC001

   [SuppressAgs]
   [DefaultValue(null)]
   public AggregateParentLink<Test1> Child { get; set; }

   public Test2()
   {
      Child = new(Test1.Field.ParentRef, this);
   }

#pragma warning restore ARC001
#pragma warning disable AGS004
   [Description("Unique key of this Test2. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Namespace.{nameof(Test2)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => null!;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => null!;
   public static Dictionary<string, Test2> GetGlobalItems() => [];
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static Test2 Empty { get; } = new() { UniqueId = "Arcanum_Empty_Test2" };

   #endregion
}