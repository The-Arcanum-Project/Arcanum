namespace Nexus.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class NexusConfigAttribute(bool generateEquality = true) : Attribute
{
   public bool GenerateEquality { get; set; } = generateEquality;
}