using System.Windows.Media.Imaging;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.NUI.Generator;
using ICommand = Arcanum.Core.CoreSystems.History.ICommand;

namespace Arcanum.UI.Components.Windows.MainWindows.MainWindowsHelpers;

public class CurrentActionVisualizer
{
   private readonly MainWindow _mainWindow;

   private const string UI_ASSEMBLY_NAME = "Arcanum_UI";
   private const string UI_PACK_PATH = $"pack://application:,,,/{UI_ASSEMBLY_NAME};component/";
   private const string ICONS_PATH = $"{UI_PACK_PATH}Assets/Icons/20x20/";
   private static readonly BitmapImage AddIcon = NEF.LoadBitmap($"{ICONS_PATH}ArrowRight20x20.png");
   private static readonly BitmapImage UpIcon = NEF.LoadBitmap($"{ICONS_PATH}ArrowUp20x20.png");
   private static readonly BitmapImage DownIcon = NEF.LoadBitmap($"{ICONS_PATH}ArrowDown20x20.png");

   public CurrentActionVisualizer(MainWindow mainWindow)
   {
      _mainWindow = mainWindow;

      AppData.HistoryManager.UndoEvent += OnUndo;
      AppData.HistoryManager.RedoEvent += OnRedo;
      AppData.HistoryManager.CommandAdded += OnAdded;
   }

   private void OnUndo(ICommand? obj)
   {
      _mainWindow.CurrentActionIcon = UpIcon;
      OnActionChanged(obj);
   }

   private void OnRedo(ICommand? obj)
   {
      _mainWindow.CurrentActionIcon = DownIcon;
      OnActionChanged(obj);
   }

   private void OnAdded(ICommand obj)
   {
      _mainWindow.CurrentActionIcon = AddIcon;
      OnActionChanged(obj);
   }

   private void OnActionChanged(ICommand? obj)
   {
      if (obj == null)
      {
         _mainWindow.CurrentActionText = "No action";
         return;
      }

      _mainWindow.CurrentActionText = obj.GetDescription;
   }
}