using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.API.Console;
using Arcanum.Core.CoreSystems.ConsoleServices;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class ConsoleWindow : IOutputReceiver
{
   private readonly IConsoleService _handler;

   public ConsoleWindow(IConsoleService handler)
   {
      _handler = handler;
      if (!_handler.HasOutputReceiver())
         handler.SetOutputReciever(this);
      InitializeComponent();

      ConsoleInputTextBox.Text = ConsoleServiceImpl.CMD_PREFIX;
      ConsoleInputTextBox.SelectionStart = ConsoleInputTextBox.Text.Length;
      ConsoleInputTextBox.SelectionLength = 0;
      
      ConsoleInputTextBox.Focus();
   }
   

   private void ConsoleInputTextBox_KeyDown(object sender, KeyEventArgs e)
   {
      if (e.Key == Key.Enter)
      {
         _handler.ProcessCommand(ConsoleInputTextBox.Text);
         ClearInput();
         e.Handled = true;
      }
      else if (e.Key == Key.Back)
      {
         if (ConsoleInputTextBox.Text.Length ==
             ConsoleServiceImpl.CMD_PREFIX_LENGTH) // we don't allow the user to delete the CMD_PREFIX chars
         {
            e.Handled = true;
         }
      }
      else if (e.Key == Key.Up)
      {
         if (_handler.HistoryIndex >= 0)
            ConsoleInputTextBox.Text = ConsoleServiceImpl.CMD_PREFIX + _handler.GetPreviousHistoryEntry();
         e.Handled = true;
         ConsoleInputTextBox.SelectionStart = ConsoleInputTextBox.Text.Length;
      }
      else if (e.Key == Key.Down)
      {
         if (_handler.HistoryIndex > _handler.GetHistory().Count - 1)
            ConsoleInputTextBox.Text = ConsoleServiceImpl.CMD_PREFIX;
         else
            ConsoleInputTextBox.Text = ConsoleServiceImpl.CMD_PREFIX + _handler.GetNextHistoryEntry();
         e.Handled = true;
         ConsoleInputTextBox.SelectionStart = ConsoleInputTextBox.Text.Length;
      }
   }

   private void ConsoleInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      if (ConsoleInputTextBox.Text.Length < ConsoleServiceImpl.CMD_PREFIX_LENGTH ||
          ConsoleInputTextBox.Text.Length == ConsoleServiceImpl.CMD_PREFIX_LENGTH &&
          !ConsoleInputTextBox.Text.Equals(ConsoleServiceImpl.CMD_PREFIX))
      {
         ConsoleInputTextBox.Text = ConsoleServiceImpl.CMD_PREFIX;
         ConsoleInputTextBox.SelectionLength = 0;
         ConsoleInputTextBox.SelectionStart = ConsoleInputTextBox.Text.Length;
      }

      if (!ConsoleInputTextBox.Text.StartsWith(ConsoleServiceImpl.CMD_PREFIX))
      {
         ConsoleInputTextBox.Text = ConsoleServiceImpl.CMD_PREFIX + ConsoleInputTextBox.Text;
         ConsoleInputTextBox.SelectionLength = 0;
         ConsoleInputTextBox.SelectionStart = ConsoleInputTextBox.Text.Length;
      }
   }

   public void ClearInput()
   {
      ConsoleInputTextBox.Clear();
      ConsoleInputTextBox.Text = ConsoleServiceImpl.CMD_PREFIX;
      ConsoleInputTextBox.SelectionStart = ConsoleInputTextBox.Text.Length;
      ConsoleInputTextBox.SelectionLength = 0;
      ConsoleInputTextBox.Focus();
   }

   public void WriteLine(string message, bool scrollToCaret, bool prefix)
   {
      if (string.IsNullOrWhiteSpace(message))
         return;

      if (ConsoleOutputTextBox.Text.Length > 0 && !ConsoleOutputTextBox.Text.EndsWith(Environment.NewLine))
         ConsoleOutputTextBox.Text += Environment.NewLine;

      ConsoleOutputTextBox.Text += (prefix ? ConsoleServiceImpl.CMD_PREFIX : string.Empty) + message;
      ConsoleOutputTextBox.SelectionStart = ConsoleOutputTextBox.Text.Length;
      ConsoleOutputTextBox.SelectionLength = 0;

      if (scrollToCaret)
         ConsoleOutputTextBox.ScrollToEnd();
   }

   public void WriteLines(List<string> messages)
   {
      if (messages.Count == 0)
         return;

      for (var i = 0; i < messages.Count; i++)
         WriteLine(messages[i], false, i == 0);

      ConsoleOutputTextBox.ScrollToEnd();
   }

   public void WriteError(string message)
   {
      if (string.IsNullOrWhiteSpace(message))
         return;

      if (ConsoleOutputTextBox.Text.Length > 0 && !ConsoleOutputTextBox.Text.EndsWith(Environment.NewLine))
         ConsoleOutputTextBox.Text += Environment.NewLine;

      ConsoleOutputTextBox.Text += ConsoleServiceImpl.CMD_PREFIX + "Error: " + message + Environment.NewLine;
      ConsoleOutputTextBox.SelectionStart = ConsoleOutputTextBox.Text.Length;
      ConsoleOutputTextBox.SelectionLength = 0;
      ConsoleOutputTextBox.ScrollToEnd();
   }

   public void Clear()
   {
      ConsoleOutputTextBox.Text = string.Empty;
      ConsoleOutputTextBox.SelectionStart = 0;
      ConsoleOutputTextBox.SelectionLength = 0;
   }
}