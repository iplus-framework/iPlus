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
        public VBSideMenuItem()
        {

        }

        public VBSideMenuItem(IACObject acComponent, ACCommand acCommand) : base (acComponent, acCommand)
        {

        }

        private Button? _button;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _button = e.NameScope.Find<Button>("PART_Button");
            if (_button != null)
                _button.Click += _button_Click;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            if (_button != null)
                _button.Click -= _button_Click;
            _button = null;

        }

        private void _button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (this.Items.Any())
            {
                VBSideMenu sideMenu = this.FindAncestorOfType<VBSideMenu>() as VBSideMenu;
                if (sideMenu != null)
                {
                     sideMenu.OnMenuItemClicked(this);
                }
            }
            else
            {
                SplitView parentSplitView = this.FindAncestorOfType<SplitView>() as SplitView;
                if (parentSplitView != null)
                {
                    parentSplitView.IsPaneOpen = false;
                }

                base.OnClick(e);
            }
        }
    }
}
