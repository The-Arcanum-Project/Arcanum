using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Nexus.Analyzer;

public static class Helpers
{
   public static (IParameterSymbol? parameter, int index) FindParameterWithAttribute(
      IMethodSymbol method,
      string attributeName)
   {
      for (var i = 0; i < method.Parameters.Length; i++)
         if (method.Parameters[i].GetAttributes().Any(a => a.AttributeClass?.Name == attributeName))
            return (method.Parameters[i], i);

      return (null, -1);
   }

   public static (IParameterSymbol? parameter, int index) FindParameterByName(IMethodSymbol method, string name)
   {
      for (var i = 0; i < method.Parameters.Length; i++)
         if (method.Parameters[i].Name == name)
            return (method.Parameters[i], i);

      return (null, -1);
   }

   public static ITypeSymbol? GetActualTypeOfValue(IOperation operation)
   {
      if (operation is IConversionOperation conv)
         return conv.Operand.Type;

      return operation?.Type;
   }

   public static bool TryGetTargetType(IInvocationOperation invocation,
                                       out ITypeSymbol? typeSymbol,
                                       out (IParameterSymbol? parameter, int index) enumParamInfo)
   {
      typeSymbol = null;
      var method = invocation.TargetMethod;
      enumParamInfo = FindParameterWithAttribute(method, "LinkedPropertyEnumAttribute");
      if (enumParamInfo.parameter == null)
         return false;

      var linkedEnumAttr = enumParamInfo.parameter.GetAttributes()
                                        .First(a => a.AttributeClass?.Name == "LinkedPropertyEnumAttribute");
      if (linkedEnumAttr.ConstructorArguments.Length == 0)
         return false;

      var targetParamName = linkedEnumAttr.ConstructorArguments[0].Value as string;
      if (string.IsNullOrEmpty(targetParamName))
         return false;

      // If "this" is used, get the value from the field enum in the class
      if (targetParamName!.Equals("this", StringComparison.OrdinalIgnoreCase) && invocation.Instance != null)
      {
         typeSymbol = GetActualTypeOfValue(invocation.Instance);
         return typeSymbol is not null;
      }

      var targetParameterInfo = FindParameterByName(method, targetParamName);
      if (targetParameterInfo.parameter == null)
         return false;

      var targetArgument = invocation.Arguments[targetParameterInfo.index];
      typeSymbol = GetActualTypeOfValue(targetArgument.Value);
      return typeSymbol is not null;
   }

   public static bool IsEnumValidForTarget(ITypeSymbol targetType,
                                           IArgumentOperation enumArgument,
                                           out IFieldSymbol? enumMemberSymbol,
                                           out ITypeSymbol? requiredEnumSymbol)
   {
      enumMemberSymbol = null;
      requiredEnumSymbol = targetType.GetMembers("Field").OfType<ITypeSymbol>().FirstOrDefault();
      if (requiredEnumSymbol is not { TypeKind: TypeKind.Enum })
         return false;

      enumMemberSymbol = GetEnumMemberSymbol(enumArgument.Value);
      if (enumMemberSymbol == null)
         return false;

      var isValid = SymbolEqualityComparer.Default.Equals(enumMemberSymbol.ContainingType, requiredEnumSymbol);
      return isValid;
   }

   private static IFieldSymbol? GetEnumMemberSymbol(IOperation operation)
   {
      return operation switch
      {
         IFieldReferenceOperation directFieldRef => directFieldRef.Field,
         IConversionOperation { Operand: IFieldReferenceOperation convertedFieldRef } => convertedFieldRef.Field,
         _ => null
      };
   }

   public static bool GetExpectedTypeFromEnumMember(IFieldSymbol enumMemberSymbol, out ITypeSymbol? expectedValueType)
   {
      var attribute = enumMemberSymbol.GetAttributes()
                                      .FirstOrDefault(attr => attr.AttributeClass?.Name == "ExpectedTypeAttribute");
      expectedValueType = null;
      if (attribute == null || attribute.ConstructorArguments.Length != 1)
         return false;

      expectedValueType = attribute.ConstructorArguments[0].Value as ITypeSymbol;
      return expectedValueType is not null;
   }
}