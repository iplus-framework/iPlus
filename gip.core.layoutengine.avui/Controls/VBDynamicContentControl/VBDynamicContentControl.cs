using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using gip.core.datamodel;
using System;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// A ContentControl that dynamically loads and applies ControlThemes based on VBContent and DataContext.
    /// Solves the issue where Avalonia's DataContext is not available during MarkupExtension.ProvideValue().
    /// </summary>
    public class VBDynamicContentControl : ContentControl
    {
        /// <summary>
        /// Defines the VBContent property
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBDynamicContentControl, string>(nameof(VBContent));

        /// <summary>
        /// Gets or sets the VBContent path for resolving the theme resource
        /// </summary>
        public string VBContent
        {
            get => GetValue(VBContentProperty);
            set => SetValue(VBContentProperty, value);
        }

        static VBDynamicContentControl()
        {
            DataContextProperty.Changed.AddClassHandler<VBDynamicContentControl>((x, e) => x.OnDataContextOrVBContentChanged());
            VBContentProperty.Changed.AddClassHandler<VBDynamicContentControl>((x, e) => x.OnDataContextOrVBContentChanged());
        }

        private void OnDataContextOrVBContentChanged()
        {
            if (DataContext == null)
                return;

            try
            {
                if (DataContext is IACObject acObject)
                {
                    IACObject objForDesign = acObject;
                    
                    // Resolve VBContent path if specified
                    if (!string.IsNullOrEmpty(VBContent))
                    {
                        object cmdResult = acObject.ACUrlCommand(VBContent);
                        if (cmdResult is IACObject acResult)
                            objForDesign = acResult;
                    }

                    // Handle DesignManagerToolItem - load theme from ResourceInclude
                    if (objForDesign is DesignManagerToolItem iconProvider)
                    {
                        try
                        {
                            ResourceInclude dict = new ResourceInclude((Uri)null) { Source = iconProvider.IconResourceDictUri };
                            
                            if (dict.TryGetResource(iconProvider.ResourceKey, null, out object resource))
                            {
                                if (resource is ControlTheme theme)
                                {
                                    Theme = theme;
                                    return;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("VBDynamicContentControl", "OnDataContextOrVBContentChanged", msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null && ex.InnerException.Message != null)
                    msg += " Inner:" + ex.InnerException.Message;

                if (datamodel.Database.Root?.Messages != null && 
                    datamodel.Database.Root.InitState == ACInitState.Initialized)
                {
                    datamodel.Database.Root.Messages.LogException(
                        nameof(VBDynamicContentControl), 
                        nameof(OnDataContextOrVBContentChanged), 
                        msg);
                }
            }
        }
    }
}
