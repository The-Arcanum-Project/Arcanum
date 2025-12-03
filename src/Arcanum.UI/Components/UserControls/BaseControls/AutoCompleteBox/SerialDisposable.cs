namespace Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox;

sealed class SerialDisposable
   : IDisposable
{
   public IDisposable? Content
   {
      get;
      set
      {
         if (field != null!)
            field.Dispose();

         field = value;
      }
   }

   public void Dispose()
   {
      Content = null;
   }
}