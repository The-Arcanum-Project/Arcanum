using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.History.Commands;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.Windows.Input;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.NUI.Generator;
using Arcanum.UI.NUI.UserControls.BaseControls;
using Common.UI.MBox;

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
   public bool IsUniqueIdBase { get; set; }

   private IEu5Object? CreatedObject { get; set; }
   private NavH? _navH;
   private List<Enum> _requiredProps = [];

   public Eu5ObjectCreator()
   {
      InitializeComponent();
   }

   // Catch Ctrl+Z and Ctrl+Y for undo/redo
   protected override void OnPreviewKeyDown(KeyEventArgs e)
   {
      if (Keyboard.Modifiers == ModifierKeys.Control)
      {
         if (e.Key == Key.Z)
         {
            AppData.HistoryManager.Undo(false);
            if (_navH != null)
               ObjectUI = Eu5UiGen.GenerateView(_navH, _requiredProps, false);
            e.Handled = true;
         }
         else if (e.Key == Key.Y)
         {
            AppData.HistoryManager.Redo(false);
            if (_navH != null)
               ObjectUI = Eu5UiGen.GenerateView(_navH, _requiredProps, false);
            e.Handled = true;
         }
         else if (e.Key == Key.Enter)
         {
            CreateButtonBase_OnClick(this, new());
            e.Handled = true;
         }
      }

      base.OnPreviewKeyDown(e);
   }

   private void CreateButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      if (!AreRequiredFieldsFilled(out var missingFields))
      {
         var createOrConfirm = IsUniqueIdBase ? "confirm" : "create";
         var fieldNames = string.Join(", ", missingFields.Select(f => f.ToString()));
         var result =
            MBox.Show($"Please fill in all required fields before {createOrConfirm}ing the object. Missing fields: {fieldNames}\n\nObject will not be {createOrConfirm}ed.",
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

   public static bool ShowDialog(Type eu5ObjectType, bool addToGlobals = false)
   {
      if (!CreateAndPromptForUniqueIdInput(eu5ObjectType, addToGlobals, out var hasUniqueIdBasedGlobalItems, out var newObject))
         return false;

      var window = new Eu5ObjectCreator
      {
         Owner = Application.Current.MainWindow,
         ObjectTitle = $"Create New {eu5ObjectType.Name}",
         CreatedObject = newObject,
         IsUniqueIdBase = hasUniqueIdBasedGlobalItems,
      };
      window.MarkRequiredFields();

      return window.ShowDialog() == true;
   }

   private void MarkRequiredFields()
   {
      if (CreatedObject == null)
         throw new InvalidOperationException("CreatedObject is null but has to be initialized before generating UI.");

      _requiredProps.Clear();
      foreach (var prop in CreatedObject.GetAllProperties())
         if (CreatedObject.IsRequired(prop))
            _requiredProps.Add(prop);

      _navH = new(CreatedObject, true, ContentPresenter, false);
      ObjectUI = Eu5UiGen.GenerateView(_navH, _requiredProps, false);
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

   public static IEu5Object? ShowPopUp(Type type, Action<IEu5Object> postCreationAction, bool addToGlobals = true)
   {
      if (!CreateAndPromptForUniqueIdInput(type, addToGlobals, out var hasUniqueIdBasedGlobalItems, out var newObj))
         return null;

      var window = new Eu5ObjectCreator
      {
         Owner = Application.Current.MainWindow,
         ObjectTitle = $"Create New {type.Name}",
         CreatedObject = newObj,
         IsUniqueIdBase = hasUniqueIdBasedGlobalItems,
      };
      window.MarkRequiredFields();
      window.WindowStyle = WindowStyle.ToolWindow;
      window.HeaderHeight = new(0);
      window.BorderThickness = new(2);
      window.BorderBrush = Brushes.Gray;

      postCreationAction(newObj);
      window.ShowDialog();

      return window.CreatedObject;
   }

   private static bool CreateAndPromptForUniqueIdInput(Type type, bool addToGlobals, out bool hasUniqueIdBasedGlobalItems, out IEu5Object newObj)
   {
      var globals1 = ((IEu5Object)EmptyRegistry.Empties[type]).GetGlobalItemsNonGeneric();
      var globals2 = ((IEu5Object)EmptyRegistry.Empties[type]).GetGlobalItemsNonGeneric();

      // if they are not the same instance we have a non UniqueId based global items dictionary
      hasUniqueIdBasedGlobalItems = ReferenceEquals(globals1, globals2);

      newObj = (IEu5Object)Activator.CreateInstance(type)!;

      if (hasUniqueIdBasedGlobalItems)
      {
         // we open the UniqueId input popup to get it first
         var inputPopup =
            new InputDialog("Unique ID", $"Enter a unique ID for the new {type.Name} object:", InputKind.String)
            {
               WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
         if (inputPopup.ShowDialog() != true)
            return false;

         var isValidUniqueId = inputPopup.Value is string uniqueId &&
                               !string.IsNullOrWhiteSpace(uniqueId) &&
                               !globals1.Contains(uniqueId);
         while (!isValidUniqueId)
         {
            var errorMessage = string.IsNullOrWhiteSpace(inputPopup.Value as string)
                                  ? "Unique ID cannot be empty. Please enter a valid unique ID."
                                  : $"An object with the Unique ID '{inputPopup.Value}' already exists. Please enter a different unique ID.";
            var retry = MBox.Show(errorMessage, "Invalid Unique ID", MBoxButton.OKCancel, MessageBoxImage.Error);
            if (retry == MBoxResult.Cancel)
               return true;

            inputPopup = new("Unique ID", $"Enter a unique ID for the new {type.Name} object:", InputKind.String)
            {
               WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            if (inputPopup.ShowDialog() != true)
               return false;

            isValidUniqueId = inputPopup.Value is string uid &&
                              !string.IsNullOrWhiteSpace(uid) &&
                              !globals1.Contains(uid);
         }

         newObj.UniqueId = (string)inputPopup.Value!;
      }

      if (newObj is IIndexRandomColor irc)
         irc.Index = newObj.GetGlobalItemsNonGeneric().Count - 1;

      var createCommand = new CreateObjectCommand(newObj, true, addToGlobals);
      createCommand.Execute();
      AppData.HistoryManager.AddCommand(createCommand);

      return true;
   }
}