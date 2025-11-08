using Arcanum.Core.CoreSystems.Jomini.AiTags;
using Arcanum.Core.CoreSystems.Jomini.AudioTags;
using Arcanum.Core.CoreSystems.Jomini.CurrencyDatas;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.Jomini.Effects;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GameObjects.Court.State;
using Arcanum.Core.GameObjects.Court.State.SubClasses;
using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GameObjects.Economy.SubClasses;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.MainMenu.States;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GameObjects.Map.SubObjects;
using Arcanum.Core.GameObjects.Pops;
using Adjacency = Arcanum.Core.GameObjects.Map.Adjacency;
using Culture = Arcanum.Core.GameObjects.Cultural.Culture;
using CultureOpinionValue = Arcanum.Core.GameObjects.Cultural.SubObjects.CultureOpinionValue;
using Estate = Arcanum.Core.GameObjects.Cultural.Estate;
using Institution = Arcanum.Core.GameObjects.Cultural.Institution;
using InstitutionState = Arcanum.Core.GameObjects.Cultural.SubObjects.InstitutionState;
using Language = Arcanum.Core.GameObjects.Cultural.Language;
using LocationRank = Arcanum.Core.GameObjects.LocationCollections.LocationRank;
using ModValInstance = Arcanum.Core.CoreSystems.Jomini.Modifiers.ModValInstance;
using ParliamentType = Arcanum.Core.GameObjects.Court.ParliamentType;
using Regency = Arcanum.Core.GameObjects.Court.Regency;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;
using Religion = Arcanum.Core.GameObjects.Religious.Religion;
using ReligionGroup = Arcanum.Core.GameObjects.Religious.ReligionGroup;
using ReligionOpinionValue = Arcanum.Core.GameObjects.Cultural.SubObjects.ReligionOpinionValue;
using ReligiousFaction = Arcanum.Core.GameObjects.Religious.ReligiousFaction;
using ReligiousFocus = Arcanum.Core.GameObjects.Religious.SubObjects.ReligiousFocus;
using ReligiousSchool = Arcanum.Core.GameObjects.Religious.ReligiousSchool;
using ReligiousSchoolOpinionValue = Arcanum.Core.GameObjects.Religious.SubObjects.ReligiousSchoolOpinionValue;
using ReligiousSchoolRelations = Arcanum.Core.GameObjects.Religious.SubObjects.ReligiousSchoolRelations;
using Road = Arcanum.Core.GameObjects.Map.Road;

namespace Arcanum.Core.Settings;

public class NUISettings
{
   public NUISetting PopSettings { get; set; } = new(Pop.Field.Type,
                                                     Enum.GetValues<Pop.Field>().Cast<Enum>().ToArray(),
                                                     Enum.GetValues<Pop.Field>().Cast<Enum>().ToArray(),
                                                     Enum.GetValues<Pop.Field>().Cast<Enum>().ToArray());
   public NUISetting PopTypeSettings { get; set; } = new(PopType.Field.UniqueId,
                                                         Enum.GetValues<PopType.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<PopType.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<PopType.Field>().Cast<Enum>().ToArray());
   public NUISetting LocationSettings { get; set; } = new(Location.Field.UniqueId,
                                                          Enum.GetValues<Location.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<Location.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<Location.Field>().Cast<Enum>().ToArray());

   public NUISetting MarketSettings { get; set; } = new(Market.Field.Location,
                                                        Enum.GetValues<Market.Field>().Cast<Enum>().ToArray(),
                                                        Enum.GetValues<Market.Field>().Cast<Enum>().ToArray(),
                                                        Enum.GetValues<Market.Field>().Cast<Enum>().ToArray());
   public NUISetting ProvinceSettings { get; set; } = new(Province.Field.UniqueId,
                                                          Enum.GetValues<Province.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<Province.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<Province.Field>().Cast<Enum>().ToArray());

   public NUISetting AreaSettings { get; set; } = new(Area.Field.UniqueId,
                                                      Enum.GetValues<Area.Field>().Cast<Enum>().ToArray(),
                                                      Enum.GetValues<Area.Field>().Cast<Enum>().ToArray(),
                                                      Enum.GetValues<Area.Field>().Cast<Enum>().ToArray());

   public NUISetting RegionSettings { get; set; } = new(Region.Field.UniqueId,
                                                        Enum.GetValues<Region.Field>().Cast<Enum>().ToArray(),
                                                        Enum.GetValues<Region.Field>().Cast<Enum>().ToArray(),
                                                        Enum.GetValues<Region.Field>().Cast<Enum>().ToArray());

   public NUISetting SuperRegionSettings { get; set; } = new(SuperRegion.Field.UniqueId,
                                                             Enum.GetValues<SuperRegion.Field>().Cast<Enum>().ToArray(),
                                                             Enum.GetValues<SuperRegion.Field>().Cast<Enum>().ToArray(),
                                                             Enum.GetValues<SuperRegion.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray());

   public NUISetting ContinentSettings { get; set; } = new(Continent.Field.UniqueId,
                                                           Enum.GetValues<Continent.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<Continent.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<Continent.Field>().Cast<Enum>().ToArray());

   public NUISetting AdjacencySettings { get; set; } = new(Adjacency.Field.Name,
                                                           Enum.GetValues<Adjacency.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<Adjacency.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<Adjacency.Field>().Cast<Enum>().ToArray());

   public NUISetting LocationRankSettings { get; set; } = new(LocationRank.Field.UniqueId,
                                                              Enum.GetValues<LocationRank.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<LocationRank.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<LocationRank.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray());

   public NUISetting RoadSettings { get; set; } = new(Road.Field.StartLocation,
                                                      Enum.GetValues<Road.Field>().Cast<Enum>().ToArray(),
                                                      Enum.GetValues<Road.Field>().Cast<Enum>().ToArray(),
                                                      Enum.GetValues<Road.Field>().Cast<Enum>().ToArray());

   public NUISetting CountrySettings { get; set; } = new(Country.Field.UniqueId,
                                                         Enum.GetValues<Country.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<Country.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<Country.Field>().Cast<Enum>().ToArray());

   public NUISetting CountryRankSettings { get; set; } = new(CountryRank.Field.UniqueId,
                                                             Enum.GetValues<CountryRank.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray(),
                                                             Enum.GetValues<CountryRank.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray(),
                                                             Enum.GetValues<CountryRank.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray());

   public NUISetting InstitutionSettings { get; set; } = new(Institution.Field.UniqueId,
                                                             Enum.GetValues<Institution.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray(),
                                                             Enum.GetValues<Institution.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray(),
                                                             Enum.GetValues<Institution.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray());


    public NUISetting ArtistTypeSettings { get; set; } = new(ArtistType.Field.UniqueId,
                                                              Enum.GetValues<ArtistType.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<ArtistType.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<ArtistType.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray());

    public NUISetting ReligiousSchoolSettings { get; set; } = new(ReligiousSchool.Field.UniqueId,
                                                                 Enum.GetValues<ReligiousSchool.Field>()
                                                                     .Cast<Enum>()
                                                                     .ToArray(),
                                                                 Enum.GetValues<ReligiousSchool.Field>()
                                                                     .Cast<Enum>()
                                                                     .ToArray(),
                                                                 Enum.GetValues<ReligiousSchool.Field>()
                                                                     .Cast<Enum>()
                                                                     .ToArray());

   public NUISetting CultureSettings { get; set; } = new(Culture.Field.UniqueId,
                                                         Enum.GetValues<Culture.Field>()
                                                             .Cast<Enum>()
                                                             .ToArray(),
                                                         Enum.GetValues<Culture.Field>()
                                                             .Cast<Enum>()
                                                             .ToArray(),
                                                         Enum.GetValues<Culture.Field>()
                                                             .Cast<Enum>()
                                                             .ToArray());

   public NUISetting LanguageNuiSettings { get; set; } = new(Language.Field.UniqueId,
                                                             Enum.GetValues<Language.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray(),
                                                             Enum.GetValues<Language.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray(),
                                                             Enum.GetValues<Language.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray());

   public NUISetting AgeSettings { get; set; } = new(Age.Field.UniqueId,
                                                     Enum.GetValues<Age.Field>().Cast<Enum>().ToArray(),
                                                     Enum.GetValues<Age.Field>().Cast<Enum>().ToArray(),
                                                     Enum.GetValues<Age.Field>().Cast<Enum>().ToArray());

   public NUISetting ClimateSettings { get; set; } = new(Climate.Field.UniqueId,
                                                         Enum.GetValues<Climate.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<Climate.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<Climate.Field>().Cast<Enum>().ToArray());

   public NUISetting VegetationSettings { get; set; } = new(Vegetation.Field.UniqueId,
                                                            Enum.GetValues<Vegetation.Field>().Cast<Enum>().ToArray(),
                                                            Enum.GetValues<Vegetation.Field>().Cast<Enum>().ToArray(),
                                                            Enum.GetValues<Vegetation.Field>().Cast<Enum>().ToArray());

   public NUISetting ModifierDefinitionSettings { get; set; } = new(ModifierDefinition.Field.UniqueId,
                                                                    Enum.GetValues<ModifierDefinition.Field>()
                                                                        .Cast<Enum>()
                                                                        .ToArray(),
                                                                    Enum.GetValues<ModifierDefinition.Field>()
                                                                        .Cast<Enum>()
                                                                        .ToArray(),
                                                                    Enum.GetValues<ModifierDefinition.Field>()
                                                                        .Cast<Enum>()
                                                                        .ToArray());

   public NUISetting ModifierGameDataSettings { get; set; } = new(ModifierGameData.Field.Category,
                                                                  Enum.GetValues<ModifierGameData.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray(),
                                                                  Enum.GetValues<ModifierGameData.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray(),
                                                                  Enum.GetValues<ModifierGameData.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray());

   public NUISetting TopographySettings { get; set; } = new(Topography.Field.UniqueId,
                                                            Enum.GetValues<Topography.Field>().Cast<Enum>().ToArray(),
                                                            Enum.GetValues<Topography.Field>().Cast<Enum>().ToArray(),
                                                            Enum.GetValues<Topography.Field>().Cast<Enum>().ToArray());

   public NUISetting ModValInstanceSettings { get; set; } = new(ModValInstance.Field.Type,
                                                                Enum.GetValues<ModValInstance.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray(),
                                                                Enum.GetValues<ModValInstance.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray(),
                                                                Enum.GetValues<ModValInstance.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray());

   public NUISetting AudioTagSettings { get; set; } = new(AudioTag.Field.UniqueId,
                                                          Enum.GetValues<AudioTag.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<AudioTag.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<AudioTag.Field>().Cast<Enum>().ToArray());

   public NUISetting EffectDefinitionSettings { get; set; } = new(EffectDefinition.Field.Name,
                                                                  Enum.GetValues<EffectDefinition.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray(),
                                                                  Enum.GetValues<EffectDefinition.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray(),
                                                                  Enum.GetValues<EffectDefinition.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray());

   public NUISetting CurrencyDataSettings { get; set; } = new(CurrencyData.Field.UniqueId,
                                                              Enum.GetValues<CurrencyData.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<CurrencyData.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<CurrencyData.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray());

   public NUISetting AiTagSettings { get; set; } = new(AiTag.Field.UniqueId,
                                                       Enum.GetValues<AiTag.Field>().Cast<Enum>().ToArray(),
                                                       Enum.GetValues<AiTag.Field>().Cast<Enum>().ToArray(),
                                                       Enum.GetValues<AiTag.Field>().Cast<Enum>().ToArray());

   public NUISetting TimedModifierSettings { get; set; } = new(TimedModifier.Field.UniqueId,
                                                               Enum.GetValues<TimedModifier.Field>()
                                                                   .Cast<Enum>()
                                                                   .ToArray(),
                                                               Enum.GetValues<TimedModifier.Field>()
                                                                   .Cast<Enum>()
                                                                   .ToArray(),
                                                               Enum.GetValues<TimedModifier.Field>()
                                                                   .Cast<Enum>()
                                                                   .ToArray());

   public NUISetting JominiDateSettings { get; set; } = new(JominiDate.Field.Year,
                                                            Enum.GetValues<JominiDate.Field>().Cast<Enum>().ToArray(),
                                                            Enum.GetValues<JominiDate.Field>().Cast<Enum>().ToArray(),
                                                            Enum.GetValues<JominiDate.Field>().Cast<Enum>().ToArray());

   public NUISetting GovernmentStateSettings { get; set; } = new(GovernmentState.Field.Ruler,
                                                                 Enum.GetValues<GovernmentState.Field>()
                                                                     .Cast<Enum>()
                                                                     .ToArray(),
                                                                 Enum.GetValues<GovernmentState.Field>()
                                                                     .Cast<Enum>()
                                                                     .ToArray(),
                                                                 Enum.GetValues<GovernmentState.Field>()
                                                                     .Cast<Enum>()
                                                                     .ToArray());

   public NUISetting RulerTermSettings { get; set; } = new(RulerTerm.Field.CharacterId,
                                                           Enum.GetValues<RulerTerm.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<RulerTerm.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<RulerTerm.Field>().Cast<Enum>().ToArray());

   public NUISetting EnactedLawSettings { get; set; } = new(EnactedLaw.Field.Key,
                                                            Enum.GetValues<EnactedLaw.Field>().Cast<Enum>().ToArray(),
                                                            Enum.GetValues<EnactedLaw.Field>().Cast<Enum>().ToArray(),
                                                            Enum.GetValues<EnactedLaw.Field>().Cast<Enum>().ToArray());
   public NUISetting RegnalNumberNUISettings { get; set; } = new(RegnalNumber.Field.Key,
                                                                 Enum.GetValues<RegnalNumber.Field>()
                                                                     .Cast<Enum>()
                                                                     .ToArray(),
                                                                 Enum.GetValues<RegnalNumber.Field>()
                                                                     .Cast<Enum>()
                                                                     .ToArray(),
                                                                 Enum.GetValues<RegnalNumber.Field>()
                                                                     .Cast<Enum>()
                                                                     .ToArray());
   public NUISetting ParliamentDefinitionSettings { get; set; } = new(ParliamentDefinition.Field.Type,
                                                                      Enum.GetValues<ParliamentDefinition.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray(),
                                                                      Enum.GetValues<ParliamentDefinition.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray(),
                                                                      Enum.GetValues<ParliamentDefinition.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray());

   public NUISetting RegencySettings { get; set; } = new(Regency.Field.UniqueId,
                                                         Enum.GetValues<Regency.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<Regency.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<Regency.Field>().Cast<Enum>().ToArray());

   public NUISetting CharacterSettings { get; set; } = new(Character.Field.UniqueId,
                                                           Enum.GetValues<Character.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<Character.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<Character.Field>().Cast<Enum>().ToArray());

   public NUISetting CharacterNameDeclarationNUISettings { get; set; } = new(CharacterNameDeclaration.Field.Name,
                                                                             Enum.GetValues<CharacterNameDeclaration.
                                                                                   Field>()
                                                                               .Cast<Enum>()
                                                                               .ToArray(),
                                                                             Enum.GetValues<CharacterNameDeclaration.
                                                                                   Field>()
                                                                               .Cast<Enum>()
                                                                               .ToArray(),
                                                                             Enum.GetValues<CharacterNameDeclaration.
                                                                                   Field>()
                                                                               .Cast<Enum>()
                                                                               .ToArray());
   public NUISetting OpinionValueSettings { get; set; } = new(CultureOpinionValue.Field.Key,
                                                              Enum.GetValues<CultureOpinionValue.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<CultureOpinionValue.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<CultureOpinionValue.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray());

   public NUISetting InstitutionStateSettings { get; set; } = new(InstitutionState.Field.UniqueId,
                                                                  Enum.GetValues<InstitutionState.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray(),
                                                                  Enum.GetValues<InstitutionState.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray(),
                                                                  Enum.GetValues<InstitutionState.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray());

   public NUISetting InstitutionManagerSettings { get; set; } = new(InstitutionManager.Field.UniqueId,
                                                                    Enum.GetValues<InstitutionManager.Field>()
                                                                        .Cast<Enum>()
                                                                        .ToArray(),
                                                                    Enum.GetValues<InstitutionManager.Field>()
                                                                        .Cast<Enum>()
                                                                        .ToArray(),
                                                                    Enum.GetValues<InstitutionManager.Field>()
                                                                        .Cast<Enum>()
                                                                        .ToArray());

   public NUISetting ReligiousSchoolOpinionValueSettings { get; set; } = new(ReligiousSchoolOpinionValue.Field.Key,
                                                                             Enum.GetValues<ReligiousSchoolOpinionValue.
                                                                                   Field>()
                                                                               .Cast<Enum>()
                                                                               .ToArray(),
                                                                             Enum.GetValues<ReligiousSchoolOpinionValue.
                                                                                   Field>()
                                                                               .Cast<Enum>()
                                                                               .ToArray(),
                                                                             Enum.GetValues<ReligiousSchoolOpinionValue.
                                                                                   Field>()
                                                                               .Cast<Enum>()
                                                                               .ToArray());

   public NUISetting ReligiousSchoolRelationsSettings { get; set; } = new(ReligiousSchoolRelations.Field.UniqueId,
                                                                          Enum.GetValues<ReligiousSchoolRelations.
                                                                                  Field>()
                                                                              .Cast<Enum>()
                                                                              .ToArray(),
                                                                          Enum.GetValues<ReligiousSchoolRelations.
                                                                                  Field>()
                                                                              .Cast<Enum>()
                                                                              .ToArray(),
                                                                          Enum.GetValues<ReligiousSchoolRelations.
                                                                                  Field>()
                                                                              .Cast<Enum>()
                                                                              .ToArray());

   public NUISetting PopDefinitionSettings { get; set; } = new(PopDefinition.Field.UniqueId,
                                                               Enum.GetValues<PopDefinition.Field>()
                                                                   .Cast<Enum>()
                                                                   .ToArray(),
                                                               Enum.GetValues<PopDefinition.Field>()
                                                                   .Cast<Enum>()
                                                                   .ToArray(),
                                                               Enum.GetValues<PopDefinition.Field>()
                                                                   .Cast<Enum>()
                                                                   .ToArray());

   public NUISetting SoundTollSettings { get; set; } = new(SoundToll.Field.StraitLocationOne,
                                                           Enum.GetValues<SoundToll.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<SoundToll.Field>().Cast<Enum>().ToArray(),
                                                           Enum.GetValues<SoundToll.Field>().Cast<Enum>().ToArray());

   public NUISetting DefaultMapDefinitionSettings { get; set; } = new(DefaultMapDefinition.Field.UniqueId,
                                                                      Enum.GetValues<DefaultMapDefinition.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray(),
                                                                      Enum.GetValues<DefaultMapDefinition.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray(),
                                                                      Enum.GetValues<DefaultMapDefinition.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray());

   public NUISetting DynastySettings { get; set; } = new(Dynasty.Field.UniqueId,
                                                         Enum.GetValues<Dynasty.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<Dynasty.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<Dynasty.Field>().Cast<Enum>().ToArray());

   public NUISetting EstateSettings { get; set; } = new(Estate.Field.UniqueId,
                                                        Enum.GetValues<Estate.Field>().Cast<Enum>().ToArray(),
                                                        Enum.GetValues<Estate.Field>().Cast<Enum>().ToArray(),
                                                        Enum.GetValues<Estate.Field>().Cast<Enum>().ToArray());

   public NUISetting EstateAttributeDefinitionSettings { get; set; } = new(EstateAttributeDefinition.Field.UniqueId,
                                                                           Enum.GetValues<
                                                                                 EstateAttributeDefinition.Field>()
                                                                             .Cast<Enum>()
                                                                             .ToArray(),
                                                                           Enum.GetValues<
                                                                                 EstateAttributeDefinition.Field>()
                                                                             .Cast<Enum>()
                                                                             .ToArray(),
                                                                           Enum.GetValues<
                                                                                 EstateAttributeDefinition.Field>()
                                                                             .Cast<Enum>()
                                                                             .ToArray());

   public NUISetting EstateSatisfactionDefinitionSettings { get; set; } =
      new(EstateSatisfactionDefinition.Field.UniqueId,
          Enum.GetValues<EstateSatisfactionDefinition.Field>().Cast<Enum>().ToArray(),
          Enum.GetValues<EstateSatisfactionDefinition.Field>().Cast<Enum>().ToArray(),
          Enum.GetValues<EstateSatisfactionDefinition.Field>().Cast<Enum>().ToArray());

   public NUISetting ReligionSettings { get; set; } = new(Religion.Field.UniqueId,
                                                          Enum.GetValues<Religion.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<Religion.Field>().Cast<Enum>().ToArray(),
                                                          Enum.GetValues<Religion.Field>().Cast<Enum>().ToArray());

   public NUISetting ReligionGroupSettings { get; set; } = new(ReligionGroup.Field.UniqueId,
                                                               Enum.GetValues<ReligionGroup.Field>()
                                                                   .Cast<Enum>()
                                                                   .ToArray(),
                                                               Enum.GetValues<ReligionGroup.Field>()
                                                                   .Cast<Enum>()
                                                                   .ToArray(),
                                                               Enum.GetValues<ReligionGroup.Field>()
                                                                   .Cast<Enum>()
                                                                   .ToArray());

   public NUISetting Eu5ObjOpinionValueSettings { get; set; } = new(ReligionOpinionValue.Field.UniqueId,
                                                                    Enum.GetValues<ReligionOpinionValue.Field>()
                                                                        .Cast<Enum>()
                                                                        .ToArray(),
                                                                    Enum.GetValues<ReligionOpinionValue.Field>()
                                                                        .Cast<Enum>()
                                                                        .ToArray(),
                                                                    Enum.GetValues<ReligionOpinionValue.Field>()
                                                                        .Cast<Enum>()
                                                                        .ToArray());

   public NUISetting ReligiousFactionSettings { get; set; } = new(ReligiousFaction.Field.UniqueId,
                                                                  Enum.GetValues<ReligiousFaction.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray(),
                                                                  Enum.GetValues<ReligiousFaction.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray(),
                                                                  Enum.GetValues<ReligiousFaction.Field>()
                                                                      .Cast<Enum>()
                                                                      .ToArray());

   public NUISetting ReligiousFocusSettings { get; set; } = new(ReligiousFocus.Field.UniqueId,
                                                                Enum.GetValues<ReligiousFocus.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray(),
                                                                Enum.GetValues<ReligiousFocus.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray(),
                                                                Enum.GetValues<ReligiousFocus.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray());

   public NUISetting DesignateHeirReasonSettings { get; set; } = new(DesignateHeirReason.Field.UniqueId,
                                                                     Enum.GetValues<DesignateHeirReason.Field>()
                                                                         .Cast<Enum>()
                                                                         .ToArray(),
                                                                     Enum.GetValues<DesignateHeirReason.Field>()
                                                                         .Cast<Enum>()
                                                                         .ToArray(),
                                                                     Enum.GetValues<DesignateHeirReason.Field>()
                                                                         .Cast<Enum>()
                                                                         .ToArray());

   public NUISetting TraitSettings { get; set; } = new(Trait.Field.UniqueId,
                                                       Enum.GetValues<Trait.Field>().Cast<Enum>().ToArray(),
                                                       Enum.GetValues<Trait.Field>().Cast<Enum>().ToArray(),
                                                       Enum.GetValues<Trait.Field>().Cast<Enum>().ToArray());

   public NUISetting ParliamentTypeSettings { get; set; } = new(ParliamentType.Field.UniqueId,
                                                                Enum.GetValues<ParliamentType.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray(),
                                                                Enum.GetValues<ParliamentType.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray(),
                                                                Enum.GetValues<ParliamentType.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray());

   public NUISetting LocationTemplateDataSettings { get; set; } = new(LocationTemplateData.Field.UniqueId,
                                                                      Enum.GetValues<LocationTemplateData.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray(),
                                                                      Enum.GetValues<LocationTemplateData.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray(),
                                                                      Enum.GetValues<LocationTemplateData.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray());

   public NUISetting RawMaterialSettings { get; set; } = new(RawMaterial.Field.UniqueId,
                                                             Enum.GetValues<RawMaterial.Field>().Cast<Enum>().ToArray(),
                                                             Enum.GetValues<RawMaterial.Field>().Cast<Enum>().ToArray(),
                                                             Enum.GetValues<RawMaterial.Field>()
                                                                 .Cast<Enum>()
                                                                 .ToArray());

   public NUISetting DemandDataSettings { get; set; } = new(DemandData.Field.UniqueId,
                                                            Enum.GetValues<DemandData.Field>().Cast<Enum>().ToArray(),
                                                            Enum.GetValues<DemandData.Field>().Cast<Enum>().ToArray(),
                                                            Enum.GetValues<DemandData.Field>().Cast<Enum>().ToArray());

   public NUISetting MapMovementAssistSettings { get; set; } = new(MapMovementAssist.Field.UniqueId,
                                                                   Enum.GetValues<MapMovementAssist.Field>()
                                                                       .Cast<Enum>()
                                                                       .ToArray(),
                                                                   Enum.GetValues<MapMovementAssist.Field>()
                                                                       .Cast<Enum>()
                                                                       .ToArray(),
                                                                   Enum.GetValues<MapMovementAssist.Field>()
                                                                       .Cast<Enum>()
                                                                       .ToArray());

   public NUISetting StaticModifierSettings { get; set; } = new(StaticModifier.Field.UniqueId,
                                                                Enum.GetValues<StaticModifier.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray(),
                                                                Enum.GetValues<StaticModifier.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray(),
                                                                Enum.GetValues<StaticModifier.Field>()
                                                                    .Cast<Enum>()
                                                                    .ToArray());

   public NUISetting EstateCountDefinitonSettings { get; set; } = new(EstateCountDefinition.Field.UniqueId,
                                                                      Enum.GetValues<EstateCountDefinition.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray(),
                                                                      Enum.GetValues<EstateCountDefinition.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray(),
                                                                      Enum.GetValues<EstateCountDefinition.Field>()
                                                                          .Cast<Enum>()
                                                                          .ToArray());

   public NUISetting CultureGroupSettings { get; set; } = new(CultureGroup.Field.UniqueId,
                                                              Enum.GetValues<CultureGroup.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<CultureGroup.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray(),
                                                              Enum.GetValues<CultureGroup.Field>()
                                                                  .Cast<Enum>()
                                                                  .ToArray());
}