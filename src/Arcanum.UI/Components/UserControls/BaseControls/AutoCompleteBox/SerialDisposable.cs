namespace Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox;

sealed class SerialDisposable
   : IDisposable
{
   private IDisposable? _content;

   public IDisposable? Content
   {
      get => _content;
      set
      {
         if (_content != null!)
            _content.Dispose();

         _content = value;
      }
   }

   public void Dispose()
   {
      Content = null;
   }
}