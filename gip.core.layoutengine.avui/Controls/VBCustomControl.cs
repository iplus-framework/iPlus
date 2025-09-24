// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    public static class ContentPropertyHandler
    {
        /// <summary>
        /// Represents the attached property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, IACBSO>("BSOACComponent", typeof(ContentPropertyHandler), inherits: true);

        /// <summary>
        /// Represents the attached property for VBValidation.
        /// </summary>
        public static readonly AttachedProperty<string> VBValidationProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, string>("VBValidation", typeof(ContentPropertyHandler), inherits: true);

        public static readonly AttachedProperty<string> RegisterVBDecoratorProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, string>("RegisterVBDecorator", typeof(ContentPropertyHandler), inherits: true);

        /// <summary>
        /// Represents the attached property for CanExecuteCyclic.
        /// </summary>
        public static readonly AttachedProperty<int> CanExecuteCyclicProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, int>("CanExecuteCyclic", typeof(ContentPropertyHandler), defaultValue: 0, inherits: true);

        /// <summary>
        /// Represents the attached property for StringFormat.
        /// </summary>
        public static readonly AttachedProperty<string> StringFormatProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, string>("StringFormat", typeof(ContentPropertyHandler), inherits: true);

        /// <summary>
        /// Represents the attached property for DisableContextMenu.
        /// </summary>
        public static readonly AttachedProperty<bool> DisableContextMenuProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, bool>("DisableContextMenu", typeof(ContentPropertyHandler), defaultValue: false, inherits: true);

        public static void SetDisableContextMenu(Control element, Boolean value)
        {
            element.SetValue(DisableContextMenuProperty, value);
        }
        public static Boolean GetDisableContextMenu(Control element)
        {
            return element.GetValue(DisableContextMenuProperty);
        }


        /// <summary>
        /// Property to Enable Drag and Drop Behaviour.
        /// </summary>
        public static readonly AttachedProperty<DragMode> DragEnabledProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, DragMode>("DragEnabled", typeof(ContentPropertyHandler), defaultValue: DragMode.Disabled, inherits: true);


        /// <summary>
        /// Dependency property to control if animations should be switched off to save gpu/rendering performance.
        /// </summary>
        public static readonly AttachedProperty<bool> AnimationOffProperty =
            AvaloniaProperty.RegisterAttached<StyledElement, bool>("AnimationOff", typeof(ContentPropertyHandler), defaultValue: false, inherits: true);
    }

    public class VBCustomControl : Control
    {
    }
}
