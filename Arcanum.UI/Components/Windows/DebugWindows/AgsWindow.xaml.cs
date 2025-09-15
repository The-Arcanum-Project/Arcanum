using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils.DevHelper;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class AgsWindow
{
   public AgsWindow()
   {
      InitializeComponent();

      var types = AgsRegistry.Ags.ToList();
      types.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
      AgsTypes = types;
      AgsItems = [];
   }

   public List<Type> AgsTypes
   {
      get => (List<Type>)GetValue(AgsTypesProperty);
      set => SetValue(AgsTypesProperty, value);
   }

   public static readonly DependencyProperty AgsTypesProperty =
      DependencyProperty.Register(nameof(AgsTypes),
                                  typeof(List<Type>),
                                  typeof(AgsWindow),
                                  new(new List<Type>()));

   public List<IAgs> AgsItems
   {
      get => (List<IAgs>)GetValue(AgsItemsProperty);
      set => SetValue(AgsItemsProperty, value);
   }

   public static readonly DependencyProperty AgsItemsProperty =
      DependencyProperty.Register(nameof(AgsItems),
                                  typeof(List<IAgs>),
                                  typeof(AgsWindow),
                                  new(new List<IAgs>()));

   public List<string> SeparatorChars { get; } = ["=", "<=", "=>", "<", ">", "?=", "!?"];

   private void AgsItemsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: Type type })
         return;

      if (type.ImplementsGenericInterface(typeof(ICollectionProvider<>), out var implementedType) &&
          implementedType != null)
      {
         var methodInfo = type.GetMethod("GetGlobalItems", BindingFlags.Public | BindingFlags.Static);
         if (methodInfo != null)
         {
            var allItems = (IEnumerable)methodInfo.Invoke(null, null)!;
            AgsItems = allItems.Cast<IAgs>().ToList();
            AgsItems.Sort((x, y) => string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));
            AgsItemsView.SelectedIndex = 0;
            return;
         }
      }

      AgsItems = [];
      AgsSavingText.Text = "";
   }

   private void ObjectSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: IAgs ags } lv)
         return;

      var context = ags.ToAgsContext(CommentChar.Text);
      var sb = new IndentedStringBuilder();
      context.BuildContext(sb);
      AgsSavingText.Text = sb.ToString();
   }

   private void ExportButton_Click(object sender, RoutedEventArgs e)
   {
   }
}