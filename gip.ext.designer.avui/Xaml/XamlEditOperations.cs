// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using gip.ext.designer.avui.Extensions;
using gip.ext.xamldom.avui;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Xaml
{
    /// <summary>
    /// Deals with operations on controls which also require access to internal XML properties of the XAML Document.
    /// </summary>
    public class XamlEditOperations
    {
        readonly XamlDesignContext _context;
        readonly XamlParserSettings _settings;


        readonly char _delimeter = Convert.ToChar(0x7F);

        /// <summary>
        /// Delimet character to seperate different piece of Xaml's
        /// </summary>
        public char Delimeter
        {
            get { return _delimeter; }
        }

        public XamlEditOperations(XamlDesignContext context, XamlParserSettings settings)
        {
            this._context = context;
            this._settings = settings;
        }

        /// <summary>
        /// Copy <paramref name="designItems"/> from the designer to clipboard.
        /// </summary>
        public void Cut(ICollection<DesignItem> designItems)
        {
            Clipboard.Clear();
            string cutXaml = "";
            var changeGroup = _context.OpenGroup("Cut " + designItems.Count + " elements", designItems);
            foreach (var item in designItems)
            {
                if (item != null && item != _context.RootItem)
                {
                    XamlDesignItem xamlItem = item as XamlDesignItem;
                    if (xamlItem != null)
                    {
                        cutXaml += XamlStaticTools.GetXaml(xamlItem.XamlObject);
                        cutXaml += _delimeter;
                    }
                }
            }
            ModelTools.DeleteComponents(designItems);
            Clipboard.SetText(cutXaml, TextDataFormat.Xaml);
            changeGroup.Commit();
        }

        /// <summary>
        /// Copy <paramref name="designItems"/> from the designer to clipboard.
        /// </summary>
        public void Copy(ICollection<DesignItem> designItems)
        {
            Clipboard.Clear();
            string copiedXaml = "";
            var changeGroup = _context.OpenGroup("Copy " + designItems.Count + " elements", designItems);
            foreach (var item in designItems)
            {
                if (item != null)
                {
                    XamlDesignItem xamlItem = item as XamlDesignItem;
                    if (xamlItem != null)
                    {
                        copiedXaml += XamlStaticTools.GetXaml(xamlItem.XamlObject);
                        copiedXaml += _delimeter;
                    }
                }
            }
            Clipboard.SetText(copiedXaml, TextDataFormat.Xaml);
            changeGroup.Commit();
        }

        /// <summary>
        /// Paste items from clipboard into the designer.
        /// </summary>
		public void Paste()
        {
            this.Paste(_context.Services.Selection.PrimarySelection);
        }

        /// <summary>
        /// Paste items from clipboard into the container.
        /// </summary>
        public void Paste(DesignItem container)
        {
            var parent = container;
            var child = container;

            bool pasted = false;
            string combinedXaml = Clipboard.GetText(TextDataFormat.Xaml);
            IEnumerable<string> xamls = combinedXaml.Split(_delimeter);
            xamls = xamls.Where(xaml => xaml != "");


            XamlDesignItem rootItem = parent.Services.DesignPanel.Context.RootItem as XamlDesignItem;
            var pastedItems = new Collection<DesignItem>();
            foreach (var xaml in xamls)
            {
                var obj = XamlParser.ParseSnippet(rootItem.XamlObject, xaml, _settings);
                if (obj != null)
                {
                    DesignItem item = ((XamlComponentService)parent.Services.Component).RegisterXamlComponentRecursive(obj);
                    if (item != null)
                        pastedItems.Add(item);
                }
            }

            if (pastedItems.Count != 0)
            {
                var changeGroup = parent.Services.DesignPanel.Context.OpenGroup("Paste " + pastedItems.Count + " elements", pastedItems);
                while (parent != null && pasted == false)
                {
                    if (parent.ContentProperty != null)
                    {
                        if (parent.ContentProperty.IsCollection)
                        {
                            if (CollectionSupport.CanCollectionAdd(parent.ContentProperty.ReturnType, pastedItems.Select(item => item.Component)) && parent.GetBehavior<IPlacementBehavior>() != null)
                            {
                                AddInParent(parent, pastedItems);
                                pasted = true;
                            }
                        }
                        else if (pastedItems.Count == 1 && parent.ContentProperty.Value == null && parent.ContentProperty.ValueOnInstance == null && parent.View is ContentControl)
                        {
                            AddInParent(parent, pastedItems);
                            pasted = true;
                        }
                        if (!pasted)
                            parent = parent.Parent;
                    }
                    else
                    {
                        parent = parent.Parent;
                    }
                }

                while (pasted == false)
                {
                    if (child.ContentProperty != null)
                    {
                        if (child.ContentProperty.IsCollection)
                        {
                            foreach (var col in child.ContentProperty.CollectionElements)
                            {
                                if (col.ContentProperty != null && col.ContentProperty.IsCollection)
                                {
                                    if (CollectionSupport.CanCollectionAdd(col.ContentProperty.ReturnType, pastedItems.Select(item => item.Component)))
                                    {
                                        pasted = true;
                                    }
                                }
                            }
                            break;
                        }
                        else if (child.ContentProperty.Value != null)
                        {
                            child = child.ContentProperty.Value;
                        }
                        else if (pastedItems.Count == 1)
                        {
                            child.ContentProperty.SetValue(pastedItems.First().Component);
                            pasted = true;
                            break;
                        }
                        else
                            break;
                    }
                    else
                        break;
                }

                foreach (var pastedItem in pastedItems)
                {
                    ((XamlComponentService)parent.Services.Component).RaiseComponentRegisteredAndAddedToContainer(pastedItem);
                }


                changeGroup.Commit();
            }
        }

        /// <summary>
        /// Adds Items under a parent given that the content property is collection and can add types of <paramref name="pastedItems"/>
        /// </summary>
        /// <param name="parent">The Parent element</param>
        /// <param name="pastedItems">The list of elements to be added</param>
        void AddInParent(DesignItem parent, IList<DesignItem> pastedItems)
        {
            IEnumerable<Rect> rects = GetRects(pastedItems);
            var operation = PlacementOperation.TryStartInsertNewComponents(parent, pastedItems, rects.ToList(), PlacementType.AddItem);
            ISelectionService selection = _context.Services.Selection;
            selection.SetSelectedComponents(pastedItems);
            if (operation != null)
                operation.Commit();
        }

        List<DesignItem> RemoveChildItemsWhenContainerIsInList(ICollection<DesignItem> designItems)
        {
            var copyList = designItems.ToList();
            foreach (var designItem in designItems)
            {
                var parent = designItem.Parent;
                while (parent != null)
                {
                    if (copyList.Contains(parent))
                    {
                        copyList.Remove(designItem);
                    }
                    parent = parent.Parent;
                }
            }

            return copyList;
        }

        IEnumerable<Rect> GetRects(IList<DesignItem> pastedItems)
        {
            List<Rect> rects = new List<Rect>();
            foreach (DesignItem i in pastedItems)
            {
                DesignItemProperty leftProp = i.Properties.GetAttachedProperty(Canvas.LeftProperty);
                DesignItemProperty topProp = i.Properties.GetAttachedProperty(Canvas.TopProperty);
                if (leftProp != null && topProp != null)
                {
                    double x = (double)leftProp.ValueOnInstance;
                    double y = (double)topProp.ValueOnInstance;
                    if (Double.IsNaN(x) || Double.IsNaN(y))
                    {
                        rects.Add(new Rect(new Point(0, 0), new Point((double)i.Properties["Width"].ValueOnInstance, (double)i.Properties["Height"].ValueOnInstance)));
                    }
                    else
                    {
                        rects.Add(new Rect(new Point(x + 5, y + 5),
                            new Size((double)i.Properties["Width"].ValueOnInstance, (double)i.Properties["Height"].ValueOnInstance)));
                    }
                }
                else
                {
                    rects.Add(new Rect(new Point(0, 0), new Point((double)i.Properties["Width"].ValueOnInstance, (double)i.Properties["Height"].ValueOnInstance)));
                }
            }
            return rects;
        }
    }
}
