A .NET library that assists with migrating from [Json.NET](https://github.com/JamesNK/Newtonsoft.Json) to [System.Text.Json](https://docs.microsoft.com/en-us/dotnet/api/system.text.json).

![Build](https://github.com/22222/JsonNetMigrate/workflows/Build/badge.svg)

# Overview
This library provides a class that implements a subset of the Json.NET JsonConvert class's API.  It also provides some JsonConverter classes to use with the System.Text.Json library to make it act more like Json.NET.

The goal of this library is just to make it easier to migrate to System.Text.Json.  What's available should mostly be compatible with Json.NET, but it is missing a lot of features that Json.NET provides.


# Installation
You have a couple options for installing this library:

- Install the [NuGet package](https://www.nuget.org/packages/JsonNetMigrate/)
- Copy the source code directly into your project

This project is available under either of two licenses: the [Json.NET MIT license](LICENSE) or the [Unlicense](UNLICENSE).  The goal is to allow you to copy any of the source code from this library into your own project without having to worry about attribution or any other licensing complexity.

Note that the API and some comments are based on the Json.NET library, and some test files that are based on Json.NET code have their own License comment header.


# Getting Started

Serializing an object works just like Json.NET with the JsonConvert.SerializeObject method:

```c#
using JsonNetMigrate.Json;

var value = new { a = 1, b = "two" };
var json = JsonConvert.SerializeObject(value);

Console.WriteLine(json);
```

```json
{"a":1,"b":"two"}
```

This library also provides a subset of the JsonSerializerSettings class for customizing the Json output:

```c#
using JsonNetMigrate.Json;

var value = new Dictionary<string, object?> { ["a"] = 1.0, ["b"] = StringComparison.Ordinal, ["c"] = new DateTime(2002, 2, 2) };
var json = JsonConvert.SerializeObject(value, new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    NullValueHandling = NullValueHandling.Ignore,
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
    Converters = new[] { new StringEnumConverter() },
});

Console.WriteLine(json);
```

```json
{
  "a": 1,
  "b": "Ordinal",
  "c": "2002-02-02T00:00:00"
}
```

You can also parse a Json string back into objects with the JsonConvert.DeserializeObject method:

```c#
using JsonNetMigrate.Json;

var json = @"{""Formatting"":1,""NullValueHandling"":1}";
var value = JsonConvert.DeserializeObject<JsonSerializerSettings>(json);

Console.WriteLine($"Formatting = {value!.Formatting}");
Console.WriteLine($"NullValueHandling = {value!.NullValueHandling}");
```

```text
Formatting = Indented
NullValueHandling = Ignore
```

The same JsonSerializerSettings class can be used for deserialization:

```c#
using JsonNetMigrate.Json;

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
```

```text
Key = a, Value = 1 (type System.Int64)
Key = b, Value = Ordinal (type System.String)
Key = c, Value =  (type null)
Key = d, Value = 2/2/2002 12:00:00 AM (type System.DateTime)
```


# Limitations

This library does not add support for the Json.NET serialization attributes (Newtonsoft.Json.JsonPropertyAttribute) or the DataMember attributes.  That might implemented in the future after System.Text.Json version 5+ is released if it's made easier to implement [object and collection converters](https://github.com/dotnet/runtime/issues/1562).

This library does not include JObject, JArray, or JToken.  This project uses `IDictionary<string, object?>` instead of JObject and `IList<object?>` instead of JArray.


# Converters

* [ObjectConverter](JsonNetMigrate/Converters/ObjectConverter.cs) - Parses Json for the System.Object type.
* [StringEnumConverter](JsonNetMigrate/Converters/StringEnumConverter.cs) - Adds support for the EnumMemberAttribute to the standard System.Text.Json converter.
* [DBNullConverter](JsonNetMigrate/Converters/DBNullConverter.cs) - Serializes DBNull as null.

The key to making this whole library work is the ObjectConverter.  As of System.Text.Json version 4.7, parsing to object just gets you a raw JsonElement object (even for Json numbers, booleans, and strings).  Using this ObjectConverter will get you much closer to the Json.NET behavior, with a couple differences:

- Objects are parsed to `Dictionary<string, object?>` (as opposed to the Json.NET JObject, which implements `IDictionary<string, JToken>`)
- Arrays are parsed to `List<object?>` (as opposed to the Json.NET JArray, which implements `IList<JToken>`)
