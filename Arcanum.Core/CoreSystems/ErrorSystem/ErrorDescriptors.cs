using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;

namespace Arcanum.Core.CoreSystems.ErrorSystem;

public static class QADesc
{
   public static readonly MiscellaneousError Misc = MiscellaneousError.Instance;
   public static readonly ParsingError Parse = ParsingError.Instance;
}

public class ErrorDescriptors
{
   private static readonly Lazy<ErrorDescriptors> LazyInstance = new(() => new ());

   public static ErrorDescriptors Instance => LazyInstance.Value;
   private ErrorDescriptors()
   {
   }
   public MiscellaneousError Misc {get;} = MiscellaneousError.Instance;
   public ParsingError Parse {get;} = ParsingError.Instance;
}
