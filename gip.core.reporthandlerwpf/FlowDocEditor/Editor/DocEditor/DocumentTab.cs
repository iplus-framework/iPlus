using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Document.Editor
{
    public class DocumentTab : TabItem
    {
        #region c'tors
        public DocumentTab(string name, Brush background, FlowDocument flowDoc)
        {
            if (flowDoc != null)
                withEventsField_Editor = new DocumentEditor(flowDoc);
            else
                withEventsField_Editor = new DocumentEditor();

            SubscribeEvents();

            BackgroundBrush = background;
            HeaderContent.Background = BackgroundBrush;
            this.BorderThickness = new Thickness(0, 0, 0, 0);
            DocName = name;
            HeaderContent.TabTitle.Text = name;
            HeaderContent.TabToolTip.Title = name;
            this.Header = HeaderContent;
            this.Height = 24;
            if (gip.core.reporthandlerwpf.Properties.Settings.Default.Options_SpellCheck)
            {
                SpellCheck.SetIsEnabled(Editor, true);
            }
            VSV.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            VSV.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            Editor.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Editor.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Editor.BorderThickness = new Thickness(0, 0, 0, 0);
            Editor.Width = Editor.Document.PageWidth;
            Editor.Height = Editor.Document.PageHeight;
            //DocumentView.VerticalAlignment = Windows.VerticalAlignment.Top
            //DocumentView.HorizontalAlignment = Windows.HorizontalAlignment.Center
            //DocumentView.Width = Editor.Width
            //DocumentView.Height = Editor.Height
            //DocumentView.Margin = New Thickness(0, -16, 0, 32)
            //DocumentView.Background = Brushes.Black
            System.Windows.Media.Effects.DropShadowEffect editoreffect = new System.Windows.Media.Effects.DropShadowEffect();
            editoreffect.BlurRadius = 32;
            editoreffect.Direction = 90;
            Editor.Effect = editoreffect;
            Editor.ClipToBounds = false;
            Editor.Margin = new Thickness(0, 0, 0, 32);
            if (gip.core.reporthandlerwpf.Properties.Settings.Default.MainWindow_ShowRuler)
            {
                VSV.Margin = new Thickness(-6, 23, 0, 0);
            }
            else
            {
                VSV.Margin = new Thickness(-5, -4, 0, 0);
                Ruler.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (gip.core.reporthandlerwpf.Properties.Settings.Default.Options_RulerMeasurement == 0)
            {
                hruler.Unit = Semagsoft.DocRuler.Unit.Inch;
            }
            else
            {
                hruler.Unit = Semagsoft.DocRuler.Unit.Cm;
            }
            hruler.AutoSize = true;
            hruler.Height = 24;
            hruler.Width = Editor.Width;
            hruler.ClipToBounds = true;
            hruler.Margin = new Thickness(0, -1, 0, 0);
            Ruler.Children.Add(hruler);
            Ruler.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Ruler.Width = Editor.Width;
            Vis.Background = Brushes.Black;
            //DocumentView.Children.Add(Vis)
            //DocumentView.Children.Add(Editor)
            VSV.Content = Editor;
            //Vis.Width = Editor.Width
            //Vis.Height = Editor.Height
            //Vis.Background = Brushes.Black
            ContentView.Children.Add(Ruler);
            ContentView.Children.Add(VSV);
            //ContentView.Children.Add(Vis)
            ContentView.Margin = new Thickness(0, -3, 0, 0);
            Content = ContentView;
            VSV.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Editor.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            TBoxContextMenu = new ContextMenu();
            BitmapImage insertimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/add16.png", UriKind.Relative));
            Image inserticon = new Image();
            BitmapImage tableimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/table16.png", UriKind.Relative));
            Image tableicon = new Image();
            BitmapImage dateimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/date16.png", UriKind.Relative));
            Image dateicon = new Image();
            BitmapImage timeimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/time16.png", UriKind.Relative));
            Image timeicon = new Image();
            BitmapImage imageimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/image16.png", UriKind.Relative));
            Image imageicon = new Image();
            BitmapImage videoimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/video16.png", UriKind.Relative));
            Image videoicon = new Image();
            BitmapImage objectimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/object16.png", UriKind.Relative));
            Image objecticon = new Image();
            BitmapImage shapeimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/shape16.png", UriKind.Relative));
            Image shapeicon = new Image();
            BitmapImage linkimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/link16.png", UriKind.Relative));
            Image linkicon = new Image();
            BitmapImage flowdocumentimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Tab/xaml16.png", UriKind.Relative));
            Image flowdocumenticon = new Image();
            BitmapImage richtextfileimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Tab/rtf16.png", UriKind.Relative));
            Image richtextfileicon = new Image();
            BitmapImage textfileimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/textfile16.png", UriKind.Relative));
            Image textfileicon = new Image();
            BitmapImage symbolimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/symbol16.png", UriKind.Relative));
            Image symbolicon = new Image();
            BitmapImage lineimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/horizontalline16.png", UriKind.Relative));
            Image lineicon = new Image();
            BitmapImage headerimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/header16.png", UriKind.Relative));
            Image headericon = new Image();
            BitmapImage footerimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Insert/footer16.png", UriKind.Relative));
            Image footericon = new Image();
            BitmapImage clearformattingimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/clearformatting16.png", UriKind.Relative));
            Image clearformattingicon = new Image();
            BitmapImage fontcolorimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/fontfacecolor16.png", UriKind.Relative));
            Image fontcoloricon = new Image();
            BitmapImage fonthighlightcolorimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/fontcolor16.png", UriKind.Relative));
            Image fonthighlightcoloricon = new Image();
            BitmapImage boldimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/bold16.png", UriKind.Relative));
            Image boldicon = new Image();
            BitmapImage italicimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/italic16.png", UriKind.Relative));
            Image italicicon = new Image();
            BitmapImage Underlineimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/underline16.png", UriKind.Relative));
            Image underlineicon = new Image();
            BitmapImage strikethroughimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/strikethrough16.png", UriKind.Relative));
            Image strikethroughicon = new Image();
            BitmapImage subscriptimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/subscript16.png", UriKind.Relative));
            Image subscripticon = new Image();
            BitmapImage superscriptimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/superscript16.png", UriKind.Relative));
            Image superscripticon = new Image();
            BitmapImage indentmoreimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/indentmore16.png", UriKind.Relative));
            Image indentmoreicon = new Image();
            BitmapImage indentlessimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/indentless16.png", UriKind.Relative));
            Image indentlessicon = new Image();
            BitmapImage bulletlistimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/bulletlist16.png", UriKind.Relative));
            Image bulletlisticon = new Image();
            BitmapImage numberlistimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/numberlist16.png", UriKind.Relative));
            Image numberlisticon = new Image();
            BitmapImage alignleftimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/left16.png", UriKind.Relative));
            Image alignlefticon = new Image();
            BitmapImage aligncenterimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/center16.png", UriKind.Relative));
            Image aligncentericon = new Image();
            BitmapImage alignrightimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/right16.png", UriKind.Relative));
            Image alignrighticon = new Image();
            BitmapImage alignjustifyimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/justify16.png", UriKind.Relative));
            Image alignjustifyicon = new Image();
            BitmapImage linespacingimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/linespacing16.png", UriKind.Relative));
            Image linespacingicon = new Image();
            BitmapImage ltrimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/ltr16.png", UriKind.Relative));
            Image ltricon = new Image();
            BitmapImage rtlimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Format/rtl16.png", UriKind.Relative));
            Image rtlicon = new Image();
            BitmapImage undoimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/undo16.png", UriKind.Relative));
            Image undoicon = new Image();
            BitmapImage redoimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/redo16.png", UriKind.Relative));
            Image redoicon = new Image();
            BitmapImage cutimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/cut16.png", UriKind.Relative));
            Image cuticon = new Image();
            BitmapImage copyimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/copy16.png", UriKind.Relative));
            Image copyicon = new Image();
            BitmapImage pasteimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/paste16.png", UriKind.Relative));
            Image pasteicon = new Image();
            BitmapImage deleteimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/delete16.png", UriKind.Relative));
            Image deleteicon = new Image();
            BitmapImage selectallimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/selectall16.png", UriKind.Relative));
            Image selectallicon = new Image();
            BitmapImage findimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/find16.png", UriKind.Relative));
            Image findicon = new Image();
            BitmapImage replaceimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/findreplace16.png", UriKind.Relative));
            Image Replaceicon = new Image();
            BitmapImage gotoimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/goto16.png", UriKind.Relative));
            Image gotoicon = new Image();
            BitmapImage uppercaseimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/uppercase16.png", UriKind.Relative));
            Image uppercaseicon = new Image();
            BitmapImage lowercaseimg = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Edit/lowercase16.png", UriKind.Relative));
            Image lowercaseicon = new Image();
            inserticon.Width = 16;
            inserticon.Height = 16;
            inserticon.Source = insertimg;
            tableicon.Width = 16;
            tableicon.Height = 16;
            tableicon.Source = tableimg;
            dateicon.Width = 16;
            dateicon.Height = 16;
            dateicon.Source = dateimg;
            timeicon.Width = 16;
            timeicon.Height = 16;
            timeicon.Source = timeimg;
            imageicon.Width = 16;
            imageicon.Height = 16;
            imageicon.Source = imageimg;
            videoicon.Width = 16;
            videoicon.Height = 16;
            videoicon.Source = videoimg;
            objecticon.Width = 16;
            objecticon.Height = 16;
            objecticon.Source = objectimg;
            shapeicon.Width = 16;
            shapeicon.Height = 16;
            shapeicon.Source = shapeimg;
            linkicon.Width = 16;
            linkicon.Height = 16;
            linkicon.Source = linkimg;
            flowdocumenticon.Width = 16;
            flowdocumenticon.Height = 16;
            flowdocumenticon.Source = flowdocumentimg;
            richtextfileicon.Width = 16;
            richtextfileicon.Height = 16;
            richtextfileicon.Source = richtextfileimg;
            textfileicon.Width = 16;
            textfileicon.Height = 16;
            textfileicon.Source = textfileimg;
            symbolicon.Width = 16;
            symbolicon.Height = 16;
            symbolicon.Source = symbolimg;
            lineicon.Width = 16;
            lineicon.Height = 16;
            lineicon.Source = lineimg;
            headericon.Width = 16;
            headericon.Height = 16;
            headericon.Source = headerimg;
            footericon.Width = 16;
            footericon.Height = 16;
            footericon.Source = footerimg;
            clearformattingicon.Width = 16;
            clearformattingicon.Height = 16;
            clearformattingicon.Source = clearformattingimg;
            fontcoloricon.Width = 16;
            fontcoloricon.Height = 16;
            fontcoloricon.Source = fontcolorimg;
            fonthighlightcoloricon.Width = 16;
            fonthighlightcoloricon.Height = 16;
            fonthighlightcoloricon.Source = fonthighlightcolorimg;
            boldicon.Width = 16;
            boldicon.Height = 16;
            boldicon.Source = boldimg;
            italicicon.Width = 16;
            italicicon.Height = 16;
            italicicon.Source = italicimg;
            underlineicon.Width = 16;
            underlineicon.Height = 16;
            underlineicon.Source = Underlineimg;
            strikethroughicon.Width = 16;
            strikethroughicon.Height = 16;
            strikethroughicon.Source = strikethroughimg;
            subscripticon.Width = 16;
            subscripticon.Height = 16;
            subscripticon.Source = subscriptimg;
            superscripticon.Width = 16;
            superscripticon.Height = 16;
            superscripticon.Source = superscriptimg;
            indentmoreicon.Width = 16;
            indentmoreicon.Height = 16;
            indentmoreicon.Source = indentmoreimg;
            indentlessicon.Width = 16;
            indentlessicon.Height = 16;
            indentlessicon.Source = indentlessimg;
            bulletlisticon.Width = 16;
            bulletlisticon.Height = 16;
            bulletlisticon.Source = bulletlistimg;
            numberlisticon.Width = 16;
            numberlisticon.Height = 16;
            numberlisticon.Source = numberlistimg;
            alignlefticon.Width = 16;
            alignlefticon.Height = 16;
            alignlefticon.Source = alignleftimg;
            aligncentericon.Width = 16;
            aligncentericon.Height = 16;
            aligncentericon.Source = aligncenterimg;
            alignrighticon.Width = 16;
            alignrighticon.Height = 16;
            alignrighticon.Source = alignrightimg;
            alignjustifyicon.Width = 16;
            alignjustifyicon.Height = 16;
            alignjustifyicon.Source = alignjustifyimg;
            linespacingicon.Width = 16;
            linespacingicon.Height = 16;
            linespacingicon.Source = linespacingimg;
            ltricon.Width = 16;
            ltricon.Height = 16;
            ltricon.Source = ltrimg;
            rtlicon.Width = 16;
            rtlicon.Height = 16;
            rtlicon.Source = rtlimg;
            undoicon.Width = 16;
            undoicon.Height = 16;
            undoicon.Source = undoimg;
            redoicon.Width = 16;
            redoicon.Height = 16;
            redoicon.Source = redoimg;
            cuticon.Width = 16;
            cuticon.Height = 16;
            cuticon.Source = cutimg;
            copyicon.Width = 16;
            copyicon.Height = 16;
            copyicon.Source = copyimg;
            pasteicon.Width = 16;
            pasteicon.Height = 16;
            pasteicon.Source = pasteimg;
            deleteicon.Width = 16;
            deleteicon.Height = 16;
            deleteicon.Source = deleteimg;
            selectallicon.Width = 16;
            selectallicon.Height = 16;
            selectallicon.Source = selectallimg;
            findicon.Width = 16;
            findicon.Height = 16;
            findicon.Source = findimg;
            Replaceicon.Width = 16;
            Replaceicon.Height = 16;
            Replaceicon.Source = replaceimg;
            gotoicon.Width = 16;
            gotoicon.Height = 16;
            gotoicon.Source = gotoimg;
            uppercaseicon.Width = 16;
            uppercaseicon.Height = 16;
            uppercaseicon.Source = uppercaseimg;
            lowercaseicon.Width = 16;
            lowercaseicon.Height = 16;
            lowercaseicon.Source = lowercaseimg;
            ObjectMenuItem.Header = "Object";
            ShapeMenuItem.Header = "Shape";
            ImageMenuItem.Header = "Image";
            LinkMenuItem.Header = "Link";
            FlowDocumentMenuItem.Header = "FlowDocument";
            RichTextFileMenuItem.Header = "Rich Text File";
            TextFileMenuItem.Header = "Text File";
            SymbolMenuItem.Header = "Symbol";
            TableMenuItem.Header = "Table";
            TableMenuItem.Icon = tableicon;
            DateMenuItem.Icon = dateicon;
            TimeMenuItem.Icon = timeicon;
            ImageMenuItem.Icon = imageicon;
            VideoMenuItem.Icon = videoicon;
            ObjectMenuItem.Icon = objecticon;
            ShapeMenuItem.Icon = shapeicon;
            LinkMenuItem.Icon = linkicon;
            FlowDocumentMenuItem.Icon = flowdocumenticon;
            RichTextFileMenuItem.Icon = richtextfileicon;
            TextFileMenuItem.Icon = textfileicon;
            SymbolMenuItem.Icon = symbolicon;
            HorizontalLineMenuItem.Icon = lineicon;
            HeaderMenuItem.Icon = headericon;
            FooterMenuItem.Icon = footericon;
            VideoMenuItem.Header = "Video";
            HorizontalLineMenuItem.Header = "Horizontal Line";
            HeaderMenuItem.Header = "Header";
            FooterMenuItem.Header = "Footer";
            DateMenuItem.Header = "Date";
            TimeMenuItem.Header = "Time";
            ClearFormattingMenuItem.Icon = clearformattingicon;
            ClearFormattingMenuItem.Header = "Clear Formatting";
            FontMenuItem.Header = "Font";
            FontSizeMenuItem.Header = "Font Size";
            FontColorMenuItem.Icon = fontcoloricon;
            FontColorMenuItem.Header = "Font Color";
            HighlightMenuItem.Icon = fonthighlightcoloricon;
            HighlightMenuItem.Header = "Highlight Color";
            BoldMenuItem.Icon = boldicon;
            BoldMenuItem.Header = "Bold";
            ItalicMenuItem.Icon = italicicon;
            ItalicMenuItem.Header = "Italic";
            UnderlineMenuItem.Icon = underlineicon;
            UnderlineMenuItem.Header = "Underline";
            StrikethroughMenuItem.Icon = strikethroughicon;
            StrikethroughMenuItem.Header = "Strikethrough";
            SubscriptMenuItem.Icon = subscripticon;
            SubscriptMenuItem.Header = "Subscript";
            SuperscriptMenuItem.Icon = superscripticon;
            SuperscriptMenuItem.Header = "Superscript";
            IndentMoreMenuItem.Icon = indentmoreicon;
            IndentMoreMenuItem.Header = "Indent More";
            IndentLessMenuItem.Icon = indentlessicon;
            IndentLessMenuItem.Header = "Indent Less";
            BulletListMenuItem.Icon = bulletlisticon;
            BulletListMenuItem.Header = "Bullet List";
            NumberListMenuItem.Icon = numberlisticon;
            NumberListMenuItem.Header = "Number List";
            AlignLeftMenuItem.Icon = alignlefticon;
            AlignLeftMenuItem.Header = "Align Left";
            AlignCenterMenuItem.Icon = aligncentericon;
            AlignCenterMenuItem.Header = "Align Center";
            AlignRightMenuItem.Icon = alignrighticon;
            AlignRightMenuItem.Header = "Align Right";
            AlignJustifyMenuItem.Icon = alignjustifyicon;
            AlignJustifyMenuItem.Header = "Align Justify";
            LineSpacingMenuItem.Icon = linespacingicon;
            LineSpacingMenuItem.Header = "Line Spacing";
            LefttoRightMenuItem.Icon = ltricon;
            LefttoRightMenuItem.Header = "Left To Right";
            RighttoLeftMenuItem.Icon = rtlicon;
            RighttoLeftMenuItem.Header = "Right To Left";
            UndoMenuItem.Header = "Undo";
            UndoMenuItem.InputGestureText = "Ctrl+Z";
            UndoMenuItem.ToolTip = "Undo (Ctrl + Z)";
            UndoMenuItem.Icon = undoicon;
            RedoMenuItem.Header = "Redo";
            RedoMenuItem.InputGestureText = "Ctrl+Y";
            RedoMenuItem.ToolTip = "Redo (Ctrl + Y)";
            RedoMenuItem.Icon = redoicon;
            CutMenuItem.Header = "Cut";
            CutMenuItem.InputGestureText = "Ctrl+X";
            CutMenuItem.ToolTip = "Cut (Ctrl + X)";
            CutMenuItem.Icon = cuticon;
            CopyMenuItem.Header = "Copy";
            CopyMenuItem.InputGestureText = "Ctrl+C";
            CopyMenuItem.ToolTip = "Copy (Ctrl + C)";
            CopyMenuItem.Icon = copyicon;
            PasteMenuItem.Header = "Paste";
            PasteMenuItem.InputGestureText = "Ctrl+V";
            PasteMenuItem.ToolTip = "Paste (Ctrl + V)";
            PasteMenuItem.Icon = pasteicon;
            DeleteMenuItem.Header = "Delete";
            DeleteMenuItem.InputGestureText = "Del";
            DeleteMenuItem.ToolTip = "Delete (Del)";
            DeleteMenuItem.Icon = deleteicon;
            SelectAllMenuItem.Header = "Select All";
            SelectAllMenuItem.InputGestureText = "Ctrl+A";
            SelectAllMenuItem.ToolTip = "Select All (Ctrl + A)";
            SelectAllMenuItem.Icon = selectallicon;
            FindMenuItem.Header = "Find";
            FindMenuItem.InputGestureText = "Ctrl+F";
            FindMenuItem.ToolTip = "Find (Ctrl + F)";
            FindMenuItem.Icon = findicon;
            ReplaceMenuItem.Header = "Replace";
            ReplaceMenuItem.InputGestureText = "Ctrl+H";
            ReplaceMenuItem.ToolTip = "Replace (Ctrl + H)";
            ReplaceMenuItem.Icon = Replaceicon;
            GoToMenuItem.Header = "Go To";
            GoToMenuItem.InputGestureText = "Ctrl+G";
            GoToMenuItem.ToolTip = "Go To (Ctrl + G)";
            GoToMenuItem.Icon = gotoicon;
            SetCaseMenuItem.Header = "Set Case";
            UppercaseMenuItem.Icon = uppercaseicon;
            UppercaseMenuItem.Header = "Uppercase";
            LowercaseMenuItem.Icon = lowercaseicon;
            LowercaseMenuItem.Header = "Lowercase";
            MenuItem insertmenuitem = new MenuItem();
            insertmenuitem.Header = "Insert";
            insertmenuitem.Icon = inserticon;
            insertmenuitem.Items.Add(TableMenuItem);
            insertmenuitem.Items.Add(new Separator());
            insertmenuitem.Items.Add(DateMenuItem);
            insertmenuitem.Items.Add(TimeMenuItem);
            insertmenuitem.Items.Add(new Separator());
            insertmenuitem.Items.Add(ImageMenuItem);
            insertmenuitem.Items.Add(VideoMenuItem);
            insertmenuitem.Items.Add(new Separator());
            insertmenuitem.Items.Add(ObjectMenuItem);
            insertmenuitem.Items.Add(ShapeMenuItem);
            insertmenuitem.Items.Add(LinkMenuItem);
            insertmenuitem.Items.Add(FlowDocumentMenuItem);
            insertmenuitem.Items.Add(RichTextFileMenuItem);
            insertmenuitem.Items.Add(TextFileMenuItem);
            insertmenuitem.Items.Add(SymbolMenuItem);
            insertmenuitem.Items.Add(HorizontalLineMenuItem);
            insertmenuitem.Items.Add(HeaderMenuItem);
            insertmenuitem.Items.Add(FooterMenuItem);
            MenuItem formatmenuitem = new MenuItem();
            formatmenuitem.Header = "Format";
            formatmenuitem.Items.Add(ClearFormattingMenuItem);
            formatmenuitem.Items.Add(new Separator());
            MenuItem fontmenuitem2 = new MenuItem();
            fontmenuitem2.Header = "Font";
            fontmenuitem2.Items.Add(FontMenuItem);
            fontmenuitem2.Items.Add(FontSizeMenuItem);
            fontmenuitem2.Items.Add(FontColorMenuItem);
            fontmenuitem2.Items.Add(HighlightMenuItem);
            formatmenuitem.Items.Add(fontmenuitem2);
            MenuItem stylemenuitem = new MenuItem();
            stylemenuitem.Header = "Style";
            stylemenuitem.Items.Add(BoldMenuItem);
            stylemenuitem.Items.Add(ItalicMenuItem);
            stylemenuitem.Items.Add(UnderlineMenuItem);
            stylemenuitem.Items.Add(StrikethroughMenuItem);
            formatmenuitem.Items.Add(stylemenuitem);
            formatmenuitem.Items.Add(SubscriptMenuItem);
            formatmenuitem.Items.Add(SuperscriptMenuItem);
            formatmenuitem.Items.Add(IndentMoreMenuItem);
            formatmenuitem.Items.Add(IndentLessMenuItem);
            formatmenuitem.Items.Add(BulletListMenuItem);
            formatmenuitem.Items.Add(NumberListMenuItem);
            MenuItem alignmenuitem = new MenuItem();
            alignmenuitem.Header = "Align";
            alignmenuitem.Items.Add(AlignLeftMenuItem);
            alignmenuitem.Items.Add(AlignCenterMenuItem);
            alignmenuitem.Items.Add(AlignRightMenuItem);
            alignmenuitem.Items.Add(AlignJustifyMenuItem);
            formatmenuitem.Items.Add(alignmenuitem);
            formatmenuitem.Items.Add(LineSpacingMenuItem);
            formatmenuitem.Items.Add(LefttoRightMenuItem);
            formatmenuitem.Items.Add(RighttoLeftMenuItem);
            TBoxContextMenu.Items.Add(insertmenuitem);
            TBoxContextMenu.Items.Add(formatmenuitem);
            TBoxContextMenu.Items.Add(new Separator());
            TBoxContextMenu.Items.Add(UndoMenuItem);
            TBoxContextMenu.Items.Add(RedoMenuItem);
            TBoxContextMenu.Items.Add(new Separator());
            TBoxContextMenu.Items.Add(CutMenuItem);
            TBoxContextMenu.Items.Add(CopyMenuItem);
            TBoxContextMenu.Items.Add(PasteMenuItem);
            TBoxContextMenu.Items.Add(DeleteMenuItem);
            TBoxContextMenu.Items.Add(new Separator());
            TBoxContextMenu.Items.Add(SelectAllMenuItem);
            TBoxContextMenu.Items.Add(new Separator());
            TBoxContextMenu.Items.Add(FindMenuItem);
            TBoxContextMenu.Items.Add(ReplaceMenuItem);
            TBoxContextMenu.Items.Add(GoToMenuItem);
            TBoxContextMenu.Items.Add(new Separator());
            TBoxContextMenu.Items.Add(SetCaseMenuItem);
            SetCaseMenuItem.Items.Add(UppercaseMenuItem);
            SetCaseMenuItem.Items.Add(LowercaseMenuItem);
            Editor.ContextMenu = TBoxContextMenu;
        }
        #endregion

        #region "Publics"

        public string DocName = null;
        public string FileFormat = "Rich Text Document";

        public Brush BackgroundBrush;
        #region "Events"

        public static RoutedEvent UpdateSelected = EventManager.RegisterRoutedEvent("UpdateSelected", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertObjectEvent = EventManager.RegisterRoutedEvent("InsertObject", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertShapeEvent = EventManager.RegisterRoutedEvent("InsertShape", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertImageEvent = EventManager.RegisterRoutedEvent("InsertImage", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertLinkEvent = EventManager.RegisterRoutedEvent("InsertLink", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertFlowDocumentEvent = EventManager.RegisterRoutedEvent("InsertFlowDocument", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertRichTextFileEvent = EventManager.RegisterRoutedEvent("InsertRichTextFile", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertTextFileEvent = EventManager.RegisterRoutedEvent("InsertTextFile", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertSymbolEvent = EventManager.RegisterRoutedEvent("InsertSymbol", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertTableEvent = EventManager.RegisterRoutedEvent("InsertTable", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertVideoEvent = EventManager.RegisterRoutedEvent("InsertVideo", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertHorizontalLineEvent = EventManager.RegisterRoutedEvent("InsertHorizontalLine", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertHeaderEvent = EventManager.RegisterRoutedEvent("InsertHeader", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertFooterEvent = EventManager.RegisterRoutedEvent("InsertFooter", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertDateEvent = EventManager.RegisterRoutedEvent("InsertDate", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent InsertTimeEvent = EventManager.RegisterRoutedEvent("InsertTime", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent ClearFormattingEvent = EventManager.RegisterRoutedEvent("ClearFormatting", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent FontEvent = EventManager.RegisterRoutedEvent("Font", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent FontSizeEvent = EventManager.RegisterRoutedEvent("FontSize", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent FontColorEvent = EventManager.RegisterRoutedEvent("FontColor", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent HighlightColorEvent = EventManager.RegisterRoutedEvent("HighlightColor", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent BoldEvent = EventManager.RegisterRoutedEvent("Bold", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent ItalicEvent = EventManager.RegisterRoutedEvent("Italic", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent UnderlineEvent = EventManager.RegisterRoutedEvent("Underline", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent StrikethroughEvent = EventManager.RegisterRoutedEvent("Strikethrough", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent SubscriptEvent = EventManager.RegisterRoutedEvent("Subscript", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent SuperscriptEvent = EventManager.RegisterRoutedEvent("Superscript", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent IndentMoreEvent = EventManager.RegisterRoutedEvent("IndentMore", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent IndentLessEvent = EventManager.RegisterRoutedEvent("IndentLess", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent BulletListEvent = EventManager.RegisterRoutedEvent("BulletList", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent NumberListEvent = EventManager.RegisterRoutedEvent("NumberList", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent AlignLeftEvent = EventManager.RegisterRoutedEvent("AlignLeft", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent AlignCenterEvent = EventManager.RegisterRoutedEvent("AlignCenter", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent AlignRightEvent = EventManager.RegisterRoutedEvent("AlignRight", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent AlignJustifyEvent = EventManager.RegisterRoutedEvent("AlignJustify", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent LineSpacingEvent = EventManager.RegisterRoutedEvent("LineSpacing", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent LeftToRightEvent = EventManager.RegisterRoutedEvent("LeftToRight", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent RightToLeftEvent = EventManager.RegisterRoutedEvent("RightToLeft", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent UndoEvent = EventManager.RegisterRoutedEvent("Undo", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent RedoEvent = EventManager.RegisterRoutedEvent("Redo", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent CutEvent = EventManager.RegisterRoutedEvent("Cut", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent CopyEvent = EventManager.RegisterRoutedEvent("Copy", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent PasteEvent = EventManager.RegisterRoutedEvent("Paste", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent DeleteEvent = EventManager.RegisterRoutedEvent("Delete", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent SelectAllEvent = EventManager.RegisterRoutedEvent("SelectAll", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent FindEvent = EventManager.RegisterRoutedEvent("Find", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        public static RoutedEvent ReplaceEvent = EventManager.RegisterRoutedEvent("Replace", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));

        public static RoutedEvent GoToEvent = EventManager.RegisterRoutedEvent("GoTo", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DocumentTab));
        #endregion

        public TabHeader HeaderContent = new TabHeader();
        public ScrollViewer VSV = new ScrollViewer();
        public Grid DocumentView = new Grid();
        public Grid Vis = new Grid();
        private DocumentEditor withEventsField_Editor;
        public DocumentEditor Editor
        {
            get { return withEventsField_Editor; }
            set
            {
                if (withEventsField_Editor != null)
                {
                    withEventsField_Editor.SelectionChanged -= TBox_SelectionChanged;
                }
                withEventsField_Editor = value;
                if (withEventsField_Editor != null)
                {
                    withEventsField_Editor.SelectionChanged += TBox_SelectionChanged;
                }
            }
        }
        public Label Title = new Label();

        public int MyIndex;
        public Grid ContentView = new Grid();
        public DockPanel Ruler = new DockPanel();

        public Semagsoft.DocRuler.Ruler hruler = new Semagsoft.DocRuler.Ruler();

        public MenuItem SetCaseMenuItem = new MenuItem();
        private ContextMenu withEventsField_TBoxContextMenu;
        public ContextMenu TBoxContextMenu
        {
            get { return withEventsField_TBoxContextMenu; }
            set
            {
                if (withEventsField_TBoxContextMenu != null)
                {
                    withEventsField_TBoxContextMenu.Opened -= TBoxContextMenu_Opened;
                }
                withEventsField_TBoxContextMenu = value;
                if (withEventsField_TBoxContextMenu != null)
                {
                    withEventsField_TBoxContextMenu.Opened += TBoxContextMenu_Opened;
                }
            }
        }
        private MenuItem withEventsField_ObjectMenuItem = new MenuItem();
        public MenuItem ObjectMenuItem
        {
            get { return withEventsField_ObjectMenuItem; }
            set
            {
                if (withEventsField_ObjectMenuItem != null)
                {
                    withEventsField_ObjectMenuItem.Click -= ObjectMenuItem_Click;
                }
                withEventsField_ObjectMenuItem = value;
                if (withEventsField_ObjectMenuItem != null)
                {
                    withEventsField_ObjectMenuItem.Click += ObjectMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_ShapeMenuItem = new MenuItem();
        public MenuItem ShapeMenuItem
        {
            get { return withEventsField_ShapeMenuItem; }
            set
            {
                if (withEventsField_ShapeMenuItem != null)
                {
                    withEventsField_ShapeMenuItem.Click -= ShapeMenuItem_Click;
                }
                withEventsField_ShapeMenuItem = value;
                if (withEventsField_ShapeMenuItem != null)
                {
                    withEventsField_ShapeMenuItem.Click += ShapeMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_ImageMenuItem = new MenuItem();
        public MenuItem ImageMenuItem
        {
            get { return withEventsField_ImageMenuItem; }
            set
            {
                if (withEventsField_ImageMenuItem != null)
                {
                    withEventsField_ImageMenuItem.Click -= ImageMenuItem_Click;
                }
                withEventsField_ImageMenuItem = value;
                if (withEventsField_ImageMenuItem != null)
                {
                    withEventsField_ImageMenuItem.Click += ImageMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_LinkMenuItem = new MenuItem();
        public MenuItem LinkMenuItem
        {
            get { return withEventsField_LinkMenuItem; }
            set
            {
                if (withEventsField_LinkMenuItem != null)
                {
                    withEventsField_LinkMenuItem.Click -= LinkMenuItem_Click;
                }
                withEventsField_LinkMenuItem = value;
                if (withEventsField_LinkMenuItem != null)
                {
                    withEventsField_LinkMenuItem.Click += LinkMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_FlowDocumentMenuItem = new MenuItem();
        public MenuItem FlowDocumentMenuItem
        {
            get { return withEventsField_FlowDocumentMenuItem; }
            set
            {
                if (withEventsField_FlowDocumentMenuItem != null)
                {
                    withEventsField_FlowDocumentMenuItem.Click -= FlowDocumentMenuItem_Click;
                }
                withEventsField_FlowDocumentMenuItem = value;
                if (withEventsField_FlowDocumentMenuItem != null)
                {
                    withEventsField_FlowDocumentMenuItem.Click += FlowDocumentMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_RichTextFileMenuItem = new MenuItem();
        public MenuItem RichTextFileMenuItem
        {
            get { return withEventsField_RichTextFileMenuItem; }
            set
            {
                if (withEventsField_RichTextFileMenuItem != null)
                {
                    withEventsField_RichTextFileMenuItem.Click -= RichTextFileMenuItem_Click;
                }
                withEventsField_RichTextFileMenuItem = value;
                if (withEventsField_RichTextFileMenuItem != null)
                {
                    withEventsField_RichTextFileMenuItem.Click += RichTextFileMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_TextFileMenuItem = new MenuItem();
        public MenuItem TextFileMenuItem
        {
            get { return withEventsField_TextFileMenuItem; }
            set
            {
                if (withEventsField_TextFileMenuItem != null)
                {
                    withEventsField_TextFileMenuItem.Click -= TextFileMenuItem_Click;
                }
                withEventsField_TextFileMenuItem = value;
                if (withEventsField_TextFileMenuItem != null)
                {
                    withEventsField_TextFileMenuItem.Click += TextFileMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_SymbolMenuItem = new MenuItem();
        public MenuItem SymbolMenuItem
        {
            get { return withEventsField_SymbolMenuItem; }
            set
            {
                if (withEventsField_SymbolMenuItem != null)
                {
                    withEventsField_SymbolMenuItem.Click -= SymbolMenuItem_Click;
                }
                withEventsField_SymbolMenuItem = value;
                if (withEventsField_SymbolMenuItem != null)
                {
                    withEventsField_SymbolMenuItem.Click += SymbolMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_TableMenuItem = new MenuItem();
        public MenuItem TableMenuItem
        {
            get { return withEventsField_TableMenuItem; }
            set
            {
                if (withEventsField_TableMenuItem != null)
                {
                    withEventsField_TableMenuItem.Click -= TableMenuItem_Click;
                }
                withEventsField_TableMenuItem = value;
                if (withEventsField_TableMenuItem != null)
                {
                    withEventsField_TableMenuItem.Click += TableMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_VideoMenuItem = new MenuItem();
        public MenuItem VideoMenuItem
        {
            get { return withEventsField_VideoMenuItem; }
            set
            {
                if (withEventsField_VideoMenuItem != null)
                {
                    withEventsField_VideoMenuItem.Click -= VideoMenuItem_Click;
                }
                withEventsField_VideoMenuItem = value;
                if (withEventsField_VideoMenuItem != null)
                {
                    withEventsField_VideoMenuItem.Click += VideoMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_HorizontalLineMenuItem = new MenuItem();
        public MenuItem HorizontalLineMenuItem
        {
            get { return withEventsField_HorizontalLineMenuItem; }
            set
            {
                if (withEventsField_HorizontalLineMenuItem != null)
                {
                    withEventsField_HorizontalLineMenuItem.Click -= HorizontalLineMenuItem_Click;
                }
                withEventsField_HorizontalLineMenuItem = value;
                if (withEventsField_HorizontalLineMenuItem != null)
                {
                    withEventsField_HorizontalLineMenuItem.Click += HorizontalLineMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_HeaderMenuItem = new MenuItem();
        public MenuItem HeaderMenuItem
        {
            get { return withEventsField_HeaderMenuItem; }
            set
            {
                if (withEventsField_HeaderMenuItem != null)
                {
                    withEventsField_HeaderMenuItem.Click -= HeaderMenuItem_Click;
                }
                withEventsField_HeaderMenuItem = value;
                if (withEventsField_HeaderMenuItem != null)
                {
                    withEventsField_HeaderMenuItem.Click += HeaderMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_FooterMenuItem = new MenuItem();
        public MenuItem FooterMenuItem
        {
            get { return withEventsField_FooterMenuItem; }
            set
            {
                if (withEventsField_FooterMenuItem != null)
                {
                    withEventsField_FooterMenuItem.Click -= FooterMenuItem_Click;
                }
                withEventsField_FooterMenuItem = value;
                if (withEventsField_FooterMenuItem != null)
                {
                    withEventsField_FooterMenuItem.Click += FooterMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_DateMenuItem = new MenuItem();
        public MenuItem DateMenuItem
        {
            get { return withEventsField_DateMenuItem; }
            set
            {
                if (withEventsField_DateMenuItem != null)
                {
                    withEventsField_DateMenuItem.Click -= DateMenuItem_Click;
                }
                withEventsField_DateMenuItem = value;
                if (withEventsField_DateMenuItem != null)
                {
                    withEventsField_DateMenuItem.Click += DateMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_TimeMenuItem = new MenuItem();
        public MenuItem TimeMenuItem
        {
            get { return withEventsField_TimeMenuItem; }
            set
            {
                if (withEventsField_TimeMenuItem != null)
                {
                    withEventsField_TimeMenuItem.Click -= TimeMenuItem_Click;
                }
                withEventsField_TimeMenuItem = value;
                if (withEventsField_TimeMenuItem != null)
                {
                    withEventsField_TimeMenuItem.Click += TimeMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_ClearFormattingMenuItem = new MenuItem();
        public MenuItem ClearFormattingMenuItem
        {
            get { return withEventsField_ClearFormattingMenuItem; }
            set
            {
                if (withEventsField_ClearFormattingMenuItem != null)
                {
                    withEventsField_ClearFormattingMenuItem.Click -= ClearFormattingMenuItem_Click;
                }
                withEventsField_ClearFormattingMenuItem = value;
                if (withEventsField_ClearFormattingMenuItem != null)
                {
                    withEventsField_ClearFormattingMenuItem.Click += ClearFormattingMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_FontMenuItem = new MenuItem();
        public MenuItem FontMenuItem
        {
            get { return withEventsField_FontMenuItem; }
            set
            {
                if (withEventsField_FontMenuItem != null)
                {
                    withEventsField_FontMenuItem.Click -= FontMenuItem_Click;
                }
                withEventsField_FontMenuItem = value;
                if (withEventsField_FontMenuItem != null)
                {
                    withEventsField_FontMenuItem.Click += FontMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_FontSizeMenuItem = new MenuItem();
        public MenuItem FontSizeMenuItem
        {
            get { return withEventsField_FontSizeMenuItem; }
            set
            {
                if (withEventsField_FontSizeMenuItem != null)
                {
                    withEventsField_FontSizeMenuItem.Click -= FontSizeMenuItem_Click;
                }
                withEventsField_FontSizeMenuItem = value;
                if (withEventsField_FontSizeMenuItem != null)
                {
                    withEventsField_FontSizeMenuItem.Click += FontSizeMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_FontColorMenuItem = new MenuItem();
        public MenuItem FontColorMenuItem
        {
            get { return withEventsField_FontColorMenuItem; }
            set
            {
                if (withEventsField_FontColorMenuItem != null)
                {
                    withEventsField_FontColorMenuItem.Click -= FontColorMenuItem_Click;
                }
                withEventsField_FontColorMenuItem = value;
                if (withEventsField_FontColorMenuItem != null)
                {
                    withEventsField_FontColorMenuItem.Click += FontColorMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_HighlightMenuItem = new MenuItem();
        public MenuItem HighlightMenuItem
        {
            get { return withEventsField_HighlightMenuItem; }
            set
            {
                if (withEventsField_HighlightMenuItem != null)
                {
                    withEventsField_HighlightMenuItem.Click -= HighlightMenuItem_Click;
                }
                withEventsField_HighlightMenuItem = value;
                if (withEventsField_HighlightMenuItem != null)
                {
                    withEventsField_HighlightMenuItem.Click += HighlightMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_BoldMenuItem = new MenuItem();
        public MenuItem BoldMenuItem
        {
            get { return withEventsField_BoldMenuItem; }
            set
            {
                if (withEventsField_BoldMenuItem != null)
                {
                    withEventsField_BoldMenuItem.Click -= BoldMenuItem_Click;
                }
                withEventsField_BoldMenuItem = value;
                if (withEventsField_BoldMenuItem != null)
                {
                    withEventsField_BoldMenuItem.Click += BoldMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_ItalicMenuItem = new MenuItem();
        public MenuItem ItalicMenuItem
        {
            get { return withEventsField_ItalicMenuItem; }
            set
            {
                if (withEventsField_ItalicMenuItem != null)
                {
                    withEventsField_ItalicMenuItem.Click -= ItalicMenuItem_Click;
                }
                withEventsField_ItalicMenuItem = value;
                if (withEventsField_ItalicMenuItem != null)
                {
                    withEventsField_ItalicMenuItem.Click += ItalicMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_UnderlineMenuItem = new MenuItem();
        public MenuItem UnderlineMenuItem
        {
            get { return withEventsField_UnderlineMenuItem; }
            set
            {
                if (withEventsField_UnderlineMenuItem != null)
                {
                    withEventsField_UnderlineMenuItem.Click -= UnderlineMenuItem_Click;
                }
                withEventsField_UnderlineMenuItem = value;
                if (withEventsField_UnderlineMenuItem != null)
                {
                    withEventsField_UnderlineMenuItem.Click += UnderlineMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_StrikethroughMenuItem = new MenuItem();
        public MenuItem StrikethroughMenuItem
        {
            get { return withEventsField_StrikethroughMenuItem; }
            set
            {
                if (withEventsField_StrikethroughMenuItem != null)
                {
                    withEventsField_StrikethroughMenuItem.Click -= StrikethroughMenuItem_Click;
                }
                withEventsField_StrikethroughMenuItem = value;
                if (withEventsField_StrikethroughMenuItem != null)
                {
                    withEventsField_StrikethroughMenuItem.Click += StrikethroughMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_SubscriptMenuItem = new MenuItem();
        public MenuItem SubscriptMenuItem
        {
            get { return withEventsField_SubscriptMenuItem; }
            set
            {
                if (withEventsField_SubscriptMenuItem != null)
                {
                    withEventsField_SubscriptMenuItem.Click -= SubscriptMenuItem_Click;
                }
                withEventsField_SubscriptMenuItem = value;
                if (withEventsField_SubscriptMenuItem != null)
                {
                    withEventsField_SubscriptMenuItem.Click += SubscriptMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_SuperscriptMenuItem = new MenuItem();
        public MenuItem SuperscriptMenuItem
        {
            get { return withEventsField_SuperscriptMenuItem; }
            set
            {
                if (withEventsField_SuperscriptMenuItem != null)
                {
                    withEventsField_SuperscriptMenuItem.Click -= SuperscriptMenuItem_Click;
                }
                withEventsField_SuperscriptMenuItem = value;
                if (withEventsField_SuperscriptMenuItem != null)
                {
                    withEventsField_SuperscriptMenuItem.Click += SuperscriptMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_IndentMoreMenuItem = new MenuItem();
        public MenuItem IndentMoreMenuItem
        {
            get { return withEventsField_IndentMoreMenuItem; }
            set
            {
                if (withEventsField_IndentMoreMenuItem != null)
                {
                    withEventsField_IndentMoreMenuItem.Click -= IndentMoreMenuItem_Click;
                }
                withEventsField_IndentMoreMenuItem = value;
                if (withEventsField_IndentMoreMenuItem != null)
                {
                    withEventsField_IndentMoreMenuItem.Click += IndentMoreMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_IndentLessMenuItem = new MenuItem();
        public MenuItem IndentLessMenuItem
        {
            get { return withEventsField_IndentLessMenuItem; }
            set
            {
                if (withEventsField_IndentLessMenuItem != null)
                {
                    withEventsField_IndentLessMenuItem.Click -= IndentLessMenuItem_Click;
                }
                withEventsField_IndentLessMenuItem = value;
                if (withEventsField_IndentLessMenuItem != null)
                {
                    withEventsField_IndentLessMenuItem.Click += IndentLessMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_BulletListMenuItem = new MenuItem();
        public MenuItem BulletListMenuItem
        {
            get { return withEventsField_BulletListMenuItem; }
            set
            {
                if (withEventsField_BulletListMenuItem != null)
                {
                    withEventsField_BulletListMenuItem.Click -= BulletListMenuItem_Click;
                }
                withEventsField_BulletListMenuItem = value;
                if (withEventsField_BulletListMenuItem != null)
                {
                    withEventsField_BulletListMenuItem.Click += BulletListMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_NumberListMenuItem = new MenuItem();
        public MenuItem NumberListMenuItem
        {
            get { return withEventsField_NumberListMenuItem; }
            set
            {
                if (withEventsField_NumberListMenuItem != null)
                {
                    withEventsField_NumberListMenuItem.Click -= NumberListMenuItem_Click;
                }
                withEventsField_NumberListMenuItem = value;
                if (withEventsField_NumberListMenuItem != null)
                {
                    withEventsField_NumberListMenuItem.Click += NumberListMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_AlignLeftMenuItem = new MenuItem();
        public MenuItem AlignLeftMenuItem
        {
            get { return withEventsField_AlignLeftMenuItem; }
            set
            {
                if (withEventsField_AlignLeftMenuItem != null)
                {
                    withEventsField_AlignLeftMenuItem.Click -= AlignLeftMenuItem_Click;
                }
                withEventsField_AlignLeftMenuItem = value;
                if (withEventsField_AlignLeftMenuItem != null)
                {
                    withEventsField_AlignLeftMenuItem.Click += AlignLeftMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_AlignCenterMenuItem = new MenuItem();
        public MenuItem AlignCenterMenuItem
        {
            get { return withEventsField_AlignCenterMenuItem; }
            set
            {
                if (withEventsField_AlignCenterMenuItem != null)
                {
                    withEventsField_AlignCenterMenuItem.Click -= AlignCenterMenuItem_Click;
                }
                withEventsField_AlignCenterMenuItem = value;
                if (withEventsField_AlignCenterMenuItem != null)
                {
                    withEventsField_AlignCenterMenuItem.Click += AlignCenterMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_AlignRightMenuItem = new MenuItem();
        public MenuItem AlignRightMenuItem
        {
            get { return withEventsField_AlignRightMenuItem; }
            set
            {
                if (withEventsField_AlignRightMenuItem != null)
                {
                    withEventsField_AlignRightMenuItem.Click -= AlignRightMenuItem_Click;
                }
                withEventsField_AlignRightMenuItem = value;
                if (withEventsField_AlignRightMenuItem != null)
                {
                    withEventsField_AlignRightMenuItem.Click += AlignRightMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_AlignJustifyMenuItem = new MenuItem();
        public MenuItem AlignJustifyMenuItem
        {
            get { return withEventsField_AlignJustifyMenuItem; }
            set
            {
                if (withEventsField_AlignJustifyMenuItem != null)
                {
                    withEventsField_AlignJustifyMenuItem.Click -= AlignJustifyMenuItem_Click;
                }
                withEventsField_AlignJustifyMenuItem = value;
                if (withEventsField_AlignJustifyMenuItem != null)
                {
                    withEventsField_AlignJustifyMenuItem.Click += AlignJustifyMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_LineSpacingMenuItem = new MenuItem();
        public MenuItem LineSpacingMenuItem
        {
            get { return withEventsField_LineSpacingMenuItem; }
            set
            {
                if (withEventsField_LineSpacingMenuItem != null)
                {
                    withEventsField_LineSpacingMenuItem.Click -= LineSpacingMenuItem_Click;
                }
                withEventsField_LineSpacingMenuItem = value;
                if (withEventsField_LineSpacingMenuItem != null)
                {
                    withEventsField_LineSpacingMenuItem.Click += LineSpacingMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_LefttoRightMenuItem = new MenuItem();
        public MenuItem LefttoRightMenuItem
        {
            get { return withEventsField_LefttoRightMenuItem; }
            set
            {
                if (withEventsField_LefttoRightMenuItem != null)
                {
                    withEventsField_LefttoRightMenuItem.Click -= LefttoRightMenuItem_Click;
                }
                withEventsField_LefttoRightMenuItem = value;
                if (withEventsField_LefttoRightMenuItem != null)
                {
                    withEventsField_LefttoRightMenuItem.Click += LefttoRightMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_RighttoLeftMenuItem = new MenuItem();
        public MenuItem RighttoLeftMenuItem
        {
            get { return withEventsField_RighttoLeftMenuItem; }
            set
            {
                if (withEventsField_RighttoLeftMenuItem != null)
                {
                    withEventsField_RighttoLeftMenuItem.Click -= RighttoLeftMenuItem_Click;
                }
                withEventsField_RighttoLeftMenuItem = value;
                if (withEventsField_RighttoLeftMenuItem != null)
                {
                    withEventsField_RighttoLeftMenuItem.Click += RighttoLeftMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_UndoMenuItem = new MenuItem();
        public MenuItem UndoMenuItem
        {
            get { return withEventsField_UndoMenuItem; }
            set
            {
                if (withEventsField_UndoMenuItem != null)
                {
                    withEventsField_UndoMenuItem.Click -= UndoMenuItem_Click;
                }
                withEventsField_UndoMenuItem = value;
                if (withEventsField_UndoMenuItem != null)
                {
                    withEventsField_UndoMenuItem.Click += UndoMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_RedoMenuItem = new MenuItem();
        public MenuItem RedoMenuItem
        {
            get { return withEventsField_RedoMenuItem; }
            set
            {
                if (withEventsField_RedoMenuItem != null)
                {
                    withEventsField_RedoMenuItem.Click -= RedoMenuItem_Click;
                }
                withEventsField_RedoMenuItem = value;
                if (withEventsField_RedoMenuItem != null)
                {
                    withEventsField_RedoMenuItem.Click += RedoMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_CutMenuItem = new MenuItem();
        public MenuItem CutMenuItem
        {
            get { return withEventsField_CutMenuItem; }
            set
            {
                if (withEventsField_CutMenuItem != null)
                {
                    withEventsField_CutMenuItem.Click -= CutMenuItem_Click;
                }
                withEventsField_CutMenuItem = value;
                if (withEventsField_CutMenuItem != null)
                {
                    withEventsField_CutMenuItem.Click += CutMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_CopyMenuItem = new MenuItem();
        public MenuItem CopyMenuItem
        {
            get { return withEventsField_CopyMenuItem; }
            set
            {
                if (withEventsField_CopyMenuItem != null)
                {
                    withEventsField_CopyMenuItem.Click -= CopyMenuItem_Click;
                }
                withEventsField_CopyMenuItem = value;
                if (withEventsField_CopyMenuItem != null)
                {
                    withEventsField_CopyMenuItem.Click += CopyMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_PasteMenuItem = new MenuItem();
        public MenuItem PasteMenuItem
        {
            get { return withEventsField_PasteMenuItem; }
            set
            {
                if (withEventsField_PasteMenuItem != null)
                {
                    withEventsField_PasteMenuItem.Click -= PasteMenuItem_Click;
                }
                withEventsField_PasteMenuItem = value;
                if (withEventsField_PasteMenuItem != null)
                {
                    withEventsField_PasteMenuItem.Click += PasteMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_DeleteMenuItem = new MenuItem();
        public MenuItem DeleteMenuItem
        {
            get { return withEventsField_DeleteMenuItem; }
            set
            {
                if (withEventsField_DeleteMenuItem != null)
                {
                    withEventsField_DeleteMenuItem.Click -= DeleteMenuItem_Click;
                }
                withEventsField_DeleteMenuItem = value;
                if (withEventsField_DeleteMenuItem != null)
                {
                    withEventsField_DeleteMenuItem.Click += DeleteMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_SelectAllMenuItem = new MenuItem();
        public MenuItem SelectAllMenuItem
        {
            get { return withEventsField_SelectAllMenuItem; }
            set
            {
                if (withEventsField_SelectAllMenuItem != null)
                {
                    withEventsField_SelectAllMenuItem.Click -= SelectAllMenuItem_Click;
                }
                withEventsField_SelectAllMenuItem = value;
                if (withEventsField_SelectAllMenuItem != null)
                {
                    withEventsField_SelectAllMenuItem.Click += SelectAllMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_FindMenuItem = new MenuItem();
        public MenuItem FindMenuItem
        {
            get { return withEventsField_FindMenuItem; }
            set
            {
                if (withEventsField_FindMenuItem != null)
                {
                    withEventsField_FindMenuItem.Click -= FindMenuItem_Click;
                }
                withEventsField_FindMenuItem = value;
                if (withEventsField_FindMenuItem != null)
                {
                    withEventsField_FindMenuItem.Click += FindMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_ReplaceMenuItem = new MenuItem();
        public MenuItem ReplaceMenuItem
        {
            get { return withEventsField_ReplaceMenuItem; }
            set
            {
                if (withEventsField_ReplaceMenuItem != null)
                {
                    withEventsField_ReplaceMenuItem.Click -= ReplaceMenuItem_Click;
                }
                withEventsField_ReplaceMenuItem = value;
                if (withEventsField_ReplaceMenuItem != null)
                {
                    withEventsField_ReplaceMenuItem.Click += ReplaceMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_GoToMenuItem = new MenuItem();
        public MenuItem GoToMenuItem
        {
            get { return withEventsField_GoToMenuItem; }
            set
            {
                if (withEventsField_GoToMenuItem != null)
                {
                    withEventsField_GoToMenuItem.Click -= GoToMenuItem_Click;
                }
                withEventsField_GoToMenuItem = value;
                if (withEventsField_GoToMenuItem != null)
                {
                    withEventsField_GoToMenuItem.Click += GoToMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_UppercaseMenuItem = new MenuItem();
        public MenuItem UppercaseMenuItem
        {
            get { return withEventsField_UppercaseMenuItem; }
            set
            {
                if (withEventsField_UppercaseMenuItem != null)
                {
                    withEventsField_UppercaseMenuItem.Click -= UppercaseMenuItem_Click;
                }
                withEventsField_UppercaseMenuItem = value;
                if (withEventsField_UppercaseMenuItem != null)
                {
                    withEventsField_UppercaseMenuItem.Click += UppercaseMenuItem_Click;
                }
            }
        }
        private MenuItem withEventsField_LowercaseMenuItem = new MenuItem();
        public MenuItem LowercaseMenuItem
        {
            get { return withEventsField_LowercaseMenuItem; }
            set
            {
                if (withEventsField_LowercaseMenuItem != null)
                {
                    withEventsField_LowercaseMenuItem.Click -= LowercaseMenuItem_Click;
                }
                withEventsField_LowercaseMenuItem = value;
                if (withEventsField_LowercaseMenuItem != null)
                {
                    withEventsField_LowercaseMenuItem.Click += LowercaseMenuItem_Click;
                }
            }
        }
        #endregion

        #region "FileBox"

        public void SetPageSize(double height, double width)
        {
            Editor.Document.PageHeight = height;
            Editor.Height = height;
            Editor.Document.PageWidth = width;
            Editor.Width = width;
            hruler.Width = width;
            Ruler.Width = width;
        }

        private bool _EventsSubscr = false;
        public void SubscribeEvents()
        {
            if (_EventsSubscr)
                return;
            withEventsField_Editor.SelectionChanged += TBox_SelectionChanged;
            withEventsField_ObjectMenuItem.Click += ObjectMenuItem_Click;
            withEventsField_ShapeMenuItem.Click += ShapeMenuItem_Click;
            withEventsField_ImageMenuItem.Click += ImageMenuItem_Click;
            withEventsField_LinkMenuItem.Click += LinkMenuItem_Click;
            withEventsField_FlowDocumentMenuItem.Click += FlowDocumentMenuItem_Click;
            withEventsField_RichTextFileMenuItem.Click += RichTextFileMenuItem_Click;
            withEventsField_TextFileMenuItem.Click += TextFileMenuItem_Click;
            withEventsField_SymbolMenuItem.Click += SymbolMenuItem_Click;
            withEventsField_TableMenuItem.Click += TableMenuItem_Click;
            withEventsField_VideoMenuItem.Click += VideoMenuItem_Click;
            withEventsField_HorizontalLineMenuItem.Click += HorizontalLineMenuItem_Click;
            withEventsField_HeaderMenuItem.Click += HeaderMenuItem_Click;
            withEventsField_FooterMenuItem.Click += FooterMenuItem_Click;
            withEventsField_DateMenuItem.Click += DateMenuItem_Click;
            withEventsField_TimeMenuItem.Click += TimeMenuItem_Click;
            withEventsField_ClearFormattingMenuItem.Click += ClearFormattingMenuItem_Click;
            withEventsField_FontMenuItem.Click += FontMenuItem_Click;
            withEventsField_FontSizeMenuItem.Click += FontSizeMenuItem_Click;
            withEventsField_FontColorMenuItem.Click += FontColorMenuItem_Click;
            withEventsField_HighlightMenuItem.Click += HighlightMenuItem_Click;
            withEventsField_BoldMenuItem.Click += BoldMenuItem_Click;
            withEventsField_ItalicMenuItem.Click += ItalicMenuItem_Click;
            withEventsField_UnderlineMenuItem.Click += UnderlineMenuItem_Click;
            withEventsField_StrikethroughMenuItem.Click += StrikethroughMenuItem_Click;
            withEventsField_SubscriptMenuItem.Click += SubscriptMenuItem_Click;
            withEventsField_SuperscriptMenuItem.Click += SuperscriptMenuItem_Click;
            withEventsField_IndentMoreMenuItem.Click += IndentMoreMenuItem_Click;
            withEventsField_IndentLessMenuItem.Click += IndentLessMenuItem_Click;
            withEventsField_BulletListMenuItem.Click += BulletListMenuItem_Click;
            withEventsField_NumberListMenuItem.Click += NumberListMenuItem_Click;
            withEventsField_AlignLeftMenuItem.Click += AlignLeftMenuItem_Click;
            withEventsField_AlignCenterMenuItem.Click += AlignCenterMenuItem_Click;
            withEventsField_AlignRightMenuItem.Click += AlignRightMenuItem_Click;
            withEventsField_AlignJustifyMenuItem.Click += AlignJustifyMenuItem_Click;
            withEventsField_LineSpacingMenuItem.Click += LineSpacingMenuItem_Click;
            withEventsField_LefttoRightMenuItem.Click += LefttoRightMenuItem_Click;
            withEventsField_RighttoLeftMenuItem.Click += RighttoLeftMenuItem_Click;
            withEventsField_UndoMenuItem.Click += UndoMenuItem_Click;
            withEventsField_RedoMenuItem.Click += RedoMenuItem_Click;
            withEventsField_CutMenuItem.Click += CutMenuItem_Click;
            withEventsField_CopyMenuItem.Click += CopyMenuItem_Click;
            withEventsField_PasteMenuItem.Click += PasteMenuItem_Click;
            withEventsField_DeleteMenuItem.Click += DeleteMenuItem_Click;
            withEventsField_SelectAllMenuItem.Click += SelectAllMenuItem_Click;
            withEventsField_FindMenuItem.Click += FindMenuItem_Click;
            withEventsField_ReplaceMenuItem.Click += ReplaceMenuItem_Click;
            withEventsField_GoToMenuItem.Click += GoToMenuItem_Click;
            withEventsField_UppercaseMenuItem.Click += UppercaseMenuItem_Click;
            withEventsField_LowercaseMenuItem.Click += LowercaseMenuItem_Click;
            _EventsSubscr = true;
        }

        public void UnSubscribeEvents()
        {
            if (!_EventsSubscr)
                return;
            withEventsField_Editor.SelectionChanged -= TBox_SelectionChanged;
            withEventsField_Editor.UnSubscribeEvents();
            withEventsField_ObjectMenuItem.Click -= ObjectMenuItem_Click;
            withEventsField_ShapeMenuItem.Click -= ShapeMenuItem_Click;
            withEventsField_ImageMenuItem.Click -= ImageMenuItem_Click;
            withEventsField_LinkMenuItem.Click -= LinkMenuItem_Click;
            withEventsField_FlowDocumentMenuItem.Click -= FlowDocumentMenuItem_Click;
            withEventsField_RichTextFileMenuItem.Click -= RichTextFileMenuItem_Click;
            withEventsField_TextFileMenuItem.Click -= TextFileMenuItem_Click;
            withEventsField_SymbolMenuItem.Click -= SymbolMenuItem_Click;
            withEventsField_TableMenuItem.Click -= TableMenuItem_Click;
            withEventsField_VideoMenuItem.Click -= VideoMenuItem_Click;
            withEventsField_HorizontalLineMenuItem.Click -= HorizontalLineMenuItem_Click;
            withEventsField_HeaderMenuItem.Click -= HeaderMenuItem_Click;
            withEventsField_FooterMenuItem.Click -= FooterMenuItem_Click;
            withEventsField_DateMenuItem.Click -= DateMenuItem_Click;
            withEventsField_TimeMenuItem.Click -= TimeMenuItem_Click;
            withEventsField_ClearFormattingMenuItem.Click -= ClearFormattingMenuItem_Click;
            withEventsField_FontMenuItem.Click -= FontMenuItem_Click;
            withEventsField_FontSizeMenuItem.Click -= FontSizeMenuItem_Click;
            withEventsField_FontColorMenuItem.Click -= FontColorMenuItem_Click;
            withEventsField_HighlightMenuItem.Click -= HighlightMenuItem_Click;
            withEventsField_BoldMenuItem.Click -= BoldMenuItem_Click;
            withEventsField_ItalicMenuItem.Click -= ItalicMenuItem_Click;
            withEventsField_UnderlineMenuItem.Click -= UnderlineMenuItem_Click;
            withEventsField_StrikethroughMenuItem.Click -= StrikethroughMenuItem_Click;
            withEventsField_SubscriptMenuItem.Click -= SubscriptMenuItem_Click;
            withEventsField_SuperscriptMenuItem.Click -= SuperscriptMenuItem_Click;
            withEventsField_IndentMoreMenuItem.Click -= IndentMoreMenuItem_Click;
            withEventsField_IndentLessMenuItem.Click -= IndentLessMenuItem_Click;
            withEventsField_BulletListMenuItem.Click -= BulletListMenuItem_Click;
            withEventsField_NumberListMenuItem.Click -= NumberListMenuItem_Click;
            withEventsField_AlignLeftMenuItem.Click -= AlignLeftMenuItem_Click;
            withEventsField_AlignCenterMenuItem.Click -= AlignCenterMenuItem_Click;
            withEventsField_AlignRightMenuItem.Click -= AlignRightMenuItem_Click;
            withEventsField_AlignJustifyMenuItem.Click -= AlignJustifyMenuItem_Click;
            withEventsField_LineSpacingMenuItem.Click -= LineSpacingMenuItem_Click;
            withEventsField_LefttoRightMenuItem.Click -= LefttoRightMenuItem_Click;
            withEventsField_RighttoLeftMenuItem.Click -= RighttoLeftMenuItem_Click;
            withEventsField_UndoMenuItem.Click -= UndoMenuItem_Click;
            withEventsField_RedoMenuItem.Click -= RedoMenuItem_Click;
            withEventsField_CutMenuItem.Click -= CutMenuItem_Click;
            withEventsField_CopyMenuItem.Click -= CopyMenuItem_Click;
            withEventsField_PasteMenuItem.Click -= PasteMenuItem_Click;
            withEventsField_DeleteMenuItem.Click -= DeleteMenuItem_Click;
            withEventsField_SelectAllMenuItem.Click -= SelectAllMenuItem_Click;
            withEventsField_FindMenuItem.Click -= FindMenuItem_Click;
            withEventsField_ReplaceMenuItem.Click -= ReplaceMenuItem_Click;
            withEventsField_GoToMenuItem.Click -= GoToMenuItem_Click;
            withEventsField_UppercaseMenuItem.Click -= UppercaseMenuItem_Click;
            withEventsField_LowercaseMenuItem.Click -= LowercaseMenuItem_Click;
            _EventsSubscr = false;
        }

        #endregion

        #region "Header"

        public void SetFileType(string s)
        {
            BitmapImage img = new BitmapImage();
            Fluent.ScreenTip tip = new Fluent.ScreenTip();
            tip.Title = DocName;
            img.BeginInit();
            if (s.ToLower() == ".xaml")
            {
                HeaderContent.TabToolTip.Text = "FlowDocument";
                HeaderContent.TabToolTip.Image = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Tab/xaml48.png", UriKind.Relative));
                img.UriSource = new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Tab/xaml16.png");
            }
            else if (s.ToLower() == ".rtf")
            {
                HeaderContent.TabToolTip.Text = "Rich Text Document";
                HeaderContent.TabToolTip.Image = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Tab/rtf48.png", UriKind.Relative));
                img.UriSource = new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Tab/rtf16.png");
            }
            else if (s.ToLower() == ".html" || s.ToLower().ToLower() == ".htm")
            {
                HeaderContent.TabToolTip.Text = "HTML Document";
                HeaderContent.TabToolTip.Image = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Tab/html48.png", UriKind.Relative));
                img.UriSource = new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Tab/html16.png");
            }
            else
            {
                HeaderContent.TabToolTip.Text = "Text Document";
                HeaderContent.TabToolTip.Image = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Tab/txt48.png", UriKind.Relative));
                img.UriSource = new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Tab/txt16.png");
            }
            img.EndInit();
            HeaderContent.FileTypeImage.Source = img;
            HeaderContent.FileTypeImage.ToolTip = s;
            FileFormat = s;
        }

        public void SetDocumentTitle(string title)
        {
            if (Editor.IsReadOnly)
            {
                HeaderContent.TabTitle.Text = title + " (Read-only)";
            }
            else
            {
                HeaderContent.TabTitle.Text = title;
            }
            DocName = title;
        }

        public void ShowCloseButton()
        {
            //if (gip.core.reporthandler.Properties.Settings.Default.Options_Tabs_CloseButtonMode == 0)
            if (true)
            {
                HeaderContent.CloseButton.Visibility = System.Windows.Visibility.Visible;
            }
            //else
            //{
            //    if (this.IsSelected)
            //    {
            //        HeaderContent.SetMenu(true);
            //        HeaderContent.CloseButton.Visibility = System.Windows.Visibility.Visible;
            //    }
            //    else
            //    {
            //        HeaderContent.SetMenu(false);
            //        HeaderContent.CloseButton.Visibility = System.Windows.Visibility.Hidden;
            //    }
            //}
        }

        #endregion

        #region "TBox"

        #region "Context Menu"

        private void TBoxContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Editor.CanUndo)
            {
                UndoMenuItem.IsEnabled = true;
            }
            else
            {
                UndoMenuItem.IsEnabled = false;
            }
            if (Editor.CanRedo)
            {
                RedoMenuItem.IsEnabled = true;
            }
            else
            {
                RedoMenuItem.IsEnabled = false;
            }
            if (Clipboard.ContainsText())
            {
                PasteMenuItem.IsEnabled = true;
            }
            else
            {
                PasteMenuItem.IsEnabled = false;
            }
        }

        private void ObjectMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertObjectEvent));
        }

        private void ShapeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertShapeEvent));
        }

        private void ImageMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertImageEvent));
        }

        private void LinkMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertLinkEvent));
        }

        private void FlowDocumentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertFlowDocumentEvent));
        }

        private void RichTextFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertRichTextFileEvent));
        }

        private void TextFileMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertTextFileEvent));
        }

        private void SymbolMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertSymbolEvent));
        }

        private void TableMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertTableEvent));
        }

        private void VideoMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertVideoEvent));
        }

        private void HorizontalLineMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertHorizontalLineEvent));
        }

        private void HeaderMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertHeaderEvent));
        }

        private void FooterMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertFooterEvent));
        }

        private void DateMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertDateEvent));
        }

        private void TimeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(InsertTimeEvent));
        }

        private void ClearFormattingMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(ClearFormattingEvent));
        }

        private void FontMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(FontEvent));
        }

        private void FontSizeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(FontSizeEvent));
        }

        private void FontColorMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(FontColorEvent));
        }

        private void HighlightMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(HighlightColorEvent));
        }

        private void BoldMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(BoldEvent));
        }

        private void ItalicMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(ItalicEvent));
        }

        private void UnderlineMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(UnderlineEvent));
        }

        private void StrikethroughMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(StrikethroughEvent));
        }

        private void SubscriptMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(SubscriptEvent));
        }

        private void SuperscriptMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(SuperscriptEvent));
        }

        private void IndentMoreMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(IndentMoreEvent));
        }

        private void IndentLessMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(IndentLessEvent));
        }

        private void BulletListMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(BulletListEvent));
        }

        private void NumberListMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(NumberListEvent));
        }

        private void AlignLeftMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(AlignLeftEvent));
        }

        private void AlignCenterMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(AlignCenterEvent));
        }

        private void AlignRightMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(AlignRightEvent));
        }

        private void AlignJustifyMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(AlignJustifyEvent));
        }

        private void LineSpacingMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(LineSpacingEvent));
        }

        private void LefttoRightMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(LeftToRightEvent));
        }

        private void RighttoLeftMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(RightToLeftEvent));
        }

        private void UndoMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(UndoEvent));
        }

        private void RedoMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(RedoEvent));
        }

        private void CutMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CutEvent));
        }

        private void CopyMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CopyEvent));
        }

        private void PasteMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(PasteEvent));
        }

        private void DeleteMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(DeleteEvent));
        }

        private void SelectAllMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(SelectAllEvent));
        }

        private void FindMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(FindEvent));
        }

        private void ReplaceMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(ReplaceEvent));
        }

        private void GoToMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(GoToEvent));
        }

        private void UppercaseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Editor.Selection.Text = Editor.Selection.Text.ToUpper();
        }

        private void LowercaseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Editor.Selection.Text = Editor.Selection.Text.ToLower();
        }

        #endregion

        private void TBox_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(UpdateSelected));
        }

        #endregion

    }

}
