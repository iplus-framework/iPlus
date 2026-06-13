// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-14-2013
// ***********************************************************************
// <copyright file="ACClassDesign.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using SkiaSharp;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace gip.core.datamodel
{
    public static class XAMLConversionHelper
    {
        /// <summary>
        /// Converts WPF XAML to Avalonia XAML using namespace mappings and find/replace patterns.
        /// This is a unified conversion method used by both ACClassDesign and VBUserACClassDesign.
        /// </summary>
        /// <param name="wpfXaml">The WPF XAML string to convert</param>
        /// <returns>Converted Avalonia XAML string</returns>
        public static string ConvertWpfToAvaloniaXaml(string wpfXaml)
        {
            if (string.IsNullOrEmpty(wpfXaml))
                return wpfXaml;

            string avaloniaXAML = CheckOrUpdateNamespaceInLayout(wpfXaml);
            
            // Apply namespace mappings
            foreach (var tuple in ACxmlnsResolver.C_AvaloniaNamespaceMapping)
            {
                avaloniaXAML = avaloniaXAML?.Replace(tuple.WpfNamespace, tuple.AvaloniaNamespace);
            }
            
            // Apply regex and string replacements
            foreach (var tuple in C_AvaloniaFindAndReplace)
            {
                if (tuple.IsRegex)
                {
                    avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", tuple.WpfPattern, tuple.AvaloniaReplacement, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                }
                else
                {
                    avaloniaXAML = avaloniaXAML?.Replace(tuple.WpfPattern, tuple.AvaloniaReplacement);
                }
            }
            
            // Handle xmlns removal: split at first root element close, process child content only
            // This preserves xmlns on root element but removes from child elements
            if (!string.IsNullOrEmpty(avaloniaXAML))
            {
                // Find the end of the first root element tag (after all xmlns declarations)
                // Pattern matches up to first real element tag (not XML declarations like <?xml...?>)
                var rootEndMatch = Regex.Match(avaloniaXAML, @"^([\s\S]*?<[a-zA-Z][\w:]*[^>]*?>)([\s\S]*)$");
                if (rootEndMatch.Success)
                {
                    string rootPart = rootEndMatch.Groups[1].Value;
                    string childContent = rootEndMatch.Groups[2].Value;
                    
                    // Remove all xmlns declarations from child content only
                    childContent = Regex.Replace(childContent, @"\s+xmlns:?\w*=""[^""]*""", "", RegexOptions.IgnoreCase);
                    
                    // Remove attributes' namespace prefix for prefixes used by element names (e.g. vb: in <vb:VBButton vb:VBContent="...">).
                    // This also covers unprefixed roots like <Grid ...> where the first prefixed element appears in child content.
                    // We skip x/xml/xmlns to preserve standard XAML/XML behavior.
                    var elementPrefixMatches = Regex.Matches(rootPart + childContent, @"<([a-zA-Z][\w.]*)\:[a-zA-Z][\w.]*");
                    var elementPrefixes = elementPrefixMatches
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Where(prefix =>
                            !string.Equals(prefix, "x", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(prefix, "xml", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(prefix, "xmlns", StringComparison.OrdinalIgnoreCase));

                    foreach (string elementPrefix in elementPrefixes)
                    {
                        // Pattern matches the prefix only if the attribute name does NOT contain a dot.
                        // This ensures simple attributes like vb:VBContent become VBContent,
                        // but attached properties like vb:VBDockingManager.IsCloseableBSORoot are preserved.
                        string prefixAttrPattern = @"(\s+)" + Regex.Escape(elementPrefix) + @":([\w]+)(=)";
                        rootPart = Regex.Replace(rootPart, prefixAttrPattern, "$1$2$3", RegexOptions.IgnoreCase);
                        childContent = Regex.Replace(childContent, prefixAttrPattern, "$1$2$3", RegexOptions.IgnoreCase);
                    }

                    avaloniaXAML = rootPart + childContent;
                }
            }

            // Convert decimal CenterX/Y to percentage (e.g., 0.669 -> 67)
            // This handles the conversion from WPF's 0.0-1.0 coordinate system to Avalonia's percentage-based values where required
            avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", @"\b(Center[XY])=""0\.(\d+)""", m =>
            {
                string attr = m.Groups[1].Value;
                string decimals = m.Groups[2].Value;
                if (double.TryParse("0." + decimals, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                {
                    int percent = (int)Math.Round(val * 100);
                    return $"{attr}=\"{percent}\"";
                }
                return m.Value;
            }, RegexOptions.IgnoreCase);

            // Convert decimal StartPoint/EndPoint to percentage (e.g., 0.46,1.0 -> 46%,100%)
            // This handles the conversion of relative points in brushes to percentage-based strings required for Avalonia
            avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", @"\b(StartPoint|EndPoint)=""([^""]+)""", m =>
            {
                string attr = m.Groups[1].Value;
                string points = m.Groups[2].Value;
                var coords = points.Split(',');
                if (coords.Length == 2)
                {
                    if (double.TryParse(coords[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double x) &&
                        double.TryParse(coords[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double y))
                    {
                        int px = (int)Math.Round(x * 100);
                        int py = (int)Math.Round(y * 100);
                        return $"{attr}=\"{px}%,{py}%\"";
                    }
                }
                return m.Value;
            }, RegexOptions.IgnoreCase);

            // Avalonia expects RadialGradientBrush radii/origin/center in percent notation.
            avaloniaXAML = ConvertRadialGradientBrushValuesToPercent(avaloniaXAML);

            // Convert WPF resource dictionary includes to Avalonia ResourceInclude + avares:// URI syntax.
            avaloniaXAML = ConvertResourceDictionarySourceToResourceInclude(avaloniaXAML);

            // Avalonia parser is strict for VBBinding argument names.
            // Convert {vb:VBBinding vb:VBContent=...} -> {vb:VBBinding VBContent=...}.
            avaloniaXAML = RemovePrefixedVBBindingParameterNames(avaloniaXAML);

            // RelativeTransform was renamed to Transform for Avalonia brushes.
            // Convert both attribute usage and *.RelativeTransform property elements.
            avaloniaXAML = ConvertRelativeTransformsToTransforms(avaloniaXAML);

            // Convert WPF ComboBox attributes to Avalonia equivalents:
            // SelectedValuePath -> SelectedValueBinding, DisplayMemberPath -> *.ItemTemplate.
            avaloniaXAML = ConvertComboBoxSelectionAttributes(avaloniaXAML);

            // Convert DataGridTextColumn.ElementStyle to supported VBDataGridTextColumn attributes.
            avaloniaXAML = ConvertDataGridTextColumnElementStyle(avaloniaXAML);

            // Remove WPF DataGrid virtualization attributes not used by Avalonia.
            avaloniaXAML = RemoveUnsupportedVirtualizationAttributes(avaloniaXAML);

            // Convert WPF Line coordinates to Avalonia points:
            // <Line X1="0" Y1="6" X2="60" Y2="6" ... /> -> <Line StartPoint="0,6" EndPoint="60,6" ... />
            // Run after StartPoint/EndPoint percentage conversion to avoid treating line coordinates as percentages.
            avaloniaXAML = ConvertLineCoordinatesToStartEndPoint(avaloniaXAML);

            // Avalonia Shape has StrokeLineCap (single value) instead of WPF's
            // StrokeStartLineCap/StrokeEndLineCap pair.
            avaloniaXAML = ConvertStrokeStartEndLineCapToStrokeLineCap(avaloniaXAML);

            // Convert WPF-style trigger blocks embedded in *.Style property elements into
            // Xaml.Behaviors-based triggers and copy default Setter values to owner attributes.
            avaloniaXAML = ConvertControlThemeTriggersToBehaviors(avaloniaXAML);

            // Convert WPF *.LayoutTransform property elements to Avalonia LayoutTransformControl wrappers.
            // WPF: <TextBlock><TextBlock.LayoutTransform><RotateTransform/></TextBlock.LayoutTransform></TextBlock>
            // Avalonia: <LayoutTransformControl><LayoutTransformControl.LayoutTransform><RotateTransform/></LayoutTransformControl.LayoutTransform><TextBlock></TextBlock></LayoutTransformControl>
            avaloniaXAML = ConvertLayoutTransformToLayoutTransformControl(avaloniaXAML);

            // Remove CenterX/CenterY from ScaleTransform elements (not supported by Avalonia ScaleTransform).
            avaloniaXAML = RemoveScaleTransformCenterProperties(avaloniaXAML);

            // Apply regex and string replacements
            foreach (var tuple in C_AvaloniaPostFindAndReplace)
            {
                if (tuple.IsRegex)
                {
                    avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", tuple.WpfPattern, tuple.AvaloniaReplacement, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                }
                else
                {
                    avaloniaXAML = avaloniaXAML?.Replace(tuple.WpfPattern, tuple.AvaloniaReplacement);
                }
            }

            // Wrap Control elements inside ToolTip.Tip Setter.Value in <Template>.
            avaloniaXAML = WrapToolTipTipInTemplate(avaloniaXAML);

            // Produce well-formed, indented XML output.
            avaloniaXAML = FormatXaml(avaloniaXAML);

            return avaloniaXAML;
        }

        private static string ConvertControlThemeTriggersToBehaviors(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                if (doc.DocumentElement == null)
                    return xaml;

                string xamlNs = GetDefaultXamlNamespace(doc);
                if (string.IsNullOrEmpty(xamlNs))
                    return xaml;

                // Search for property elements that hold a Style/ControlTheme value.
                // Matches:
                //   - Names containing '.Style' (e.g. ListBox.ItemContainerStyle)
                //   - Names ending with 'Style' (e.g. GraphEdgeStyle)
                //   - Names ending with 'Theme' (e.g. ItemContainerTheme)
                // XPath 1.0 has no ends-with(), so we use substring(length-N) for suffix checks.
                var stylePropertyNodes = doc.SelectNodes(
                    "//*[contains(local-name(), '.Style') " +
                    "or substring(local-name(), string-length(local-name()) - 4) = 'Style' " +
                    "or substring(local-name(), string-length(local-name()) - 4) = 'Theme']");
                if (stylePropertyNodes == null || stylePropertyNodes.Count == 0)
                    return xaml;

                var styleProperties = stylePropertyNodes
                    .OfType<XmlNode>()
                    .OfType<XmlElement>()
                    .ToList();

                foreach (var styleProperty in styleProperties)
                {
                    if (!styleProperty.LocalName.EndsWith(".Style", StringComparison.OrdinalIgnoreCase) &&
                        !styleProperty.LocalName.EndsWith("Style", StringComparison.OrdinalIgnoreCase) &&
                        !styleProperty.LocalName.EndsWith("Theme", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var ownerElement = styleProperty.ParentNode as XmlElement;
                    if (ownerElement == null)
                        continue;

                    // Accept both WPF (Style) and Avalonia (ControlTheme) names,
                    // since find-and-replace converts `<Style ` but NOT `<Style.Triggers>`
                    // (the dot prevents the space-pattern match).
                    var controlTheme = styleProperty
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme", StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(e.LocalName, "Style", StringComparison.OrdinalIgnoreCase));

                    if (controlTheme == null)
                        continue;

                    var triggersElement = controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme.Triggers", StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(e.LocalName, "Style.Triggers", StringComparison.OrdinalIgnoreCase));

                    // Copy default setter values from ControlTheme root and optional ControlTheme.Setters block.
                    // Only promote setters to owner attributes when the style targets the owner itself.
                    // If TargetType differs from the owner (e.g. GraphEdgeStyle targets VBGraphEdge,
                    // not VBGraphSurface), keep setters inside the ControlTheme.
                    var targetTypeAttr = controlTheme.GetAttribute("TargetType");
                    bool targetsOwner = string.IsNullOrEmpty(targetTypeAttr) ||
                                        OwnerMatchesTargetType(ownerElement, targetTypeAttr);

                    var defaultSetters = new List<XmlElement>();
                    defaultSetters.AddRange(controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)));

                    var settersContainer = controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme.Setters", StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(e.LocalName, "Style.Setters", StringComparison.OrdinalIgnoreCase));

                    if (settersContainer != null)
                    {
                        defaultSetters.AddRange(settersContainer
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)));
                    }

                    int movedDefaultSetterCount = 0;
                    if (targetsOwner)
                    {
                        foreach (var setter in defaultSetters)
                        {
                            var propertyName = setter.GetAttribute("Property");
                            var propertyValue = setter.GetAttribute("Value");
                            if (string.IsNullOrWhiteSpace(propertyName))
                                continue;

                            // Only convert plain properties to owner attributes.
                            if (propertyName.Contains(".") || propertyName.Contains(":"))
                                continue;

                            propertyName = NormalizeTriggerPropertyName(propertyName);
                            propertyValue = NormalizeTriggerPropertyValue(propertyName, propertyValue);

                            if (!ownerElement.HasAttribute(propertyName))
                            {
                                ownerElement.SetAttribute(propertyName, propertyValue);
                                movedDefaultSetterCount++;
                            }
                        }
                    }

                    if (triggersElement == null)
                    {
                        // Clean up redundant style wrappers:
                        // 1) setters were successfully moved to owner attributes, or
                        // 2) style has no default setters at all.
                        if (movedDefaultSetterCount > 0 || defaultSetters.Count == 0)
                        {
                            ownerElement.RemoveChild(styleProperty);
                        }

                        continue;
                    }

                    var interactionBehaviors = doc.CreateElement("Interaction.Behaviors", xamlNs);
                    bool hasBehavior = false;

                    foreach (var dataTrigger in triggersElement
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "DataTrigger", StringComparison.OrdinalIgnoreCase)))
                    {
                        var behavior = doc.CreateElement("DataTriggerBehavior", xamlNs);

                        foreach (var attr in dataTrigger.Attributes.OfType<XmlAttribute>())
                        {
                            // Collapse embedded whitespace (newlines, tabs, multiple spaces) into single spaces
                            // so OuterXml does not emit &#xA; entities for multi-line attribute values.
                            string value = attr.Value;
                            if (value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0 || value.IndexOf('\t') >= 0)
                            {
                                value = string.Join(" ", value
                                    .Split(new[] { '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(v => v.Trim()));
                            }
                            behavior.SetAttribute(attr.Name, value);
                        }

                        int actionCount = 0;
                        foreach (var setter in dataTrigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)))
                        {
                            var propertyName = setter.GetAttribute("Property");
                            if (string.IsNullOrWhiteSpace(propertyName))
                                continue;

                            // ChangePropertyAction expects simple target property names.
                            if (propertyName.Contains(".") || propertyName.Contains(":"))
                                continue;

                            propertyName = NormalizeTriggerPropertyName(propertyName);
                            var propertyValue = NormalizeTriggerPropertyValue(propertyName, setter.GetAttribute("Value"));

                            var action = doc.CreateElement("ChangePropertyAction", xamlNs);
                            action.SetAttribute("PropertyName", propertyName);
                            action.SetAttribute("Value", propertyValue);
                            behavior.AppendChild(action);
                            actionCount++;
                        }

                        var enterActions = dataTrigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .FirstOrDefault(e => string.Equals(e.LocalName, "DataTrigger.EnterActions", StringComparison.OrdinalIgnoreCase));

                        actionCount += AppendBeginStoryboardAnimations(doc, xamlNs, ownerElement, behavior, enterActions, null);

                        if (actionCount > 0)
                        {
                            interactionBehaviors.AppendChild(behavior);
                            hasBehavior = true;
                        }
                    }

                    foreach (var trigger in triggersElement
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "Trigger", StringComparison.OrdinalIgnoreCase)))
                    {
                        var triggerProperty = trigger.GetAttribute("Property");
                        if (string.IsNullOrWhiteSpace(triggerProperty))
                            continue;

                        // Keep Trigger conversion aligned with ChangePropertyAction constraints.
                        if (triggerProperty.Contains(".") || triggerProperty.Contains(":"))
                            continue;

                        triggerProperty = NormalizeTriggerPropertyName(triggerProperty);
                        string triggerValue = NormalizeTriggerPropertyValue(triggerProperty, trigger.GetAttribute("Value"));

                        var behavior = doc.CreateElement("DataTriggerBehavior", xamlNs);
                        behavior.SetAttribute("Value", triggerValue);

                        var behaviorBindingProperty = doc.CreateElement("DataTriggerBehavior.Binding", xamlNs);
                        var behaviorBinding = doc.CreateElement("Binding", xamlNs);
                        behaviorBinding.SetAttribute("Path", triggerProperty);
                        behaviorBinding.SetAttribute("RelativeSource", "{RelativeSource Self}");
                        behaviorBindingProperty.AppendChild(behaviorBinding);
                        behavior.AppendChild(behaviorBindingProperty);

                        int actionCount = 0;
                        foreach (var setter in trigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)))
                        {
                            var propertyName = setter.GetAttribute("Property");
                            if (string.IsNullOrWhiteSpace(propertyName))
                                continue;

                            if (propertyName.Contains(".") || propertyName.Contains(":"))
                                continue;

                            propertyName = NormalizeTriggerPropertyName(propertyName);
                            var propertyValue = NormalizeTriggerPropertyValue(propertyName, setter.GetAttribute("Value"));

                            var action = doc.CreateElement("ChangePropertyAction", xamlNs);
                            action.SetAttribute("PropertyName", propertyName);
                            action.SetAttribute("Value", propertyValue);
                            behavior.AppendChild(action);
                            actionCount++;
                        }

                        var enterActions = trigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .FirstOrDefault(e => string.Equals(e.LocalName, "Trigger.EnterActions", StringComparison.OrdinalIgnoreCase));

                        var stopStoryboardFallbackValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        actionCount += AppendBeginStoryboardAnimations(doc, xamlNs, ownerElement, behavior, enterActions, stopStoryboardFallbackValues);

                        if (actionCount > 0)
                        {
                            interactionBehaviors.AppendChild(behavior);
                            hasBehavior = true;
                        }

                        var exitActions = trigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .FirstOrDefault(e => string.Equals(e.LocalName, "Trigger.ExitActions", StringComparison.OrdinalIgnoreCase));

                        bool hasStopStoryboard = exitActions != null && exitActions
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Any(e => string.Equals(e.LocalName, "StopStoryboard", StringComparison.OrdinalIgnoreCase));

                        string inverseValue = TryInvertBooleanLiteral(triggerValue);
                        if (hasStopStoryboard && !string.IsNullOrWhiteSpace(inverseValue) && stopStoryboardFallbackValues.Count > 0)
                        {
                            var stopBehavior = doc.CreateElement("DataTriggerBehavior", xamlNs);
                            stopBehavior.SetAttribute("Value", inverseValue);

                            var stopBehaviorBindingProperty = doc.CreateElement("DataTriggerBehavior.Binding", xamlNs);
                            var stopBehaviorBinding = doc.CreateElement("Binding", xamlNs);
                            stopBehaviorBinding.SetAttribute("Path", triggerProperty);
                            stopBehaviorBinding.SetAttribute("RelativeSource", "{RelativeSource Self}");
                            stopBehaviorBindingProperty.AppendChild(stopBehaviorBinding);
                            stopBehavior.AppendChild(stopBehaviorBindingProperty);

                            int stopActionCount = 0;
                            foreach (var kvp in stopStoryboardFallbackValues)
                            {
                                var stopAction = doc.CreateElement("ChangePropertyAction", xamlNs);
                                stopAction.SetAttribute("PropertyName", kvp.Key);
                                stopAction.SetAttribute("Value", kvp.Value);
                                stopBehavior.AppendChild(stopAction);
                                stopActionCount++;
                            }

                            if (stopActionCount > 0)
                            {
                                interactionBehaviors.AppendChild(stopBehavior);
                                hasBehavior = true;
                            }
                        }
                    }

                    foreach (var multiDataTrigger in triggersElement
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "MultiDataTrigger", StringComparison.OrdinalIgnoreCase)))
                    {
                        var conditionsElement = multiDataTrigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .FirstOrDefault(e => string.Equals(e.LocalName, "MultiDataTrigger.Conditions", StringComparison.OrdinalIgnoreCase));

                        if (conditionsElement == null)
                            continue;

                        var conditionElements = conditionsElement
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "Condition", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (conditionElements.Count == 0)
                            continue;

                        var behavior = doc.CreateElement("DataTriggerBehavior", xamlNs);
                        behavior.SetAttribute("Value", "True");

                        var behaviorBindingProperty = doc.CreateElement("DataTriggerBehavior.Binding", xamlNs);
                        var multiBinding = doc.CreateElement("MultiBinding", xamlNs);
                        multiBinding.SetAttribute("Converter", "{x:Static BoolConverters.And}");

                        bool allConditionsConverted = true;
                        foreach (var condition in conditionElements)
                        {
                            var conditionBinding = condition.GetAttribute("Binding");
                            if (string.IsNullOrWhiteSpace(conditionBinding))
                            {
                                allConditionsConverted = false;
                                break;
                            }

                            var bindingElement = CreateBindingElementFromMarkup(doc, xamlNs, conditionBinding);
                            if (bindingElement == null)
                            {
                                allConditionsConverted = false;
                                break;
                            }

                            string expectedValue = condition.GetAttribute("Value");
                            bool expectedIsTrue = IsTrueLiteral(expectedValue);
                            bool hasConverter = BindingElementHasConverter(bindingElement);

                            // Preserve existing converter behavior for boolean True checks.
                            // For value comparisons, attach ObjectEqualsConverter if none exists.
                            if (!expectedIsTrue)
                            {
                                if (hasConverter)
                                {
                                    allConditionsConverted = false;
                                    break;
                                }

                                var converterProperty = CreateElementWithResolvedNamespace(doc, xamlNs, $"{bindingElement.Name}.Converter");
                                var objectEqualsConverter = CreateElementWithResolvedNamespace(doc, xamlNs, "vb:ObjectEqualsConverter");
                                if (converterProperty == null || objectEqualsConverter == null)
                                {
                                    allConditionsConverted = false;
                                    break;
                                }

                                converterProperty.AppendChild(objectEqualsConverter);
                                bindingElement.AppendChild(converterProperty);

                                if (!string.IsNullOrWhiteSpace(expectedValue))
                                {
                                    bindingElement.SetAttribute("ConverterParameter", expectedValue);
                                }
                            }

                            multiBinding.AppendChild(bindingElement);
                        }

                        if (!allConditionsConverted)
                            continue;

                        behaviorBindingProperty.AppendChild(multiBinding);
                        behavior.AppendChild(behaviorBindingProperty);

                        // Collect setters from both direct children AND nested .Setters container.
                        // WPF XAML can use either <MultiDataTrigger><Setter .../></MultiDataTrigger>
                        // or <MultiDataTrigger><MultiDataTrigger.Setters><Setter .../></MultiDataTrigger.Setters></MultiDataTrigger>.
                        var settersFromDirectChildren = multiDataTrigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        var mdtSettersContainer = multiDataTrigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .FirstOrDefault(e => string.Equals(e.LocalName, "MultiDataTrigger.Setters", StringComparison.OrdinalIgnoreCase));

                        if (mdtSettersContainer != null)
                        {
                            var settersFromContainer = mdtSettersContainer
                                .ChildNodes
                                .OfType<XmlElement>()
                                .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase))
                                .ToList();
                            settersFromDirectChildren.AddRange(settersFromContainer);
                        }

                        foreach (var setter in settersFromDirectChildren)
                        {
                            var propertyName = setter.GetAttribute("Property");
                            if (string.IsNullOrWhiteSpace(propertyName))
                                continue;

                            if (propertyName.Contains(".") || propertyName.Contains(":"))
                                continue;

                            propertyName = NormalizeTriggerPropertyName(propertyName);
                            var propertyValue = NormalizeTriggerPropertyValue(propertyName, setter.GetAttribute("Value"));

                            var action = doc.CreateElement("ChangePropertyAction", xamlNs);
                            action.SetAttribute("PropertyName", propertyName);
                            action.SetAttribute("Value", propertyValue);
                            behavior.AppendChild(action);
                        }

                        interactionBehaviors.AppendChild(behavior);
                        hasBehavior = true;
                    }

                    if (!hasBehavior)
                        continue;

                    // Inject behaviors INTO the ControlTheme instead of replacing the entire style property.
                    // This preserves the *.GraphEdgeStyle property assignment and its TargetType.
                    controlTheme.AppendChild(interactionBehaviors);

                    // Remove the original triggers element since we've converted it to behaviors.
                    controlTheme.RemoveChild(triggersElement);
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string NormalizeTriggerPropertyName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return propertyName;

            if (string.Equals(propertyName, "Visibility", StringComparison.OrdinalIgnoreCase))
                return "IsVisible";

            return propertyName;
        }

        private static string ConvertStoryboardTargetProperty(string targetProperty)
        {
            if (string.IsNullOrWhiteSpace(targetProperty))
                return targetProperty;

            string trimmed = targetProperty.Trim();
            if (trimmed.Contains("(") && trimmed.Contains(")"))
            {
                var matches = Regex.Matches(trimmed, @"\(([^)]+)\)");
                if (matches.Count > 0)
                {
                    return matches[matches.Count - 1].Groups[1].Value.Trim();
                }
            }

            return trimmed;
        }

        private static int AppendBeginStoryboardAnimations(
            XmlDocument doc,
            string xamlNs,
            XmlElement ownerElement,
            XmlElement behavior,
            XmlElement actionsContainer,
            Dictionary<string, string> stopStoryboardFallbackValues)
        {
            if (doc == null || ownerElement == null || behavior == null || actionsContainer == null)
                return 0;

            int actionCount = 0;
            foreach (var beginStoryboard in actionsContainer
                .ChildNodes
                .OfType<XmlElement>()
                .Where(e => string.Equals(e.LocalName, "BeginStoryboard", StringComparison.OrdinalIgnoreCase)))
            {
                var storyboard = beginStoryboard
                    .ChildNodes
                    .OfType<XmlElement>()
                    .FirstOrDefault(e => string.Equals(e.LocalName, "Storyboard", StringComparison.OrdinalIgnoreCase));

                if (storyboard == null)
                    continue;

                foreach (var doubleAnimation in storyboard
                    .ChildNodes
                    .OfType<XmlElement>()
                    .Where(e => string.Equals(e.LocalName, "DoubleAnimation", StringComparison.OrdinalIgnoreCase)))
                {
                    string targetProperty = ConvertStoryboardTargetProperty(doubleAnimation.GetAttribute("Storyboard.TargetProperty"));
                    string from = doubleAnimation.GetAttribute("From");
                    string to = doubleAnimation.GetAttribute("To");
                    string duration = doubleAnimation.GetAttribute("Duration");

                    if (string.IsNullOrWhiteSpace(targetProperty)
                        || string.IsNullOrWhiteSpace(from)
                        || string.IsNullOrWhiteSpace(to)
                        || string.IsNullOrWhiteSpace(duration))
                        continue;

                    var beginAnimationAction = doc.CreateElement("BeginAnimationAction", xamlNs);
                    var beginAnimationActionProperty = doc.CreateElement("BeginAnimationAction.Animation", xamlNs);
                    var animation = doc.CreateElement("Animation", xamlNs);
                    animation.SetAttribute("Duration", duration);
                    animation.SetAttribute("IterationCount", ConvertRepeatBehaviorToIterationCount(doubleAnimation.GetAttribute("RepeatBehavior")));
                    animation.SetAttribute("FillMode", "Forward");
                    animation.SetAttribute("SetterTargetType", "http://schemas.microsoft.com/winfx/2006/xaml", ownerElement.LocalName);

                    if (IsExplicitTrueLiteral(doubleAnimation.GetAttribute("AutoReverse")))
                    {
                        animation.SetAttribute("PlaybackDirection", "Alternate");
                    }

                    var keyFrameStart = doc.CreateElement("KeyFrame", xamlNs);
                    keyFrameStart.SetAttribute("Cue", "0%");
                    var keyFrameStartSetter = doc.CreateElement("Setter", xamlNs);
                    keyFrameStartSetter.SetAttribute("Property", targetProperty);
                    keyFrameStartSetter.SetAttribute("Value", from);
                    keyFrameStart.AppendChild(keyFrameStartSetter);

                    var keyFrameEnd = doc.CreateElement("KeyFrame", xamlNs);
                    keyFrameEnd.SetAttribute("Cue", "100%");
                    var keyFrameEndSetter = doc.CreateElement("Setter", xamlNs);
                    keyFrameEndSetter.SetAttribute("Property", targetProperty);
                    keyFrameEndSetter.SetAttribute("Value", to);
                    keyFrameEnd.AppendChild(keyFrameEndSetter);

                    animation.AppendChild(keyFrameStart);
                    animation.AppendChild(keyFrameEnd);
                    beginAnimationActionProperty.AppendChild(animation);
                    beginAnimationAction.AppendChild(beginAnimationActionProperty);
                    behavior.AppendChild(beginAnimationAction);

                    if (stopStoryboardFallbackValues != null)
                    {
                        stopStoryboardFallbackValues[targetProperty] = to;
                    }

                    actionCount++;
                }
            }

            return actionCount;
        }

        private static string ConvertRepeatBehaviorToIterationCount(string repeatBehavior)
        {
            if (string.IsNullOrWhiteSpace(repeatBehavior))
                return "1";

            if (string.Equals(repeatBehavior.Trim(), "Forever", StringComparison.OrdinalIgnoreCase))
                return "Infinite";

            return "1";
        }

        private static bool IsExplicitTrueLiteral(string value)
        {
            return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
        }

        private static string TryInvertBooleanLiteral(string value)
        {
            if (string.Equals(value, "True", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase))
                return "False";

            if (string.Equals(value, "False", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "0", StringComparison.OrdinalIgnoreCase))
                return "True";

            return null;
        }

        private static string NormalizeTriggerPropertyValue(string normalizedPropertyName, string propertyValue)
        {
            if (!string.Equals(normalizedPropertyName, "IsVisible", StringComparison.OrdinalIgnoreCase))
                return propertyValue;

            if (string.Equals(propertyValue, "Visible", StringComparison.OrdinalIgnoreCase))
                return "True";

            if (string.Equals(propertyValue, "Hidden", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(propertyValue, "Collapsed", StringComparison.OrdinalIgnoreCase))
                return "False";

            return propertyValue;
        }

        private static bool IsTrueLiteral(string value)
        {
            return string.IsNullOrWhiteSpace(value) ||
                   string.Equals(value, "True", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
        }

        private static bool BindingElementHasConverter(XmlElement bindingElement)
        {
            if (bindingElement == null)
                return false;

            if (bindingElement.HasAttribute("Converter"))
                return true;

            return bindingElement
                .ChildNodes
                .OfType<XmlElement>()
                .Any(e => e.LocalName.EndsWith(".Converter", StringComparison.OrdinalIgnoreCase));
        }

        private static XmlElement CreateBindingElementFromMarkup(XmlDocument doc, string xamlNs, string markup)
        {
            if (doc == null || string.IsNullOrWhiteSpace(markup))
                return null;

            var extension = ParseMarkupExtension(markup);
            if (extension == null || string.IsNullOrWhiteSpace(extension.TypeName))
                return null;

            var bindingElement = CreateElementWithResolvedNamespace(doc, xamlNs, extension.TypeName);
            if (bindingElement == null)
                return null;

            foreach (var kvp in extension.Properties)
            {
                if (string.Equals(kvp.Key, "Converter", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryAppendConverterFromMarkup(doc, xamlNs, bindingElement, kvp.Value))
                        return null;
                    continue;
                }

                bindingElement.SetAttribute(kvp.Key, kvp.Value);
            }

            return bindingElement;
        }

        private static bool TryAppendConverterFromMarkup(XmlDocument doc, string xamlNs, XmlElement owner, string converterMarkup)
        {
            if (doc == null || owner == null || string.IsNullOrWhiteSpace(converterMarkup))
                return false;

            var converterExtension = ParseMarkupExtension(converterMarkup);
            if (converterExtension == null || string.IsNullOrWhiteSpace(converterExtension.TypeName))
                return false;

            var converterProperty = CreateElementWithResolvedNamespace(doc, xamlNs, $"{owner.Name}.Converter");
            var converterElement = CreateElementWithResolvedNamespace(doc, xamlNs, converterExtension.TypeName);
            if (converterProperty == null || converterElement == null)
                return false;

            foreach (var kvp in converterExtension.Properties)
            {
                converterElement.SetAttribute(kvp.Key, kvp.Value);
            }

            converterProperty.AppendChild(converterElement);
            owner.AppendChild(converterProperty);
            return true;
        }

        private static XmlElement CreateElementWithResolvedNamespace(XmlDocument doc, string fallbackNamespace, string qualifiedName)
        {
            if (doc == null || string.IsNullOrWhiteSpace(qualifiedName))
                return null;

            string prefix = string.Empty;
            string localName = qualifiedName;

            int colonIndex = qualifiedName.IndexOf(':');
            if (colonIndex > 0 && colonIndex < qualifiedName.Length - 1)
            {
                prefix = qualifiedName.Substring(0, colonIndex);
                localName = qualifiedName.Substring(colonIndex + 1);
            }

            string namespaceUri = fallbackNamespace;
            if (doc.DocumentElement != null)
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    string defaultNamespace = GetDefaultXamlNamespace(doc);
                    if (!string.IsNullOrWhiteSpace(defaultNamespace))
                    {
                        namespaceUri = defaultNamespace;
                    }
                }
                else
                {
                    string mappedNamespace = doc.DocumentElement.GetNamespaceOfPrefix(prefix);
                    if (!string.IsNullOrWhiteSpace(mappedNamespace))
                    {
                        namespaceUri = mappedNamespace;
                    }
                }
            }

            return string.IsNullOrEmpty(prefix)
                ? doc.CreateElement(localName, namespaceUri)
                : doc.CreateElement(prefix, localName, namespaceUri);
        }

        private static string GetDefaultXamlNamespace(XmlDocument doc)
        {
            if (doc?.DocumentElement == null)
                return string.Empty;

            string defaultNamespace = doc.DocumentElement.GetNamespaceOfPrefix(string.Empty);
            if (!string.IsNullOrWhiteSpace(defaultNamespace))
                return defaultNamespace;

            return doc.DocumentElement.NamespaceURI ?? string.Empty;
        }

        private sealed class ParsedMarkupExtension
        {
            public string TypeName { get; set; }
            public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private static ParsedMarkupExtension ParseMarkupExtension(string markup)
        {
            if (string.IsNullOrWhiteSpace(markup))
                return null;

            string text = markup.Trim();
            if (!text.StartsWith("{", StringComparison.Ordinal) || !text.EndsWith("}", StringComparison.Ordinal))
                return null;

            text = text.Substring(1, text.Length - 2).Trim();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            int splitIndex = text.IndexOfAny(new[] { ' ', ',' });
            string typeName = splitIndex < 0 ? text : text.Substring(0, splitIndex).Trim();
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            var parsed = new ParsedMarkupExtension
            {
                TypeName = typeName
            };

            if (splitIndex < 0)
                return parsed;

            string propertyText = text.Substring(splitIndex).Trim();
            foreach (var part in SplitTopLevel(propertyText, ','))
            {
                string assignment = part?.Trim();
                if (string.IsNullOrWhiteSpace(assignment))
                    continue;

                int equalsIndex = assignment.IndexOf('=');
                if (equalsIndex <= 0 || equalsIndex >= assignment.Length - 1)
                    continue;

                string key = assignment.Substring(0, equalsIndex).Trim();
                string value = assignment.Substring(equalsIndex + 1).Trim();
                if (value.Length >= 2 && value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                if (!string.IsNullOrWhiteSpace(key))
                    parsed.Properties[key] = value;
            }

            return parsed;
        }

        private static List<string> SplitTopLevel(string input, char separator)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(input))
                return result;

            var sb = new StringBuilder();
            int braceDepth = 0;
            bool inQuotes = false;

            foreach (char ch in input)
            {
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    sb.Append(ch);
                    continue;
                }

                if (!inQuotes)
                {
                    if (ch == '{')
                        braceDepth++;
                    else if (ch == '}' && braceDepth > 0)
                        braceDepth--;

                    if (ch == separator && braceDepth == 0)
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }

                sb.Append(ch);
            }

            if (sb.Length > 0)
                result.Add(sb.ToString());

            return result;
        }

        private static string ConvertRadialGradientBrushValuesToPercent(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                var brushNodes = doc.SelectNodes("//*[local-name()='RadialGradientBrush']");
                if (brushNodes == null || brushNodes.Count == 0)
                    return xaml;

                foreach (var brush in brushNodes.OfType<XmlNode>().OfType<XmlElement>())
                {
                    ConvertPercentAttribute(brush, "RadiusX");
                    ConvertPercentAttribute(brush, "RadiusY");
                    ConvertPercentPointAttribute(brush, "Center");
                    ConvertPercentPointAttribute(brush, "GradientOrigin");
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ConvertRelativeTransformsToTransforms(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                // Convert property elements like <RadialGradientBrush.RelativeTransform>...</...>
                // into <RadialGradientBrush.Transform>...</...>.
                var relativeTransformElements = doc.SelectNodes("//*[contains(local-name(), '.RelativeTransform')]");
                if (relativeTransformElements != null)
                {
                    foreach (var element in relativeTransformElements.OfType<XmlNode>().OfType<XmlElement>().ToList())
                    {
                        if (element.LocalName.EndsWith(".RelativeTransform", StringComparison.OrdinalIgnoreCase))
                        {
                            if (IsEmptyRelativeTransformElement(element))
                            {
                                element.ParentNode?.RemoveChild(element);
                                continue;
                            }

                            string transformedLocalName = element.LocalName.Substring(0, element.LocalName.Length - ".RelativeTransform".Length) + ".Transform";
                            var replacement = doc.CreateElement(element.Prefix, transformedLocalName, element.NamespaceURI);

                            if (element.HasAttributes)
                            {
                                foreach (XmlAttribute attribute in element.Attributes)
                                {
                                    var copied = doc.CreateAttribute(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI);
                                    copied.Value = attribute.Value;
                                    replacement.Attributes.Append(copied);
                                }
                            }

                            while (element.HasChildNodes)
                            {
                                replacement.AppendChild(element.FirstChild);
                            }

                            element.ParentNode?.ReplaceChild(replacement, element);
                        }
                    }
                }

                // Convert attribute usage: RelativeTransform="..." -> Transform="..."
                var allElements = doc.SelectNodes("//*");
                if (allElements != null)
                {
                    foreach (var element in allElements.OfType<XmlNode>().OfType<XmlElement>())
                    {
                        var relativeTransformAttributes = element.Attributes
                            .OfType<XmlAttribute>()
                            .Where(a => string.Equals(a.LocalName, "RelativeTransform", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        foreach (var attribute in relativeTransformAttributes)
                        {
                            if (string.IsNullOrWhiteSpace(attribute.Value))
                            {
                                element.Attributes.Remove(attribute);
                                continue;
                            }

                            bool hasTransformAlready = element.Attributes
                                .OfType<XmlAttribute>()
                                .Any(a => string.Equals(a.LocalName, "Transform", StringComparison.OrdinalIgnoreCase) &&
                                          string.Equals(a.NamespaceURI, attribute.NamespaceURI, StringComparison.Ordinal));

                            if (!hasTransformAlready)
                            {
                                var transformedAttribute = doc.CreateAttribute(attribute.Prefix, "Transform", attribute.NamespaceURI);
                                transformedAttribute.Value = attribute.Value;
                                element.Attributes.Append(transformedAttribute);
                            }

                            element.Attributes.Remove(attribute);
                        }
                    }
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string RemoveUnsupportedVirtualizationAttributes(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                var allElements = doc.SelectNodes("//*");
                if (allElements == null || allElements.Count == 0)
                    return xaml;

                foreach (var element in allElements.OfType<XmlNode>().OfType<XmlElement>())
                {
                    // Remove by local-name so both prefixed and unprefixed forms are handled.
                    var attributesToRemove = element.Attributes
                        .OfType<XmlAttribute>()
                        .Where(a =>
                            string.Equals(a.LocalName, "VirtualizingStackPanel.IsVirtualizing", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(a.LocalName, "VirtualizingStackPanel.VirtualizationMode", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(a.LocalName, "EnableRowVirtualization", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var attribute in attributesToRemove)
                    {
                        element.Attributes.Remove(attribute);
                    }
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ConvertComboBoxSelectionAttributes(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                string xamlNs = GetDefaultXamlNamespace(doc);
                if (string.IsNullOrEmpty(xamlNs))
                    return xaml;

                var elements = doc.SelectNodes("//*");
                if (elements == null || elements.Count == 0)
                    return xaml;

                foreach (var element in elements.OfType<XmlNode>().OfType<XmlElement>())
                {
                    bool isComboBox =
                        string.Equals(element.LocalName, "ComboBox", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(element.LocalName, "VBComboBox", StringComparison.OrdinalIgnoreCase);

                    if (!isComboBox)
                        continue;

                    // WPF: SelectedValuePath="MaterialWFID"
                    // Avalonia: SelectedValueBinding="{Binding Path=MaterialWFID}"
                    if (element.HasAttribute("SelectedValuePath"))
                    {
                        string selectedValuePath = element.GetAttribute("SelectedValuePath")?.Trim();

                        if (!string.IsNullOrWhiteSpace(selectedValuePath) && !element.HasAttribute("SelectedValueBinding"))
                        {
                            element.SetAttribute("SelectedValueBinding", $"{{Binding Path={selectedValuePath}}}");
                        }

                        element.RemoveAttribute("SelectedValuePath");
                    }

                    // WPF: DisplayMemberPath="Name"
                    // Avalonia: <*.ItemTemplate><DataTemplate><TextBlock Text="{Binding Path=Name}"/></DataTemplate></*.ItemTemplate>
                    if (!element.HasAttribute("DisplayMemberPath"))
                        continue;

                    string displayMemberPath = element.GetAttribute("DisplayMemberPath")?.Trim();
                    element.RemoveAttribute("DisplayMemberPath");

                    if (string.IsNullOrWhiteSpace(displayMemberPath))
                        continue;

                    bool hasItemTemplate = element
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Any(e => e.LocalName.EndsWith(".ItemTemplate", StringComparison.OrdinalIgnoreCase));

                    if (hasItemTemplate)
                        continue;

                    XmlElement itemTemplateProperty;
                    if (string.IsNullOrEmpty(element.Prefix))
                    {
                        itemTemplateProperty = doc.CreateElement($"{element.LocalName}.ItemTemplate", element.NamespaceURI);
                    }
                    else
                    {
                        itemTemplateProperty = doc.CreateElement(element.Prefix, $"{element.LocalName}.ItemTemplate", element.NamespaceURI);
                    }

                    var dataTemplate = doc.CreateElement("DataTemplate", xamlNs);
                    var textBlock = doc.CreateElement("TextBlock", xamlNs);
                    textBlock.SetAttribute("Text", $"{{Binding Path={displayMemberPath}}}");

                    dataTemplate.AppendChild(textBlock);
                    itemTemplateProperty.AppendChild(dataTemplate);
                    element.AppendChild(itemTemplateProperty);
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ConvertDataGridTextColumnElementStyle(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                var styleNodes = doc.SelectNodes("//*[contains(local-name(), '.ElementStyle')]");
                if (styleNodes == null || styleNodes.Count == 0)
                    return xaml;

                foreach (var styleElement in styleNodes.OfType<XmlNode>().OfType<XmlElement>().ToList())
                {
                    if (!styleElement.LocalName.EndsWith(".ElementStyle", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var ownerElement = styleElement.ParentNode as XmlElement;
                    if (ownerElement == null)
                        continue;

                    bool isTextColumn = string.Equals(ownerElement.LocalName, "VBDataGridTextColumn", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(ownerElement.LocalName, "DataGridTextColumn", StringComparison.OrdinalIgnoreCase);
                    bool isDateTimeColumn = string.Equals(ownerElement.LocalName, "VBDataGridDateTimeColumn", StringComparison.OrdinalIgnoreCase);
                    bool isComboBoxColumn = string.Equals(ownerElement.LocalName, "VBDataGridComboBoxColumn", StringComparison.OrdinalIgnoreCase);
                    bool isCheckBoxColumn = string.Equals(ownerElement.LocalName, "VBDataGridCheckBoxColumn", StringComparison.OrdinalIgnoreCase);
                    bool isACValueColumn = string.Equals(ownerElement.LocalName, "VBDataGridACValueColumn", StringComparison.OrdinalIgnoreCase);

                    if (!(isTextColumn || isDateTimeColumn || isComboBoxColumn || isCheckBoxColumn || isACValueColumn))
                        continue;

                    var controlTheme = styleElement
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme", StringComparison.OrdinalIgnoreCase));

                    if (controlTheme == null)
                        continue;

                    foreach (var setter in controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)))
                    {
                        string propertyName = setter.GetAttribute("Property");
                        string propertyValue = setter.GetAttribute("Value");

                        if (string.Equals(propertyName, "Foreground", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(propertyValue)
                            && !ownerElement.HasAttribute("Foreground"))
                        {
                            ownerElement.SetAttribute("Foreground", propertyValue);
                        }
                        else if (string.Equals(propertyName, "Background", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(propertyValue)
                            && !ownerElement.HasAttribute("CellBackground"))
                        {
                            ownerElement.SetAttribute("CellBackground", propertyValue);
                        }
                        else if (string.Equals(propertyName, "TextAlignment", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(propertyValue)
                            && (isTextColumn || isDateTimeColumn || isACValueColumn)
                            && !ownerElement.HasAttribute("CellTextAlignment"))
                        {
                            ownerElement.SetAttribute("CellTextAlignment", propertyValue);
                        }
                        else if (string.Equals(propertyName, "FontWeight", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(propertyValue)
                            && !ownerElement.HasAttribute("FontWeight"))
                        {
                            ownerElement.SetAttribute("FontWeight", propertyValue);
                        }
                        else if (string.Equals(propertyName, "FontStyle", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(propertyValue)
                            && !ownerElement.HasAttribute("FontStyle"))
                        {
                            ownerElement.SetAttribute("FontStyle", propertyValue);
                        }
                        else if (string.Equals(propertyName, "FontSize", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(propertyValue)
                            && !ownerElement.HasAttribute("FontSize"))
                        {
                            ownerElement.SetAttribute("FontSize", propertyValue);
                        }
                    }

                    var triggers = controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme.Triggers", StringComparison.OrdinalIgnoreCase));

                    if (triggers != null)
                    {
                        foreach (var dataTrigger in triggers
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "DataTrigger", StringComparison.OrdinalIgnoreCase)))
                        {
                            string triggerValue = dataTrigger.GetAttribute("Value");
                            if (!IsTrueLiteral(triggerValue))
                                continue;

                            string bindingMarkup = dataTrigger.GetAttribute("Binding");
                            string conditionPath = ExtractBindingPathFromMarkup(bindingMarkup);
                            if (string.IsNullOrWhiteSpace(conditionPath))
                                continue;

                            string trueColor = dataTrigger
                                .ChildNodes
                                .OfType<XmlElement>()
                                .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)
                                            && string.Equals(e.GetAttribute("Property"), "Foreground", StringComparison.OrdinalIgnoreCase))
                                .Select(e => e.GetAttribute("Value"))
                                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

                            string trueBackgroundColor = dataTrigger
                                .ChildNodes
                                .OfType<XmlElement>()
                                .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)
                                            && string.Equals(e.GetAttribute("Property"), "Background", StringComparison.OrdinalIgnoreCase))
                                .Select(e => e.GetAttribute("Value"))
                                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

                            if (string.IsNullOrWhiteSpace(trueColor) && string.IsNullOrWhiteSpace(trueBackgroundColor))
                                continue;

                            if (!ownerElement.HasAttribute("ConditionalForegroundPath") && !string.IsNullOrWhiteSpace(trueColor))
                                ownerElement.SetAttribute("ConditionalForegroundPath", conditionPath);

                            if (!ownerElement.HasAttribute("ConditionalForegroundTrueColor") && !string.IsNullOrWhiteSpace(trueColor))
                                ownerElement.SetAttribute("ConditionalForegroundTrueColor", trueColor);

                            if (!ownerElement.HasAttribute("ConditionalForegroundFalseColor")
                                && !string.IsNullOrWhiteSpace(trueColor)
                                && ownerElement.HasAttribute("Foreground"))
                            {
                                ownerElement.SetAttribute("ConditionalForegroundFalseColor", ownerElement.GetAttribute("Foreground"));
                            }

                            if (!ownerElement.HasAttribute("ConditionalBackgroundPath") && !string.IsNullOrWhiteSpace(trueBackgroundColor))
                                ownerElement.SetAttribute("ConditionalBackgroundPath", conditionPath);

                            if (!ownerElement.HasAttribute("ConditionalBackgroundTrueColor") && !string.IsNullOrWhiteSpace(trueBackgroundColor))
                                ownerElement.SetAttribute("ConditionalBackgroundTrueColor", trueBackgroundColor);

                            if (!ownerElement.HasAttribute("ConditionalBackgroundFalseColor")
                                && !string.IsNullOrWhiteSpace(trueBackgroundColor)
                                && ownerElement.HasAttribute("CellBackground"))
                            {
                                ownerElement.SetAttribute("ConditionalBackgroundFalseColor", ownerElement.GetAttribute("CellBackground"));
                            }

                            break;
                        }
                    }

                    ownerElement.RemoveChild(styleElement);
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ExtractBindingPathFromMarkup(string bindingMarkup)
        {
            if (string.IsNullOrWhiteSpace(bindingMarkup))
                return null;

            var parsed = ParseMarkupExtension(bindingMarkup);
            if (parsed != null)
            {
                if (parsed.Properties.TryGetValue("Path", out string pathFromProperty)
                    && !string.IsNullOrWhiteSpace(pathFromProperty))
                {
                    return pathFromProperty;
                }
            }

            var positionalMatch = Regex.Match(
                bindingMarkup,
                @"^\{\s*(?:[a-zA-Z_][\w.]*)?Binding\s+(?<path>[^,}\s]+)",
                RegexOptions.IgnoreCase);

            if (positionalMatch.Success)
                return positionalMatch.Groups["path"].Value.Trim();

            return null;
        }

        private static bool IsEmptyRelativeTransformElement(XmlElement relativeTransformElement)
        {
            if (relativeTransformElement == null)
                return true;

            var childElements = relativeTransformElement.ChildNodes.OfType<XmlElement>().ToList();
            if (childElements.Count == 0)
                return true;

            if (childElements.Count == 1 &&
                string.Equals(childElements[0].LocalName, "TransformGroup", StringComparison.OrdinalIgnoreCase))
            {
                return !childElements[0].ChildNodes.OfType<XmlElement>().Any();
            }

            return false;
        }

        /// <summary>
        /// Converts WPF *.LayoutTransform property elements to Avalonia LayoutTransformControl wrappers.
        /// WPF: &lt;TextBlock&gt;&lt;TextBlock.LayoutTransform&gt;&lt;RotateTransform/&gt;&lt;/TextBlock.LayoutTransform&gt;&lt;/TextBlock&gt;
        /// Avalonia: &lt;LayoutTransformControl&gt;&lt;LayoutTransformControl.LayoutTransform&gt;&lt;RotateTransform/&gt;&lt;/LayoutTransformControl.LayoutTransform&gt;&lt;TextBlock&gt;&lt;/TextBlock&gt;&lt;/LayoutTransformControl&gt;
        /// Layout attached properties (Grid.*, Canvas.*, etc.) are moved from the owner to the wrapper.
        /// </summary>
        private static string ConvertLayoutTransformToLayoutTransformControl(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                // Find all elements with .LayoutTransform property elements
                var layoutTransformElements = doc.SelectNodes("//*[contains(local-name(), '.LayoutTransform')]");
                if (layoutTransformElements == null || layoutTransformElements.Count == 0)
                    return xaml;

                string xamlNs = GetDefaultXamlNamespace(doc);

                // Process from the end to avoid index issues when modifying the DOM
                var layoutTransformList = layoutTransformElements.OfType<XmlNode>().OfType<XmlElement>().ToList();

                foreach (var layoutTransformElement in layoutTransformList)
                {
                    // Skip if the element has already been processed or removed from the DOM
                    if (layoutTransformElement.ParentNode == null)
                        continue;

                    if (!layoutTransformElement.LocalName.EndsWith(".LayoutTransform", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var ownerElement = layoutTransformElement.ParentNode as XmlElement;
                    if (ownerElement == null)
                        continue;

                    // Check if this element has already been wrapped (parent is already a LayoutTransformControl)
                    var grandParent = ownerElement.ParentNode as XmlElement;
                    if (grandParent != null &&
                        string.Equals(grandParent.LocalName, "LayoutTransformControl", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Create the LayoutTransformControl wrapper
                    var wrapper = doc.CreateElement("LayoutTransformControl", xamlNs);

                    // Layout panel attached properties to move from owner to wrapper
                    var layoutAttachedProperties = new[]
                    {
                        "Grid.Row", "Grid.RowSpan", "Grid.Column", "Grid.ColumnSpan",
                        "Canvas.Left", "Canvas.Top", "Canvas.Right", "Canvas.Bottom",
                        "Dock.Dock"
                    };

                    // Presentation properties to move from owner to wrapper
                    var presentationProperties = new[]
                    {
                        "HorizontalAlignment", "VerticalAlignment",
                        "HorizontalContentAlignment", "VerticalContentAlignment",
                        "Margin", "Panel.ZIndex"
                    };

                    bool hasLayoutProperty = false;

                    // Move layout attached properties to the wrapper
                    foreach (var propertyName in layoutAttachedProperties)
                    {
                        if (ownerElement.HasAttribute(propertyName))
                        {
                            wrapper.SetAttribute(propertyName, ownerElement.GetAttribute(propertyName));
                            hasLayoutProperty = true;
                        }
                    }

                    // Move presentation properties to the wrapper
                    foreach (var propertyName in presentationProperties)
                    {
                        if (ownerElement.HasAttribute(propertyName))
                        {
                            wrapper.SetAttribute(propertyName, ownerElement.GetAttribute(propertyName));
                            hasLayoutProperty = true;
                        }
                    }

                    // Clone the owner element (without the layout/presentation attributes that were moved)
                    var clonedOwner = (XmlElement)ownerElement.CloneNode(true);
                    if (hasLayoutProperty)
                    {
                        // Remove the moved attributes from the cloned owner
                        foreach (var propertyName in layoutAttachedProperties.Concat(presentationProperties))
                        {
                            clonedOwner.RemoveAttribute(propertyName);
                        }
                    }

                    // Remove the original *.LayoutTransform child from the cloned owner
                    foreach (var child in clonedOwner.ChildNodes.OfType<XmlElement>().ToList())
                    {
                        if (child.LocalName.EndsWith(".LayoutTransform", StringComparison.OrdinalIgnoreCase))
                        {
                            clonedOwner.RemoveChild(child);
                        }
                    }

                    // Create the new LayoutTransformControl.LayoutTransform property element
                    var newLayoutTransformElement = doc.CreateElement("LayoutTransformControl.LayoutTransform", xamlNs);
                    foreach (XmlAttribute attr in layoutTransformElement.Attributes)
                    {
                        newLayoutTransformElement.SetAttribute(attr.LocalName, attr.Value);
                    }
                    foreach (XmlNode child in layoutTransformElement.ChildNodes)
                    {
                        newLayoutTransformElement.AppendChild(doc.ImportNode(child, true));
                    }

                    // Build the wrapper: append cloned owner first, then the LayoutTransform property element
                    wrapper.AppendChild(clonedOwner);
                    wrapper.AppendChild(newLayoutTransformElement);

                    // Replace the original owner with the wrapper in the DOM
                    ownerElement.ParentNode?.ReplaceChild(wrapper, ownerElement);
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        /// <summary>
        /// Removes CenterX and CenterY attributes from ScaleTransform elements.
        /// WPF ScaleTransform supports CenterX/CenterY, but Avalonia ScaleTransform does not.
        /// </summary>
        private static string RemoveScaleTransformCenterProperties(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                var scaleTransforms = doc.SelectNodes("//*[local-name()='ScaleTransform']");
                if (scaleTransforms == null || scaleTransforms.Count == 0)
                    return xaml;

                foreach (var element in scaleTransforms.OfType<XmlNode>().OfType<XmlElement>())
                {
                    element.RemoveAttribute("CenterX");
                    element.RemoveAttribute("CenterY");
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ConvertResourceDictionarySourceToResourceInclude(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                string xamlNs = GetDefaultXamlNamespace(doc);
                if (string.IsNullOrEmpty(xamlNs))
                    return xaml;

                var resourcesNodes = doc.SelectNodes("//*[contains(local-name(), '.Resources')]");
                if (resourcesNodes == null || resourcesNodes.Count == 0)
                    return xaml;

                foreach (var resourcesElement in resourcesNodes.OfType<XmlNode>().OfType<XmlElement>())
                {
                    if (!resourcesElement.LocalName.EndsWith(".Resources", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var sourceDictionaries = resourcesElement
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "ResourceDictionary", StringComparison.OrdinalIgnoreCase)
                                    && e.HasAttribute("Source"))
                        .ToList();

                    var includeSources = new List<string>();
                    foreach (var sourceDictionary in sourceDictionaries)
                    {
                        string source = sourceDictionary.GetAttribute("Source");
                        string avaresSource = ConvertWpfResourceSourceToAvares(source);
                        if (!string.IsNullOrWhiteSpace(avaresSource))
                        {
                            includeSources.Add(avaresSource);
                        }

                        resourcesElement.RemoveChild(sourceDictionary);
                    }

                    if (includeSources.Count > 0)
                    {
                        var targetDictionary = resourcesElement
                            .ChildNodes
                            .OfType<XmlElement>()
                            .FirstOrDefault(e => string.Equals(e.LocalName, "ResourceDictionary", StringComparison.OrdinalIgnoreCase));

                        if (targetDictionary == null)
                        {
                            targetDictionary = doc.CreateElement("ResourceDictionary", xamlNs);
                            resourcesElement.AppendChild(targetDictionary);
                        }

                        var mergedDictionaries = targetDictionary
                            .ChildNodes
                            .OfType<XmlElement>()
                            .FirstOrDefault(e => string.Equals(e.LocalName, "ResourceDictionary.MergedDictionaries", StringComparison.OrdinalIgnoreCase));

                        if (mergedDictionaries == null)
                        {
                            mergedDictionaries = doc.CreateElement("ResourceDictionary.MergedDictionaries", xamlNs);
                            targetDictionary.AppendChild(mergedDictionaries);
                        }

                        var existingSources = new HashSet<string>(
                            mergedDictionaries
                                .ChildNodes
                                .OfType<XmlElement>()
                                .Where(e => string.Equals(e.LocalName, "ResourceInclude", StringComparison.OrdinalIgnoreCase))
                                .Select(e => e.GetAttribute("Source"))
                                .Where(s => !string.IsNullOrWhiteSpace(s)),
                            StringComparer.OrdinalIgnoreCase);

                        foreach (string source in includeSources)
                        {
                            if (existingSources.Contains(source))
                                continue;

                            var resourceInclude = doc.CreateElement("ResourceInclude", xamlNs);
                            resourceInclude.SetAttribute("Source", source);
                            mergedDictionaries.AppendChild(resourceInclude);
                            existingSources.Add(source);
                        }
                    }

                    // Handle nested WPF dictionaries inside existing MergedDictionaries blocks:
                    // <ResourceDictionary Source="..." /> -> <ResourceInclude Source="avares://..." />
                    var mergedDictionariesNodes = resourcesElement.SelectNodes(".//*[local-name()='ResourceDictionary.MergedDictionaries']");
                    if (mergedDictionariesNodes == null)
                        continue;

                    foreach (var mergedDictionariesNode in mergedDictionariesNodes.OfType<XmlNode>().OfType<XmlElement>())
                    {
                        var nestedSourceDictionaries = mergedDictionariesNode
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "ResourceDictionary", StringComparison.OrdinalIgnoreCase)
                                        && e.HasAttribute("Source"))
                            .ToList();

                        if (nestedSourceDictionaries.Count == 0)
                            continue;

                        var nestedExistingSources = new HashSet<string>(
                            mergedDictionariesNode
                                .ChildNodes
                                .OfType<XmlElement>()
                                .Where(e => string.Equals(e.LocalName, "ResourceInclude", StringComparison.OrdinalIgnoreCase))
                                .Select(e => e.GetAttribute("Source"))
                                .Where(s => !string.IsNullOrWhiteSpace(s)),
                            StringComparer.OrdinalIgnoreCase);

                        foreach (var nestedSourceDictionary in nestedSourceDictionaries)
                        {
                            string source = nestedSourceDictionary.GetAttribute("Source");
                            string avaresSource = ConvertWpfResourceSourceToAvares(source);
                            if (string.IsNullOrWhiteSpace(avaresSource))
                                continue;

                            if (!nestedExistingSources.Contains(avaresSource))
                            {
                                var resourceInclude = doc.CreateElement("ResourceInclude", xamlNs);
                                resourceInclude.SetAttribute("Source", avaresSource);
                                mergedDictionariesNode.AppendChild(resourceInclude);
                                nestedExistingSources.Add(avaresSource);
                            }

                            mergedDictionariesNode.RemoveChild(nestedSourceDictionary);
                        }
                    }
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ConvertWpfResourceSourceToAvares(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            string normalized = source.Trim().Replace('\\', '/');
            if (normalized.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeAvaresExtension(normalized);
            }

            const string packPrefix = "pack://application:,,,/";
            if (normalized.StartsWith(packPrefix, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(packPrefix.Length);
            }

            if (normalized.StartsWith("/", StringComparison.Ordinal))
            {
                normalized = normalized.Substring(1);
            }

            int componentIndex = normalized.IndexOf(";component/", StringComparison.OrdinalIgnoreCase);
            if (componentIndex <= 0)
                return null;

            string assemblyName = normalized.Substring(0, componentIndex);
            string path = normalized.Substring(componentIndex + ";component/".Length).TrimStart('/');

            if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(path))
                return null;

            if (!assemblyName.EndsWith(".avui", StringComparison.OrdinalIgnoreCase))
            {
                assemblyName += ".avui";
            }

            if (path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - ".xaml".Length) + ".axaml";
            }

            return $"avares://{assemblyName}/{path}";
        }

        private static string NormalizeAvaresExtension(string avaresUri)
        {
            if (string.IsNullOrWhiteSpace(avaresUri))
                return avaresUri;

            if (avaresUri.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                return avaresUri.Substring(0, avaresUri.Length - ".xaml".Length) + ".axaml";
            }

            return avaresUri;
        }

        private static string RemovePrefixedVBBindingParameterNames(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                // Match {vb:VBBinding ...} and keep nested single-level braces in arguments.
                return Regex.Replace(
                    xaml,
                    @"\{vb:VBBinding(?<body>(?:[^{}]|\{[^{}]*\})*)\}",
                    m =>
                    {
                        string body = m.Groups["body"].Value;
                        if (string.IsNullOrEmpty(body))
                            return m.Value;

                        // Remove namespace prefixes from argument names only:
                        // " vb:VBContent=..." -> " VBContent=..."
                        string normalizedBody = Regex.Replace(
                            body,
                            @"(^|[\s,])(?:[a-zA-Z_][\w.]*)\:([a-zA-Z_][\w.]*)\s*=",
                            "$1$2=",
                            RegexOptions.Singleline);

                        return "{vb:VBBinding" + normalizedBody + "}";
                    },
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static void ConvertPercentAttribute(XmlElement element, string attributeName)
        {
            if (element == null || string.IsNullOrWhiteSpace(attributeName) || !element.HasAttribute(attributeName))
                return;

            var raw = element.GetAttribute(attributeName);
            var converted = ConvertNumericValueToPercent(raw);
            if (!string.IsNullOrWhiteSpace(converted))
            {
                element.SetAttribute(attributeName, converted);
            }
        }

        private static void ConvertPercentPointAttribute(XmlElement element, string attributeName)
        {
            if (element == null || string.IsNullOrWhiteSpace(attributeName) || !element.HasAttribute(attributeName))
                return;

            var raw = element.GetAttribute(attributeName);
            if (string.IsNullOrWhiteSpace(raw))
                return;

            var parts = raw.Split(',');
            if (parts.Length != 2)
                return;

            var x = ConvertNumericValueToPercent(parts[0]);
            var y = ConvertNumericValueToPercent(parts[1]);
            if (string.IsNullOrWhiteSpace(x) || string.IsNullOrWhiteSpace(y))
                return;

            element.SetAttribute(attributeName, $"{x},{y}");
        }

        private static string ConvertNumericValueToPercent(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            string text = raw.Trim();
            if (text.EndsWith("%", StringComparison.Ordinal))
                return text;

            if (!double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value))
                return null;

            // WPF commonly stores these values as 0..1; Avalonia expects percent.
            // If the value already appears to be a percent number (e.g. 50), keep the magnitude.
            double percent = value <= 1d ? value * 100d : value;
            int rounded = (int)Math.Round(percent);
            return $"{rounded}%";
        }

        private static string ConvertLineCoordinatesToStartEndPoint(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                var lineNodes = doc.SelectNodes("//*[local-name()='Line']");
                if (lineNodes == null || lineNodes.Count == 0)
                    return xaml;

                foreach (var line in lineNodes.OfType<XmlNode>().OfType<XmlElement>())
                {
                    var x1 = line.GetAttribute("X1");
                    var y1 = line.GetAttribute("Y1");
                    var x2 = line.GetAttribute("X2");
                    var y2 = line.GetAttribute("Y2");

                    if (!line.HasAttribute("StartPoint") && line.HasAttribute("X1") && line.HasAttribute("Y1"))
                    {
                        line.SetAttribute("StartPoint", $"{x1},{y1}");
                    }

                    if (!line.HasAttribute("EndPoint") && line.HasAttribute("X2") && line.HasAttribute("Y2"))
                    {
                        line.SetAttribute("EndPoint", $"{x2},{y2}");
                    }

                    // Remove obsolete WPF line coordinate attributes after conversion.
                    line.RemoveAttribute("X1");
                    line.RemoveAttribute("Y1");
                    line.RemoveAttribute("X2");
                    line.RemoveAttribute("Y2");
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ConvertStrokeStartEndLineCapToStrokeLineCap(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                // Any element can derive from Shape, so inspect all elements for legacy attributes.
                var elements = doc.SelectNodes("//*");
                if (elements == null || elements.Count == 0)
                    return xaml;

                foreach (var element in elements.OfType<XmlNode>().OfType<XmlElement>())
                {
                    bool hasStart = element.HasAttribute("StrokeStartLineCap");
                    bool hasEnd = element.HasAttribute("StrokeEndLineCap");
                    if (!hasStart && !hasEnd)
                        continue;

                    // Keep existing StrokeLineCap if already present; otherwise map from legacy attributes.
                    if (!element.HasAttribute("StrokeLineCap"))
                    {
                        string start = element.GetAttribute("StrokeStartLineCap");
                        string end = element.GetAttribute("StrokeEndLineCap");

                        string mappedValue = !string.IsNullOrWhiteSpace(start) ? start : end;
                        if (!string.IsNullOrWhiteSpace(mappedValue))
                        {
                            element.SetAttribute("StrokeLineCap", mappedValue);
                        }
                    }

                    element.RemoveAttribute("StrokeStartLineCap");
                    element.RemoveAttribute("StrokeEndLineCap");
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        static public string CheckOrUpdateNamespaceInLayout(string xmlLayout)
        {
            if (String.IsNullOrEmpty(xmlLayout))
                return xmlLayout;
            // Beispiel XAML-Layout
            // <StackPanel>
            //      <Button x:Name="BtnKlick">Klick mich</Button>
            //      <TextBox x:Name="TBAusgabe">HalliHallo</TextBox>
            //  </StackPanel>

            string xmlDeclaration = "";
            string xamlContent = xmlLayout;
            
            // Falls <?xml ....> Deklaration
            int posXmlDekl = xmlLayout.IndexOf("<?xml");
            if (posXmlDekl >= 0)
            {
                // Finde nächste öffnende Klammer
                int posNextTag = xmlLayout.IndexOf("<", posXmlDekl + 1);
                if (posNextTag >= 0)
                {
                    // Preserve the XML declaration
                    xmlDeclaration = xmlLayout.Substring(0, posNextTag);
                    xamlContent = xmlLayout.Substring(posNextTag);
                }
            }

            int pos1 = xamlContent.IndexOf(">");
            string part1 = "";
            string part2 = "";
            if (pos1 > 0)
            {
                part1 = xamlContent.Substring(0, pos1);
                part2 = xamlContent.Substring(pos1);
            }
            // Falls keine Namespace-Deklaration im Root-Element vorhanden, dann füge ein
            if (part1.IndexOf("xmlns") < 0)
            {
                foreach (string nameSpace in ACxmlnsResolver.NamespaceList)
                {
                    part1 += " " + nameSpace;
                }
            }
            
            return xmlDeclaration + part1 + part2;
        }

        public static readonly (string WpfPattern, string AvaloniaReplacement, bool IsRegex)[] C_AvaloniaFindAndReplace = new[]
        {
            // Simple string replacements (fast, non-regex)
            ("<Style ", "<ControlTheme ", false),
            ("<Style.Setters>", "<ControlTheme.Setters>", false),
            ("</Style.Setters>", "</ControlTheme.Setters>", false),
            ("</Style>", "</ControlTheme>", false),
            // Convert *.Style property elements to *.Theme (e.g. Border.Style → Border.Theme)
            // The leading dot ensures we don't accidentally match names like GraphEdgeStyle.
            (@"\.(Style)(\s*(?:/>|>))", @".Theme$2", true),
            // Convert Setter Property="Visibility" to Property="IsVisible" with value conversion
            // These remain inside ControlTheme when TargetType differs from the owner element.
            (@"Property=""Visibility""\s+Value=""Collapsed""", @"Property=""IsVisible"" Value=""False""", true),
            (@"Property=""Visibility""\s+Value=""Hidden""", @"Property=""IsVisible"" Value=""False""", true),
            (@"Property=""Visibility""\s+Value=""Visible""", @"Property=""IsVisible"" Value=""True""", true),
            // Fallback: convert Property="Visibility" without value change (may need manual review)
            (@"Property=""Visibility""", @"Property=""IsVisible""", true),
            (" ToolTip=", " ToolTip.Tip=", false),
            ("DataGrid.Columns", "vb:VBDataGrid.Columns", false),
            (@"<DataGridTextColumn(?=[\s>])", "<vb:VBDataGridTextColumn", true),
            (@"</DataGridTextColumn(?=\s*>)", "</vb:VBDataGridTextColumn", true),
            ("AllowDrop=", "DragDrop.AllowDrop=", false),
            ("<Style", "<ControlTheme", false),
            ("</Style", "</ControlTheme", false),    
            (" Style=", " Theme=", false),    
            (".TreeItemTemplate", ".ItemTemplate", false),
            (" VirtualizingStackPanel.IsVirtualizing=\"True\"", " ", false),
            (" VirtualizingStackPanel.VirtualizationMode=\"Recycling\"", " ", false),
            (" EnableRowVirtualization=\"True\"", "", false),
            (" ScrollViewerVisibility=\"Hidden\"", " ScrollViewerVisibility=\"False\"", false),
            (" Visibility=\"Hidden\"", " IsVisible=\"False\"", false),
            (" Visibility=\"Collapsed\"", " IsVisible=\"False\"", false),
            (" Visibility=\"Visible\"", " IsVisible=\"True\"", false),
            (" Key=\"", " x:Key=\"", false),
            (" SumVisibility=\"Visible\"", " SumVisibility=\"True\"", false),
            (" FillRule=\"Nonzero\"", " FillRule=\"NonZero\"", false),
            (" MouseDown=", " PointerPressed=", false),
            (" MouseLeftButtonDown=", " PointerPressed=", false),
            (" MouseRightButtonDown=", " PointerPressed=", false),
            (" MouseUp=", " PointerReleased=", false),
            (" MouseLeftButtonUp=", " PointerReleased=", false),
            (" MouseRightButtonUp=", " PointerReleased=", false),
            (" MouseMove=", " PointerMoved=", false),
            (" MouseEnter=", " PointerEntered=", false),
            (" MouseLeave=", " PointerExited=", false),
            (" MouseWheel=", " PointerWheelChanged=", false),
            (@" ?PreviewMouseLeftButtonDown=""\{vb:VBDelegate (.*?)\}""", @" PointerPressed=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewMouseRightButtonDown=""\{vb:VBDelegate (.*?)\}""", @" PointerPressed=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewMouseLeftButtonUp=""\{vb:VBDelegate (.*?)\}""", @" PointerReleased=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewMouseRightButtonUp=""\{vb:VBDelegate (.*?)\}""", @" PointerReleased=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewKeyDown=""\{vb:VBDelegate (.*?)\}""", @" KeyDown=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewKeyUp=""\{vb:VBDelegate (.*?)\}""", @" KeyUp=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewMouseLeftButtonDown=""\{vb:VBDelegateExtension (.*?)\}""", @" PointerPressed=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewMouseRightButtonDown=""\{vb:VBDelegateExtension (.*?)\}""", @" PointerPressed=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewMouseLeftButtonUp=""\{vb:VBDelegateExtension (.*?)\}""", @" PointerReleased=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewMouseRightButtonUp=""\{vb:VBDelegateExtension (.*?)\}""", @" PointerReleased=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewKeyDown=""\{vb:VBDelegateExtension (.*?)\}""", @" KeyDown=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            (@" ?PreviewKeyUp=""\{vb:VBDelegateExtension (.*?)\}""", @" KeyUp=""{vb:VBDelegate $1, HandlePreviewEvents=True}""", true),
            
            (" ColorInterpolationMode=\"SRgbLinearInterpolation\"", " ", false),
            (" MappingMode=\"RelativeToBoundingBox\"", " ", false),
            ("Property=\"X2\" Value=\"1\"", "Property=\"EndPoint\" Value=\"1,0\"", false),
            ("Property=\"Y2\" Value=\"1\"", "Property=\"EndPoint\" Value=\"0,1\"", false),
            ("RelativeSource={x:Static RelativeSource.Self}}", "RelativeSource={RelativeSource Self}}", false),
            ("StrokeLineJoin=", "StrokeJoin=", false),
            (" RelativeTransform=\"Identity\"", " ", false),
            (" Transform=\"Identity\"", " ", false),
            ("GlassEffect=\"Visible\"", "GlassEffect=\"True\"", false),
            ("GlassEffect=\"Hidden\"", "GlassEffect=\"False\"", false),
            ("GlassEffect=\"Collapsed\"", "GlassEffect=\"False\"", false),
            ("Rotor=\"Visible\"", "Rotor=\"True\"", false),
            ("Rotor=\"Hidden\"", "Rotor=\"False\"", false),
            ("Rotor=\"Collapsed\"", "Rotor=\"False\"", false),
            ("<vb:VBInstanceInfo x:Key=", "<vb:VBInstanceInfo Key=", false),
            ("Visibility=\"{vb:VBBinding Converter={vb:ConverterVisibilitySingle UseCollapsed=True,c ACUrlCommand=\\Environment!GetVisibilityForPANotifyState},", "IsVisible=\"{vb:VBBinding Converter={vb:ConverterObject ACUrlCommand=\\Environment!GetVisibilityForPANotifyStateAv},", false),
            ("ListBox.ItemContainerStyle", "ListBox.ItemContainerTheme", false),
            // Visibility to IsVisible converters
            (@"\bVisibility=""\{vb:VBBinding\s+Converter=\{vb:VisibilityNullConverter\}(.*?)\}""", @"IsVisible=""{vb:VBBinding Converter={x:Static vb:IsVisibleNullConverter.Current}$1}""", true),
            (@"\bVisibility=""\{vb:VBBinding\s+(.*?),\s*Converter=\{vb:VisibilityNullConverter\}\}""", @"IsVisible=""{vb:VBBinding $1, Converter={x:Static vb:IsVisibleNullConverter.Current}}""", true),
            (@"\bVisibility=""\{vb:VBBinding\s+Converter=\{vb:ConverterVisibilityBool\}(.*?)\}""", @"IsVisible=""{vb:VBBinding Converter={x:Static vb:ConverterIsVisibleBool.Current}$1}""", true),
            (@"\bVisibility=""\{vb:VBBinding\s+Converter=\{vb:ConverterVisibilityInverseBool\}(.*?)\}""", @"IsVisible=""{vb:VBBinding Converter={x:Static vb:ConverterIsVisibleInverseBool.Current}$1}""", true),
            (@"\bVisibility=""\{vb:VBBinding\s+Converter=\{vb:ConverterVisibilitySingle(.*?)\}(.*?)\}""", @"IsVisible=""{vb:VBBinding Converter={vb:ConverterIsVisibleSingle$1}$2}""", true),
            (@"\bVisibility=""\{vb:VBBinding\s+(.*?),\s*Converter=\{vb:ConverterControlModesVisibility\}\}""", @"IsVisible=""{vb:VBBinding $1, Converter={x:Static vb:ConverterControlModesVisibility.Current}}""", true),
            (@"\bVisibility=""\{Binding\s+Converter=\{vb:VisibilityNullConverter\}(.*?)\}""", @"IsVisible=""{Binding Converter={x:Static vb:IsVisibleNullConverter.Current}$1}""", true),
            (@"\bVisibility=""\{Binding\s+(.*?),\s*Converter=\{vb:VisibilityNullConverter\}\}""", @"IsVisible=""{Binding $1, Converter={x:Static vb:IsVisibleNullConverter.Current}}""", true),
            (@"\bVisibility=""\{Binding\s+(.*?),\s*Converter=\{vb:ConverterVisibilityBool\}\}""", @"IsVisible=""{Binding $1, Converter={x:Static vb:ConverterIsVisibleBool.Current}}""", true),
            (@"\bVisibility=""\{Binding\s+(.*?),\s*Converter=\{vb:ConverterVisibilityInverseBool\}\}""", @"IsVisible=""{Binding $1, Converter={x:Static vb:ConverterIsVisibleInverseBool.Current}}""", true),
            (@"\bVisibility=""\{Binding\s+Converter=\{vb:ConverterVisibilitySingle(.*?)\}(.*?)\}""", @"IsVisible=""{Binding Converter={vb:ConverterIsVisibleSingle$1}$2}""", true),
            (@"\bVisibility=""\{Binding\s+(.*?),\s*Converter=\{vb:ConverterControlModesVisibility\}\}""", @"IsVisible=""{Binding $1, Converter={x:Static vb:ConverterControlModesVisibility.Current}}""", true),

            // Regex-based patterns for complex multi-line replacements
            (@"<vb:VBTreeView\.TreeItemTemplate>\s*<DataTemplate>", "<TreeView.ItemTemplate>\n    <TreeDataTemplate ItemsSource=\"{Binding VisibleItemsT}\">", true),
            (@"<vb:VBTreeView\.ItemTemplate>\s*<DataTemplate>", "<TreeView.ItemTemplate>\n    <TreeDataTemplate ItemsSource=\"{Binding VisibleItemsT}\">", true),
            (@"<TreeView\.TreeItemTemplate>\s*<DataTemplate>", "<TreeView.ItemTemplate>\n    <TreeDataTemplate ItemsSource=\"{Binding VisibleItemsT}\">", true),
            (@"<TreeView\.ItemTemplate>\s*<DataTemplate>", "<TreeView.ItemTemplate>\n    <TreeDataTemplate ItemsSource=\"{Binding VisibleItemsT}\">", true),
            (@"</DataTemplate>\s*</vb:VBTreeView\.TreeItemTemplate>", "</TreeDataTemplate>\n</TreeView.ItemTemplate>", true),
            (@"</DataTemplate>\s*</vb:VBTreeView\.ItemTemplate>", "</TreeDataTemplate>\n</TreeView.ItemTemplate>", true),
            (@"</DataTemplate>\s*</TreeView\.TreeItemTemplate>", "</TreeDataTemplate>\n</TreeView.ItemTemplate>", true),
            (@"</DataTemplate>\s*</TreeView\.ItemTemplate>", "</TreeDataTemplate>\n</TreeView.ItemTemplate>", true),
            (@"\bACStateBrush\b", "ACStateBrushAv", true),
            (@"\bBrushButtonText\b", "BrushButtonTextAv", true),
            (@"\bBrushOnOff\b", "BrushOnOffAv", true),
            (@"\bBrushTextRed\b", "BrushTextRedAv", true),
            (@"\bDropShadowEffect\b", "DropShadowDirectionEffect", true),
            (@"\bUIElement\b", "Control", true),
            (@"\bFrameworkElement\b", "Control", true),

            // Remove CenterX and CenterY from SkewTransform
            (@"<SkewTransform\s+([^>]*?)CenterX=""[^""]*""\s*", @"<SkewTransform $1", true),
            (@"<SkewTransform\s+([^>]*?)CenterY=""[^""]*""\s*", @"<SkewTransform $1", true),

            // Remove obsolete Shape property not available in Avalonia.
            (@"\s+StrokeMiterLimit=""[^""]*""", "", true),

            // Pen property mapping: StartLineCap -> LineCap, removal of EndLineCap
            (@"<Pen\s+([^>]*?)StartLineCap=""([^""]*)""([^>]*?)", @"<Pen $1LineCap=""$2""$3", true),
            (@"<Pen\s+([^>]*?)\s+EndLineCap=""[^""]*""([^>]*?)", @"<Pen $1$2", true),
            (@"<Pen\s+([^>]*?)\s+DashCap=""[^""]*""([^>]*?)", @"<Pen $1$2", true),

            // ImageBrush property mapping: Viewport -> SourceRect, ImageSource -> Source, removal of ViewportUnits
            (@"<ImageBrush\s+([^>]*?)Viewport=""([^""]*)""([^>]*?)", @"<ImageBrush $1SourceRect=""$2""$3", true),
            (@"<ImageBrush\s+([^>]*?)ImageSource=""([^""]*)""([^>]*?)", @"<ImageBrush $1Source=""$2""$3", true),
            (@"<ImageBrush\s+([^>]*?)\s+ViewportUnits=""[^""]*""([^>]*?)", @"<ImageBrush $1$2", true),
            (@"<DrawingBrush\s+([^>]*?)Viewbox=""([^""]*)""([^>]*?)", @"<DrawingBrush $1SourceRect=""$2""$3", true),
            (@"<DrawingBrush\s+([^>]*?)\s+ViewboxUnits=""[^""]*""([^>]*?)", @"<DrawingBrush $1$2", true),
            (@"<DrawingBrush\s+([^>]*?)x:Key=""[^""]*""([^>]*?)", @"<DrawingBrush $1$2", true),
            (@"<DrawingBrush\s+([^>]*?)\s+Key=""[^""]*""([^>]*?)", @"<DrawingBrush $1$2", true),

            // Convert Image with VBStaticResource to VBDynamicImage (with ResourceKey parameter)
            (@"<Image\s+([^>]*?)Source=""\{vb:VBStaticResource\s+ResourceKey=([^,}]+)[^}]*\}""([^>]*?)(/?>)", @"<vb:VBDynamicImage $1VBContent=""$2""$3$4", true),
            // Convert Image with VBStaticResource to VBDynamicImage (with positional ResourceKey)
            (@"<Image\s+([^>]*?)Source=""\{vb:VBStaticResource\s+([^,}]+)\}""([^>]*?)(/?>)", @"<vb:VBDynamicImage $1VBContent=""$2""$3$4", true),
            // Convert Image with VBStaticResource to VBDynamicImage (no ResourceKey - keep VBContent if already present)
            (@"<Image\s+([^>]*?)Source=""\{vb:VBStaticResource\s*\}""([^>]*?)(/?>)", @"<vb:VBDynamicImage $1$2$3", true),

            // Convert WPF raw matrix string (6 comma-separated numbers) to Avalonia matrix() syntax.
            // WPF: RenderTransform="M11,M12,M21,M22,OffX,OffY"
            // Avalonia: RenderTransform="matrix(M11,M12,M21,M22,OffX,OffY)"
            // Note: no spaces between values — Avalonia's ParseCommaDelimitedValues does not trim the last value.
            (@"RenderTransform=""(-?[\d.]+(?:[eE][+-]?\d+)?),\s*(-?[\d.]+(?:[eE][+-]?\d+)?),\s*(-?[\d.]+(?:[eE][+-]?\d+)?),\s*(-?[\d.]+(?:[eE][+-]?\d+)?),\s*(-?[\d.]+(?:[eE][+-]?\d+)?),\s*(-?[\d.]+(?:[eE][+-]?\d+)?)""", @"RenderTransform=""matrix($1,$2,$3,$4,$5,$6)""", true),

            // Convert StrokeDashArray spaces into commas for Avalonia
            // This pattern specifically targets space-separated numbers inside StrokeDashArray quotes.
            // Simplified regex to match single pairs and relies on multiple passes if needed, 
            // but global replacement in Regex.Replace (which is used here) handles it if the pattern is right.
            (@"\bStrokeDashArray=""([\d\s.]+)""", @"StrokeDashArray=""$1""", true), // Helper to find them, but we need a more surgical replacement for the spaces themselves.
            // Note: The conversion engine usually runs these patterns. If we want to replace spaces with commas INSIDE the value:
            // We use a lookahead/lookbehind approach or a specialized mapping.
            (@"\bStrokeDashArray=""([^""]*?\d)\s+(\d[^""]*?)\""", @"StrokeDashArray=""$1,$2""", true),

            // Note: xmlns removal from child elements is handled separately in XAMLDesign property to preserve root element xmlns
        };

        /// <summary>
        /// Checks whether the owner element's tag name matches the TargetType of a Style/ControlTheme.
        /// Returns true if they match or if TargetType is empty (meaning the style applies to the owner).
        /// Used to decide whether default setters can be safely promoted to owner attributes.
        /// </summary>
        private static bool OwnerMatchesTargetType(XmlElement ownerElement, string targetType)
        {
            if (string.IsNullOrWhiteSpace(targetType))
                return true;

            // Extract the type name from TargetType, e.g.:
            //   "{x:Type vb:VBGraphEdge}" -> "VBGraphEdge"
            //   "{x:Type Border}" -> "Border"
            //   "Border" -> "Border"
            string targetTypeName = targetType;
            if (targetType.IndexOf('{') >= 0 && targetType.IndexOf('}') >= 0)
            {
                var inner = targetType.Substring(targetType.IndexOf('{') + 1, targetType.LastIndexOf('}') - targetType.IndexOf('{') - 1);
                inner = inner.Trim();
                // Strip {x:Type ...} prefix
                const string typePrefix = "x:Type ";
                if (inner.StartsWith(typePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    inner = inner.Substring(typePrefix.Length).Trim();
                }
                // Strip namespace prefix (e.g. "vb:" or "ctrl:")
                int colonIndex = inner.IndexOf(':');
                if (colonIndex >= 0)
                {
                    inner = inner.Substring(colonIndex + 1);
                }
                targetTypeName = inner;
            }
            else if (targetType.IndexOf(':') > 0)
            {
                // Handle "ns:TypeName" without braces
                int colonIndex = targetType.LastIndexOf(':');
                targetTypeName = targetType.Substring(colonIndex + 1);
            }

            // Extract owner element's local name (without namespace prefix)
            string ownerLocalName = ownerElement.LocalName;

            return string.Equals(ownerLocalName, targetTypeName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Strips a leading XML declaration (&lt;?xml ...&gt;) from the input and returns it in <paramref name="declaration"/>.
        /// Returns the rest of the string (trimmed of leading whitespace).
        /// </summary>
        private static string StripXmlDeclaration(string xaml, out string declaration)
        {
            declaration = null;
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            var trimmed = xaml.TrimStart();
            if (trimmed.StartsWith("<?xml", StringComparison.Ordinal))
            {
                var closeIndex = trimmed.IndexOf("?>", StringComparison.Ordinal);
                if (closeIndex > 0)
                {
                    declaration = trimmed.Substring(0, closeIndex + 2).Trim();
                    return trimmed.Substring(closeIndex + 2).TrimStart();
                }
            }

            return xaml;
        }

        /// <summary>
        /// Wraps content of Setter.Value in ToolTip.Tip setters into a Template element.
        /// </summary>
        private static string WrapToolTipTipInTemplate(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var body = StripXmlDeclaration(xaml, out string xmlDecl);
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml("<Root>" + body + "</Root>");

                var setters = doc.SelectNodes("//*[local-name()='Setter']")
                    ?.OfType<XmlElement>()
                    .ToList();

                if (setters == null || setters.Count == 0)
                    return xaml;

                foreach (var setter in setters)
                {
                    var prop = setter.GetAttribute("Property");
                    if (!string.Equals(prop, "ToolTip.Tip", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(prop, "ToolTip", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var valueElement = setter.ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "Setter.Value", StringComparison.OrdinalIgnoreCase));

                    if (valueElement == null)
                        continue;

                    var childElements = valueElement.ChildNodes.OfType<XmlElement>().ToList();
                    if (childElements.Count == 0)
                        continue;

                    // Already wrapped in a single Template element.
                    if (childElements.Count == 1
                        && string.Equals(childElements[0].LocalName, "Template", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var nodesToWrap = valueElement.ChildNodes.Cast<XmlNode>().ToList();

                    XmlElement template = string.IsNullOrEmpty(valueElement.NamespaceURI)
                        ? doc.CreateElement("Template")
                        : doc.CreateElement("Template", valueElement.NamespaceURI);

                    valueElement.RemoveAll();
                    valueElement.AppendChild(template);

                    foreach (var node in nodesToWrap)
                    {
                        template.AppendChild(node);
                    }
                }

                var result = doc.DocumentElement?.InnerXml ?? body;
                if (!string.IsNullOrEmpty(xmlDecl))
                    result = xmlDecl + Environment.NewLine + result;

                return result;
            }
            catch
            {
                return xaml;
            }
        }

        /// <summary>
        /// Formats XAML with proper indentation and line breaks for readability.
        /// </summary>
        private static string FormatXaml(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var body = StripXmlDeclaration(xaml, out string xmlDecl);
                var doc = new XmlDocument();
                doc.PreserveWhitespace = false;
                doc.LoadXml("<Root>" + body + "</Root>");

                using (var sw = new StringWriter())
                {
                    var settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "  ",
                        NewLineHandling = NewLineHandling.Entitize,
                        OmitXmlDeclaration = true,
                        Encoding = Encoding.UTF8
                    };

                    using (var writer = XmlWriter.Create(sw, settings))
                    {
                        doc.DocumentElement.WriteContentTo(writer);
                    }

                    var result = sw.ToString();

                    // Restore the XML declaration at the top.
                    if (!string.IsNullOrEmpty(xmlDecl))
                        result = xmlDecl + Environment.NewLine + result;

                    return result;
                }
            }
            catch
            {
                return xaml;
            }
        }

        public static readonly (string WpfPattern, string AvaloniaReplacement, bool IsRegex)[] C_AvaloniaPostFindAndReplace = new[]
        {
            (" ToolTip=", " ToolTip.Tip=", false),
            ("Property=\"ToolTip\"", "Property=\"ToolTip.Tip\"", false)
        };
    }
}
