using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Avalonia.Controls;
using Avalonia;

namespace gip.core.layoutengine.avui
{
    public class FractalImagePresenter
    {
        private const double DefaultXS = -10;
        private const double DefaultYS = -8;
        private const double DefaultXE = 6;
        private const double DefaultYE = 7;

        private const int DefaultImageWidth = 640;
        private const int DefaultImageHeight = 480;

        public FractalImagePresenter(Window vbMasterPage)
        {
            this.Randomizer = new Random(DateTime.Now.Millisecond);
            this.Generator = new FractalImageGenerator(vbMasterPage, (int)DefaultImageWidth, (int)DefaultImageHeight);
        }

        /// <summary>
        /// Gets or sets the current XS.
        /// </summary>
        /// <value>The current XS.</value>
        private double CurrentXS { get; set; }
        /// <summary>
        /// Gets or sets the current YS.
        /// </summary>
        /// <value>The current YS.</value>
        private double CurrentYS { get; set; }
        /// <summary>
        /// Gets or sets the current XE.
        /// </summary>
        /// <value>The current XE.</value>
        private double CurrentXE { get; set; }
        /// <summary>
        /// Gets or sets the current YE.
        /// </summary>
        /// <value>The current YE.</value>
        private double CurrentYE { get; set; }
        /// <summary>
        /// Gets or sets the start point.
        /// </summary>
        /// <value>The start point.</value>
        public Point? StartPoint { get; set; }
        /// <summary>
        /// Gets or sets the generator.
        /// </summary>
        /// <value>The generator.</value>
        public FractalImageGenerator Generator { get; set; }
        /// <summary>
        /// Gets or sets the randomizer.
        /// </summary>
        /// <value>The randomizer.</value>
        public Random Randomizer { get; set; }


        /// <summary>
        /// Randomizes this instance.
        /// </summary>
        public void Randomize()
        {
            this.CurrentXS = FractalImagePresenter.DefaultXS;
            this.CurrentYS = FractalImagePresenter.DefaultYS;
            this.CurrentXE = FractalImagePresenter.DefaultXE;
            this.CurrentYE = FractalImagePresenter.DefaultYE;

            int xs = this.Randomizer.Next(15, (int)DefaultImageWidth);
            int ys = this.Randomizer.Next(15, (int)DefaultImageHeight);
            int w = this.Randomizer.Next(3, 100);
            int h = (int)(w / (DefaultImageWidth / DefaultImageHeight));

            this.Redraw(xs, ys, xs + w, ys + h);
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset()
        {
            this.CurrentXS = FractalImagePresenter.DefaultXS;
            this.CurrentYS = FractalImagePresenter.DefaultYS;
            this.CurrentXE = FractalImagePresenter.DefaultXE;
            this.CurrentYE = FractalImagePresenter.DefaultYE;
            this.Redraw(0, 0, DefaultImageWidth, DefaultImageHeight);
        }

        /// <summary>
        /// Redraws the specified xs.
        /// </summary>
        /// <param name="xs">The xs.</param>
        /// <param name="ys">The ys.</param>
        /// <param name="xe">The xe.</param>
        /// <param name="ye">The ye.</param>
        private void Redraw(double xs, double ys, double xe, double ye)
        {
            //this.SetEnabled(false);

            double w = this.CurrentXE - this.CurrentXS;
            double h = this.CurrentYE - this.CurrentYS;

            double xsp = (xs * 100 / DefaultImageWidth);
            double cxs = (w / 100 * xsp) + this.CurrentXS;
            double xep = (xe * 100 / DefaultImageWidth);
            double cxe = (w / 100 * xep) + this.CurrentXS;
            double ysp = (ys * 100 / DefaultImageHeight);
            double cys = (h / 100 * ysp) + this.CurrentYS;
            double yep = (ye * 100 / DefaultImageHeight);
            double cye = (h / 100 * yep) + this.CurrentYS;

            this.CurrentXS = cxs;
            this.CurrentXE = cxe;
            this.CurrentYS = cys;
            this.CurrentYE = cye;

            this.Generator.Generate(this.CurrentXS, this.CurrentYS, this.CurrentXE, this.CurrentYE);
        }


    }
}
