using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;

namespace Arcanum.Core.FlowControlServices;

public static class EmptyValidation
{
   public static void ValidateEmptyObjects()
   {
      foreach (var obj in EmptyRegistry.Empties.Values)
      {
         if (obj is not IEu5Object eu5Object)
            continue;

         foreach (var prop in eu5Object.GetAllProperties())
         {
            var value = eu5Object._getValue(prop);
            if (value == null!)
               eu5Object._setValue(prop, eu5Object.GetDefaultValue(prop));
         }
      }

      // Post validation steps
      foreach (var obj in EmptyRegistry.Empties.Values)
      {
         if (obj is not IEu5Object eu5Object)
            continue;

         foreach (var prop in eu5Object.GetAllProperties())
         {
            var value = eu5Object._getValue(prop);
            if (value == null!)
               throw new InvalidOperationException($"Empty object of type {eu5Object.GetType().Name} has null property {prop} after validation.");
         }
      }
   }
}