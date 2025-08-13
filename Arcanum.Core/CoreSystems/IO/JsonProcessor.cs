using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Arcanum.API.Core.IO;
using Arcanum.API.UtilServices;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.IO.JsonConverters;

// For the interface

namespace Arcanum.Core.CoreSystems.IO;

/// <summary>
/// Implements IJsonProcessor using System.Text.Json.
/// </summary>
internal static class JsonProcessor
{
   private static readonly JsonSerializerOptions DefaultSerializerOptions;
   private static readonly JsonSerializerOptions DefaultDeserializerOptions;

   static JsonProcessor()
   {
      var defaultRules = new JsonSerializationRules { WriteIndented = true, };

      var defaultDeserializationRules = new JsonDeserializationRules
      {
         PropertyNameCaseInsensitive = true, AllowTrailingCommas = true,
      };

      // Initialize default serializer options
      DefaultSerializerOptions = CreateSerializerOptions(defaultRules);

      // Initialize default deserializer options
      DefaultDeserializerOptions = CreateDeserializerOptions(defaultDeserializationRules);
   }

   /// <summary>
   /// Creates JsonSerializerOptions from JsonSerializationRules.
   /// </summary>
   private static JsonSerializerOptions CreateSerializerOptions(JsonSerializationRules? rules)
   {
      if (rules == null)
         return new(DefaultSerializerOptions); // Copy defaults

      var options = new JsonSerializerOptions
      {
         WriteIndented = rules.WriteIndented,
         DefaultBufferSize = rules.DefaultBufferSize,
         IgnoreReadOnlyProperties = rules.IgnoreReadOnlyProperties,
         IncludeFields = rules.IncludeFields,
         Converters =
         {
            new KeyGestureJsonConverter(),
         },
      };

      if (rules.IgnoreNullValues)
         options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

      switch (rules.PropertyNamingPolicy)    
      {
         case JsonPropertyNamingPolicyType.CamelCase:
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            break;
         case JsonPropertyNamingPolicyType.Default:
         default:
            options.PropertyNamingPolicy = null; // Use default (as-is)
            break;
      }

      var stringEnumConverterAdded = false;
      if (rules.EnumSerialization != JsonStringEnumConverterOptions.SerializeAsNumber)
      {
         var enumNamingPolicy = rules.PropertyNamingPolicy == JsonPropertyNamingPolicyType.CamelCase ||
                                rules.EnumSerialization ==
                                JsonStringEnumConverterOptions.SerializeAsStringCamelCase
                                   ? JsonNamingPolicy.CamelCase
                                   : null;
         options.Converters.Add(new JsonStringEnumConverter(enumNamingPolicy));
         stringEnumConverterAdded = true;
      }

      foreach (var converter in rules.Converters)
      {
         // Avoid adding JsonStringEnumConverter twice if already handled
         if (stringEnumConverterAdded && converter is JsonStringEnumConverter)
            continue;

         options.Converters.Add(converter);
      }

      return options;
   }

   /// <summary>
   /// Creates JsonSerializerOptions from JsonDeserializationRules.
   /// </summary>
   private static JsonSerializerOptions CreateDeserializerOptions(JsonDeserializationRules? rules)
   {
      if (rules == null)
         return new(DefaultDeserializerOptions); // Copy defaults

      var options = new JsonSerializerOptions
      {
         PropertyNameCaseInsensitive = rules.PropertyNameCaseInsensitive,
         AllowTrailingCommas = rules.AllowTrailingCommas,
         DefaultBufferSize = rules.DefaultBufferSize,
         Converters = { new KeyGestureJsonConverter() },
      };

      var stringEnumConverterAdded = false;
      if (rules.EnumDeserialization != JsonStringEnumConverterOptions.SerializeAsNumber)
      {
         var enumNamingPolicy =
            rules.EnumDeserialization == JsonStringEnumConverterOptions.SerializeAsStringCamelCase
               ? JsonNamingPolicy.CamelCase
               : null;
         options.Converters.Add(new JsonStringEnumConverter(enumNamingPolicy,
                                                            allowIntegerValues:
                                                            true)); // Allow numbers too for robustness
         stringEnumConverterAdded = true;
      }

      foreach (var converter in rules.Converters)
      {
         if (stringEnumConverterAdded && converter is JsonStringEnumConverter)
            continue;

         options.Converters.Add(converter);
      }

      return options;
   }

   public static void Serialize<T>(string path,
                                   T value,
                                   Encoding? encoding = null,
                                   JsonSerializationRules? rules = null)
   {
      if (string.IsNullOrEmpty(path))
         throw new ArgumentException("Path cannot be null or empty.", nameof(path));

      File.WriteAllText(path, Serialize(value, rules), encoding ?? Encoding.UTF8);
   }

   public static string Serialize<T>(T value, JsonSerializationRules? rules = null)
   {
      var options = rules == null
                       ? DefaultSerializerOptions
                       : CreateSerializerOptions(rules);
      try
      {
         return JsonSerializer.Serialize(value, options);
      }
      catch (JsonException ex)
      {
         throw new JsonSerializationException($"Failed to serialize type {typeof(T).FullName}.", ex);
      }
      catch (NotSupportedException ex) // Can be thrown for types that cannot be serialized
      {
         throw new JsonSerializationException($"Serialization not supported for type {typeof(T).FullName}.", ex);
      }
   }

   public static T? DefaultDeserialize<T>(string path,
                                          Encoding? encoding = null,
                                          JsonDeserializationRules? rules = null)
   {
      if (string.IsNullOrEmpty(path) || !File.Exists(path))
         return default;

      return Deserialize<T>(File.ReadAllText(path, encoding ?? Encoding.UTF8), rules);
   }

   public static T? Deserialize<T>(string json, JsonDeserializationRules? rules = null)
   {
      if (string.IsNullOrEmpty(json))
         return default;

      var options = rules == null
                       ? DefaultSerializerOptions
                       : CreateDeserializerOptions(rules);
      try
      {
         return JsonSerializer.Deserialize<T>(json, options);
      }
      catch (JsonException ex)
      {
         throw new
            JsonDeserializationException($"Failed to deserialize JSON to type {typeof(T).FullName}. JSON: '{json[..Math.Min(json.Length, 100)]}{(json.Length > 100 ? "..." : "")}'",
                                         ex);
      }
      catch (NotSupportedException ex) // Can be thrown for types that cannot be deserialized
      {
         throw new JsonDeserializationException($"Deserialization not supported for type {typeof(T).FullName}.", ex);
      }
   }

   public static bool TryDeserialize<T>(string json, out T? value, JsonDeserializationRules? rules = null)
   {
      try
      {
         value = Deserialize<T>(json, rules);
      }
      catch (JsonDeserializationException)
      {
         value = default;
         return false;
      }

      return true;
   }

   public static async Task<string> SerializeAsync<T>(T value,
                                                      JsonSerializationRules? rules = null,
                                                      CancellationToken cancellationToken = default)
   {
      // System.Text.Json.SerializeAsync is for writing to a stream which we get for serializing to a string asynchronously.
      using var stream = new MemoryStream();
      var options = rules == null
                       ? DefaultSerializerOptions
                       : CreateSerializerOptions(rules);
      try
      {
         await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);
         stream.Position = 0;
         using var reader = new StreamReader(stream, Encoding.UTF8);
         return await reader.ReadToEndAsync(cancellationToken);
      }
      catch (JsonException ex)
      {
         throw new JsonSerializationException($"Failed to asynchronously serialize type {typeof(T).FullName}.", ex);
      }
      catch (NotSupportedException ex)
      {
         throw new
            JsonSerializationException($"Asynchronous serialization not supported for type {typeof(T).FullName}.", ex);
      }
   }

   public static async Task<T?> DeserializeAsync<T>(string json,
                                                    JsonDeserializationRules? rules = null,
                                                    CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrEmpty(json))
         return default;

      // System.Text.Json.SerializeAsync is for writing to a stream which we get for serializing to a string asynchronously.
      using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
      var options = rules == null
                       ? DefaultSerializerOptions
                       : CreateDeserializerOptions(rules);
      try
      {
         return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
      }
      catch (JsonException ex)
      {
         throw new
            JsonDeserializationException($"Failed to asynchronously deserialize JSON to type {typeof(T).FullName}. JSON: '{json[..Math.Min(json.Length, 100)]}{(json.Length > 100 ? "..." : "")}'",
                                         ex);
      }
      catch (NotSupportedException ex)
      {
         throw new
            JsonDeserializationException($"Asynchronous deserialization not supported for type {typeof(T).FullName}.",
                                         ex);
      }
   }

   public static async Task<T?> DeserializeAsync<T>(Stream utf8JsonStream,
                                                    JsonDeserializationRules? rules = null,
                                                    CancellationToken cancellationToken = default)
   {
      if (utf8JsonStream is not { CanRead: true })
         throw new ArgumentException("Stream cannot be null and must be readable.", nameof(utf8JsonStream));

      var options = rules == null
                       ? DefaultSerializerOptions
                       : CreateDeserializerOptions(rules);
      try
      {
         return await JsonSerializer.DeserializeAsync<T>(utf8JsonStream, options, cancellationToken);
      }
      catch (JsonException ex)
      {
         throw new
            JsonDeserializationException($"Failed to asynchronously deserialize JSON stream to type {typeof(T).FullName}.",
                                         ex);
      }
      catch (NotSupportedException ex)
      {
         throw new
            JsonDeserializationException($"Asynchronous deserialization not supported for type {typeof(T).FullName}.",
                                         ex);
      }
   }

   public static async Task SerializeAsync<T>(Stream utf8JsonStream,
                                              T value,
                                              JsonSerializationRules? rules = null,
                                              CancellationToken cancellationToken = default)
   {
      if (utf8JsonStream is not { CanWrite: true })
         throw new ArgumentException("Stream cannot be null and must be writable.", nameof(utf8JsonStream));

      var options = rules == null
                       ? DefaultSerializerOptions
                       : CreateSerializerOptions(rules);
      try
      {
         await JsonSerializer.SerializeAsync(utf8JsonStream, value, options, cancellationToken);
      }
      catch (JsonException ex)
      {
         throw new
            JsonSerializationException($"Failed to asynchronously serialize type {typeof(T).FullName} to stream.", ex);
      }
      catch (NotSupportedException ex)
      {
         throw new
            JsonSerializationException($"Asynchronous serialization not supported for type {typeof(T).FullName}.", ex);
      }
   }
}