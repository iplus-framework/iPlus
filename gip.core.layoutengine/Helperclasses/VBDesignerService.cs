using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.core.datamodel;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Markup;

namespace gip.core.layoutengine.Helperclasses
{
    public class VBDesignerService : IVBDesignerService
    {
        public Msg GetPresenterElements(out List<string> result, string xaml)
        {
            result = new List<string>();

            if (string.IsNullOrEmpty(xaml))
                return null;

            try
            {
                Canvas root = XamlReader.Parse(xaml) as Canvas;
                if (root != null)
                    GetElements(root, result);
            }
            catch (Exception e)
            {
                return new Msg(eMsgLevel.Exception, e.Message);
            }
            result = result.Where(c => !string.IsNullOrEmpty(c)).ToList();
            return null;
        }

        private void GetElements(FrameworkElement item, List<string> results)
        {
            if (item == null)
                return;

            results.Add(item.Name);

            ContentControl contentControl = item as ContentControl;
            if (contentControl != null && contentControl.Content != null)
            {
                GetElements(contentControl.Content as FrameworkElement, results);
            }
            else
            {
                Canvas canvas = item as Canvas;
                if (canvas != null && canvas.Children.Count > 0)
                {
                    foreach (var child in canvas.Children)
                    {
                        GetElements(child as FrameworkElement, results);
                    }
                }
            }
        }
    }
}
