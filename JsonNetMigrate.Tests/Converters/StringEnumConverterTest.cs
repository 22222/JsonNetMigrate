using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace JsonNetMigrate.Json.Converters
{
    public class StringEnumConverterTest
    {
        [Fact]
        public void Read_StringComparisonEnum_ValidName_ShouldMatch()
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
    }
}
