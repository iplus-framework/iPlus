// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using gip.core.datamodel;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.DataContracts;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.Collections.Generic;
using gip.core.autocomponent;


public interface IACPropertyJsonConverter
{
    ushort DetailLevel { get; set; }
    string[] RequestedFields { get; set; }
    Dictionary<string, gip.core.datamodel.ACClass> EntityTypes { get; set; }
}

public class ACPropertyJsonConverter<T> : JsonConverter<T>, IACPropertyJsonConverter where T : VBEntityObject
{
    private ushort _detailLevel;
    public ushort DetailLevel
    {
        get => _detailLevel;
        set
        {
            if (value < 0 || value > 3)
                throw new ArgumentOutOfRangeException(nameof(value), "Detail level must be between 0 and 3.");
            _detailLevel = value;
        }
    }

    private string[] _requestedFields;
    public string[] RequestedFields
    {
        get => _requestedFields;
        set
        {
            _requestedFields = value;
        }
    }

    public Dictionary<string, gip.core.datamodel.ACClass> EntityTypes { get; set; }


    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(VBEntityObject).IsAssignableFrom(typeToConvert);
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization not implemented");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        var type = value.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        EntityTypes.TryGetValue(type.Name, out ACClass entityType);
        Dictionary<string, gip.core.datamodel.ACClassProperty> acProperties = null;
        if (entityType != null)
            acProperties = entityType.Properties.ToDictionary<gip.core.datamodel.ACClassProperty, string, gip.core.datamodel.ACClassProperty>(p => p.ACIdentifier, p => p);

        foreach (var property in properties)
        {
            // Skip if property has IgnoreDataMember attribute
            if (property.GetCustomAttribute<IgnoreDataMemberAttribute>() != null)
                continue;

            // Skip if property has JsonIgnore attribute
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                continue;

            if (IgnoreNaming(property))
                continue;

            // Handle user-defined field selection (detailLevel = 3)
            if (DetailLevel == 3 && RequestedFields != null && RequestedFields.Count() > 0)
            {
                if (!ShouldIncludeProperty(property.Name, RequestedFields))
                    continue;
            }

            // Only include properties that have ACPropertyInfo attribute OR DataMember attribute
            //ACPropertyBase acPropertyInfo = property.GetCustomAttribute<ACPropertyBase>(true);

            ACClassProperty acPropertyInfo = null;
            acProperties?.TryGetValue(property.Name, out acPropertyInfo);
            var hasACPropertyInfo = acPropertyInfo != null;
            var hasDataMember = property.GetCustomAttribute<DataMemberAttribute>() != null;
            bool isCollection = false;
            bool isNavigationProp = IsNavigationProperty(property, out isCollection);

            // Apply detail level filtering
            if (!ShouldIncludePropertyByDetailLevel(property, acPropertyInfo, DetailLevel, isNavigationProp, isCollection, writer.CurrentDepth))
                continue;

            if (!hasACPropertyInfo && !hasDataMember && !isNavigationProp && !IsSimpleType(property.PropertyType))
                continue;

            // Skip navigation properties and complex objects that might cause circular references
            try
            {
                var propertyValue = property.GetValue(value);
                var propertyName = GetJsonPropertyName(property, options);

                if (propertyValue == null)
                {
                    if (options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
                    {
                        writer.WriteNull(propertyName);
                    }
                }
                else if (IsSimpleType(property.PropertyType))
                {
                    WriteSimpleProperty(writer, propertyName, propertyValue, options);
                }
                else if (property.PropertyType == typeof(string))
                {
                    writer.WriteString(propertyName, propertyValue.ToString());
                }
                else if (typeof(VBEntityObject).IsAssignableFrom(property.PropertyType))
                {
                    WriteComplexProperty(writer, propertyName, propertyValue, options, DetailLevel);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) &&
                         property.PropertyType != typeof(string) &&
                         property.PropertyType != typeof(byte[]))
                {
                    WriteCollectionProperty(writer, propertyName, propertyValue, options, DetailLevel);
                }
                else
                {
                    // For other complex types, write a simplified representation
                    writer.WriteString(propertyName, $"[{property.PropertyType.Name}]");
                }
            }
            catch (Exception ex)
            {
                // Log error and continue with next property
                writer.WriteString($"{property.Name}_Error", ex.Message);
            }
        }

        writer.WriteEndObject();
    }

    private HashSet<string> GetUserDefinedFields(JsonSerializerOptions options, List<KeyValuePair<string, object>> parametersKVP)
    {
        var detailLevel = DetailLevel;
        if (detailLevel != 3 || parametersKVP == null)
            return null;

        var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in parametersKVP)
        {
            if (kvp.Value is string fieldList)
            {
                // Handle comma-separated field list
                if (fieldList.Contains(','))
                {
                    var fieldNames = fieldList.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(f => f.Trim())
                                            .Where(f => !string.IsNullOrEmpty(f));
                    foreach (var field in fieldNames)
                        fields.Add(field);
                }
                else
                {
                    fields.Add(fieldList.Trim());
                }
            }
        }

        return fields.Count > 0 ? fields : null;
    }

    private bool ShouldIncludeProperty(string propertyName, string[] userDefinedFields)
    {
        return userDefinedFields.Contains(propertyName) ||
               userDefinedFields.Any(field => field.Contains($"{propertyName}.", StringComparison.OrdinalIgnoreCase));
    }

    private bool ShouldIncludePropertyByDetailLevel(PropertyInfo property, ACClassProperty acPropertyInfo, ushort detailLevel, bool isNavigationProp, bool isCollection, int currentDepth)
    {
        switch (detailLevel)
        {
            case 0: // Minimal
                return !isNavigationProp || currentDepth <= 1 || (acPropertyInfo != null && acPropertyInfo.SortIndex < 10);

            case 1: // First-degree relationships
                if (isNavigationProp && isCollection)
                    return currentDepth <= 1;
                if (isNavigationProp && !isCollection)
                    return currentDepth <= 2;
                return (acPropertyInfo != null && acPropertyInfo.SortIndex < 10);

            case 2: // Complete
                return currentDepth <= 3; // Prevent infinite recursion

            case 3: // User-defined - handled separately
                return true;

            default:
                return !isNavigationProp || currentDepth <= 1;
        }
    }

    private void WriteComplexProperty(Utf8JsonWriter writer, string propertyName, object value, JsonSerializerOptions options, ushort detailLevel)
    {
        if (value == null)
        {
            writer.WriteNull(propertyName);
            return;
        }

        // Apply detail level limits
        if ((detailLevel == 0 && writer.CurrentDepth >= 2) ||
            (detailLevel == 1 && writer.CurrentDepth >= 3) ||
            (writer.CurrentDepth >= 5)) // Hard limit to prevent stack overflow
        {
            writer.WriteString(propertyName, $"[{value.GetType().Name}]");
            return;
        }

        writer.WritePropertyName(propertyName);

        if (value is VBEntityObject entityObject)
        {
            // Create a new converter instance for the specific type
            var converterType = typeof(ACPropertyJsonConverter<>).MakeGenericType(value.GetType());
            var converter = (JsonConverter)Activator.CreateInstance(converterType);
            if (converter is IACPropertyJsonConverter aCPropertyJsonConverter)
            {
                aCPropertyJsonConverter.DetailLevel = DetailLevel;
                aCPropertyJsonConverter.RequestedFields = RequestedFields;
                aCPropertyJsonConverter.EntityTypes = EntityTypes;
            }

            // Use reflection to call the Write method
            var writeMethod = converterType.GetMethod("Write");
            writeMethod.Invoke(converter, new object[] { writer, value, options });
        }
        else
        {
            writer.WriteStringValue($"[{value.GetType().Name}]");
        }
    }

    private void WriteCollectionProperty(Utf8JsonWriter writer, string propertyName, object value, JsonSerializerOptions options, ushort detailLevel)
    {
        if (value == null)
        {
            writer.WriteNull(propertyName);
            return;
        }

        // Apply detail level limits for collections
        if ((detailLevel == 0 && writer.CurrentDepth >= 1) ||
            (detailLevel == 1 && writer.CurrentDepth >= 2) ||
            (writer.CurrentDepth >= 3))
        {
            writer.WriteString(propertyName, $"[Collection of {value.GetType().Name}]");
            return;
        }

        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();

        if (value is IEnumerable enumerable)
        {
            int count = 0;
            int maxItems = detailLevel switch
            {
                0 => 3,  // Minimal - very few items
                1 => 10, // First-degree - moderate items
                2 => 50, // Complete - many items
                3 => 10, // User-defined - moderate items
                _ => 10
            };

            foreach (var item in enumerable)
            {
                if (count >= maxItems)
                {
                    writer.WriteStringValue($"... and more items (showing {maxItems} of collection)");
                    break;
                }

                if (item == null)
                {
                    writer.WriteNullValue();
                }
                else if (IsSimpleType(item.GetType()))
                {
                    WriteSimpleValue(writer, item);
                }
                else if (typeof(VBEntityObject).IsAssignableFrom(item.GetType()))
                {
                    // For VBEntityObject items, create a simplified representation
                    writer.WriteStartObject();
                    writer.WriteString("Type", item.GetType().Name);

                    // Try to get an identifier property
                    var identifierProp = item.GetType().GetProperty("ACIdentifier");
                    if (identifierProp != null)
                    {
                        var identifierValue = identifierProp.GetValue(item);
                        writer.WriteString("ACIdentifier", identifierValue?.ToString() ?? "");
                    }

                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStringValue($"[{item.GetType().Name}]");
                }

                count++;
            }
        }

        writer.WriteEndArray();
    }

    private void WriteSimpleValue(Utf8JsonWriter writer, object value)
    {
        switch (value)
        {
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case short s:
                writer.WriteNumberValue(s);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case float f:
                writer.WriteNumberValue(f);
                break;
            case decimal dec:
                writer.WriteNumberValue(dec);
                break;
            case DateTime dt:
                writer.WriteStringValue(dt.ToString("O"));
                break;
            case Guid g:
                writer.WriteStringValue(g.ToString());
                break;
            case Enum e:
                writer.WriteStringValue(e.ToString());
                break;
            default:
                writer.WriteStringValue(value.ToString());
                break;
        }
    }

    private bool IsNavigationProperty(PropertyInfo property, out bool isCollection)
    {
        isCollection = false;
        if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
            && property.PropertyType.IsGenericType
            && property.PropertyType.GenericTypeArguments != null
            && property.PropertyType.GenericTypeArguments.Any()
            && typeof(VBEntityObject).IsAssignableFrom(property.PropertyType.GenericTypeArguments.FirstOrDefault()))
        {
            isCollection = true;
            return true;
        }

        // Check if it's another entity type
        if (typeof(VBEntityObject).IsAssignableFrom(property.PropertyType))
        {
            return true;
        }

        return false;
    }

    private bool IgnoreNaming(PropertyInfo property)
    {
        // Check for specific naming patterns that indicate navigation properties
        var propertyName = property.Name;
        if (propertyName.EndsWith("_IsLoaded"))
        {
            return true;
        }
        return false;
    }

    private bool IsSimpleType(Type type)
    {
        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType != null)
            type = nullableType;

        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid);
    }

    private void WriteSimpleProperty(Utf8JsonWriter writer, string propertyName, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case bool b:
                writer.WriteBoolean(propertyName, b);
                break;
            case int i:
                writer.WriteNumber(propertyName, i);
                break;
            case long l:
                writer.WriteNumber(propertyName, l);
                break;
            case short s:
                writer.WriteNumber(propertyName, s);
                break;
            case double d:
                writer.WriteNumber(propertyName, d);
                break;
            case float f:
                writer.WriteNumber(propertyName, f);
                break;
            case decimal dec:
                writer.WriteNumber(propertyName, dec);
                break;
            case DateTime dt:
                writer.WriteString(propertyName, dt.ToString("O")); // ISO 8601 format
                break;
            case Guid g:
                writer.WriteString(propertyName, g.ToString());
                break;
            case Enum e:
                writer.WriteString(propertyName, e.ToString());
                break;
            default:
                writer.WriteString(propertyName, value.ToString());
                break;
        }
    }

    private string GetJsonPropertyName(PropertyInfo property, JsonSerializerOptions options)
    {
        // Check for JsonPropertyName attribute
        var jsonPropertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        if (jsonPropertyName != null)
            return jsonPropertyName.Name;

        // Use the property naming policy if available
        return options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
    }
}

// Generic converter that works for any VBEntityObject
public class ACPropertyJsonConverterFactory : JsonConverterFactory
{
    private ushort _detailLevel;
    private string[] _requestedFields;
    private Dictionary<string, gip.core.datamodel.ACClass> _EntityTypes;


    public ACPropertyJsonConverterFactory(ushort detailLevel, string[] requestedFields, Dictionary<string, gip.core.datamodel.ACClass> entityTypes)
    {
        _detailLevel = detailLevel;
        _requestedFields = requestedFields;
        _EntityTypes = entityTypes;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(VBEntityObject).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ACPropertyJsonConverter<>).MakeGenericType(typeToConvert);
        var converter = (JsonConverter)Activator.CreateInstance(converterType);
        if (converter is IACPropertyJsonConverter aCPropertyJsonConverter)
        {
            aCPropertyJsonConverter.DetailLevel = _detailLevel;
            aCPropertyJsonConverter.RequestedFields = _requestedFields;
            aCPropertyJsonConverter.EntityTypes = _EntityTypes;
        }
        return converter;
    }
}

public class RawJsonConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(object);
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value is string jsonString && IsValidJson(jsonString))
        {
            // Write raw JSON without escaping
            using var document = JsonDocument.Parse(jsonString);
            document.RootElement.WriteTo(writer);
        }
        else
        {
            // Use default serialization
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }

    private static bool IsValidJson(string jsonString)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonString);
            return true;
        }
        catch
        {
            return false;
        }
    }
}