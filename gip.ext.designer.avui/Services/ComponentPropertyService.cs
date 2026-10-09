// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;

namespace gip.ext.designer.avui.Services
{
	public class ComponentPropertyService : IComponentPropertyService
	{
		protected HashSet<string> IgnoreTypes = new HashSet<string>(new[]
		{
			//typeof(XmlAttributeProperties).Name,
			//typeof(Typography).Name,
			//typeof(ContextMenuService).Name,
			//typeof(DesignerProperties).Name,
			//typeof(InputLanguageManager).Name,
			typeof(InputMethod).Name,
			typeof(KeyboardNavigation).Name,
			//typeof(NumberSubstitution).Name,
			typeof(RenderOptions).Name,
			typeof(TextSearch).Name,
			//typeof(ToolTipService).Name,
			//typeof(Validation).Name,
			//typeof(Stylus).Name
		});

		public virtual IEnumerable<MemberDescriptor> GetAvailableProperties(DesignItem designItem)
		{
			// Only offer attached layout properties that match the parent container
			// (e.g. Canvas.Top only when the element is placed inside a Canvas).
			Type parentType = designItem.Parent?.ComponentType;
			return TypeHelper.GetAvailableProperties(designItem.Component, parentType: parentType)
				.Where(x => !x.Name.Contains(".") || !IgnoreTypes.Contains(x.Name.Split('.')[0]));
		}

		public virtual IEnumerable<MemberDescriptor> GetAvailableEvents(DesignItem designItem)
		{
			return TypeHelper.GetAvailableEvents(designItem.ComponentType);
		}

		public virtual IEnumerable<MemberDescriptor> GetCommonAvailableProperties(IEnumerable<DesignItem> designItems)
		{
			var items = designItems.ToList();
			// Use the parent type only if all selected items share the same parent,
			// otherwise no attached property is meaningful for the selection.
			Type parentType = null;
			var parents = items.Select(i => i.Parent).ToList();
			if (parents.Count > 0 && parents.All(p => p == parents[0]))
				parentType = parents[0]?.ComponentType;
			return TypeHelper.GetCommonAvailableProperties(items.Select(t => t.Component))
				.Where(x => !(x is TypeHelper.AttachedPropertyDescriptor attached)
						|| parentType == null
						|| IsOwnerOfParentType(attached.AvaloniaProperty.OwnerType, parentType))
				.Where(x => !x.Name.Contains(".") || !IgnoreTypes.Contains(x.Name.Split('.')[0]));
		}

		protected static bool IsOwnerOfParentType(Type ownerType, Type parentType)
		{
			for (var t = parentType; t != null; t = t.BaseType)
			{
				if (t == ownerType)
					return true;
			}
			return false;
		}
	}
}
