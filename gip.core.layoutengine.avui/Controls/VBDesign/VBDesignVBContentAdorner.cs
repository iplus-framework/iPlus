using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    //Custom content must be added to the root element of the BSO's Mainlayout
    //Example:
    //<vb:VBDockingManager.Resources>
    //	 <ResourceDictionary>
    //		<vb:VBDesignVBContentInfo x:Key="ContentInfo">
    //			<vb:VBDesignVBContentText VBContent ="ACUrl" />
    //          <vb:VBDesignVBContentText VBContent ="ACCaption" />
    //      </vb:VBDesignVBContentInfo>
    //	 </ResourceDictionary>
    //</vb:VBDockingManager.Resources>

    public class VBDesignVBContentAdorner : Adorner
    {
        public VBDesignVBContentAdorner(Control adornedElement, VBDesignVBContentInfo adornerInfo) : base(adornedElement)
        {
            _AdornerInfo = adornerInfo;
        }

        VBDesignVBContentInfo _AdornerInfo;

        public override void Render(DrawingContext drawingContext)
        {
            IVBContent contentControl = AdornedElement as IVBContent;
            if (contentControl != null && contentControl.VBContent != null)
            {
                IBrush background = Brushes.Black;
                IBrush foreground = Brushes.White;
                double fontSize = 15;

                if (_AdornerInfo != null)
                {
                    if (_AdornerInfo.Background != null)
                        background = _AdornerInfo.Background;

                    if (_AdornerInfo.FontSize != null)
                        fontSize = _AdornerInfo.FontSize.Value;

                    if (_AdornerInfo.Foreground != null)
                        foreground = _AdornerInfo.Foreground;
                }


                if (!string.IsNullOrEmpty(contentControl.VBContent) && (_AdornerInfo == null || !_AdornerInfo.TextCollection.Any()))
                {
                    FormattedText text = new FormattedText(contentControl.VBContent, System.Globalization.CultureInfo.CurrentCulture, FlowDirection, new Typeface("Arial"), fontSize, foreground);//, VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    drawingContext.DrawRectangle(background, new Pen(foreground, 1), new Rect(new Size(text.Width + 2, fontSize + 2)));
                    drawingContext.DrawText(text, new Point(1,1));
                }
                else if (_AdornerInfo != null && _AdornerInfo.TextCollection.Any())
                {
                    List<FormattedText> textList = new List<FormattedText>();

                    double height = 0;

                    foreach (VBDesignVBContentText textItem in _AdornerInfo.TextCollection)
                    {
                        if (string.IsNullOrEmpty(textItem.VBContent))
                            continue;

                        IACInteractiveObject control = contentControl as IACInteractiveObject;
                        if (control == null || control.ContextACObject == null)
                            continue;

                        object val = control.ContextACObject.GetValue(textItem.VBContent);

                        if (val == null)
                            continue;

                        height += fontSize;

                        FormattedText text = new FormattedText(val.ToString(), System.Globalization.CultureInfo.CurrentCulture, FlowDirection, new Typeface("Arial"), fontSize, foreground); //, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                        textList.Add(text);
                    }

                    if (textList.Any())
                    {
                        double width = textList.OrderByDescending(c => c.Width).FirstOrDefault().Width;
                        drawingContext.DrawRectangle(background, new Pen(Brushes.White, 1), new Rect(new Size(width + 2, height + textList.Count + 1)));

                        double yPoint = 0;

                        foreach (FormattedText text in textList)
                        {
                            drawingContext.DrawText(text, new Point(1, yPoint));
                            yPoint += fontSize + 1;
                        }
                    }
                }
            }
            base.Render(drawingContext);
        }
    }

    public class VBDesignVBContentInfo
    {
        public VBDesignVBContentInfo()
        {
            TextCollection = new VBDesignVBContentTextCollection(); 
        }

        [Content]
        [Category("VBControl")]
        public VBDesignVBContentTextCollection TextCollection
        {
            get; set;
        }

        [Category("VBControl")]
        public SolidColorBrush Foreground
        {
            get;
            set;
        }

        [Category("VBControl")]
        public SolidColorBrush Background
        {
            get;
            set;
        }

        [Category("VBControl")]
        public double? FontSize
        {
            get;
            set;
        }
    }

    public class VBDesignVBContentTextCollection : List<VBDesignVBContentText>, ICollection<VBDesignVBContentText>
    {
        public VBDesignVBContentTextCollection()
        {

        }
    }

    public class VBDesignVBContentText
    {
        public VBDesignVBContentText()
        {

        }

        [Category("VBControl")]
        public string VBContent
        {
            get;
            set;
        }

        [Category("VBControl")]
        public SolidColorBrush Foreground
        {
            get;
            set;
        }
    }
}
