using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace gip.core.layoutengine.avui
{
    public class FractalImageCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <value>The image.</value>
        public ImageSource Image { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FractalImageCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        public FractalImageCompletedEventArgs(ImageSource image)
        {
            this.Image = image;
        }
    }
}
