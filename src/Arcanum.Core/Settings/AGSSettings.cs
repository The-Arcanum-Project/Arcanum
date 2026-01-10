using Arcanum.Core.CoreSystems.SavingSystem.AGS;

namespace Arcanum.Core.Settings;

// ReSharper disable once InconsistentNaming
public class AGSSettings
{
   public AgsSettings ModifierData { get; set; } = new();
   public AgsSettings GameObjectLocator { get; set; } = new();
   public AgsSettings CountryTemplate { get; set; } = new();
   public AgsSettings NudgeData { get; set; } = new();
   public AgsSettings VariableDeclaration { get; set; } = new();
   public AgsSettings SocientalValue { get; set; } = new();
   public AgsSettings SocientalValueEntry { get; set; } = new();
   public AgsSettings CharacterNameDeclaration { get; set; } = new();
   public AgsSettings WealthImpactData { get; set; } = new();
   public AgsSettings Age { get; set; } = new();
   public AgsSettings Vegetation { get; set; } = new();
   public AgsSettings VariableDataBlock { get; set; } = new();
   public AgsSettings OpinionValue { get; set; } = new();
   public AgsSettings BuildingsManager { get; set; } = new();
   public AgsSettings TownSetup { get; set; } = new();
   public AgsSettings Topography { get; set; } = new();
   public AgsSettings BuildingDefinition { get; set; } = new();
   public AgsSettings BuildingLevel { get; set; } = new();
   public AgsSettings InstitutionPresence { get; set; } = new();
   public AgsSettings InstitutionState { get; set; } = new();
   public AgsSettings Building { get; set; } = new();
   public AgsSettings Climate { get; set; } = new();
   public AgsSettings Road { get; set; } = new();
   public AgsSettings Province { get; set; } = new();
   public AgsSettings Area { get; set; } = new();
   public AgsSettings Region { get; set; } = new();
   public AgsSettings SuperRegion { get; set; } = new();
   public AgsSettings Continent { get; set; } = new();
   public AgsSettings Country { get; set; } = new()
   {
      CustomSaveOrder = true,
      WriteEmptyCollectionHeader = false,
      SaveOrder =
      [
         // @formatter:off
         GameObjects.InGame.Map.LocationCollections.Country.Field.CountryType,
         GameObjects.InGame.Map.LocationCollections.Country.Field.OwnControlCores,
         GameObjects.InGame.Map.LocationCollections.Country.Field.OwnControlIntegrated,
         GameObjects.InGame.Map.LocationCollections.Country.Field.OwnControlConquered,
         GameObjects.InGame.Map.LocationCollections.Country.Field.OwnControlColony, 
         GameObjects.InGame.Map.LocationCollections.Country.Field.OwnCores,
         GameObjects.InGame.Map.LocationCollections.Country.Field.OwnConquered, 
         GameObjects.InGame.Map.LocationCollections.Country.Field.OwnIntegrated,
         GameObjects.InGame.Map.LocationCollections.Country.Field.OwnColony, 
         GameObjects.InGame.Map.LocationCollections.Country.Field.ControlCores,
         GameObjects.InGame.Map.LocationCollections.Country.Field.Control,
         GameObjects.InGame.Map.LocationCollections.Country.Field.OurCoresConqueredByOthers,
         GameObjects.InGame.Map.LocationCollections.Country.Field.AddedPopsFromLocations,
         GameObjects.InGame.Map.LocationCollections.Country.Field.DiscoveredProvinces,
         GameObjects.InGame.Map.LocationCollections.Country.Field.DiscoveredAreas,
         GameObjects.InGame.Map.LocationCollections.Country.Field.DiscoveredRegions, 
         GameObjects.InGame.Map.LocationCollections.Country.Field.CourtLanguage,
         GameObjects.InGame.Map.LocationCollections.Country.Field.LiturgicalLanguage,
         GameObjects.InGame.Map.LocationCollections.Country.Field.ReligiousSchool,
         GameObjects.InGame.Map.LocationCollections.Country.Field.StartingTechLevel, 
         GameObjects.InGame.Map.LocationCollections.Country.Field.Includes,
         GameObjects.InGame.Map.LocationCollections.Country.Field.GovernmentState, 
         GameObjects.InGame.Map.LocationCollections.Country.Field.Revolt,
         GameObjects.InGame.Map.LocationCollections.Country.Field.Capital,
         GameObjects.InGame.Map.LocationCollections.Country.Field.Dynasty,
         GameObjects.InGame.Map.LocationCollections.Country.Field.ToleratedCultures,
         GameObjects.InGame.Map.LocationCollections.Country.Field.AcceptedCultures, 
         GameObjects.InGame.Map.LocationCollections.Country.Field.Flag,
         GameObjects.InGame.Map.LocationCollections.Country.Field.CountryName, 
         GameObjects.InGame.Map.LocationCollections.Country.Field.CurrencyData,
         GameObjects.InGame.Map.LocationCollections.Country.Field.CountryRank, 
         GameObjects.InGame.Map.LocationCollections.Country.Field.IsValidForRelease,
         GameObjects.InGame.Map.LocationCollections.Country.Field.Variables,
         GameObjects.InGame.Map.LocationCollections.Country.Field.AiAdvancePreferenceTags,
         GameObjects.InGame.Map.LocationCollections.Country.Field.TimedModifier,
         // @formatter:on
      ],
   };
   public AgsSettings Location { get; set; } = new();
   public AgsSettings Culture { get; set; } = new();
   public AgsSettings TimedModifier { get; set; } = new() { SkipDefaultValues = false };
   public AgsSettings JominiDate { get; set; } = new();
   public AgsSettings GovernmentState { get; set; } = new();
   public AgsSettings LocationTemplateData { get; set; } = new();
   public AgsSettings CultureGroup { get; set; } = new();
   public AgsSettings RawMaterial { get; set; } = new();
   public AgsSettings DemandData { get; set; } = new();
   public AgsSettings MapMovementAssist { get; set; } = new();
   public AgsSettings StaticModifier { get; set; } = new();
   public AgsSettings RulerTerm { get; set; } = new();
   public AgsSettings Religion { get; set; } = new();
   public AgsSettings ReligionGroup { get; set; } = new();
   public AgsSettings Eu5ObjOpinionValue { get; set; } = new();
   public AgsSettings ReligiousFaction { get; set; } = new();
   public AgsSettings ReligiousFocus { get; set; } = new();
   public AgsSettings DesignateHeirReason { get; set; } = new();
   public AgsSettings Trait { get; set; } = new();
   public AgsSettings ParliamentType { get; set; } = new();
   public AgsSettings Dynasty { get; set; } = new();
   public AgsSettings Estate { get; set; } = new();
   public AgsSettings EnactedLaw { get; set; } = new();
   public AgsSettings Institution { get; set; } = new();
   public AgsSettings ArtistType { get; set; } = new();
   public AgsSettings ModValInstance { get; set; } = new();
   public AgsSettings EstateCountDefiniton { get; set; } = new();
   public AgsSettings RegnalNumber { get; set; } = new();
   public AgsSettings CountryRank { get; set; } = new();
   public AgsSettings ParliamentDefinition { get; set; } = new();
   public AgsSettings ReligiousSchoolRelations { get; set; } = new();
   public AgsSettings CountryDefinition { get; set; } = new();
   public AgsSettings Regency { get; set; } = new();
   public AgsSettings Market { get; set; } = new();
   public AgsSettings LocationRank { get; set; } = new();
   public AgsSettings Language { get; set; } = new();
   public AgsSettings Character { get; set; } = new();
   public AgsSettings ReligiousSchool { get; set; } = new();
   public AgsSettings ReligiousSchoolOpinionValue { get; set; } = new();
   public AgsSettings PopType { get; set; } = new();
   public AgsSettings PopDefinition { get; set; } = new();
   public AgsSettings SoundToll { get; set; } = new();
   public AgsSettings DefaultMapDefinition { get; set; } = new();
   public AgsSettings EstateAttributeDefinition { get; set; } = new();
   public AgsSettings EstateSatisfactionDefinition { get; set; } = new();
}