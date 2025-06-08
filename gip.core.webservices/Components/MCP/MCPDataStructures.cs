// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace gip.core.webservices
{
    public class ComponentDiscoveryResult
    {
        public List<ComponentInfo> Components { get; set; } = new List<ComponentInfo>();
        public int TotalFound { get; set; }
        public bool ResultsLimited { get; set; }
    }

    public class ComponentInfo
    {
        public string ACUrl { get; set; }
        public string ACIdentifier { get; set; }
        public string ACCaption { get; set; }
        public string TypeName { get; set; }
        public string Comment { get; set; }
        public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
        public List<MethodInfo> Methods { get; set; } = new List<MethodInfo>();
    }

    public class PropertyInfo
    {
        public string ACIdentifier { get; set; }
        public string ACCaption { get; set; }
        public string DataType { get; set; }
        public string ACUrl { get; set; }
        public bool IsReadOnly { get; set; }
        public string Comment { get; set; }
    }

    public class MethodInfo
    {
        public string ACIdentifier { get; set; }
        public string ACCaption { get; set; }
        public string ACUrl { get; set; }
        public string Comment { get; set; }
        public string ReturnType { get; set; }
        public List<ParameterInfo> Parameters { get; set; } = new List<ParameterInfo>();
    }

    public class ParameterInfo
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
    }

    public class ACUrlCommandResult
    {
        public bool Success { get; set; }
        public string ACUrl { get; set; }
        public object Result { get; set; }
        public string ResultType { get; set; }
        public string Error { get; set; }
        public string ErrorType { get; set; }
    }
}