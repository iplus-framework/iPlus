// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using gip.ext.designer.avui.Xaml;
using gip.ext.designer.avui.Services;
using System.Diagnostics;
using gip.ext.xamldom.avui;
using System.Threading;
using System.Globalization;
using gip.ext.design.avui;
using gip.ext.widgets.avui;
using gip.core.datamodel;
using gip.ext.designer.avui.Controls;
using System.ComponentModel;
using Avalonia.Controls;

namespace gip.ext.designer.avui
{
    /// <summary>
    /// Surface hosting the WPF designer.
    /// </summary>
    public partial class DesignSurface : UserControl, INotifyPropertyChanged
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

            this.SourceUpdated += new EventHandler<DataTransferEventArgs>(DesignSurface_SourceUpdated);
            this.TargetUpdated += new EventHandler<DataTransferEventArgs>(DesignSurface_TargetUpdated);

            Binding binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath("RasterSize");
            binding.NotifyOnSourceUpdated = true;
            binding.NotifyOnTargetUpdated = true;
            binding.Mode = BindingMode.OneWayToSource;
            _UpDownRasterSize.SetBinding(NumericUpDown.ValueProperty, binding);

            _RotationChangedSelection = true;
            binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath("RotationAngle");
            binding.NotifyOnSourceUpdated = true;
            binding.NotifyOnTargetUpdated = true;
            binding.Mode = BindingMode.OneWayToSource;
            _UpDownRotation.SetBinding(NumericUpDown.ValueProperty, binding);
            _RotationChangedSelection = false;

            binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath("HitTestLayerNum");
            binding.NotifyOnSourceUpdated = true;
            binding.NotifyOnTargetUpdated = true;
            binding.Mode = BindingMode.TwoWay;
            _LayerHitTest.SetBinding(NumericUpDown.ValueProperty, binding);

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

            this.AddCommandHandler(Commands.RotateLeftCommand, () => ModelTools.ApplyTransform(this.DesignContext.Services.Selection.PrimarySelection, new RotateTransform(-90), true, this.DesignContext.RootItem == this.DesignContext.Services.Selection.PrimarySelection ? LayoutTransformProperty : RenderTransformProperty), () => this.DesignContext.Services.Selection.PrimarySelection != null);
            this.AddCommandHandler(Commands.RotateRightCommand, () => ModelTools.ApplyTransform(this.DesignContext.Services.Selection.PrimarySelection, new RotateTransform(90), true, this.DesignContext.RootItem == this.DesignContext.Services.Selection.PrimarySelection ? LayoutTransformProperty : RenderTransformProperty), () => this.DesignContext.Services.Selection.PrimarySelection != null);

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
                _designPanel.RequestBringIntoView += _partDesignContent_RequestBringIntoView;

            }
        }


        private bool enableBringIntoView = false;

        public void ScrollIntoView(DesignItem designItem)
        {
            enableBringIntoView = true;
            LogicalTreeHelper.BringIntoView(designItem.View);
            enableBringIntoView = false;
        }

        void _partDesignContent_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            if (!enableBringIntoView)
                e.Handled = true;
            enableBringIntoView = false;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.OriginalSource == uxZoom)
            {
                UnselectAll();
            }
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

            this.AddHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(_sceneContainer_PreviewMouseMove), true);
            this.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(_sceneContainer_PreviewMouseDown), true);
            this.AddHandler(UIElement.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(_sceneContainer_PreviewMouseUp), true);
            _PreviewHandlerAdded = true;
            if (context.RootItem != null)
            {
                _sceneContainer.Child = context.RootItem.View;
            }

            context.Services.RunWhenAvailable<UndoService>(
                undoService => undoService.UndoStackChanged += delegate
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            );
            context.Services.Selection.SelectionChanged += delegate
            {
                CommandManager.InvalidateRequerySuggested();
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
            // Damir: Norbert deine Änderung vom 23.05. hatte zur Folge, dass die Methode designPanel_DragOver von einem falschen DesignManagerControlTool aufgerufen worden ist
            //if (clearServices)
            //{
                if (_designContext != null)
                {
                    foreach (object o in _designContext.Services.AllServices)
                    {
                        IDisposable d = o as IDisposable;
                        if (d != null) d.Dispose();
                    }
                }
            //}
            uxZoom.DesignContext = null;
            _designContext = null;
            _designPanel.Context = null;
            if (_PreviewHandlerAdded)
            {
                this.RemoveHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(_sceneContainer_PreviewMouseMove));
                this.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(_sceneContainer_PreviewMouseDown));
                this.RemoveHandler(UIElement.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(_sceneContainer_PreviewMouseUp));
            }
            _sceneContainer.Child = null;
            _designPanel.Adorners.Clear();
        }

        void _sceneContainer_PreviewMouseMove(object sender, MouseEventArgs e)
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
        void _sceneContainer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_sceneContainer.Child != null)
            {
                _PointStart = e.GetPosition(_sceneContainer.Child);
                _XCoord1.Text = System.Convert.ToInt32(_PointStart.X).ToString();
                _YCoord1.Text = System.Convert.ToInt32(_PointStart.Y).ToString();
            }
        }

        void _sceneContainer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
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
            AdornerLayer layer;
            if ((_designPanel.AdornerRaster != null) && RasterSize < 5)
            {
                layer = AdornerLayer.GetAdornerLayer(_designPanel);
                layer.Remove(_designPanel.AdornerRaster);
                _designPanel.AdornerRaster = null;
                return;
            }
            if (_designPanel.AdornerRaster != null)
            {
                _designPanel.AdornerRaster.RasterSize = System.Convert.ToInt32(RasterSize);
                return;
            }
            layer = AdornerLayer.GetAdornerLayer(_designPanel);
            _designPanel.AdornerRaster = new DesignSurfaceRasterAdorner(_sceneContainer, System.Convert.ToInt32(RasterSize));
            layer.Add(_designPanel.AdornerRaster);
        }

        void DesignSurface_SourceUpdated(object sender, DataTransferEventArgs e)
        {
        }

        void DesignSurface_TargetUpdated(object sender, DataTransferEventArgs e)
        {
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
                    if ((designItem != null) && (designItem.View != null) && (designItem.View is UIElement))
                    {
                        DesignItemProperty prop = designItem.Properties.GetProperty(UIElement.RenderTransformProperty);
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
                if ((designItem != null) && (designItem.View != null) && (designItem.View is UIElement))
                    return true;
            }
            return false;
        }

        public void DesignerFlipHorz()
        {
            if (_designContext != null)
            {
                DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                if ((designItem != null) && (designItem.View != null) && (designItem.View is UIElement))
                {
                    DesignItemProperty prop = designItem.Properties.GetProperty(UIElement.RenderTransformProperty);
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
                if ((designItem != null) && (designItem.View != null) && (designItem.View is UIElement))
                    return true;
            }
            return false;
        }

        public void DesignerFlipVert()
        {
            if (_designContext != null)
            {
                DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                if ((designItem != null) && (designItem.View != null) && (designItem.View is UIElement))
                {
                    DesignItemProperty prop = designItem.Properties.GetProperty(UIElement.RenderTransformProperty);
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
                if ((designItem != null) && (designItem.View != null) && (designItem.View is UIElement))
                    return true;
            }
            return false;
        }

        public void DesignerResetTransform()
        {
            if (_designContext != null)
            {
                DesignItem designItem = _designContext.Services.Selection.PrimarySelection;
                if ((designItem != null) && (designItem.View != null) && (designItem.View is UIElement))
                {
                    DesignItemProperty prop = designItem.Properties.GetProperty(UIElement.RenderTransformProperty);
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
                if ((designItem != null) && (designItem.View != null) && (designItem.View is UIElement))
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
            if ((designItem != null) && (designItem.View != null) && (designItem.View is UIElement))
            {
                DesignItemProperty prop = designItem.Properties.GetProperty(UIElement.RenderTransformProperty);
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
            Keyboard.Focus(this);
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
            //XamlDesignContext xamlContext = _designContext as XamlDesignContext;
            //ISelectionService selectionService = GetService<ISelectionService>();
            //if (xamlContext != null && selectionService != null)
            //{
            //    xamlContext.XamlEditAction.Copy(selectionService.SelectedItems);
            //}
        }

        public bool CanCut()
        {
            return _designContext?.Services?.CopyPasteService?.CanCut(_designContext) == true;
        }

        public void Cut()
        {
            _designContext?.Services?.CopyPasteService?.Cut(_designContext);
            //XamlDesignContext xamlContext = _designContext as XamlDesignContext;
            //ISelectionService selectionService = GetService<ISelectionService>();
            //if (xamlContext != null && selectionService != null)
            //{
            //    xamlContext.XamlEditAction.Cut(selectionService.SelectedItems);
            //}
        }

        public bool CanDelete()
        {
            return _designContext?.Services?.CopyPasteService?.CanDelete(_designContext) == true;
            //if (_designContext != null)
            //{
            //    return ModelTools.CanDeleteComponents(_designContext.Services.Selection.SelectedItems);
            //}
            //return false;
        }

 
        public event EventHandler OnDeleteItem;

        public void Delete()
        {
            _designContext?.Services?.CopyPasteService?.Delete(_designContext);
            if (OnDeleteItem != null)
                    OnDeleteItem(this, new EventArgs());
            //if (_designContext != null)
            //{
            //    //NOTE(ihrastinski): on delete key up on design item
            //    //ModelTools.DeleteComponents(_designContext.Services.Selection.SelectedItems);
            //    if (OnDeleteItem != null)
            //        OnDeleteItem(this, new EventArgs());
            //}
        }



        public bool CanPaste()
        {
            return _designContext?.Services?.CopyPasteService?.CanPasteAsync(_designContext) == true;
            //ISelectionService selectionService = GetService<ISelectionService>();
            //if (selectionService != null && selectionService.SelectedItems.Count != 0)
            //{
            //    string xaml = Clipboard.GetText(TextDataFormat.Xaml);
            //    if (xaml != "" && xaml != " ")
            //        return true;
            //}
            //return false;
        }

        public void Paste()
        {
            _designContext?.Services?.CopyPasteService?.Paste(_designContext);
            //XamlDesignContext xamlContext = _designContext as XamlDesignContext;
            //if (xamlContext != null)
            //{
            //    xamlContext.XamlEditAction.Paste();
            //}
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

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null)
                ev(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class DesignSurfaceRasterAdorner : Adorner
    {
        public DesignSurfaceRasterAdorner(UIElement adornedElement, int rasterSize)
            : base(adornedElement)
        {
            _RasterSize = rasterSize;
            IsHitTestVisible = false;
        }

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

        protected override void OnRender(DrawingContext drawingContext)
        {
            Size desiredSize = this.AdornedElement.DesiredSize;
            if (RasterSize < 1)
                return;
            //FrameworkElement designPanel = this.AdornedElement as FrameworkElement;
            //if (designPanel != null)
            //{
            //    if ((designPanel.ActualWidth > 0.01) && (designPanel.ActualHeight > 0.01))
            //    {
            //        desiredSize.Width = designPanel.ActualWidth;
            //        desiredSize.Height = designPanel.ActualHeight;
            //    }
            //}

            if (desiredSize.Width == 0 && desiredSize.Height == 0)
            {
                desiredSize.Width = (this.AdornedElement as FrameworkElement).ActualWidth;
                desiredSize.Height = (this.AdornedElement as FrameworkElement).ActualHeight;
            }

            //GeneralTransform x2 = this.AdornedElement.TransformToAncestor((Visual)(this.AdornedElement as Border).Child);
            //GeneralTransform x2 = this.TransformToVisual(this.AdornedElement);
            //Transform x2 = new TranslateTransform(200, 200);
            //drawingContext.PushTransform((Transform)x2);

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
            renderPen.DashStyle = DashStyles.Solid;
            drawingContext.DrawGeometry(Brushes.Transparent, renderPen, geomGroup);
        }
    }

}
