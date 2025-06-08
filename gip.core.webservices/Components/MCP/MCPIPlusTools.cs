// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using gip.core.autocomponent;
using gip.core.datamodel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace gip.core.webservices
{
    [McpServerToolType]
    public sealed class MCPIPlusTools
    {
        [McpServerTool]
        [Description("Discover iPlus components, their properties and methods based on search criteria. Use this to find objects before executing commands.")]
        public static async Task<string> DiscoverComponents(
            IACComponent root,
            [Description("Search term to filter components by ACIdentifier, ACCaption, or ACType.Comment. Leave empty to get all components.")]
            string searchTerm = "",
            [Description("Maximum depth to search in the component tree. 0 means unlimited depth.")]
            int maxDepth = 2,
            [Description("Include detailed property information in the result.")]
            bool includeProperties = true,
            [Description("Include detailed method information in the result.")]
            bool includeMethods = true)
        {
            try
            {
                var discoveryResult = new ComponentDiscoveryResult();

                // Find components based on search criteria
                var components = root.FindChildComponents<IACComponent>(component =>
                {
                    if (string.IsNullOrEmpty(searchTerm))
                        return true;

                    return component.ACIdentifier?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                           component.ACCaption?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                           component.ACType?.Comment?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true;
                }, null, maxDepth);

                foreach (var component in components.Take(100)) // Limit results to prevent overwhelming response
                {
                    var componentInfo = new ComponentInfo
                    {
                        ACUrl = component.GetACUrl(),
                        ACIdentifier = component.ACIdentifier,
                        ACCaption = component.ACCaption,
                        TypeName = component.ACType?.ACIdentifier,
                        Comment = component.ACType?.Comment
                    };

                    if (includeProperties)
                    {
                        componentInfo.Properties = GetComponentProperties(component);
                    }

                    if (includeMethods)
                    {
                        componentInfo.Methods = GetComponentMethods(component);
                    }

                    discoveryResult.Components.Add(componentInfo);
                }

                discoveryResult.TotalFound = components.Count;
                discoveryResult.ResultsLimited = components.Count > 100;

                return JsonSerializer.Serialize(discoveryResult, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        [McpServerTool]
        [Description("Execute ACUrlCommand on iPlus components. Use this to get property values, set property values, or invoke methods.")]
        public static async Task<string> ExecuteACUrlCommand(
            IACComponent root,
            [Description("The ACUrl command to execute. Examples: '\\\\MyComponent\\MyProperty' (get property), '\\\\MyComponent\\MyProperty=NewValue' (set property), '\\\\MyComponent\\!MyMethod' (invoke method)")]
            string acUrl,
            [Description("Optional parameters for method calls as JSON array. Example: '[\"param1\", 123, true]'")]
            string parametersJson = "")
        {
            try
            {
                object[] parameters = null;

                if (!string.IsNullOrEmpty(parametersJson))
                {
                    var jsonParams = JsonSerializer.Deserialize<JsonElement[]>(parametersJson);
                    parameters = ConvertJsonParametersToObjects(jsonParams);
                }

                object result = root.ACUrlCommand(acUrl, parameters);

                var response = new ACUrlCommandResult
                {
                    Success = true,
                    ACUrl = acUrl,
                    Result = ConvertResultToJson(result),
                    ResultType = result?.GetType()?.Name
                };

                return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                var errorResponse = new ACUrlCommandResult
                {
                    Success = false,
                    ACUrl = acUrl,
                    Error = ex.Message,
                    ErrorType = ex.GetType().Name
                };

                return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        private static List<PropertyInfo> GetComponentProperties(IACComponent component)
        {
            var properties = new List<PropertyInfo>();

            try
            {
                if (component.ComponentClass?.Properties != null)
                {
                    foreach (var property in component.ComponentClass.Properties)
                    {
                        var propInfo = new PropertyInfo
                        {
                            ACIdentifier = property.ACIdentifier,
                            ACCaption = property.ACCaption,
                            DataType = property.ObjectType?.Name,
                            ACUrl = component.GetACUrl() + "\\" + property.ACIdentifier,
                            IsReadOnly = property.IsInput,
                            Comment = property.Comment
                        };

                        properties.Add(propInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                properties.Add(new PropertyInfo
                {
                    ACIdentifier = "ERROR",
                    Comment = $"Error reading properties: {ex.Message}"
                });
            }

            return properties;
        }

        private static List<MethodInfo> GetComponentMethods(IACComponent component)
        {
            var methods = new List<MethodInfo>();

            try
            {
                foreach (var method in component.ACClassMethods)
                {
                    var methodInfo = new MethodInfo
                    {
                        ACIdentifier = method.ACIdentifier,
                        ACCaption = method.ACCaption,
                        ACUrl = component.GetACUrl() + "!" + method.ACIdentifier,
                        Comment = method.Comment
                    };

                    // Try to get parameter information from XML
                    if (!string.IsNullOrEmpty(method.XMLACMethod))
                    {
                        try
                        {
                            var acMethod = ACClassMethod.DeserializeACMethod(method.XMLACMethod);
                            if (acMethod != null)
                            {
                                methodInfo.Parameters = acMethod.ParameterValueList?.Select(p => new ParameterInfo
                                {
                                    Name = p.ACIdentifier,
                                    DataType = p.ObjectType?.Name,
                                    IsRequired = p.Option == Global.ParamOption.Required
                                }).ToList() ?? new List<ParameterInfo>();

                                methodInfo.ReturnType = acMethod.ResultValueList?.FirstOrDefault()?.ObjectType?.Name;
                            }
                        }
                        catch
                        {
                            // If XML parsing fails, continue without parameter info
                        }
                    }

                    methods.Add(methodInfo);
                }
            }
            catch (Exception ex)
            {
                methods.Add(new MethodInfo
                {
                    ACIdentifier = "ERROR",
                    Comment = $"Error reading methods: {ex.Message}"
                });
            }

            return methods;
        }

        private static object[] ConvertJsonParametersToObjects(JsonElement[] jsonParams)
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

        private static object ConvertResultToJson(object result)
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
    }
}