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

            // Convert direct element trigger blocks (e.g. Border.Triggers/EventTrigger)
            // into behavior-based equivalents.
            avaloniaXAML = ConvertElementTriggersToBehaviors(avaloniaXAML);
            
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

                var stylePropertyNodes = doc.SelectNodes("//*[contains(local-name(), '.Style')]");
                if (stylePropertyNodes == null || stylePropertyNodes.Count == 0)
                    return xaml;

                var styleProperties = stylePropertyNodes
                    .OfType<XmlNode>()
                    .OfType<XmlElement>()
                    .ToList();

                foreach (var styleProperty in styleProperties)
                {
                    if (!styleProperty.LocalName.EndsWith(".Style", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var ownerElement = styleProperty.ParentNode as XmlElement;
                    if (ownerElement == null)
                        continue;

                    var controlTheme = styleProperty
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme", StringComparison.OrdinalIgnoreCase));

                    if (controlTheme == null)
                        continue;

                    var triggersElement = controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme.Triggers", StringComparison.OrdinalIgnoreCase));

                    // Copy default setter values from ControlTheme root and optional ControlTheme.Setters block.
                    var defaultSetters = new List<XmlElement>();
                    defaultSetters.AddRange(controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)));

                    var settersContainer = controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme.Setters", StringComparison.OrdinalIgnoreCase));

                    if (settersContainer != null)
                    {
                        defaultSetters.AddRange(settersContainer
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)));
                    }

                    int movedDefaultSetterCount = 0;
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
                            behavior.SetAttribute(attr.Name, attr.Value);
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

                        int actionCount = 0;
                        foreach (var setter in multiDataTrigger
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

                        if (actionCount > 0)
                        {
                            interactionBehaviors.AppendChild(behavior);
                            hasBehavior = true;
                        }
                    }

                    if (!hasBehavior)
                        continue;

                    ownerElement.ReplaceChild(interactionBehaviors, styleProperty);
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ConvertElementTriggersToBehaviors(string xaml)
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

                var triggersPropertyNodes = doc.SelectNodes("//*[contains(local-name(), '.Triggers') and not(local-name()='ControlTheme.Triggers')]");
                if (triggersPropertyNodes == null || triggersPropertyNodes.Count == 0)
                    return xaml;

                var triggersProperties = triggersPropertyNodes
                    .OfType<XmlNode>()
                    .OfType<XmlElement>()
                    .ToList();

                foreach (var triggersProperty in triggersProperties)
                {
                    if (!triggersProperty.LocalName.EndsWith(".Triggers", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var ownerElement = triggersProperty.ParentNode as XmlElement;
                    if (ownerElement == null)
                        continue;

                    var interactionBehaviors = doc.CreateElement("Interaction.Behaviors", xamlNs);
                    int behaviorCount = AppendEventTriggers(doc, xamlNs, ownerElement, interactionBehaviors, triggersProperty);

                    if (behaviorCount > 0)
                    {
                        ownerElement.ReplaceChild(interactionBehaviors, triggersProperty);
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

        private static int AppendEventTriggers(
            XmlDocument doc,
            string xamlNs,
            XmlElement ownerElement,
            XmlElement interactionBehaviors,
            XmlElement triggersContainer)
        {
            if (doc == null || ownerElement == null || interactionBehaviors == null || triggersContainer == null)
                return 0;

            int behaviorCount = 0;
            foreach (var eventTrigger in triggersContainer
                .ChildNodes
                .OfType<XmlElement>()
                .Where(e => string.Equals(e.LocalName, "EventTrigger", StringComparison.OrdinalIgnoreCase)))
            {
                string eventName = ConvertRoutedEventToEventName(eventTrigger.GetAttribute("RoutedEvent"), ownerElement.LocalName);
                if (string.IsNullOrWhiteSpace(eventName))
                    continue;

                var behavior = doc.CreateElement("EventTriggerBehavior", xamlNs);
                behavior.SetAttribute("EventName", eventName);

                int actionCount = AppendBeginStoryboardAnimations(doc, xamlNs, ownerElement, behavior, eventTrigger, null);
                if (actionCount > 0)
                {
                    interactionBehaviors.AppendChild(behavior);
                    behaviorCount++;
                }
            }

            return behaviorCount;
        }

        private static string ConvertRoutedEventToEventName(string routedEvent, string ownerTypeName)
        {
            if (string.IsNullOrWhiteSpace(routedEvent))
                return null;

            string text = routedEvent.Trim();
            int dotIndex = text.LastIndexOf('.');
            if (dotIndex >= 0 && dotIndex < text.Length - 1)
            {
                string eventOwner = text.Substring(0, dotIndex);
                string eventName = text.Substring(dotIndex + 1);

                if (string.IsNullOrWhiteSpace(ownerTypeName)
                    || string.Equals(eventOwner, ownerTypeName, StringComparison.OrdinalIgnoreCase)
                    || eventOwner.EndsWith(ownerTypeName, StringComparison.OrdinalIgnoreCase))
                {
                    return eventName;
                }

                return eventName;
            }

            return text;
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

                    if (sourceDictionaries.Count == 0)
                        continue;

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

                    if (includeSources.Count == 0)
                        continue;

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
            ("ToolTip=", "ToolTip.Tip=", false),
            ("DataGrid.Columns", "vb:VBDataGrid.Columns", false),
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
            (@"\bVisibility=""\{vb:VBBinding\s+Converter=\{vb:ConverterVisibilityBool\}(.*?)\}""", @"IsVisible=""{vb:VBBinding Converter={x:Static vb:ConverterIsVisibleBool.Current}$1}""", true),
            (@"\bVisibility=""\{vb:VBBinding\s+Converter=\{vb:ConverterVisibilityInverseBool\}(.*?)\}""", @"IsVisible=""{vb:VBBinding Converter={x:Static vb:ConverterIsVisibleInverseBool.Current}$1}""", true),
            (@"\bVisibility=""\{vb:VBBinding\s+Converter=\{vb:ConverterVisibilitySingle(.*?)\}(.*?)\}""", @"IsVisible=""{vb:VBBinding Converter={vb:ConverterIsVisibleSingle$1}$2}""", true),
            (@"\bVisibility=""\{Binding\s+Converter=\{vb:VisibilityNullConverter\}(.*?)\}""", @"IsVisible=""{Binding Converter={x:Static vb:IsVisibleNullConverter.Current}$1}""", true),
            (@"\bVisibility=""\{Binding\s+(.*?),\s*Converter=\{vb:ConverterVisibilityBool\}\}""", @"IsVisible=""{Binding $1, Converter={x:Static vb:ConverterIsVisibleBool.Current}}""", true),
            (@"\bVisibility=""\{Binding\s+(.*?),\s*Converter=\{vb:ConverterVisibilityInverseBool\}\}""", @"IsVisible=""{Binding $1, Converter={x:Static vb:ConverterIsVisibleInverseBool.Current}}""", true),
            (@"\bVisibility=""\{Binding\s+Converter=\{vb:ConverterVisibilitySingle(.*?)\}(.*?)\}""", @"IsVisible=""{Binding Converter={vb:ConverterIsVisibleSingle$1}$2}""", true),

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
            (@"\s+StrokeDashCap=""[^""]*""", "", true),

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
    }
}
