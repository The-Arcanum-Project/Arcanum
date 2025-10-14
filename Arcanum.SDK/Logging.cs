namespace Arcanum.API;

public enum LoggingVerbosity
{
   Info = 0, // [Inf] Default verbosity, used for general information
   Warning = 1, // [War] Used for warnings that do not require immediate attention
   Error = 2, // [Err] Used for errors that need to be addressed
}