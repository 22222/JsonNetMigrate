﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonNetMigrate.Json.Converters
{
    /// <summary>
    /// A custom <see cref="JsonConverter"/> for the <see cref="object"/> type.
    /// </summary>
    public class ObjectConverter : JsonConverter<object>
    {
        private readonly FloatParseHandling floatParseHandling;
        private readonly DateParseHandling dateParseHandling;

        /// <summary>
        /// Constructs a default <see cref="ObjectConverter"/>.
        /// </summary>
        public ObjectConverter()
            : this(floatParseHandling: FloatParseHandling.Double, dateParseHandling: DateParseHandling.DateTime) { }

        /// <summary>
        /// Constructs an <see cref="ObjectConverter"/> with the specified options.
        /// </summary>
        /// <param name="floatParseHandling">Specifies how floating point numbers are parsed when reading JSON text.</param>
        /// <param name="dateParseHandling">Specifies how date formatted strings are parsed when reading JSON text.</param>
        public ObjectConverter(FloatParseHandling floatParseHandling, DateParseHandling dateParseHandling)
        {
            this.floatParseHandling = floatParseHandling;
            this.dateParseHandling = dateParseHandling;
        }

        /// <inheritdoc />
#if NULLABLE_ATTR
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
#endif
        public override object Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;

            if (tokenType == JsonTokenType.Null)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 // Possible null reference return.
            }

            if (tokenType == JsonTokenType.True)
            {
                return true;
            }

            if (tokenType == JsonTokenType.False)
            {
                return false;
            }

            if (tokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out long valueLong))
                {
                    return valueLong;
                }

                if (floatParseHandling == FloatParseHandling.Decimal)
                {
                    return reader.GetDecimal();
                }

                return reader.GetDouble();
            }

            if (tokenType == JsonTokenType.String)
            {
                if (dateParseHandling == DateParseHandling.DateTimeOffset)
                {
                    if (reader.TryGetDateTimeOffset(out DateTimeOffset valueDto))
                    {
                        return valueDto;
                    }
                }
                else if (dateParseHandling == DateParseHandling.DateTime)
                {
                    if (reader.TryGetDateTime(out DateTime valueDt))
                    {
                        return valueDt;
                    }
                }

                return reader.GetString();
            }

            if (tokenType == JsonTokenType.StartArray)
            {
                return ReadList(ref reader, options);
            }

            if (tokenType == JsonTokenType.StartObject)
            {
                return ReadDictionary(ref reader, options);
            }

            throw new JsonException();
        }

        private List<object?> ReadList(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            var list = new List<object?>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return list;
                }

                var element = Read(ref reader, typeof(object), options);
                list.Add(element);
            }

            throw new JsonException();
        }

        private Dictionary<string, object?> ReadDictionary(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var dictionary = new Dictionary<string, object?>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();

                    reader.Read();
                    var propertyValue = Read(ref reader, typeof(object), options);
                    dictionary[propertyName] = propertyValue;
                }
                else
                {
                    throw new JsonException();
                }
            }

            throw new JsonException();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is JsonElement valueJsonElement)
            {
                valueJsonElement.WriteTo(writer);
                return;
            }

            throw new InvalidOperationException();
        }
    }
}
