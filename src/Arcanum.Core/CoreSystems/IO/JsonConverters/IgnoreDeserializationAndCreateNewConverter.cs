#define DEBUG_RESET_SETTINGS
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arcanum.Core.CoreSystems.IO.JsonConverters;

public class IgnoreDeserializationAndCreateNewConverter<T> : JsonConverter<T> where T : new()
{
   public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      // TODO: why is this here what did this do
#if DEBUG_RESET_SETTINGS
      reader.Skip();
      return new();
#endif

      // standard behavior
      return JsonSerializer.Deserialize<T>(ref reader, options)!;
   }

   public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
   {
      JsonSerializer.Serialize(writer, value, options);
   }
}