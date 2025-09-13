// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using gip.ext.design.avui;

namespace gip.ext.designer.avui.OutlineView
{
	/// <summary>
	/// Description of OulineNodeNameService.
	/// </summary>
	public class OutlineNodeNameService : IOutlineNodeNameService
	{
		public OutlineNodeNameService()
		{
		}

		#region IOutlineNodeNameService implementation

		public string GetOutlineNodeName(DesignItem designItem)
		{
			if (designItem == null)
				return "";
			if (string.IsNullOrEmpty(designItem.Name)) {
					return designItem.ComponentType.Name;
				}
				return designItem.ComponentType.Name + " (" + designItem.Name + ")";
		}

		#endregion
	}
}
