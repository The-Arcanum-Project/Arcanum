using System.IO;
using Arcanum.API.Core.IO;
using Arcanum.API.UtilServices;

namespace Arcanum.Core.CoreSystems.IO;

public class APIWrapperJsonProcessor : IJsonProcessor
{
   public void Unload()
   {
   }

   // We have no internal state to verify in this service, so we return Ok state.
   public IService.ServiceState VerifyState() => IService.ServiceState.Ok;

   public string Serialize<T>(T value, JsonSerializationRules? rules = null) => JsonProcessor.Serialize(value, rules);

   public T? Deserialize<T>(string json, JsonDeserializationRules? rules = null)
      => JsonProcessor.Deserialize<T>(json, rules);

   public bool TryDeserialize<T>(string json, out T? value, JsonDeserializationRules? rules = null)
      => JsonProcessor.TryDeserialize(json, out value, rules);

   public Task<string> SerializeAsync<T>(T value,
                                         JsonSerializationRules? rules = null,
                                         CancellationToken cancellationToken = default)
      => JsonProcessor.SerializeAsync(value, rules, cancellationToken);

   public Task<T?> DeserializeAsync<T>(string json,
                                       JsonDeserializationRules? rules = null,
                                       CancellationToken cancellationToken = default)
      => JsonProcessor.DeserializeAsync<T>(json, rules, cancellationToken);

   public Task<T?> DeserializeAsync<T>(Stream utf8JsonStream,
                                       JsonDeserializationRules? rules = null,
                                       CancellationToken cancellationToken = default)
      => JsonProcessor.DeserializeAsync<T>(utf8JsonStream, rules, cancellationToken);

   public Task SerializeAsync<T>(Stream utf8JsonStream,
                                 T value,
                                 JsonSerializationRules? rules = null,
                                 CancellationToken cancellationToken = default)
      => JsonProcessor.SerializeAsync(utf8JsonStream, value, rules, cancellationToken);
}