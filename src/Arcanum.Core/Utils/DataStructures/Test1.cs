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
    private Test2 _parentRef = Test2.Empty;
    
    [DefaultValue(null)]
    public Test2 ParentRef
    {
        get => _parentRef;
        set
        {
            if (!_parentRef.Child.Lock && !value.Child.Lock)
            {
                if (_parentRef != Test2.Empty)
                    _parentRef.Child._removeFromChild(this);
                if (_parentRef != Test2.Empty)
                    _parentRef.Child._addFromChild(this);
            }

            _parentRef = value;
        }
    }

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