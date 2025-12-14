using System.IO;
using System.Text;
using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Setup;

public abstract class SetupFileWriter
{
   public readonly Type[] ContainedTypes;
   public readonly string FileName;
   public virtual Encoding FileEncoding => new UTF8Encoding(true);

   public string FullPath => Path.Combine(FileManager.ModDataSpace.FullPath, "main_menu", "setup", "start", FileName);

   protected SetupFileWriter(Type[] containedTypes, string fileName)
   {
      ContainedTypes = containedTypes;
      FileName = fileName;
   }

   public abstract IndentedStringBuilder WriteFile();
}