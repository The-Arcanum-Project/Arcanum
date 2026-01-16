using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Pops;
using Arcanum.Core.GameObjects.InGame.Religious;
using Arcanum.Core.GlobalStates;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class TemplateToPopsWindow : INotifyPropertyChanged
{
   public TemplateToPopsWindow()
   {
      InitializeComponent();

      CultureMap = Globals.Cultures;
      ReligionMap = Globals.Religions;
      DataContext = this;
   }

   public Dictionary<string, Culture> CultureMap { get; set; }
   public Dictionary<string, Religion> ReligionMap { get; set; }

   // --- Main Toggle ---
   public bool WriteTemplateToPops
   {
      get;
      set
      {
         if (field != value)
         {
            field = value;
            OnPropertyChanged();
            // 1. Update Dropdowns state
            OnPropertyChanged(nameof(CanEditSpecifics));
            // 2. Update Sub-Toggles state (Optional: disable imports if main toggle is off)
            OnPropertyChanged(nameof(CanEditImports));
         }
      }
   }

   // --- NEW: Sub Toggles ---
   public bool ImportCulture
   {
      get;
      set
      {
         if (field != value)
         {
            field = value;
            OnPropertyChanged();
         }
      }
   } = true; // Default to true

   public bool ImportReligion
   {
      get;
      set
      {
         if (field != value)
         {
            field = value;
            OnPropertyChanged();
         }
      }
   } = true; // Default to true

   // Helper: Disable the dropdowns if we are writing template
   public bool CanEditSpecifics => !WriteTemplateToPops;

   // Helper: Enable the Import toggles only if we are writing template
   public bool CanEditImports => WriteTemplateToPops;

   public Culture SelectedCulture
   {
      get;
      set
      {
         if (field != value)
         {
            field = value;
            OnPropertyChanged();
         }
      }
   } = Culture.Empty;

   public Religion SelectedReligion
   {
      get;
      set
      {
         if (field != value)
         {
            field = value;
            OnPropertyChanged();
         }
      }
   } = Religion.Empty;

   public event PropertyChangedEventHandler? PropertyChanged;

   protected void OnPropertyChanged([CallerMemberName] string? name = null)
   {
      PropertyChanged?.Invoke(this, new(name));
   }

   private void OnApplyClick(object sender, RoutedEventArgs e)
   {
      foreach (var location in Selection.GetSelectedLocations)
      {
         foreach (var pop in location.Pops)
            if (WriteTemplateToPops)
            {
               if (ImportCulture && location.TemplateData.Culture != Culture.Empty)
                  Nx.Set(pop, PopDefinition.Field.Culture, location.TemplateData.Culture);
               if (ImportReligion && location.TemplateData.Religion != Religion.Empty)
                  Nx.Set(pop, PopDefinition.Field.Religion, location.TemplateData.Religion);
            }
            else
            {
               if (SelectedCulture != Culture.Empty)
                  Nx.Set(pop, PopDefinition.Field.Culture, SelectedCulture);
               if (SelectedReligion != Religion.Empty)
                  Nx.Set(pop, PopDefinition.Field.Religion, SelectedReligion);
            }
      }
   }
}