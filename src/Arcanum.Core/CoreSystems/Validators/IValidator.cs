namespace Arcanum.Core.CoreSystems.Validators;

public interface IValidator
{
   public int Priority { get; }
   public void Validate();
}