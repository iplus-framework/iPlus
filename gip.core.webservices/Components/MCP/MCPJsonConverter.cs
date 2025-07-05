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

public class ACPropertyJsonConverter<T> : JsonConverter<T> where T : VBEntityObject
{
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

            // Only include properties that have ACPropertyInfo attribute OR DataMember attribute
            var hasACPropertyInfo = property.GetCustomAttribute<ACPropertyBase>() != null;
            var hasDataMember = property.GetCustomAttribute<DataMemberAttribute>() != null;
            bool isCollection = false;
            bool isNavigationProp = IsNavigationProperty(property, out isCollection);

            if (isNavigationProp && (writer.CurrentDepth >= 3 || isCollection))
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
                    WriteComplexProperty(writer, propertyName, propertyValue, options);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && 
                         property.PropertyType != typeof(string) && 
                         property.PropertyType != typeof(byte[]))
                {
                    WriteCollectionProperty(writer, propertyName, propertyValue, options);
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

    private void WriteComplexProperty(Utf8JsonWriter writer, string propertyName, object value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNull(propertyName);
            return;
        }

        // Prevent deep recursion
        if (writer.CurrentDepth >= 3)
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
            
            // Use reflection to call the Write method
            var writeMethod = converterType.GetMethod("Write");
            writeMethod.Invoke(converter, new object[] { writer, value, options });
        }
        else
        {
            writer.WriteStringValue($"[{value.GetType().Name}]");
        }
    }

    private void WriteCollectionProperty(Utf8JsonWriter writer, string propertyName, object value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNull(propertyName);
            return;
        }

        // Prevent deep recursion for collections
        if (writer.CurrentDepth >= 2)
        {
            writer.WriteString(propertyName, $"[Collection of {value.GetType().Name}]");
            return;
        }

        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();

        if (value is IEnumerable enumerable)
        {
            int count = 0;
            const int maxItems = 10; // Limit collection size to prevent large JSON

            foreach (var item in enumerable)
            {
                if (count >= maxItems)
                {
                    writer.WriteStringValue($"... and more items");
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
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(VBEntityObject).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(ACPropertyJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType);
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