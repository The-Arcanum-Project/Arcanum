using System.Windows;
using System.Windows.Input;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.Components.Windows.DebugWindows;
using Arcanum.UI.NUI.Nui2.Nui2Gen;
using Arcanum.UI.NUI.Nui2.Nui2Gen.NavHistory;
using Arcanum.UI.NUI.UserControls.BaseControls;
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
      set => SetValue(ObjectTitleProperty, value);
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

   public Type? Eu5ObjectType { get; set; }

   public IEu5Object? CreatedObject { get; set; }

   public Eu5ObjectCreator()
   {
      InitializeComponent();
   }

   private void CreateButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      DialogResult = true;
      Close();
   }

   private void CancelButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      DialogResult = false;
      Close();
   }

   public static bool ShowDialog(Type eu5ObjectType, out IEu5Object? newObject)
   {
      var window = new Eu5ObjectCreator
      {
         Owner = Application.Current.MainWindow,
         ObjectTitle = $"Create New {eu5ObjectType.Name}",
         Eu5ObjectType = eu5ObjectType,
         CreatedObject = (IEu5Object?)Activator.CreateInstance(eu5ObjectType)
      };
      window.MarkRequiredFields();

      newObject = window.CreatedObject;
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

      ObjectUI = Eu5UiGen.GenerateView(new(CreatedObject, false, ContentPresenter), requiredProps, false);
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
}