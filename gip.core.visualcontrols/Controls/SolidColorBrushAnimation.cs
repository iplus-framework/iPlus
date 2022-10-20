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
using gip.core.datamodel;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace gip.core.visualcontrols
{
    public abstract class SolidColorBrushAnimationBase : AnimationTimeline
    {
        protected SolidColorBrushAnimationBase() : base()
        {
        }

        public new SolidColorBrushAnimationBase Clone()
        {
            return (SolidColorBrushAnimationBase)base.Clone();
        }

        public sealed override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (defaultOriginValue == null)
            {
                throw new ArgumentNullException("defaultOriginValue");
            }
            if (defaultDestinationValue == null)
            {
                throw new ArgumentNullException("defaultDestinationValue");
            }
            return this.GetCurrentValue((SolidColorBrush)defaultOriginValue, (SolidColorBrush)defaultDestinationValue, animationClock);
        }

        public SolidColorBrush GetCurrentValue(SolidColorBrush defaultOriginValue, SolidColorBrush defaultDestinationValue, AnimationClock animationClock)
        {
            base.ReadPreamble();
            if (animationClock == null)
            {
                throw new ArgumentNullException("animationClock");
            }
            if (animationClock.CurrentState == ClockState.Stopped)
            {
                return defaultDestinationValue;
            }
            return this.GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
        }

        protected abstract SolidColorBrush GetCurrentValueCore(SolidColorBrush defaultOriginValue, SolidColorBrush defaultDestinationValue, AnimationClock animationClock);

        // Properties
        public override Type TargetPropertyType
        {
            get { return typeof(SolidColorBrush); }
        }
    }

    public class SolidColorBrushAnimation : SolidColorBrushAnimationBase
    {
        public SolidColorBrushAnimation() : base()
        {
        }

        public SolidColorBrushAnimation(SolidColorBrush fromValue,
               SolidColorBrush toValue, Duration duration)
            : this()
        {
            this.From = fromValue;
            this.To = toValue;
            base.Duration = duration;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new SolidColorBrushAnimation();
        }

        protected override SolidColorBrush GetCurrentValueCore(SolidColorBrush defaultOriginValue, SolidColorBrush defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null)
                return From;

            SolidColorBrush brush = new SolidColorBrush();

            Color color1 = From.Color;
            Color color2 = To.Color;
            Color newColor = Color.Subtract(color2, color1);
            newColor = Color.Multiply(newColor,
                            (float)animationClock.CurrentProgress);
            newColor = Color.Add(newColor, color1);
            brush.Color = newColor;

            return brush;
        }

        private static bool ValidateValues(object value)
        {
            return true;
        }

        private static void AnimationFunction_Changed(DependencyObject d,
                            DependencyPropertyChangedEventArgs e)
        {
            SolidColorBrushAnimation animation = (SolidColorBrushAnimation)d;
        }

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(SolidColorBrush), typeof(SolidColorBrushAnimation),
            new PropertyMetadata(null, new PropertyChangedCallback(SolidColorBrushAnimation.AnimationFunction_Changed)),
            new ValidateValueCallback(SolidColorBrushAnimation.ValidateValues));

        public SolidColorBrush From
        {
            get { return (SolidColorBrush)base.GetValue(FromProperty); }
            set { base.SetValue(FromProperty, value); }
        }

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(SolidColorBrush), typeof(SolidColorBrushAnimation),
            new PropertyMetadata(null, new PropertyChangedCallback(SolidColorBrushAnimation.AnimationFunction_Changed)),
            new ValidateValueCallback(SolidColorBrushAnimation.ValidateValues));

        public SolidColorBrush To
        {
            get { return (SolidColorBrush)base.GetValue(ToProperty); }
            set { base.SetValue(ToProperty, value); }
        }
    }
}
