namespace Arcanum.Core.CoreSystems.Validators;

public interface IValidator
{
   public string Name { get; }
   public int Priority { get; }
   public void Validate();
}