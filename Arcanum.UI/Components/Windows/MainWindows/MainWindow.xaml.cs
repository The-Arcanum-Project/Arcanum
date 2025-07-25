using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Arcanum.Core.CoreSystems.ConsoleServices;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.Parsing.DocsParsing;
using Arcanum.Core.CoreSystems.Parsing.MapParsing;
using Arcanum.Core.CoreSystems.Parsing.ModifierParsing;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.FlowControlServices;
using Arcanum.Core.OldAndDebug;
using Arcanum.Core.Utils;
using Arcanum.UI.Components.Views.MainWindow;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.HostUIServices.SettingsGUI;
using Application = System.Windows.Application;

namespace Arcanum.UI.Components.Windows.MainWindows;

public partial class MainWindow : IPerformanceMeasured, INotifyPropertyChanged
{
   public const int DEFAULT_WIDTH = 1920;
   public const int DEFAULT_HEIGHT = 1080;

   private readonly MainWindowView _view;
   private string _ramUsage = "RAM: [0 MB]";
   private string _cpuUsage = "CPU: [0%]";

   #region Properties

   public string RamUsage
   {
      get => _ramUsage;
      private set
      {
         if (value == _ramUsage)
            return;

         _ramUsage = value;
         OnPropertyChanged();
      }
   }
   public string CpuUsage
   {
      get => _cpuUsage;
      private set
      {
         if (value == _cpuUsage)
            return;

         _cpuUsage = value;
         OnPropertyChanged();
      }
   }

   #endregion

   public MainWindow()
   {
      InitializeComponent();
      PerformanceCountersHelper.Initialize(this);

      _view = DataContext as MainWindowView ??
              throw new InvalidOperationException("DataContext is not set or is not of type MainWindowView.");
   }

   public void GoToArcanumMenuScreenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
   {
      var mw = new MainMenuScreen { MainMenuViewModel = { TargetedView = MainMenuScreen.MainMenuScreenView.Arcanum } };
      Application.Current.MainWindow = mw;
      Application.Current.MainWindow.Show();
      mw.Activate();
      Close();
   }

   private void CommandCanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

   private void ExitArcanum_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      LifecycleManager.Instance.RunShutdownSequence();
      Application.Current.Shutdown();
   }

   private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
   {
      var interop = new WindowInteropHelper(this);
      var screen = Screen.FromHandle(interop.Handle);
      if (screen.Bounds.Height <= DEFAULT_HEIGHT || screen.Bounds.Width <= DEFAULT_WIDTH)
      {
         Height = screen.WorkingArea.Height * 0.8;
         Width = screen.WorkingArea.Width * 0.8;
         WindowState = WindowState.Maximized;
      }
   }

   private void OpenPluginSettingsWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var settingsWindow = new PluginSettingsWindow();
      settingsWindow.ShowDialog();
   }

   private void OpenSettingsWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      SettingsWindow.ShowSettingsWindow();
   }

   private void SaveAllModified_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      // TODO @MelCo link this to the actual save logic
   }

   private void OpenSaveSelector_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      // TODO @MelCo link this to the actual save selector logic
   }

   private void OpenReloadFileWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      // TODO @MelCo link this to the actual reload file logic
   }

   private void OpenReloadFolderWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      // TODO @MelCo link this to the actual reload folder logic
   }

   public void SetCpuUsage(string cpuUsage)
   {
      if (string.Equals(cpuUsage, CpuUsage, StringComparison.Ordinal))
         return;

      CpuUsage = cpuUsage;
   }

   public void SetMemoryUsage(string memoryUsage)
   {
      if (string.Equals(memoryUsage, RamUsage, StringComparison.Ordinal))
         return;

      RamUsage = memoryUsage;
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }

   private void OpenConsoleCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var consoleWindow = new ConsoleWindow(new ConsoleServiceImpl(LifecycleManager.Instance.PluginManager.Host,
                                                                   "DebugConsole",
                                                                   category: DefaultCommands.CommandCategory.All));
      consoleWindow.Show();
   }

   private void DebugParsingCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      const string path = "C:\\Users\\david\\Dokumente\\Paradox Interactive\\Europa Universalis V\\docs\\triggers.log";
      const string effPath = "C:\\Users\\david\\Dokumente\\Paradox Interactive\\Europa Universalis V\\docs\\effects.log";
      var docsTriggers = DocsParsing.ParseDocs(path);
      var sb = new StringBuilder();
      sb.AppendLine(DocsObj.GetCsvHeader());
      foreach (var trigger in docsTriggers)
         sb.AppendLine(trigger.ToCsv());
      
      File.WriteAllText("EU5_Triggers_Data.csv", sb.ToString());

      var docsEffects = DocsParsing.ParseDocs(effPath);
      sb.Clear();
      sb.AppendLine(DocsObj.GetCsvHeader());
      foreach (var effect in docsEffects)
         sb.AppendLine(effect.ToCsv());
      
      File.WriteAllText("EU5_Effects_Data.csv", sb.ToString());
      
      
      
      // var modifiers = ParseModifiers.Load();
      //
      // var sb = new StringBuilder();
      // foreach (var modifier in modifiers)
      //    sb.AppendLine(modifier.ToString());
      //
      // File.WriteAllText("EU5_Modifiers_Data.txt", sb.ToString());
      //
      // sb.Clear();
      // sb.AppendLine(ModifierDefinition.GetCsvHeader());
      // foreach (var modifier in modifiers)
      //    sb.AppendLine(modifier.ToCsv());
      //
      // File.WriteAllText("EU5_Modifiers_Data_CSV.csv", sb.ToString());

      // var regex = new Regex(@"(\w+)\s*=\s*{");
      // var filePath =
      //    "S:\\SteamLibrary\\steamapps\\common\\Project Caesar Review\\game\\main_menu\\common\\modifier_icons\\00_modifier_icons.txt";
      // var content = IO.ReadAllTextUtf8(filePath);
      // var matches = regex.Matches(content);
      // var sb = new StringBuilder();
      //
      // var strs = matches.Select(match => match.Groups[1].Value).ToList();
      // strs.Sort();
      //
      // foreach (var str in strs)
      //    sb.AppendLine(str);

      //File.WriteAllText("EU5_Modifiers_Data.txt", sb.ToString());

      NamedLocationLoading.LoadNamedLocations();
      
      var (provToId, colorToBorder) = OldMapLoading.LoadLocations();
      var provIds = new List<int>(provToId.Count);
      var index = 0;
      
      foreach (var kvp in provToId)
         provIds.Add(kvp.Value.Count);
      
      provIds.Sort();
      var sbb = new StringBuilder();
      foreach (var provicne in provIds)
         sbb.AppendLine(provicne.ToString());
      
      File.WriteAllText("EU5_Provinces_Pixel_Count.txt", sbb.ToString());

      // List< (Memory<System.Drawing.Point>, int)> provs = new (provToId.Count);
      //
      // foreach (var (color, points) in provToId)
      //    provs.provs.Add((new(points.ToArray()), color));
      //
      // var debugBmp = new Bitmap(16384, 8192);
      // foreach (var kvp in provs)
      //    MapDrawing.DrawPixelsParallel(kvp.Item1, kvp.Item2, debugBmp);
      //
      // debugBmp.Save("debugMap.png");
      
      // var sw = new Stopwatch();
      // sw.Start();
      // var totalPixels = colorToBorder.Values.Sum(points => points.Values.Sum(section => section.Count));
      // List<(Memory<System.Drawing.Point>, int)> borders = new(colorToBorder.Count);
      // foreach (var (color, points) in colorToBorder)
      // {
      //    List<System.Drawing.Point> borderSections = [];
      //    foreach (var borderSection in points.Values)
      //       borderSections.AddRange(borderSection);
      //    borders.Add((new(borderSections.ToArray()), color));
      // }
      //
      // var debugBmp = new Bitmap(16384, 8192);
      // var graphics = Graphics.FromImage(debugBmp);
      // graphics.Clear(Color.FromArgb(255, 42, 42, 42)); // Set a background color for better visibility
      // var bmpData = debugBmp.LockBits(new (0, 0, debugBmp.Width, debugBmp.Height), 
      //    System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      // foreach (var kvp in borders)
      //    MapDrawing.DrawPixelsParallel(kvp.Item1, kvp.Item2, bmpData);
      // debugBmp.UnlockBits(bmpData);
      //
      // sw.Stop();
      // Debug.WriteLine($"Drawing took {sw.ElapsedMilliseconds} ms for {borders.Count} borders wit {totalPixels} points");
      // debugBmp.Save("debugBorderMap.png");
      //
      // Debug.WriteLine("Loaded " + provToId.Count + " provinces and " + colorToBorder.Count + " borders from old map files.");
   }

   private void OpenEffectWikiCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      DocsObjBrowser.ShowDocsObjBrowser(DocsObjBrowser.DocsObjBrowserType.Effects).ShowDialog();
   }

   private void OpenModifierWikiCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      ModifierBrowser.ShowModifierBrowser();
   }

   private void OpenTriggerWikiCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      DocsObjBrowser.ShowDocsObjBrowser(DocsObjBrowser.DocsObjBrowserType.Triggers).ShowDialog();
   }
}