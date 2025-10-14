/*using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Arcanum.SDK.Settings;

namespace Arcanum.PluginHost.Settings;

public abstract class PluginSettingConverter : JsonConverter<IPluginSetting>
{
    private const string TYPE_DISCRIMINATOR = "$type";

    // No constructor needed for DI anymore

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IPluginSetting).IsAssignableFrom(typeToConvert);
    }

    public override IPluginSetting? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonObject = JsonNode.Parse(ref reader);
        if (jsonObject == null) return null;

        if (!jsonObject.TryGetPropertyValue(TYPE_DISCRIMINATOR, out var typeNode) || typeNode is null)
        {
            throw new JsonException($"Could not find required '{TYPE_DISCRIMINATOR}' property.");
        }

        var typeKey = typeNode.GetValue<string>();
        // Use the static registry to resolve the type
        var type = PluginSettingTypeRegistry.ResolveType(typeKey!);

        if (type == null)
        {
            throw new JsonException($"The plugin setting type key '{typeKey}' is not registered. Ensure PluginSettingTypeRegistry.Initialize() was called.");
        }

        jsonObject.Remove(TYPE_DISCRIMINATOR);
        return jsonObject.Deserialize(type, options) as IPluginSetting;
    }

    public override void Write(Utf8JsonWriter writer, IPluginSetting value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var type = value.GetType();
        // Use the static registry to resolve the key
        var typeKey = PluginSettingTypeRegistry.ResolveKey(type);

        if (string.IsNullOrEmpty(typeKey))
        {
            throw new JsonException($"The type '{type.FullName}' has not been registered. Does it have a {nameof(PluginSettingKeyAttribute)}?");
        }

        var jsonObject = JsonSerializer.SerializeToNode(value, type, options)!.AsObject();
        jsonObject.Add(TYPE_DISCRIMINATOR, JsonValue.Create(typeKey));
        jsonObject.WriteTo(writer, options);
    }
}*/

