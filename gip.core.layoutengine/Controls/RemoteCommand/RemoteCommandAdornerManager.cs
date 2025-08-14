// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using gip.core.datamodel;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace gip.core.layoutengine
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
            _activePopups = new Dictionary<FrameworkElement, PopupInfo>();
            LoadTemplateResources();
        }

        private void LoadTemplateResources()
        {
            if (_templateResources == null)
            {
                try
                {
                    _templateResources = new ResourceDictionary
                    {
                        Source = new Uri("/gip.core.layoutengine;Component/Controls/RemoteCommand/RemoteCommandPopupTemplate.xaml", UriKind.Relative)
                    };
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
        private Dictionary<FrameworkElement, PopupInfo> _activePopups;

        private class PopupInfo
        {
            public VBContentRemoteControlAdorner Adorner { get; set; }
            public DispatcherTimer Timer { get; set; }
        }

        /// <summary>
        /// Shows a custom popup for the specified duration using a separate adorner layer
        /// </summary>
        /// <param name="frameworkElement">The framework element to adorn</param>
        /// <param name="content">The content to display in the popup</param>
        /// <param name="durationSeconds">Duration in seconds to show the popup</param>
        public void ShowCustomTimedPopup(FrameworkElement frameworkElement, UIElement content, double durationSeconds = 3.0)
        {
            // Hide any existing popup for this element first
            HideCustomPopup(frameworkElement);

            try
            {
                // Get the adorner layer
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(frameworkElement);
                if (adornerLayer == null)
                    return;

                // Create and add the popup adorner
                VBContentRemoteControlAdorner currentPopupAdorner = new VBContentRemoteControlAdorner(frameworkElement, content);
                adornerLayer.Add(currentPopupAdorner);

                // Set up timer to hide popup after specified duration
                DispatcherTimer popupTimer = new DispatcherTimer(DispatcherPriority.Normal, frameworkElement.Dispatcher)
                {
                    Interval = TimeSpan.FromSeconds(durationSeconds)
                };

                // Store popup info
                _activePopups[frameworkElement] = new PopupInfo
                {
                    Adorner = currentPopupAdorner,
                    Timer = popupTimer
                };

                popupTimer.Tick += (sender, e) => PopupTimer_Tick(frameworkElement);
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
        /// <param name="frameworkElement">The framework element to adorn</param>
        /// <param name="text">The text to display</param>
        /// <param name="popupType">The type of popup template to use</param>
        /// <param name="durationSeconds">Duration in seconds to show the popup</param>
        public void ShowCustomTimedPopup(FrameworkElement frameworkElement, string text, RemoteCommandPopupType popupType = RemoteCommandPopupType.Default, double durationSeconds = 3.0)
        {
            try
            {
                // Get the appropriate template
                DataTemplate template = GetPopupTemplate(popupType);
                if (template == null)
                {
                    // Fallback to hardcoded version if template not found
                    ShowCustomTimedPopupFallback(frameworkElement, text, durationSeconds);
                    return;
                }

                // Create ContentPresenter with the template
                var contentPresenter = new ContentPresenter
                {
                    Content = text,
                    ContentTemplate = template
                };

                ShowCustomTimedPopup(frameworkElement, contentPresenter, durationSeconds);
            }
            catch (Exception ex)
            {
                // Log error and fallback
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null)
                    datamodel.Database.Root.Messages.LogException("RemoteCommandAdornerManager", "ShowCustomTimedPopup", ex.Message);
                
                ShowCustomTimedPopupFallback(frameworkElement, text, durationSeconds);
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
        private void ShowCustomTimedPopupFallback(FrameworkElement frameworkElement, string text, double durationSeconds = 3.0)
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
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 315,
                    ShadowDepth = 3,
                    Opacity = 0.5
                }
            };

            ShowCustomTimedPopup(frameworkElement, border, durationSeconds);
        }

        /// <summary>
        /// Hides the current popup if visible for the specified element
        /// </summary>
        /// <param name="frameworkElement">The framework element whose popup should be hidden</param>
        public void HideCustomPopup(FrameworkElement frameworkElement)
        {
            try
            {
                if (_activePopups.TryGetValue(frameworkElement, out PopupInfo popupInfo))
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
                        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(frameworkElement);
                        if (adornerLayer != null)
                        {
                            adornerLayer.Remove(popupInfo.Adorner);
                        }
                        popupInfo.Adorner = null;
                    }

                    // Remove from tracking dictionary
                    _activePopups.Remove(frameworkElement);
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

        private void PopupTimer_Tick(FrameworkElement frameworkElement)
        {
            HideCustomPopup(frameworkElement);
        }

        public void VisualizeIfRemoteControlled(string acUrl, FrameworkElement uiElement, IACComponent acComponent, bool isCommand, RemoteCommandPopupType commandPopupType = RemoteCommandPopupType.Default)
        {
            if (string.IsNullOrEmpty(acUrl) || uiElement == null)
                return;
            if (!RemoteCommandManager.Instance.HasRemoteCommand(acComponent, acUrl, isCommand))
                return;
            ShowCustomTimedPopup(uiElement, "", RemoteCommandPopupType.Default, 6.0);
        }

        public void VisualizeIfRemoteControlled(FrameworkElement uiElement, IACComponent acComponent, bool isCommand, RemoteCommandPopupType commandPopupType = RemoteCommandPopupType.Default)
        {
            IVBContent vbContent = uiElement as IVBContent;
            if (vbContent == null)
                return;
            VisualizeIfRemoteControlled(vbContent.VBContent, uiElement, acComponent, isCommand, commandPopupType);
        }
    }
}
