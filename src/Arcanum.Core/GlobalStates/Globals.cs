using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;
using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GameObjects.Court.State.SubClasses;
using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GameObjects.Economy.SubClasses;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.SubObjects;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GameObjects.Pops;
using Adjacency = Arcanum.Core.GameObjects.Map.Adjacency;
using Country = Arcanum.Core.GameObjects.LocationCollections.Country;
using Culture = Arcanum.Core.GameObjects.Cultural.Culture;
using Estate = Arcanum.Core.GameObjects.Cultural.Estate;
using Institution = Arcanum.Core.GameObjects.Cultural.Institution;
using Language = Arcanum.Core.GameObjects.Cultural.Language;
using LocationRank = Arcanum.Core.GameObjects.LocationCollections.LocationRank;
using ParliamentType = Arcanum.Core.GameObjects.Court.ParliamentType;
using Regency = Arcanum.Core.GameObjects.Court.Regency;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;
using Religion = Arcanum.Core.GameObjects.Religious.Religion;
using ReligionGroup = Arcanum.Core.GameObjects.Religious.ReligionGroup;
using ReligiousFaction = Arcanum.Core.GameObjects.Religious.ReligiousFaction;
using ReligiousFocus = Arcanum.Core.GameObjects.Religious.SubObjects.ReligiousFocus;
using ReligiousSchool = Arcanum.Core.GameObjects.Religious.ReligiousSchool;
using Road = Arcanum.Core.GameObjects.Map.Road;
using ArtistType = Arcanum.Core.GameObjects.Cultural.ArtistType;
using BuildingsManager = Arcanum.Core.GameObjects.MainMenu.States.BuildingsManager;

namespace Arcanum.Core.GlobalStates;

public enum AppState
{
   Error,
   Loading,
   EditingAllowed,
   EditingDisabled,
   Saving,
}

public static class Globals
{
   public const string REPLACE_DESCRIPTION = "???REPLACE_ME???";
   public const string DO_NOT_PARSE_ME = "DO_NOT_PARSE_ME";

   public static GameState State { get; set; } = new();

   public static DefaultMapDefinition DefaultMapDefinition { get; set; } = null!;
   public static Dictionary<string, PopType> PopTypes { get; } = [];
   public static Dictionary<string, LocationRank> LocationRanks { get; } = [];
   public static Dictionary<string, CountryRank> CountryRanks { get; } = [];
   public static Dictionary<string, Road> Roads { get; set; } = [];

   public static Dictionary<string, Country> Countries { get; set; } = [];
   public static Dictionary<string, Institution> Institutions { get; set; } = [];
   public static Dictionary<string, ArtistType> ArtistTypes { get; set; } = [];
   public static Dictionary<string, TownSetup> TownSetups { get; set; } = [];
   public static Dictionary<string, BuildingLevel> BuildingLevels { get; set; } = [];
   public static Dictionary<string, SocientalValue> SocientalValues { get; } = [];

   #region Religion

   public static Dictionary<string, ReligiousFocus> ReligiousFocuses { get; set; } = [];
   public static Dictionary<string, Religion> Religions { get; } = [];
   public static Dictionary<string, ReligionGroup> ReligionGroups { get; } = [];
   public static Dictionary<string, ReligiousSchool> ReligiousSchools { get; set; } = [];
   public static Dictionary<string, ReligiousFaction> ReligiousFactions { get; set; } = [];

   #endregion

   #region Culture

   public static Dictionary<string, Culture> Cultures { get; } = [];
   public static Dictionary<string, CultureGroup> CultureGroups { get; } = [];

   #endregion

   #region Map

   public static Dictionary<string, LocationTemplateData> LocationTemplateData { get; } = [];
   public static Dictionary<string, CountryDefinition> CountryDefinitions { get; } = [];
   public static Dictionary<string, Climate> Climates { get; set; } = [];
   public static Dictionary<string, Vegetation> Vegetation { get; set; } = [];
   public static Dictionary<string, Topography> Topography { get; set; } = [];
   public static Dictionary<string, Location> Locations { get; } = [];
   public static Dictionary<string, Province> Provinces { get; } = [];
   public static Dictionary<string, Area> Areas { get; } = [];
   public static Dictionary<string, Region> Regions { get; } = [];
   public static Dictionary<string, SuperRegion> SuperRegions { get; } = [];
   public static Dictionary<string, Continent> Continents { get; } = [];
   public static Dictionary<string, Adjacency> Adjacencies { get; } = [];

   #endregion

   #region Court

   public static Dictionary<string, ParliamentType> ParliamentTypes { get; set; } = [];
   public static Dictionary<string, Trait> Traits { get; } = [];
   public static Dictionary<string, DesignateHeirReason> DesignateHeirReasons { get; set; } = [];
   public static Dictionary<string, Language> Languages { get; set; } = [];
   public static Dictionary<string, Language> Dialects { get; set; } = [];
   public static Dictionary<string, Regency> Regencies { get; set; } = [];
   public static Dictionary<string, Character> Characters { get; } = [];
   public static Dictionary<string, Dynasty> Dynasties { get; } = [];
   public static Dictionary<string, Estate> Estates { get; } = [];

   #endregion

   #region Common

   public static Dictionary<string, ModifierDefinition> ModifierDefinitions { get; set; } = [];

   #endregion

   #region Economy

   public static Dictionary<string, Building> Buildings { get; } = [];
   public static Dictionary<string, RawMaterial> RawMaterials { get; } = [];
   public static Dictionary<string, Market> Markets { get; } = [];
   public static BuildingsManager BuildingsManager { get; set; } = new();

   #endregion

   #region Modifiers

   public static Dictionary<string, StaticModifier> StaticModifiers { get; } = [];

   #endregion

   public static Dictionary<string, Age> Ages { get; set; } = [];

   public static SetupContentNodes SetupContentNodes { get; set; } = new();
}