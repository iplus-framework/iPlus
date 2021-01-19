using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents the tile item in <see cref="VBTileGrid"/>.
    /// </summary>
    /// <summary>
    /// Stellt das Kachelobjekt in <see cref="VBTileGrid"/> dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTile'}de{'VBTile'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTile : Button, IACInteractiveObject, IACMenuBuilderWPFTree, IACObject, IVBTileGrid
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TileStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTileGrid/Themes/TileStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "MenuStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTileGrid/Themes/TileStyleAero.xaml" },
        };
        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBTile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTile), new FrameworkPropertyMetadata(typeof(VBTile)));
        }

        /// <summary>
        /// Creates a new instance of VBTile.
        /// </summary>
        public VBTile()
        {
        }

        /// <summary>
        /// Overides the OnApplyTemplate method.
        /// </summary>
        public override void OnApplyTemplate()
        {
            //Grid grid = Template.FindName("TileItemGrid", this) as Grid;
            //if (grid != null)
            //{
            //    var accentColor = new SolidColorBrush(AccentColorSet.ActiveSet["SystemAccent"]);
                
            //    if (accentColor != null)
            //        grid.Background = accentColor;
            //    else
            //        grid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3385ff"));
                
            //}
            base.OnApplyTemplate();
        }

        Point mouseOffset;
        bool _NewRow = false;
        int _NewRowInsertIndex;

        private Grid _ParentGrid
        {
            get
            {
                if (Parent != null && Parent is Grid)
                    return Parent as Grid;
                return null;
            }
        }

        private VBTileGrid _VBTileGrid
        {
            get
            {
                if (_ParentGrid != null)
                    return _ParentGrid.Parent as VBTileGrid;
                return null;
            }
        }

        #region Dependency prop

        /// <summary>
        /// Gets or sets the Column. Determines in which column is placed.
        /// </summary>
        [Category("VBControl")]
        public int Column
        {
            get { return (int)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the Column.
        /// </summary>
        public static readonly DependencyProperty ColumnProperty =
            DependencyProperty.Register("Column", typeof(int), typeof(VBTile), new PropertyMetadata(0, OnDepPropChanged));

        /// <summary>
        /// Gets or sets the Row. Determines in which row is placed.
        /// </summary>
        [Category("VBControl")]
        public int Row
        {
            get { return (int)GetValue(RowProperty); }
            set { SetValue(RowProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the Row.
        /// </summary>
        public static readonly DependencyProperty RowProperty =
            DependencyProperty.Register("Row", typeof(int), typeof(VBTile), new PropertyMetadata(0, OnDepPropChanged));

        /// <summary>
        /// Gets or sets the title of VBTile.
        /// </summary>
        [Category("VBControl")]
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the Title.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(VBTile));

        /// <summary>
        /// Gets or sets the ACUrl. Represents the ACUrl of BSO which VBTile it starts.
        /// </summary>
        public string ACUrl
        {
            get { return (string)GetValue(ACUrlProperty); }
            set { SetValue(ACUrlProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the ACUrl.
        /// </summary>
        public static readonly DependencyProperty ACUrlProperty =
            DependencyProperty.Register(Const.ACUrlPrefix, typeof(string), typeof(VBTile), new PropertyMetadata("", OnDepPropChanged));

        /// <summary>
        /// Gets or sets the IconACUrl.
        /// </summary>
        [ACPropertyInfo(999)]
        [Category("VBControl")]
        public string IconACUrl
        {
            get { return (string)GetValue(IconACUrlProperty); }
            set { SetValue(IconACUrlProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the IconACUrl.
        /// </summary>
        public static readonly DependencyProperty IconACUrlProperty =
            DependencyProperty.Register("IconACUrl", typeof(string), typeof(VBTileGrid), new PropertyMetadata(""));

        private static void OnDepPropChanged(DependencyObject dependencyObject,
                                             DependencyPropertyChangedEventArgs args)
        {
            VBTile vbTile = dependencyObject as VBTile;
            if(args.Property == RowProperty)
            {
                vbTile.Row = (int)args.NewValue;
                Grid.SetRow(vbTile, (int)args.NewValue);
            }
            else if(args.Property == ColumnProperty)
            {
                vbTile.Column = (int)args.NewValue;
                Grid.SetColumn(vbTile, (int)args.NewValue);
                if(vbTile != null && vbTile._VBTileGrid != null)
                    vbTile._VBTileGrid.AlignTile(vbTile);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the postion and inserts the row.
        /// </summary>
        /// <param name="col">The column.</param>
        /// <param name="row">The row.</param>
        /// <param name="dropPoint">The drop point.</param>
        public void GetPositionAndInsertRow(out int col, out int row, Nullable<Point> dropPoint = null)
        {
            _NewRowInsertIndex = 0;
            Grid grid = Parent as Grid; 
            Point point = Mouse.GetPosition(grid);
            if (dropPoint != null && dropPoint.HasValue)
                point = dropPoint.Value;
            row = 0;
            col = 0;
            double accumulatedHeight = 0.0;
            double accumulatedWidth = 0.0;

            // calc row mouse was over
            foreach (var rowDefinition in grid.RowDefinitions)
            {
                accumulatedHeight += rowDefinition.ActualHeight;
                if (accumulatedHeight >= point.Y && rowDefinition.ActualHeight > 30)
                {
                    if (rowDefinition.ActualHeight == 40)
                    {
                        _NewRow = true;
                        _NewRowInsertIndex = grid.RowDefinitions.IndexOf(rowDefinition);
                    }
                    break;
                }
                row++;
                if (row >= grid.RowDefinitions.Count())
                    _NewRow = true;
            }

            // calc col mouse was over
            foreach (var columnDefinition in grid.ColumnDefinitions)
            {
                accumulatedWidth += columnDefinition.ActualWidth;
                if (accumulatedWidth >= point.X )
                    break;
                col++;
            }
            if (_NewRow)
            {
                if (_NewRowInsertIndex == 0)
                {
                    _NewRowInsertIndex = grid.RowDefinitions.Count() - 1;
                    row = _NewRowInsertIndex;
                }
                VBTileGrid.InsertRow(grid, _VBTileGrid.TileSize, null, _NewRowInsertIndex);
                _NewRow = false;
            }
        }

        private void CheckAndMoveTile(Grid parent, int oldCol, int oldRow)
        {
            VBTile tile = parent.Children.OfType<VBTile>().FirstOrDefault(c => c.Row == this.Row && c.Column == this.Column && c != this);
            if (tile != null)
            {
                int row, col;
                row = tile.Row;
                col = tile.Column;
                bool changeGroup = false;

                if (row < oldRow)
                {
                    for (int i = row; i < oldRow; i++)
                        if (parent.Children.OfType<TextBlock>().FirstOrDefault(c => Grid.GetRow(c) == i) != null)
                        {
                            changeGroup = true;
                            break;
                        }
                }
                else
                {
                    for (int j = oldRow; j < row; j++)
                    {
                        if (parent.Children.OfType<TextBlock>().FirstOrDefault(c => Grid.GetRow(c) == j) != null)
                        {
                            changeGroup = true;
                            break;
                        }
                    }
                }

                if(changeGroup)
                {
                    int groupRow = 0, spliterRow = 0;
                    for(int g = row; g > 0; g--)
                    {
                        if (parent.Children.OfType<TextBox>().FirstOrDefault(c => Grid.GetRow(c) == g) != null)
                        {
                            groupRow = g;
                            break;
                        }
                    }

                    for (int s = row; s < parent.RowDefinitions.Count(); s++)
                    {
                        if (parent.Children.OfType<TextBlock>().FirstOrDefault(c => Grid.GetRow(c) == s) != null)
                        {
                            spliterRow = s;
                            break;
                        }
                    }

                    VBTile freePos = null;
                    for (int i = col; i < parent.ColumnDefinitions.Count(); i++)
                    {
                        for (int j = groupRow+1; j < spliterRow; j++)
                        {
                            if (parent.RowDefinitions[j].ActualHeight > 40)
                            {
                                freePos = parent.Children.OfType<VBTile>().FirstOrDefault(c => c.Row == j && c.Column == i);
                                if (freePos == null)
                                {
                                    row = j;
                                    col = i;
                                    break;
                                }
                            }
                        }
                    }

                    if(freePos != null)
                    {
                        VBTileGrid.InsertRow(parent, _VBTileGrid.TileSize, parent.RowDefinitions[spliterRow]);
                        row = spliterRow;
                    }
                    tile.Row = row;
                    tile.Column = col;
                    Grid.SetRow(tile, row);
                    Grid.SetColumn(tile, col);
                }
                else
                {
                    tile.Row = oldRow;
                    tile.Column = oldCol;
                    Grid.SetRow(tile, oldRow);
                    Grid.SetColumn(tile, oldCol);
                }
            }
        }

        private void CheckAndDeleteEmptyRow(Grid parent, int oldRow)
        {
            if(!_VBTileGrid.Items.Any(c => c.TileRow == oldRow))
            {
                Border currentBorder = VBTileGrid.FindBorder(parent, oldRow);
                Grid.SetRowSpan(currentBorder, Grid.GetRowSpan(currentBorder) - 1);
                _ParentGrid.RowDefinitions.RemoveAt(oldRow);
                CorrectElementsPosition(parent, oldRow, false);
            }
        }

        /// <summary>
        /// Corrects the elements position in grid.
        /// </summary>
        /// <param name="grid">The grid where will be elements corrected.</param>
        /// <param name="correctIndex">The correct index.</param>
        /// <param name="correctPositive">Determines is correcting positive or negative.</param>
        public static void CorrectElementsPosition(Grid grid, int correctIndex, bool correctPositive = true)
        {
            int correctFactor = 1;
            if (!correctPositive)
                correctFactor = -1;
            foreach (var element in grid.Children.OfType<FrameworkElement>().Where(c => Grid.GetRow(c) >= correctIndex))
            {
                int oldRow = Grid.GetRow(element);
                int row = oldRow + correctFactor;
                Grid.SetRow(element, row);
                if (element is VBTile)
                    ((VBTile)element).Row = row;
                else if(element is TextBox)
                {
                    var item = ((VBTileGrid)grid.Parent).Items.FirstOrDefault(c => c.TileRow == oldRow && c.VBTileType == Global.VBTileType.Group);
                    item.TileRow = (short)row;
                }
            }
        }

        /// <summary>
        /// Renames the title of VBTile.
        /// </summary>
        [ACMethodInteraction("", "en{'Rename'}de{'Rename'}",999,false)]
        public void RenameTitle()
        {
            TextBox tb = Template.FindName("PART_TextBox", this) as TextBox;
            if (tb != null)
            {
                tb.IsReadOnly = false;
                tb.SelectAll();
                tb.Focus();
                tb.IsReadOnly = false;
            }
        }

        /// <summary>
        /// Determines is enabled title renaming or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledRenameTitle()
        {
            return true;
        }

        /// <summary>
        /// Deletes the VBTile.
        /// </summary>
        [ACMethodInteraction("", "en{'Delete'}de{'Löschen'}", 999, false)]
        public void DeleteTile()
        {
            if(!string.IsNullOrEmpty(_VBTileGrid.OnTileDeleted))
            {
                int tileRow = Grid.GetRow(this);
                if (_VBTileGrid.Items.Count(c => c.TileRow == tileRow) == 1)
                {
                    Border currentBorder = VBTileGrid.FindBorder(_ParentGrid, tileRow);
                    Grid.SetRowSpan(currentBorder, Grid.GetRowSpan(currentBorder) - 1);
                    _ParentGrid.RowDefinitions.RemoveAt(tileRow);
                    CorrectElementsPosition(_ParentGrid, tileRow, false);
                }
                ContextACObject.ACUrlCommand(_VBTileGrid.OnTileDeleted, this as IVBTileGrid);
                BindingOperations.ClearAllBindings(this);
                _ParentGrid.Children.Remove(this);
            }
        }

        #endregion

        #region OverrideEvents

        /// <summary>
        /// Handles the OnPreviewMouseLeftButtonDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            mouseOffset = Mouse.GetPosition(this);
            CaptureMouse();
            Panel.SetZIndex(this, 2);
            base.OnPreviewMouseLeftButtonDown(e);
        }

        /// <summary>
        /// Handles the OnPreviewMouseLeftButtonUp event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            if (Math.Abs(Margin.Bottom) <= 3 && Math.Abs(Margin.Top) <= 3 && Math.Abs(Margin.Left) <= 3 && Math.Abs(Margin.Right) <= 3)
            {
                ContextACObject.ACUrlCommand(_VBTileGrid.VBContent, ACUrl);
            }
            else
            {
                int oldRow = this.Row;
                int oldCol = this.Column;
                Margin = new Thickness();
                int col, row;
                GetPositionAndInsertRow(out col, out row);
                Grid grid = Parent as Grid;
                this.Column = col;
                this.Row = row;
                Grid.SetRow(this, Row);
                Grid.SetColumn(this, Column);
                CheckAndMoveTile(grid, oldCol, oldRow);
                CheckAndDeleteEmptyRow(grid, oldRow);
            }
            Panel.SetZIndex(this, 1);
            base.OnPreviewMouseLeftButtonUp(e);
        }

        /// <summary>
        /// Handles the OnPreviewMouseMove event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Point mouseDelta = Mouse.GetPosition(this);
                mouseDelta.Offset(-mouseOffset.X, -mouseOffset.Y);

                Margin = new Thickness(
                    Margin.Left + mouseDelta.X,
                    Margin.Top + mouseDelta.Y,
                    Margin.Right - mouseDelta.X,
                    Margin.Bottom - mouseDelta.Y);
            }
            base.OnPreviewMouseMove(e);
        }

        /// <summary>
        /// Handles the OnLostFocus event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            TextBox tb = Template.FindName("PART_TextBox", this) as TextBox;
            if (tb != null)
            {
                tb.IsReadOnly = true;
                tb.Background = new SolidColorBrush(Colors.Transparent);
            }
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Handles the OnKeyUp event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = Template.FindName("PART_TextBox", this) as TextBox;
                if (tb != null)
                {
                    tb.IsReadOnly = true;
                    tb.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
            base.OnKeyUp(e);
        }

        /// <summary>
        /// Handles the OnPreviewMouseRightButtonDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (ContextACObject != null)
            {
                Point point = e.GetPosition(this);
                ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.X, point.Y, Global.ElementActionType.ContextMenu);
                ((IACComponent)ContextACObject).ACAction(actionArgs);
                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {
                    VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
                    if (vbContextMenu.PlacementTarget == null)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.IsOpen = true;
                }
                e.Handled = true;
            }
            base.OnPreviewMouseRightButtonDown(e);
        }

        #endregion

        #region IACMenuBuilderWpfTree

        /// <summary>
        /// Appends the menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        /// <param name="acMenuItemList">The acMenuItemlist parameter.</param>
        public void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            VBLogicalTreeHelper.AppendMenu(this, vbContent, vbControl, ref acMenuItemList);
            this.GetDesignManagerMenu(VBContent, ref acMenuItemList);
        }

        /// <summary>
        /// Gets the menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        /// <returns>The list of ACMenu items.</returns>
        public ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            AppendMenu(vbContent, vbControl, ref acMenuItemList);
            return acMenuItemList;
        }
        #endregion

        #region IACInteractiveObject

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get;set;
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get { return DataContext as IACObject; }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBTile), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            if(actionArgs.ElementAction == Global.ElementActionType.ACCommand)
            {
                var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                if (query.Any())
                {
                    ACCommand acCommand = query.First() as ACCommand;
                    ACUrlCommand(acCommand.GetACUrl(), null);
                }
            }
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return true;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return this.Name; }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return this.ReflectACType(); }
        }

        List<IACObject> _ACContentList = new List<IACObject>();
        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return _ACContentList; }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return true;
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        #endregion

        #region IVBTileGrid

        /// <summary>
        /// Gets the VBTile column.
        /// </summary>
        public short? TileColumn
        {
            get
            {
                return (short)Column;
            }
            set
            {
            }
        }

        /// <summary>
        /// Gets the VBTile row.
        /// </summary>
        public short TileRow
        {
            get
            {
                return (short)Row;
            }
            set
            {
            }
        }

        /// <summary>
        /// Gets the VBTile type.
        /// </summary>
        public Global.VBTileType VBTileType
        {
            get
            {
                return Global.VBTileType.Tile;
            }
            set
            {
            }
        }

        #endregion
    }
}
