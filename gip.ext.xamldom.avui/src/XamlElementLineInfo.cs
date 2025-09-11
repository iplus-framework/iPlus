// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

namespace gip.ext.xamldom.avui
{

    public class XamlElementLineInfo
	{
		public XamlElementLineInfo(int lineNumber, int linePosition)
		{
			this.LineNumber = lineNumber;
			this.LinePosition = linePosition;
		}

		public int LineNumber { get; set; }
		public int LinePosition { get; set; }

		public int Position { get; set; }
		public int Length { get; set; }
	}
}
