namespace Arcanum.Core.CoreSystems.IO.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

public class KeyGestureJsonConverter : JsonConverter<KeyGesture>
{
   public override KeyGesture? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      var gestureText = reader.GetString();
      return gestureText != null ? (KeyGesture)new KeyGestureConverter().ConvertFromString(gestureText)! : null;
   }

   public override void Write(Utf8JsonWriter writer, KeyGesture value, JsonSerializerOptions options)
   {
      var gestureText = new KeyGestureConverter().ConvertToString(value);
      writer.WriteStringValue(gestureText);
   }
}