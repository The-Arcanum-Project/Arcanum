using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;
using Arcanum.Core.GameObjects.InGame.AbstractMechanics;
using Arcanum.Core.GameObjects.InGame.gfx.map;
using Adjacency = Arcanum.Core.GameObjects.InGame.Map.Adjacency;
using Age = Arcanum.Core.GameObjects.InGame.AbstractMechanics.Age;
using Area = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Area;
using Country = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Country;
using Culture = Arcanum.Core.GameObjects.InGame.Cultural.Culture;
using Estate = Arcanum.Core.GameObjects.InGame.Cultural.Estate;
using Institution = Arcanum.Core.GameObjects.InGame.Cultural.Institution;
using Language = Arcanum.Core.GameObjects.InGame.Cultural.Language;
using LocationRank = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.LocationRank;
using ParliamentType = Arcanum.Core.GameObjects.InGame.Court.ParliamentType;
using Regency = Arcanum.Core.GameObjects.InGame.Court.Regency;
using Region = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Region;
using Religion = Arcanum.Core.GameObjects.InGame.Religious.Religion;
using ReligionGroup = Arcanum.Core.GameObjects.InGame.Religious.ReligionGroup;
using ReligiousFaction = Arcanum.Core.GameObjects.InGame.Religious.ReligiousFaction;
using ReligiousFocus = Arcanum.Core.GameObjects.InGame.Religious.SubObjects.ReligiousFocus;
using ReligiousSchool = Arcanum.Core.GameObjects.InGame.Religious.ReligiousSchool;
using Road = Arcanum.Core.GameObjects.InGame.Map.Road;
using ArtistType = Arcanum.Core.GameObjects.InGame.Cultural.ArtistType;
using Building = Arcanum.Core.GameObjects.InGame.Economy.Building;
using BuildingLevel = Arcanum.Core.GameObjects.InGame.Economy.SubClasses.BuildingLevel;
using BuildingsManager = Arcanum.Core.GameObjects.MainMenu.States.BuildingsManager;
using Character = Arcanum.Core.GameObjects.InGame.Court.Character;
using Climate = Arcanum.Core.GameObjects.InGame.Map.Climate;
using Continent = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Continent;
using CountryDefinition = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects.CountryDefinition;
using CountryRank = Arcanum.Core.GameObjects.InGame.CountryLevel.CountryRank;
using CultureGroup = Arcanum.Core.GameObjects.InGame.Cultural.CultureGroup;
using DefaultMapDefinition = Arcanum.Core.GameObjects.InGame.Map.DefaultMapDefinition;
using DesignateHeirReason = Arcanum.Core.GameObjects.InGame.Court.State.SubClasses.DesignateHeirReason;
using Dynasty = Arcanum.Core.GameObjects.InGame.Court.Dynasty;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using LocationTemplateData = Arcanum.Core.GameObjects.InGame.Map.LocationTemplateData;
using Market = Arcanum.Core.GameObjects.InGame.Economy.Market;
using ModifierDefinition = Arcanum.Core.GameObjects.InGame.Common.ModifierDefinition;
using PopType = Arcanum.Core.GameObjects.InGame.Pops.PopType;
using Province = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Province;
using RawMaterial = Arcanum.Core.GameObjects.InGame.Economy.RawMaterial;
using SocientalValue = Arcanum.Core.GameObjects.InGame.Court.State.SubClasses.SocientalValue;
using StaticModifier = Arcanum.Core.GameObjects.InGame.Common.StaticModifier;
using SuperRegion = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SuperRegion;
using Topography = Arcanum.Core.GameObjects.InGame.Map.Topography;
using Trait = Arcanum.Core.GameObjects.InGame.Court.Trait;
using Vegetation = Arcanum.Core.GameObjects.InGame.Map.Vegetation;

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

   public static Dictionary<string, GameObjectLocator> GameObjectLocators { get; } = [];

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