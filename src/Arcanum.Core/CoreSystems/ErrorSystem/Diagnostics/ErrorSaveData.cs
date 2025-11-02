namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

public struct ErrorObjData(DiagnosticReportSeverity severity, DiagnosticSeverity diagnosticSeverity, string name)
{
   public DiagnosticReportSeverity Severity { get; set; } = severity;
   public DiagnosticSeverity DiagnosticSeverity { get; set; } = diagnosticSeverity;
   public string Name { get; set; } = name;
}

public struct ErrorDataClass(string className)
{
   public string ClassName { get; set; } = className;
   public List<ErrorObjData> ErrorObjects { get; set; } = [];
}