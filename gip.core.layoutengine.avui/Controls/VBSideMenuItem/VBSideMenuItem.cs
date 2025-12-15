using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    public class VBSideMenuItem : VBMenuItem
    {
        #region c'tors
        public VBSideMenuItem()
        {

        }

        public VBSideMenuItem(IACObject acComponent, ACCommand acCommand) : base(acComponent, acCommand)
        {

        }
        #endregion

        #region Properties

        private Button _MenuButton;

        #endregion

        #region Methods

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _MenuButton = e.NameScope.Find<Button>("PART_Button");
            if (_MenuButton != null)
                _MenuButton.Click += _button_Click;
        }

        public virtual void DeInitVBControl()
        {
            if (_MenuButton != null)
                _MenuButton.Click -= _button_Click;

            foreach (var item in this.Items)
            {
                VBSideMenuItem menuItem = item as VBSideMenuItem;
                if (menuItem != null)
                {
                    menuItem.DeInitVBControl();
                }
            }
        }

        private void _button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (this.Items.Any())
            {
                VBSideMenu sideMenu = this.FindAncestorOfType<VBSideMenu>();
                if (sideMenu != null)
                {
                    sideMenu.OnMenuItemClicked(this);
                }
            }
            else
            {
                base.OnClick(e);
            }
        }

        #endregion
    }
}
