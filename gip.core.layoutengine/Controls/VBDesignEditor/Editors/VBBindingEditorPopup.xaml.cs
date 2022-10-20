// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using gip.ext.design.PropertyGrid;


namespace gip.core.layoutengine.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a popup editor for binding.
    /// </summary>
	public partial class VBBindingEditorPopup
	{
		public VBBindingEditorPopup()
		{
			InitializeComponent();
		}

        protected override void OnOpened(EventArgs e)
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
                        (PopupContent.Content as VBBindingEditor).InitEditor(Property.ValueItem);
                    }
                    else
                    {
                        (PopupContent.Content as VBMultiBindingEditor).InitEditor(Property.ValueItem);
                    }
                }
            }
            else
            {
                PopupContent.Content = null;
            }
            base.OnOpened(e);
        }

		protected override void OnClosed(EventArgs e)
		{
		    base.OnClosed(e);
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

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape) IsOpen = false;
		}
	}
}
