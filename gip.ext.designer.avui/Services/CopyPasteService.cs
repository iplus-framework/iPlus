using gip.ext.design.avui;
using gip.ext.designer.avui;
using gip.ext.designer.avui.Xaml;
using gip.ext.designer.avui.Services;
using System;
using System.Windows;
using Avalonia.Controls;
using System.Threading.Tasks;

namespace gip.ext.designer.avui.Services
{
	public class CopyPasteService : ICopyPasteService
	{
		public virtual bool CanCopy(DesignContext designContext)
		{
			ISelectionService selectionService = designContext.Services.GetService<ISelectionService>();
			if (selectionService != null)
			{
				if (selectionService.SelectedItems.Count == 0)
					return false;
				if (selectionService.SelectedItems.Contains(designContext.RootItem))
					return false;
			}
			return true;
		}

		public virtual void Copy(DesignContext designContext)
		{
			XamlDesignContext xamlContext = designContext as XamlDesignContext;
			ISelectionService selectionService = designContext.Services.GetService<ISelectionService>();
			if (xamlContext != null && selectionService != null && !selectionService.SelectedItems.Contains(designContext.RootItem))
			{
				xamlContext.XamlEditAction.Copy(selectionService.SelectedItems);
			}
		}

		public virtual bool CanCut(DesignContext designContext)
		{
			return CanCopy(designContext);
		}

		public virtual void Cut(DesignContext designContext)
		{
			XamlDesignContext xamlContext = designContext as XamlDesignContext;
			ISelectionService selectionService = designContext.Services.GetService<ISelectionService>();
			if (xamlContext != null && selectionService != null)
			{
				xamlContext.XamlEditAction.Cut(selectionService.SelectedItems);
			}
		}

		public virtual bool CanDelete(DesignContext designContext)
		{
			if (designContext != null)
			{
				return ModelTools.CanDeleteComponents(designContext.Services.Selection.SelectedItems);
			}
			return false;
		}

		public virtual void Delete(DesignContext designContext)
		{
			if (designContext != null)
			{
				ModelTools.DeleteComponents(designContext.Services.Selection.SelectedItems);
			}
		}

		public virtual async Task<bool> CanPasteAsync(DesignContext designContext)
		{
			ISelectionService selectionService = designContext.Services.GetService<ISelectionService>();
			if (selectionService != null && selectionService.SelectedItems.Count != 0)
			{
				try
				{
                    string xaml = await TopLevel.GetTopLevel(designContext.RootItem.View)?.Clipboard?.GetTextAsync();
                    //string xaml = Clipboard.GetText(TextDataFormat.Xaml);
					if (xaml != "" && xaml != " ")
						return true;
				}
				catch (Exception)
				{
				}
			}
			return false;
		}

		public virtual bool CanPaste(DesignContext designContext)
		{
			ISelectionService selectionService = designContext.Services.GetService<ISelectionService>();
			if (selectionService != null && selectionService.SelectedItems.Count != 0)
			{
				try
				{
					string xaml = TopLevel.GetTopLevel(designContext.RootItem.View)?.Clipboard?.GetTextAsync().Result;
					if (xaml != "" && xaml != " ")
						return true;
				}
				catch (Exception)
				{
				}
			}
			return false;
		}

		public virtual void Paste(DesignContext designContext)
		{
			XamlDesignContext xamlContext = designContext as XamlDesignContext;
			if (xamlContext != null)
			{
				xamlContext.XamlEditAction.Paste();
			}
		}
	}
}
