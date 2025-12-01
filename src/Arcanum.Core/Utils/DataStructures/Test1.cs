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

[ObjectSaveAs]
[NexusConfig]
public partial class Test1 : IEu5Object<Test1>
{
    [DefaultValue(null)]
    public Test2 ParentRef
    {
        get;
        set
        {
            if (!field.Child.Lock && !value.Child.Lock)
            {
                if (field != Test2.Empty)
                    field.Child._removeFromChild(this);
                if (field != Test2.Empty)
                    field.Child._addFromChild(this);
            }

            field = value;
        }
    } = Test2.Empty;

#pragma warning disable AGS004
    [Description("Unique key of this Test1. Must be unique among all objects of this type.")]
    [DefaultValue("null")]
    public string UniqueId { get; set; } = null!;

    [SuppressAgs] public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

    #region IEu5Object

    public string GetNamespace => $"Namespace.{nameof(Test1)}";
    public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
    public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
    public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
    public bool IsReadonly => false;
    public NUISetting NUISettings => null!;
    public INUINavigation[] Navigations => [];
    public AgsSettings AgsSettings => null!; //Config.Settings.AgsSettings.Test1AgsSettings;
    public static Dictionary<string, Test1> GetGlobalItems() => [];
    public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
    public InjRepType InjRepType { get; set; } = InjRepType.None;

    public static Test1 Empty { get; } = new() { UniqueId = "Arcanum_Empty_Test1" };

    #endregion
}