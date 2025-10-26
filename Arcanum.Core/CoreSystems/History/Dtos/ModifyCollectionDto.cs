namespace Arcanum.Core.CoreSystems.History.Dtos;

public class ModifyCollectionDto
{
   public ModifyCollectionDto()
   {
   }

   public Eu5Dto[] Targets { get; set; } = [];
   public object Value { get; set; } = null!;
   public Enum TargetProperty { get; set; } = null!;
}