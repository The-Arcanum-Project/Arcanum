namespace BuildTimeCodeGeneration;

public class Program
{
   public static int Main(string[] args)
   {
      // TODO: @Minnator reenable once we want to start generating code again for effects / triggers
      return 0;
      //
      // if (args.Length != 6)
      // {
      //    Console.Error.WriteLine("Usage: CodeGenerator <input_txt_path> <output_cs_path>");
      //    return 1;
      // }
      //
      // try
      // {
      //    if (!File.Exists(args[1]))
      //    {
      //       var definitions = DocumentationParser.ParseScopesDocumentationFile(args[0]);
      //       FileGenerator.GenerateScopesFile(definitions, args[1]);
      //    }
      //
      //    if (!File.Exists(args[3]))
      //    {
      //       var triggers = DocumentationParser.ParseDocs(args[2]);
      //       FileGenerator.GenerateEffectsTriggerFile(triggers, args[3], "Triggers");
      //    }
      //
      //    if (!File.Exists(args[5]))
      //    {
      //       var effects = DocumentationParser.ParseDocs(args[4]);
      //       FileGenerator.GenerateEffectsTriggerFile(effects, args[5], "Effects");
      //    }
      //
      //    return 0;
      // }
      // catch (Exception ex)
      // {
      //    Console.Error.WriteLine($"ERROR: {ex.Message}");
      //    return 1;
      // }
   }
}