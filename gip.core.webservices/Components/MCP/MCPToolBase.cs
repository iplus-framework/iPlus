// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gip.core.webservices
{
    public abstract class MCPToolBase
    {
        #region Properties
        protected ACMonitorObject _80000_Lock = new ACMonitorObject(80000);

        protected IACComponent _ACRoot;
        #endregion

        #region Methods
        public string[] SplitParamsToArray(string param)
        {
            string[] arr = null;
            if (!string.IsNullOrEmpty(param))
            {
                arr =
                param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(id => id.Trim())
               .Where(id => !string.IsNullOrEmpty(id))
               .ToArray();
            }
            return arr;
        }

        public object[] ConvertJsonParametersToObjects(JsonElement[] jsonParams)
        {
            if (jsonParams == null || jsonParams.Length == 0)
                return null;

            var parameters = new object[jsonParams.Length];

            for (int i = 0; i < jsonParams.Length; i++)
            {
                var param = jsonParams[i];

                parameters[i] = param.ValueKind switch
                {
                    JsonValueKind.String => param.GetString(),
                    JsonValueKind.Number => param.TryGetInt32(out int intVal) ? intVal : param.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => param.ToString()
                };
            }

            return parameters;
        }

        public List<KeyValuePair<string, object>> ConvertJsonToKeyValuePairs(string parametersJson)
        {
            if (string.IsNullOrEmpty(parametersJson))
                return null;

            try
            {
                using var document = JsonDocument.Parse(parametersJson);
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    return null; // Only process JSON objects for key-value pairs
                }

                var keyValuePairs = new List<KeyValuePair<string, object>>();

                foreach (var property in root.EnumerateObject())
                {
                    var value = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => (object)property.Value.GetString(),
                        JsonValueKind.Number => property.Value.TryGetInt32(out int intVal) ? (object)intVal : property.Value.GetDouble(),
                        JsonValueKind.True => (object)true,
                        JsonValueKind.False => (object)false,
                        JsonValueKind.Null => null,
                        _ => (object)property.Value.ToString()
                    };

                    keyValuePairs.Add(new KeyValuePair<string, object>(property.Name, value));
                }

                return keyValuePairs;
            }
            catch
            {
                // If JSON parsing fails, return null
                return null;
            }
        }

        public List<KeyValuePair<string, object>> ConvertJsonParametersToObjects(string parametersJson)
        {
            if (string.IsNullOrEmpty(parametersJson))
                return null;

            try
            {
                using var document = JsonDocument.Parse(parametersJson);
                var root = document.RootElement;
                List<KeyValuePair<string, object>> keyValuePairs = null;

                // Check if it's a JSON array
                if (root.ValueKind == JsonValueKind.Array)
                {
                    keyValuePairs = new List<KeyValuePair<string, object>>();
                    var jsonParams = new JsonElement[root.GetArrayLength()];
                    int index = 0;
                    foreach (var element in root.EnumerateArray())
                    {
                        jsonParams[index++] = element;
                    }
                    object[] parameterObjArr = ConvertJsonParametersToObjects(jsonParams);
                    if (parameterObjArr != null && parameterObjArr.Length > 0)
                    {
                        for (int i = 0; i < parameterObjArr.Length; i++)
                        {
                            keyValuePairs.Add(new KeyValuePair<string, object>(i.ToString(), parameterObjArr[i]));
                        }
                    }
                }
                // Check if it's a JSON object (key-value pairs) - extract values only
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    keyValuePairs = ConvertJsonToKeyValuePairs(parametersJson);
                }
                else
                {
                    // Handle single value case
                    object singleValue = root.ValueKind switch
                    {
                        JsonValueKind.String => root.GetString(),
                        JsonValueKind.Number => root.TryGetInt32(out int intVal) ? (object)intVal : root.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => root.ToString()
                    };
                    keyValuePairs = new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("0", singleValue) };
                }
                return keyValuePairs;
            }
            catch
            {
                // If JSON parsing fails, return null
                return null;
            }
        }

        public object ConvertResultToJson(object result)
        {
            if (result == null)
                return null;

            // Handle common types
            if (result is string || result is bool || result.GetType().IsPrimitive)
                return result;

            // For complex objects, try to convert to XML first, then to a readable format
            try
            {
                if (result is IACObjectWithInit)
                {
                    // For ACObjects, return basic information
                    var acObject = result as IACObjectWithInit;
                    return new
                    {
                        ACIdentifier = acObject.ACIdentifier,
                        ACCaption = acObject.ACCaption,
                        TypeName = acObject.GetType().Name,
                        ACUrl = acObject.GetACUrl()
                    };
                }

                Type type = result.GetType();

                if (  !type.IsEnum
                    && (   (!type.IsGenericType && typeof(VBEntityObject).IsAssignableFrom(type))
                        || (type.IsGenericType && typeof(VBEntityObject).IsAssignableFrom(type.GetGenericArguments()[0]))
                        )
                   )
                {
                    string jsonResult = JsonSerializer.Serialize(result, SerializerOptions);
                    if (!string.IsNullOrEmpty(jsonResult))
                        return jsonResult;
                }

                // Try XML serialization for other objects
                string xmlResult = ACConvert.ObjectToXML(result, true);
                return new { XmlData = xmlResult, TypeName = result.GetType().Name };
            }
            catch
            {
                // Fallback to string representation
                return new { StringValue = result.ToString(), TypeName = result.GetType().Name };
            }
        }

        public string CreateExceptionResponse(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            Exception tmpEc = ex;
            while (tmpEc != null)
            {
                sb.AppendLine(tmpEc.Message);
                tmpEc = tmpEc.InnerException;
            }

            return JsonSerializer.Serialize(new MCP_Exception() { Error = sb.ToString(), Success = false },
                                            new JsonSerializerOptions { WriteIndented = false });
        }

        public object[] ConvertKVPValues(ACComponent aCComponent, string acMethodName, List<KeyValuePair<string, object>> kvpParams)
        {
            string acMethodName1;
            ACClassMethod acClassMethod = aCComponent.GetACClassMethod(acMethodName, out acMethodName1);
            if (acClassMethod == null || String.IsNullOrEmpty(acMethodName1))
                return kvpParams.Select(c => c.Value).ToArray();

            ACMethod acMethod = acClassMethod.TypeACSignature();
            if (acMethod == null)
                return kvpParams.Select(c => c.Value).ToArray();
            bool passACMethodAsParam = acClassMethod.IsParameterACMethod || (acClassMethod.BasedOnACClassMethod != null && acClassMethod.BasedOnACClassMethod.IsParameterACMethod);
            int i = 0;
            bool hasKeys = true;
            List<object> convParams = new List<object>();
            foreach (KeyValuePair<string, object> kvp in kvpParams)
            {
                if (i == 0 && kvp.Key == "0")
                    hasKeys = false;

                if (!hasKeys)
                {
                    if (kvp.Value is string)
                    {
                        ACValue acValue = acMethod.ParameterValueList[i];
                        if (acValue != null)
                            convParams.Add(ACConvert.XMLToObject(acValue.ObjectFullType, kvp.Value as string, true, aCComponent.Database.ContextIPlus));
                    }
                    else
                        convParams.Add(kvp.Value);
                }
                else
                {
                    ACValue acValue = acMethod.ParameterValueList.GetACValue(kvp.Key);
                    if (acValue != null)
                    {
                        if (kvp.Value is string)
                        {
                            if (passACMethodAsParam)
                                acValue.Value = ACConvert.XMLToObject(acValue.ObjectFullType, kvp.Value as string, true, aCComponent.Database.ContextIPlus);
                            else
                                convParams.Add(ACConvert.XMLToObject(acValue.ObjectFullType, kvp.Value as string, true, aCComponent.Database.ContextIPlus));
                        }
                        else
                        {
                            if (passACMethodAsParam)
                                acValue.Value = kvp.Value;
                            else
                                convParams.Add(kvp.Value);
                        }
                    }
                    else
                    {
                        // If the key does not match any parameter, just add the value as is
                        convParams.Add(kvp.Value);
                    }
                }
                i++;
            }

            if (passACMethodAsParam)
                convParams.Add(acMethod);

            return convParams.ToArray();
        }

        public object[] ConvertBulkValues(ACComponent aCComponent, string acMethodName, IEnumerable<string> bulkValues)
        {
            string acMethodName1;
            ACClassMethod acClassMethod = aCComponent.GetACClassMethod(acMethodName, out acMethodName1);
            if (acClassMethod == null || String.IsNullOrEmpty(acMethodName1))
                return bulkValues.Select(c => (object)c).ToArray();

            ACMethod acMethod = acClassMethod.TypeACSignature();
            if (acMethod == null)
                return bulkValues.Select(c => (object)c).ToArray();
            bool passACMethodAsParam = acClassMethod.IsParameterACMethod || (acClassMethod.BasedOnACClassMethod != null && acClassMethod.BasedOnACClassMethod.IsParameterACMethod);
            int i = 0;
            bool hasKeys = true;
            List<object> convParams = new List<object>();
            foreach (string bulkValue in bulkValues)
            {
                if (!hasKeys)
                {
                    ACValue acValue = acMethod.ParameterValueList[i];
                    if (acValue != null)
                        convParams.Add(ACConvert.XMLToObject(acValue.ObjectFullType, bulkValue, true, aCComponent.Database.ContextIPlus));
                }
                i++;
            }

            if (passACMethodAsParam)
                convParams.Add(acMethod);

            return convParams.ToArray();
        }

        private static JsonSerializerOptions _SerializerOptions;
        public static JsonSerializerOptions SerializerOptions
        {
            get
            {
                if (_SerializerOptions != null)
                    return _SerializerOptions;
                _SerializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    //MaxDepth = 2
                };
                _SerializerOptions.Converters.Add(new ACPropertyJsonConverterFactory());
                return _SerializerOptions;
            }
        }

        #endregion

    }

}