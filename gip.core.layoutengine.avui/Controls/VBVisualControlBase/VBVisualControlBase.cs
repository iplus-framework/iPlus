using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Base class for VBVisualControl.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBVisualControlBase'}de{'VBVisualControlBase'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public abstract class VBVisualControlBase : TemplatedControl
    {
        #region Additional Dependency-Properties

        #region ACState

        /// <summary>
        /// Represents the dependency property for ACState.
        /// </summary>
        /// 
        public static readonly StyledProperty<string> ACStateProperty = AvaloniaProperty.Register<VBVisualControlBase, string>(nameof(ACState));

        /// <summary>
        /// Gets or sets the ACState.
        /// </summary>
        [Category("Appearance")]
        [Bindable(true)]
        public string ACState
        {
            get { return (string)GetValue(ACStateProperty); }
            set { SetValue(ACStateProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ACStateProperty)
                OnACStateChanged(change);
        }

        /// <summary>
        /// Invokes on ACState changed.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnACStateChanged(AvaloniaPropertyChangedEventArgs e)
        {
        }
        #endregion



        public static readonly StyledProperty<bool> AnimationOffProperty = AvaloniaProperty.Register<VBVisualControlBase, bool>(nameof(AnimationOff));

        /// <summary>
        /// Dependency property to control if animations should be switched off to save gpu/rendering performance.
        /// </summary>
        [Category("VBControl")]
        public bool AnimationOff
        {
            get { return (bool)GetValue(AnimationOffProperty); }
            set { SetValue(AnimationOffProperty, value); }
        }


        #endregion

    }
}
