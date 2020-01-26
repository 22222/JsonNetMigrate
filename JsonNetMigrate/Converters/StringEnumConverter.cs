using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonNetMigrate.Json.Converters
{
    /// <summary>
    /// An converter that extends <see cref="System.Text.Json.Serialization.JsonStringEnumConverter"/>
    /// to add support for <see cref="System.Runtime.Serialization.EnumMemberAttribute"/> for name overrides.
    /// </summary>
    public class StringEnumConverter : JsonConverterFactory
    {
        private readonly JsonNamingPolicy? namingPolicy;
        private readonly bool allowIntegerValues;

        /// <summary>
        /// Constructs a default <see cref="StringEnumConverter"/>.
        /// </summary>
        public StringEnumConverter()
            : this(namingPolicy: null) { }

        /// <summary>
        /// Constructs a <see cref="StringEnumConverter"/> with the specified naming policy for enum values.
        /// </summary>
        /// <param name="namingPolicy">The naming policy used to resolve how enum values are written.</param>
        public StringEnumConverter(JsonNamingPolicy? namingPolicy)
            : this(namingPolicy: namingPolicy, allowIntegerValues: true) { }

        /// <summary>
        /// Constructs a <see cref="StringEnumConverter"/> with the specified options.
        /// </summary>
        /// <param name="namingPolicy">The naming policy used to resolve how enum values are written.</param>
        /// <param name="allowIntegerValues">Whether integers are allowed when reading.</param>
        public StringEnumConverter(JsonNamingPolicy? namingPolicy, bool allowIntegerValues)
        {
            this.namingPolicy = namingPolicy;
            this.allowIntegerValues = allowIntegerValues;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == null) throw new ArgumentNullException(nameof(typeToConvert));

            return typeToConvert.IsEnum;
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converter = (JsonConverter)Activator.CreateInstance(
                type: typeof(JsonStringEnumConverter<>).MakeGenericType(typeToConvert),
                bindingAttr: BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object?[] { namingPolicy, allowIntegerValues },
                culture: null
            );
            return converter;
        }

        private sealed class JsonStringEnumConverter<T> : JsonConverter<T>
            where T : struct, Enum
        {
            private readonly JsonConverter<T> defaultConverter;
            private readonly JsonNamingPolicy readNamingPolicy;

            public JsonStringEnumConverter(JsonNamingPolicy? namingPolicy, bool allowIntegerValues)
            {
                var enumType = typeof(T);
                var writeNamingPolicy = new EnumNamingPolicy(enumType: enumType, fallbackNamingPolicy: namingPolicy, purpose: NamingPolicyPurpose.Write);
                var defaultConverterFactory = new JsonStringEnumConverter(namingPolicy: writeNamingPolicy, allowIntegerValues: allowIntegerValues);
                this.defaultConverter = (JsonConverter<T>)defaultConverterFactory.CreateConverter(typeToConvert: enumType, options: null);
                this.readNamingPolicy = new EnumNamingPolicy(enumType: enumType, fallbackNamingPolicy: null, purpose: NamingPolicyPurpose.Read);
            }

            /// <inheritdoc />
            public override bool CanConvert(Type type)
            {
                return defaultConverter.CanConvert(type);
            }

            /// <inheritdoc />
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                JsonTokenType tokenType = reader.TokenType;
                if (tokenType == JsonTokenType.String)
                {
                    string? name = reader.GetString();
                    string? transformedName = name != null ? readNamingPolicy.ConvertName(name) : null;
                    if (transformedName != null && !transformedName.Equals(name, StringComparison.Ordinal))
                    {
                        if (Enum.TryParse(transformedName, out T value)
                            || Enum.TryParse(transformedName, ignoreCase: true, result: out value))
                        {
                            return value;
                        }
                    }
                }

                return defaultConverter.Read(ref reader, typeToConvert, options);
            }

            /// <inheritdoc />
            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                defaultConverter.Write(writer, value, options);
            }
        }

        private sealed class EnumNamingPolicy : JsonNamingPolicy
        {
            private readonly JsonNamingPolicy? fallbackNamingPolicy;
            private readonly Lazy<ConcurrentDictionary<string, string>?> lazyNameTransformCache;
            private readonly Lazy<ConcurrentDictionary<string, string>?> lazyNameIgnoreCaseTransformCache;

            public EnumNamingPolicy(Type enumType, JsonNamingPolicy? fallbackNamingPolicy, NamingPolicyPurpose purpose)
            {
                this.fallbackNamingPolicy = fallbackNamingPolicy;
                this.lazyNameTransformCache = new Lazy<ConcurrentDictionary<string, string>?>(
                    () => CreateNameTransformCacheOrNull(enumType, purpose, ignoreCase: false)
                );
                this.lazyNameIgnoreCaseTransformCache = new Lazy<ConcurrentDictionary<string, string>?>(
                    () => purpose == NamingPolicyPurpose.Read ?
                        CreateNameTransformCacheOrNull(enumType, purpose, ignoreCase: true)
                        : null
                );
            }

            /// <inheritdoc />
            public override string ConvertName(string name)
            {
                var nameTransformCache = lazyNameTransformCache.Value;
                if (nameTransformCache != null && nameTransformCache.TryGetValue(name, out string? transformedName))
                {
                    return transformedName;
                }

                var nameIgnoreCaseTransformCache = lazyNameIgnoreCaseTransformCache.Value;
                if (nameIgnoreCaseTransformCache != null && nameIgnoreCaseTransformCache.TryGetValue(name, out string? transformedNameIgnoreCase))
                {
                    return transformedNameIgnoreCase;
                }

                return fallbackNamingPolicy?.ConvertName(name) ?? name;
            }

            private static ConcurrentDictionary<string, string>? CreateNameTransformCacheOrNull(Type enumType, NamingPolicyPurpose purpose, bool ignoreCase)
            {
                if (enumType == null) return null;
                if (!enumType.IsEnum) return null;

                string[] names = Enum.GetNames(enumType);
                ConcurrentDictionary<string, string>? nameTransformCache = null;
                foreach (var name in names)
                {
                    var fieldInfo = enumType.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                    if (fieldInfo == null)
                    {
                        continue;
                    }

                    var enumMemberAttribute = fieldInfo.GetCustomAttribute<EnumMemberAttribute>();
                    if (enumMemberAttribute == null)
                    {
                        continue;
                    }

                    var transformedName = enumMemberAttribute.Value;
                    if (transformedName != null && !transformedName.Equals(name, StringComparison.Ordinal))
                    {
                        if (nameTransformCache == null)
                        {
                            nameTransformCache = new ConcurrentDictionary<string, string>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
                        }

                        if (purpose == NamingPolicyPurpose.Read)
                        {
                            nameTransformCache[transformedName] = name;
                        }
                        else
                        {
                            nameTransformCache[name] = transformedName;
                        }
                    }
                }

                return nameTransformCache;
            }
        }

        private enum NamingPolicyPurpose
        {
            Write,
            Read,
        }
    }
}
