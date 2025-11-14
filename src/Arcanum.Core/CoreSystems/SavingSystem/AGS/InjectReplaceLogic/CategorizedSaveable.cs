using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.InjectReplaceLogic;

public class CategorizedSaveable(IEu5Object target, SavingCategory savingCategory)
{
   public IEu5Object Target { get; } = target;
   public SavingCategory SavingCategory { get; set; } = savingCategory;
   public Eu5FileObj SaveLocation = Eu5FileObj.Empty;
   public (Enum, object)[] Injects = [];
   public InjectObj InjectedObj = InjectObj.Empty;

   public HashSet<PropertySavingMetadata> GetPropertiesToSave()
   {
      HashSet<PropertySavingMetadata> properties = [];

      foreach (var (@enum, _) in Injects)
      {
         var prop = Target.SaveableProps.FirstOrDefault(x => Equals(x.NxProp, @enum));
         if (prop is not null)
            properties.Add(prop);
      }

      return properties;
   }
}