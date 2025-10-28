//Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.ComponentModel;            // DesignerSerializationVisibility


namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// GridView is a built-in view of the ListView control.  It is unique
    /// from other built-in views because of its table-like layout.  Data in
    /// details view is shown in a table with each row corresponding to an
    /// entity in the data collection and each column being generated from a
    /// data-bound template, populated with values from the bound data entity.
    /// </summary>

    //[StyleTypedProperty(Property = "ColumnHeaderContainerStyle", StyleTargetType = typeof(GridViewColumnHeader))]
    public class GridView : ViewBase, IAddChild
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        //-------------------------------------------------------------------
        //
        //  Public Methods
        //
        //-------------------------------------------------------------------

        #region Public Methods

        /// <summary>
        ///  Add an object child to this control
        /// </summary>
        void IAddChild.AddChild(object column)
        {
            AddChild(column);
        }

        /// <summary>
        ///  Add an object child to this control
        /// </summary>
        protected virtual void AddChild(object column)
        {
            GridViewColumn c = column as GridViewColumn;

            if (c != null)
            {
                Columns.Add(c);
            }
            else
            {
                throw new InvalidOperationException("ListView_IllegalChildrenType");
            }
        }


        //void IAddChild.AddText(string text)
        //{
        //    AddText(text);
        //}

        /// <summary>
        ///  Add a text string to this control
        /// </summary>
        protected virtual void AddText(string text)
        {
            AddChild(text);
        }

        /// <summary>
        ///     Returns a string representation of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("GridView, Columns: {0}", Columns.Count);
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------

        #region Public Properties

        // For all the DPs on GridView, null is treated as unset,
        // because it's impossible to distinguish null and unset.
        // Change a property between null and unset, PropertyChangedCallback will not be called.

        // ----------------------------------------------------------------------------
        //  Defines the names of the resources to be consumed by the GridView style.
        //  Used to restyle several roles of GridView without having to restyle
        //  all of the control.
        // ----------------------------------------------------------------------------

        #region StyleKeys

        /// <summary>
        /// Key used to mark the template of ScrollViewer for use by GridView
        /// </summary>
        public static object GridViewScrollViewerStyleKey
        {
            get
            {
                return "GridViewScrollViewer";
                //return SystemResourceKey.GridViewScrollViewerStyleKey;
            }
        }

        /// <summary>
        /// Key used to mark the ControlTheme of GridView
        /// </summary>
        public static object GridViewStyleKey
        {
            get
            {
                return "GridViewStyle";
                //return SystemResourceKey.GridViewStyleKey;
            }
        }

        /// <summary>
        /// Key used to mark the ControlTheme of ItemContainer
        /// </summary>
        public static object GridViewItemContainerStyleKey
        {
            get
            {
                return "GridViewItemContainerStyle";
                //return SystemResourceKey.GridViewItemContainerStyleKey;
            }
        }

        #endregion StyleKeys

        #region GridViewColumnCollection Attached Property

        /// <summary>
        /// Reads the attached property GridViewColumnCollection from the given element.
        /// </summary>
        /// <param name="element">The element from which to read the GridViewColumnCollection attached property.</param>
        /// <returns>The property's value.</returns>
        public static GridViewColumnCollection GetColumnCollection(AvaloniaObject element)
        {
            ArgumentNullException.ThrowIfNull(element);
            return element.GetValue(ColumnCollectionProperty);
        }

        /// <summary>
        /// Writes the attached property GridViewColumnCollection to the given element.
        /// </summary>
        /// <param name="element">The element to which to write the GridViewColumnCollection attached property.</param>
        /// <param name="collection">The collection to set</param>
        public static void SetColumnCollection(AvaloniaObject element, GridViewColumnCollection collection)
        {
            ArgumentNullException.ThrowIfNull(element);
            element.SetValue(ColumnCollectionProperty, collection);
        }

        /// <summary>
        /// This is the attached property registered for the GridView' ColumnCollection attached property.
        /// </summary>
        public static readonly AttachedProperty<GridViewColumnCollection> ColumnCollectionProperty =
            AvaloniaProperty.RegisterAttached<GridView, AvaloniaObject, GridViewColumnCollection>(
                "ColumnCollection");

        /// <summary>
        /// Whether should serialize ColumnCollection attach DP
        /// </summary>
        /// <param name="obj">Object on which this DP is set</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ShouldSerializeColumnCollection(AvaloniaObject obj)
        {
            ListViewItem listViewItem = obj as ListViewItem;
            if (listViewItem != null)
            {
                ListView listView = listViewItem.FindAncestorOfType<ListView>();
                if (listView != null)
                {
                    GridView gridView = listView.View as GridView;
                    if (gridView != null)
                    {
                        // if GridViewColumnCollection attached on ListViewItem is Details.Columns, it should't be serialized.
                        GridViewColumnCollection localValue = listViewItem.GetValue(ColumnCollectionProperty);
                        return (localValue != gridView.Columns);
                    }
                }
            }

            return true;
        }

        #endregion

        /// <summary> GridViewColumn List</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Content]
        public GridViewColumnCollection Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new GridViewColumnCollection
                    {
                        // Give the collection a back-link, this is used for the inheritance context
                        Owner = this,
                        InViewMode = true
                    };
                }

                return _columns;
            }
        }

        #region ColumnHeaderContainerStyle

        /// <summary>
        /// ColumnHeaderContainerStyleProperty StyledProperty
        /// </summary>
        public static readonly StyledProperty<ControlTheme> ColumnHeaderContainerStyleProperty =
                AvaloniaProperty.Register<GridView, ControlTheme>(
                    nameof(ColumnHeaderContainerStyle)
                );

        /// <summary>
        /// header container's style
        /// </summary>
        public ControlTheme ColumnHeaderContainerStyle
        {
            get { return GetValue(ColumnHeaderContainerStyleProperty); }
            set { SetValue(ColumnHeaderContainerStyleProperty, value); }
        }

        #endregion // ColumnHeaderContainerStyle

        #region ColumnHeaderTemplate

        /// <summary>
        /// ColumnHeaderTemplate StyledProperty
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> ColumnHeaderTemplateProperty =
            AvaloniaProperty.Register<GridView, IDataTemplate>(
                nameof(ColumnHeaderTemplate),
                coerce: CoerceColumnHeaderTemplate
            );

        /// <summary>
        /// column header template
        /// </summary>
        public IDataTemplate ColumnHeaderTemplate
        {
            get { return GetValue(ColumnHeaderTemplateProperty); }
            set { SetValue(ColumnHeaderTemplateProperty, value); }
        }

        private static IDataTemplate CoerceColumnHeaderTemplate(AvaloniaObject sender, IDataTemplate value)
        {
            GridView dv = (GridView)sender;
            
            // Check to prevent Template and TemplateSelector at the same time
            Helper.CheckTemplateAndTemplateSelector("GridViewColumnHeader", ColumnHeaderTemplateProperty, ColumnHeaderTemplateSelectorProperty, dv);
            
            return value;
        }

        #endregion  ColumnHeaderTemplate

        #region ColumnHeaderTemplateSelector

        /// <summary>
        /// ColumnHeaderTemplateSelector StyledProperty
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> ColumnHeaderTemplateSelectorProperty =
            AvaloniaProperty.Register<GridView, IDataTemplate>(
                nameof(ColumnHeaderTemplateSelector),
                coerce: CoerceColumnHeaderTemplateSelector
            );

        /// <summary>
        /// header template selector
        /// </summary>
        /// <remarks>
        ///     This property is ignored if <seealso cref="ColumnHeaderTemplate"/> is set.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDataTemplate ColumnHeaderTemplateSelector
        {
            get { return GetValue(ColumnHeaderTemplateSelectorProperty); }
            set { SetValue(ColumnHeaderTemplateSelectorProperty, value); }
        }

        private static IDataTemplate CoerceColumnHeaderTemplateSelector(AvaloniaObject sender, IDataTemplate value)
        {
            GridView dv = (GridView)sender;

            // Check to prevent Template and TemplateSelector at the same time
            Helper.CheckTemplateAndTemplateSelector("GridViewColumnHeader", ColumnHeaderTemplateProperty, ColumnHeaderTemplateSelectorProperty, dv);
            
            return value;
        }

        #endregion ColumnHeaderTemplateSelector

        #region ColumnHeaderStringFormat

        /// <summary>
        /// ColumnHeaderStringFormat StyledProperty
        /// </summary>
        public static readonly StyledProperty<string> ColumnHeaderStringFormatProperty =
            AvaloniaProperty.Register<GridView, string>(
                nameof(ColumnHeaderStringFormat)
            );

        /// <summary>
        /// column header string format
        /// </summary>
        public string ColumnHeaderStringFormat
        {
            get { return GetValue(ColumnHeaderStringFormatProperty); }
            set { SetValue(ColumnHeaderStringFormatProperty, value); }
        }

        #endregion  ColumnHeaderStringFormat

        #region AllowsColumnReorder

        /// <summary>
        /// AllowsColumnReorderProperty StyledProperty
        /// </summary>
        public static readonly StyledProperty<bool> AllowsColumnReorderProperty =
                AvaloniaProperty.Register<GridView, bool>(
                    nameof(AllowsColumnReorder),
                    defaultValue: true
                );

        /// <summary>
        /// AllowsColumnReorder
        /// </summary>
        public bool AllowsColumnReorder
        {
            get { return GetValue(AllowsColumnReorderProperty); }
            set { SetValue(AllowsColumnReorderProperty, value); }
        }

        #endregion AllowsColumnReorder

        #region ColumnHeaderContextMenu

        /// <summary>
        /// ColumnHeaderContextMenuProperty StyledProperty
        /// </summary>
        public static readonly StyledProperty<ContextMenu> ColumnHeaderContextMenuProperty =
                AvaloniaProperty.Register<GridView, ContextMenu>(
                    nameof(ColumnHeaderContextMenu)
                );

        /// <summary>
        /// ColumnHeaderContextMenu
        /// </summary>
        public ContextMenu ColumnHeaderContextMenu
        {
            get { return GetValue(ColumnHeaderContextMenuProperty); }
            set { SetValue(ColumnHeaderContextMenuProperty, value); }
        }

        #endregion ColumnHeaderContextMenu

        #region ColumnHeaderToolTip

        /// <summary>
        /// ColumnHeaderToolTipProperty StyledProperty
        /// </summary>
        public static readonly StyledProperty<object> ColumnHeaderToolTipProperty =
                AvaloniaProperty.Register<GridView, object>(
                    nameof(ColumnHeaderToolTip)
                );

        /// <summary>
        /// ColumnHeaderToolTip
        /// </summary>
        public object ColumnHeaderToolTip
        {
            get { return GetValue(ColumnHeaderToolTipProperty); }
            set { SetValue(ColumnHeaderToolTipProperty, value); }
        }

        #endregion ColumnHeaderToolTip

        #endregion // Public Properties

        //-------------------------------------------------------------------
        //
        //  Protected Methods
        //
        //-------------------------------------------------------------------

        #region Protected Methods

        /// <summary>
        /// called when ListView is prepare container for item.
        /// GridView override this method to attache the column collection
        /// </summary>
        /// <param name="item">the container</param>
        protected internal override void PrepareItem(ListViewItem item)
        {
            base.PrepareItem(item);

            // attach GridViewColumnCollection to ListViewItem.
            SetColumnCollection(item, _columns);
        }

        /// <summary>
        /// called when ListView is clear container for item.
        /// GridView override this method to clear the column collection
        /// </summary>
        /// <param name="item">the container</param>
        protected internal override void ClearItem(ListViewItem item)
        {
            item.ClearValue(ColumnCollectionProperty);

            base.ClearItem(item);
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Protected Properties
        //
        //-------------------------------------------------------------------

        #region Protected Properties

        /// <summary>
        /// override with style key of GridView.
        /// </summary>
        protected internal override object DefaultStyleKey
        {
            get { return GridViewStyleKey; }
        }

        /// <summary>
        /// override with style key using GridViewRowPresenter.
        /// </summary>
        protected internal override object ItemContainerDefaultStyleKey
        {
            get { return GridViewItemContainerStyleKey; }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Internal Methods
        //
        //-------------------------------------------------------------------

        #region Internal Methods

        //internal override void OnInheritanceContextChangedCore(EventArgs args)
        //{
        //    base.OnInheritanceContextChangedCore(args);

        //    if (_columns != null)
        //    {
        //        foreach (GridViewColumn column in _columns)
        //        {
        //            column.OnInheritanceContextChanged(args);
        //        }
        //    }
        //}

        // Propagate theme changes to contained headers
        internal override void OnThemeChanged()
        {
            //if (_columns != null)
            //{
            //    for (int i=0; i<_columns.Count; i++)
            //    {
            //        _columns[i].OnThemeChanged();
            //    }
            //}
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Private Fields
        //
        //-------------------------------------------------------------------

        #region Private Fields

        private GridViewColumnCollection _columns;

        #endregion // Private Fields

        internal GridViewHeaderRowPresenter HeaderRowPresenter
        {
            get { return _gvheaderRP; }
            set { _gvheaderRP = value; }
        }

        private GridViewHeaderRowPresenter _gvheaderRP;
    }

    /// <summary>
    /// Helper class for template and template selector validation
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// Check to prevent Template and TemplateSelector at the same time
        /// </summary>
        internal static void CheckTemplateAndTemplateSelector(string templateName, AvaloniaProperty templateProperty, AvaloniaProperty templateSelectorProperty, AvaloniaObject obj)
        {
            // In Avalonia, we typically don't need this validation as the template system works differently
            // But we can keep this as a placeholder for any future validation needs
        }
    }

    /// <summary>
    /// Helper class to provide boxed boolean values
    /// </summary>
    internal static class BooleanBoxes
    {
        internal static readonly object TrueBox = true;
        internal static readonly object FalseBox = false;
    }
}
