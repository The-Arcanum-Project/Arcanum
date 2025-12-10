using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Clipboard;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class ClipboardHistory
{
   public ObservableCollection<ClipboardEntry> HistoryItems { get; set; } = [];
   private bool _isClosing;

   public ClipboardHistory()
   {
      InitializeComponent();

      // Initialize Data (Replace this with your actual Clipboard Manager source)
      foreach (var item in ArcClipboard.GetHistory())
      {
         var typeName = item.Value.GetType().Name;
         var propName = item.Property?.ToString() ?? "<Entire Object>";
         var valueStr = item.Value.ToString() ?? "null";
         HistoryItems.Add(new(typeName, propName, valueStr));
      }

      HistoryList.ItemsSource = HistoryItems;

      Deactivated += (_, _) => SafeClose();

      PreviewKeyDown += (_, e) =>
      {
         if (e.Key == Key.Escape)
            SafeClose();
      };

      SetPositionToMouse();
   }

   private void SafeClose()
   {
      if (_isClosing)
         return;

      _isClosing = true;
      Close();
   }

   private void SetPositionToMouse()
   {
      // Get Mouse Position (Win32 Native is most reliable for screen coordinates)
      GetCursorPos(out var lMousePosition);

      // Adjust for DPI scaling if necessary, usually WPF handles units reasonably well
      // but for exact pixel mapping on the screen edge, simple assignment works for popups.

      // Offset slightly so it doesn't appear exactly under the click
      Left = lMousePosition.X;
      Top = lMousePosition.Y;

      // Ensure we don't spawn off-screen (Right/Bottom Edge Check)
      var screenWidth = SystemParameters.PrimaryScreenWidth;
      var screenHeight = SystemParameters.PrimaryScreenHeight;

      if (Left + Width > screenWidth)
         Left = lMousePosition.X - Width; // Flip to left

      if (Top + Height > screenHeight)
         Top = lMousePosition.Y - Height; // Flip up
   }

   private void HistoryList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
   {
      // Handle Selection
      if (HistoryList.SelectedItem is ClipboardEntry item)
      {
         // TODO: Logic to Paste 'item.Value' goes here
         // e.g., Clipboard.SetText(item.Value);

         Console.WriteLine($"Selected: {item.Value}");
         Close();
      }
   }

   // --- PInvoke for Mouse Position ---
   [StructLayout(LayoutKind.Sequential)]
   // ReSharper disable once InconsistentNaming
   public struct POINT
   {
      public int X;
      public int Y;
   }

   [LibraryImport("user32.dll")]
   [return: MarshalAs(UnmanagedType.Bool)]
   private static partial bool GetCursorPos(out POINT lpPoint);
}

// Data Class for the List
public sealed class ClipboardEntry(string type, string prop, string val)
{
   public string ObjectType { get; } = type;
   public string PropertyName { get; } = prop;
   public string Value { get; } = val;
}