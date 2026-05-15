// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using Scryber.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace gip.core.reporthandler
{
    /// <summary>
    /// Shared Scryber PDF rendering utility for report templates.
    /// </summary>
    public static class ScryberReportEngine
    {
        private static readonly Regex EachBlockRegex = new Regex(@"\{\{#each\s+([^}]+)\}\}(.*?)\{\{/each\}\}", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ValueExpressionRegex = new Regex(@"\{\{\s*([^#/][^}]*)\s*\}\}", RegexOptions.Singleline | RegexOptions.Compiled);
        private const string DefaultScryberHtmlTemplate = "<!doctype html>\n"
            + "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n"
            + "<head>\n"
            + "  <meta charset=\"utf-8\" />\n"
            + "  <title>New Scryber Report</title>\n"
            + "  <style>body { font-family: Helvetica, Arial, sans-serif; font-size: 11pt; margin: 24pt; } h1 { margin: 0 0 12pt 0; } p { margin: 4pt 0; color: #333; }</style>\n"
            + "</head>\n"
            + "<body>\n"
            + "  <h1>New Scryber Report</h1>\n"
            + "  <p>Default model convention: {{vb.Get(\"model\\\\FieldName\")}}</p>\n"
            + "  <p>Named value convention: {{vb.Get(\"values\\\\Document\\\\FieldName\")}}</p>\n"
            + "  <p>Legacy root-key convention: {{vb.Get(\"Document\\\\FieldName\")}}</p>\n"
            + "</body>\n"
            + "</html>";

        public static bool IsScryberTemplate(string template)
        {
            if (String.IsNullOrWhiteSpace(template))
                return false;

            string trimmed = template.TrimStart();
            if (trimmed.StartsWith("<FlowDocument", StringComparison.OrdinalIgnoreCase)
                || trimmed.IndexOf("<FlowDocument", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            if (trimmed.StartsWith("<!doctype html", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase)
                || trimmed.IndexOf("xmlns=\"http://www.w3.org/1999/xhtml\"", StringComparison.OrdinalIgnoreCase) >= 0
                || trimmed.IndexOf("xmlns='http://www.w3.org/1999/xhtml'", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return trimmed.IndexOf("{{", StringComparison.OrdinalIgnoreCase) >= 0
                   && trimmed.IndexOf("<template", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static byte[] RenderPdf(string template, ReportData reportData)
        {
            if (String.IsNullOrWhiteSpace(template))
                throw new ArgumentException("Template is empty.", nameof(template));
            if (reportData == null)
                throw new ArgumentNullException(nameof(reportData));

            using (TextReader reader = new StringReader(template))
            using (Document document = IsScryberTemplate(template)
                ? Document.ParseHtmlDocument(reader)
                : Document.ParseDocument(reader))
            {
                BindReportData(document, reportData);

                reportData.InformComponents(document, ACPrintingPhase.Started);
                try
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        document.SaveAsPDF(memory);
                        reportData.InformComponents(document, ACPrintingPhase.Completed);
                        return memory.ToArray();
                    }
                }
                catch
                {
                    reportData.InformComponents(document, ACPrintingPhase.Cancelled);
                    throw;
                }
            }
        }

        /// <summary>
        /// Renders a Scryber template using a custom layout renderer.
        /// </summary>
        /// <param name="template">Template content (HTML/XHTML).</param>
        /// <param name="reportData">Report data for binding.</param>
        /// <param name="renderer">Custom renderer that consumes the computed layout tree.</param>
        /// <returns>Renderer output as bytes.</returns>
        public static byte[] RenderWithLayoutRenderer(string template, ReportData reportData, IDocumentLayoutRenderer renderer)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            using (MemoryStream memory = new MemoryStream())
            {
                RenderWithLayoutRenderer(template, reportData, renderer, memory);
                return memory.ToArray();
            }
        }

        /// <summary>
        /// Renders a Scryber template using a custom layout renderer and writes output to the destination stream.
        /// </summary>
        /// <param name="template">Template content (HTML/XHTML).</param>
        /// <param name="reportData">Report data for binding.</param>
        /// <param name="renderer">Custom renderer that consumes the computed layout tree.</param>
        /// <param name="output">Destination stream for renderer output.</param>
        public static void RenderWithLayoutRenderer(string template, ReportData reportData, IDocumentLayoutRenderer renderer, Stream output)
        {
            if (String.IsNullOrWhiteSpace(template))
                throw new ArgumentException("Template is empty.", nameof(template));
            if (reportData == null)
                throw new ArgumentNullException(nameof(reportData));
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            using (TextReader reader = new StringReader(template))
            using (Document document = IsScryberTemplate(template)
                ? Document.ParseHtmlDocument(reader)
                : Document.ParseDocument(reader))
            {
                BindReportData(document, reportData);

                reportData.InformComponents(document, ACPrintingPhase.Started);
                try
                {
                    document.SaveAs(output, true, renderer);
                    reportData.InformComponents(document, ACPrintingPhase.Completed);
                }
                catch
                {
                    reportData.InformComponents(document, ACPrintingPhase.Cancelled);
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns the default Scryber HTML template used when creating new DUReport designs.
        /// </summary>
        public static string GetDefaultHtmlTemplate()
        {
            return DefaultScryberHtmlTemplate;
        }

        /// <summary>
        /// Creates an evaluated HTML preview for editors.
        /// Runs the Scryber pipeline first to ensure the template can be parsed and bound,
        /// then materializes common handlebars expressions for browser preview.
        /// </summary>
        public static string RenderHtmlPreview(string template, ReportData reportData)
        {
            if (String.IsNullOrWhiteSpace(template))
                return String.Empty;
            if (reportData == null)
                throw new ArgumentNullException(nameof(reportData));
            if (!IsScryberTemplate(template))
                return template;

            // Validate by running the real Scryber pipeline once.
            ExecuteScryberPipeline(template, reportData);

            object model = GetDefaultModel(reportData);
            return ExpandTemplateForPreview(template, reportData, model, model, 0);
        }

        public static void BindReportData(Document document, ReportData reportData)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (reportData == null)
                throw new ArgumentNullException(nameof(reportData));

            foreach (KeyValuePair<string, object> entry in reportData.ReportDocumentValues)
            {
                if (!String.IsNullOrWhiteSpace(entry.Key))
                    document.Params[entry.Key] = entry.Value;
            }

            object model = GetDefaultModel(reportData);
            if (model != null)
                document.Params["model"] = model;

            document.Params["reportData"] = reportData;
            document.Params["values"] = reportData.ReportDocumentValues;
            document.Params["vb"] = new VBContentValueResolver(reportData);
        }

        /// <summary>
        /// Resolves a legacy VBContent path using one concrete mapping convention:
        /// - Separators: '\\' and '/' are always supported; '.' is also supported when no slash separator exists.
        /// - Explicit roots: 'model', 'values', 'reportData'.
        /// - Legacy root-key mode: first segment is resolved against ReportDocumentValues (case-insensitive);
        ///   if not found, resolution falls back to the default model.
        /// </summary>
        public static object ResolveVBContent(ReportData reportData, string vbContentPath)
        {
            if (reportData == null || String.IsNullOrWhiteSpace(vbContentPath))
                return null;

            string[] segments = SplitVBContentSegments(vbContentPath);

            if (segments.Length == 0)
                return null;

            object current = null;
            int startIndex = 0;

            if (segments[0].Equals("model", StringComparison.OrdinalIgnoreCase))
            {
                current = GetDefaultModel(reportData);
                startIndex = 1;
            }
            else if (segments[0].Equals("values", StringComparison.OrdinalIgnoreCase))
            {
                current = reportData.ReportDocumentValues;
                startIndex = 1;
            }
            else if (segments[0].Equals("reportData", StringComparison.OrdinalIgnoreCase))
            {
                current = reportData;
                startIndex = 1;
            }
            else if (!TryGetValueCaseInsensitive(reportData.ReportDocumentValues, segments[0], out current))
            {
                current = GetDefaultModel(reportData);
            }
            else
            {
                startIndex = 1;
            }

            if (current == null)
                return null;

            for (int i = startIndex; i < segments.Length; i++)
            {
                current = ResolveSegment(current, segments[i]);
                if (current == null)
                    return null;
            }

            return current;
        }

        private static string[] SplitVBContentSegments(string vbContentPath)
        {
            if (String.IsNullOrWhiteSpace(vbContentPath))
                return Array.Empty<string>();

            string normalized = vbContentPath.Trim();
            normalized = normalized.Replace('/', '\\');

            // Support dotted notation as separator only if no explicit slash separator is used.
            if (normalized.IndexOf('\\') < 0)
                normalized = normalized.Replace('.', '\\');

            return normalized
                .Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !String.IsNullOrWhiteSpace(c))
                .ToArray();
        }

        private static bool TryGetValueCaseInsensitive(IDictionary<string, object> dictionary, string key, out object value)
        {
            value = null;
            if (dictionary == null || String.IsNullOrWhiteSpace(key))
                return false;

            if (dictionary.TryGetValue(key, out value))
                return true;

            KeyValuePair<string, object> match = dictionary.FirstOrDefault(c =>
                c.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (!String.IsNullOrWhiteSpace(match.Key))
            {
                value = match.Value;
                return true;
            }

            return false;
        }

        internal static object GetDefaultModel(ReportData reportData)
        {
            if (reportData == null || reportData.ReportDocumentValues == null || reportData.ReportDocumentValues.Count == 0)
                return null;

            KeyValuePair<string, object> value = reportData.ReportDocumentValues.FirstOrDefault(c => c.Value != null);
            return value.Value;
        }

        private static void ExecuteScryberPipeline(string template, ReportData reportData)
        {
            using (TextReader reader = new StringReader(template))
            using (Document document = Document.ParseHtmlDocument(reader))
            {
                BindReportData(document, reportData);
                document.SaveAsPDF(Stream.Null);
            }
        }

        private static string ExpandTemplateForPreview(string template, ReportData reportData, object model, object currentItem, int index)
        {
            if (String.IsNullOrEmpty(template))
                return String.Empty;

            string expanded = template;
            while (true)
            {
                Match match = EachBlockRegex.Match(expanded);
                if (!match.Success)
                    break;

                string collectionExpression = match.Groups[1].Value.Trim();
                string blockTemplate = match.Groups[2].Value;

                object collection = ResolveTemplateExpression(collectionExpression, reportData, model, currentItem, index);
                StringBuilder replacement = new StringBuilder();

                if (collection is IEnumerable enumerable && !(collection is string))
                {
                    int itemIndex = 0;
                    foreach (object item in enumerable)
                    {
                        replacement.Append(ExpandTemplateForPreview(blockTemplate, reportData, model, item, itemIndex));
                        itemIndex++;
                    }
                }

                expanded = expanded.Substring(0, match.Index)
                           + replacement
                           + expanded.Substring(match.Index + match.Length);
            }

            return ValueExpressionRegex.Replace(expanded, m =>
            {
                string expression = (m.Groups[1].Value ?? String.Empty).Trim();
                if (String.IsNullOrWhiteSpace(expression)
                    || expression.StartsWith("else", StringComparison.OrdinalIgnoreCase))
                {
                    return String.Empty;
                }

                object value = ResolveTemplateExpression(expression, reportData, model, currentItem, index);
                return WebUtility.HtmlEncode(FormatValue(value));
            });
        }

        private static object ResolveTemplateExpression(string expression, ReportData reportData, object model, object currentItem, int index)
        {
            if (String.IsNullOrWhiteSpace(expression))
                return null;

            string expr = expression.Trim();

            if (expr.Equals("@index", StringComparison.OrdinalIgnoreCase))
                return index;
            if (expr.Equals("this", StringComparison.OrdinalIgnoreCase))
                return currentItem;
            if (expr.Equals("model", StringComparison.OrdinalIgnoreCase))
                return model;

            if (TryResolveVbGetExpression(expr, reportData, out object vbValue))
                return vbValue;

            if (expr.StartsWith("this.", StringComparison.OrdinalIgnoreCase))
                return ResolvePath(currentItem, expr.Substring(5));

            if (expr.StartsWith("model.", StringComparison.OrdinalIgnoreCase))
                return ResolvePath(model, expr.Substring(6));

            if (expr.StartsWith("values.", StringComparison.OrdinalIgnoreCase))
                return ResolvePath(reportData?.ReportDocumentValues, expr.Substring(7));

            if (expr.StartsWith("reportData.", StringComparison.OrdinalIgnoreCase))
                return ResolvePath(reportData, expr.Substring(11));

            object fromCurrent = ResolvePath(currentItem, expr);
            if (fromCurrent != null)
                return fromCurrent;

            object fromModel = ResolvePath(model, expr);
            if (fromModel != null)
                return fromModel;

            object fromValues = ResolvePath(reportData?.ReportDocumentValues, expr);
            if (fromValues != null)
                return fromValues;

            return ResolveVBContent(reportData, expr);
        }

        private static bool TryResolveVbGetExpression(string expression, ReportData reportData, out object value)
        {
            value = null;
            if (!expression.StartsWith("vb.Get(", StringComparison.OrdinalIgnoreCase)
                || !expression.EndsWith(")", StringComparison.Ordinal))
            {
                return false;
            }

            string inner = expression.Substring(7, expression.Length - 8).Trim();
            if (inner.Length < 2)
                return false;

            bool isQuoted = (inner.StartsWith("\"", StringComparison.Ordinal) && inner.EndsWith("\"", StringComparison.Ordinal))
                || (inner.StartsWith("'", StringComparison.Ordinal) && inner.EndsWith("'", StringComparison.Ordinal));

            if (!isQuoted)
                return false;

            string vbPath = inner.Substring(1, inner.Length - 2);
            value = ResolveVBContent(reportData, vbPath);
            return true;
        }

        private static object ResolvePath(object source, string path)
        {
            if (source == null)
                return null;

            if (String.IsNullOrWhiteSpace(path) || path.Trim() == ".")
                return source;

            string normalized = path.Trim();
            if (normalized.StartsWith(".", StringComparison.Ordinal))
                normalized = normalized.Substring(1);

            normalized = normalized.Replace("[", ".").Replace("]", String.Empty);

            object current = source;
            string[] segments = normalized.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawSegment in segments)
            {
                string segment = rawSegment.Trim();
                if (String.IsNullOrWhiteSpace(segment))
                    continue;

                current = ResolveSegment(current, segment);
                if (current == null)
                    return null;
            }

            return current;
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return String.Empty;

            if (value is string text)
                return text;

            return value.ToString() ?? String.Empty;
        }

        private static object ResolveSegment(object source, string segment)
        {
            if (source == null || String.IsNullOrWhiteSpace(segment))
                return null;

            if (segment == ".")
                return source;

            if (source is IDictionary<string, object> genericDictionary)
            {
                if (genericDictionary.TryGetValue(segment, out object foundGeneric))
                    return foundGeneric;
                KeyValuePair<string, object> match = genericDictionary.FirstOrDefault(c =>
                    c.Key.Equals(segment, StringComparison.OrdinalIgnoreCase));
                return !String.IsNullOrWhiteSpace(match.Key) ? match.Value : null;
            }

            if (source is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key != null
                        && String.Equals(entry.Key.ToString(), segment, StringComparison.OrdinalIgnoreCase))
                    {
                        return entry.Value;
                    }
                }
            }

            Type type = source.GetType();
            PropertyInfo property = type.GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
                return property.GetValue(source, null);

            FieldInfo field = type.GetField(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
                return field.GetValue(source);

            if (Int32.TryParse(segment, out int index) && source is IEnumerable enumerable)
            {
                int current = 0;
                foreach (object item in enumerable)
                {
                    if (current == index)
                        return item;
                    current++;
                }
            }

            return null;
        }
    }

    public sealed class VBContentValueResolver
    {
        private readonly ReportData _reportData;

        public VBContentValueResolver(ReportData reportData)
        {
            _reportData = reportData;
        }

        public object Model
        {
            get
            {
                return ScryberReportEngine.GetDefaultModel(_reportData);
            }
        }

        public IDictionary<string, object> Values
        {
            get
            {
                return _reportData?.ReportDocumentValues;
            }
        }

        public object Get(string vbContentPath)
        {
            return ScryberReportEngine.ResolveVBContent(_reportData, vbContentPath);
        }
    }
}