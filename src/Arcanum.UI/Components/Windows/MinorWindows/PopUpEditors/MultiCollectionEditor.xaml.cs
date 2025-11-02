using System.Collections;
using System.Reflection;
using System.Windows;

namespace Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;

public partial class MultiCollectionEditor
{
   private ICollectionEditorViewModel? ViewModel => DataContext as ICollectionEditorViewModel;
   private object? _result;

   public MultiCollectionEditor()
   {
      InitializeComponent();
   }

   // ===========================
   // NON-GENERIC ENTRY POINT
   // ===========================
   public static object ShowDialogN(
      Window owner,
      string title,
      Type itemType,
      IEnumerable<ICollection> sourceCollections,
      IEnumerable? globalItemPool = null)
   {
      var genericMethod = typeof(MultiCollectionEditor)
        .GetMethod(nameof(ShowDialogGeneric), BindingFlags.Static | BindingFlags.Public)!;

      var specificMethod = genericMethod.MakeGenericMethod(itemType);

      return specificMethod.Invoke(null, [owner, title, sourceCollections, globalItemPool])!;
   }

   // ===========================
   // GENERIC IMPLEMENTATION 
   // ===========================
   public static CollectionEditResult<T> ShowDialogGeneric<T>(
      Window owner,
      string title,
      IEnumerable<ICollection> sourceCollections,
      IEnumerable? globalItemPool = null) where T : notnull
   {
      var window = new MultiCollectionEditor
      {
         Owner = owner,
         Title = title,
         DataContext = new DualListEditorViewModel<T>(sourceCollections.Cast<ICollection<T>>(),
                                                      globalItemPool?.Cast<T>()),
      };

      window.ShowDialog();
      return window._result as CollectionEditResult<T> ?? new CollectionEditResult<T> { Canceled = true };
   }

   private void OkButton_Click(object sender, RoutedEventArgs e)
   {
      _result = ViewModel?.GetResult();
      DialogResult = true;
      Close();
   }
}