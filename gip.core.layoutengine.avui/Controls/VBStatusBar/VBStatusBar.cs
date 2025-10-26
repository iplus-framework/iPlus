using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using StatusBar.Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui
{

    //[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(StatusBarItem))]
    public class StatusBar : ItemsControl
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructors

        static StatusBar()
        {
            ItemsPanelTemplate template = new ItemsPanelTemplate() { Content = new DockPanel() };
            //template.Build();
            ItemsPanelProperty.OverrideMetadata(typeof(StatusBar), new StyledPropertyMetadata<ITemplate<Panel>>(template));
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Protected Methods
        //
        //-------------------------------------------------------------------

        #region Protected Methods

        //private object _currentItem;

        ///// <summary>
        ///// Return true if the item is (or is eligible to be) its own ItemUI
        ///// </summary>
        //protected override bool IsItemItsOwnContainerOverride(object item)
        //{
        //    bool ret = (item is StatusBarItem) || (item is Separator);
        //    if (!ret)
        //    {
        //        _currentItem = item;
        //    }

        //    return ret;
        //}

        ////protected override void ContainerForItemPreparedOverride(Control container, object item, int index)
        ////{
        ////    base.ContainerForItemPreparedOverride(container, item, index);
        ////}

        //protected override Control CreateContainerForItemOverride()
        //{
        //    object currentItem = _currentItem;
        //    _currentItem = null;

        //    if (UsesItemContainerTemplate)
        //    {
        //        DataTemplate itemContainerTemplate = ItemContainerTemplateSelector.SelectTemplate(currentItem, this);
        //        if (itemContainerTemplate != null)
        //        {
        //            object itemContainer = itemContainerTemplate.LoadContent();
        //            if (itemContainer is StatusBarItem || itemContainer is Separator)
        //            {
        //                return itemContainer as AvaloniaObject;
        //            }
        //            else
        //            {
        //                throw new InvalidOperationException(SR.Format(SR.InvalidItemContainer, this.GetType().Name, nameof(StatusBarItem), nameof(Separator), itemContainer));
        //            }
        //        }
        //    }

        //    return new StatusBarItem();
        //}

        ///// <summary>
        ///// Prepare the element to display the item.  This may involve
        ///// applying styles, setting bindings, etc.
        ///// </summary>
        //protected override void PrepareContainerForItemOverride(AvaloniaObject element, object item)
        //{
        //    base.PrepareContainerForItemOverride(element, item);

        //    Separator separator = element as Separator;
        //    if (separator != null)
        //    {
        //        bool hasModifiers;
        //        BaseValueSourceInternal vs = separator.GetValueSource(StyleProperty, null, out hasModifiers);
        //        if (vs <= BaseValueSourceInternal.ImplicitReference)
        //            separator.SetResourceReference(StyleProperty, SeparatorStyleKey);
        //        separator.DefaultStyleKey = SeparatorStyleKey;
        //    }
        //}

        ///// <summary>
        ///// Determine whether the ItemContainerStyle/StyleSelector should apply to the container
        ///// </summary>
        ///// <returns>false if item is a Separator, otherwise return true</returns>
        //protected override bool ShouldApplyItemContainerStyle(AvaloniaObject container, object item)
        //{
        //    if (item is Separator)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return base.ShouldApplyItemContainerStyle(container, item);
        //    }
        //}

        #endregion

    }


    public class VBStatusBar : StatusBar
    {
        public VBStatusBar() : base()
        {
        }
    }
}
