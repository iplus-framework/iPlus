using System;
using System.Windows.Controls;
using System.Windows.Documents;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// Special event args for image processing events
    /// </summary>
    public class ImageEventArgs : EventArgs
    {
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
        public ImageEventArgs() : this(null, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="report">associated report document</param>
        public ImageEventArgs(ReportDocument report) : this(report, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="report">associated report document</param>
        /// <param name="image">Image object being processed</param>
        public ImageEventArgs(ReportDocument report, Image image)
        {
            ReportDocument = report;
            Image = image;
        }
    }
}
