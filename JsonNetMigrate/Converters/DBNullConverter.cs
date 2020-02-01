using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonNetMigrate.Json.Converters
{
    /// <summary>
    /// A custom converter for <see cref="DBNull"/> values.
    /// </summary>
    public class DBNullConverter : JsonConverter<DBNull?>
    {
        /// <inheritdoc />
        public override DBNull? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;

            if (tokenType == JsonTokenType.Null)
            {
                return DBNull.Value;
            }

            if (tokenType == JsonTokenType.String)
            {
                var valueString = reader.GetString();
                if (!string.IsNullOrEmpty(valueString))
                {
                    throw new JsonException();
                }

                return null;
            }

            throw new JsonException();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DBNull? value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteNullValue();
        }
    }
}
