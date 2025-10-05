using System.Windows;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.NUI.Nui2.Nui2Gen;
using Arcanum.UI.NUI.UserControls.BaseControls;
using Common.UI.MBox;
using Nexus.Core;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class Eu5ObjectCreator
{
   public static readonly DependencyProperty ObjectTitleProperty =
      DependencyProperty.Register(nameof(ObjectTitle),
                                  typeof(string),
                                  typeof(Eu5ObjectCreator),
                                  new(default(string)));

   public string ObjectTitle
   {
      get => (string)GetValue(ObjectTitleProperty);
      init => SetValue(ObjectTitleProperty, value);
   }

   public static readonly DependencyProperty ObjectUIProperty =
      DependencyProperty.Register(nameof(ObjectUI),
                                  typeof(BaseView),
                                  typeof(Eu5ObjectCreator),
                                  new(default(BaseView)));

   public BaseView ObjectUI
   {
      get => (BaseView)GetValue(ObjectUIProperty);
      set => SetValue(ObjectUIProperty, value);
   }

   private IEu5Object? CreatedObject { get; set; }

   public Eu5ObjectCreator()
   {
      InitializeComponent();
   }

   private void CreateButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      if (!AreRequiredFieldsFilled(out var missingFields))
      {
         var fieldNames = string.Join(", ", missingFields.Select(f => f.ToString()));
         var result =
            MBox.Show($"Please fill in all required fields before creating the object. Missing fields: {fieldNames}\n\nObject will not be created.",
                      "Missing Required Fields",
                      MBoxButton.OKCancel,
                      MessageBoxImage.Warning);
         if (result == MBoxResult.Cancel)
         {
            CreatedObject = null;
            Close();
         }
      }
      else
      {
         Close();
      }
   }

   private void CancelButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      CreatedObject = null;
      Close();
   }

   public static bool ShowDialog(Type eu5ObjectType, out IEu5Object? newObject, bool addToGlobals = false)
   {
      var window = new Eu5ObjectCreator
      {
         Owner = Application.Current.MainWindow,
         ObjectTitle = $"Create New {eu5ObjectType.Name}",
         CreatedObject = (IEu5Object?)Activator.CreateInstance(eu5ObjectType),
      };
      window.MarkRequiredFields();

      newObject = window.CreatedObject;
      if (addToGlobals)
         newObject?.GetGlobalItemsNonGeneric().Add(newObject.UniqueId, newObject);
      return window.ShowDialog() == true;
   }

   private void MarkRequiredFields()
   {
      if (CreatedObject == null)
         throw new InvalidOperationException("CreatedObject is null but has to be initialized before generating UI.");

      List<Enum> requiredProps = [];
      foreach (var prop in CreatedObject.GetAllProperties())
         if (CreatedObject.IsRequired(prop))
            requiredProps.Add(prop);

      ObjectUI = Eu5UiGen.GenerateView(new(CreatedObject, false, ContentPresenter), requiredProps, false, true);
   }

   private bool AreRequiredFieldsFilled(out List<Enum> missingFields)
   {
      missingFields = [];
      if (CreatedObject == null)
         return false;

      foreach (var prop in CreatedObject.GetAllProperties())
         if (CreatedObject.IsRequired(prop))
         {
            object? value = null;
            Nx.ForceGet(CreatedObject, prop, ref value);
            var defaultValue = CreatedObject.GetDefaultValue(prop);
            if (value == null || value.Equals(defaultValue))
               missingFields.Add(prop);
         }

      return missingFields.Count == 0;
   }

   public static void ShowPopUp(Type type, Action<object> action, bool addToGlobals = true)
   {
      var window = new Eu5ObjectCreator
      {
         Owner = Application.Current.MainWindow,
         ObjectTitle = $"Create New {type.Name}",
         CreatedObject = (IEu5Object?)Activator.CreateInstance(type),
      };
      window.MarkRequiredFields();
      window.WindowStyle = WindowStyle.ToolWindow;
      window.HeaderHeight = new(0);
      window.BorderThickness = new(2);
      window.BorderBrush = System.Windows.Media.Brushes.Gray;

      window.ShowDialog();

      if (window.CreatedObject != null)
      {
         var newObject = window.CreatedObject!;
         if (addToGlobals)
            newObject.GetGlobalItemsNonGeneric().Add(newObject.UniqueId, newObject);
         action(newObject);
      }
   }
}