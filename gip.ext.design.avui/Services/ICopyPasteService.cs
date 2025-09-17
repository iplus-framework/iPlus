// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

namespace gip.ext.design.avui
{
	public interface ICopyPasteService
	{
		bool CanCopy(DesignContext designContext);

		void Copy(DesignContext designContext);

		bool CanPasteAsync(DesignContext designContext);

		void Paste(DesignContext designContext);

		bool CanCut(DesignContext designContext);

		void Cut(DesignContext designContext);

		bool CanDelete(DesignContext designContext);

		void Delete(DesignContext designContext);
	}
}
