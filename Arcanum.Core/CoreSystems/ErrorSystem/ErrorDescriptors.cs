using System.Reflection;
using Arcanum.API.Attributes;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.ErrorSystem;

public class ErrorDescriptors
{
#pragma warning disable CS0618 // Type or member is obsolete
   private static readonly Lazy<ErrorDescriptors> LazyInstance = new(() => new());
#pragma warning restore CS0618 // Type or member is obsolete

   public static ErrorDescriptors Instance => LazyInstance.Value;

   [Obsolete("Marked as obsolete to prevent direct instantiation. Use Instance property instead.")]
   public ErrorDescriptors()
   {
   }

   public List<ErrorDataClass> Save()
   {
      List<ErrorDataClass> errorDataClasses = [];
      var properties = typeof(ErrorDescriptors).GetProperties();
      foreach (var property in properties)
      {
         if (property.Name.Equals(nameof(Instance)))
            continue;
         
         ErrorDataClass edc = new(property.Name);
         if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
         {
            var edcProperties = property.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var nestedProp in edcProperties)
            {
               var nType = nestedProp.PropertyType;
               if (nType != typeof(DiagnosticDescriptor))
                  continue;

               var diagnosticDescriptor = (DiagnosticDescriptor)nestedProp.GetValue(property.GetValue(this))!;
               edc.ErrorObjects.Add(new(diagnosticDescriptor.ReportSeverity,
                                        diagnosticDescriptor.Severity,
                                        nestedProp.Name));
            }
         }

         errorDataClasses.Add(edc);
      }

      return errorDataClasses;
   }

   public void WriteConfig(List<ErrorDataClass> edcs)
   {
      foreach (var edc in edcs)
      {
         var property = typeof(ErrorDescriptors).GetProperty(edc.ClassName);
         if (property == null || !property.CanWrite)
            continue;

         var edInstance = property.GetValue(this);
         if (edInstance == null)
            continue;

         foreach (var edo in edc.ErrorObjects)
         {
            var ddProperty = edInstance.GetType().GetProperty(edo.Name);
            if (ddProperty == null)
               continue;

            var diagnosticDescriptor = (DiagnosticDescriptor)ddProperty.GetValue(edInstance)!;
            diagnosticDescriptor.ReportSeverity = edo.Severity;
            diagnosticDescriptor.Severity = edo.DiagnosticSeverity;
         }
      }
   }

   [SettingsForceInlinePropertyGrid]
   public MiscellaneousError Misc { get; set; } = MiscellaneousError.Instance;
   [SettingsForceInlinePropertyGrid]
   public ParsingError Parse { get; set; } = ParsingError.Instance;
}