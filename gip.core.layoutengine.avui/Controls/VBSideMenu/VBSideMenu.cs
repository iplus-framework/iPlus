using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    public class VBSideMenu : Menu
    {
        #region c'tors

        public VBSideMenu() : base()
        {

        }

        #endregion

        #region Properties

        private ItemsControl _itemsControl;
        private Button _backButton;
        private TextBlock _titleText;

        public VBSideMenuItem CurrentItem
        {
            get;
            set;
        }

        public static readonly StyledProperty<bool> IsBackVisibleProperty =
            AvaloniaProperty.Register<VBSideMenu, bool>(nameof(IsBackVisible), false);

        public bool IsBackVisible
        {
            get => GetValue(IsBackVisibleProperty);
            set => SetValue(IsBackVisibleProperty, value);
        }

        public static readonly StyledProperty<string> CurrentTitleProperty =
            AvaloniaProperty.Register<VBSideMenu, string>(nameof(CurrentTitle), "Menu");

        public string CurrentTitle
        {
            get => GetValue(CurrentTitleProperty);
            set => SetValue(CurrentTitleProperty, value);
        }

        #endregion

        #region Methods

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _itemsControl = e.NameScope.Find<ItemsControl>("PART_ItemsControl");
            _backButton = e.NameScope.Find<Button>("PART_BackButton");
            _titleText = e.NameScope.Find<TextBlock>("PART_Title");

            if (_backButton != null)
            {
                _backButton.Click += OnBackClick;
            }

            InitializeMenu();
        }

        private void InitializeMenu()
        {
            CurrentItem = Items.FirstOrDefault() as VBSideMenuItem;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_itemsControl == null || CurrentItem == null) 
                return;

            _itemsControl.ItemsSource = CurrentItem.Items;

            CurrentTitle = CurrentItem != null && CurrentItem.Header != null ? CurrentItem.Header.ToString() : "Menu";
            IsBackVisible = CurrentItem != null && CurrentItem.Parent is VBSideMenuItem;
        }

        internal void OnMenuItemClicked(VBSideMenuItem menuItem)
        {
            if (menuItem != null)
            {
                CurrentItem = menuItem;
                UpdateDisplay();
            }
        }

        private void OnBackClick(object? sender, RoutedEventArgs e)
        {
            if (CurrentItem != null)
            {
                VBSideMenuItem parentItem = CurrentItem.Parent as VBSideMenuItem;
                if (parentItem != null)
                {
                    CurrentItem = parentItem;
                    UpdateDisplay();
                }
            }
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);

            if (_backButton != null)
            {
                _backButton.Click -= OnBackClick;
            }

            foreach (var item in Items)
            {
                VBSideMenuItem menuItem = item as VBSideMenuItem;
                if (menuItem != null)
                {
                    menuItem.DeInitVBControl();
                }
            }
        }

        #endregion
    }
}