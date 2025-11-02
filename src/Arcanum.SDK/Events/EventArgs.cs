using System.Reflection;

namespace Arcanum.API.Events;

public enum EventSource
{
   Core,
   Plugin,
   User
}

// Base class for all plugin-related event arguments
public class BasePluginEventArgs(Guid pluginGuid, EventSource source) : EventArgs
{
   public Guid PluginGuid { get; } = pluginGuid;
   public EventSource Source { get; } = source;
}

public class OverrideBaseEventArgs(Guid pluginGuid, EventSource source) : BasePluginEventArgs(pluginGuid, source)
{
   public bool Handled { get; set; } = false;
}

/// <summary>
///
///
///
/// 
/// PropertyInfo is null if the entire plugin setting is being changed.
/// </summary>
/// <param name="pluginGuid"></param>
/// <param name="source"></param>
/// <param name="info"></param>
/// <param name="value"></param>
public class PluginSettingEventArgs(Guid pluginGuid, EventSource source, PropertyInfo? info, object? value)
   : BasePluginEventArgs(pluginGuid, source)
{
   public object? Value { get; } = value;
   public PropertyInfo? PropertyInfo { get; } = info;
}

#region Control Event Args

public class InputConfirmEventArgs(string value, string pluginName, string key) : EventArgs
{
   public string SettingKey { get; } = key;
   public string Value { get; } = value;
   public string PluginName { get; } = pluginName;
}

#endregion