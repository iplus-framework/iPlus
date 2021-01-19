using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    public static class ContentPropertyHandler
    {
        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty
            = DependencyProperty.RegisterAttached("BSOACComponent", typeof(IACBSO), typeof(ContentPropertyHandler), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Represents the dependency property for VBValidation.
        /// </summary>
        public static readonly DependencyProperty VBValidationProperty
            = DependencyProperty.RegisterAttached("VBValidation", typeof(string), typeof(ContentPropertyHandler), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty RegisterVBDecoratorProperty
            = DependencyProperty.RegisterAttached("RegisterVBDecorator", typeof(string), typeof(ContentPropertyHandler), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Represents the dependency property for CanExecuteCyclic.
        /// </summary>
        public static readonly DependencyProperty CanExecuteCyclicProperty
            = DependencyProperty.RegisterAttached("CanExecuteCyclic", typeof(int), typeof(ContentPropertyHandler), new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly DependencyProperty StringFormatProperty
            = DependencyProperty.RegisterAttached("StringFormat", typeof(string), typeof(ContentPropertyHandler), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly DependencyProperty DisableContextMenuProperty
            = DependencyProperty.RegisterAttached("DisableContextMenu", typeof(bool), typeof(ContentPropertyHandler), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetDisableContextMenu(UIElement element, Boolean value)
        {
            element.SetValue(DisableContextMenuProperty, value);
        }
        public static Boolean GetDisableContextMenu(UIElement element)
        {
            return (Boolean)element.GetValue(DisableContextMenuProperty);
        }

        //private static DependencyProperty _BSOACComponentProperty;
        //public static DependencyProperty BSOACComponentProperty
        //{
        //    get
        //    {
        //        if (_BSOACComponentProperty == null)
        //            _BSOACComponentProperty = DependencyProperty.RegisterAttached("BSOACComponent", typeof(IACBSO), typeof(VBCustomControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        //        return _BSOACComponentProperty;
        //    }
        //}

        //private static DependencyProperty _VBValidationProperty;
        //public static DependencyProperty VBValidationProperty
        //{
        //    get
        //    {
        //        if (_VBValidationProperty == null)
        //            _VBValidationProperty = DependencyProperty.RegisterAttached("VBValidation", typeof(string), typeof(VBCustomControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        //        return _VBValidationProperty;
        //    }
        //}

        //private static DependencyProperty _CanExecuteCyclicProperty;
        //public static DependencyProperty CanExecuteCyclicProperty
        //{
        //    get
        //    {
        //        if (_CanExecuteCyclicProperty == null)
        //            _CanExecuteCyclicProperty = DependencyProperty.RegisterAttached("CanExecuteCyclic", typeof(int), typeof(VBCustomControl), new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.None));
        //        return _CanExecuteCyclicProperty;
        //    }
        //}

        //private static DependencyProperty _StringFormatProperty;
        //public static DependencyProperty StringFormatProperty
        //{
        //    get
        //    {
        //        if (_StringFormatProperty == null)
        //            _StringFormatProperty = DependencyProperty.RegisterAttached("StringFormat", typeof(string), typeof(VBCustomControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        //        return _StringFormatProperty;
        //    }
        //}

        //private static DependencyProperty _DisableContextMenuProperty;
        //public static DependencyProperty DisableContextMenuProperty
        //{
        //    get
        //    {
        //        if (_DisableContextMenuProperty == null)
        //            _DisableContextMenuProperty = DependencyProperty.RegisterAttached("DisableContextMenu", typeof(bool), typeof(VBCustomControl), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));
        //        return _DisableContextMenuProperty;
        //    }
        //}
    }

    public class VBCustomControl : FrameworkElement
    {
    }
}
