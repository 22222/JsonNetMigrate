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
    /// to add support for <see cref="System.Runtime.Serialization.EnumMemberAttribute"/> name overrides.
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
        public override bool CanConvert(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.IsEnum;
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converter = (JsonConverter)Activator.CreateInstance(
                type: typeof(GenericStringEnumConverter<>).MakeGenericType(typeToConvert),
                bindingAttr: BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object?[] { namingPolicy, allowIntegerValues },
                culture: null
            );
            return converter;
        }

        private sealed class GenericStringEnumConverter<T> : JsonConverter<T>
            where T : struct, Enum
        {
            private readonly Lazy<EnumNamingPolicyCache<T>> lazyEnumNamingPolicyCache;

            public GenericStringEnumConverter(JsonNamingPolicy? namingPolicy, bool allowIntegerValues)
            {
                this.lazyEnumNamingPolicyCache = new Lazy<EnumNamingPolicyCache<T>>(() => new EnumNamingPolicyCache<T>(namingPolicy, allowIntegerValues));
            }

            /// <inheritdoc />
            public override bool CanConvert(Type type)
            {
                return type.IsEnum;
            }

            /// <inheritdoc />
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                JsonTokenType tokenType = reader.TokenType;
                if (tokenType == JsonTokenType.String)
                {
                    string? name = reader.GetString();
                    string? transformedName = name != null ? lazyEnumNamingPolicyCache.Value.ReadNamingPolicy.ConvertName(name) : null;
                    if (transformedName != null && !transformedName.Equals(name, StringComparison.Ordinal))
                    {
                        if (Enum.TryParse(transformedName, out T value)
                            || Enum.TryParse(transformedName, ignoreCase: true, result: out value))
                        {
                            return value;
                        }
                    }
                }

                var defaultConverter = lazyEnumNamingPolicyCache.Value.DefaultJsonConverter;
                return defaultConverter.Read(ref reader, typeToConvert, options);
            }

            /// <inheritdoc />
            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                var defaultConverter = lazyEnumNamingPolicyCache.Value.DefaultJsonConverter;
                defaultConverter.Write(writer, value, options);
            }
        }

        private sealed class EnumNamingPolicyCache<T>
        {
            public EnumNamingPolicyCache(JsonNamingPolicy? defaultWriteNamingPolicy, bool allowIntegerValues)
            {
                var enumType = typeof(T);
                var enumNameCache = new EnumNameCache(enumType, defaultWriteNamingPolicy);
                ReadNamingPolicy = new EnumReadNamingPolicy(enumNameCache);

                var writeNamingPolicy = new EnumWriteNamingPolicy(enumNameCache, defaultWriteNamingPolicy);
                var defaultJsonConverterFactory = new JsonStringEnumConverter(namingPolicy: writeNamingPolicy, allowIntegerValues: allowIntegerValues);
                DefaultJsonConverter = (JsonConverter<T>)defaultJsonConverterFactory.CreateConverter(typeToConvert: enumType, options: null);
            }

            public JsonNamingPolicy ReadNamingPolicy { get; }

            public JsonConverter<T> DefaultJsonConverter { get; }
        }

        private sealed class EnumReadNamingPolicy : JsonNamingPolicy
        {
            private readonly EnumNameCache enumNameCache;

            public EnumReadNamingPolicy(EnumNameCache enumNameCache)
            {
                this.enumNameCache = enumNameCache ?? throw new ArgumentNullException(nameof(enumNameCache));
            }

            /// <inheritdoc />
            public override string ConvertName(string name)
            {
                var nameTransformDictionary = enumNameCache.ReadNameTransformDictionary;
                if (nameTransformDictionary != null && nameTransformDictionary.TryGetValue(name, out string? transformedName))
                {
                    return transformedName;
                }

                var nameIgnoreCaseTransformDictionary = enumNameCache.ReadNameIgnoreCaseTransformDictionary;
                if (nameIgnoreCaseTransformDictionary != null && nameIgnoreCaseTransformDictionary.TryGetValue(name, out string? transformedNameIgnoreCase))
                {
                    return transformedNameIgnoreCase;
                }

                return name;
            }
        }

        private sealed class EnumWriteNamingPolicy : JsonNamingPolicy
        {
            private readonly EnumNameCache enumNameCache;
            private readonly JsonNamingPolicy? defaultNamingPolicy;

            public EnumWriteNamingPolicy(EnumNameCache enumNameCache, JsonNamingPolicy? defaultNamingPolicy)
            {
                this.enumNameCache = enumNameCache ?? throw new ArgumentNullException(nameof(enumNameCache));
                this.defaultNamingPolicy = defaultNamingPolicy;
            }

            /// <inheritdoc />
            public override string ConvertName(string name)
            {
                var nameTransformDictionary = enumNameCache.WriteNameTransformDictionary;
                if (nameTransformDictionary != null && nameTransformDictionary.TryGetValue(name, out string? transformedName))
                {
                    return transformedName;
                }

                if (defaultNamingPolicy != null)
                {
                    return defaultNamingPolicy.ConvertName(name);
                }

                return name;
            }
        }

        private sealed class EnumNameCache
        {
            public EnumNameCache(Type enumType, JsonNamingPolicy? writeNamingPolicy)
            {
                if (enumType == null) throw new ArgumentNullException(nameof(enumType));
                if (!enumType.IsEnum) throw new ArgumentException($"Type {enumType.FullName} is not an enum type");

                string[] names = Enum.GetNames(enumType);
                foreach (var name in names)
                {
                    var fieldInfo = enumType.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                    if (fieldInfo == null)
                    {
                        continue;
                    }

                    string? explicitName;
                    var enumMemberAttribute = fieldInfo.GetCustomAttribute<EnumMemberAttribute>();
                    if (enumMemberAttribute != null && !string.IsNullOrEmpty(enumMemberAttribute.Value))
                    {
                        explicitName = enumMemberAttribute.Value;
                    }
                    else
                    {
                        explicitName = null;
                    }

                    string transformedName;
                    if (explicitName != null)
                    {
                        transformedName = explicitName;
                    }
                    else if (writeNamingPolicy != null)
                    {
                        transformedName = writeNamingPolicy.ConvertName(name);
                    }
                    else
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(transformedName))
                    {
                        continue;
                    }

                    if (!transformedName.Equals(name, StringComparison.Ordinal))
                    {
                        if (explicitName != null)
                        {
                            if (WriteNameTransformDictionary == null)
                            {
                                WriteNameTransformDictionary = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
                            }

                            WriteNameTransformDictionary[name] = explicitName;
                        }

                        if (ReadNameTransformDictionary == null)
                        {
                            ReadNameTransformDictionary = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
                        }

                        ReadNameTransformDictionary[transformedName] = name;
                    }

                    if (!transformedName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (ReadNameIgnoreCaseTransformDictionary == null)
                        {
                            ReadNameIgnoreCaseTransformDictionary = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        }

                        ReadNameIgnoreCaseTransformDictionary[transformedName] = name;
                    }
                }
            }

            public ConcurrentDictionary<string, string>? WriteNameTransformDictionary { get; }

            public ConcurrentDictionary<string, string>? ReadNameTransformDictionary { get; }

            public ConcurrentDictionary<string, string>? ReadNameIgnoreCaseTransformDictionary { get; }
        }
    }
}
