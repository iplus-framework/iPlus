// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Diagnostics;
using System.Windows;
using gip.ext.design.avui.Adorners;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// Resize thumb that automatically disappears if the adornered element is too small.
	/// </summary>
	public class ResizeThumb : DesignerThumb
	{
		bool checkWidth, checkHeight;

		public ResizeThumb(bool checkWidth, bool checkHeight)
		{
			Debug.Assert((checkWidth && checkHeight) == false);
			this.checkWidth = checkWidth;
			this.checkHeight = checkHeight;
		}

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			AdornerPanel parent = this.Parent as AdornerPanel;
			if (parent != null && parent.AdornedElement != null)
			{
				if (checkWidth)
					this.ThumbVisible = PlacementOperation.GetRealElementSize(parent.AdornedElement).Width > 14;
				else if (checkHeight)
					this.ThumbVisible = PlacementOperation.GetRealElementSize(parent.AdornedElement).Height > 14;
			}
			return base.ArrangeOverride(arrangeBounds);
		}
	}

    /// <summary>
    /// Resize thumb that automatically disappears if the adornered element is too small.
    /// </summary>
    sealed class ResizeThumbImpl : ResizeThumb
    {
        bool checkWidth, checkHeight;

        internal ResizeThumbImpl(bool checkWidth, bool checkHeight)
        {
            Debug.Assert((checkWidth && checkHeight) == false);
            this.checkWidth = checkWidth;
            this.checkHeight = checkHeight;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            AdornerPanel parent = this.Parent as AdornerPanel;
            if (parent != null && parent.AdornedElement != null)
            {
                if (checkWidth)
                    this.ResizeThumbVisible = parent.AdornedElement.RenderSize.Width > 14;
                else if (checkHeight)
                    this.ResizeThumbVisible = parent.AdornedElement.RenderSize.Height > 14;
            }
            return base.ArrangeOverride(arrangeBounds);
        }
    }
}
