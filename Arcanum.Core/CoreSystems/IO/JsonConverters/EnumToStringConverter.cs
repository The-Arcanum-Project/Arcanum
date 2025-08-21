using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class EnumJsonConverter : JsonConverter<Enum>
{
    private const string TypePropertyName = "type";
    private const string ValuePropertyName = "value";

    // This tells the serializer that this converter can handle the Enum type.
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Enum).IsAssignableFrom(typeToConvert);
    }

    // --- Serialization: From an Enum object to JSON ---
    public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        // Write the full assembly-qualified name of the enum type.
        // This is crucial for finding the correct type during deserialization.
        writer.WriteString(TypePropertyName, value.GetType().AssemblyQualifiedName);
        // Write the string name of the enum member (e.g., "Type", "Culture").
        writer.WriteString(ValuePropertyName, value.ToString());
        writer.WriteEndObject();
    }
    
    // --- Deserialization: From JSON to an Enum object ---
    public override Enum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token for Enum deserialization.");
        }

        string typeName = null;
        string valueName = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                // We've reached the end of our object, now we can build the enum.
                if (typeName != null && valueName != null)
                {
                    // Find the Type from its assembly-qualified name.
                    Type enumType = Type.GetType(typeName);
                    if (enumType == null || !enumType.IsEnum)
                    {
                        throw new JsonException($"The type '{typeName}' is not a valid enum type.");
                    }
                    // Parse the string value into an instance of that specific enum type.
                    return (Enum)Enum.Parse(enumType, valueName);
                }
                throw new JsonException($"JSON object for Enum is missing '{TypePropertyName}' or '{ValuePropertyName}' properties.");
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();
                reader.Read(); // Move to the property value.
                switch (propertyName)
                {
                    case TypePropertyName:
                        typeName = reader.GetString();
                        break;
                    case ValuePropertyName:
                        valueName = reader.GetString();
                        break;
                }
            }
        }
        
        throw new JsonException("Unexpected end of JSON when reading Enum object.");
    }
}