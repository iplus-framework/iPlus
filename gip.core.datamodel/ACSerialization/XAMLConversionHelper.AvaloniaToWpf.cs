// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
// <copyright file="XAMLConversionHelper.AvaloniaToWpf.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary>
// Reverse conversion: Avalonia XAML -> WPF XAML.
// Used when a Design was authored directly in Avalonia XAML and a WPF
// counterpart (XMLDesign) is required.
// </summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace gip.core.datamodel
{
    public static partial class XAMLConversionHelper
    {
        /// <summary>
        /// Converts Avalonia XAML back to WPF XAML using reverse namespace mappings
        /// and reverse find/replace patterns. This is the counterpart of
        /// <see cref="ConvertWpfToAvaloniaXaml"/> for designs that were written
        /// directly in Avalonia XAML and have no WPF predecessor.
        /// </summary>
        /// <param name="avaloniaXaml">The Avalonia XAML string to convert</param>
        /// <returns>Converted WPF XAML string</returns>
        public static string ConvertAvaloniaToWpfXaml(string avaloniaXaml)
        {
            if (string.IsNullOrEmpty(avaloniaXaml))
                return avaloniaXaml;

            string wpfXaml = avaloniaXaml;

            // 0) Reverse Interaction.Behaviors back to WPF triggers. This must run first,
            // while the document is still Avalonia XAML (default xmlns = avaloniaui),
            // because the emitted Style.Triggers elements are then renamed together with
            // their owning ControlTheme by the string rules below.
            wpfXaml = ConvertBehaviorsToTriggers(wpfXaml);

            // 1) Reverse namespace mappings (Avalonia -> WPF).
            // Note: the mapping table is not injective (several WPF namespaces map to the
            // same Avalonia namespace, e.g. clr-namespace:gip.core.layoutengine... and
            // http://www.iplus-framework.com/xaml both map to .../axaml). We use the
            // canonical WPF namespace (the first entry in the table that maps to the
            // Avalonia namespace), which matches how the forward converter emits layouts.
            foreach (var tuple in ACxmlnsResolver.C_AvaloniaNamespaceMapping)
            {
                if (string.Equals(tuple.WpfNamespace, tuple.AvaloniaNamespace, StringComparison.Ordinal))
                    continue; // identical in both worlds (e.g. the xaml namespace)
                wpfXaml = ReplaceOrdinal(wpfXaml, tuple.AvaloniaNamespace, tuple.WpfNamespace);
            }

            // 2) Reverse avares:// URIs back to WPF pack URIs.
            wpfXaml = ConvertAvaresToPackUris(wpfXaml);

            // 3) Reverse ResourceInclude back to merged ResourceDictionary Source.
            wpfXaml = ConvertResourceIncludeToResourceDictionarySource(wpfXaml);

            // 4) Apply reverse find/replace rules.
            foreach (var tuple in C_WpfFindAndReplace)
            {
                if (tuple.IsRegex)
                    wpfXaml = Regex.Replace(wpfXaml, tuple.WpfPattern, tuple.AvaloniaReplacement);
                else
                    wpfXaml = ReplaceOrdinal(wpfXaml, tuple.WpfPattern, tuple.AvaloniaReplacement);
            }

            // 5) Reverse percentage-based values back to WPF decimal notation.
            wpfXaml = ConvertPercentValuesToDecimal(wpfXaml);

            // 6) Reverse matrix(...) RenderTransform to raw WPF matrix string.
            wpfXaml = ConvertMatrixTransformToWpfMatrix(wpfXaml);

            // 7) Reverse Transform -> RelativeTransform on brushes.
            wpfXaml = ConvertTransformsToRelativeTransforms(wpfXaml);

            // 8) Restore xmlns declarations on child elements (WPF style keeps them).
            wpfXaml = RestoreChildXmlnsDeclarations(wpfXaml);

            return wpfXaml;
        }

        /// <summary>
        /// Ordinal (case-sensitive) string replace without regex.
        /// </summary>
        private static string ReplaceOrdinal(string input, string search, string replacement)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(search))
                return input;
            if (string.Equals(search, replacement, StringComparison.Ordinal))
                return input;
            return input.Replace(search, replacement);
        }

        /// <summary>
        /// Reverse rules for the forward converter's C_AvaloniaFindAndReplace table.
        /// Order matters: more specific rules must run before generic ones.
        /// </summary>
        public static readonly (string WpfPattern, string AvaloniaReplacement, bool IsRegex)[] C_WpfFindAndReplace = new[]
        {
            // --- ControlTheme back to Style ---
            ("<ControlTheme.Setters>", "<Style.Setters>", false),
            ("</ControlTheme.Setters>", "</Style.Setters>", false),
            ("</ControlTheme>", "</Style>", false),
            ("<ControlTheme ", "<Style ", false),
            ("<ControlTheme>", "<Style>", false),
            // *.Theme property elements back to *.Style (e.g. Border.Theme -> Border.Style)
            (@"\.Theme(\s*(?:/>|>))", ".Style$1", true),
            (" Theme=", " Style=", false),
            // DataGrid theme properties back to WPF style properties
            ("VBDataGrid.RowTheme", "VBDataGrid.RowStyle", false),
            ("VBDataGrid.CellTheme", "VBDataGrid.CellStyle", false),
            ("VBDataGrid.ColumnHeaderTheme", "VBDataGrid.ColumnHeaderStyle", false),
            ("VBDataGrid.RowGroupTheme", "VBDataGrid.RowGroupStyle", false),
            (@"Property=""RowTheme""", "Property=\"RowStyle\"", true),
            (@"Property=""CellTheme""", "Property=\"CellStyle\"", true),
            (@"Property=""ColumnHeaderTheme""", "Property=\"ColumnHeaderStyle\"", true),
            (@"Property=""RowGroupTheme""", "Property=\"RowGroupStyle\"", true),
            ("ListBox.ItemContainerTheme", "ListBox.ItemContainerStyle", false),

            // --- IsVisible back to Visibility ---
            (@"Property=""IsVisible""\s+Value=""False""", "Property=\"Visibility\" Value=\"Collapsed\"", true),
            (@"Property=""IsVisible""\s+Value=""True""", "Property=\"Visibility\" Value=\"Visible\"", true),
            (@"Property=""IsVisible""", "Property=\"Visibility\"", true),
            (" IsVisible=\"False\"", " Visibility=\"Collapsed\"", false),
            (" IsVisible=\"True\"", " Visibility=\"Visible\"", false),
            // IsVisible converters back to Visibility converters
            (@"IsVisible=""\{vb:VBBinding\s+Converter=\{x:Static vb:IsVisibleNullConverter\.Current\}([^""]*?)\}""", "Visibility=\"{vb:VBBinding Converter={vb:VisibilityNullConverter}$1}\"", true),
            (@"IsVisible=""\{vb:VBBinding\s+([^""]*?),\s*Converter=\{x:Static vb:IsVisibleNullConverter\.Current\}\}""", "Visibility=\"{vb:VBBinding $1, Converter={vb:VisibilityNullConverter}}\"", true),
            (@"IsVisible=""\{vb:VBBinding\s+Converter=\{x:Static vb:ConverterIsVisibleBool\.Current\}([^""]*?)\}""", "Visibility=\"{vb:VBBinding Converter={vb:ConverterVisibilityBool}$1}\"", true),
            (@"IsVisible=""\{vb:VBBinding\s+([^""]*?),\s*Converter=\{x:Static vb:ConverterIsVisibleBool\.Current\}\}""", "Visibility=\"{vb:VBBinding $1, Converter={vb:ConverterVisibilityBool}}\"", true),
            (@"IsVisible=""\{vb:VBBinding\s+([^""]*?),\s*Converter=\{x:Static vb:ConverterIsVisibleInverseBool\.Current\}\}""", "Visibility=\"{vb:VBBinding $1, Converter={vb:ConverterVisibilityInverseBool}}\"", true),
            (@"IsVisible=""\{vb:VBBinding\s+([^""]*?),\s*Converter=\{x:Static vb:ConverterControlModesVisibility\.Current\}\}""", "Visibility=\"{vb:VBBinding $1, Converter={vb:ConverterControlModesVisibility}}\"", true),
            (@"IsVisible=""\{vb:VBBinding\s+Converter=\{vb:ConverterIsVisibleSingle([^""]*?)\}([^""]*?)\}""", "Visibility=\"{vb:VBBinding Converter={vb:ConverterVisibilitySingle$1}$2}\"", true),
            (@"IsVisible=""\{Binding\s+([^""]*?),\s*Converter=\{x:Static vb:IsVisibleNullConverter\.Current\}\}""", "Visibility=\"{Binding $1, Converter={vb:VisibilityNullConverter}}\"", true),
            (@"IsVisible=""\{Binding\s+([^""]*?),\s*Converter=\{x:Static vb:ConverterIsVisibleBool\.Current\}\}""", "Visibility=\"{Binding $1, Converter={vb:ConverterVisibilityBool}}\"", true),
            (@"IsVisible=""\{Binding\s+([^""]*?),\s*Converter=\{x:Static vb:ConverterIsVisibleInverseBool\.Current\}\}""", "Visibility=\"{Binding $1, Converter={vb:ConverterVisibilityInverseBool}}\"", true),
            (@"IsVisible=""\{Binding\s+([^""]*?),\s*Converter=\{x:Static vb:ConverterControlModesVisibility\.Current\}\}""", "Visibility=\"{Binding $1, Converter={vb:ConverterControlModesVisibility}}\"", true),

            // --- iPlus-specific reverse rules ---
            ("vb:VBDataGrid.Columns", "DataGrid.Columns", false),
            ("vb:VBDataGrid.RowDetailsTemplate", "DataGrid.RowDetailsTemplate", false),
            ("vb:VBGridViewColumn.CellTemplate", "GridViewColumn.CellTemplate", false),
            ("vb:VBGridViewColumn.CellTemplateSelector", "GridViewColumn.CellTemplateSelector", false),
            ("<vb:VBDataGridTemplateColumn", "<DataGridTemplateColumn", false),
            ("</vb:VBDataGridTemplateColumn", "</DataGridTemplateColumn", false),
            ("vb:VBDataGridTemplateColumn.CellTemplateSelector", "DataGridTemplateColumn.CellTemplateSelector", false),
            ("vb:VBDataGridTemplateColumn.CellEditingTemplateSelector", "DataGridTemplateColumn.CellEditingTemplateSelector", false),
            ("vb:VBDataGridTemplateColumn.CellEditingTemplate", "DataGridTemplateColumn.CellEditingTemplate", false),
            ("vb:VBDataGridTemplateColumn.CellTemplate", "DataGridTemplateColumn.CellTemplate", false),
            ("<vb:VBDataGridTextColumn", "<DataGridTextColumn", false),
            ("</vb:VBDataGridTextColumn", "</DataGridTextColumn", false),
            (@"<vb:VBDynamicImage\s+([^>]*?)VBContent=""([^""]*)""([^>]*?)(/?>)", "<Image $1Source=\"{vb:VBStaticResource ResourceKey=$2}\"$3$4", true),
            // TreeDataTemplate back to HierarchicalDataTemplate
            (@"<TreeView\.ItemTemplate>\s*<TreeDataTemplate ItemsSource=""\{Binding VisibleItemsT\}"">", "<TreeView.ItemTemplate>\n    <HierarchicalDataTemplate>", true),
            (@"</TreeDataTemplate>\s*</TreeView\.ItemTemplate>", "</HierarchicalDataTemplate>\n</TreeView.ItemTemplate>", true),
            (".ItemTemplate", ".TreeItemTemplate", false),

            // --- Pointer events back to WPF mouse events ---
            (" PointerPressed=", " MouseLeftButtonDown=", false),
            (" PointerReleased=", " MouseLeftButtonUp=", false),
            (" PointerMoved=", " MouseMove=", false),
            (" PointerEntered=", " MouseEnter=", false),
            (" PointerExited=", " MouseLeave=", false),
            (" PointerWheelChanged=", " MouseWheel=", false),
            (@"\bIsPointerOver\b", "IsMouseOver", true),
            (@"\bControl\b", "UIElement", true),

            // --- DragDrop attached property back to plain attribute ---
            ("DragDrop.AllowDrop=", "AllowDrop=", false),

            // --- ToolTip.Tip back to ToolTip ---
            (" ToolTip.Tip=", " ToolTip=", false),
            ("Property=\"ToolTip.Tip\"", "Property=\"ToolTip\"", false),

            // --- Misc WPF-only attributes that were dropped/renamed ---
            ("StrokeJoin=", "StrokeLineJoin=", false),
            (" FillRule=\"NonZero\"", " FillRule=\"Nonzero\"", false),
            (" ScrollViewerVisibility=\"False\"", " ScrollViewerVisibility=\"Hidden\"", false),
            (" SumVisibility=\"True\"", " SumVisibility=\"Visible\"", false),
            (" SumVisibility=\"False\"", " SumVisibility=\"Collapsed\"", false),
            (" vb:SumVisibility=\"True\"", " vb:SumVisibility=\"Visible\"", false),
            (" vb:SumVisibility=\"False\"", " vb:SumVisibility=\"Collapsed\"", false),
            (" GlassEffect=\"True\"", " GlassEffect=\"Visible\"", false),
            (" GlassEffect=\"False\"", " GlassEffect=\"Hidden\"", false),
            (" Rotor=\"True\"", " Rotor=\"Visible\"", false),
            (" Rotor=\"False\"", " Rotor=\"Hidden\"", false),
            ("<vb:VBInstanceInfo Key=", "<vb:VBInstanceInfo x:Key=", false),
            ("RelativeSource={RelativeSource Self}}", "RelativeSource={x:Static RelativeSource.Self}}", false),
            ("Property=\"EndPoint\" Value=\"1,0\"", "Property=\"X2\" Value=\"1\"", false),
            ("Property=\"EndPoint\" Value=\"0,1\"", "Property=\"Y2\" Value=\"1\"", false),
            // x:Key back to plain Key where the forward converter added the x: prefix
            // (only for VBInstanceInfo-like cases handled above; generic x:Key stays).

            // --- OxyPlot Axis/Series Key attribute was restored from x:Key by the
            // forward converter; the reverse direction keeps Key as-is (WPF OxyPlot
            // uses Key as well). No rule needed.
        };

        /// <summary>
        /// Converts Avalonia avares:// URIs back to WPF pack://application:,,, URIs.
        /// e.g. "avares://gip.core.layoutengine.avui/Images/alarmChild.png"
        ///   -> "pack://application:,,,/gip.core.layoutengine;component/Images/alarmChild.png"
        /// The .avui suffix is stripped from the assembly name.
        /// </summary>
        private static string ConvertAvaresToPackUris(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml) || xaml.IndexOf("avares://", StringComparison.OrdinalIgnoreCase) < 0)
                return xaml;

            return Regex.Replace(xaml, @"avares://([\w.]+?)(?:\.avui)?/([^\s""<>]+)", m =>
            {
                string assembly = m.Groups[1].Value;
                string path = m.Groups[2].Value;
                return $"pack://application:,,,/{assembly};component/{path}";
            }, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Converts Avalonia ResourceInclude elements back to WPF merged ResourceDictionary
        /// Source entries.
        /// Avalonia: &lt;ResourceInclude Source="avares://Asm/Themes/Style.axaml"/&gt;
        /// WPF:      &lt;ResourceDictionary Source="pack://application:,,,/Asm;component/Themes/Style.xaml"/&gt;
        /// </summary>
        private static string ConvertResourceIncludeToResourceDictionarySource(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml) || xaml.IndexOf("<ResourceInclude", StringComparison.OrdinalIgnoreCase) < 0)
                return xaml;

            return Regex.Replace(xaml, @"<ResourceInclude\s+Source=""([^""]+)""\s*/>", m =>
            {
                string source = ConvertAvaresToPackUris(m.Groups[1].Value);
                if (source.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase))
                    source = source.Substring(0, source.Length - ".axaml".Length) + ".xaml";
                return $"<ResourceDictionary Source=\"{source}\"/>";
            }, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Reverses the percentage-based values that the forward converter produced:
        /// - CenterX/CenterY percentage integers back to WPF 0.0-1.0 decimals
        /// - StartPoint/EndPoint percentage strings back to decimal point pairs
        /// - RadialGradientBrush radii/center/origin percentages back to decimals
        /// </summary>
        private static string ConvertPercentValuesToDecimal(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            // CenterX/CenterY: "67" -> "0.67"
            xaml = Regex.Replace(xaml, @"\b(Center[XY])=""(\d{1,2})""", m =>
            {
                if (!double.TryParse(m.Groups[2].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                    return m.Value;
                return $"{m.Groups[1].Value}=\"{(val / 100.0).ToString(System.Globalization.CultureInfo.InvariantCulture)}\"";
            }, RegexOptions.IgnoreCase);

            // StartPoint/EndPoint: "46%,100%" -> "0.46,1"
            xaml = Regex.Replace(xaml, @"\b(StartPoint|EndPoint)=""([^""]+)""", m =>
            {
                string[] coords = m.Groups[2].Value.Split(',');
                if (coords.Length != 2)
                    return m.Value;
                if (!TryParsePercent(coords[0], out double x) || !TryParsePercent(coords[1], out double y))
                    return m.Value;
                return $"{m.Groups[1].Value}=\"{FormatDecimal(x)},{FormatDecimal(y)}\"";
            }, RegexOptions.IgnoreCase);

            // RadialGradientBrush attributes: "50%" -> "0.5"
            xaml = Regex.Replace(xaml, @"(<RadialGradientBrush\b[^>]*?\b(?:RadiusX|RadiusY|CenterX|CenterY|GradientOriginX|GradientOriginY))=""(\d{1,3}(?:\.\d+)?)%""", m =>
            {
                if (!double.TryParse(m.Groups[2].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                    return m.Value;
                return $"{m.Groups[1].Value}=\"{FormatDecimal(val / 100.0)}\"";
            }, RegexOptions.IgnoreCase);

            return xaml;
        }

        private static bool TryParsePercent(string text, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;
            text = text.Trim();
            bool isPercent = text.EndsWith("%", StringComparison.Ordinal);
            if (isPercent)
                text = text.Substring(0, text.Length - 1);
            if (!double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
                return false;
            if (isPercent)
                value /= 100.0;
            return true;
        }

        private static string FormatDecimal(double value)
        {
            return value.ToString("0.0############", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts Avalonia matrix(...) RenderTransform back to the raw WPF matrix string.
        /// Avalonia: RenderTransform="matrix(M11,M12,M21,M22,OffX,OffY)"
        /// WPF:      RenderTransform="M11,M12,M21,M22,OffX,OffY"
        /// </summary>
        private static string ConvertMatrixTransformToWpfMatrix(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml) || xaml.IndexOf("matrix(", StringComparison.OrdinalIgnoreCase) < 0)
                return xaml;

            return Regex.Replace(xaml, @"RenderTransform=""matrix\(([^)]+)\)""", m =>
            {
                return $"RenderTransform=\"{m.Groups[1].Value}\"";
            }, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Reverses Transform -> RelativeTransform on gradient brushes (attribute and
        /// property-element forms). Only applied inside brush elements, matching the
        /// forward converter's scope.
        /// </summary>
        private static string ConvertTransformsToRelativeTransforms(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            // Property elements: <LinearGradientBrush.Transform> -> <LinearGradientBrush.RelativeTransform>
            xaml = Regex.Replace(xaml, @"<(LinearGradientBrush|RadialGradientBrush)\.Transform>", "<$1.RelativeTransform>", RegexOptions.IgnoreCase);
            xaml = Regex.Replace(xaml, @"</(LinearGradientBrush|RadialGradientBrush)\.Transform>", "</$1.RelativeTransform>", RegexOptions.IgnoreCase);

            // Attributes on brush elements only.
            return Regex.Replace(xaml, @"<(LinearGradientBrush|RadialGradientBrush)\b([^>]*?)\sTransform=""", "<$1$2 RelativeTransform=\"", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// The forward converter removes xmlns declarations from child elements (WPF keeps
        /// them only on the root). For the WPF target this is not required - WPF resolves
        /// prefixes via the root declarations as well - so child content is returned as-is.
        /// Kept as a hook for future WPF-specific xmlns normalization.
        /// </summary>
        private static string RestoreChildXmlnsDeclarations(string xaml)
        {
            return xaml;
        }

        // =====================================================================
        // Interaction.Behaviors -> WPF Triggers (reverse of
        // ConvertControlThemeTriggersToBehaviors / ConvertResourceControlThemeTriggers)
        // =====================================================================

        /// <summary>
        /// Reverses the trigger-to-behavior conversion of the forward converter.
        ///
        /// The forward converter emits a deterministic structure that can be mapped back
        /// bijectively:
        ///   WPF DataTrigger     -> DataTriggerBehavior with Binding="..." attribute (copied verbatim)
        ///   WPF Trigger         -> DataTriggerBehavior with DataTriggerBehavior.Binding element
        ///                          containing &lt;Binding Path="P" RelativeSource="{RelativeSource Self}"/&gt;
        ///   WPF MultiDataTrigger-> DataTriggerBehavior Value="True" with a MultiBinding
        ///                          (Converter=ConverterMultiDataTrigger, ConverterParameter=expected values)
        ///   Setter              -> ChangePropertyAction PropertyName/Value
        ///   EnterActions/BeginStoryboard/Storyboard/DoubleAnimation
        ///                       -> BeginAnimationAction/Animation with KeyFrames (Cue 0% = From, 100% = To)
        ///   ExitActions/StopStoryboard
        ///                       -> synthetic inverse DataTriggerBehavior with the animation
        ///                          target properties as ChangePropertyActions
        ///
        /// Only behaviors that sit next to a *.Theme/*.Style property element (or inside a
        /// ControlTheme) are converted - hand-written Avalonia behaviors without a style
        /// context are left untouched.
        ///
        /// NOT reconstructible (forward converter dropped them or they are ambiguous):
        ///   - Default setters promoted to owner attributes (indistinguishable from
        ///     hand-written attributes; harmless - WPF keeps them as direct attributes).
        ///   - Resource-level ControlTheme triggers (forward converter dropped them entirely).
        ///   - SciChart/OxyPlot chart layouts (WPF uses its own chart implementation).
        /// </summary>
        private static string ConvertBehaviorsToTriggers(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml) ||
                xaml.IndexOf("Interaction.Behaviors", StringComparison.OrdinalIgnoreCase) < 0)
                return xaml;

            try
            {
                var doc = new XmlDocument { PreserveWhitespace = true };
                doc.LoadXml(xaml);

                if (doc.DocumentElement == null)
                    return xaml;

                string xamlNs = GetDefaultXamlNamespace(doc);
                if (string.IsNullOrEmpty(xamlNs))
                    return xaml;

                var behaviorsContainers = doc.SelectNodes("//*[local-name() = 'Interaction.Behaviors']");
                if (behaviorsContainers == null || behaviorsContainers.Count == 0)
                    return xaml;

                foreach (var containerNode in behaviorsContainers.OfType<XmlNode>().OfType<XmlElement>().ToList())
                {
                    var parent = containerNode.ParentNode as XmlElement;
                    if (parent == null)
                        continue;

                    // Determine where the triggers belong:
                    //  - parent is a ControlTheme (forward converter case: style targets a
                    //    different type than the owner) -> triggers go into the ControlTheme.
                    //  - parent is a control with a *.Theme/*.Style property element
                    //    (forward converter case: style targets the owner) -> triggers go
                    //    into that ControlTheme.
                    XmlElement controlTheme;
                    if (IsControlThemeElement(parent))
                    {
                        controlTheme = parent;
                    }
                    else
                    {
                        controlTheme = parent
                            .ChildNodes
                            .OfType<XmlElement>()
                            .FirstOrDefault(e => e.LocalName.EndsWith(".Theme", StringComparison.OrdinalIgnoreCase) ||
                                                 e.LocalName.EndsWith(".Style", StringComparison.OrdinalIgnoreCase))
                            ?.ChildNodes
                            .OfType<XmlElement>()
                            .FirstOrDefault(IsControlThemeElement);

                        // No style context: these are hand-written Avalonia behaviors, not
                        // converted triggers. Leave them untouched.
                        if (controlTheme == null)
                            continue;
                    }

                    var behaviors = containerNode
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "DataTriggerBehavior", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (behaviors.Count == 0)
                        continue;

                    var triggersElement = doc.CreateElement("Style.Triggers", xamlNs);
                    bool hasTrigger = false;
                    var consumedBehaviors = new HashSet<XmlElement>();

                    foreach (var behavior in behaviors)
                    {
                        if (consumedBehaviors.Contains(behavior))
                            continue;

                        var trigger = BuildTriggerFromBehavior(doc, xamlNs, behavior);
                        if (trigger == null)
                            continue;

                        // Detect the synthetic inverse "stop behavior" (reverse of
                        // ExitActions/StopStoryboard): a behavior bound to the same property
                        // with the inverted value whose ChangePropertyActions restore the
                        // animation end values.
                        if (TryFindAndConvertStopBehavior(doc, xamlNs, behaviors, behavior, trigger, consumedBehaviors))
                        {
                            // stop behavior was merged into trigger.ExitActions
                        }

                        triggersElement.AppendChild(trigger);
                        hasTrigger = true;
                    }

                    if (!hasTrigger)
                        continue;

                    // Insert Style.Triggers before Interaction.Behaviors position, then remove
                    // the behaviors container.
                    controlTheme.AppendChild(triggersElement);
                    parent.RemoveChild(containerNode);
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static bool IsControlThemeElement(XmlElement element)
        {
            return string.Equals(element.LocalName, "ControlTheme", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(element.LocalName, "Style", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Builds a WPF trigger element (Trigger/DataTrigger/MultiDataTrigger) from a
        /// DataTriggerBehavior element. Returns null if the behavior does not match one of
        /// the structures emitted by the forward converter.
        /// </summary>
        private static XmlElement BuildTriggerFromBehavior(XmlDocument doc, string xamlNs, XmlElement behavior)
        {
            string behaviorValue = behavior.GetAttribute("Value");

            // Case 1: WPF DataTrigger - the Binding was copied verbatim as an attribute.
            string bindingAttr = behavior.GetAttribute("Binding");
            if (!string.IsNullOrWhiteSpace(bindingAttr))
            {
                var dataTrigger = doc.CreateElement("DataTrigger", xamlNs);
                dataTrigger.SetAttribute("Binding", bindingAttr);
                dataTrigger.SetAttribute("Value", behaviorValue);
                AppendSettersFromBehavior(doc, xamlNs, behavior, dataTrigger);
                AppendEnterActionsFromBehavior(doc, xamlNs, behavior, dataTrigger);
                return dataTrigger;
            }

            // Case 2/3: Binding property element (Trigger or MultiDataTrigger).
            var bindingPropertyElement = behavior
                .ChildNodes
                .OfType<XmlElement>()
                .FirstOrDefault(e => string.Equals(e.LocalName, "DataTriggerBehavior.Binding", StringComparison.OrdinalIgnoreCase));

            var bindingElement = bindingPropertyElement?
                .ChildNodes
                .OfType<XmlElement>()
                .FirstOrDefault();
            if (bindingElement == null)
                return null;

            if (string.Equals(bindingElement.LocalName, "MultiBinding", StringComparison.OrdinalIgnoreCase))
            {
                // Case 3: WPF MultiDataTrigger.
                // The forward converter evaluated conditions centrally via
                // ConverterMultiDataTrigger with the expected values joined by ',' in
                // ConverterParameter. Split them back and serialize each child binding
                // to its markup form.
                string converterParameter = bindingElement.GetAttribute("ConverterParameter");
                if (string.IsNullOrWhiteSpace(converterParameter))
                    return null;

                var conditionBindings = bindingElement
                    .ChildNodes
                    .OfType<XmlElement>()
                    .ToList();
                var expectedValues = SplitTopLevelWpf(converterParameter, ',');
                if (conditionBindings.Count == 0 || expectedValues.Count != conditionBindings.Count)
                    return null;

                var multiDataTrigger = doc.CreateElement("MultiDataTrigger", xamlNs);
                var conditionsElement = doc.CreateElement("MultiDataTrigger.Conditions", xamlNs);
                for (int i = 0; i < conditionBindings.Count; i++)
                {
                    string bindingMarkup = SerializeBindingToMarkup(conditionBindings[i]);
                    if (string.IsNullOrWhiteSpace(bindingMarkup))
                        return null;

                    var condition = doc.CreateElement("Condition", xamlNs);
                    condition.SetAttribute("Binding", bindingMarkup);
                    condition.SetAttribute("Value", expectedValues[i]);
                    conditionsElement.AppendChild(condition);
                }
                multiDataTrigger.AppendChild(conditionsElement);
                AppendSettersFromBehavior(doc, xamlNs, behavior, multiDataTrigger);
                return multiDataTrigger;
            }

            // Case 2: WPF Trigger - Binding Path + RelativeSource Self.
            if (!string.Equals(bindingElement.LocalName, "Binding", StringComparison.OrdinalIgnoreCase))
                return null;

            string path = bindingElement.GetAttribute("Path");
            string relativeSource = bindingElement.GetAttribute("RelativeSource");
            if (string.IsNullOrWhiteSpace(path) ||
                relativeSource.IndexOf("RelativeSource.Self", StringComparison.OrdinalIgnoreCase) < 0)
                return null;

            var trigger = doc.CreateElement("Trigger", xamlNs);
            trigger.SetAttribute("Property", path);
            trigger.SetAttribute("Value", behaviorValue);
            AppendSettersFromBehavior(doc, xamlNs, behavior, trigger);
            AppendEnterActionsFromBehavior(doc, xamlNs, behavior, trigger);
            return trigger;
        }

        /// <summary>
        /// Appends WPF Setter elements (reverse-normalized) for all ChangePropertyAction
        /// children of the behavior.
        /// </summary>
        private static void AppendSettersFromBehavior(XmlDocument doc, string xamlNs, XmlElement behavior, XmlElement trigger)
        {
            foreach (var action in behavior
                .ChildNodes
                .OfType<XmlElement>()
                .Where(e => string.Equals(e.LocalName, "ChangePropertyAction", StringComparison.OrdinalIgnoreCase)))
            {
                string propertyName = action.GetAttribute("PropertyName");
                if (string.IsNullOrWhiteSpace(propertyName))
                    continue;

                var setter = doc.CreateElement("Setter", xamlNs);
                setter.SetAttribute("Property", DenormalizeTriggerPropertyName(propertyName));
                setter.SetAttribute("Value", DenormalizeTriggerPropertyValue(DenormalizeTriggerPropertyName(propertyName), action.GetAttribute("Value")));
                trigger.AppendChild(setter);
            }
        }

        /// <summary>
        /// Appends DataTrigger.EnterActions / Trigger.EnterActions with
        /// BeginStoryboard/Storyboard animations reconstructed from BeginAnimationAction
        /// children. Returns the list of animation target properties (used for
        /// stop-behavior detection).
        /// </summary>
        private static List<string> AppendEnterActionsFromBehavior(XmlDocument doc, string xamlNs, XmlElement behavior, XmlElement trigger)
        {
            var animationTargetProperties = new List<string>();

            var beginAnimationActions = behavior
                .ChildNodes
                .OfType<XmlElement>()
                .Where(e => string.Equals(e.LocalName, "BeginAnimationAction", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (beginAnimationActions.Count == 0)
                return animationTargetProperties;

            var enterActions = doc.CreateElement(trigger.LocalName + ".EnterActions", xamlNs);

            foreach (var beginAnimationAction in beginAnimationActions)
            {
                var animation = beginAnimationAction
                    .ChildNodes
                    .OfType<XmlElement>()
                    .FirstOrDefault(e => string.Equals(e.LocalName, "BeginAnimationAction.Animation", StringComparison.OrdinalIgnoreCase))
                    ?.ChildNodes
                    .OfType<XmlElement>()
                    .FirstOrDefault(e => string.Equals(e.LocalName, "Animation", StringComparison.OrdinalIgnoreCase));

                if (animation == null)
                    continue;

                string duration = animation.GetAttribute("Duration");
                if (string.IsNullOrWhiteSpace(duration))
                    continue;

                var keyFrames = animation
                    .ChildNodes
                    .OfType<XmlElement>()
                    .Where(e => string.Equals(e.LocalName, "KeyFrame", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                string from = GetKeyFrameValue(keyFrames, "0%");
                string to = GetKeyFrameValue(keyFrames, "100%");
                string targetProperty = GetKeyFrameProperty(keyFrames, "100%") ?? GetKeyFrameProperty(keyFrames, "0%");
                if (string.IsNullOrWhiteSpace(targetProperty) || string.IsNullOrWhiteSpace(to))
                    continue;

                var animationElementName = LooksLikeColor(to) ? "ColorAnimation" : "DoubleAnimation";
                var animationElement = doc.CreateElement(animationElementName, xamlNs);
                animationElement.SetAttribute("Storyboard.TargetProperty", targetProperty);
                if (!string.IsNullOrWhiteSpace(from))
                    animationElement.SetAttribute("From", from);
                animationElement.SetAttribute("To", to);
                animationElement.SetAttribute("Duration", duration);

                if (string.Equals(animation.GetAttribute("IterationCount"), "Infinite", StringComparison.OrdinalIgnoreCase))
                    animationElement.SetAttribute("RepeatBehavior", "Forever");

                if (string.Equals(animation.GetAttribute("PlaybackDirection"), "Alternate", StringComparison.OrdinalIgnoreCase))
                    animationElement.SetAttribute("AutoReverse", "True");

                var storyboard = doc.CreateElement("Storyboard", xamlNs);
                storyboard.AppendChild(animationElement);

                var beginStoryboard = doc.CreateElement("BeginStoryboard", xamlNs);
                beginStoryboard.AppendChild(storyboard);
                enterActions.AppendChild(beginStoryboard);

                animationTargetProperties.Add(targetProperty);
            }

            if (enterActions.HasChildNodes)
                trigger.AppendChild(enterActions);

            return animationTargetProperties;
        }

        private static string GetKeyFrameValue(List<XmlElement> keyFrames, string cue)
        {
            return keyFrames?
                .FirstOrDefault(k => string.Equals(k.GetAttribute("Cue"), cue, StringComparison.OrdinalIgnoreCase))
                ?.ChildNodes
                .OfType<XmlElement>()
                .FirstOrDefault(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase))
                ?.GetAttribute("Value");
        }

        private static string GetKeyFrameProperty(List<XmlElement> keyFrames, string cue)
        {
            return keyFrames?
                .FirstOrDefault(k => string.Equals(k.GetAttribute("Cue"), cue, StringComparison.OrdinalIgnoreCase))
                ?.ChildNodes
                .OfType<XmlElement>()
                .FirstOrDefault(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase))
                ?.GetAttribute("Property");
        }

        private static bool LooksLikeColor(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            string trimmed = value.Trim();
            if (trimmed.StartsWith("#", StringComparison.Ordinal))
                return true;
            // Named colors emitted by ColorAnimation (forward converter kept them verbatim).
            switch (trimmed.ToLowerInvariant())
            {
                case "red":
                case "green":
                case "blue":
                case "yellow":
                case "orange":
                case "black":
                case "white":
                case "gray":
                case "grey":
                case "transparent":
                case "lightgray":
                case "darkgray":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Detects the synthetic inverse "stop behavior" that the forward converter created
        /// for WPF Trigger.ExitActions/StopStoryboard and converts it into an
        /// ExitActions/StopStoryboard element on the given trigger.
        ///
        /// Detection criteria (all established by the forward converter):
        ///   - Binding property element with the same Path as the trigger property and
        ///     RelativeSource Self
        ///   - Value is the inverted trigger value
        ///   - ChangePropertyActions restore exactly the animation target properties
        /// </summary>
        private static bool TryFindAndConvertStopBehavior(
            XmlDocument doc,
            string xamlNs,
            List<XmlElement> behaviors,
            XmlElement sourceBehavior,
            XmlElement trigger,
            HashSet<XmlElement> consumedBehaviors)
        {
            // Only triggers with animations can have a stop behavior.
            var enterActions = trigger
                .ChildNodes
                .OfType<XmlElement>()
                .FirstOrDefault(e => string.Equals(e.LocalName, trigger.LocalName + ".EnterActions", StringComparison.OrdinalIgnoreCase));
            if (enterActions == null)
                return false;

            var animationTargetProperties = enterActions
                .SelectNodes(".//*[local-name() = 'DoubleAnimation' or local-name() = 'ColorAnimation']")
                ?.OfType<XmlElement>()
                .Select(e => e.GetAttribute("Storyboard.TargetProperty"))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList() ?? new List<string>();
            if (animationTargetProperties.Count == 0)
                return false;

            string triggerProperty = trigger.GetAttribute("Property");
            string triggerValue = trigger.GetAttribute("Value");
            string inverseValue = TryInvertBooleanLiteral(triggerValue);
            if (string.IsNullOrWhiteSpace(triggerProperty) || string.IsNullOrWhiteSpace(inverseValue))
                return false;

            foreach (var candidate in behaviors)
            {
                if (candidate == sourceBehavior || consumedBehaviors.Contains(candidate))
                    continue;

                var bindingPropertyElement = candidate
                    .ChildNodes
                    .OfType<XmlElement>()
                    .FirstOrDefault(e => string.Equals(e.LocalName, "DataTriggerBehavior.Binding", StringComparison.OrdinalIgnoreCase));
                var bindingElement = bindingPropertyElement?
                    .ChildNodes
                    .OfType<XmlElement>()
                    .FirstOrDefault();
                if (bindingElement == null ||
                    !string.Equals(bindingElement.LocalName, "Binding", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.Equals(bindingElement.GetAttribute("Path"), triggerProperty, StringComparison.Ordinal))
                    continue;
                if (!string.Equals(candidate.GetAttribute("Value"), inverseValue, StringComparison.Ordinal))
                    continue;

                var stopProperties = candidate
                    .ChildNodes
                    .OfType<XmlElement>()
                    .Where(e => string.Equals(e.LocalName, "ChangePropertyAction", StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.GetAttribute("PropertyName"))
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();

                if (stopProperties.Count != animationTargetProperties.Count ||
                    !stopProperties.All(p => animationTargetProperties.Contains(p, StringComparer.Ordinal)))
                    continue;

                var exitActions = doc.CreateElement(trigger.LocalName + ".ExitActions", xamlNs);
                exitActions.AppendChild(doc.CreateElement("StopStoryboard", xamlNs));
                trigger.AppendChild(exitActions);

                consumedBehaviors.Add(candidate);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reverse of the forward converter's NormalizeTriggerPropertyName:
        /// IsVisible -> Visibility.
        /// </summary>
        private static string DenormalizeTriggerPropertyName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return propertyName;

            if (string.Equals(propertyName, "IsVisible", StringComparison.OrdinalIgnoreCase))
                return "Visibility";

            return propertyName;
        }

        /// <summary>
        /// Reverse of the forward converter's NormalizeTriggerPropertyValue:
        /// Visibility True -> Visible, False -> Collapsed.
        /// </summary>
        private static string DenormalizeTriggerPropertyValue(string denormalizedPropertyName, string propertyValue)
        {
            if (!string.Equals(denormalizedPropertyName, "Visibility", StringComparison.OrdinalIgnoreCase))
                return propertyValue;

            if (string.Equals(propertyValue, "True", StringComparison.OrdinalIgnoreCase))
                return "Visible";

            if (string.Equals(propertyValue, "False", StringComparison.OrdinalIgnoreCase))
                return "Collapsed";

            return propertyValue;
        }

        /// <summary>
        /// Serializes a Binding element back to its markup-extension form,
        /// e.g. &lt;Binding Path="X" Converter="{x:Static vb:C.Current}"/&gt;
        /// -> "{Binding Path=X, Converter={x:Static vb:C.Current}}".
        /// </summary>
        private static string SerializeBindingToMarkup(XmlElement bindingElement)
        {
            if (bindingElement == null)
                return null;

            var args = new List<string>();

            foreach (XmlAttribute attr in bindingElement.Attributes)
            {
                if (string.Equals(attr.Name, "xmlns", StringComparison.Ordinal) || attr.Name.StartsWith("xmlns:", StringComparison.Ordinal))
                    continue;
                args.Add($"{attr.Name}={QuoteMarkupValue(attr.Value)}");
            }

            // Property-element children (e.g. Binding.Converter) become nested markup args.
            foreach (var child in bindingElement.ChildNodes.OfType<XmlElement>())
            {
                string localName = child.LocalName;
                int dotIndex = localName.IndexOf('.', StringComparison.Ordinal);
                if (dotIndex >= 0)
                    localName = localName.Substring(dotIndex + 1);
                string nested = SerializeBindingToMarkup(child);
                if (string.IsNullOrWhiteSpace(nested))
                    return null;
                args.Add($"{localName}={nested}");
            }

            if (args.Count == 0)
                return "{Binding}";

            return "{Binding " + string.Join(", ", args) + "}";
        }

        /// <summary>
        /// Quotes a markup-extension argument value when it contains characters that
        /// require quoting (commas, spaces, braces). Inverse of the forward converter's
        /// StripSurroundingQuotes/Unquote.
        /// </summary>
        private static string QuoteMarkupValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.StartsWith("{", StringComparison.Ordinal))
                return value; // nested markup extension

            if (value.IndexOfAny(new[] { ',', ' ', '{', '}' }) >= 0)
                return "'" + value + "'";

            return value;
        }

        /// <summary>
        /// Splits a string at the given separator, ignoring separators inside quotes or
        /// nested braces (inverse of the forward converter's SplitTopLevel).
        /// </summary>
        private static List<string> SplitTopLevelWpf(string input, char separator)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(input))
                return result;

            var sb = new StringBuilder();
            int braceDepth = 0;
            bool inSingleQuote = false;
            bool inDoubleQuote = false;

            foreach (char c in input)
            {
                if (inSingleQuote)
                {
                    sb.Append(c);
                    if (c == '\'')
                        inSingleQuote = false;
                    continue;
                }
                if (inDoubleQuote)
                {
                    sb.Append(c);
                    if (c == '"')
                        inDoubleQuote = false;
                    continue;
                }

                switch (c)
                {
                    case '{':
                        braceDepth++;
                        sb.Append(c);
                        break;
                    case '}':
                        braceDepth--;
                        sb.Append(c);
                        break;
                    case '\'':
                        inSingleQuote = true;
                        sb.Append(c);
                        break;
                    case '"':
                        inDoubleQuote = true;
                        sb.Append(c);
                        break;
                    default:
                        if (c == separator && braceDepth == 0)
                        {
                            result.Add(sb.ToString().Trim());
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }

            if (sb.Length > 0)
                result.Add(sb.ToString().Trim());

            return result;
        }
    }
}
