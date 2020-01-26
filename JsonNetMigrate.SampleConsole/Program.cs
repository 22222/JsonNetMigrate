using JsonNetMigrate.Json;
using JsonNetMigrate.Json.Converters;
using JsonNetMigrate.Json.Serialization;
using System;
using System.Collections.Generic;

namespace JsonNetMigrate.SampleConsole
{
    class Program
    {
        static void Main()
        {
            Sample1();
            Console.WriteLine();
            Sample2();
            Console.WriteLine();
            Sample3();
            Console.WriteLine();
            Sample4();
            Console.WriteLine();
        }

        private static void Sample1()
        {
            var value = new { a = 1, b = "two" };
            var json = JsonConvert.SerializeObject(value);

            Console.WriteLine(json);
        }

        private static void Sample2()
        {
            var value = new Dictionary<string, object?> { ["a"] = 1.0, ["b"] = StringComparison.Ordinal, ["c"] = null, ["d"] = new DateTime(2002, 2, 2) };
            var json = JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter() },
            });

            Console.WriteLine(json);
        }

        private static void Sample3()
        {
            var json = @"{""Formatting"":1,""NullValueHandling"":1}";
            var value = JsonConvert.DeserializeObject<JsonSerializerSettings>(json);

            Console.WriteLine($"Formatting = {value!.Formatting}");
            Console.WriteLine($"NullValueHandling = {value!.NullValueHandling}");
        }

        private static void Sample4()
        {
            var json = @"{""a"": 1.1,""b"": ""Ordinal"",""c"": null,""d"": ""2002-02-02T00:00:00""}";
            var value = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTimeOffset,
                FloatParseHandling = FloatParseHandling.Decimal,
            });

            var valueDictionary = (IDictionary<string, object>)value!;
            foreach (var kv in valueDictionary)
            {
                Console.WriteLine($"Key = {kv.Key}, Value = {kv.Value} (type {kv.Value?.GetType()?.FullName ?? "null"})");
            }
        }
    }
}
