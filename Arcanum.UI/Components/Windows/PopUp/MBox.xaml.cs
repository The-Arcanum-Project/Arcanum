using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Arcanum.API.UI;

namespace Arcanum.UI.Components.Windows.PopUp;

public partial class MBox
{
   public MBoxResult Result { get; private set; }


   public MBox(string message, string title, MBoxButton buttons, MessageBoxImage icon)
   {
      InitializeComponent();

      Title = title;
      MessageText.Text = message;
      SetupButtons(buttons);
      SetupIcon(icon);
      
      Loaded += (_, _) =>
      {
         if (OkButton.Visibility == Visibility.Visible)
            Keyboard.Focus(OkButton);
         else if (CancelButton.Visibility == Visibility.Visible)
            Keyboard.Focus(CancelButton);
      };
   }

   private void SetupButtons(MBoxButton buttons)
   {
      OkButton.Visibility = Visibility.Collapsed;
      CancelButton.Visibility = Visibility.Collapsed;
      RetryButton.Visibility = Visibility.Collapsed;

      switch (buttons)
      {
         case MBoxButton.OK:
            OkButton.Visibility = Visibility.Visible;
            break;
         case MBoxButton.OKCancel:
            OkButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            break;
         case MBoxButton.OKRetryCancel:
            OkButton.Visibility = Visibility.Visible;
            RetryButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            break;
         case MBoxButton.OKRetry:
            OkButton.Visibility = Visibility.Visible;
            RetryButton.Visibility = Visibility.Visible;
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(buttons), buttons, null);
      }
   }

   private void SetupIcon(MessageBoxImage icon)
   {
      var iconKey = icon switch
      {
         MessageBoxImage.Information => "Info",
         MessageBoxImage.Warning => "Warning",
         MessageBoxImage.Error => "Error",
         MessageBoxImage.Question => "Help",
         _ => null,
      };

      if (iconKey != null)
      {
         var drawing = SystemIconsHelper.GetIcon(iconKey);
         if (drawing != null)
            IconImage.Source = drawing;
      }
   }

   private void OkButton_Click(object sender, RoutedEventArgs e)
   {
      Result = MBoxResult.OK;
      DialogResult = true;
   }

   private void RetryButton_Click(object sender, RoutedEventArgs e)
   {
      Result = MBoxResult.Retry;
      DialogResult = true;
   }

   private void CancelButton_Click(object sender, RoutedEventArgs e)
   {
      Result = MBoxResult.Cancel;
      DialogResult = true;
   }

   public static MBoxResult Show(string message,
                                   string title = "Message",
                                   MBoxButton buttons = MBoxButton.OK,
                                   MessageBoxImage icon = MessageBoxImage.None,
                                   int height = 150,
                                   int width = 400)
   {
      var box = new MBox(message, title, buttons, icon)
      {
         Height = height,
         Width = width,
         WindowStartupLocation = WindowStartupLocation.CenterScreen,
         ResizeMode = ResizeMode.NoResize,
         ShowInTaskbar = false,
         Topmost = true,
      };
      box.ShowDialog();
      return box.Result;
   }
}

public static class SystemIconsHelper
{
   public static ImageSource? GetIcon(string name) => name switch
   {
      "Info" => Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Information.Handle,
                                                    Int32Rect.Empty,
                                                    BitmapSizeOptions.FromEmptyOptions()),
      "Warning" => Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Warning.Handle,
                                                       Int32Rect.Empty,
                                                       BitmapSizeOptions.FromEmptyOptions()),
      "Error" => Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle,
                                                     Int32Rect.Empty,
                                                     BitmapSizeOptions.FromEmptyOptions()),
      "Help" => Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Question.Handle,
                                                    Int32Rect.Empty,
                                                    BitmapSizeOptions.FromEmptyOptions()),
      _ => null,
   };
}