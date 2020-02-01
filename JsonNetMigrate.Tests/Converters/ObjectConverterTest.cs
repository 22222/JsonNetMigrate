using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Two.JsonDeepEqual;
using Xunit;

namespace JsonNetMigrate.Json.Converters
{
    public class ObjectConverterTest
    {
        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("\"test\"", "test")]
        [InlineData("\"2002-02-22T13:14:15Z\"", "2002-02-22T13:14:15Z")]
        [InlineData("\"1\"", "1")]
        [InlineData("2", 2L)]
        [InlineData("3.0", 3d)]
        [InlineData("4.5", 4.5d)]
        [InlineData("null", null)]
        public void Read_SimpleValue_ShouldMatchExpected(string json, object? expected)
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();

            var converter = new ObjectConverter(FloatParseHandling.Double, DateParseHandling.None);
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Read_NumericValue_FloatParseHandlingDecimal_ShouldReturnDecimal()
        {
            var json = "2.0";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();

            var converter = new ObjectConverter(floatParseHandling: FloatParseHandling.Decimal, dateParseHandling: DateParseHandling.None);
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            Assert.Equal(2m, actual);
        }

        [Fact]
        public void Read_DateValue_ShouldReturnDateTime()
        {
            var json = "\"2002-02-22\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();

            var converter = new ObjectConverter();
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            var expected = new DateTime(2002, 2, 22, 0, 0, 0, DateTimeKind.Unspecified);
            Assert.Equal(expected, actual);
            Assert.Equal(expected.Kind, ((DateTime)actual!).Kind);
        }

        [Fact]
        public void Read_DateTimeValue_ShouldReturnDateTime()
        {
            var json = "\"2002-02-22T13:14:15\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();

            var converter = new ObjectConverter();
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            var expected = new DateTime(2002, 2, 22, 13, 14, 15, DateTimeKind.Unspecified);
            Assert.Equal(expected, actual);
            Assert.Equal(expected.Kind, ((DateTime)actual!).Kind);
        }

        [Fact]
        public void Read_DateTimeValueUtc_ShouldReturnDateTime()
        {
            var json = "\"2002-02-22T13:14:15Z\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();

            var converter = new ObjectConverter();
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());
            var actualDt = (DateTime)actual!;

            var expected = new DateTime(2002, 2, 22, 13, 14, 15, DateTimeKind.Utc);
            Assert.Equal(expected, actual);
            Assert.Equal(expected.Kind, ((DateTime)actual!).Kind);
        }

        [Fact]
        public void Read_DateTimeValueNotUtc_ShouldReturnDateTime()
        {
            var json = "\"2002-02-22T13:14:15-08:00\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();

            var converter = new ObjectConverter();
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());
            var actualDt = (DateTime)actual!;

            var expected = new DateTime(2002, 2, 22, 21, 14, 15, DateTimeKind.Utc);
            Assert.Equal(expected, TimeZoneInfo.ConvertTimeToUtc(actualDt));
            Assert.Equal(DateTimeKind.Local, actualDt.Kind);
        }

        [Fact]
        public void Read_DateValue_DateParseHandlingOffset_ShouldReturnDateTimeOffset()
        {
            var json = "\"2002-02-22\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();

            var converter = new ObjectConverter(FloatParseHandling.Double, DateParseHandling.DateTimeOffset);
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            var expectedDt = new DateTime(2002, 2, 22, 0, 0, 0);
            Assert.Equal(expectedDt, ((DateTimeOffset)actual!).DateTime);
        }

        [Fact]
        public void Read_DateTimeValue_DateParseHandlingOffset_ShouldReturnDateTimeOffset()
        {
            var json = "\"2002-02-22T13:14:15\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();

            var converter = new ObjectConverter(FloatParseHandling.Double, DateParseHandling.DateTimeOffset);
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            var expectedDt = new DateTime(2002, 2, 22, 13, 14, 15);
            Assert.Equal(expectedDt, ((DateTimeOffset)actual!).DateTime);
        }

        [Fact]
        public void Read_DateTimeValueUtc_DateParseHandlingOffset_ShouldReturnDateTimeOffset()
        {
            var json = "\"2002-02-22T13:14:15Z\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();

            var converter = new ObjectConverter(FloatParseHandling.Double, DateParseHandling.DateTimeOffset);
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            var expected = new DateTimeOffset(2002, 2, 22, 13, 14, 15, TimeSpan.Zero);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Read_DateTimeValueNotUtc_DateParseHandlingOffset_ShouldReturnDateTimeOffset()
        {
            var json = "\"2002-02-22T13:14:15-08:00\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();

            var converter = new ObjectConverter(FloatParseHandling.Double, DateParseHandling.DateTimeOffset);
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            var expected = new DateTimeOffset(2002, 2, 22, 13, 14, 15, TimeSpan.FromHours(-8));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(JsonCommentHandling.Allow)]
        [InlineData(JsonCommentHandling.Skip)]
        public void Read_ObjectWithComments_ShouldMatch(JsonCommentHandling commentHandling)
        {
            const string json = @"/* Comment */{ /* Comment */
""a"":1,
// Comment
""b"": // Comment
    2,
/* Comment */
""c"": /* Comment */ 3,
""d"": [ // Comment
4/* Comment */,/* Comment */ 5 /* Comment */ ]/* Comment */
/* Comment */}";

            var jsonReaderOptions = new JsonReaderOptions
            {
                CommentHandling = commentHandling,
            };
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan(), jsonReaderOptions);
            reader.Read();

            var converter = new ObjectConverter();
            var actual = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            var expected = new Dictionary<string, object>
            {
                ["a"] = 1L,
                ["b"] = 2L,
                ["c"] = 3L,
                ["d"] = new List<object> { 4L, 5L },
            };
            JsonDeepEqualAssert.Equal(expected, actual);
        }

        [Fact]
        public void Read_CommentOnly_ShouldThrowJsonException()
        {
            const string json = @"// Comment";

            var converter = new ObjectConverter();
            var jsonReaderOptions = new JsonReaderOptions { CommentHandling = JsonCommentHandling.Allow };
            Assert.Throws<JsonException>(() =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan(), jsonReaderOptions);
                reader.Read();
                converter.Read(ref reader, typeof(object), new JsonSerializerOptions());
            });
        }

        [Fact]
        public void Read_TokenTypeNone_ShouldThrowJsonException()
        {
            var converter = new ObjectConverter();
            Assert.Throws<JsonException>(() =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes("1").AsSpan());
                converter.Read(ref reader, typeof(object), new JsonSerializerOptions());
            });
        }

        [Fact]
        public void Write_Object_ShouldReturnNewObject()
        {
            var options = new JsonSerializerOptions();
            var converter = new ObjectConverter();

            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
                converter.Write(writer, new object(), options);
            }
            var actual = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.Equal("{}", actual);
        }

        [Fact]
        public void Write_Null_ShouldThrowJsonException()
        {
            var options = new JsonSerializerOptions();
            var converter = new ObjectConverter();

            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                Assert.Throws<JsonException>(() => converter.Write(writer, null, options));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }

        [Fact]
        public void Write_Dictionary_ShouldThrowJsonException()
        {
            var options = new JsonSerializerOptions();
            var converter = new ObjectConverter();

            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
                Assert.Throws<JsonException>(() => converter.Write(writer, new Dictionary<string, object>(), options));
            }
        }

        [Fact]
        public void CanConvert_Object_ShouldReturnTrue()
        {
            var converter = new ObjectConverter();
            Assert.True(converter.CanConvert(typeof(object)));
        }

        [Fact]
        public void CanConvert_String_ShouldReturnFalse()
        {
            var converter = new ObjectConverter();
            Assert.False(converter.CanConvert(typeof(string)));
        }

        [Fact]
        public void CanConvert_Dictionary_ShouldReturnFalse()
        {
            var converter = new ObjectConverter();
            Assert.False(converter.CanConvert(typeof(Dictionary<string, object>)));
        }
    }
}
