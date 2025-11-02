using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arcanum.Core.CoreSystems.IO.JsonConverters;

public class EnumJsonConverter : JsonConverter<Enum>
{
   private const string TYPE_PROPERTY_NAME = "type";
   private const string VALUE_PROPERTY_NAME = "value";

   public override bool CanConvert(Type typeToConvert)
   {
      return typeof(Enum).IsAssignableFrom(typeToConvert);
   }

   public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
   {
      if (value == null!)
      {
         writer.WriteNullValue();
         return;
      }

      writer.WriteStartObject();
      writer.WriteString(TYPE_PROPERTY_NAME, value.GetType().AssemblyQualifiedName);
      writer.WriteString(VALUE_PROPERTY_NAME, value.ToString());
      writer.WriteEndObject();
   }

   public override Enum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      if (reader.TokenType == JsonTokenType.Null)
         return null!;

      if (reader.TokenType != JsonTokenType.StartObject)
         throw new JsonException("Expected StartObject token for Enum deserialization.");

      string? typeName = null;
      string? valueName = null;

      while (reader.Read())
      {
         if (reader.TokenType == JsonTokenType.EndObject)
         {
            if (typeName != null && valueName != null)
            {
               var enumType = Type.GetType(typeName);
               if (enumType is not { IsEnum: true })
                  throw new JsonException($"The type '{typeName}' is not a valid enum type.");

               return (Enum)Enum.Parse(enumType, valueName);
            }

            throw new
               JsonException($"JSON object for Enum is missing '{TYPE_PROPERTY_NAME}' or '{VALUE_PROPERTY_NAME}' properties.");
         }

         if (reader.TokenType == JsonTokenType.PropertyName)
         {
            var propertyName = reader.GetString();
            reader.Read();
            switch (propertyName)
            {
               case TYPE_PROPERTY_NAME:
                  typeName = reader.GetString();
                  break;
               case VALUE_PROPERTY_NAME:
                  valueName = reader.GetString();
                  break;
            }
         }
      }

      throw new JsonException("Unexpected end of JSON when reading Enum object.");
   }
}