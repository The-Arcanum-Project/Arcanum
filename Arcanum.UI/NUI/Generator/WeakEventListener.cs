namespace Arcanum.UI.NUI.Generator;

public class WeakEventListener<TInstance, TSource, TEventArgs>(TInstance instance,
                                                               Action<TInstance, object?, TEventArgs> onEventAction,
                                                               Action<WeakEventListener<TInstance, TSource, TEventArgs>>
                                                                  onDetachAction)
   where TInstance : class
{
   private readonly WeakReference<TInstance> _weakInstance = new(instance);

   public void OnEvent(object? sender, TEventArgs e)
   {
      if (_weakInstance.TryGetTarget(out var instance))
         onEventAction(instance, sender, e);
      else
         Detach();
   }

   public void Detach()
   {
      onDetachAction(this);
   }
}