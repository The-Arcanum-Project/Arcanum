#region

using Arcanum.UI.AppFeatures;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

#endregion

namespace Arcanum.UI.Documentation.Implementation;

public class FeatureIdYamlConverter : IYamlTypeConverter
{
   public bool Accepts(Type type) => type == typeof(FeatureId);

   public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
   {
      var scalar = parser.Consume<Scalar>();
      if (string.IsNullOrEmpty(scalar.Value))
         return FeatureIds.Empty;

      return new FeatureId(scalar.Value.Trim().ToLower());
   }

   public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
   {
      var id = (FeatureId)value!;
      emitter.Emit(new Scalar(id.Value));
   }
}