using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using gip.core.layoutengine.avui;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia;
using Avalonia.VisualTree;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents an object that specifies the layout of row data in tree view.
    /// </summary>
    public class VBTreeGridViewRowPresenter : GridViewRowPresenter
    {
        #region c'tors
        /// <summary>
        /// Creates a new instance of VBTreeGridViewRowPresenter.
        /// </summary>
        public VBTreeGridViewRowPresenter()
        {
            _Childs = new AvaloniaList<Control>(this, this);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Represents the dependency property for the FirstColumnIndent.
        /// </summary>
        public static readonly StyledProperty<Double> FirstColumnIndentProperty = AvaloniaProperty.Register<VBTreeGridViewRowPresenter, Double>(nameof(FirstColumnIndent), 0d);

        /// <summary>
        /// Gets or sets the FirstColumn indent.
        /// </summary>
        public Double FirstColumnIndent
        {
            get { return (Double)this.GetValue(FirstColumnIndentProperty); }
            set { this.SetValue(FirstColumnIndentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the Expander.
        /// </summary>
        public static readonly StyledProperty<Control> ExpanderProperty = AvaloniaProperty.Register<VBTreeGridViewRowPresenter, Control>(nameof(Expander));

        /// <summary>
        /// Gets or sets the Expander.
        /// </summary>
        public Control Expander
        {
            get { return (Control)this.GetValue(ExpanderProperty); }
            set { this.SetValue(ExpanderProperty, value); }
        }

        private static PropertyInfo ActualIndexProperty = typeof(GridViewColumn).GetProperty(nameof(GridViewColumn.ActualIndex), BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo DesiredWidthProperty = typeof(GridViewColumn).GetProperty(nameof(GridViewColumn.DesiredWidth), BindingFlags.NonPublic | BindingFlags.Instance);

        private AvaloniaList<Control> _Childs;
        #endregion

        #region Methods

        /// <summary>
        /// Handles the ArrageOverride.
        /// </summary>
        /// <param name="arrangeSize">The arrange size.</param>
        /// <returns>The arrange size.</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Size s = base.ArrangeOverride(arrangeSize);

            if (this.Columns == null || this.Columns.Count == 0) return s;
            Control expander = this.Expander;

            double current = 0;
            double max = arrangeSize.Width;
            for (int x = 0; x < this.Columns.Count; x++)
            {
                GridViewColumn column = this.Columns[x];

                // Actual index needed for column reorder
                Control uiColumn = GetVisualChild((int)ActualIndexProperty.GetValue(column, null)) as Control;

                // Compute column width
                double w = Math.Min(max, (Double.IsNaN(column.Width)) ? (double)DesiredWidthProperty.GetValue(column, null) : column.Width);

                // First column indent
                if (x == 0 && expander != null)
                {
                    double indent = FirstColumnIndent + expander.DesiredSize.Width;
                    uiColumn.Arrange(new Rect(current + indent, 0, Math.Max(0, w - indent), arrangeSize.Height));
                }
                else
                {
                    uiColumn.Arrange(new Rect(current, 0, w, arrangeSize.Height));
                }
                max -= w;
                current += w;
            }

            // Show expander
            if (expander != null)
            {
                expander.Arrange(new Rect(this.FirstColumnIndent, 0, expander.DesiredSize.Width, expander.DesiredSize.Height));
            }
            
            return s;
        }

        /// <summary>
        /// Hanles the MeasureOverride.
        /// </summary>
        /// <param name="constraint">The constraint size.</param>
        /// <returns>The calculated size.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size s = base.MeasureOverride(constraint);

            // Measure expander
            Control expander = this.Expander;
            if (expander != null)
            {
                // Compute max measure
                expander.Measure(constraint);
                s = new Size(Math.Max(s.Width, expander.DesiredSize.Width),
                             Math.Max(s.Height, expander.DesiredSize.Height));
            }
            return s;
        }

        /// <summary>
        /// Gets the Visual child according index.
        /// </summary>
        /// <param name="index">The index parameter.</param>
        /// <returns>The visual child.</returns>
        protected Visual GetVisualChild(int index)
        {
            // Last element is always the expander
            // called by render engine
            if (index < base.VisualChildren.Count)
                return VisualChildren[index];
            else
                return this.Expander;
        }

        ///// <summary>
        ///// Gets the visual children count.
        ///// </summary>
        //protected override int VisualChildrenCount
        //{
        //    get
        //    {
        //        // Last element is always the expander
        //        if (this.Expander != null)
        //            return base.VisualChildrenCount + 1;
        //        else
        //            return base.VisualChildrenCount;
        //    }
        //}

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ExpanderProperty)
            {
                this._Childs.Remove(change.OldValue as Control);
                this._Childs.Add((Control)change.NewValue);
            }
            base.OnPropertyChanged(change);
        }

        #endregion

    }
}
