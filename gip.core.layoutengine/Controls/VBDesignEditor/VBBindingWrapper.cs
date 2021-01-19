using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using gip.ext.designer.OutlineView;
using gip.ext.design;
using gip.ext.design.PropertyGrid;
using gip.ext.designer.PropertyGrid;
using System.Linq;
using System.Windows.Markup;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a wrapper for single binding editor.
    /// </summary>
    public class VBBindingEditorWrapperSingle : BindingEditorWrapperSingle
    {
        public VBBindingEditorWrapperSingle(DesignItem designObjectBinding, VBBindingEditorWrapperMulti parentMultiWrapper)
            : base(designObjectBinding, parentMultiWrapper)
        {
        }

        protected override PropertyNode CreatePropertyNode()
        {
            return new VBPropertyNode();
        }

        public override string Description
        {
            get
            {
                string result = Path.ValueString + ElementName.ValueString + Source.ValueString;
                if (VBContent != null)
                {
                    if (!String.IsNullOrEmpty(VBContent.ValueString))
                        result = VBContent.ValueString;
                    else
                        result = "VBBinding";
                }
                else if (String.IsNullOrEmpty(result))
                    result = "---";
                return result;
            }
        }

        protected override void node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == Const.Value) && ((sender == Path) || (sender == ElementName) || (sender == Source) || (sender == VBContent)))
            {
                RaisePropertyChanged("Description");
            }
        }

        public PropertyNode VBContent
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "VBContent").FirstOrDefault();
            }
        }

    }

    /// <summary>
    /// Represents a wrapper for multi binding editor.
    /// </summary>
    public class VBBindingEditorWrapperMulti : BindingEditorWrapperMulti
    {
        public VBBindingEditorWrapperMulti(DesignItem designObjectBinding) 
            : base(designObjectBinding)
        {
        }

        protected override PropertyNode CreatePropertyNode()
        {
            return new VBPropertyNode();
        }

        protected override BindingEditorWrapperSingle CreateBindingWrapper(DesignItem bindingChild)
        {
            if ((bindingChild.Component is Binding) || (bindingChild.Component is VBBindingExt))
                return new VBBindingEditorWrapperSingle(bindingChild,this);
            return null;
        }

        public override MarkupExtension CreateNewBinding()
        {
            return new VBBinding();
        }
    }
}
