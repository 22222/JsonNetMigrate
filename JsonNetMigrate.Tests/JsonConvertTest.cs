using JsonNetMigrate.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Two.JsonDeepEqual;
using Xunit;

namespace JsonNetMigrate.Json
{
    public class JsonConvertTest
    {
        [Fact]
        public void SerializeObject_SampleObject_ShouldMatchJsonNet()
        {
            var obj = CreateObject();
            var expected = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            var actual = JsonConvert.SerializeObject(obj);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Formatting.None, NullValueHandling.Include)]
        [InlineData(Formatting.Indented, NullValueHandling.Include)]
        [InlineData(Formatting.None, NullValueHandling.Ignore)]
        [InlineData(Formatting.Indented, NullValueHandling.Ignore)]
        public void SerializeObject_SampleObject_CamelCaseProperties_ShouldMatchJsonNet(Formatting formatting, NullValueHandling nullValueHandling)
        {
            var obj = CreateObject();
            var expectedSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Formatting = Enum.Parse<Newtonsoft.Json.Formatting>(formatting.ToString()),
                NullValueHandling = Enum.Parse<Newtonsoft.Json.NullValueHandling>(nullValueHandling.ToString()),
            };
            var expected = Newtonsoft.Json.JsonConvert.SerializeObject(obj, expectedSettings);

            var actualSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = formatting,
                NullValueHandling = nullValueHandling,
            };
            var actual = JsonConvert.SerializeObject(obj, actualSettings);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(FloatParseHandling.Double, DateParseHandling.DateTime)]
        [InlineData(FloatParseHandling.Decimal, DateParseHandling.DateTimeOffset)]
        [InlineData(FloatParseHandling.Double, DateParseHandling.None)]
        public void SerializeObject_Then_DeserializeObjectAsDictionary_Sample_ShouldMatchJsonNet(FloatParseHandling floatParseHandling, DateParseHandling dateParseHandling)
        {
            var obj = CreateObject();
            var expectedJson = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            var expectedSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                FloatParseHandling = Enum.Parse<Newtonsoft.Json.FloatParseHandling>(floatParseHandling.ToString()),
                DateParseHandling = Enum.Parse<Newtonsoft.Json.DateParseHandling>(dateParseHandling.ToString()),
            };
            var expected = Newtonsoft.Json.JsonConvert.DeserializeObject(expectedJson, typeof(Dictionary<string, object>), expectedSettings);

            var actualJson = JsonConvert.SerializeObject(obj);
            var actualSettings = new JsonSerializerSettings
            {
                FloatParseHandling = floatParseHandling,
                DateParseHandling = dateParseHandling,
            };
            var actual = JsonConvert.DeserializeObject(actualJson, typeof(Dictionary<string, object>), actualSettings);
            JsonDeepEqualAssert.Equal(expected, actual);

            actual = JsonConvert.DeserializeObject<Dictionary<string, object>>(actualJson, actualSettings);
            JsonDeepEqualAssert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeObject_Then_DeserializeObjectAsObject_Sample_ShouldMatchJsonNet()
        {
            var obj = CreateObject();
            var expectedJson = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            var expected = Newtonsoft.Json.JsonConvert.DeserializeObject(expectedJson, typeof(object));

            var actualJson = JsonConvert.SerializeObject(obj);
            var actual = JsonConvert.DeserializeObject(actualJson, typeof(object));
            JsonDeepEqualAssert.Equal(expected, actual, new JsonDeepEqualDiffOptions { DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffK" });

            actual = JsonConvert.DeserializeObject<object>(expectedJson);
            JsonDeepEqualAssert.Equal(expected, actual, new JsonDeepEqualDiffOptions { DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffK" });

            expected = Newtonsoft.Json.JsonConvert.DeserializeObject(expectedJson);
            actual = JsonConvert.DeserializeObject(actualJson);
            JsonDeepEqualAssert.Equal(expected, actual, new JsonDeepEqualDiffOptions { DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffK" });
        }

        private static object CreateObject()
        {
            var obj = new
            {
                ValueString = "Hello World",
                ValueEmptyString = string.Empty,
                ValueNull = default(object),
                ValueTrue = true,
                ValueFalse = false,
                ValueInt = 1,
                ValueIntMin = int.MinValue,
                ValueIntMax = int.MaxValue,
                ValueLong = 1L,
                ValueLongMin = long.MinValue,
                ValueLongMax = long.MaxValue,
                ValueDouble = 1.1d,
                ValueFloat = 1.1f,
                ValueDateTimeUtc = new DateTime(2002, 2, 2, 12, 22, 22, 222, DateTimeKind.Utc),
                ValueDateTimeUnspecified = new DateTime(2002, 2, 2, 12, 22, 22, 222, DateTimeKind.Unspecified),
                ValueDateTimeOffsetUtc = new DateTimeOffset(2002, 2, 2, 12, 22, 22, 222, TimeSpan.Zero),
                ValueDateTimeOffsetNotLocal = new DateTimeOffset(2002, 2, 2, 12, 22, 22, 222, TimeSpan.FromHours(-12)),
                ValueGuid = new Guid("92ec72c2-2484-4f82-ad17-1ec86cef09b2"),
                ValueBinary = new byte[] { 1, 2, 3 },
                ValueUri = new Uri("http://example.com"),
                ValueIntArray = new int[] { 1, 2, 3 },
                ValueStringList = new List<string> { "one", "two", "three" },
                ValueObjectArray = new object[] { "one", 2, new object[] { 3, "four" }, new { five = "5", six = 6 } },
                ValueObject = new { a = 1, b = new object[] { "c", "d" }, e = new { f = 1, g = "test" }, h = new Dictionary<string, object> { ["i"] = 0 } },
                ValueDictionary = new Dictionary<string, object> { ["a"] = 1, ["b"] = "two", ["c"] = new object[] { "d", 3 } },
                ValueEnum = StringComparison.OrdinalIgnoreCase,
                ValueNullableEnum = (StringComparison?)StringComparison.Ordinal,
                ValueNullableEnumNull = (StringComparison?)null,
                ValueTestEnum = TestEnum.Two,
                ValueNullableTestEnum = (TestEnum?)TestEnum.Two,
            };
            return obj;
        }

        [Fact]
        public void DeserializeObjectOfDbNull_EmptyString_ShouldMatchJsonNet()
        {
            var json = "\"\"";
            var expected = Newtonsoft.Json.JsonConvert.DeserializeObject<DBNull>(json);

            var actual = JsonConvert.DeserializeObject<DBNull>(json);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(TestEnum.Zero)]
        [InlineData(TestEnum.One)]
        [InlineData(TestEnum.Two)]
        [InlineData(TestEnum.Three)]
        [InlineData(TestEnum.Four)]
        [InlineData(TestEnum.Five)]
        [InlineData(TestEnum.Six)]
        [InlineData(TestEnum.Seven)]
        [InlineData(TestEnum.Eight)]
        [InlineData(TestEnum.Nine)]
        [InlineData(TestEnum.Ten)]
        public void SerializeObject_TestEnum_ShouldMatchJsonNet(TestEnum input)
        {
            var expected = Newtonsoft.Json.JsonConvert.SerializeObject(input);

            var actual = JsonConvert.SerializeObject(input);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(TestEnum.Zero)]
        [InlineData(TestEnum.One)]
        [InlineData(TestEnum.Two)]
        [InlineData(TestEnum.Three)]
        [InlineData(TestEnum.Four)]
        [InlineData(TestEnum.Five)]
        [InlineData(TestEnum.Six)]
        [InlineData(TestEnum.Seven)]
        [InlineData(TestEnum.Eight)]
        [InlineData(TestEnum.Nine)]
        [InlineData(TestEnum.Ten)]
        public void SerializeObject_Then_DeserializeObject_TestEnum_ShouldMatchJsonNet(TestEnum input)
        {
            var expectedJson = Newtonsoft.Json.JsonConvert.SerializeObject(input);
            var expected = Newtonsoft.Json.JsonConvert.DeserializeObject<TestEnum>(expectedJson);

            var actualJson = JsonConvert.SerializeObject(input);
            var actual = JsonConvert.DeserializeObject<TestEnum>(actualJson);
            JsonDeepEqualAssert.Equal(expected, actual);
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
            [EnumMember(Value = "Test")]
            Seven,
            Eight,
            [EnumMember(Value = "EIGHT")]
            Nine,
            [EnumMember(Value = "eight")]
            Ten,
        }
    }
}
