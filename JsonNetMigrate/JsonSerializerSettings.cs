using JsonNetMigrate.Json.Converters;
using JsonNetMigrate.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonNetMigrate.Json
{
    /// <summary>
    /// Options for <see cref="JsonConvert"/> based on the Newtonsoft.Json.JsonSerializerSettings class.
    /// </summary>
    public class JsonSerializerSettings
    {
        /// <summary>
        /// How JSON text output is formatted.
        /// The default value is <see cref="Formatting.None" />.
        /// </summary>
        public Formatting Formatting { get; set; } = Formatting.None;

        /// <summary>
        /// How null values are handled when writing JSON.
        /// The default value is <see cref="NullValueHandling.Include"/>.
        /// </summary>
        public NullValueHandling NullValueHandling { get; set; } = NullValueHandling.Include;

        /// <summary>
        /// How date formatted strings are parsed when reading JSON.
        /// The default value is <see cref="DateParseHandling.DateTime"/>.
        /// </summary>
        public DateParseHandling DateParseHandling { get; set; } = DateParseHandling.DateTime;

        /// <summary>
        /// How floating point numbers are parsed when reading JSON text.
        /// The default value is <see cref="FloatParseHandling.Double"/>.
        /// </summary>
        public FloatParseHandling FloatParseHandling { get; set; } = FloatParseHandling.Double;

        /// <summary>
        /// The contract resolver used when reading and writing JSON.
        /// </summary>
        public DefaultContractResolver? ContractResolver { get; set; }

        /// <summary>
        /// A JsonConverter collection that will be used during when reading and writing JSON.
        /// </summary>
        public IList<JsonConverter> Converters { get; set; } = new List<JsonConverter>();

        /// <summary>
        /// Returns a <see cref="System.Text.Json.JsonSerializerOptions"/> object equivalent to this.
        /// </summary>
        /// <returns>The options.</returns>
        public JsonSerializerOptions ToJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.IgnoreNullValues = NullValueHandling == NullValueHandling.Ignore;
            options.WriteIndented = Formatting == Formatting.Indented;
            options.PropertyNamingPolicy = ContractResolver?.NamingStrategy?.ToPropertyNamingPolicy();
            options.DictionaryKeyPolicy = ContractResolver?.NamingStrategy?.ToDictionaryKeyPolicy();
            options.ReadCommentHandling = JsonCommentHandling.Skip;
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;

            if (Converters != null)
            {
                foreach (var converter in Converters)
                {
                    options.Converters.Add(converter);
                }
            }

            options.Converters.Add(new ObjectConverter(
                floatParseHandling: FloatParseHandling,
                dateParseHandling: DateParseHandling
            ));
            options.Converters.Add(new DBNullConverter());

            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            return options;
        }
    }

    /// <summary>
    /// Specifies formatting options when writing JSON text.
    /// </summary>
    public enum Formatting
    {
        /// <summary>
        /// No special formatting is applied.
        /// </summary>
        None = 0,

        /// <summary>
        /// JSON is formatted with pretty printing.
        /// </summary>
        Indented = 1,
    }

    /// <summary>
    /// Specifies how null values are handled when writing JSON text.
    /// </summary>
    public enum NullValueHandling
    {
        /// <summary>
        /// Include null values.
        /// </summary>
        Include = 0,

        /// <summary>
        /// Ignore null values.
        /// </summary>
        Ignore = 1,
    }

    /// <summary>
    /// Specifies how date formatted strings are parsed when reading JSON text.
    /// </summary>
    public enum DateParseHandling
    {
        /// <summary>
        /// Date formatted strings are are treated like normal strings with no special date parsing.
        /// </summary>
        None = 0,

        /// <summary>
        /// Date formatted strings are parsed to <see cref="System.DateTime"/>.
        /// </summary>
        DateTime = 1,

        /// <summary>
        /// Date formatted strings are parsed to <see cref="System.DateTimeOffset"/>.
        /// </summary>
        DateTimeOffset = 2,
    }

    /// <summary>
    /// Specifies how floating point numbers are parsed when reading JSON text.
    /// </summary>
    public enum FloatParseHandling
    {
        /// <summary>
        /// Floating point numbers are parsed to <see cref="double"/>.
        /// </summary>
        Double = 0,

        /// <summary>
        /// Floating point numbers are parsed to <see cref="decimal"/>.
        /// </summary>
        Decimal = 1,
    }

    namespace Serialization
    {
        /// <summary>
        /// Resolves property names using camel-casing.
        /// </summary>
        public class CamelCasePropertyNamesContractResolver : DefaultContractResolver
        {
            /// <summary>
            /// Constructs a default <see cref="CamelCasePropertyNamesContractResolver"/>.
            /// </summary>
            public CamelCasePropertyNamesContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true,
                    OverrideSpecifiedNames = true,
                };
            }
        }

        /// <summary>
        /// Used to resolve a JSON contract for a given type.
        /// This is a very limited subset of the real Newtonsoft.Json.Serialization.DefaultContractResolver class.
        /// </summary>
        public class DefaultContractResolver
        {
            /// <summary>
            /// The naming strategy used to resolve how property names and dictionary keys are serialized.
            /// </summary>
            public NamingStrategy? NamingStrategy { get; set; }
        }

        /// <summary>
        /// A camel case naming strategy.
        /// </summary>
        public sealed class CamelCaseNamingStrategy : NamingStrategy
        {
            /// <summary>
            /// Constructs a default <see cref="CamelCaseNamingStrategy"/>.
            /// </summary>
            public CamelCaseNamingStrategy()
            {
            }

            /// <summary>
            /// Constructs a <see cref="CamelCaseNamingStrategy"/> with the given options.
            /// </summary>
            /// <param name="processDictionaryKeys">Whether dictionary keys should be processed.</param>
            /// <param name="overrideSpecifiedNames">Whether explicitly specified property names should be processed.</param>
            public CamelCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
            {
                ProcessDictionaryKeys = processDictionaryKeys;
                OverrideSpecifiedNames = overrideSpecifiedNames;
            }

            /// <inheritdoc />
            public override JsonNamingPolicy? ToPropertyNamingPolicy()
            {
                return JsonNamingPolicy.CamelCase;
            }
        }

        /// <summary>
        /// A base class for resolving how property names and dictionary keys are serialized.
        /// </summary>
        public abstract class NamingStrategy
        {
            /// <summary>
            /// Whether dictionary keys should be processed.
            /// Defaults to false.
            /// </summary>
            public bool ProcessDictionaryKeys { get; set; }

            /// <summary>
            /// Wwhether explicitly specified property names should be processed.
            /// Defaults to false.
            /// </summary>
            public bool OverrideSpecifiedNames { get; set; }

            /// <summary>
            /// Gets the serialized name for a given property name.
            /// </summary>
            /// <param name="name">The initial property name.</param>
            /// <param name="hasSpecifiedName">A flag indicating whether the property has had a name explicitly specified.</param>
            /// <returns>The serialized property name.</returns>
            public virtual string GetPropertyName(string name, bool hasSpecifiedName)
            {
                if (hasSpecifiedName && !OverrideSpecifiedNames)
                {
                    return name;
                }

                return ResolvePropertyName(name);
            }

            /// <summary>
            /// Resolves the specified property name.
            /// </summary>
            /// <param name="name">The property name to resolve.</param>
            /// <returns>The resolved property name.</returns>
            protected virtual string ResolvePropertyName(string name)
            {
                return ToPropertyNamingPolicy()?.ConvertName(name) ?? name;
            }

            /// <summary>
            /// Gets the serialized key for a given dictionary key.
            /// </summary>
            /// <param name="key">The initial dictionary key.</param>
            /// <returns>The serialized dictionary key.</returns>
            public virtual string GetDictionaryKey(string key)
            {
                if (!ProcessDictionaryKeys)
                {
                    return key;
                }

                return ResolvePropertyName(key);
            }

            /// <summary>
            /// Returns a <see cref="System.Text.Json.JsonNamingPolicy"/> equivalent to these settings for property names.
            /// </summary>
            /// <returns>The naming policy.</returns>
            public abstract JsonNamingPolicy? ToPropertyNamingPolicy();

            /// <summary>
            /// Returns a <see cref="System.Text.Json.JsonNamingPolicy"/> equivalent to these settings for dictionary keys.
            /// </summary>
            /// <returns>The naming policy.</returns>
            public virtual JsonNamingPolicy? ToDictionaryKeyPolicy()
                => ProcessDictionaryKeys ? ToPropertyNamingPolicy() : null;
        }
    }
}
