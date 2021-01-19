using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a grid which is container for the <see cref="VBTile"/>. 
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Gitter dar, das Container für <see cref="VBTile"/> ist. 
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTileGrid'}de{'VBTileGrid'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTileGrid : VBScrollViewer, IACMenuBuilderWPFTree, IACInteractiveObject, IACObject
    {
        /// <summary>
        /// Creates a new instance of the VBTileGrid.
        /// </summary>
        public VBTileGrid()
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            AllowDrop = true;
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            InitVBControl();
            base.OnApplyTemplate();
        }

        bool _Initialized = false;
        Point _helperPoint;
        int _RowIndex = 0;
        internal static int Col;

        private void InitVBControl()
        {
            if (ContextACObject == null || string.IsNullOrEmpty(VBSource) || _Initialized)
                return;

            Binding bind = new Binding();
            bind.Source = this.ContextACObject;
            bind.Path = new PropertyPath(VBSource);
            bind.NotifyOnSourceUpdated = true;
            bind.NotifyOnTargetUpdated = true;
            SetBinding(ItemsProperty, bind);

            if(this.Content == null)
            {
                if (Items != null)
                    foreach (var item in Items)
                        if (item.TileColumn.HasValue && item.TileColumn.Value + 1 > Columns)
                            Columns = item.TileColumn.Value + 1;

                Col = Columns;
                Grid grid = new Grid();
                grid.MinWidth = TileSize * Columns + 20;
                grid.HorizontalAlignment = HorizontalAlignment.Center;
                grid.VerticalAlignment = VerticalAlignment.Top;
                grid.Background = Brushes.Transparent;
               


                for (int i = 0; i < Columns; i++)
                {
                    ColumnDefinition colDef = new ColumnDefinition();
                    if(i == 0)
                        colDef.Width = new GridLength(TileSize+10);
                    else if(i == Columns-1)
                        colDef.Width = new GridLength(TileSize + 10);
                    else
                        colDef.Width = new GridLength(TileSize);
                    grid.ColumnDefinitions.Add(colDef);
                }

                this.Content = grid;
                InsertTilesAndRows(grid);
            }
            _Initialized = true;
        }

        private void DeInitVBControl(IACComponent bso)
        {
            if (Content != null)
            {
                foreach (FrameworkElement fe in ((Grid)Content).Children.OfType<FrameworkElement>())
                    BindingOperations.ClearAllBindings(fe);
                ((Grid)Content).Children.Clear();
            }
            BindingOperations.ClearBinding(this, ItemsProperty);
            BindingOperations.ClearAllBindings(this);
            _Initialized = false;
        }

        #region DependencyProperties

        /// <summary>
        /// Gets or sets the VBTileGrid items.
        /// </summary>
        public IEnumerable<IVBTileGrid> Items
        {
            get { return (IEnumerable<IVBTileGrid>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the Items.
        /// </summary>
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<IVBTileGrid>), typeof(VBTileGrid), new PropertyMetadata(OnDepPropChanged));

        /// <summary>
        /// Determines the nubmer of columns in the VBTileGrid (default:2).
        /// </summary>
        [Category("VBControl")]
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the Columns.
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(VBTileGrid), new PropertyMetadata(2));

        /// <summary>
        /// Determines the size of the VBTile.(default:100.0)
        /// </summary>
        [Category("VBControl")]
        public double TileSize
        {
            get { return (double)GetValue(TileSizeProperty); }
            set { SetValue(TileSizeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the TileSize
        /// </summary>
        public static readonly DependencyProperty TileSizeProperty =
            DependencyProperty.Register("TileSize", typeof(double), typeof(VBTileGrid), new PropertyMetadata(100.0));

        /// <summary>
        /// Gets or sets the OnTileAdded.
        /// </summary>
        public string OnTileAdded
        {
            get { return (string)GetValue(OnTileAddedProperty); }
            set { SetValue(OnTileAddedProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the OnTileAdded.
        /// </summary>
        public static readonly DependencyProperty OnTileAddedProperty =
            DependencyProperty.Register("OnTileAdded", typeof(string), typeof(VBTileGrid));

        /// <summary>
        /// Gets or sets the OnTileDeleted.
        /// </summary>
        public string OnTileDeleted
        {
            get { return (string)GetValue(OnTileDeletedProperty); }
            set { SetValue(OnTileDeletedProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the OnTileDeleted.
        /// </summary>
        public static readonly DependencyProperty OnTileDeletedProperty =
            DependencyProperty.Register("OnTileDeleted", typeof(string), typeof(VBTileGrid));

        /// <summary>
        /// Represents the property where you enter the name of Bso's property, which contains a list with items whcih will be shown in this control.
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get { return (string)GetValue(VBSourceProperty); }
            set { SetValue(VBSourceProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the VBSource.
        /// </summary>
        public static readonly DependencyProperty VBSourceProperty =
            DependencyProperty.Register("VBSource", typeof(string), typeof(VBTileGrid));

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBTileGrid), new FrameworkPropertyMetadata(null, 
                FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBTileGrid),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private static void OnDepPropChanged(DependencyObject dependencyObject,
              DependencyPropertyChangedEventArgs args)
        {
            VBTileGrid thisControl = dependencyObject as VBTileGrid;
            if (args.Property == BSOACComponentProperty)
            {
                if (args.NewValue == null && args.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = args.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
                }
            }
        }

        #endregion

        #region Override events

        /// <summary>
        /// Handles the OnPreviewMouseRightButtonDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreviewMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ContextACObject != null)
            {
                _helperPoint = e.GetPosition(this.Content as Grid);
                ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, _helperPoint.X, _helperPoint.Y, Global.ElementActionType.ContextMenu);
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
                //e.Handled = true;
            }
            base.OnPreviewMouseRightButtonUp(e);
        }

        /// <summary>
        /// Handles the OnDragOver event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
        }

        /// <summary>
        /// Handles the OnDrop event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDrop(DragEventArgs e)
        {
            IACInteractiveObject item = VBDragDrop.GetDropObject(e);
            if(item != null && item.ACContentList != null && item.ACContentList.Any())
            {
                ACCommand containter = item.ACContentList.FirstOrDefault() as ACCommand;
                if(containter != null && !string.IsNullOrEmpty((containter).ACUrl))
                {
                    int col = 0, row = 0;
                    int rowNum = ((Grid)this.Content).RowDefinitions.Count();
                    VBTile vbTile = new VBTile() { Height = TileSize, Width = TileSize };
                    vbTile.Title = containter.ACCaption;
                    vbTile.ACUrl = (containter).ACUrl;
                    if (containter is ACMenuItem && !string.IsNullOrEmpty(((ACMenuItem)containter).IconACUrl))
                        vbTile.IconACUrl = ((ACMenuItem)containter).IconACUrl;
                    ((Grid)this.Content).Children.Add(vbTile);
                    vbTile.GetPositionAndInsertRow(out col, out row, e.GetPosition(this.Content as Grid));
                    Grid.SetColumn(vbTile, col);
                    Grid.SetRow(vbTile, row);
                    vbTile.Column = col;
                    vbTile.Row = row;

                    AlignTile(vbTile);

                    if(!string.IsNullOrEmpty(OnTileAdded))
                    {
                        ContextACObject.ACUrlCommand(OnTileAdded, vbTile);
                        IVBTileGrid tileItem = Items.FirstOrDefault(c => c.TileRow == vbTile.TileRow && c.TileColumn == vbTile.TileColumn && c.ACUrl == vbTile.ACUrl);
                        if (tileItem != null)
                            SetBinding(vbTile, tileItem); 
                    }
                }
            }
            base.OnDrop(e);
        }

        #endregion

        #region Methods

        private void InsertTilesAndRows(Grid grid)
        {
            RowDefinition splitter = null;
            if (Items == null || !Items.Any())
            {
                AddGroup();
                return;
            }
            foreach (var item in Items.OrderBy(c => c.TileRow).ThenBy(x => x.TileColumn))
            {
                if (item.VBTileType == Global.VBTileType.Group)
                    CreateGroup(grid, item, out splitter);
                else
                {
                    while (item.TileRow + 1 >= grid.RowDefinitions.Count())
                        InsertRow(grid, TileSize, splitter);

                    VBTile vbTile = new VBTile() { Height = TileSize, Width = TileSize };
                    grid.Children.Add(SetBinding(vbTile, item));

                    AlignTile(vbTile);
                }
            }
        }

        /// <summary>
        /// Inserts the row.
        /// </summary>
        /// <param name="grid">The grid where row will be inserted.</param>
        /// <param name="size">The row size.</param>
        /// <param name="splitter">The row splitter.</param>
        /// <param name="insertIndex">The insert index.</param>
        public static void InsertRow(Grid grid, double size, RowDefinition splitter, int insertIndex = 0)
        {
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(size);
            if (splitter != null)
            {
                grid.RowDefinitions.Insert(grid.RowDefinitions.IndexOf(splitter), rowDefinition);
                VBTile.CorrectElementsPosition(grid, grid.RowDefinitions.IndexOf(splitter) - 1);
            }
            else if(splitter == null && insertIndex > 0)
            {
                grid.RowDefinitions.Insert(insertIndex, rowDefinition);
                VBTile.CorrectElementsPosition(grid, insertIndex);
            }
            else
                grid.RowDefinitions.Add(rowDefinition);
            Border currentBorder = FindBorder(grid, grid.RowDefinitions.IndexOf(rowDefinition));
            Grid.SetRowSpan(currentBorder, Grid.GetRowSpan(currentBorder) + 1);
        }

        private void CreateGroup(Grid grid, IVBTileGrid item, out RowDefinition splitter)
        {
            RowDefinition grNameRow = new RowDefinition();
            grNameRow.Height = new GridLength(30);
            grid.RowDefinitions.Add(grNameRow);

            TextBox groupName = new TextBox();
            groupName.Text = item.Title;
            groupName.Margin = new Thickness(7, 3, 0, 0);
            groupName.Background = new SolidColorBrush(Colors.Transparent);
            if (ControlManager.WpfTheme == eWpfTheme.Gip)
                groupName.Foreground = Brushes.White;
            else
                groupName.Foreground = Brushes.Black;
            groupName.FontSize = 16;
            groupName.BorderThickness = new Thickness();
            Panel.SetZIndex(groupName, 1);

            Binding title = new Binding();
            title.Source = item;
            title.Path = new PropertyPath("Title");
            title.Mode = BindingMode.TwoWay;
            title.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            groupName.SetBinding(TextBox.TextProperty, title);

            Grid.SetRow(groupName, grid.RowDefinitions.IndexOf(grNameRow));
            Grid.SetColumn(groupName, 0);
            Grid.SetColumnSpan(groupName, Columns);
            grid.Children.Add(groupName);

            splitter = CreateSplitter(grid);

            CreateBorder(grid, grNameRow);
        }

        private VBTile SetBinding(VBTile vbTile, IVBTileGrid item)
        {
            Binding col = new Binding();
            col.Source = item;
            col.Path = new PropertyPath("TileColumn");
            col.Mode = BindingMode.TwoWay;
            vbTile.SetBinding(VBTile.ColumnProperty, col);

            Binding row = new Binding();
            row.Source = item;
            row.Path = new PropertyPath("TileRow");
            row.Mode = BindingMode.TwoWay;
            vbTile.SetBinding(VBTile.RowProperty, row);

            Binding title = new Binding();
            title.Source = item;
            title.Path = new PropertyPath("Title");
            title.Mode = BindingMode.TwoWay;
            vbTile.SetBinding(VBTile.TitleProperty, title);

            Binding acurl = new Binding();
            acurl.Source = item;
            acurl.Path = new PropertyPath(Const.ACUrlPrefix);
            acurl.Mode = BindingMode.TwoWay;
            vbTile.SetBinding(VBTile.ACUrlProperty, acurl);

            Binding iconACurl = new Binding();
            iconACurl.Source = item;
            iconACurl.Path = new PropertyPath("IconACUrl");
            vbTile.SetBinding(VBTile.IconACUrlProperty, iconACurl);

            return vbTile;
        }

        /// <summary>
        /// Creates a row splitter.
        /// </summary>
        /// <param name="grid">The grid where row splitter will be added.</param>
        /// <returns>The row splitter.</returns>
        public RowDefinition CreateSplitter(Grid grid)
        {
            RowDefinition splitter = new RowDefinition();
            splitter.Height = new GridLength(40);
            grid.RowDefinitions.Add(splitter);
            splitter.AllowDrop = true;
            TextBlock tblock = new TextBlock();
            tblock.Text = "+";
            tblock.Margin = new Thickness(10, 5, 0, 0);
            tblock.Background = new SolidColorBrush(Colors.Transparent);
            if (ControlManager.WpfTheme == eWpfTheme.Gip)
                tblock.Foreground = new SolidColorBrush(Colors.White);
            else
                tblock.Foreground = Brushes.Black;
            Grid.SetRow(tblock, grid.RowDefinitions.IndexOf(splitter));
            grid.Children.Add(tblock);

            return splitter;
        }

        private void CreateBorder(Grid grid, RowDefinition row)
        {
            Border bd = new Border();
            bd.Background = Brushes.Transparent;
            if (ControlManager.WpfTheme == eWpfTheme.Gip)
                bd.BorderBrush = Brushes.White;
            else
                bd.BorderBrush = Brushes.Black;
            bd.BorderThickness = new Thickness(1);
            bd.Margin = new Thickness(0, 0, 0, 7);
            Grid.SetRowSpan(bd, 2);
            Grid.SetColumnSpan(bd, Columns);
            Grid.SetRow(bd, grid.RowDefinitions.IndexOf(row));
            grid.Children.Add(bd);
        }

        internal static Border FindBorder(Grid grid, int row)
        {
            int minDiff = 50;
            Border currentBorder = null;
            foreach (var border in grid.Children.OfType<Border>().Where(c => Grid.GetRow(c) <= row))
            {
                if (row - Grid.GetRow(border) < minDiff)
                {
                    minDiff = row - Grid.GetRow(border);
                    currentBorder = border;
                }
            }
            return currentBorder;
        }

        /// <summary>
        /// Adds a new group in the VBTileGrid.
        /// </summary>
        [ACMethodInteraction("", "en{'Add group'}de{'Add gruppe'}", 996, false)]
        public void AddGroup()
        {
            Grid grid = this.Content as Grid;
            RowDefinition grNameRow = new RowDefinition();
            grNameRow.Height = new GridLength(30);
            grid.RowDefinitions.Add(grNameRow);

            TextBox groupName = new TextBox();
            groupName.Margin = new Thickness(7, 3, 0, 0);
            groupName.Text = "Gruppe";
            Panel.SetZIndex(groupName, 1);
            groupName.Background = Brushes.Transparent;
            if (ControlManager.WpfTheme == eWpfTheme.Gip)
                groupName.Foreground = Brushes.White;
            else
                groupName.Foreground = Brushes.Black;
            groupName.FontSize = 16;
            groupName.BorderThickness = new Thickness();
            Grid.SetRow(groupName, grid.RowDefinitions.IndexOf(grNameRow));
            Grid.SetColumn(groupName, 0);
            Grid.SetColumnSpan(groupName, grid.ColumnDefinitions.Count());
            grid.Children.Add(groupName);

            CreateSplitter(grid);

            CreateBorder(grid, grNameRow);

            if (!string.IsNullOrEmpty(OnTileAdded) && ContextACObject != null && Items != null)
            {
                VBTile helper = new VBTile() { Title = groupName.Text, Row = grid.RowDefinitions.IndexOf(grNameRow) };
                ContextACObject.ACUrlCommand(OnTileAdded, helper);
                IVBTileGrid item = Items.FirstOrDefault(c => c.TileRow == helper.TileRow && c.TileColumn == helper.TileColumn && c.ACUrl == helper.ACUrl);
                if (item != null)
                {
                    Binding bind = new Binding();
                    bind.Source = item;
                    bind.Path = new PropertyPath("Title");
                    bind.Mode = BindingMode.TwoWay;
                    bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    groupName.SetBinding(TextBox.TextProperty, bind);
                }
            }
        }

        /// <summary>
        /// Removes the group from the VBTileGrid.
        /// </summary>
        [ACMethodInteraction("", "en{'Delete group'}de{'Gruppe löschen'}", 999, false)]
        public void RemoveGroup()
        {
            if (_RowIndex > 0)
            {
                ContextACObject.ACUrlCommand(OnTileDeleted, new VBTile() { Row = _RowIndex } as IVBTileGrid);

                TextBlock tbs = ((Grid)Content).Children.OfType<TextBlock>().FirstOrDefault(c => Grid.GetRow(c) > _RowIndex && c.Text.Equals("+"));
                int splitterIndex = Grid.GetRow(tbs);
                ((Grid)Content).Children.Remove(tbs);

                List<VBTile> tempDeleteList = new List<VBTile>();
                for (int i = _RowIndex + 1; i < splitterIndex; i++)
                    tempDeleteList.AddRange(((Grid)Content).Children.OfType<VBTile>().Where(c => Grid.GetRow(c) == i));

                tempDeleteList.ForEach(c => c.DeleteTile());

                Border bd = ((Grid)Content).Children.OfType<Border>().FirstOrDefault(c => Grid.GetRow(c) == _RowIndex);
                ((Grid)Content).Children.Remove(bd);

                TextBox tb = ((Grid)Content).Children.OfType<TextBox>().FirstOrDefault(c => Grid.GetRow(c) == _RowIndex);
                BindingOperations.ClearAllBindings(tb);
                ((Grid)Content).Children.Remove(tb);

                ((Grid)Content).RowDefinitions.RemoveAt(_RowIndex);
                VBTile.CorrectElementsPosition(Content as Grid, _RowIndex, false);
                ((Grid)Content).RowDefinitions.RemoveAt(_RowIndex);
                VBTile.CorrectElementsPosition(Content as Grid, _RowIndex, false);
            }
        }

        /// <summary>
        /// Determines is enabled to remove group or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledRemoveGroup()
        {
            _RowIndex = GetGroupRow(this.Content as Grid, _helperPoint);
            if (_RowIndex > 0)
                return true;
            else
                return false;
        }

        private int GetGroupRow(Grid grid, Nullable<Point> dropPoint = null)
        {
            Point point = Mouse.GetPosition(grid);
            if (dropPoint != null && dropPoint.HasValue)
                point = dropPoint.Value;
            int row = 0;
            double accumulatedHeight = 0.0;

            foreach (var rowDefinition in grid.RowDefinitions)
            {
                accumulatedHeight += rowDefinition.ActualHeight;
                if (accumulatedHeight - 35 <= point.Y && accumulatedHeight+2 >= point.Y && rowDefinition.ActualHeight == 30)
                    return row;
                row++;
            }
            return 0;
        }

        /// <summary>
        /// Align the VBTile right, center or left according that in which column is it.
        /// </summary>
        /// <param name="vbTile">The vbTile.</param>
        public void AlignTile(VBTile vbTile)
        {
            if (vbTile.Column == 0 && this.Columns > 1)
                vbTile.HorizontalAlignment = HorizontalAlignment.Right;
            else if (vbTile.Column == VBTileGrid.Col - 1 && this.Columns > 1)
                vbTile.HorizontalAlignment = HorizontalAlignment.Left;
            else
                vbTile.HorizontalAlignment = HorizontalAlignment.Center;
        }

        #endregion

        #region IACMenuBuilderWpfTree

        /// <summary>
        /// Appends the menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        /// <param name="acMenuItemList">The list of a acMenu items.</param>
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
        /// <returns>The list of a acMenu items.</returns>
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
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty =
            DependencyProperty.Register("VBContent", typeof(string), typeof(VBTileGrid));

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
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            if (actionArgs.ElementAction == Global.ElementActionType.ACCommand)
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
            get { return ""; }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return this.ReflectACType(); }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return null; }
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
        /// Determines is enabled ACUrlCommand or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>True if is enabled, otherwise false.</returns>
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
    }
}
