namespace Arcanum.API.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class CustomResetMethod(string methodName) : Attribute
{
   public string MethodName { get; } = methodName;
}