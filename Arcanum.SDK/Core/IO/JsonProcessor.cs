using System.IO;
using System.Text.Json;
using Arcanum.API.UtilServices;

namespace Arcanum.API.Core.IO;
// Exposing this via API

/// <summary>
/// Provides an abstraction for JSON serialization and deserialization operations
/// with support for custom rules.
/// </summary>
public interface IJsonProcessor : IService
{
   /// <summary>
   /// Serializes the specified object to a JSON string using custom rules.
   /// </summary>
   /// <typeparam name="T">The type of the object to serialize.</typeparam>
   /// <param name="value">The object to serialize.</param>
   /// <param name="rules">Custom serialization rules. If null, default rules are applied.</param>
   /// <returns>A JSON string representation of the object.</returns>
   /// <exception cref="JsonSerializationException">Thrown if serialization fails.</exception>
   string Serialize<T>(T value, JsonSerializationRules? rules = null);

   /// <summary>
   /// Deserializes the JSON string to the specified .NET type using custom rules.
   /// </summary>
   /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
   /// <param name="json">The JSON string to deserialize.</param>
   /// <param name="rules">Custom deserialization rules. If null, default rules are applied.</param>
   /// <returns>The deserialized object from the JSON string, or null if the JSON string is null, empty, or represents a JSON null.</returns>
   /// <exception cref="JsonDeserializationException">Thrown if deserialization fails due to invalid JSON or type mismatch.</exception>
   T? Deserialize<T>(string json, JsonDeserializationRules? rules = null);

   /// <summary>
   /// Attempts to deserialize the given JSON string to an object of the specified type using custom rules.
   /// </summary>
   /// <typeparam name="T">The type of the object to deserialize.</typeparam>
   /// <param name="json">The JSON string to deserialize.</param>
   /// <param name="value">When this method returns, contains the deserialized object, if successful; otherwise, the default value of <typeparamref name="T"/>.</param>
   /// <param name="rules">Custom deserialization rules. If null, default rules are applied.</param>
   /// <returns><c>true</c> if the deserialization succeeds; otherwise, <c>false</c>.</returns>
   /// <exception cref="JsonException">Thrown if the JSON string is malformed, or the deserialization fails unexpectedly.</exception>
   bool TryDeserialize<T>(string json, out T? value, JsonDeserializationRules? rules = null);

   /// <summary>
   /// Asynchronously serializes the specified object to a JSON string using custom rules.
   /// </summary>
   Task<string> SerializeAsync<T>(T value,
                                  JsonSerializationRules? rules = null,
                                  CancellationToken cancellationToken = default);

   /// <summary>
   /// Asynchronously deserializes the JSON string to the specified .NET type using custom rules.
   /// </summary>
   Task<T?> DeserializeAsync<T>(string json,
                                JsonDeserializationRules? rules = null,
                                CancellationToken cancellationToken = default);

   /// <summary>
   /// Asynchronously deserializes JSON from a stream to the specified .NET type using custom rules.
   /// </summary>
   Task<T?> DeserializeAsync<T>(Stream utf8JsonStream,
                                JsonDeserializationRules? rules = null,
                                CancellationToken cancellationToken = default);

   /// <summary>
   /// Asynchronously serializes the specified object to a UTF-8 JSON stream using custom rules.
   /// </summary>
   Task SerializeAsync<T>(Stream utf8JsonStream,
                          T value,
                          JsonSerializationRules? rules = null,
                          CancellationToken cancellationToken = default);
}

// Custom exceptions for clarity
public class JsonSerializationException(string message, Exception innerException) : Exception(message, innerException);

public class
   JsonDeserializationException(string message, Exception innerException) : Exception(message, innerException);