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
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Base class for VBVisualControl.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBVisualControlBase'}de{'VBVisualControlBase'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public abstract class VBVisualControlBase : Control
    {
        #region Additional Dependency-Properties

        #region ACState

        /// <summary>
        /// Represents the dependency property for ACState.
        /// </summary>
        public static readonly DependencyProperty ACStateProperty
            = DependencyProperty.Register(Const.ACState, typeof(string), typeof(VBVisualControlBase), new PropertyMetadata(new PropertyChangedCallback(OnACStateChanged)));
        
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

        private static void OnACStateChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is VBVisualControlBase)
            {
                VBVisualControlBase control = d as VBVisualControlBase;
                control.OnACStateChanged(e);
            }
        }

        /// <summary>
        /// Invokes on ACState changed.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnACStateChanged(DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion



        public static readonly DependencyProperty AnimationOffProperty = ContentPropertyHandler.AnimationOffProperty.AddOwner(typeof(VBVisualControlBase), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
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
