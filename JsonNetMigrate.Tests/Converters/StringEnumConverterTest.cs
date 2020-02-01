using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Xunit;

namespace JsonNetMigrate.Json.Converters
{
    public class StringEnumConverterTest
    {
        [Fact]
        public void Read_StringComparisonEnum_ExactName_ShouldMatch()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter().CreateConverter(typeof(StringComparison), options);

            var json = $"\"{nameof(StringComparison.OrdinalIgnoreCase)}\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(StringComparison), options);

            Assert.Equal(StringComparison.OrdinalIgnoreCase, actual);
        }

        [Fact]
        public void Read_StringComparisonEnum_CamelCaseName_ShouldMatch()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter().CreateConverter(typeof(StringComparison), options);

            var json = "\"ordinalIgnoreCase\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(StringComparison), options);

            Assert.Equal(StringComparison.OrdinalIgnoreCase, actual);
        }

        [Fact]
        public void Read_StringComparisonEnum_SnakeName_ShouldThrowJsonException()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter().CreateConverter(typeof(StringComparison), options);

            var json = "\"ordinal_ignore_case\"";
            Assert.Throws<JsonException>(() =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
                reader.Read();
                converter.Read(ref reader, typeof(StringComparison), options);
            });
        }

        [Fact]
        public void Read_StringComparisonEnum_CamelCaseNamingPolicy_ExactName_ShouldMatch()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter(JsonNamingPolicy.CamelCase).CreateConverter(typeof(StringComparison), options);

            var json = $"\"{nameof(StringComparison.OrdinalIgnoreCase)}\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(StringComparison), options);

            Assert.Equal(StringComparison.OrdinalIgnoreCase, actual);
        }

        [Fact]
        public void Read_StringComparisonEnum_CamelCaseNamingPolicy_CamelCaseName_ShouldMatch()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter(JsonNamingPolicy.CamelCase).CreateConverter(typeof(StringComparison), options);

            var json = "\"ordinalIgnoreCase\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(StringComparison), options);

            Assert.Equal(StringComparison.OrdinalIgnoreCase, actual);
        }

        [Fact]
        public void Read_StringComparisonEnum_SnakeCaseNamingPolicy_ExactName_ShouldMatch()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter(new SnakeCaseNamingPolicy()).CreateConverter(typeof(StringComparison), options);

            var json = $"\"{nameof(StringComparison.OrdinalIgnoreCase)}\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(StringComparison), options);

            Assert.Equal(StringComparison.OrdinalIgnoreCase, actual);
        }

        [Fact]
        public void Read_StringComparisonEnum_SnakeCaseNamingPolicy_CamelCaseName_ShouldMatch()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter(new SnakeCaseNamingPolicy()).CreateConverter(typeof(StringComparison), options);

            var json = "\"ordinalIgnoreCase\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(StringComparison), options);

            Assert.Equal(StringComparison.OrdinalIgnoreCase, actual);
        }

        [Fact]
        public void Read_StringComparisonEnum_SnakeCaseNamingPolicy_SnakeCaseName_ShouldMatch()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter(new SnakeCaseNamingPolicy()).CreateConverter(typeof(StringComparison), options);

            var json = "\"ordinal_ignore_case\"";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(StringComparison), options);

            Assert.Equal(StringComparison.OrdinalIgnoreCase, actual);
        }

        [Fact]
        public void Read_StringComparisonEnum_ValidNumber_ShouldMatch()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter().CreateConverter(typeof(StringComparison), options);

            var json = $"{(int)StringComparison.OrdinalIgnoreCase}";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(StringComparison), options);

            Assert.Equal(StringComparison.OrdinalIgnoreCase, actual);
        }

        [Fact]
        public void Read_StringComparisonEnum_InvalidName_ShouldThrowJsonException()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter().CreateConverter(typeof(StringComparison), options);

            var json = $"\"Garbage\"";
            Assert.Throws<JsonException>(() =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
                reader.Read();
                converter.Read(ref reader, typeof(StringComparison), options);
            });
        }

        [Fact]
        public void Read_StringComparisonEnum_InvalidNumber_ShouldMatch()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter().CreateConverter(typeof(StringComparison), options);

            var json = $"-123456";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(StringComparison), options);

            Assert.Equal((StringComparison)(-123456), actual);
        }

        [Fact]
        public void Read_StringComparisonEnum_OutOfRangeNumber_ShouldThrowJsonException()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter().CreateConverter(typeof(StringComparison), options);

            var json = $"{long.MaxValue}";
            Assert.Throws<JsonException>(() =>
            {
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
                reader.Read();
                converter.Read(ref reader, typeof(StringComparison), options);
            });
        }

        [Theory]
        [InlineData("\"One\"", TestEnum.One)]
        [InlineData("1", TestEnum.One)]
        [InlineData("\"Deux\"", TestEnum.Two)]
        [InlineData("\"deux\"", TestEnum.Two)]
        [InlineData("\"DEUX\"", TestEnum.Two)]
        [InlineData("\"Two\"", TestEnum.Two)]
        [InlineData("\"two\"", TestEnum.Two)]
        [InlineData("\"TWO\"", TestEnum.Two)]
        [InlineData("2", TestEnum.Two)]
        [InlineData("\"Four\"", TestEnum.Three)]
        [InlineData("\"four\"", TestEnum.Three)]
        [InlineData("3", TestEnum.Three)]
        [InlineData("\"Three\"", TestEnum.Four)]
        [InlineData("\"three\"", TestEnum.Four)]
        [InlineData("\"five\"", TestEnum.Five)]
        [InlineData("\"Five\"", TestEnum.Five)]
        [InlineData("\"Six\"", TestEnum.Six)]
        [InlineData("\"Seven\"", TestEnum.Seven)]
        [InlineData("\"test\"", TestEnum.Seven)]
        [InlineData("8", TestEnum.Eight)]
        [InlineData("\"Eight\"", TestEnum.Nine)]
        [InlineData("\"eight\"", TestEnum.Ten)]
        [InlineData("\"EIGHT\"", TestEnum.Ten)]
        public void Read_TestEnum(string json, TestEnum expected)
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<TestEnum>)new StringEnumConverter().CreateConverter(typeof(TestEnum), options);

            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json).AsSpan());
            reader.Read();
            var actual = converter.Read(ref reader, typeof(StringComparison), options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Write_StringComparisonEnum_ShouldMatchName()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter().CreateConverter(typeof(StringComparison), options);

            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
                converter.Write(writer, StringComparison.OrdinalIgnoreCase, options);
            }
            var actual = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.Equal($"\"{nameof(StringComparison.OrdinalIgnoreCase)}\"", actual);
        }

        [Fact]
        public void Write_StringComparisonEnum_CamelCaseNamingPolicy_ShouldMatchName()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter(JsonNamingPolicy.CamelCase).CreateConverter(typeof(StringComparison), options);

            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
                converter.Write(writer, StringComparison.OrdinalIgnoreCase, options);
            }
            var actual = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.Equal("\"ordinalIgnoreCase\"", actual);
        }

        [Fact]
        public void Write_StringComparisonEnum_SnakeCaseNamingPolicy_ShouldMatchName()
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<StringComparison>)new StringEnumConverter(new SnakeCaseNamingPolicy()).CreateConverter(typeof(StringComparison), options);

            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
                converter.Write(writer, StringComparison.OrdinalIgnoreCase, options);
            }
            var actual = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.Equal("\"ordinal_ignore_case\"", actual);
        }

        [Theory]
        [InlineData(TestEnum.One, "\"One\"")]
        [InlineData(TestEnum.Two, "\"Deux\"")]
        [InlineData(TestEnum.Three, "\"Four\"")]
        [InlineData(TestEnum.Four, "\"Three\"")]
        [InlineData(TestEnum.Five, "\"five\"")]
        [InlineData(TestEnum.Six, "\"test\"")]
        [InlineData(TestEnum.Seven, "\"test\"")]
        [InlineData(TestEnum.Eight, "\"Eight\"")]
        [InlineData(TestEnum.Nine, "\"Eight\"")]
        [InlineData(TestEnum.Ten, "\"eight\"")]
        public void Write_TestEnum(TestEnum input, string expected)
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<TestEnum>)new StringEnumConverter().CreateConverter(typeof(TestEnum), options);

            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
                converter.Write(writer, input, options);
            }
            var actual = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(TestEnum.One, "\"one\"")]
        [InlineData(TestEnum.Two, "\"Deux\"")]
        [InlineData(TestEnum.Three, "\"Four\"")]
        [InlineData(TestEnum.Four, "\"Three\"")]
        [InlineData(TestEnum.Five, "\"five\"")]
        [InlineData(TestEnum.Six, "\"test\"")]
        [InlineData(TestEnum.Seven, "\"test\"")]
        [InlineData(TestEnum.Eight, "\"eight\"")]
        [InlineData(TestEnum.Nine, "\"Eight\"")]
        [InlineData(TestEnum.Ten, "\"eight\"")]
        public void Write_TestEnum_CamelCase(TestEnum input, string expected)
        {
            var options = new JsonSerializerOptions();
            var converter = (JsonConverter<TestEnum>)new StringEnumConverter(JsonNamingPolicy.CamelCase).CreateConverter(typeof(TestEnum), options);

            MemoryStream memoryStream;
            using (memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream))
            {
                converter.Write(writer, input, options);
            }
            var actual = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanConvert_StringComparisonEnum_ShouldReturnTrue()
        {
            var converter = new StringEnumConverter();
            Assert.True(converter.CanConvert(typeof(StringComparison)));
        }

        [Fact]
        public void CanConvert_TestEnum_ShouldReturnTrue()
        {
            var converter = new StringEnumConverter();
            Assert.True(converter.CanConvert(typeof(TestEnum)));
        }

        [Fact]
        public void CanConvert_EnumClass_ShouldReturnFalse()
        {
            var converter = new StringEnumConverter();
            Assert.False(converter.CanConvert(typeof(Enum)));
        }

        [Fact]
        public void CanConvert_String_ShouldReturnFalse()
        {
            var converter = new DBNullConverter();
            Assert.False(converter.CanConvert(typeof(string)));
        }

        public enum TestEnum
        {
            Zero = 0,
            One,
            [EnumMember(Value = "Deux")]
            Two,
            [EnumMember(Value = "Four")]
            Three,
            [EnumMember(Value = "Three")]
            Four,
            [EnumMember(Value = "five")]
            Five,
            [EnumMember(Value = "test")]
            Six,
            [EnumMember(Value = "test")]
            Seven,
            Eight,
            [EnumMember(Value = "Eight")]
            Nine,
            [EnumMember(Value = "eight")]
            Ten,
        }

        private sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                if (name == null) throw new ArgumentNullException(nameof(name));

#pragma warning disable CA1308 // Normalize strings to uppercase
                return Regex.Replace(name, "(?<=.)([A-Z])", "_$0").ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
            }
        }
    }
}
