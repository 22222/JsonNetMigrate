#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Test = Xunit.FactAttribute;

namespace JsonNetMigrate.Json
{
    /// <remarks>
    /// These tests are all based on ones from the [Newtonsoft.Json library](https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json.Tests/JsonConvertTest.cs).
    /// </remarks>
    public class NewtonsoftJsonConvertTest
    {
        [Test]
        public void SerializeObjectEnsureEscapedArrayLength()
        {
            const char nonAsciiChar = (char)257;
            const char escapableNonQuoteAsciiChar = '\0';

            string value = nonAsciiChar + @"\" + escapableNonQuoteAsciiChar;

            string convertedValue = JsonConvert.SerializeObject((object)value);
            Assert.Equal(@"""" + nonAsciiChar + @"\\\u0000""", convertedValue);
        }

        public class PopulateTestObject
        {
            public decimal Prop { get; set; }
        }

        public class NameTableTestClass
        {
            public string? Value { get; set; }
        }

        [Test]
        public void DeserializeObject_EmptyString()
        {
            object? result = JsonConvert.DeserializeObject(string.Empty);
            Assert.Null(result);
        }

        [Test]
        public void DeserializeObject_Integer()
        {
            object? result = JsonConvert.DeserializeObject("1");
            Assert.Equal(1L, result);
        }

        [Test]
        public void DeserializeObject_Integer_EmptyString()
        {
            int? value = JsonConvert.DeserializeObject<int?>("");
            Assert.Null(value);
        }

        [Test]
        public void DeserializeObject_Decimal_EmptyString()
        {
            decimal? value = JsonConvert.DeserializeObject<decimal?>("");
            Assert.Null(value);
        }

        [Test]
        public void DeserializeObject_DateTime_EmptyString()
        {
            DateTime? value = JsonConvert.DeserializeObject<DateTime?>("");
            Assert.Null(value);
        }

        [Test]
        public void SerializeObjectInvalid()
        {
            // Assert.Throws<ArgumentException>(() => JsonConvert.SerializeObject(new Version(1, 0)));
            string json = JsonConvert.SerializeObject(new Version(1, 0));
            Assert.Equal(@"{""Major"":1,""Minor"":0,""Build"":-1,""Revision"":-1,""MajorRevision"":-1,""MinorRevision"":-1}", json);
        }

        [Test]
        public void GuidToString()
        {
            Guid guid = new Guid("BED7F4EA-1A96-11d2-8F08-00A0C9A6186D");
            string json = JsonConvert.SerializeObject(guid);
            Assert.Equal(@"""bed7f4ea-1a96-11d2-8f08-00a0c9a6186d""", json);
        }

        [Test]
        public void EnumToString()
        {
            string json = JsonConvert.SerializeObject(StringComparison.CurrentCultureIgnoreCase);
            Assert.Equal("1", json);
        }

        [Test]
        public void ObjectToString()
        {
            object? value;

            value = 1;
            Assert.Equal("1", JsonConvert.SerializeObject(value));

            value = 1.1;
            Assert.Equal("1.1", JsonConvert.SerializeObject(value));

            value = 1.1m;
            Assert.Equal("1.1", JsonConvert.SerializeObject(value));

            value = (float)1.1;
            Assert.Equal("1.1", JsonConvert.SerializeObject(value));

            value = (short)1;
            Assert.Equal("1", JsonConvert.SerializeObject(value));

            value = (long)1;
            Assert.Equal("1", JsonConvert.SerializeObject(value));

            value = (byte)1;
            Assert.Equal("1", JsonConvert.SerializeObject(value));

            value = (uint)1;
            Assert.Equal("1", JsonConvert.SerializeObject(value));

            value = (ushort)1;
            Assert.Equal("1", JsonConvert.SerializeObject(value));

            value = (sbyte)1;
            Assert.Equal("1", JsonConvert.SerializeObject(value));

            value = (ulong)1;
            Assert.Equal("1", JsonConvert.SerializeObject(value));

            const long initialJavaScriptDateTicks = 621355968000000000;

            value = new DateTime(initialJavaScriptDateTicks, DateTimeKind.Utc);
            Assert.Equal(@"""1970-01-01T00:00:00Z""", JsonConvert.SerializeObject(value));

            value = new DateTimeOffset(initialJavaScriptDateTicks, TimeSpan.Zero);
            Assert.Equal(@"""1970-01-01T00:00:00+00:00""", JsonConvert.SerializeObject(value));

            value = null;
            Assert.Equal("null", JsonConvert.SerializeObject(value));

            value = DBNull.Value;
            Assert.Equal("null", JsonConvert.SerializeObject(value));

            value = "I am a string";
            Assert.Equal(@"""I am a string""", JsonConvert.SerializeObject(value));

            value = true;
            Assert.Equal("true", JsonConvert.SerializeObject(value));

            value = 'c';
            Assert.Equal(@"""c""", JsonConvert.SerializeObject(value));
        }

        [Test]
        public void TestInvalidStrings()
        {
            Assert.Throws<JsonException>(() =>
            {
                string orig = @"this is a string ""that has quotes"" ";

                string serialized = JsonConvert.SerializeObject(orig);

                // *** Make string invalid by stripping \" \"
                serialized = serialized.Replace(@"\""", "\"", StringComparison.Ordinal);

                JsonConvert.DeserializeObject<string>(serialized);
            });
        }

        [Test]
        public void DeserializeValueObjects()
        {
            int i = JsonConvert.DeserializeObject<int>("1");
            Assert.Equal(1, i);

            DateTimeOffset d = JsonConvert.DeserializeObject<DateTimeOffset>(@"""0100-01-01T01:01:01Z""");
            Assert.Equal(new DateTimeOffset(new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc)), d);

            bool b = JsonConvert.DeserializeObject<bool>("true");
            Assert.True(b);

            object? n = JsonConvert.DeserializeObject<object>("null");
            Assert.Null(n);

            Assert.Throws<JsonException>(() => JsonConvert.DeserializeObject<object>("undefined"));
        }

        [Test]
        public void FloatToString()
        {
            Assert.Equal("1.1", JsonConvert.SerializeObject(1.1));
            Assert.Equal("1.11", JsonConvert.SerializeObject(1.11));
            Assert.Equal("1.111", JsonConvert.SerializeObject(1.111));
            Assert.Equal("1.1111", JsonConvert.SerializeObject(1.1111));
            Assert.Equal("1.11111", JsonConvert.SerializeObject(1.11111));
            Assert.Equal("1.111111", JsonConvert.SerializeObject(1.111111));
            //Assert.Equal("1.0", JsonConvert.SerializeObject(1.0));
            Assert.Equal("1", JsonConvert.SerializeObject(1.0));
            //Assert.Equal("1.0", JsonConvert.SerializeObject(1d));
            Assert.Equal("1", JsonConvert.SerializeObject(1d));
            //Assert.Equal("-1.0", JsonConvert.SerializeObject(-1d));
            Assert.Equal("-1", JsonConvert.SerializeObject(-1d));
            Assert.Equal("1.01", JsonConvert.SerializeObject(1.01));
            Assert.Equal("1.001", JsonConvert.SerializeObject(1.001));
            //Assert.Equal("Infinity", JsonConvert.SerializeObject(double.PositiveInfinity));
            Assert.Throws<JsonException>(() => JsonConvert.SerializeObject(double.PositiveInfinity));
            //Assert.Equal("-Infinity", JsonConvert.SerializeObject(double.NegativeInfinity));
            Assert.Throws<JsonException>(() => JsonConvert.SerializeObject(double.NegativeInfinity));
            //Assert.Equal("NaN", JsonConvert.SerializeObject(double.NaN));
            Assert.Throws<JsonException>(() => JsonConvert.SerializeObject(double.NaN));
        }

        [Test]
        public void DecimalToString()
        {
            Assert.Equal("1.1", JsonConvert.SerializeObject(1.1m));
            Assert.Equal("1.11", JsonConvert.SerializeObject(1.11m));
            Assert.Equal("1.111", JsonConvert.SerializeObject(1.111m));
            Assert.Equal("1.1111", JsonConvert.SerializeObject(1.1111m));
            Assert.Equal("1.11111", JsonConvert.SerializeObject(1.11111m));
            Assert.Equal("1.111111", JsonConvert.SerializeObject(1.111111m));
            Assert.Equal("1.0", JsonConvert.SerializeObject(1.0m));
            Assert.Equal("-1.0", JsonConvert.SerializeObject(-1.0m));
            //Assert.Equal("-1.0", JsonConvert.SerializeObject(-1m));
            //Assert.Equal("1.0", JsonConvert.SerializeObject(1m));
            Assert.Equal("1.01", JsonConvert.SerializeObject(1.01m));
            Assert.Equal("1.001", JsonConvert.SerializeObject(1.001m));
            //Assert.Equal("79228162514264337593543950335.0", JsonConvert.SerializeObject(Decimal.MaxValue));
            //Assert.Equal("-79228162514264337593543950335.0", JsonConvert.SerializeObject(Decimal.MinValue));
            Assert.Equal("79228162514264337593543950335", JsonConvert.SerializeObject(decimal.MaxValue));
            Assert.Equal("-79228162514264337593543950335", JsonConvert.SerializeObject(decimal.MinValue));
        }

        [Test]
        public void StringEscaping()
        {
            string v = "It's a good day\r\n\"sunshine\"";

            string json = JsonConvert.SerializeObject(v);
            Assert.Equal(@"""It's a good day\r\n\""sunshine\""""", json);
        }

        [Test]
        public void SerializeObjectDateTimeZoneHandling()
        {
            string json = JsonConvert.SerializeObject(
                new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified));

            Assert.Equal(@"""2000-01-01T01:01:01""", json);
        }

        [Test]
        public void DeserializeObject()
        {
            string json = @"{
        ""Name"": ""Bad Boys"",
        ""ReleaseDate"": ""1995-04-07T00:00:00"",
        ""Genres"": [
          ""Action"",
          ""Comedy""
        ]
      }";

            Movie? m = JsonConvert.DeserializeObject<Movie>(json);
            Assert.Equal("Bad Boys", m?.Name);
        }

        public class Movie
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? Classification { get; set; }
            public string? Studio { get; set; }
            public DateTime? ReleaseDate { get; set; }
            public List<string>? ReleaseCountries { get; set; }
        }

        [Test]
        public void GenericBaseClassSerialization()
        {
            string json = JsonConvert.SerializeObject(new NonGenericChildClass());
            Assert.Equal("{\"Data\":null}", json);
        }

        public class GenericBaseClass<T1, T2>
        {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            public virtual T2 Data { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        }

        public class GenericIntermediateClass<T1> : GenericBaseClass<T1, string>
        {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            public override string Data { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        }

        public class NonGenericChildClass : GenericIntermediateClass<int>
        {
        }
    }
}