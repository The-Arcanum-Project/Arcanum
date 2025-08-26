using System.ComponentModel;
using Arcanum.API;
using Arcanum.API.Settings;

namespace Arcanum.UI.WpfTesting;

public class ExamplePluginSettings : IPluginSetting
{
   public Guid OwnerGuid { get; } = Guid.NewGuid();
   
   public string Name { get; set; } = "Example Plugin Settings";
   public string Description { get; set; } = "Settings for the Example Plugin";
   public string Version { get; set; } = "1.0.0";
   public string Author { get; set; } = "Example Author";
   public string AssemblyPath { get; set; } = "path/to/assembly.dll";
   
   [DefaultValue(22)]
   public int Order { get; set; } = 1;
}
public class ExamplePluginSettings2 : IPluginSetting
{
   public Guid OwnerGuid { get; } = Guid.NewGuid();
   
   public string Name { get; set; } = "Example Plugin Settings";
   public string Description { get; set; } = "Settings for the Example Plugin";
   public string Version { get; set; } = "1.0.0";
   public string Author { get; set; } = "Example Author";
   public string AssemblyPath { get; set; } = "path/to/assembly.dll";
   
   [DefaultValue(22)]
   public int Order { get; set; } = 1;
   [DefaultValue(2)]
   public int Order2 { get; set; } = 1;
   [DefaultValue(221)]
   public int Order3 { get; set; } = 1;
   [DefaultValue(222)]
   public int Order4 { get; set; } = 1;
   [DefaultValue(225)]
   public int Order5 { get; set; } = 1;
}

public class ExamplePlugin : IPlugin
{
   public Guid Guid { get; } = Guid.NewGuid();
   public Version PluginVersion { get; } = null!;
   public Version RequiredHostVersion { get; } = null!;
   public string Name { get; } = "Example Plugin";
   public string Author { get; } = null!;
   public IEnumerable<IPluginMetadata.PluginDependency> Dependencies { get; } = null!;
   public void Log(string message, LoggingVerbosity verbosity = LoggingVerbosity.Info)
   {
      throw new NotImplementedException();
   }

   public PluginStatus Status { get; set; }
   public bool IsActive { get; set; }
   public PluginRuntimeInfo RuntimeInfo { get; set; } = null!;
   public string AssemblyPath { get; set; } = null!;
   public bool Initialize(IPluginHost host) => throw new NotImplementedException();

   public void OnEnable()
   {
      throw new NotImplementedException();
   }

   public void OnDisable()
   {
      throw new NotImplementedException();
   }

   public void Dispose()
   {
      throw new NotImplementedException();
   }
}

public class TheMotherOfAllPluginNamesIsHere : IPlugin
{
   public Guid Guid { get; } = Guid.NewGuid();
   public Version PluginVersion { get; } = null!;
   public Version RequiredHostVersion { get; } = null!;
   public string Name { get; } = "TheMotherOfAllPlugins";
   public string Author { get; } = null!;
   public IEnumerable<IPluginMetadata.PluginDependency> Dependencies { get; } = null!;
   public void Log(string message, LoggingVerbosity verbosity = LoggingVerbosity.Info)
   {
      throw new NotImplementedException();
   }

   public PluginStatus Status { get; set; }
   public bool IsActive { get; set; }
   public PluginRuntimeInfo RuntimeInfo { get; set; } = null!;
   public string AssemblyPath { get; set; } = null!;
   public bool Initialize(IPluginHost host) => throw new NotImplementedException();

   public void OnEnable()
   {
      throw new NotImplementedException();
   }

   public void OnDisable()
   {
      throw new NotImplementedException();
   }

   public void Dispose()
   {
      throw new NotImplementedException();
   }
}


