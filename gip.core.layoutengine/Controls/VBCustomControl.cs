// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
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


        /// <summary>
        /// Property to Enable Drag and Drop Behaviour.
        /// </summary>
        public static readonly DependencyProperty DragEnabledProperty
            = DependencyProperty.RegisterAttached("DragEnabled", typeof(DragMode), typeof(ContentPropertyHandler), new FrameworkPropertyMetadata(DragMode.Disabled, FrameworkPropertyMetadataOptions.Inherits));


        /// <summary>
        /// Dependency property to control if animations should be switched off to save gpu/rendering performance.
        /// </summary>
        public static readonly DependencyProperty AnimationOffProperty
            = DependencyProperty.RegisterAttached("AnimationOff", typeof(bool), typeof(ContentPropertyHandler), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));
    }

    public class VBCustomControl : FrameworkElement
    {
    }
}
