// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using gip.ext.design.avui.PropertyGrid;

namespace gip.ext.designer.avui.Services
{
	public class ComponentPropertyService : IComponentPropertyService
	{
		protected HashSet<string> IgnoreTypes = new HashSet<string>(new[]
		{
			typeof(XmlAttributeProperties).Name,
			typeof(Typography).Name,
			typeof(ContextMenuService).Name,
			typeof(DesignerProperties).Name,
			typeof(InputLanguageManager).Name,
			typeof(InputMethod).Name,
			typeof(KeyboardNavigation).Name,
			typeof(NumberSubstitution).Name,
			typeof(RenderOptions).Name,
			typeof(TextSearch).Name,
			typeof(ToolTipService).Name,
			typeof(Validation).Name,
			typeof(Stylus).Name
		});

		public virtual IEnumerable<MemberDescriptor> GetAvailableProperties(DesignItem designItem)
		{
			return TypeHelper.GetAvailableProperties(designItem.Component)
				.Where(x => !x.Name.Contains(".") || !IgnoreTypes.Contains(x.Name.Split('.')[0]));
		}

		public virtual IEnumerable<MemberDescriptor> GetAvailableEvents(DesignItem designItem)
		{
			return TypeHelper.GetAvailableEvents(designItem.ComponentType);
		}

		public virtual IEnumerable<MemberDescriptor> GetCommonAvailableProperties(IEnumerable<DesignItem> designItems)
		{
			return TypeHelper.GetCommonAvailableProperties(designItems.Select(t => t.Component))
				.Where(x => !x.Name.Contains(".") || !IgnoreTypes.Contains(x.Name.Split('.')[0]));
		}
	}
}
