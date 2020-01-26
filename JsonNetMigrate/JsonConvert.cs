using System;
using System.Collections.Generic;
using System.Text.Json;

namespace JsonNetMigrate.Json
{
    /// <summary>
    /// Provides methods for converting between .NET types and JSON strings.
    /// </summary>
    public static class JsonConvert
    {
        /// <summary>
        /// Serializes the specified object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object? value)
            => SerializeObject(value, settings: null);

        /// <summary>
        /// Serializes the specified object to a JSON string using formatting.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object? value, Formatting formatting)
            => SerializeObject(value, settings: new JsonSerializerSettings { Formatting = formatting });

        /// <summary>
        /// Serializes the specified object to a JSON string using <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object? value, JsonSerializerSettings? settings)
        {
            var inputType = value?.GetType() ?? typeof(object);
            if (settings == null)
            {
                settings = new JsonSerializerSettings();
            }

            var serializerOptions = settings.ToJsonSerializerOptions();
            try
            {
                return JsonSerializer.Serialize(value, inputType, serializerOptions);
            }
            catch (InvalidOperationException ex)
            {
                throw new JsonException("JSON serialization failed", ex);
            }
            catch (ArgumentException ex)
            {
                throw new JsonException("JSON serialization failed", ex);
            }
        }

        /// <summary>
        /// Deserializes the JSON to a .NET object.
        /// </summary>
        /// <param name="json">The JSON to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object? DeserializeObject(string json)
            => DeserializeObject(json, typeof(object), settings: null);

        /// <summary>
        /// Deserializes the JSON to a .NET object using <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="json">The JSON to deserialize.</param>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to deserialize the object.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object? DeserializeObject(string json, JsonSerializerSettings? settings)
            => DeserializeObject(json, typeof(object), settings: settings);

        /// <summary>
        /// Deserializes the JSON to the specified .NET type.
        /// </summary>
        /// <param name="json">The JSON to deserialize.</param>
        /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object? DeserializeObject(string json, Type type)
            => DeserializeObject(json, type, settings: null);

        /// <summary>
        /// Deserializes the JSON to the specified .NET type using <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="json">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to deserialize the object.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object? DeserializeObject(string json, Type type, JsonSerializerSettings? settings)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            if (settings == null)
            {
                settings = new JsonSerializerSettings();
            }

            var deserializeOptions = settings.ToJsonSerializerOptions();
            return JsonSerializer.Deserialize(json, type, deserializeOptions);
        }

        /// <summary>
        /// Deserializes the JSON to the specified .NET type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="json">The JSON to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
#if NULLABLE_ATTR
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
#endif
        public static T DeserializeObject<T>(string json)
            => DeserializeObject<T>(json, settings: null);

        /// <summary>
        /// Deserializes the JSON to the specified .NET type using <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="json">The object to deserialize.</param>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to deserialize the object.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
#if NULLABLE_ATTR
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
#endif
        public static T DeserializeObject<T>(string json, JsonSerializerSettings? settings)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
                return default;
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
            }

            if (settings == null)
            {
                settings = new JsonSerializerSettings();
            }

            var deserializeOptions = settings.ToJsonSerializerOptions();
            return JsonSerializer.Deserialize<T>(json, deserializeOptions);
        }
    }
}
