using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the control that allows multiple selections in combobox.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt das Steuerelement dar, das Mehrfachselektionen in der Combobox erlaubt.
    /// </summary>
    public class VBMultiSelectComboBox : MultiSelector 
    {
        private bool _SelectionLock = false;

        #region DP

        /// <summary>
        /// Represents the dependency property for IsDropDownOpen.
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty = ComboBox.IsDropDownOpenProperty.AddOwner(typeof(VBMultiSelectComboBox), new FrameworkPropertyMetadata(new PropertyChangedCallback(IsDropDownOpenChangedHandler)));

        /// <summary>
        /// Represents the dependency property for IsEditable.
        /// </summary>
        public static readonly DependencyProperty IsEditableProperty = ComboBox.IsEditableProperty.AddOwner(typeof(VBMultiSelectComboBox));

        /// <summary>
        /// Represents the dependency property for StaysOpenOnEdit.
        /// </summary>
        public static readonly DependencyProperty StaysOpenOnEditProperty = ComboBox.StaysOpenOnEditProperty.AddOwner(typeof(VBMultiSelectComboBox));

        /// <summary>
        /// Represents the dependency property for MaxDropDownHeight.
        /// </summary>
        public static readonly DependencyProperty MaxDropDownHeightProperty = ComboBox.MaxDropDownHeightProperty.AddOwner(typeof(VBMultiSelectComboBox));

        /// <summary>
        /// Represents the dependency property for MultipleSelection.
        /// </summary>
        public static readonly DependencyProperty MultipleSelectionProperty = DependencyProperty.Register("MultipleSelection", typeof(bool), typeof(VBMultiSelectComboBox), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(MultipleSelectionChangedHandler)));

        /// <summary>
        /// Represents the dependency property for Selection.
        /// </summary>
        public static readonly DependencyProperty SelectionProperty = DependencyProperty.Register("Selection", typeof(IEnumerable), typeof(VBMultiSelectComboBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(SelectionChangedHandler)));

        /// <summary>
        /// Represents the dependency property key for TextProperty.
        /// </summary>
        private static readonly DependencyPropertyKey TextPropertyKey = DependencyProperty.RegisterReadOnly("Text", typeof(string), typeof(VBMultiSelectComboBox), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Represents the dependency property for Text.
        /// </summary>
        public static readonly DependencyProperty TextProperty = TextPropertyKey.DependencyProperty;

        #endregion  

        /// <summary>
        /// Represents the event for DropDownClosed.
        /// </summary>
        public static readonly RoutedEvent DropDownClosedEvent = EventManager.RegisterRoutedEvent("DropDownClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VBMultiSelectComboBox));

        /// <summary>
        /// Represents the event for DropDownOpened.
        /// </summary>
        public static readonly RoutedEvent DropDownOpenedEvent = EventManager.RegisterRoutedEvent("DropDownOpened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VBMultiSelectComboBox));

        #region Events
        /// <summary>
        /// Adds or removes event handler for DropDownClosed.
        /// </summary>
        public event RoutedEventHandler DropDownClosed
        {
            add { AddHandler(DropDownClosedEvent, value); }
            remove { RemoveHandler(DropDownClosedEvent, value); }
        }

        /// <summary>
        /// Add or removes event handler for DropDownOpened.
        /// </summary>
        public event RoutedEventHandler DropDownOpened
        {
            add { AddHandler(DropDownOpenedEvent, value); }
            remove { RemoveHandler(DropDownOpenedEvent, value); }
        }
        #endregion

        #region Properties

        /// <summary>
        /// Determines is DropDownOpen or not.
        /// </summary>
        [Category("VBControl")]
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        /// <summary>
        /// Determines is a content editable or not.
        /// </summary>
        [Category("VBControl")]
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        /// <summary>
        /// Determines is stays open on edit or not.
        /// </summary>
        [Category("VBControl")]
        public bool StaysOpenOnEdit
        {
            get { return (bool)GetValue(StaysOpenOnEditProperty); }
            set { SetValue(StaysOpenOnEditProperty, value); }
        }

        /// <summary>
        /// Gets or sets the maximum drop down height.
        /// </summary>
        [Category("VBControl")]
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Text.
        /// </summary>
        [Category("VBControl")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
        }

        /// <summary>
        /// Determines is multiple selection or not.
        /// </summary>
        [Category("VBControl")]
        public bool MultipleSelection
        {
            get { return (bool)GetValue(MultipleSelectionProperty); }
            set { SetValue(MultipleSelectionProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Selection.
        /// </summary>
        public IEnumerable Selection
        {
            get { return (IEnumerable)GetValue(SelectionProperty); }
            set { SetValue(SelectionProperty, value); }
        }

        #endregion
        
        #region Constructors

        static VBMultiSelectComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBMultiSelectComboBox), new FrameworkPropertyMetadata(typeof(VBMultiSelectComboBox)));
        }

        /// <summary>
        /// Creates a new instance of VBMultiSelectComboBox.
        /// </summary>
        public VBMultiSelectComboBox()
        {
            base.CanSelectMultipleItems = this.MultipleSelection;
        }

        #endregion

        #region Protected

        protected void Close()
        {
            this.IsDropDownOpen = false;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is VBMultiSelectComboBoxItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new VBMultiSelectComboBoxItem();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            SetValue(TextPropertyKey, string.Join(",", (from object o in this.SelectedItems where o != null select o.ToString())));

            if (!_SelectionLock)
            {
                _SelectionLock = true;
                try
                {
                    if (this.MultipleSelection)
                    {
                        object[] items = new object[this.SelectedItems.Count];

                        this.SelectedItems.CopyTo(items, 0);
                        this.Selection = items;
                    }
                    else
                    {
                        this.Selection = new object[] { this.SelectedItem };
                    }
                }
                finally
                {
                    _SelectionLock = false;
                }
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (e.OriginalSource == this && this.IsDropDownOpen) this.Close();
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            if (Mouse.Captured != this)
            {
                if (e.OriginalSource == this)
                {
                    if (Mouse.Captured == null || this.IsDescendantOf((DependencyObject)Mouse.Captured)) this.Close();
                }
                else
                {
                    if (this.IsDescendantOf(e.OriginalSource as DependencyObject))
                    {
                        if (this.IsDropDownOpen && Mouse.Captured == null)
                        {
                            e.Handled = true;
                            Mouse.Capture(this, CaptureMode.SubTree);
                        }
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
        }

        #endregion

        #region Private

        private static void MultipleSelectionChangedHandler(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            VBMultiSelectComboBox tmp = (VBMultiSelectComboBox)sender;
            tmp.CanSelectMultipleItems = (bool)e.NewValue;
        }

        private static void IsDropDownOpenChangedHandler(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Mouse.Capture((VBMultiSelectComboBox)sender, System.Windows.Input.CaptureMode.SubTree);
            }
            else
            {
                ((VBMultiSelectComboBox)sender).ReleaseMouseCapture();
            }
        }

        private static void SelectionChangedHandler(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            VBMultiSelectComboBox tmp = (VBMultiSelectComboBox)sender;
            if (!tmp._SelectionLock)
            {
                tmp._SelectionLock = true;
                try
                {
                    if (e.NewValue == null)
                    {
                        if (tmp.MultipleSelection) tmp.SelectedItems.Clear(); else tmp.SelectedItem = null;
                    }
                    else
                    {
                        if (tmp.MultipleSelection)
                        {
                            tmp.SelectedItems.Clear();
                            foreach (object item in (IEnumerable)e.NewValue)
                            {
                                tmp.SelectedItems.Add(item);
                            }
                        }
                        else
                        {
                            foreach (object item in (IEnumerable)e.NewValue)
                            {
                                tmp.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    tmp._SelectionLock = false;
                }
            }
        }

        #endregion
    }
}
