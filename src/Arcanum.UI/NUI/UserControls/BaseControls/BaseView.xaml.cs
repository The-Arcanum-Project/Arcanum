namespace Arcanum.UI.NUI.UserControls.BaseControls;

public partial class BaseView : IDisposable
{
   private readonly List<IDisposable> _disposables = [];

   public BaseView()
   {
      InitializeComponent();
   }

   public void RegisterDisposable(IDisposable disposable)
   {
      _disposables.Add(disposable);
   }

   public void UnregisterDisposable(IDisposable disposable)
   {
      _disposables.Remove(disposable);
   }

   public void Dispose()
   {
      foreach (var disposable in _disposables)
         disposable.Dispose();
      _disposables.Clear();

      if (BaseViewBorder != null)
         BaseViewBorder.Child = null;

      Content = null;
      GC.SuppressFinalize(this);
   }
}