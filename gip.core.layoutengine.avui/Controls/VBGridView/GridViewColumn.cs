using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.Styling;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace gip.core.layoutengine.avui
{
    //[StyleTypedProperty(Property = "HeaderContainerStyle", StyleTargetType = typeof(GridViewColumnHeader))]
    public class GridViewColumn : StyledElement
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        public GridViewColumn()
        {
            ResetPrivateData();

            // Descendant of this class can override the metadata to give it
            // a value other than NaN and without trigger the propertychange
            // callback and thus, result in _state be out-of-sync with the
            // Width property.
            _state = Double.IsNaN(Width) ? ColumnMeasureState.Init : ColumnMeasureState.SpecificWidth;
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Methods
        //
        //-------------------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", this.GetType(), Header);
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------

        #region Public Properties

        // For all the StyledProperties on GridViewColumn, null is treated as unset,
        // because it's impossible to distinguish null and unset.
        // Change a property between null and unset, OnPropertyChanged will not be called.

        #region Header

        /// <summary>
        /// Header StyledProperty
        /// </summary>
        public static readonly StyledProperty<object> HeaderProperty =
            AvaloniaProperty.Register<GridViewColumn, object>(
                nameof(Header));

        /// <summary>
        /// If provide a GridViewColumnHeader or an instance of its sub class , it will be used as header.
        /// Otherwise, it will be used as content of header
        /// </summary>
        /// <remarks>
        /// typical usage is to assign the content of the header or the container
        /// <code>
        ///         GridViewColumn column = new GridViewColumn();
        ///         column.Header = "Name";
        /// </code>
        /// or
        /// <code>
        ///         GridViewColumnHeader header = new GridViewColumnHeader();
        ///         header.Content = "Name";
        ///         header.Click += ...
        ///         ...
        ///         GridViewColumn column = new GridViewColumn();
        ///         column.Header = header;
        /// </code>
        /// </remarks>
        [Content]
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        #endregion Header

        #region HeaderContainerStyle

        /// <summary>
        /// HeaderContainerStyle StyledProperty
        /// </summary>
        public static readonly StyledProperty<ControlTheme> HeaderContainerStyleProperty =
            AvaloniaProperty.Register<GridViewColumn, ControlTheme>(
                nameof(HeaderContainerStyle));

        /// <summary>
        /// Header container's style
        /// </summary>
        public ControlTheme HeaderContainerStyle
        {
            get { return GetValue(HeaderContainerStyleProperty); }
            set { SetValue(HeaderContainerStyleProperty, value); }
        }

        #endregion HeaderContainerStyle

        #region HeaderTemplate

        /// <summary>
        /// HeaderTemplate StyledProperty
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
            AvaloniaProperty.Register<GridViewColumn, IDataTemplate>(
                nameof(HeaderTemplate));

        /// <summary>
        /// column header template
        /// </summary>
        public IDataTemplate HeaderTemplate
        {
            get { return GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        #endregion  HeaderTemplate

        #region HeaderTemplateSelector

        /// <summary>
        /// HeaderTemplateSelector StyledProperty - In Avalonia, we use IDataTemplate directly
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> HeaderTemplateSelectorProperty =
            AvaloniaProperty.Register<GridViewColumn, IDataTemplate>(
                nameof(HeaderTemplateSelector));

        /// <summary>
        /// header template selector - In Avalonia, this is just another IDataTemplate
        /// </summary>
        /// <remarks>
        ///     This property is ignored if <seealso cref="HeaderTemplate"/> is set.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDataTemplate HeaderTemplateSelector
        {
            get { return GetValue(HeaderTemplateSelectorProperty); }
            set { SetValue(HeaderTemplateSelectorProperty, value); }
        }

        #endregion HeaderTemplateSelector

        #region HeaderStringFormat

        /// <summary>
        ///     The StyledProperty for the HeaderStringFormat property.
        ///     Flags:              None
        ///     Default Value:      null
        /// </summary>
        public static readonly StyledProperty<string> HeaderStringFormatProperty =
            AvaloniaProperty.Register<GridViewColumn, string>(
                nameof(HeaderStringFormat));

        /// <summary>
        ///     HeaderStringFormat is the format used to display the header content as a string.
        ///     This arises only when no template is available.
        /// </summary>
        public string HeaderStringFormat
        {
            get { return GetValue(HeaderStringFormatProperty); }
            set { SetValue(HeaderStringFormatProperty, value); }
        }

        #endregion HeaderStringFormat

        #region DisplayMemberBinding

        /// <summary>
        /// DisplayMemberBinding StyledProperty
        /// </summary>
        public static readonly StyledProperty<BindingBase> DisplayMemberBindingProperty =
            AvaloniaProperty.Register<GridViewColumn, BindingBase>(
                nameof(DisplayMemberBinding));

        /// <summary>
        /// BindingBase is be used to generate each cell of this column.
        /// Set to null make this property do not work.
        /// </summary>
        public BindingBase DisplayMemberBinding
        {
            get { return GetValue(DisplayMemberBindingProperty); }
            set { SetValue(DisplayMemberBindingProperty, value); }
        }

        /// <summary>
        /// If DisplayMemberBinding property changed, NotifyPropertyChanged event will be raised with this string.
        /// </summary>
        internal const string c_DisplayMemberBindingName = "DisplayMemberBinding";

        #endregion

        #region CellTemplate

        /// <summary>
        /// CellTemplate StyledProperty
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> CellTemplateProperty =
            AvaloniaProperty.Register<GridViewColumn, IDataTemplate>(
                nameof(CellTemplate));

        /// <summary>
        /// template for this column's item UI
        /// </summary>
        public IDataTemplate CellTemplate
        {
            get { return GetValue(CellTemplateProperty); }
            set { SetValue(CellTemplateProperty, value); }
        }

        #endregion

        #region CellTemplateSelector

        /// <summary>
        /// CellTemplateSelector StyledProperty - In Avalonia, we use IDataTemplate directly
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> CellTemplateSelectorProperty =
            AvaloniaProperty.Register<GridViewColumn, IDataTemplate>(
                nameof(CellTemplateSelector));

        /// <summary>
        /// templateSelector for this column's item UI - In Avalonia, this is just another IDataTemplate
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDataTemplate CellTemplateSelector
        {
            get { return GetValue(CellTemplateSelectorProperty); }
            set { SetValue(CellTemplateSelectorProperty, value); }
        }

        #endregion

        #region Width

        /// <summary>
        /// Width StyledProperty
        /// </summary>
        public static readonly StyledProperty<double> WidthProperty =
            Control.WidthProperty.AddOwner<GridViewColumn>();

        /// <summary>
        /// width of the column
        /// </summary>
        /// <remarks>
        /// The default value is Double.NaN which means size to max visible item width.
        /// </remarks>
        [TypeConverter(typeof(LengthConverter))]
        public double Width
        {
            get { return GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        #endregion

        #region ActualWidth

        /// <summary>
        /// ActualWidth StyledProperty
        /// </summary>
        public static readonly StyledProperty<double> ActualWidthProperty =
            AvaloniaProperty.Register<GridViewColumn, double>(
                nameof(ActualWidth),
                defaultValue: 0.0);

        /// <summary>
        /// actual width of this column
        /// </summary>
        public double ActualWidth
        {
            get { return GetValue(ActualWidthProperty); }
            private set { SetValue(ActualWidthProperty, value); }
        }

        #endregion

        #endregion Public Properties

        //-------------------------------------------------------------------
        //
        //  Property Change Handling
        //
        //-------------------------------------------------------------------

        #region Property Change Handling

        /// <summary>
        /// Override OnPropertyChanged to handle property change notifications
        /// </summary>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == HeaderTemplateProperty)
            {
                OnHeaderTemplateChanged(change.OldValue as IDataTemplate, change.NewValue as IDataTemplate);
            }
            else if (change.Property == HeaderTemplateSelectorProperty)
            {
                OnHeaderTemplateSelectorChanged(change.OldValue as IDataTemplate, change.NewValue as IDataTemplate);
            }
            else if (change.Property == HeaderStringFormatProperty)
            {
                OnHeaderStringFormatChanged(change.OldValue as string, change.NewValue as string);
            }
            else if (change.Property == DisplayMemberBindingProperty)
            {
                OnDisplayMemberBindingChanged(change.OldValue as BindingBase, change.NewValue as BindingBase);
            }
            else if (change.Property == ActualWidthProperty)
            {
                OnActualWidthChanged((double)change.OldValue, (double)change.NewValue);
            }
            else if (change.Property == WidthProperty)
            {
                OnWidthChanged((double)change.OldValue, (double)change.NewValue);
            }
        }

        private void OnHeaderTemplateChanged(IDataTemplate oldValue, IDataTemplate newValue)
        {
            // Check to prevent Template and TemplateSelector at the same time
            CheckTemplateAndTemplateSelector("Header", HeaderTemplateProperty, HeaderTemplateSelectorProperty);
        }

        private void OnHeaderTemplateSelectorChanged(IDataTemplate oldValue, IDataTemplate newValue)
        {
            // Check to prevent Template and TemplateSelector at the same time
            CheckTemplateAndTemplateSelector("Header", HeaderTemplateProperty, HeaderTemplateSelectorProperty);
        }

        /// <summary>
        ///     This method is invoked when the HeaderStringFormat property changes.
        /// </summary>
        /// <param name="oldHeaderStringFormat">The old value of the HeaderStringFormat property.</param>
        /// <param name="newHeaderStringFormat">The new value of the HeaderStringFormat property.</param>
        protected virtual void OnHeaderStringFormatChanged(string oldHeaderStringFormat, string newHeaderStringFormat)
        {
        }

        /// <summary>
        /// This method is invoked when the DisplayMemberBinding property changes.
        /// </summary>
        /// <param name="oldValue">The old value of the DisplayMemberBinding property.</param>
        /// <param name="newValue">The new value of the DisplayMemberBinding property.</param>
        private void OnDisplayMemberBindingChanged(BindingBase oldValue, BindingBase newValue)
        {
            // Custom logic for DisplayMemberBinding changes can be added here
        }

        /// <summary>
        /// This method is invoked when the ActualWidth property changes.
        /// </summary>
        /// <param name="oldValue">The old value of the ActualWidth property.</param>
        /// <param name="newValue">The new value of the ActualWidth property.</param>
        private void OnActualWidthChanged(double oldValue, double newValue)
        {
            // Validate the new ActualWidth value
            if (Double.IsNaN(newValue) || Double.IsInfinity(newValue) || newValue < 0.0)
            {
                Debug.Fail("Invalid value for ActualWidth.");
                // Reset to old value if invalid
                SetValue(ActualWidthProperty, oldValue);
            }
        }

        private void OnWidthChanged(double oldWidth, double newWidth)
        {
            // reset DesiredWidth if width is set to auto
            State = Double.IsNaN(newWidth) ? ColumnMeasureState.Init : ColumnMeasureState.SpecificWidth;
        }

        /// <summary>
        /// Helper method to check Template and TemplateSelector at the same time
        /// </summary>
        private void CheckTemplateAndTemplateSelector(string templateName, AvaloniaProperty templateProperty, AvaloniaProperty templateSelectorProperty)
        {
            // In Avalonia, we can have both template and templateSelector set simultaneously
            // This is just a placeholder for the original WPF Helper.CheckTemplateAndTemplateSelector functionality
            // You can implement custom logic here if needed
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Internal Methods
        //
        //-------------------------------------------------------------------

        #region Internal Methods

        /// <summary>
        /// ensure final column width is no less than a value
        /// </summary>
        internal double EnsureWidth(double width)
        {
            if (width > DesiredWidth)
            {
                DesiredWidth = width;
            }
            return DesiredWidth;
        }

        /// <summary>
        /// column collection should call this when remove a column from the collection.
        /// </summary>
        internal void ResetPrivateData()
        {
            _actualIndex = -1;
            _desiredWidth = 0.0;
            _state = Double.IsNaN(Width) ? ColumnMeasureState.Init : ColumnMeasureState.SpecificWidth;
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Internal Properties
        //
        //-------------------------------------------------------------------

        #region Internal Properties

        /// <summary>
        ///  Reachable State Transition Diagram:
        ///
        ///                        +- - - - - - - - - - +
        ///                        |       Init         |
        ///                        +- - - - - - - - - - +
        ///                           / /|   A   |\ \
        ///                          / /     |     \ \
        ///                         / /      |      \ \
        ///                        / /       |       \ \
        ///                       / /        |        \ \
        ///                      / /         |         \ \
        ///                     / /          |          \ \
        ///                   |/ /           |           \ \|
        ///    +--------------------+        |        +--------------------+
        ///    |      Headered      |--------+------->|        Data        |
        ///    +--------------------+        |        +--------------------+
        ///                      \           |           /
        ///                       \          |          /
        ///                        \         |         /
        ///                         \        |        /
        ///                          \       |       /
        ///                           \      |      /
        ///                            \|    |    |/
        ///                        +--------------------+
        ///                        |   SpecificWidth    |
        ///                        +--------------------+
        ///
        /// Note:
        ///
        /// 1) Init is a intermidiated state, that is a column should not stop on such a state;
        /// 2) Headered, Data and SpecificWidth are terminal state, that is a column can stop at
        ///     the state if no further data change / user interaction to trigger a change.
        ///
        /// Typical state transiton flows:
        ///
        ///   Case 1: column is auto, LV has header and data
        ///     Init --> [ Headered --> ] Data
        ///
        ///   Case 2: column is auto, LV has header but no data
        ///     Init --> Headered
        ///
        ///   Case 3: column has a specified width
        ///     SpecificWidth
        ///
        ///   Case 4: couble click a column of case 3
        ///     SpecificWidth --> Init --> Headered / Data (depends on the data)
        ///
        ///   Case 5: resize a column which has width as auto
        ///     Headered / Data --> SpecificWidth
        ///
        /// </summary>
        internal ColumnMeasureState State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;

                    if (value != ColumnMeasureState.Init) // Headered, Data or SpecificWidth
                    {
                        UpdateActualWidth();
                    }
                    else
                    {
                        DesiredWidth = 0.0;
                    }
                }
                else if (value == ColumnMeasureState.SpecificWidth)
                {
                    UpdateActualWidth();
                }
            }
        }

        // NOTE: Perf optimization. To avoid re-search index again and again
        // by every GridViewRowPresenter, add an index here.
        internal int ActualIndex
        {
            get { return _actualIndex; }
            set { _actualIndex = value; }
        }

        /// <summary>
        /// Minimum width requirement for this column. Shared by all visible cells in this column
        /// </summary>
        /// <remarks>
        /// Below table shows an example of how column width is shared:
        ///
        ///     1. In the first round of layout, DesiredWidth continue to grow when each row comes into measure
        ///
        ///     2. after the 1st round, the desired width for this column is decided, each row on layout updated
        ///         with check this value with its copy of maxDesiredWidth, if not equal, triger another round of
        ///         measure.
        ///
        ///     3. after 2nd round of layout, all rows should be in same size.
        ///     +------------+-----------+--------------+------------+------------+-------------+
        ///     |            |   Width   |    Cell      |  Desired   | Presenter  |   Column    |
        ///     |            |           | DesiredWidth |   Width    | LocalCopy  |    State    |
        ///     |------------+-----------+--------------+------------+------------|-------------|
        ///     | 1st round  |   NaN     |              |    10.0    |            |    Init     |
        ///     |            |           |              |            |            |             |
        ///     |  (row 1)   |           |    12.0      |    12.0    |            |             |
        ///     |  (row 2)   |           |    70.0      |    70.0    |            |             |
        ///     |  (row 3)   |           |    80.0      |    80.0    |            |             |
        ///     |  (row 4)   |           |    60.0      |    80.0    |            |             |
        ///     |------------+-----------+--------------+------------+------------|-------------|
        ///     | layout     |   NaN     |              |            |            |             |
        ///     | updated    |           |              |            |            |             |
        ///     |            |           |              |            |            |             |
        ///     | [hdr_row]  |           |              |            |            | [Headered]* |
        ///     |            |           |              |            |            |             |
        ///     |  (row 1)   |           |              |    80.0    |    12.0    |    Data     |
        ///     |  (row 2)   |           |              |    80.0    |    70.0    |             |
        ///     |  (row 3)   |           |              |    80.0    |    80.0    |             |
        ///     |  (row 4)   |           |              |    80.0    |    80.0    |             |
        ///     |------------+-----------+--------------+------------+------------|-------------|
        ///     | 2nd round  |   NaN     |              |            |            |             |
        ///     |            |           |              |            |            |             |
        ///     |  (row 1)   |           |    12.0      |    80.0    |    80.0    |             |
        ///     |  (row 2)   |           |    70.0      |    80.0    |    80.0    |             |
        ///     +------------+-----------+--------------+------------+------------+-------------+
        ///
        ///   * Depends on the tree structure, it is possible that HeaderRowPresenter accomplish first
        ///     layout first. So the column state can be Headered for a while. But will be changed to
        ///     'Data' once a data row accomplish its first layout.
        ///
        /// </remarks>
        internal double DesiredWidth
        {
            get { return _desiredWidth; }
            private set { _desiredWidth = value; }
        }

        internal const string c_ActualWidthName = "ActualWidth";

        #endregion

        //-------------------------------------------------------------------
        //
        //  Private Methods / Fields
        //
        //-------------------------------------------------------------------

        #region Private Methods

        /// <summary>
        /// force ActualWidth to be reevaluated
        /// </summary>
        private void UpdateActualWidth()
        {
            ActualWidth = (State == ColumnMeasureState.SpecificWidth) ? Width : DesiredWidth;
        }

        #endregion

        #region Private Fields

        private double _desiredWidth;
        private int _actualIndex;
        private ColumnMeasureState _state;

        #endregion
    }

    /// <summary>
    /// States of column when doing layout
    /// See GridViewColumn.State for reachable state transition diagram
    /// </summary>
    internal enum ColumnMeasureState
    {
        /// <summary>
        /// Column width is just initialized and will size to content width
        /// </summary>
        Init = 0,

        /// <summary>
        /// Column width reach max desired width of header(s) in this column
        /// </summary>
        Headered = 1,

        /// <summary>
        /// Column width reach max desired width of data row(s) in this column
        /// </summary>
        Data = 2,

        /// <summary>
        /// Column has a specific value as width
        /// </summary>
        SpecificWidth = 3
    }
}
