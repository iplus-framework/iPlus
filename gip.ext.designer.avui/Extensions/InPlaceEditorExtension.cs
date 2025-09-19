// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Diagnostics;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;

namespace gip.ext.designer.avui.Extensions
{
    /// <summary>
    /// Extends In-Place editor to edit any text in the designer which is wrapped in the Visual tree under TexBlock
    /// </summary>
    [ExtensionFor(typeof(TextBlock))]
    public class InPlaceEditorExtension : PrimarySelectionAdornerProvider
    {
        AdornerPanel adornerPanel;
        RelativePlacement placement;
        InPlaceEditor editor;
        /// <summary> Is the element in the Visual tree of the extended element which is being edited. </summary>
        TextBlock textBlock;
        Control element;
        DesignPanel designPanel;

        bool isGettingDragged;   // Flag to get/set whether the extended element is dragged.
        bool isMouseDown;        // Flag to get/set whether left-button is down on the element.
        int numClicks;           // No of left-button clicks on the element.

        public InPlaceEditorExtension()
        {
            adornerPanel = new AdornerPanel();
            isGettingDragged = false;
            //isMouseDown = Mouse.LeftButton == MouseButtonState.Pressed ? true : false;
            numClicks = 0;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            element = ExtendedItem.Component as Control;
            editor = new InPlaceEditor(ExtendedItem);
            editor.DataContext = element;
            editor.IsVisible = false; // Hide the editor first, It's visibility is governed by mouse events.

            placement = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
            adornerPanel.Children.Add(editor);
            Adorners.Add(adornerPanel);

            designPanel = ExtendedItem.Services.GetService<IDesignPanel>() as DesignPanel;
            Debug.Assert(designPanel != null);

            /* Add mouse event handlers */
            designPanel.PointerPressed += DesignPanel_PointerPressed;
            designPanel.PointerReleased += DesignPanel_PointerReleased;
            designPanel.PointerMoved += DesignPanel_PointerMoved;

            /* To update the position of Editor in case of resize operation */
            ExtendedItem.PropertyChanged += PropertyChanged;

            eventsAdded = true;
        }

        /// <summary>
        /// Checks whether heigth/width have changed and updates the position of editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (textBlock != null && _PosOfElement != null && _PosOfTextBlock != null)
            {
                if (e.PropertyName == "Width")
                {
                    placement.XOffset = _PosOfElement.Value.X - _PosOfTextBlock.Value.X - 2.8;
                    editor.MaxWidth = Math.Max((ModelTools.GetWidth(element) - placement.XOffset), 0);
                }
                if (e.PropertyName == "Height")
                {
                    placement.YOffset = _PosOfElement.Value.Y - _PosOfTextBlock.Value.Y - 1;
                    editor.MaxHeight = Math.Max((ModelTools.GetHeight(element) - placement.YOffset), 0);
                }
                AdornerPanel.SetPlacement(editor, placement);
            }
        }

        /// <summary>
        /// Places the handle from a calculated offset using Mouse Positon
        /// </summary>
        /// <param name="text"></param>
        /// <param name="e"></param>
        void PlaceEditor(Visual text, PointerEventArgs e)
        {
            textBlock = text as TextBlock;
            Debug.Assert(textBlock != null);

            /* Gets the offset between the top-left corners of the element and the editor*/
            placement.XOffset = e.GetPosition(element).X - e.GetPosition(textBlock).X - 2.8;
            placement.YOffset = e.GetPosition(element).Y - e.GetPosition(textBlock).Y - 1;
            placement.XRelativeToAdornerWidth = 0;
            placement.XRelativeToContentWidth = 0;
            placement.YRelativeToAdornerHeight = 0;
            placement.YRelativeToContentHeight = 0;
            editor.SetBinding(textBlock);

            /* Change data context of the editor to the TextBlock */
            editor.DataContext = textBlock;

            /* Set MaxHeight and MaxWidth so that editor doesn't cross the boundaries of the control */
            var height = ModelTools.GetHeight(element);
            var width = ModelTools.GetWidth(element);
            editor.MaxHeight = Math.Max((height - placement.YOffset), 0);
            editor.MaxWidth = Math.Max((width - placement.XOffset), 0);

            /* Hides the TextBlock in control because of some minor offset in placement, overlaping makes text look fuzzy */
            textBlock.IsVisible = false; // 
            AdornerPanel.SetPlacement(editor, placement);

            RemoveBorder(); // Remove the highlight border.
        }

        /// <summary>
        /// Aborts the editing. This aborts the underlying change group of the editor
        /// </summary>
        public void AbortEdit()
        {
            editor.AbortEditing();
        }

        /// <summary>
        /// Starts editing once again. This aborts the underlying change group of the editor
        /// </summary>
        public void StartEdit()
        {
            editor.StartEditing();
        }

        #region MouseEvents
        DesignPanelHitTestResult result;
        Point Current;
        Point Start;
        Point? _PosOfElement;
        Point? _PosOfTextBlock;

        void DesignPanel_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            StoreLastPointerPos(e);

            // Only preview events
            if (!e.Properties.IsLeftButtonPressed || e.Route != Avalonia.Interactivity.RoutingStrategies.Tunnel)
                return;
            result = designPanel.HitTest(e.GetPosition(designPanel), false, true);
            if (result.ModelHit == ExtendedItem && result.VisualHit is TextBlock)
            {
                Start = e.GetPosition(null);
                Current = Start;
                isMouseDown = true;
            }
            numClicks++;
        }

        void DesignPanel_PointerMoved(object sender, PointerEventArgs e)
        {
            StoreLastPointerPos(e);

            Current += e.GetPosition(null) - Start;
            result = designPanel.HitTest(e.GetPosition(designPanel), false, true);
            if (result.ModelHit == ExtendedItem && result.VisualHit is TextBlock)
            {
                if (numClicks > 0)
                {
                    if (isMouseDown &&
                        ((Current - Start).X > DragListener.MinimumDragDistance
                         || (Current - Start).Y > DragListener.MinimumDragDistance))
                    {
                        isGettingDragged = true;
                        editor.Focus();
                    }
                }
                DrawBorder((Control)result.VisualHit);
            }
            else
            {
                RemoveBorder();
            }
        }

        void DesignPanel_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            result = designPanel.HitTest(e.GetPosition(designPanel), true, true, HitTestType.Default);
            if (((result.ModelHit == ExtendedItem && result.VisualHit is TextBlock) || (result.VisualHit != null && result.VisualHit.TryFindParent<InPlaceEditor>() == editor)) && numClicks > 0)
            {
                if (!isGettingDragged)
                {
                    PlaceEditor(ExtendedItem.View, e);
                    foreach (var extension in ExtendedItem.Extensions)
                    {
                        if (!(extension is InPlaceEditorExtension) && !(extension is SelectedElementRectangleExtension))
                        {
                            ExtendedItem.RemoveExtension(extension);
                        }
                    }
                    editor.IsVisible = true;
                }
            }
            else
            { // Clicked outside the Text - > hide the editor and make the actual text visible again
                RemoveEventsAndShowControl();
                this.ExtendedItem.ReapplyAllExtensions();
            }

            isMouseDown = false;
            isGettingDragged = false;
        }

        private void StoreLastPointerPos(PointerEventArgs e)
        {
            if (element != null)
                _PosOfElement = e.GetPosition(element);
            if (textBlock != null)
                _PosOfTextBlock = e.GetPosition(textBlock);
        }

        #endregion

        #region HighlightBorder
        private Border _border;
        private sealed class BorderPlacement : AdornerPlacement
        {
            private readonly Control _element;

            public BorderPlacement(Control element)
            {
                _element = element;
            }

            public override void Arrange(AdornerPanel panel, Control adorner, Size adornedElementSize)
            {
                Point? p = _element.TranslatePoint(new Point(), panel.AdornedElement as Visual);
                var rect = new Rect(p.Value, _element.Bounds.Size);
                rect.Inflate(new Thickness(3, 1, 0, 0));
                adorner.Arrange(rect);
            }
        }

        private void DrawBorder(Control item)
        {
            if (editor != null && !editor.IsVisible)
            {
                if (adornerPanel.Children.Contains(_border))
                    adornerPanel.Children.Remove(_border);
                _border = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1.4) }; //, SnapsToDevicePixels = true };
                ToolTip.SetTip(_border, "Edit this Text");
                var shadow = new DropShadowEffect { Color = Colors.LightGray };
                _border.Effect = shadow;
                var bp = new BorderPlacement(item);
                AdornerPanel.SetPlacement(_border, bp);
                adornerPanel.Children.Add(_border);
            }
        }

        private void RemoveBorder()
        {
            if (adornerPanel.Children.Contains(_border))
                adornerPanel.Children.Remove(_border);
        }
        #endregion

        protected override void OnRemove()
        {
            if (textBlock != null)
                textBlock.IsVisible = true;
            ExtendedItem.PropertyChanged -= PropertyChanged;
            designPanel.PointerPressed -= DesignPanel_PointerPressed;
            designPanel.PointerMoved -= DesignPanel_PointerMoved;
            designPanel.PointerReleased -= DesignPanel_PointerReleased;
            base.OnRemove();
        }

        private bool eventsAdded;

        private void RemoveEventsAndShowControl()
        {
            editor.IsVisible = false;

            if (textBlock != null)
            {
                textBlock.IsVisible = true;
            }

            if (eventsAdded)
            {
                eventsAdded = false;
                ExtendedItem.PropertyChanged -= PropertyChanged;
                designPanel.PointerPressed -= DesignPanel_PointerPressed;
                designPanel.PointerMoved -= DesignPanel_PointerMoved;
                designPanel.PointerReleased -= DesignPanel_PointerReleased;
            }
        }
    }
}
