using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Two.JsonDeepEqual;
using Xunit;

namespace JsonNetMigrate.Json.Converters
{
    public class DBNullConverterTest
    {
        [Fact]
        public void Read_Null_ShouldReturnDBNull()
        {
            var options = new JsonSerializerOptions();
            var converter = new DBNullConverter();

            var json = "null";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(DBNull), options);

            Assert.True(Convert.IsDBNull(actual));
        }

        [Fact]
        public void Read_EmptyString_ShouldReturnDBNull()
        {
            var options = new JsonSerializerOptions();
            var converter = new DBNullConverter();

            var json = "\"\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(DBNull), options);

            Assert.Null(actual);
        }

        [Fact]
        public void Read_EmptyObject_ShouldThrowJsonException()
        {
            var options = new JsonSerializerOptions();
            var converter = new DBNullConverter();

            var json = "{}";
            Assert.Throws<JsonException>(() =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
                reader.Read();
                converter.Read(ref reader, typeof(DBNull), options);
            });
        }

        [Fact]
        public void Read_Zero_ShouldThrowJsonException()
        {
            var options = new JsonSerializerOptions();
            var converter = new DBNullConverter();

            var json = "0";
            Assert.Throws<JsonException>(() =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
                reader.Read();
                converter.Read(ref reader, typeof(DBNull), options);
            });
        }

        [Fact]
        public void Write_DBNull_ShouldReturnNullValue()
        {
            var options = new JsonSerializerOptions();
            var converter = new DBNullConverter();

            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
                converter.Write(writer, DBNull.Value, options);
            }
            var actual = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.Equal("null", actual);
        }

        [Fact]
        public void Write_Null_ShouldReturnNullValue()
        {
            var options = new JsonSerializerOptions();
            var converter = new DBNullConverter();

            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                converter.Write(writer, null, options);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
            var actual = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.Equal("null", actual);
        }

        [Fact]
        public void CanConvert_DBNull_ShouldReturnTrue()
        {
            var converter = new DBNullConverter();
            Assert.True(converter.CanConvert(typeof(DBNull)));
        }

        [Fact]
        public void CanConvert_String_ShouldReturnFalse()
        {
            var converter = new DBNullConverter();
            Assert.False(converter.CanConvert(typeof(string)));
        }
    }
}
