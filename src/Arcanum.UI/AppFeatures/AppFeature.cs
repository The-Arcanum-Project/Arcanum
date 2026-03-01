using Arcanum.Core.Utils;

namespace Arcanum.UI.AppFeatures;

public record AppFeature(FeatureId Id,
                         string DisplayName,
                         string Description,
                         FeatureCategory Category,
                         FeatureLevel Level,
                         FeatureId? ParentFeatureId,
                         FeatureLocation Location,
                         FeatureScale Scale,
                         IEnumerable<string> AssociatedScopes,
                         IEnumerable<string> SearchSynonyms,
                         IEnumerable<FeatureNote> QuickPoints,
                         IEnumerable<ExternalReference>? Links,
                         VersionNumber IntroducedIn,
                         FeatureStatus Status,
                         string? IconPath) : IAppFeature;