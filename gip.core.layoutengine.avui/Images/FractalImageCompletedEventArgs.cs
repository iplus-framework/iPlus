using Avalonia.Media;
using System;
using System.Net;

namespace gip.core.layoutengine.avui
{
    public class FractalImageCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <value>The image.</value>
        public IImage Image { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FractalImageCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        public FractalImageCompletedEventArgs(IImage image)
        {
            this.Image = image;
        }
    }
}
