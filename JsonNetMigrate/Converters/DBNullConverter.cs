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
            if (reader.TokenType == JsonTokenType.Null)
            {
                return DBNull.Value;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var valueString = reader.GetString();
                if (!string.IsNullOrEmpty(valueString))
                {
                    throw new JsonException();
                }

                // We're returning null instead of DBNull.Value For compatibility with Json.NET
                return null;
            }

            throw new JsonException();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DBNull? value, JsonSerializerOptions options)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteNullValue();
        }
    }
}
