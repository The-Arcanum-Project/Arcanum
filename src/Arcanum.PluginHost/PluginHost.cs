using System.Diagnostics;
using Arcanum.API;
using Arcanum.API.Events;
using Arcanum.API.Settings;
using Arcanum.API.UtilServices;
using Arcanum.PluginHost.PluginServices;

namespace Arcanum.PluginHost;

public static class HostInfo
{
   public static Version Version => new(0, 9, 2, 0);
}

public class PluginHost : IPluginHost
{
   private readonly Dictionary<Type, IService> _services = new();

   // Required services for the host to be functional and valid.
   private readonly ISettingsUiService _uiSettingsService = null!;

   public void RegisterDefaultServices()
   {
      RegisterService<IEventBus>(EventBus.Instance);
      //RegisterService<ISettingsStore>(new SettingsStore(_uiSettingsService, this));
   }

   public void Unload()
   {
      foreach (var service in _services.Values)
         service.Unload();
      _services.Clear();

      // Unload all required services.
      // TODO_uiSettingsService.Unload();
   }

   public IService.ServiceState VerifyState()
   {
      var allOk = true;

      foreach (var service in _services.Values)
         IsServiceOk(service, ref allOk);

      IsServiceOk(_uiSettingsService, ref allOk);

      return allOk ? IService.ServiceState.Ok : IService.ServiceState.Error;
   }

   private static void IsServiceOk(IService service, ref bool allOk)
   {
      Debug.Assert(service != null, "Service cannot be null.");

      if (service.VerifyState() != IService.ServiceState.Ok)
         allOk = false;
   }

   public string GuidToName(Guid guid)
   {
      return GetService<IPluginInfoService>().GetPluginByGuid(guid)?.Name ??
             throw new InvalidOperationException($"No plugin found with GUID: {guid}");
   }

   public T GetService<T>() where T : class, IService
   {
      if (_services.TryGetValue(typeof(T), out var service))
         return (T)service;

      throw new InvalidOperationException($"Service of type {typeof(T).Name} is not (yet?) registered.");
   }

   public void RegisterService<T>(T service) where T : class, IService
   {
      if (_services.ContainsKey(typeof(T)))
         throw new InvalidOperationException($"Service of type {typeof(T).Name} is already registered.");

      _services[typeof(T)] = service ?? throw new ArgumentNullException(nameof(service), "Service cannot be null.");
   }

   public void Log(string message, LoggingVerbosity verbosity = LoggingVerbosity.Info)
      => Console.WriteLine($"[{GetVerbosityPrefix(verbosity)}] [PluginHost] {message}");

   public void Log(string subRoutinePreFix, string message, LoggingVerbosity verbosity = LoggingVerbosity.Info)
      => Console.WriteLine($"[{GetVerbosityPrefix(verbosity)}] [PluginHost.{subRoutinePreFix[..8]}] {message}");

   private static string GetVerbosityPrefix(LoggingVerbosity verbosity)
   {
      return verbosity switch
      {
         LoggingVerbosity.Info => "Inf",
         LoggingVerbosity.Warning => "War",
         LoggingVerbosity.Error => "Err",
         _ => "Unk",
      };
   }
}