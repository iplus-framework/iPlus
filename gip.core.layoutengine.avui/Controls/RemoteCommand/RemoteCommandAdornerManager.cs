// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    public enum RemoteCommandPopupType
    {
        Default,
        Success,
        Error,
        Warning
    }

    public class RemoteCommandAdornerManager
    {
        private static RemoteCommandAdornerManager _instance;
        private static ResourceDictionary _templateResources;

        public static RemoteCommandAdornerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RemoteCommandAdornerManager();
                }
                return _instance;
            }
        }

        private RemoteCommandAdornerManager()
        {
            _activePopups = new Dictionary<Control, PopupInfo>();
            LoadTemplateResources();
        }

        private void LoadTemplateResources()
        {
            if (_templateResources == null)
            {
                try
                {
                    _templateResources = AvaloniaXamlLoader.Load(new Uri("avares://gip.core.layoutengine.avui/Controls/RemoteCommand/RemoteCommandPopupTemplate.axaml", UriKind.Absolute)) as ResourceDictionary;
                }
                catch (Exception ex)
                {
                    // Log error if available
                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null)
                        datamodel.Database.Root.Messages.LogException("RemoteCommandAdornerManager", "LoadTemplateResources", ex.Message);
                }
            }
        }

        // Track multiple popups for different elements
        private Dictionary<Control, PopupInfo> _activePopups;

        private class PopupInfo
        {
            public VBContentRemoteControlAdorner Adorner { get; set; }
            public DispatcherTimer Timer { get; set; }
        }

        /// <summary>
        /// Shows a custom popup for the specified duration using a separate adorner layer
        /// </summary>
        /// <param name="Control">The framework element to adorn</param>
        /// <param name="content">The content to display in the popup</param>
        /// <param name="durationSeconds">Duration in seconds to show the popup</param>
        public void ShowCustomTimedPopup(Control Control, Control content, double durationSeconds = 3.0)
        {
            // Hide any existing popup for this element first
            HideCustomPopup(Control);

            try
            {
                // Get the adorner layer
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(Control);
                if (adornerLayer == null)
                    return;

                // Create and add the popup adorner
                VBContentRemoteControlAdorner currentPopupAdorner = new VBContentRemoteControlAdorner(Control, content);
                adornerLayer.Children.Add(currentPopupAdorner);

                // Set up timer to hide popup after specified duration
                DispatcherTimer popupTimer = new DispatcherTimer(DispatcherPriority.Normal);//, Control.Dispatcher)
                popupTimer.Interval = TimeSpan.FromSeconds(durationSeconds);

                // Store popup info
                _activePopups[Control] = new PopupInfo
                {
                    Adorner = currentPopupAdorner,
                    Timer = popupTimer
                };

                popupTimer.Tick += (sender, e) => PopupTimer_Tick(Control);
                popupTimer.Start();
            }
            catch (Exception ex)
            {
                // Log error if available
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null)
                    datamodel.Database.Root.Messages.LogException("RemoteCommandAdornerManager", "ShowCustomTimedPopup", ex.Message);
            }
        }

        /// <summary>
        /// Shows a templated text popup for the specified duration
        /// </summary>
        /// <param name="Control">The framework element to adorn</param>
        /// <param name="text">The text to display</param>
        /// <param name="popupType">The type of popup template to use</param>
        /// <param name="durationSeconds">Duration in seconds to show the popup</param>
        public void ShowCustomTimedPopup(Control Control, string text, RemoteCommandPopupType popupType = RemoteCommandPopupType.Default, double durationSeconds = 3.0)
        {
            try
            {
                // Get the appropriate template
                DataTemplate template = GetPopupTemplate(popupType);
                if (template == null)
                {
                    // Fallback to hardcoded version if template not found
                    ShowCustomTimedPopupFallback(Control, text, durationSeconds);
                    return;
                }

                // Create ContentPresenter with the template
                var contentPresenter = new ContentPresenter
                {
                    Content = text,
                    ContentTemplate = template
                };

                ShowCustomTimedPopup(Control, contentPresenter, durationSeconds);
            }
            catch (Exception ex)
            {
                // Log error and fallback
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null)
                    datamodel.Database.Root.Messages.LogException("RemoteCommandAdornerManager", "ShowCustomTimedPopup", ex.Message);
                
                ShowCustomTimedPopupFallback(Control, text, durationSeconds);
            }
        }

        private DataTemplate GetPopupTemplate(RemoteCommandPopupType popupType)
        {
            if (_templateResources == null)
                return null;

            string templateKey = "RemoteCommandPopupTemplate"; // Default template

            //string templateKey = popupType switch
            //{
            //    RemoteCommandPopupType.Success => "RemoteCommandPopupSuccessTemplate",
            //    RemoteCommandPopupType.Error => "RemoteCommandPopupErrorTemplate",
            //    RemoteCommandPopupType.Warning => "RemoteCommandPopupWarningTemplate",
            //    _ => "RemoteCommandPopupTemplate"
            //};

            return _templateResources[templateKey] as DataTemplate;
        }

        /// <summary>
        /// Fallback method using hardcoded styling (original implementation)
        /// </summary>
        private void ShowCustomTimedPopupFallback(Control Control, string text, double durationSeconds = 3.0)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                Padding = new Thickness(8),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 300
            };

            var border = new Border
            {
                Background = Brushes.Black,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Child = textBlock,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    //Direction = 315,
                    BlurRadius = 3,
                    Opacity = 0.5
                }
            };

            ShowCustomTimedPopup(Control, border, durationSeconds);
        }

        /// <summary>
        /// Hides the current popup if visible for the specified element
        /// </summary>
        /// <param name="Control">The framework element whose popup should be hidden</param>
        public void HideCustomPopup(Control Control)
        {
            try
            {
                if (_activePopups.TryGetValue(Control, out PopupInfo popupInfo))
                {
                    // Stop and dispose timer
                    if (popupInfo.Timer != null)
                    {
                        popupInfo.Timer.Stop();
                        popupInfo.Timer = null;
                    }

                    // Remove adorner
                    if (popupInfo.Adorner != null)
                    {
                        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(Control);
                        if (adornerLayer != null)
                        {
                            adornerLayer.Children.Remove(popupInfo.Adorner);
                        }
                        popupInfo.Adorner = null;
                    }

                    // Remove from tracking dictionary
                    _activePopups.Remove(Control);
                }
            }
            catch (Exception ex)
            {
                // Log error if available
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null)
                    datamodel.Database.Root.Messages.LogException("RemoteCommandAdornerManager", "HideCustomPopup", ex.Message);
            }
        }

        /// <summary>
        /// Hides all active popups
        /// </summary>
        public void HideAllPopups()
        {
            var elementsToHide = _activePopups.Keys.ToList();
            foreach (var element in elementsToHide)
            {
                HideCustomPopup(element);
            }
        }

        private void PopupTimer_Tick(Control Control)
        {
            HideCustomPopup(Control);
        }

        public void VisualizeIfRemoteControlled(string acUrl, Control uiElement, IACComponent acComponent, bool isCommand, RemoteCommandPopupType commandPopupType = RemoteCommandPopupType.Default)
        {
            if (string.IsNullOrEmpty(acUrl) || uiElement == null)
                return;
            if (!RemoteCommandManager.Instance.HasRemoteCommand(acComponent, acUrl, isCommand))
                return;
            ShowCustomTimedPopup(uiElement, "", RemoteCommandPopupType.Default, 6.0);
        }

        public void VisualizeIfRemoteControlled(Control uiElement, IACComponent acComponent, bool isCommand, RemoteCommandPopupType commandPopupType = RemoteCommandPopupType.Default)
        {
            IVBContent vbContent = uiElement as IVBContent;
            if (vbContent == null)
                return;
            VisualizeIfRemoteControlled(vbContent.VBContent, uiElement, acComponent, isCommand, commandPopupType);
        }
    }
}
