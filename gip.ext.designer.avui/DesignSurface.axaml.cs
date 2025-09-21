// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using gip.ext.designer.avui.Xaml;
using gip.ext.designer.avui.Services;
using System.Diagnostics;
using gip.ext.xamldom.avui;
using System.Threading;
using System.Globalization;
using gip.ext.design.avui;
using gip.core.datamodel;
using gip.ext.designer.avui.Controls;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia;
using Avalonia.Media;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Controls.Primitives;

namespace gip.ext.designer.avui
{
    /// <summary>
    /// Surface hosting the AvaloniaUI designer.
    /// </summary>
    public partial class DesignSurface : UserControl
    {
        private FocusNavigator _focusNav;
        static DesignSurface()
        {
            //TODO: this is for converters (see PropertyGrid)
            //Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        public DesignSurface()
        {
            InitializeComponent();

            // Event handlers for CheckBox
            _GroupedOnly.IsCheckedChanged += GroupedOnly_Checked;

            Binding binding = new Binding();
            binding.Source = this;
            binding.Path = "RasterSize";
            binding.Mode = BindingMode.OneWayToSource;
            _UpDownRasterSize.Bind(gip.ext.designer.avui.Controls.NumericUpDown.ValueProperty, binding);

            _RotationChangedSelection = true;
            binding = new Binding();
            binding.Source = this;
            binding.Path = "RotationAngle";
            binding.Mode = BindingMode.OneWayToSource;
            _UpDownRotation.Bind(gip.ext.designer.avui.Controls.NumericUpDown.ValueProperty, binding);
            _RotationChangedSelection = false;

            binding = new Binding();
            binding.Source = this;
            binding.Path = "HitTestLayerNum";
            binding.Mode = BindingMode.TwoWay;
            _LayerHitTest.Bind(gip.ext.designer.avui.Controls.NumericUpDown.ValueProperty, binding);

            // Register command handlers
            this.AddCommandHandler(ApplicationCommands.Undo, Undo, CanUndo);
            this.AddCommandHandler(ApplicationCommands.Redo, Redo, CanRedo);
            this.AddCommandHandler(ApplicationCommands.Copy, Copy, CanCopyOrCut);
            this.AddCommandHandler(ApplicationCommands.Cut, Cut, CanCopyOrCut);
            this.AddCommandHandler(ApplicationCommands.Delete, Delete, CanDelete);
            this.AddCommandHandler(ApplicationCommands.Paste, Paste, CanPaste);
            this.AddCommandHandler(ApplicationCommands.SelectAll, SelectAll, CanSelectAll);

            this.AddCommandHandler(Commands.AlignTopCommand, () => ModelTools.ArrangeItems(this.DesignContext.Services.Selection.SelectedItems, ArrangeDirection.Top), () => this.DesignContext.Services.Selection.SelectedItems.Count() > 1);
            this.AddCommandHandler(Commands.AlignMiddleCommand, () => ModelTools.ArrangeItems(this.DesignContext.Services.Selection.SelectedItems, ArrangeDirection.VerticalMiddle), () => this.DesignContext.Services.Selection.SelectedItems.Count() > 1);
            this.AddCommandHandler(Commands.AlignBottomCommand, () => ModelTools.ArrangeItems(this.DesignContext.Services.Selection.SelectedItems, ArrangeDirection.Bottom), () => this.DesignContext.Services.Selection.SelectedItems.Count() > 1);
            this.AddCommandHandler(Commands.AlignLeftCommand, () => ModelTools.ArrangeItems(this.DesignContext.Services.Selection.SelectedItems, ArrangeDirection.Left), () => this.DesignContext.Services.Selection.SelectedItems.Count() > 1);
            this.AddCommandHandler(Commands.AlignCenterCommand, () => ModelTools.ArrangeItems(this.DesignContext.Services.Selection.SelectedItems, ArrangeDirection.HorizontalMiddle), () => this.DesignContext.Services.Selection.SelectedItems.Count() > 1);
            this.AddCommandHandler(Commands.AlignRightCommand, () => ModelTools.ArrangeItems(this.DesignContext.Services.Selection.SelectedItems, ArrangeDirection.Right), () => this.DesignContext.Services.Selection.SelectedItems.Count() > 1);

            this.AddCommandHandler(Commands.RotateLeftCommand, () => ModelTools.ApplyTransform(this.DesignContext.Services.Selection.PrimarySelection, new RotateTransform(-90), true, this.DesignContext.RootItem == this.DesignContext.Services.Selection.PrimarySelection ? Control.RenderTransformProperty : Control.RenderTransformProperty), () => this.DesignContext.Services.Selection.PrimarySelection != null);
            this.AddCommandHandler(Commands.RotateRightCommand, () => ModelTools.ApplyTransform(this.DesignContext.Services.Selection.PrimarySelection, new RotateTransform(90), true, this.DesignContext.RootItem == this.DesignContext.Services.Selection.PrimarySelection ? Control.RenderTransformProperty : Control.RenderTransformProperty), () => this.DesignContext.Services.Selection.PrimarySelection != null);

            this.AddCommandHandler(Commands.StretchToSameWidthCommand, () => ModelTools.StretchItems(this.DesignContext.Services.Selection.SelectedItems, StretchDirection.Width), () => this.DesignContext.Services.Selection.SelectedItems.Count() > 1);
            this.AddCommandHandler(Commands.StretchToSameHeightCommand, () => ModelTools.StretchItems(this.DesignContext.Services.Selection.SelectedItems, StretchDirection.Height), () => this.DesignContext.Services.Selection.SelectedItems.Count() > 1);

#if DEBUG || CODE_ANALYSIS
            if (_designPanel == null)
            {
                Environment.FailFast("designpanel should be initialized earlier");
                // Fake call to DesignPanel constructor because FxCop doesn't look inside XAML files
                // and we'd get tons of warnings about uncalled private code.
                _designPanel = new DesignPanel();
            }
#endif
            if (_designPanel != null)
            {
                _designPanel.DesignSurface = this;
                _designPanel.Child = _sceneContainer;
                // Note: RequestBringIntoView event doesn't exist in AvaloniaUI
                // _designPanel.RequestBringIntoView += _partDesignContent_RequestBringIntoView;
            }
        }

        private bool enableBringIntoView = false;

        public void ScrollIntoView(DesignItem designItem)
        {
            enableBringIntoView = true;
            designItem.View?.BringIntoView();
            enableBringIntoView = false;
        }

        void _partDesignContent_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            if (!enableBringIntoView)
                e.Handled = true;
            enableBringIntoView = false;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.Source == uxZoom)
            {
                UnselectAll();
            }
            base.OnPointerPressed(e);
        }

        public ZoomControl ZoomControl { get { return uxZoom; } }

        DesignContext _designContext;

        /// <summary>
        /// Gets the active design context.
        /// </summary>
        public DesignContext DesignContext
        {
            get { return _designContext; }
        }

        /// <summary>
        /// Gets the DesignPanel
        /// </summary>
        public DesignPanel DesignPanel
        {
            get { return _designPanel; }
        }

        /// <summary>
        /// Initializes the designer content from the specified XmlReader.
        /// </summary>
        public void LoadDesigner(XmlReader xamlReader, XamlLoadSettings loadSettings)
        {
            UnloadDesigner();
            loadSettings = loadSettings ?? new XamlLoadSettings();
            loadSettings.CustomServiceRegisterFunctions.Add(
                context => context.Services.AddService(typeof(IDesignPanel), _designPanel));
            //loadSettings.DesignerAssemblies.Add(typeof(ArrowLine).Assembly);
            //loadSettings.TypeFinder.RegisterAssembly(typeof(ArrowLine).Assembly);
            InitializeDesigner(new XamlDesignContext(xamlReader, loadSettings));
        }

        /// <summary>
        /// Saves the designer content into the specified XmlWriter.
        /// </summary>
        public void SaveDesigner(XmlWriter writer)
        {
            _designContext.Save(writer);
        }

        bool _PreviewHandlerAdded = false;
        void InitializeDesigner(DesignContext context)
        {
            _designContext = context;
            uxZoom.DesignContext = _designContext;
            _designPanel.Context = context;
            _designPanel.ClearContextMenu();

            this.AddHandler(InputElement.PointerMovedEvent, _sceneContainer_PreviewMouseMove, RoutingStrategies.Tunnel);
            this.AddHandler(InputElement.PointerPressedEvent, _sceneContainer_PreviewMouseDown, RoutingStrategies.Tunnel);
            this.AddHandler(InputElement.PointerReleasedEvent, _sceneContainer_PreviewMouseUp, RoutingStrategies.Tunnel);
            _PreviewHandlerAdded = true;
            if (context.RootItem != null)
            {
                _sceneContainer.Child = context.RootItem.View;
            }

            context.Services.RunWhenAvailable<UndoService>(
                undoService => undoService.UndoStackChanged += delegate
                {
                    // Invalidate command bindings (equivalent to CommandManager.InvalidateRequerySuggested())
                }
            );
            context.Services.Selection.SelectionChanged += delegate
            {
                // Invalidate command bindings
            };

            context.Services.Selection.SelectionChanged += new EventHandler<DesignItemCollectionEventArgs>(Selection_SelectionChanged);

            context.Services.AddService(typeof(IKeyBindingService), new DesignerKeyBindings(this));
            _focusNav = new FocusNavigator(this);
            _focusNav.Start();
        }

        /// <summary>
        /// Unloads the designer content.
        /// </summary>
        public void UnloadDesigner(bool clearServices = false)
        {
            if (_designContext != null)
            {
                foreach (object o in _designContext.Services.AllServices)
                {
                    IDisposable d = o as IDisposable;
                    if (d != null) d.Dispose();
                }
            }
            
            uxZoom.DesignContext = null;
            _designContext = null;
            _designPanel.Context = null;
            if (_PreviewHandlerAdded)
            {
                this.RemoveHandler(InputElement.PointerMovedEvent, _sceneContainer_PreviewMouseMove);
                this.RemoveHandler(InputElement.PointerPressedEvent, _sceneContainer_PreviewMouseDown);
                this.RemoveHandler(InputElement.PointerReleasedEvent, _sceneContainer_PreviewMouseUp);
            }
            _sceneContainer.Child = null;
            _designPanel.Adorners.Clear();
        }

        void _sceneContainer_PreviewMouseMove(object sender, PointerEventArgs e)
        {
            if (_sceneContainer.Child != null)
            {
                Point coord = e.GetPosition(_sceneContainer.Child);
                _XCoord.Text = System.Convert.ToInt32(coord.X).ToString();
                _YCoord.Text = System.Convert.ToInt32(coord.Y).ToString();
                _XCoord2.Text = System.Convert.ToInt32((coord.X - _PointStart.X)).ToString();
                _YCoord2.Text = System.Convert.ToInt32((coord.Y - _PointStart.Y)).ToString();
            }
        }

        private Point _PointStart = new Point();
        void _sceneContainer_PreviewMouseDown(object sender, PointerPressedEventArgs e)
        {
            if (_sceneContainer.Child != null)
            {
                _PointStart = e.GetPosition(_sceneContainer.Child);
                _XCoord1.Text = System.Convert.ToInt32(_PointStart.X).ToString();
                _YCoord1.Text = System.Convert.ToInt32(_PointStart.Y).ToString();
            }
        }

        void _sceneContainer_PreviewMouseUp(object sender, PointerReleasedEventArgs e)
        {
            if (_sceneContainer.Child != null)
            {
                _PointStart = new Point();
                _XCoord1.Text = System.Convert.ToInt32(_PointStart.X).ToString();
                _YCoord1.Text = System.Convert.ToInt32(_PointStart.Y).ToString();
            }
        }

        public Double RasterSize
        {
            get
            {
                return _designPanel.RasterSize;
            }
            set
            {
                _designPanel.RasterSize = value;
                RefreshRaster();
            }
        }

        public bool IsRasterOn
        {
            get
            {
                return _designPanel.IsRasterOn;
            }
        }

        protected void RefreshRaster()
        {
            if (_sceneContainer.Child == null)
                return;
            if ((_designPanel.AdornerRaster == null) && RasterSize < 5)
                return;

            var adornerLayer = _designPanel.AdornerLayer;
            if ((_designPanel.AdornerRaster != null) && RasterSize < 5)
            {
                // Remove the adorner panel that contains the raster adorner
                var adornerToRemove = adornerLayer.Adorners.FirstOrDefault(a => 
                    a.Children.Count > 0 && a.Children[0] == _designPanel.AdornerRaster);
                if (adornerToRemove != null)
                {
                    adornerLayer.Adorners.Remove(adornerToRemove);
                }
                _designPanel.AdornerRaster = null;
                return;
            }
            if (_designPanel.AdornerRaster != null)
            {
                _designPanel.AdornerRaster.RasterSize = System.Convert.ToInt32(RasterSize);
                return;
            }

            // Create new raster adorner
            _designPanel.AdornerRaster = new DesignSurfaceRasterAdorner(_sceneContainer, System.Convert.ToInt32(RasterSize));
            
            // Create adorner panel and add the raster adorner to it
            var adornerPanel = new gip.ext.design.avui.Adorners.AdornerPanel();
            adornerPanel.SetAdornedElement(_sceneContainer, null);
            adornerPanel.Children.Add(_designPanel.AdornerRaster);
            
            adornerLayer.Adorners.Add(adornerPanel);
        }

        private bool _RotationChangedSelection = false;
        private Double _RotationAngle = 0;
        public Double RotationAngle
        {
            get
            {
                return _RotationAngle;
            }
            set
            {
                _RotationAngle = value;
                if (!_RotationChangedSelection && (_designContext != null))
                {
                    DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                    if ((designItem != null) && (designItem.View != null) && (designItem.View is Control))
                    {
                        DesignItemProperty prop = designItem.Properties.GetProperty(Control.RenderTransformProperty);
                        if (prop != null)
                        {
                            if (prop.Value == null)
                            {
                                NewTransformGroupWithRotate(prop, value);
                            }
                            else if (typeof(RotateTransform).IsAssignableFrom(prop.Value.ComponentType))
                            {
                                DesignItemProperty propAngle = prop.Value.Properties.GetProperty(RotateTransform.AngleProperty);
                                if (propAngle != null)
                                {
                                    propAngle.SetValue(value);
                                }
                            }
                            else if (typeof(TransformGroup).IsAssignableFrom(prop.Value.ComponentType))
                            {
                                UpdateTransformGroupWithRotate(prop, value);
                            }
                            // Sonst Skew oder ScaleTransform => Umverpackung in TransformGroup
                            else if (typeof(Transform).IsAssignableFrom(prop.Value.ComponentType))
                            {
                                NewTransformGroupWithRotate(prop, value);
                            }
                        }
                    }
                }
            }
        }

        private void NewTransformGroupWithRotate(DesignItemProperty prop, Double value)
        {
            TransformGroup transformFroup = new TransformGroup();
            prop.SetValue(transformFroup);
            UpdateTransformGroupWithRotate(prop, value);
        }

        private void UpdateTransformGroupWithRotate(DesignItemProperty prop, Double value)
        {
            if (prop.Value.ContentProperty.IsCollection)
            {
                DesignItem transformObject = null;
                foreach (DesignItem child in prop.Value.ContentProperty.CollectionElements)
                {
                    if (typeof(RotateTransform).IsAssignableFrom(child.ComponentType))
                    {
                        transformObject = child;
                        break;
                    }
                }

                if (transformObject == null)
                {
                    RotateTransform rotTransform = new RotateTransform();
                    transformObject = GetService<IComponentService>().RegisterComponentForDesigner(rotTransform);
                    prop.Value.ContentProperty.CollectionElements.Add(transformObject);
                }
                DesignItemProperty propAngle = transformObject.Properties.GetProperty(RotateTransform.AngleProperty);
                if (propAngle != null)
                    propAngle.SetValue(value);
            }
        }

        private void NewTransformGroupWithScale(DesignItemProperty prop, bool swapHorz)
        {
            TransformGroup transformFroup = new TransformGroup();
            prop.SetValue(transformFroup);
            UpdateTransformGroupWithScale(prop, swapHorz);
        }

        private void UpdateTransformGroupWithScale(DesignItemProperty prop, bool swapHorz)
        {
            if (prop.Value.ContentProperty.IsCollection)
            {
                DesignItem transformObject = null;
                foreach (DesignItem child in prop.Value.ContentProperty.CollectionElements)
                {
                    if (typeof(ScaleTransform).IsAssignableFrom(child.ComponentType))
                    {
                        transformObject = child;
                        break;
                    }
                }

                if (transformObject == null)
                {
                    ScaleTransform rotTransform = new ScaleTransform();
                    transformObject = GetService<IComponentService>().RegisterComponentForDesigner(rotTransform);
                    prop.Value.ContentProperty.CollectionElements.Add(transformObject);
                }
                if (swapHorz)
                {
                    DesignItemProperty propAngle = transformObject.Properties.GetProperty(ScaleTransform.ScaleXProperty);
                    if (propAngle != null)
                    {
                        if (Convert.ToInt32(propAngle.ValueOnInstance) == 1)
                            propAngle.SetValue((Double)(-1.0));
                        else
                            propAngle.SetValue((Double)(1));
                    }
                }
                else
                {
                    DesignItemProperty propAngle = transformObject.Properties.GetProperty(ScaleTransform.ScaleYProperty);
                    if (propAngle != null)
                    {
                        if (Convert.ToInt32(propAngle.ValueOnInstance) == 1)
                            propAngle.SetValue((Double)(-1.0));
                        else
                            propAngle.SetValue((Double)(1));
                    }
                }
            }
        }

        public void DesignerRotateR90()
        {
            if (RotationAngle <= 0.001)
            {
                _UpDownRotation.Value = 90;
            }
            else if (Convert.ToInt32(RotationAngle) % 90 == 0)
            {
                if (Convert.ToInt32(RotationAngle) >= 270)
                    _UpDownRotation.Value = 0;
                else
                    _UpDownRotation.Value += 90;
            }
            else
                _UpDownRotation.Value = 90;
        }

        public bool IsEnabledDesignerRotateR90()
        {
            if (_designContext != null)
            {
                DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                if ((designItem != null) && (designItem.View != null) && (designItem.View is Control))
                    return true;
            }
            return false;
        }

        public void DesignerFlipHorz()
        {
            if (_designContext != null)
            {
                DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                if ((designItem != null) && (designItem.View != null) && (designItem.View is Control))
                {
                    DesignItemProperty prop = designItem.Properties.GetProperty(Control.RenderTransformProperty);
                    if (prop != null)
                    {
                        if (prop.Value == null)
                        {
                            NewTransformGroupWithScale(prop, true);
                        }
                        else if (typeof(ScaleTransform).IsAssignableFrom(prop.Value.ComponentType))
                        {
                            DesignItemProperty propAngle = prop.Value.Properties.GetProperty(ScaleTransform.ScaleXProperty);
                            if (propAngle != null)
                            {
                                if (Convert.ToInt32(propAngle.ValueOnInstance) == 1)
                                    propAngle.SetValue((Double)(-1.0));
                                else
                                    propAngle.SetValue((Double)(1));
                            }
                        }
                        else if (typeof(TransformGroup).IsAssignableFrom(prop.Value.ComponentType))
                        {
                            UpdateTransformGroupWithScale(prop, true);
                        }
                        // Sonst Skew oder ScaleTransform => Umverpackung in TransformGroup
                        else if (typeof(Transform).IsAssignableFrom(prop.Value.ComponentType))
                        {
                            NewTransformGroupWithScale(prop, true);
                        }
                    }
                }
            }
        }

        public bool IsEnabledDesignerFlipHorz()
        {
            if (_designContext != null)
            {
                DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                if ((designItem != null) && (designItem.View != null) && (designItem.View is Control))
                    return true;
            }
            return false;
        }

        public void DesignerFlipVert()
        {
            if (_designContext != null)
            {
                DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                if ((designItem != null) && (designItem.View != null) && (designItem.View is Control))
                {
                    DesignItemProperty prop = designItem.Properties.GetProperty(Control.RenderTransformProperty);
                    if (prop != null)
                    {
                        if (prop.Value == null)
                        {
                            NewTransformGroupWithScale(prop, false);
                        }
                        else if (typeof(ScaleTransform).IsAssignableFrom(prop.Value.ComponentType))
                        {
                            DesignItemProperty propAngle = prop.Value.Properties.GetProperty(ScaleTransform.ScaleYProperty);
                            if (propAngle != null)
                            {
                                if (Convert.ToInt32(propAngle.ValueOnInstance) == 1)
                                    propAngle.SetValue((Double)(-1.0));
                                else
                                    propAngle.SetValue((Double)(1));
                            }
                        }
                        else if (typeof(TransformGroup).IsAssignableFrom(prop.Value.ComponentType))
                        {
                            UpdateTransformGroupWithScale(prop, false);
                        }
                        // Sonst Skew oder ScaleTransform => Umverpackung in TransformGroup
                        else if (typeof(Transform).IsAssignableFrom(prop.Value.ComponentType))
                        {
                            NewTransformGroupWithScale(prop, false);
                        }
                    }
                }
            }
        }

        public bool IsEnabledDesignerFlipVert()
        {
            if (_designContext != null)
            {
                DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                if ((designItem != null) && (designItem.View != null) && (designItem.View is Control))
                    return true;
            }
            return false;
        }

        public void DesignerResetTransform()
        {
            if (_designContext != null)
            {
                DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                if ((designItem != null) && (designItem.View != null) && (designItem.View is Control))
                {
                    DesignItemProperty prop = designItem.Properties.GetProperty(Control.RenderTransformProperty);
                    if (prop != null)
                    {
                        prop.Reset();
                    }
                }
            }
        }

        public bool IsEnabledDesignerResetTransform()
        {
            if (_designContext != null)
            {
                DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                if ((designItem != null) && (designItem.View != null) && (designItem.View is Control))
                    return true;
            }
            return false;
        }

        void Selection_SelectionChanged(object sender, DesignItemCollectionEventArgs e)
        {
            if (_designContext == null)
                return;
            _RotationChangedSelection = true;
            DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
            if ((designItem != null) && (designItem.View != null) && (designItem.View is Control))
            {
                DesignItemProperty prop = designItem.Properties.GetProperty(Control.RenderTransformProperty);
                if ((prop != null) && (prop.Value != null) && (typeof(RotateTransform).IsAssignableFrom(prop.Value.ComponentType)))
                {
                    DesignItemProperty propAngle = prop.Value.Properties.GetProperty(RotateTransform.AngleProperty);
                    if (propAngle != null)
                    {
                        if (propAngle.ValueOnInstance != null)
                            _UpDownRotation.Value = (Double)propAngle.ValueOnInstance;
                        else
                        {
                            propAngle.SetValue((double)0.0);
                            _UpDownRotation.Value = (Double)propAngle.ValueOnInstance;
                        }
                    }
                }
                else
                {
                    _UpDownRotation.Value = 0;
                }
            }
            else
            {
                _UpDownRotation.Value = 0;
            }
            _RotationChangedSelection = false;
            Focus();
        }

        #region Commands

        public bool CanUndo()
        {
            UndoService undoService = GetService<UndoService>();
            return undoService != null && undoService.CanUndo;
        }

        public void Undo()
        {
            UndoService undoService = GetService<UndoService>();
            IUndoAction action = undoService.UndoActions.First();
#if DEBUG
            Debug.WriteLine("Undo " + action.Title);
#endif
            undoService.Undo();
            _designContext.Services.Selection.SetSelectedComponents(GetLiveElements(action.AffectedElements));
        }

        public bool CanRedo()
        {
            UndoService undoService = GetService<UndoService>();
            return undoService != null && undoService.CanRedo;
        }

        public void Redo()
        {
            UndoService undoService = GetService<UndoService>();
            IUndoAction action = undoService.RedoActions.First();
#if DEBUG
            Debug.WriteLine("Redo " + action.Title);
#endif
            undoService.Redo();
            _designContext.Services.Selection.SetSelectedComponents(GetLiveElements(action.AffectedElements));
        }

        public bool CanCopyOrCut()
        {
            ISelectionService selectionService = GetService<ISelectionService>();
            if (selectionService != null)
            {
                if (selectionService.SelectedItems.Count == 0)
                    return false;
                if (selectionService.SelectedItems.Count == 1 && selectionService.PrimarySelection == DesignContext.RootItem)
                    return false;
            }
            return true;
        }

        public bool CanCopy()
        {
            return _designContext?.Services?.CopyPasteService?.CanCopy(_designContext) == true;
        }

        public void Copy()
        {
            _designContext?.Services?.CopyPasteService?.Copy(_designContext);
        }

        public bool CanCut()
        {
            return _designContext?.Services?.CopyPasteService?.CanCut(_designContext) == true;
        }

        public void Cut()
        {
            _designContext?.Services?.CopyPasteService?.Cut(_designContext);
        }

        public bool CanDelete()
        {
            return _designContext?.Services?.CopyPasteService?.CanDelete(_designContext) == true;
        }

        public event EventHandler OnDeleteItem;

        public void Delete()
        {
            _designContext?.Services?.CopyPasteService?.Delete(_designContext);
            if (OnDeleteItem != null)
                OnDeleteItem(this, new EventArgs());
        }

        public bool CanPaste()
        {
            return _designContext?.Services?.CopyPasteService?.CanPaste(_designContext) == true;
        }

        public void Paste()
        {
            _designContext?.Services?.CopyPasteService?.Paste(_designContext);
        }

        public bool CanSelectAll()
        {
            return DesignContext != null;
        }

        //TODO: Do not select layout root
        public void SelectAll()
        {
            var items = Descendants(DesignContext.RootItem).Where(item => ModelTools.CanSelectComponent(item)).ToArray();
            DesignContext.Services.Selection.SetSelectedComponents(items);
        }

        public void UnselectAll()
        {
            DesignContext.Services.Selection.SetSelectedComponents(null);
        }

        //TODO: Share with Outline / PlacementBehavior
        public static IEnumerable<DesignItem> DescendantsAndSelf(DesignItem item)
        {
            yield return item;
            foreach (var child in Descendants(item))
            {
                yield return child;
            }
        }

        public static IEnumerable<DesignItem> Descendants(DesignItem item)
        {
            if (item.ContentPropertyName != null)
            {
                var content = item.ContentProperty;
                if (content.IsCollection)
                {
                    foreach (var child in content.CollectionElements)
                    {
                        foreach (var child2 in DescendantsAndSelf(child))
                        {
                            yield return child2;
                        }
                    }
                }
                else
                {
                    if (content.Value != null)
                    {
                        foreach (var child2 in DescendantsAndSelf(content.Value))
                        {
                            yield return child2;
                        }
                    }
                }
            }
        }

        // Filters an element list, dropping all elements that are not part of the xaml document
        // (e.g. because they were deleted).
        static List<DesignItem> GetLiveElements(ICollection<DesignItem> items)
        {
            List<DesignItem> result = new List<DesignItem>(items.Count);
            foreach (DesignItem item in items)
            {
                if (ModelTools.IsInDocument(item) && ModelTools.CanSelectComponent(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        T GetService<T>() where T : class
        {
            if (_designContext != null)
                return _designContext.Services.GetService<T>();
            else
                return null;
        }

        #endregion

        private void GroupedOnly_Checked(object sender, RoutedEventArgs e)
        {
            if (_GroupedOnly.IsChecked.HasValue)
                _designPanel.HitTestGroupedPreferred = _GroupedOnly.IsChecked.Value;
            else
                _designPanel.HitTestGroupedPreferred = false;
        }

        public Double HitTestLayerNum
        {
            get
            {
                return Convert.ToDouble(_designPanel.HitTestLayer);
            }
            set
            {
                _designPanel.HitTestLayer = Convert.ToInt32(value);
            }
        }
    }

    public class DesignSurfaceRasterAdorner : Control
    {
        public DesignSurfaceRasterAdorner(Control adornedElement, int rasterSize)
        {
            _adornedElement = adornedElement;
            _RasterSize = rasterSize;
            IsHitTestVisible = false;
        }

        private readonly Control _adornedElement;
        public Control AdornedElement => _adornedElement;

        private int _RasterSize;
        public int RasterSize
        {
            get
            {
                return _RasterSize;
            }
            set
            {
                _RasterSize = value;
                this.InvalidateVisual();
            }
        }

        public override void Render(DrawingContext drawingContext)
        {
            Size desiredSize = this.AdornedElement.DesiredSize;
            if (RasterSize < 1)
                return;

            if (desiredSize.Width == 0 && desiredSize.Height == 0)
            {
                desiredSize = new Size(this.AdornedElement.Bounds.Width, this.AdornedElement.Bounds.Height);
            }

            GeometryGroup geomGroup = new GeometryGroup();
            for (int y = RasterSize; y < desiredSize.Width; y += RasterSize)
            {
                geomGroup.Children.Add(new LineGeometry(new Point(y, 0), new Point(y, desiredSize.Height)));
            }
            for (int x = RasterSize; x < desiredSize.Height; x += RasterSize)
            {
                geomGroup.Children.Add(new LineGeometry(new Point(0, x), new Point(desiredSize.Width, x)));
            }

            SolidColorBrush penBrush = new SolidColorBrush(Colors.Blue);
            penBrush.Opacity = 0.2;
            Pen renderPen = new Pen(penBrush, 1);
            drawingContext.DrawGeometry(Brushes.Transparent, renderPen, geomGroup);
        }
    }
}
