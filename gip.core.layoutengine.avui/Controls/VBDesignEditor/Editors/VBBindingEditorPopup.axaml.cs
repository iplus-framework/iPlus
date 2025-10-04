// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using gip.ext.design.avui.PropertyGrid;
using System;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a popup editor for binding.
    /// </summary>
	public partial class VBBindingEditorPopup : Popup
    {
		public VBBindingEditorPopup()
		{
			InitializeComponent();
            Opened += VBBindingEditorPopup_Opened;
            Closed += VBBindingEditorPopup_Closed;
            KeyDown += VBBindingEditorPopup_KeyDown;
        }

        private void VBBindingEditorPopup_Opened(object sender, EventArgs e)
        {
            //this.Child;
            if (Property != null)
            {
                if (PopupContent.Content == null)
                {
                    if (Property.ValueItem != null)
                    {
                        if (typeof(Binding).IsAssignableFrom(Property.ValueItem.ComponentType)
                            || typeof(VBBindingExt).IsAssignableFrom(Property.ValueItem.ComponentType))
                        {
                            PopupContent.Content = new VBBindingEditor();
                        }
                        else if (typeof(MultiBinding).IsAssignableFrom(Property.ValueItem.ComponentType))
                        {
                            PopupContent.Content = new VBMultiBindingEditor();
                        }
                        else
                            PopupContent.Content = null;
                    }
                    else
                        PopupContent.Content = null;
                }
                else if (PopupContent.Content != null)
                {
                    if (Property.ValueItem != null)
                    {
                        if ((typeof(Binding).IsAssignableFrom(Property.ValueItem.ComponentType)
                            || typeof(VBBindingExt).IsAssignableFrom(Property.ValueItem.ComponentType))
                            && !(PopupContent.Content is VBBindingEditor))
                        {
                            PopupContent.Content = new VBBindingEditor();
                        }
                        else if (typeof(MultiBinding).IsAssignableFrom(Property.ValueItem.ComponentType)
                                && !(PopupContent.Content is VBMultiBindingEditor))
                        {
                            PopupContent.Content = new VBMultiBindingEditor();
                        }
                        else
                            PopupContent.Content = null;
                    }
                    else
                        PopupContent.Content = null;
                }

                if (PopupContent.Content != null)
                {
                    if (PopupContent.Content is VBBindingEditor)
                    {
                        (PopupContent.Content as VBBindingEditor).LoadItemsCollection(Property.ValueItem);
                    }
                    else
                    {
                        (PopupContent.Content as VBMultiBindingEditor).LoadItemsCollection(Property.ValueItem);
                    }
                }
            }
            else
            {
                PopupContent.Content = null;
            }
            //base.OnOpened(e);
        }

        private void VBBindingEditorPopup_Closed(object sender, EventArgs e)
        {
		    //base.OnClosed(e);
		    //VBBindingEditorView.VBBindingEditor.Commit();
		}

        public IPropertyNode Property
        {
            get
            {
                return DataContext as IPropertyNode;
            }
            set
            {
                DataContext = value;
            }
        }

        private void VBBindingEditorPopup_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
			if (e.Key == Key.Escape) 
                IsOpen = false;
		}
	}
}
