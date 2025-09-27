using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;

namespace Arcanum.Core.GameObjects.Court;

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
   public bool IsRandom { get; set; }

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.CharacterNameDeclarationNUISettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.CharacterNameDeclarationAgsSettings;
   public string UniqueId
   {
      get => ToString() ?? string.Empty;
      set { }
   }
   public Eu5FileObj Source { get; set; } = null!;
   public string SavingKey { get; set; } = string.Empty;
   public static CharacterNameDeclaration Empty { get; } = new() { Name = "Arcanum_CharacterNameDeclaration_Empty", };

   #region Equals and GetHashCode

   protected bool Equals(CharacterNameDeclaration other) => Name == other.Name;

   public static Dictionary<string, CharacterNameDeclaration> GetGlobalItems() => [];

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((CharacterNameDeclaration)obj);
   }

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();

   public override string? ToString() => Name;

   #endregion

   public string GetNamespace => $"Characters.{nameof(CharacterNameDeclaration)}";

   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, Name, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.GameObjects;
}