using System.Reflection;
using System.Windows.Input;

namespace Arcanum.API.Core.KeyBinds;

public abstract class KeyBindProvider
{
   /// <summary>
   /// We use reflection on self to find all properties that are of type KeyGesture and return them as a dictionary.
   /// </summary>
   /// <returns></returns>
   public virtual Dictionary<string, KeyGesture> GetKeyBinds()
   {
      var keyBinds = new Dictionary<string, KeyGesture>();
      var properties = GetType().GetProperties();

      foreach (var property in properties)
         if (property.PropertyType == typeof(KeyGesture))
            keyBinds.Add(property.Name, (KeyGesture)property.GetValue(this)!);

      return keyBinds;
   }

   /// <summary>
   /// Sets multiple key binds at once using a dictionary.
   /// </summary>
   /// <param name="keyBinds"></param>
   public virtual void SetKeyBinds(Dictionary<string, KeyGesture> keyBinds)
   {
      foreach (var kvp in keyBinds)
         SetKeyBind(kvp.Key, kvp.Value);
   }

   /// <summary>
   /// Resets all key binds to their initialization values.
   /// </summary>
   /// <exception cref="InvalidOperationException"></exception>
   public virtual void ResetAllKeyBinds()
   {
      if (GetType().GetConstructor(Type.EmptyTypes) == null)
         throw new
            InvalidOperationException("KeyBindProvider must have a parameterless constructor to reset key binds.");

      var provider = Activator.CreateInstance(GetType());
      if (provider == null)
         throw new InvalidOperationException($"Could not create an instance of {GetType()}.");

      foreach (var keyBind in GetKeyBinds().Keys)
         InternalKeyReset(keyBind, (KeyBindProvider)provider);
   }

   /// <summary>
   /// Searches for the keybind withe the name keyBindName and resets it to its initialization value
   /// </summary>
   /// <param name="keyBindName"></param>
   /// <exception cref="KeyNotFoundException"></exception>
   public virtual void ResetKeyBind(string keyBindName)
   {
      if (GetType().GetConstructor(Type.EmptyTypes) == null)
         throw new
            InvalidOperationException("KeyBindProvider must have a parameterless constructor to reset key binds.");

      var provider = Activator.CreateInstance(GetType());
      if (provider == null)
         throw new InvalidOperationException($"Could not create an instance of {GetType()}.");

      InternalKeyReset(keyBindName, (KeyBindProvider)provider);
   }

   private void InternalKeyReset(string keyBindName, KeyBindProvider provider)
   {
      var defaultValue = GetKeyBind(provider, keyBindName);
      if (defaultValue == null)
         throw new KeyNotFoundException($"Key bind '{keyBindName}' not found in {provider.GetType()}.");

      var actualProperty = GetKeyBind(this, keyBindName);
      if (actualProperty == null)
         throw new KeyNotFoundException($"Key bind '{keyBindName}' not found in {GetType()}.");

      actualProperty.SetValue(this, defaultValue.GetValue(provider));
   }

   /// <summary>
   /// we use reflection on self to find all properties that are of type KeyGesture
   /// and set the value of the property with the name keyBindName.
   /// </summary>
   /// <param name="keyBindName"></param>
   /// <param name="keyGesture"></param>
   public virtual void SetKeyBind(string keyBindName, KeyGesture keyGesture)
   {
      GetKeyBind(this, keyBindName).SetValue(this, keyGesture);
   }

   private static PropertyInfo GetKeyBind(KeyBindProvider provider, string keyBindName)
   {
      var properties = provider.GetType().GetProperties();
      foreach (var property in properties)
         if (property.PropertyType == typeof(KeyGesture) && property.Name == keyBindName)
            return property;

      throw new KeyNotFoundException($"Key bind '{keyBindName}' not found in {provider.GetType()}.");
   }
}