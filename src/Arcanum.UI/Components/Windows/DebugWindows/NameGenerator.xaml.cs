using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Arcanum.Core.CoreSystems.NUI;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class NameGenerator : INotifyPropertyChanged
{
   public static readonly DependencyProperty LanguagesProperty = DependencyProperty.Register(nameof(Languages),
                                                                                             typeof(ObservableRangeCollection<
                                                                                                Core.Utils.NameGenerator.NameGenerator.Language>),
                                                                                             typeof(NameGenerator),
                                                                                             new PropertyMetadata(default(ObservableRangeCollection<
                                                                                                       Core.Utils.NameGenerator.NameGenerator.Language>)));

   public NameGenerator()
   {
      InitializeComponent();
      DataContext = this;
      Core.Utils.NameGenerator.NameGenerator.LoadLanguages();
      Languages = new(Core.Utils.NameGenerator.NameGenerator.Languages);
      SelectedLanguage = Languages.FirstOrDefault()!;

      GenerateNameCommand = new RelayCommand(_ =>
      {
         var sb = new StringBuilder();
         for (var i = 0; i < 20; i++)
            sb.AppendLine(SelectedLanguage.GenerateWord(4));
         GenerationOutput = sb.ToString();
      });
   }

   private static double RandomDouble(double min, double max) => Random.Shared.NextDouble() * (max - min) + min;

   public RelayCommand GenerateNameCommand { get; }
   public string GenerationOutput
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = string.Empty;
   public Core.Utils.NameGenerator.NameGenerator.Language SelectedLanguage
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   }
   public ObservableRangeCollection<Core.Utils.NameGenerator.NameGenerator.Language> Languages
   {
      get => (ObservableRangeCollection<Core.Utils.NameGenerator.NameGenerator.Language>)GetValue(LanguagesProperty);
      set => SetValue(LanguagesProperty, value);
   }
   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }
}