using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.SavingSystem.Services;

public interface ISavingService
{
   public static ISavingService Dummy { get; } = new DummySavingService();

   public class DummySavingService : ISavingService
   {
      public void AddSavingComment(IndentedStringBuilder sb, Eu5FileObj context)
      {
         throw new NotImplementedException();
      }

      public void AddHeader(IndentedStringBuilder sb, Eu5FileObj context)
      {
         throw new NotImplementedException();
      }

      public void AddFooter(IndentedStringBuilder sb, Eu5FileObj context)
      {
         throw new NotImplementedException();
      }

      public string GetSavePrompt(Eu5FileObj context)
      {
         throw new NotImplementedException();
      }
   }

   public void AddSavingComment(IndentedStringBuilder sb, Eu5FileObj context);
   public void AddHeader(IndentedStringBuilder sb, Eu5FileObj context);
   public void AddFooter(IndentedStringBuilder sb, Eu5FileObj context);
   public string GetSavePrompt(Eu5FileObj context);
}