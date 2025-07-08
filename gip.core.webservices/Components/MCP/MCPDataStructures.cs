// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gip.core.webservices
{
    public class MCP_Exception
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class MCP_BaseType
    {
        [JsonPropertyName("ACId")]
        public string ACIdentifier { get; set; }
        [JsonPropertyName("D")]
        public string Description { get; set; }
    }

    public class MCP_TypeInfoBase : MCP_BaseType
    {
        //[JsonPropertyName("CId")]
        public string ClassID { get; set; }
        //[JsonPropertyName("BCID")]
        public string BaseClassID { get; set; }
    }

    public class MCP_TypeInfo : MCP_TypeInfoBase
    {
        public bool IsTypeOfAInstance { get; set; }
        public bool IsTypeOfADbTable { get; set; }
        public bool IsWorkflowType { get; set; }
        public bool IsMultiInstanceType { get; set; }
        public bool IsCodeOnGithub { get ; set; }
        public string ManualMCP { get; set; }
    }

    public class MCP_TypeInfoWithProperties : MCP_TypeInfo
    {
        public List<MCP_PropertyInfo> Properties { get; set; } = new List<MCP_PropertyInfo>();
    }

    public class MCP_TypeInfoWithMethods : MCP_TypeInfo
    {
        public List<MCP_MethodInfo> Methods { get; set; } = new List<MCP_MethodInfo>();
    }

    public class MCP_InstanceInfo : MCP_TypeInfoBase
    {
        public string MatchingBaseClassID { get; set; }
        public List<MCP_InstanceInfo> Childs { get; set; } = new List<MCP_InstanceInfo>();
    }

    public class MCP_ComponentDiscoveryResult
    {
        public List<MCP_ComponentInfo> Components { get; set; } = new List<MCP_ComponentInfo>();
        public int TotalFound { get; set; }
        public bool ResultsLimited { get; set; }
    }

    public class MCP_ComponentInfo
    {
        public string ACUrl { get; set; }
        public string ACIdentifier { get; set; }
        public string ACCaption { get; set; }
        public string TypeName { get; set; }
        public string Comment { get; set; }
        public List<MCP_PropertyInfo> Properties { get; set; } = new List<MCP_PropertyInfo>();
        public List<MCP_MethodInfo> Methods { get; set; } = new List<MCP_MethodInfo>();
    }

    public class MCP_PropertyInfo : MCP_BaseType
    {
        public string DataType { get; set; }
        public string InnerDataTypeClassID { get; set; }
        public string GenericTypeClassID { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class MCP_MethodInfo : MCP_BaseType
    {
        public string ReturnType { get; set; }
        public string ManualMCP { get; set; }
        public List<MCP_ParameterInfo> Parameters { get; set; } = new List<MCP_ParameterInfo>();
    }

    public class MCP_ParameterInfo
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
    }

    public class MCP_ACUrlCommandResult
    {
        public bool Success { get; set; }
        public string ACUrl { get; set; }
        [JsonConverter(typeof(RawJsonConverter))]
        public object Result { get; set; }
        public string ResultType { get; set; }
        public string Error { get; set; }
        public string Recommendation { get; set; }

    }
}