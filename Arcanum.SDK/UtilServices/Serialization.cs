using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arcanum.API.UtilServices
{
   /// <summary>
   /// Defines common JSON property naming policies.
   /// </summary>
   public enum JsonPropertyNamingPolicyType
   {
      /// <summary>
      /// No specific naming policy is applied (property names are used as-is). This is the default.
      /// </summary>
      Default,

      /// <summary>
      /// Property names are converted to camelCase (e.g., "PropertyName" becomes "propertyName").
      /// </summary>
      CamelCase,
   }

   /// <summary>
   /// Defines custom rules for JSON serialization.
   /// </summary>
   public class JsonSerializationRules
   {
      /// <summary>
      /// Gets or sets a value that indicates whether the JSON output should be indented.
      /// Default is false.
      /// </summary>
      public bool WriteIndented { get; set; } = false;

      /// <summary>
      /// Gets or sets the policy used to convert property names.
      /// Default is JsonPropertyNamingPolicyType.Default.
      /// </summary>
      public JsonPropertyNamingPolicyType PropertyNamingPolicy { get; set; } = JsonPropertyNamingPolicyType.Default;

      /// <summary>
      /// Gets or sets a value that indicates whether null values should be ignored during serialization.
      /// Default is false (null values are included).
      /// </summary>
      public bool IgnoreNullValues { get; set; } = false;

      /// <summary>
      /// Gets or sets a value that indicates whether read-only properties are ignored during serialization.
      /// Default is false (read-only properties are included).
      /// </summary>
      public bool IgnoreReadOnlyProperties { get; set; } = false;

      /// <summary>
      /// Gets or sets a value that indicates whether fields are handled during serialization.
      /// Default is false (fields are ignored). Set to true to include fields.
      /// </summary>
      public bool IncludeFields { get; set; } = false;

      /// <summary>
      /// Gets or sets the default buffer size in bytes to use when creating temporary buffers.
      /// The default is 16_384.
      /// </summary>
      public int DefaultBufferSize { get; set; } = 16_384;

      /// <summary>
      /// Gets a list of custom converters to be used during serialization.
      /// </summary>
      public IList<JsonConverter> Converters { get; } = new List<JsonConverter>();

      /// <summary>
      /// Gets or sets how enums should be serialized.
      /// The default is to serialize as strings.
      /// </summary>
      public JsonStringEnumConverterOptions EnumSerialization { get; set; } =
         JsonStringEnumConverterOptions.SerializeAsString;
   }

   /// <summary>
   /// Defines options for how enums are serialized when JsonStringEnumConverter is used.
   /// </summary>
   public enum JsonStringEnumConverterOptions
   {
      /// <summary>
      /// Serializes enums as their underlying numeric value.
      /// </summary>
      SerializeAsNumber,

      /// <summary>
      /// Serializes enums as strings, respecting JsonPropertyNameAttribute if present on enum members.
      /// Naming policy is applied if specified in JsonSerializationRules.
      /// </summary>
      SerializeAsString,

      /// <summary>
      /// Serializes enums as strings, converting to camelCase if the CamelCase naming policy is active.
      /// </summary>
      SerializeAsStringCamelCase // Convenience for new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
   }

   /// <summary>
   /// Defines custom rules for JSON deserialization.
   /// </summary>
   public class JsonDeserializationRules
   {
      /// <summary>
      /// Gets or sets a value that determines whether property names are compared case-insensitively during deserialization.
      /// Default is false (case-sensitive).
      /// </summary>
      public bool PropertyNameCaseInsensitive { get; set; } = false;

      /// <summary>
      /// Gets or sets a value that indicates whether comments are allowed and handled during deserialization.
      /// The Default is JsonCommentHandling.Disallow.
      /// </summary>
      public JsonCommentHandling CommentHandling { get; set; } = JsonCommentHandling.Disallow;

      /// <summary>
      /// Gets or sets a value that defines whether an extra comma at the end of a list of JSON values in an object or array is allowed (and ignored).
      /// Default is false.
      /// </summary>
      public bool AllowTrailingCommas { get; set; } = false;

      /// <summary>
      /// Gets or sets the default buffer size in bytes to use when creating temporary buffers.
      /// The Default is 16_384.
      /// </summary>
      public int DefaultBufferSize { get; set; } = 16_384;

      /// <summary>
      /// Gets a list of custom converters to be used during deserialization.
      /// </summary>
      public IList<JsonConverter> Converters { get; } = new List<JsonConverter>();

      /// <summary>
      /// Gets or sets how enums should be deserialized when represented as strings.
      /// The Default is to expect numbers or case-sensitive string names.
      /// </summary>
      public JsonStringEnumConverterOptions EnumDeserialization { get; set; } =
         JsonStringEnumConverterOptions.SerializeAsNumber;
   }
}