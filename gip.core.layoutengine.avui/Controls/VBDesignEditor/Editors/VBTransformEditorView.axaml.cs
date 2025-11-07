// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using System;
using System.ComponentModel;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a view editor for tranformations.
    /// </summary>
    public partial class VBTransformEditorView : UserControl
    {
		/// <summary>
        /// Create a new VBTransformEditorView instance.
		/// </summary>
		public VBTransformEditorView()
		{
            InitializeComponent();
            DataContext = this;
        }

        bool _Loaded = false;
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (_Loaded)
                return;
            RefreshView();
            _Loaded = true;
        }

        /// <summary>
        /// Defines the Property styled property.
        /// </summary>
        public static readonly StyledProperty<IPropertyNode> PropertyProperty =
            AvaloniaProperty.Register<VBTransformEditorView, IPropertyNode>(nameof(Property));

        /// <summary>
        /// Gets or sets the property node.
        /// </summary>
        public IPropertyNode Property
        {
            get => GetValue(PropertyProperty);
            set => SetValue(PropertyProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == PropertyProperty)
            {
                RefreshView();
            }
        }

        protected DesignItem DesignObject
        {
            get
            {
                if (Property != null)
                    return Property.ValueItem;
                return null;
            }
        }

        private void RefreshView()
        {
            if (Property != null)
            {
                if ((Property.ValueItem == null) && (PART_TransformEditor.Content != null))
                    PART_TransformEditor.Content = null;
                else if (Property.Value is TransformGroup)
                {
                    if ((PART_TransformEditor.Content == null) || !(PART_TransformEditor.Content is VBTransformEditorGroupView))
                        PART_TransformEditor.Content = new VBTransformEditorGroupView();
                    (PART_TransformEditor.Content as VBTransformEditorGroupView).Property = this.Property;
                }
                else if (Property.Value is Transform)
                {
                    if ((PART_TransformEditor.Content == null) || !(PART_TransformEditor.Content is VBTransformEditorSingleView))
                        PART_TransformEditor.Content = new VBTransformEditorSingleView();
                    (PART_TransformEditor.Content as VBTransformEditorSingleView).Property = this.Property;
                }
            }
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            if (Property == null)
                return;
            Property.Reset();
            RefreshView();
        }

        private void PART_One_Click(object sender, RoutedEventArgs e)
        {
            //PART_One.ContextMenu.IsOpen = true;
            if (Property != null)
            {
                Property.Reset();
                Property.Value = new RotateTransform();
                RefreshView();
            }
        }

        private void PART_More_Click(object sender, RoutedEventArgs e)
        {
            if (Property != null)
            {
                Property.Reset();
                Property.Value = new TransformGroup();
                RefreshView();
            }
        }

        private void VBMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem itemClicked = sender as MenuItem;
            switch (itemClicked.Header as String)
            {
                case "RotateTransform":
                    if (!(Property.Value is RotateTransform))
                    {
                        Property.Reset();
                        Property.Value = new RotateTransform();
                    }
                    break;
                case "ScaleTransform":
                    if (!(Property.Value is ScaleTransform))
                    {
                        Property.Reset();
                        Property.Value = new ScaleTransform();
                    }
                    break;
                case "SkewTransform":
                    if (!(Property.Value is SkewTransform))
                    {
                        Property.Reset();
                        Property.Value = new SkewTransform();
                    }
                    break;
                case "TranslateTransform":
                    if (!(Property.Value is TranslateTransform))
                    {
                        Property.Reset();
                        Property.Value = new TranslateTransform();
                    }
                    break;
                case "MatrixTransform":
                    if (!(Property.Value is MatrixTransform))
                    {
                        Property.Reset();
                        Property.Value = new MatrixTransform();
                    }
                    break;
                default:
                    Property.Reset();
                    break;
            }
            RefreshView();
            PART_One.ContextMenu.Close();
        }
    }
}
