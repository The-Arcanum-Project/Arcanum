using System.Text.Json.Serialization;
using Arcanum.API.Attributes;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.IO.JsonConverters;

namespace Arcanum.Core.CoreSystems.ErrorSystem;


public class ErrorDescriptors
{
#pragma warning disable CS0618 // Type or member is obsolete
   private static readonly Lazy<ErrorDescriptors> LazyInstance = new(() => new ());
#pragma warning restore CS0618 // Type or member is obsolete

   public static ErrorDescriptors Instance => LazyInstance.Value;
   
   [Obsolete("Marked as obsolete to prevent direct instantiation. Use Instance property instead.")]
   public ErrorDescriptors()
   {
   }
   [SettingsForceInlinePropertyGrid]
   public MiscellaneousError Misc {get; set; } = MiscellaneousError.Instance;
   [SettingsForceInlinePropertyGrid]
   public ParsingError Parse {get; set; } = ParsingError.Instance;
}
