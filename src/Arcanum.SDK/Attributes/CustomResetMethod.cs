namespace Arcanum.API.Attributes;

public class CustomResetMethod(string methodName) : Attribute
{
   public string MethodName { get; } = methodName;
}