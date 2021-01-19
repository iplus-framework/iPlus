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
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.Transactions;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents the factory for animation.
    /// </summary>
    public class AnimationFactory
    {
        /// <summary>
        /// Gets a new instance of AnimationFactory.
        /// </summary>
        public static AnimationFactory Instance
        {
            get
            {
                return new AnimationFactory();
            }
        }

        /// <summary>
        /// Gets the animation.
        /// </summary>
        /// <param name="target">The target parameter.</param>
        /// <param name="to">The to parameter.</param>
        /// <param name="from">The from parameter</param>
        /// <returns>The animation.</returns>
        public Storyboard GetAnimation(DependencyObject target, double to, double from)
        {
            Storyboard story = new Storyboard();
            Storyboard.SetTargetProperty(story, new PropertyPath("(TextBlock.RenderTransform).(TranslateTransform.X)"));
            Storyboard.SetTarget(story, target);

            var doubleAnimation = new DoubleAnimationUsingKeyFrames();

            EasingDoubleKeyFrame fromFrame = new EasingDoubleKeyFrame(from);
            fromFrame.EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut };
            fromFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0));

            EasingDoubleKeyFrame toFrame = new EasingDoubleKeyFrame(to);
            toFrame.EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut };
            toFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(2000));

            doubleAnimation.KeyFrames.Add(fromFrame);
            doubleAnimation.KeyFrames.Add(toFrame);
            story.Children.Add(doubleAnimation);

            return story;
        }
    }
}
