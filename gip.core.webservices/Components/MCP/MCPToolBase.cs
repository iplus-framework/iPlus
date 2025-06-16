// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

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
                if (result is IACObject)
                {
                    // For ACObjects, return basic information
                    var acObject = result as IACObject;
                    return new
                    {
                        ACIdentifier = acObject.ACIdentifier,
                        ACCaption = acObject.ACCaption,
                        TypeName = acObject.GetType().Name,
                        ACUrl = acObject.GetACUrl()
                    };
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
        #endregion

    }

}