using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a control icon with content of any type.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Steuersymbol mit beliebigen Inhalten dar.
    /// </summary>
    public class VBContentControlIcon : ContentControl, IVBDynamicIcon
    {
        public VBContentControlIcon() : base()
        {
        }

        /// <summary>
        /// Represents the dependency property for ContentStroke.
        /// </summary>
        public static readonly StyledProperty<IBrush> ContentStrokeProperty = AvaloniaProperty.Register<VBContentControlIcon, IBrush>(nameof(ContentStroke));

        /// <summary>
        /// Gets or sets the stroke of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Strich des Inhalts.
        /// </summary>
        [Category("VBControl")]
        public IBrush ContentStroke
        {
            get { return (Brush)GetValue(ContentStrokeProperty); }
            set { SetValue(ContentStrokeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ContentFill.
        /// </summary>
        public static readonly StyledProperty<IBrush> ContentFillProperty = AvaloniaProperty.Register<VBContentControlIcon, IBrush>(nameof(ContentFill));

        /// <summary>
        /// Gets or sets the fill of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Füllung des Inhalts.
        /// </summary>
        [Category("VBControl")]
        public IBrush ContentFill
        {
            get { return (Brush)GetValue(ContentFillProperty); }
            set { SetValue(ContentFillProperty, value); }
        }

    }
}
