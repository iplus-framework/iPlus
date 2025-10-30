using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Media;

namespace gip.core.layoutengine.avui
{
    public class FractalImageGenerator
    {
        /// <summary>
        /// Occurs when [completed].
        /// </summary>
        public event EventHandler<FractalImageCompletedEventArgs> Completed;
        /// <summary>
        /// Gets or sets the palette.
        /// </summary>
        /// <value>The palette.</value>
        private Color[] Palette { get; set; }
        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public int Width { get; set; }
        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height { get; set; }
        /// <summary>
        /// Gets or sets the UI element.
        /// </summary>
        /// <value>The UI element.</value>
        public Control Control { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FractalImageGenerator"/> class.
        /// </summary>
        public FractalImageGenerator(Control Control, int width, int height)
        {
            this.Control = Control;
            this.Width = width;
            this.Height = height;
            this.Palette = GeneratePalette();
        }

        /// <summary>
        /// Draws the specified sx.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private void Draw(object state)
        {
            GenerationState size = state as GenerationState;

            if (size == null)
                throw new InvalidOperationException();

            int[] bmap = new int[this.Width * this.Height];
            byte[,,] pixels = new byte[this.Width, this.Height, 4];

            // Creates the Bitmap we draw to
            // From here on out is just converted from the c++ version.
            double x, y, x1, y1, xx, xmin, xmax, ymin, ymax = 0.0;

            int looper, s, z = 0;
            double intigralX, intigralY = 0.0;
            xmin = size.SX; // Start x value, normally -2.1
            ymin = size.SY; // Start y value, normally -1.3
            xmax = size.FX; // Finish x value, normally 1
            ymax = size.FY; // Finish y value, normally 1.3
            intigralX = (xmax - xmin) / this.Width; // Make it fill the whole window
            intigralY = (ymax - ymin) / this.Height;
            x = xmin;

            for (s = 0; s < this.Width; s++)
            {
                y = ymin;
                for (z = 0; z < this.Height; z++)
                {
                    x1 = 0;
                    y1 = 0;
                    looper = 0;

                    while (looper < 100 && ((x1 * x1) + (y1 * y1)) < 4)
                    {
                        looper++;
                        xx = (x1 * x1) - (y1 * y1) + x;
                        y1 = 2 * x1 * y1 + y;
                        x1 = xx;
                    }

                    // Get the percent of where the looper stopped
                    double perc = looper / (100.0);
                    // Get that part of a 255 scale
                    int val = ((int)(perc * (this.Palette.Length - 1)));
                    // Use that number to set the color

                    Color px = this.Palette[val];
                    bmap[z * this.Width + s] = px.A << 24 | px.R << 16 | px.G << 8 | px.B;
                    pixels[s, z, 0] = px.B;
                    pixels[s, z, 1] = px.G;
                    pixels[s, z, 2] = px.R;
                    pixels[s, z, 3] = px.A;

                    y += intigralY;
                }

                x += intigralX;
            }

            //this.Control.Dispatcher.BeginInvoke(
            //    new Action<byte[,,]>(
            //        data =>
            //        {
            //            WriteableBitmap output = new WriteableBitmap(this.Width, this.Height, 150, 150, PixelFormats.Bgra32, null);

            //            try
            //            {
            //                output.Lock();
            //                for (int a = 0; a < this.Width; a++)
            //                {
            //                    for (int b = 0; b < this.Height; b++)
            //                    {
            //                        Int32Rect rect = new Int32Rect(a, b, 1, 1);
            //                        int stride = output.PixelWidth * output.Format.BitsPerPixel / 8;
            //                        byte[] colordata = { data[a, b, 0], data[a, b, 1], data[a, b, 2], data[a, b, 3] };
            //                        output.WritePixels(rect, colordata, stride, 0);
            //                    }
            //                }

            //                this.OnCompleted(new FractalImageCompletedEventArgs(output));
            //            }
            //            finally
            //            {
            //                output.Unlock();
            //            }

            //        }), pixels);
        }

        /// <summary>
        /// Generates the palette.
        /// </summary>
        /// <returns></returns>
        private Color[] GeneratePalette()
        {
            List<Color> colors = new List<Color>();

            /*for (int c = 0; c < 256; c++)
                colors.Add(Color.FromArgb(0xff, (byte)c, 0, 0));
            for (int c = 0; c < 256; c++)
                colors.Add(Color.FromArgb(0xff, 0xff, 0, (byte)c));
            for (int c = 255; c >= 0; c--)
                colors.Add(Color.FromArgb(0xff, (byte)c, 0, 0xff));
            for (int c = 0; c < 256; c++)
                colors.Add(Color.FromArgb(0xff, 0, (byte)c, 0xff));
            for (int c = 255; c >= 0; c--)
                colors.Add(Color.FromArgb(0xff, 0, 0xff, (byte)c));
            for (int c = 0; c < 256; c++)
                colors.Add(Color.FromArgb(0xff, 0, (byte)c, 0xff));
            for (int c = 255; c >= 0; c--)
                colors.Add(Color.FromArgb(0xff, (byte)c, 0, 0xff));
            for (int c = 0; c < 256; c++)
                colors.Add(Color.FromArgb(0xff, 0xff, 0, (byte)c));
            for (int c = 0; c < 256; c++)
                colors.Add(Color.FromArgb(0xff, (byte)c, 0, 0));*/

            for (int c = 0; c < 256; c++)
                colors.Add(Color.FromArgb(0xff, (byte)c, (byte)c, (byte)c));
            for (int c = 0; c < 256; c++)
                colors.Add(Color.FromArgb(0xff, (byte)c, 0, 0));

            var half =
                colors.Concat(
                    colors.Reverse<Color>());

            return half.Concat(half).ToArray();
        }

        /// <summary>
        /// Raises the <see cref="E:Completed"/> event.
        /// </summary>
        /// <param name="e">The instance containing the event data.</param>
        private void OnCompleted(FractalImageCompletedEventArgs e)
        {
            EventHandler<FractalImageCompletedEventArgs> handler = Completed;

            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Generates the specified sx.
        /// </summary>
        /// <param name="sx">The sx.</param>
        /// <param name="sy">The sy.</param>
        /// <param name="fx">The fx.</param>
        /// <param name="fy">The fy.</param>
        public void Generate(double sx, double sy, double fx, double fy)
        {
            ThreadPool.QueueUserWorkItem(
                new WaitCallback(Draw), new GenerationState { SX = sx, SY = sy, FX = fx, FY = fy });
        }

        /// <summary>
        /// 
        /// </summary>
        private class GenerationState
        {
            public double SX { get; set; }
            public double SY { get; set; }
            public double FX { get; set; }
            public double FY { get; set; }
        }
    }
}
