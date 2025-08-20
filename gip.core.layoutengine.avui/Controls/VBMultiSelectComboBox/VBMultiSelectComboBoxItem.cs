using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a selectable item in <see cref="VBMultiSelectComboBox"/>.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein auswählbares Element in der VBMultiSelectComboBox dar.
    /// </summary>
    public class VBMultiSelectComboBoxItem : ComboBoxItem
    {
        #region Properties

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        public VBMultiSelectComboBox Host
        {
            get { return (VBMultiSelectComboBox)ItemsControl.ItemsControlFromItemContainer(this); }
        }

        #endregion

        #region Constructors

        static VBMultiSelectComboBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBMultiSelectComboBoxItem), new FrameworkPropertyMetadata(typeof(VBMultiSelectComboBoxItem)));
        }

        #endregion

        #region Protected

        /// <summary>
        /// Handles the OnMouseLeftButtonDown event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                if (this.Host.MultipleSelection)
                    this.IsSelected = !this.IsSelected;
                else
                    this.IsSelected = true;
            }
            base.OnMouseLeftButtonDown(e);
        }

        #endregion
    }
}
