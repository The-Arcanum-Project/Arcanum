using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Religious;
using Common.UI;
using Estate = Arcanum.Core.GameObjects.Cultural.Estate;
using ArtistType = Arcanum.Core.GameObjects.Cultural.ArtistType;

namespace Arcanum.Core.GameObjects.Court;

[ObjectSaveAs]
public partial class Character : IEu5Object<Character>
{
   public Character()
   {
      Mother = Empty;
      PregnancyRealFather = Empty;
      Father = Empty;
   }

   private Character(string uniqueId)
   {
      UniqueId = uniqueId;
   }

   #region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("first_name", AstNodeType.StatementNode)]
   [Description("The character's first name.")]
   public CharacterNameDeclaration FirstName { get; set; } = CharacterNameDeclaration.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("last_name", AstNodeType.StatementNode)]
   [Description("The character's first name.")]
   public CharacterNameDeclaration LastName { get; set; } = CharacterNameDeclaration.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("nickname", AstNodeType.StatementNode)]
   [Description("The character's first name.")]
   public CharacterNameDeclaration NickName { get; set; } = CharacterNameDeclaration.Empty;

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("female")]
   [Description("Is this character female?")]
   public bool IsFemale { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("has_patronym")]
   [Description("If true, this character uses a patronymic naming system.")]
   public bool HasPatronym { get; set; }

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [ParseAs("mother")]
   [Description("The mother of this character.")]
   public Character Mother { get; set; } = null!;

   [SaveAs(SavingValueType.Identifier, isShattered: true)]
   [DefaultValue(null)]
   [ParseAs("spouse", isShatteredList: true, itemNodeType: AstNodeType.ContentNode)]
   [Description("The spouse of this character. Can have multiple.")]
   public ObservableRangeCollection<Character> Spouses { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [ParseAs("father")]
   [Description("The father of this character.")]
   public Character Father { get; set; } = null!;

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(null)]
   [ParseAs("pregnancy_real_father")]
   [Description("The real father of this character.")]
   public Character PregnancyRealFather { get; set; } = null!;

   [SaveAs]
   [ParseAs("religious_school")]
   [DefaultValue(null)]
   [Description("The religious school of this character.")]
   public ReligiousSchool ReligiousSchool { get; set; } = ReligiousSchool.Empty;

   [PropertyConfig(minValue: 0, maxValue: 100)]
   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("adm")]
   [Description("The administrative skill of this character.")]
   public int Adm { get; set; }

   [PropertyConfig(minValue: 0, maxValue: 100)]
   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("dip")]
   [Description("The diplomatic skill of this character.")]
   public int Dip { get; set; }

   [PropertyConfig(minValue: 0, maxValue: 100)]
   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("mil")]
   [Description("The military skill of this character.")]
   public int Mil { get; set; }

   [PropertyConfig(minValue: 0, maxValue: 100 )]
   [SaveAs]
   [DefaultValue(0f)]
   [ParseAs("fertility")]
   [Description("The fertility of this character.")]
   public float Fertility { get; set; }

   [PropertyConfig(minValue: 0, maxValue: 1)]
   [SaveAs]
   [DefaultValue(0f)]
   [ParseAs("artist_skill")]
   [Description("The artistic skill of this character.")]
   public float ArtistSkill { get; set; }

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue("")]
   [ParseAs("artist")]
   [Description("The type of artist this character is.")]
   public ArtistType ArtistType { get; set; } = ArtistType.Empty;

    //TODO: Alias 'traits'
   [SaveAs(isShattered: true)]
   [DefaultValue(null)]
   [ParseAs("ruler_trait", isShatteredList: true)]
   [Description("The traits of this character.")]
   public ObservableRangeCollection<Trait> RulerTraits { get; set; } = [];

   [SaveAs(isShattered: true)]
   [DefaultValue(null)]
   [ParseAs("artist_trait", isShatteredList: true)]
   [Description("The artistic traits of this character.")]
   public ObservableRangeCollection<Trait> ArtistTraits { get; set; } = [];

   [SaveAs(isShattered: true)]
   [DefaultValue(null)]
   [ParseAs("explorer_trait", isShatteredList: true)]
   [Description("The explorer traits of this character.")]
   public ObservableRangeCollection<Trait> ExplorerTraits { get; set; } = [];

   [SaveAs(isShattered: true)]
   [DefaultValue(null)]
   [ParseAs("child_trait", isShatteredList: true)]
   [Description("The traits of this child character.")]
   public ObservableRangeCollection<Trait> ChildTraits { get; set; } = [];

   [SaveAs(isShattered: true)]
   [DefaultValue(null)]
   [ParseAs("religious_figure_trait", isShatteredList: true)]
   [Description("The religous traits of this character.")]
   public ObservableRangeCollection<Trait> ReligiousFigureTraits { get; set; } = [];

   [SaveAs(isShattered: true)]
   [DefaultValue(null)]
   [ParseAs("admiral_trait", isShatteredList: true)]
   [Description("The admiral traits of this character.")]
   public ObservableRangeCollection<Trait> AdmiralTraits { get; set; } = [];

   [SaveAs(isShattered: true)]
   [DefaultValue(null)]
   [ParseAs("general_trait", isShatteredList: true)]
   [Description("The general traits of this character.")]
   public ObservableRangeCollection<Trait> GeneralTraits { get; set; } = [];

   [SaveAs(isShattered: true, isEmbeddedObject: true)]
   [ParseAs("timed_modifier",
              AstNodeType.BlockNode,
              isEmbedded: true,
              isShatteredList: true,
              itemNodeType: AstNodeType.BlockNode)]
   [DefaultValue(null)]
   [Description("A modifier starting and ending at a given date.")]
   public ObservableRangeCollection<TimedModifier> TimedModifier { get; set; } = [];

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue(0)]
   [ParseAs("estate")]
   [Description("The estate this character belongs to.")]
   public Estate Estate { get; set; } = Estate.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue("")]
   [ParseAs("culture")]
   [Description("The culture this character belongs to.")]
   public Culture Culture { get; set; } = Culture.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue("")]
   [ParseAs("religion")]
   [Description("The religion this character follows.")]
   public Religion Religion { get; set; } = Religion.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("birth_date")]
   [Description("The character's birth date.")]
   public JominiDate BirthDate { get; set; } = JominiDate.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("death_date")]
   [Description("The character's death date.")]
   public JominiDate DeathDate { get; set; } = JominiDate.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("birth")]
   [Description("The location where this character was born.")]
   public Location BirthPlace { get; set; } = Location.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue("")]
   [ParseAs("dynasty")]
   [Description("The dynasty of this character")]
   public Dynasty Dynasty { get; set; } = Dynasty.Empty;

   //TODO: Alias 'country'
   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("tag")]
   [Description("The country this is a character of.")]
   public Country AssociatedCountry { get; set; } = Country.Empty;

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this Character. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public string GetNamespace => $"Court.{nameof(Character)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.CharacterSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.CharacterAgsSettings;
   public static Dictionary<string, Character> GetGlobalItems() => Globals.Characters;

   private static readonly Lazy<Character> EmptyInstance = new(() =>
   {
      var emptyChar = new Character("Arcanum_Empty_Character");
      emptyChar.Mother = emptyChar;
      emptyChar.Father = emptyChar;
      emptyChar.PregnancyRealFather = emptyChar;

      return emptyChar;
   });

   public static Character Empty => EmptyInstance.Value;

   private static readonly Lazy<Character> RandomInstance = new(() => new("random"));
   public static Character RandomCharacter => RandomInstance.Value;

   #endregion

   #region Equality Members

   protected bool Equals(Character other) => UniqueId == other.UniqueId;

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((Character)obj);
   }

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => UniqueId.GetHashCode();

   #endregion

   public override string ToString() => UniqueId;
}