using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Arcanum.API.Console;
using Arcanum.Core.CoreSystems.ConsoleServices;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public enum ArgDataType
{
   String,
   Int,
   Float,
   Bool,
   RemainingText,
   Flag,
}

public class ArgumentDefinition : INotifyPropertyChanged
{
   public string Name
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = "arg";

   public ArgDataType DataType
   {
      get;
      set
      {
         field = value;
         if (field == ArgDataType.Flag)
            IsFixedPosition = false;
         OnPropertyChanged();
      }
   } = ArgDataType.String;

   public bool IsRequired
   {
      get;
      set
      {
         field = value;
         if (!field)
            IsFixedPosition = false;
         OnPropertyChanged();
      }
   } = true;

   public bool IsFixedPosition
   {
      get;
      set
      {
         if (value && !IsRequired)
            return;

         if (value && DataType == ArgDataType.Flag)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = true;

   public string Description
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = "";

   public event PropertyChangedEventHandler? PropertyChanged;
   private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

public class MainViewModel : INotifyPropertyChanged
{
   // Properties
   public string Name
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         Generate();
      }
   } = "name";
   public string AliasesRaw
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         Generate();
      }
   } = "";
   public string Description
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         Generate();
      }
   } = "description";
   public ClearanceLevel SelectedClearance
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         Generate();
      }
   } = ClearanceLevel.User;
   public DefaultCommands.CommandCategory SelectedCategory
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         Generate();
      }
   } = DefaultCommands.CommandCategory.StandardUser;

   public ObservableCollection<ArgumentDefinition> Arguments { get; } = [];

   public string GeneratedCode
   {
      get;
      private set
      {
         field = value;
         OnPropertyChanged();
      }
   } = "";

   // Static sources
   public static ArgDataType[] ArgTypes => Enum.GetValues<ArgDataType>();
   public static ClearanceLevel[] ClearanceLevels => Enum.GetValues<ClearanceLevel>();
   public static DefaultCommands.CommandCategory[] Categories => Enum.GetValues<DefaultCommands.CommandCategory>()
                                                                     .Where(x => x != DefaultCommands.CommandCategory.All &&
                                                                                 x != DefaultCommands.CommandCategory.None)
                                                                     .ToArray();

   // Commands
   public ICommand CopyCommand { get; }
   public ICommand ExportCommand { get; }
   public ICommand RemoveArgumentCommand { get; }
   public ICommand AddArgumentCommand { get; }

   public MainViewModel()
   {
      Arguments.CollectionChanged += Arguments_CollectionChanged;

      // Add a default argument
      Arguments.Add(new() { Name = "target", DataType = ArgDataType.String });

      CopyCommand = new RelayCommand(_ => Clipboard.SetText(GeneratedCode));
      ExportCommand = new RelayCommand(_ => Generate());
      RemoveArgumentCommand = new RelayCommand(_ =>
      {
         if (Arguments.Count > 0)
            Arguments.RemoveAt(Arguments.Count - 1);
      });
      AddArgumentCommand = new RelayCommand(_ => Arguments.Add(new()));

      Generate();
   }

   private void Arguments_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
   {
      if (e.NewItems != null)
      {
         foreach (ArgumentDefinition item in e.NewItems)
            item.PropertyChanged += Argument_PropertyChanged;
      }

      if (e.OldItems != null)
      {
         foreach (ArgumentDefinition item in e.OldItems)
            item.PropertyChanged -= Argument_PropertyChanged;
      }

      Generate();
   }

   private void Argument_PropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      Generate();
   }

   private void Generate()
   {
      var pascalName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Name);
      var aliasesList = string.IsNullOrWhiteSpace(AliasesRaw)
                           ? "[]"
                           : $"[{string.Join(", ", AliasesRaw.Split(',').Select(x => $"\"{x.Trim()}\""))}]";

      var usageBuilder = new StringBuilder();
      usageBuilder.Append(Name);

      foreach (var arg in Arguments)
      {
         usageBuilder.Append(" ");
         if (arg.DataType == ArgDataType.Flag)
            usageBuilder.Append($"[-{arg.Name}]");
         else if (arg.IsRequired)
            usageBuilder.Append($"<{arg.Name}>");
         else
            usageBuilder.Append($"[{arg.Name}]");
      }

      usageBuilder.Append($" | {Description}");

      if (Arguments.Any(a => !string.IsNullOrWhiteSpace(a.Description)))
      {
         foreach (var arg in Arguments.Where(a => !string.IsNullOrWhiteSpace(a.Description)))
         {
            var prefix = arg.DataType == ArgDataType.Flag ? $"-{arg.Name}" : arg.Name;
            usageBuilder.Append($" {prefix}: {arg.Description}.");
         }
      }

      var parsingLogic = new StringBuilder();

      var fixedArgs = Arguments.Where(x => x.IsFixedPosition).ToList();

      if (fixedArgs.Count > 0)
      {
         parsingLogic.AppendLine($"""
                                              if (args.Length < {fixedArgs.Count})
                                                  return ["Usage: " + usage];
                                  """);
         parsingLogic.AppendLine();
      }

      var fixedIndex = 0;

      foreach (var arg in Arguments)
      {
         var varName = arg.Name.Replace(" ", "_").Replace("-", "").ToLower();
         const string indent = "            ";

         if (arg.DataType == ArgDataType.Flag)
         {
            parsingLogic.AppendLine($"{indent}      // Flag: -{arg.Name}");
            parsingLogic.AppendLine($"{indent}      var {varName} = args.Contains(\"-{arg.Name}\", StringComparer.OrdinalIgnoreCase);");
            parsingLogic.AppendLine($"{indent}      if ({varName})");
            parsingLogic.AppendLine($"{indent}      {{");
            parsingLogic.AppendLine($"{indent}          // TODO: Implement flag logic for '-{arg.Name}'");
            parsingLogic.AppendLine($"{indent}      }}");
            parsingLogic.AppendLine();
            continue;
         }

         if (arg.IsFixedPosition)
         {
            var indexAccess = $"args[{fixedIndex}]";

            GenerateTypeParsing(parsingLogic, indent, arg, indexAccess, varName, isFixed: true, fixedIndex);

            if (arg.DataType != ArgDataType.RemainingText)
               fixedIndex++;
            continue;
         }

         parsingLogic.AppendLine($"{indent}// Optional/Dynamic: {arg.Name}");
         parsingLogic.AppendLine($"{indent}{GetCSharpType(arg.DataType)} {varName} = default;");
         parsingLogic.AppendLine($"{indent}if (args.Length > {fixedIndex})");
         parsingLogic.AppendLine($"{indent}{{");

         const string innerIndent = "                ";
         var optIndexAccess = $"args[{fixedIndex}]";

         parsingLogic.AppendLine($"{innerIndent}if (!args[{fixedIndex}].StartsWith('-'))");
         parsingLogic.AppendLine($"{innerIndent}{{");

         GenerateTypeParsing(parsingLogic, innerIndent + "    ", arg, optIndexAccess, varName, isFixed: false, fixedIndex);

         parsingLogic.AppendLine($"{innerIndent}}}");
         parsingLogic.AppendLine($"{indent}}}");

         if (arg.DataType != ArgDataType.RemainingText)
            fixedIndex++;

         parsingLogic.AppendLine();
      }

      GeneratedCode = $$"""
                        private static DefaultCommands.DefaultCommandDefinition Create{{pascalName}}Command()
                        {
                            const string usage = "{{usageBuilder}}";

                            return new(name: "{{Name.ToLower()}}",
                                       usage: usage,
                                       execute: args =>
                                       {
                        {{parsingLogic}}
                                           // TODO: Implement Logic
                                           return ["Success."];
                                       },
                                       clearance: ClearanceLevel.{{SelectedClearance}},
                                       category: DefaultCommands.CommandCategory.{{SelectedCategory}},
                                       aliases: {{aliasesList}});
                        }
                        """;
   }

   private static void GenerateTypeParsing(StringBuilder sb,
                                           string indent,
                                           ArgumentDefinition arg,
                                           string indexAccess,
                                           string varName,
                                           bool isFixed,
                                           int fixedIndex)
   {
      switch (arg.DataType)
      {
         case ArgDataType.String:
            sb.AppendLine(isFixed ? $"{indent}var {varName} = {indexAccess};" : $"{indent}{varName} = {indexAccess};");
            break;
         case ArgDataType.Int:
            sb.AppendLine($"{indent}if (!int.TryParse({indexAccess}, out int {(isFixed ? varName : "temp_" + varName)}))");
            sb.AppendLine($"{indent}    return [\"Error: Argument '{arg.Name}' must be an integer.\"];");
            if (!isFixed)
               sb.AppendLine($"{indent}{varName} = temp_{varName};");
            break;
         case ArgDataType.Float:
            sb.AppendLine($"{indent}if (!float.TryParse({indexAccess}, out float {(isFixed ? varName : "temp_" + varName)}))");
            sb.AppendLine($"{indent}    return [\"Error: Argument '{arg.Name}' must be a number.\"];");
            if (!isFixed)
               sb.AppendLine($"{indent}{varName} = temp_{varName};");
            break;
         case ArgDataType.Bool:
            sb.AppendLine($"{indent}if (!bool.TryParse({indexAccess}, out bool {(isFixed ? varName : "temp_" + varName)}))");
            sb.AppendLine($"{indent}    return [\"Error: Argument '{arg.Name}' must be true/false.\"];");
            if (!isFixed)
               sb.AppendLine($"{indent}{varName} = temp_{varName};");
            break;
         case ArgDataType.RemainingText:
            sb.AppendLine(isFixed
                             ? $"{indent}var {varName} = string.Join(\" \", args[{fixedIndex}..]);"
                             : $"{indent}{varName} = string.Join(\" \", args[{fixedIndex}..]);");
            break;
      }
   }

   private static string GetCSharpType(ArgDataType type) => type switch
   {
      ArgDataType.Int => "int",
      ArgDataType.Float => "float",
      ArgDataType.Bool => "bool",
      _ => "string",
   };

   public event PropertyChangedEventHandler? PropertyChanged;
   private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

public class RelayCommand(Action<object?> execute) : ICommand
{
   public event EventHandler? CanExecuteChanged;
   public bool CanExecute(object? parameter) => true;
   public void Execute(object? parameter) => execute(parameter);
}

public partial class CommandGenerator
{
   public CommandGenerator()
   {
      InitializeComponent();
      DataContext = new MainViewModel();
   }
}