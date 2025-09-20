using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Court;

[ObjectSaveAs]
#pragma warning disable ARC002
public partial class CharacterNameDeclaration : INUI, IAgs, IEmpty<CharacterNameDeclaration>
#pragma warning restore ARC002
{
   [SaveAs]
   [DefaultValue("")]
   [Description("The name declaration of the character.")]
   [ParseAs("name")]
   public string Name { get; set; } = string.Empty;

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.CharacterNameDeclarationNUISettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.CharacterNameDeclarationAgsSettings;
   public string SavingKey => "first_name";
   public static CharacterNameDeclaration Empty { get; } = new() { Name = "Arcanum_CharacterNameDeclaration_Empty", };

   #region Equals and GetHashCode

   protected bool Equals(CharacterNameDeclaration other) => Name == other.Name;

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

   public override int GetHashCode() => Name.GetHashCode();

   #endregion
}