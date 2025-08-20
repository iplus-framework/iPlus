// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Windows.Controls;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Special event args for image errors
    /// </summary>
    public class ImageErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the exception
        /// </summary>
        public Exception Exception { get; protected set; }

        /// <summary>
        /// Gets or sets the handled state. If handled is true the current image processing exception is 
        /// suppressed and report generation will continue.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the Image object being processed
        /// </summary>
        public Image Image { get; protected set; }

        /// <summary>
        /// Gets the associated ReportDocument
        /// </summary>
        public ReportDocument ReportDocument { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageErrorEventArgs() : this(null, null, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">exception</param>
        public ImageErrorEventArgs(Exception exception)
            : this(exception, null, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">exception</param>
        /// <param name="report">associated report document</param>
        public ImageErrorEventArgs(Exception exception, ReportDocument report)
            : this(exception, report, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">exception</param>
        /// <param name="report">associated report document</param>
        /// <param name="image">image object being processed</param>
        public ImageErrorEventArgs(Exception exception, ReportDocument report, Image image)
        {
            Exception = exception;
            Image = image;
            Handled = false;
            ReportDocument = report;
        }
    }
}
