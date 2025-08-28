using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

// Requires regions, areas, provinces, locations, location_ranks, climate, topography, vacation, coastal, river to be loaded first
// public class DevelopmentParsing : FileLoadingService
// {
//    public override List<Type> ParsedObjects { get; }
//    public override string GetFileDataDebugInfo() => throw new NotImplementedException();
//
//    public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null) => throw new NotImplementedException();
//
//    public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor) => throw new NotImplementedException();
// }