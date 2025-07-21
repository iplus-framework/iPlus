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

        public const ushort DetailLevel_Minimal = 0;
        public const ushort DetailLevel_FirstDegree = 1;
        public const ushort DetailLevel_Complete = 2;
        public const ushort DetailLevel_UserDefined = 3;
        #endregion

        #region Methods
        public string[] SplitParamsToArray(string param)
        {
            string[] arr = null;
            if (!string.IsNullOrEmpty(param))
            {
                arr =
                param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(id => id.Trim().Replace("\"","").Replace("{", "").Replace("}", ""))
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


        // Updated code to handle custom options using the dictionary
        public object ConvertResultToJson(object result, ushort detailLevel, List<KeyValuePair<string, object>> parametersKVP = null, string[] bulkParams = null)
        {
            if (result == null)
                return null;

            if (result is string || result is bool || result.GetType().IsPrimitive)
                return result;

            try
            {
                string xmlResult = null;
                if (result is IACObjectWithInit)
                {
                    var acObject = result as IACObjectWithInit;
                    var acComponent = acObject as ACComponent;
                    if (acComponent != null)
                    {
                        int maxChildDepth = detailLevel switch
                        {
                            MCPToolBase.DetailLevel_Minimal => 1,
                            MCPToolBase.DetailLevel_FirstDegree => 2,
                            MCPToolBase.DetailLevel_Complete => 0,
                            3 => 0,
                            _ => 0
                        };

                        xmlResult = acComponent.DumpAsXMLString(maxChildDepth);
                        return new { XmlData = xmlResult, TypeName = result.GetType().Name };
                    }
                    else
                    {
                        return new
                        {
                            ACIdentifier = acObject.ACIdentifier,
                            ACCaption = acObject.ACCaption,
                            TypeName = acObject.GetType().Name,
                            ACUrl = acObject.GetACUrl()
                        };
                    }
                }

                Type type = result.GetType();

                bool serializeJSON = false;
                if (!type.IsEnum)
                {
                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                    {
                        // Collections must be minimum detail level 1 because Serializers needs two steps
                        if (detailLevel == 0)
                            detailLevel++;
                        while (type != null)
                        {
                            if ((!type.IsGenericType && (typeof(IACObjectKeyComparer).IsAssignableFrom(type) || typeof(IACObjectKeyComparer[]).IsAssignableFrom(type)))
                                || (type.IsGenericType && typeof(IACObjectKeyComparer).IsAssignableFrom(type.GetGenericArguments()[0])))
                            {
                                serializeJSON = true;
                                break;
                            }
                            type = type.BaseType;
                        }
                    }
                    else if ((!type.IsGenericType && typeof(IACObjectKeyComparer).IsAssignableFrom(type))
                                || (type.IsGenericType && typeof(IACObjectKeyComparer).IsAssignableFrom(type.GetGenericArguments()[0])))
                    {
                        serializeJSON = true;
                    }
                }

                if (serializeJSON)
                {
                    string[] requestedFields = null;
                    if (bulkParams != null && bulkParams.Length > 0)
                        requestedFields = bulkParams;
                    else if (parametersKVP != null && parametersKVP.Count > 0)
                    {
                        requestedFields = parametersKVP.Select(c => c.Key).ToArray();
                        if (requestedFields[0] == "0")
                            requestedFields = parametersKVP.Select(c => c.Value as string).ToArray();
                    }

                    JsonSerializerOptions customOptions = GetNewSerializerOptions(detailLevel, requestedFields, EntityTypes);
                    string jsonResult = JsonSerializer.Serialize(result, customOptions);
                    if (!string.IsNullOrEmpty(jsonResult))
                        return jsonResult;
                }

                xmlResult = ACConvert.ObjectToXML(result, true);
                return new { XmlData = xmlResult, TypeName = result.GetType().Name };
            }
            catch
            {
                return new { StringValue = result.ToString(), TypeName = result.GetType().Name };
            }
        }

        protected abstract Dictionary<string, gip.core.datamodel.ACClass> EntityTypes { get; }

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

        public object[] ConvertKVPValues(IACObjectWithInit acObj, string acMethodName, List<KeyValuePair<string, object>> kvpParams, int currCommandPos, int countACUrlCommands, StringBuilder sbErr, StringBuilder sbRec, out ACClassMethod acClassMethod)
        {
            string acMethodName1 = null;
            ACComponent aCComponent = acObj as ACComponent;
            acClassMethod = GetACClassMethod(acObj, acMethodName, out acMethodName1);
            if (acClassMethod == null || String.IsNullOrEmpty(acMethodName1))
            {
                throw new ArgumentException(String.Format("Methodname {0} doesn't exist. First, search for the correct method using get_method_info() and read the required parameter list of the method signature.", acMethodName));
                //return kvpParams.Select(c => c.Value).ToArray();
            }

            IACEntityObjectContext dbContext = aCComponent != null && aCComponent.Database != null ? aCComponent.Database : ACRoot.SRoot.Database.ContextIPlus;

            ACMethod acMethod = acClassMethod.TypeACSignature();
            if (acMethod == null)
                return kvpParams.Select(c => c.Value).ToArray();
            bool passACMethodAsParam = acClassMethod.IsParameterACMethod || (acClassMethod.BasedOnACClassMethod != null && acClassMethod.BasedOnACClassMethod.IsParameterACMethod);
            int i = 0;
            bool hasKeys = true;
            List<object> convParams = new List<object>();
            List<KeyValuePair<string, object>> kvpParams1 = kvpParams.ToList();
            List<object> dynParams = null;
            string dynParamsKey = null;
            // If bulk operations and for each method the LLM has passed separate parameterset, then split the parameters
            if (acMethod.ParameterValueList.Count < kvpParams.Count)
            {
                if (acMethod.ParameterValueList.Count == 1)
                {
                    if (acMethod.ParameterValueList[0].ObjectType == typeof(object[]))
                        return kvpParams.Select(c => c.Value).ToArray();
                }
                kvpParams1 = new List<KeyValuePair<string, object>>();
                int offset = currCommandPos * acMethod.ParameterValueList.Count;
                for (int j = 0; j < acMethod.ParameterValueList.Count; j++)
                {
                    if (dynParams == null && acMethod.ParameterValueList[j].ObjectType == typeof(object[]))
                    {
                        dynParams = new List<object>();
                        dynParamsKey = kvpParams[j + offset].Key;
                    }
                    if (dynParams != null)
                        dynParams.Add(kvpParams[j + offset].Value);
                    else
                        kvpParams1.Add(kvpParams[j + offset]);
                }
            }
            else if (acMethod.ParameterValueList.Count > kvpParams.Count)
            {
                throw new ArgumentException(String.Format("Too few parameters were passed {0}. The method {1} requires {2} parameters. Read the method signature and correct your parameter passing accordingly.", kvpParams.Count, acMethodName, acMethod.ParameterValueList.Count));
            }

            if (dynParams != null && !string.IsNullOrEmpty(dynParamsKey))
                kvpParams1.Add(new KeyValuePair<string, object>(dynParamsKey, dynParams.ToArray()));

            List<object> dynParams2 = null;
            foreach (KeyValuePair<string, object> kvp in kvpParams1)
            {
                if (i == 0 && kvp.Key == "0")
                    hasKeys = false;

                if (dynParams2 != null)
                {
                    dynParams2.Add(kvp.Value);
                    continue;
                }
                if (!hasKeys)
                {
                    if (kvp.Value is object[])
                    {
                        convParams.Add(kvp.Value);
                        break;
                    }
                    else if (kvp.Value is string)
                    {
                        ACValue acValue = acMethod.ParameterValueList[i];
                        if (acValue != null)
                        {
                            if (acValue.ObjectType == typeof(object[]))
                            {
                                dynParams2 = new List<object>();
                                dynParams2.Add(kvp.Value);
                                continue;
                            }
                            else
                                convParams.Add(ACConvert.XMLToObject(acValue.ObjectFullType, kvp.Value as string, true, dbContext));
                        }
                    }
                    else
                        convParams.Add(kvp.Value);
                }
                else
                {
                    ACValue acValue = acMethod.ParameterValueList.GetACValue(kvp.Key);
                    if (acValue != null)
                    {
                        if (acValue.ObjectType == typeof(object[]))
                        {
                            dynParams2 = new List<object>();
                            dynParams2.Add(kvp.Value);
                            continue;
                        }
                        else if (kvp.Value is object[])
                        {
                            convParams.Add(kvp.Value);
                            break;
                        }
                        else if (kvp.Value is string)
                        {
                            if (passACMethodAsParam)
                                acValue.Value = ACConvert.XMLToObject(acValue.ObjectFullType, kvp.Value as string, true, dbContext);
                            else
                                convParams.Add(ACConvert.XMLToObject(acValue.ObjectFullType, kvp.Value as string, true, dbContext));
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
                        if (sbRec == null)
                        {
                            sbRec = new StringBuilder();
                            sbRec.AppendLine(String.Format("You passed wrong parameter names! The method {0} requires the following parameters:", acMethodName));
                        }
                        if (kvp.Value is object[])
                        {
                            convParams.Add(kvp.Value);
                            break;
                        }
                        else if (kvp.Value is string)
                        {
                            acValue = acMethod.ParameterValueList[i];
                            if (acValue != null)
                            {
                                convParams.Add(ACConvert.XMLToObject(acValue.ObjectFullType, kvp.Value as string, true, dbContext));
                                sbRec.AppendLine(String.Format("Right Parametername is {0} and you passed {1}. This value {2} was used and the method call may fail.", acValue.ACIdentifier, kvp.Key, kvp.Value));
                            }
                        }
                        else
                            // If the key does not match any parameter, just add the value as is
                            convParams.Add(kvp.Value);
                    }
                }
                i++;
            }

            if (dynParams2 != null)
                convParams.Add(dynParams2.ToArray());

            if (passACMethodAsParam)
                convParams.Add(acMethod);

            return convParams.ToArray();
        }

        public object[] ConvertBulkValues(IACObjectWithInit acObj, string acMethodName, string[] bulkValues, int currCommandPos, int countACUrlCommands, StringBuilder sbErr, StringBuilder sbRec, out ACClassMethod acClassMethod)
        {
            string acMethodName1 = null;
            acClassMethod = GetACClassMethod(acObj, acMethodName, out acMethodName1);
            ACComponent aCComponent = acObj as ACComponent;
            acClassMethod = GetACClassMethod(acObj, acMethodName, out acMethodName1);
            if (acClassMethod == null || String.IsNullOrEmpty(acMethodName1))
            {
                throw new ArgumentException(String.Format("Methodname {0} doesn't exist. First, search for the correct method using get_method_info() and read the required parameter list of the method signature.", acMethodName));
                //return bulkValues.Select(c => (object)c).ToArray();
            }

            IACEntityObjectContext dbContext = aCComponent != null && aCComponent.Database != null ? aCComponent.Database : ACRoot.SRoot.Database.ContextIPlus;

            ACMethod acMethod = acClassMethod.TypeACSignature();
            if (acMethod == null)
                return bulkValues.Select(c => (object)c).ToArray();
            bool passACMethodAsParam = acClassMethod.IsParameterACMethod || (acClassMethod.BasedOnACClassMethod != null && acClassMethod.BasedOnACClassMethod.IsParameterACMethod);
            int i = 0;
            List<object> convParams = new List<object>();
            List<object> bulkValues1 = bulkValues.Select(c => (object)c).ToList();
            List<object> dynParams = null;
            // If bulk operations and for each method the LLM has passed separate parameterset, then split the parameters
            if (acMethod.ParameterValueList.Count < bulkValues.Count())
            {
                if (acMethod.ParameterValueList.Count == 1)
                {
                    if (acMethod.ParameterValueList[0].ObjectType == typeof(object[]))
                        return bulkValues1.ToArray();
                }

                bulkValues1 = new List<object>();
                int offset = currCommandPos * acMethod.ParameterValueList.Count;
                for (int j = 0; j < acMethod.ParameterValueList.Count; j++)
                {
                    if (dynParams == null && acMethod.ParameterValueList[j].ObjectType == typeof(object[]))
                        dynParams = new List<object>();
                    if (dynParams != null)
                        dynParams.Add(bulkValues[j + offset]);
                    else
                        bulkValues1.Add(bulkValues[j + offset]);
                }
            }
            else if (acMethod.ParameterValueList.Count > bulkValues.Count())
            {
                throw new ArgumentException(String.Format("Too few parameters were passed {0}. The method {1} requires {2} parameters. Read the method signature and correct your parameter passing accordingly.", bulkValues.Count(), acMethodName, acMethod.ParameterValueList.Count));
            }

            if (dynParams != null)
                convParams.Add(dynParams.ToArray());

            foreach (object bulkValue in bulkValues1)
            {
                if (bulkValue is object[])
                {
                    convParams.Add(bulkValue);
                    break;
                }
                ACValue acValue = acMethod.ParameterValueList[i];
                if (acValue != null)
                    convParams.Add(ACConvert.XMLToObject(acValue.ObjectFullType, bulkValue as string, true, dbContext.ContextIPlus));
                else
                    convParams.Add(bulkValue);
                i++;
            }

            if (passACMethodAsParam)
                convParams.Add(acMethod);

            return convParams.ToArray();
        }

        public ACClassMethod GetACClassMethod(IACObjectWithInit acObj, string acMethodName, out string acMethodName1)
        {
            ACComponent aCComponent = acObj as ACComponent;
            if (aCComponent != null)
                return aCComponent.GetACClassMethod(acMethodName, out acMethodName1);
            else
            {
                gip.core.datamodel.ACClass acClass = acObj.ACType as gip.core.datamodel.ACClass;
                if (acClass != null)
                {
                    int pos = acMethodName.IndexOf('!');
                    if (pos == 0)
                        acMethodName1 = acMethodName.Substring(1);
                    else
                        acMethodName1 = acMethodName;
                    return acClass.GetMethod(acMethodName1);
                }
                else
                    throw new ArgumentException(String.Format("The object {0} is not a valid ACComponent or ACClass. Cannot find method {1}.", acObj.ACIdentifier, acMethodName));
            }
        }

        public static JsonSerializerOptions GetNewSerializerOptions(ushort detailLevel, string[] requestedFields, Dictionary<string, gip.core.datamodel.ACClass> entityTypes)
        {
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                //MaxDepth = 2
            };
            serializerOptions.Converters.Add(new ACPropertyJsonConverterFactory(detailLevel, requestedFields, entityTypes));
            return serializerOptions;
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
                return _SerializerOptions;
            }
        }

        #endregion

    }

}