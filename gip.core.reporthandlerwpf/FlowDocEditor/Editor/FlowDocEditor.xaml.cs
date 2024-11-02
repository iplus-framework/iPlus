// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Data;
using System.Diagnostics;
//using System.Speech;
//using System.Speech.Synthesis;
using System.Windows.Shell;
using System.IO;
using Fluent;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xml;
using gip.core.datamodel;
using gip.core.layoutengine;
using System.ComponentModel;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für FlowDocEditor.xaml
    /// </summary>
    public partial class FlowDocEditor : UserControl, INotifyPropertyChanged
    {
        public FlowDocEditor()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Initialized += MainWindow_Initialized;
            KeyDown += MainWindow_KeyDown;
            withEventsField_DocPreviewScrollViewer.Loaded += DocPreviewScrollViewer_Loaded;
        }

        public DocumentTab _SelectedDocument;
        public DocumentTab SelectedDocument
        {
            get => _SelectedDocument;
            set
            {
                _SelectedDocument = value;
                OnPropertyChanged("SelectedDocument");
            }
        }
        //private SpeechSynthesizer Speech = new SpeechSynthesizer();
        private JumpList myJumpList = new JumpList();

        private DocumentEditor DocumentPreview = new DocumentEditor();
        private ScrollViewer withEventsField_DocPreviewScrollViewer = new ScrollViewer();
        private ScrollViewer DocPreviewScrollViewer
        {
            get { return withEventsField_DocPreviewScrollViewer; }
            set
            {
                if (withEventsField_DocPreviewScrollViewer != null)
                {
                    withEventsField_DocPreviewScrollViewer.Loaded -= DocPreviewScrollViewer_Loaded;
                }
                withEventsField_DocPreviewScrollViewer = value;
                if (withEventsField_DocPreviewScrollViewer != null)
                {
                    withEventsField_DocPreviewScrollViewer.Loaded += DocPreviewScrollViewer_Loaded;
                }
            }
        }
        #region "Reuseable Code"

        private void UpdateDocumentPreview()
        {
            TextRange range = new TextRange(SelectedDocument.Editor.Document.ContentStart, SelectedDocument.Editor.Document.ContentEnd);
            MemoryStream stream = new MemoryStream();
            XamlWriter.Save(range, stream);
            range.Save(stream, DataFormats.XamlPackage, true);
            FlowDocument previewdoc = new FlowDocument();
            TextRange range2 = new TextRange(previewdoc.ContentEnd, previewdoc.ContentEnd);
            range2.Load(stream, DataFormats.XamlPackage);
            //TODO: (2013.xx) set background color for preview document
            DocumentPreview.Document.PageWidth = SelectedDocument.Editor.Document.PageWidth;
            DocumentPreview.Document.PageHeight = SelectedDocument.Editor.Document.PageHeight;
            DocumentPreview.Width = DocumentPreview.Document.PageWidth;
            DocumentPreview.Height = DocumentPreview.Document.PageHeight;
            DocumentPreview.Document = previewdoc;
            DocumentPreview.Document.PagePadding = SelectedDocument.Editor.docpadding;
            DocumentPreview.InvalidateVisual();
            DocumentPreview.UpdateLayout();
            Canvas c = DocPreviewScrollViewer.Content as Canvas;
            if (c.Children.Count == 0)
            {
                c.Children.Add(DocumentPreview);
            }
            DocumentPreview.InvalidateVisual();
            DocumentPreview.UpdateLayout();
            DocPreviewScrollViewer.Content = c;
        }

        #region "Document

        public FlowDocument Document
        {
            get
            {
                if (TabCell.Items.Count <= 0)
                    return null;
                return SelectedDocument.Editor.Document;
            }
            set
            {
                if (TabCell.Items.Count <= 0)
                {
                    NewDocument("Design",value);
                }
                else if (SelectedDocument != null)
                {
                    CloseDocument(SelectedDocument);
                    NewDocument("Design",value);
                }

                if (value != null)
                SelectedDocument.Editor.Document = value;
               
                SelectedDocument.HeaderContent.FileTypeImage.ToolTip = ".xaml";
            }
        }

        public void NewDocument(string title, FlowDocument flowDoc)
        {
            if (TabCell.Items.Count > 0)
            {
                SelectedDocument.IsSelected = false;
            }
            DocumentTab tb = new DocumentTab(title, Brushes.Transparent, flowDoc);
            tb.Ruler.Background = Background;
            TabCell.Items.Add(tb);
            ResizeTabs(TabCell);
            foreach (DocumentTab t in TabCell.Items)
            {
                t.HeaderContent.Padding = new Thickness(4, 5, 4, 5);
                t.HeaderContent.FileTypeImage.Margin = new Thickness(0, 1, 0, 0);
                t.HeaderContent.TabTitle.Margin = new Thickness(0, 0, 0, 0);
                t.HeaderContent.CloseButton.Margin = new Thickness(0, 1, 0, 0);
            }
            UpdateUI();
            tb.IsSelected = true;
            SelectedDocument.Editor.Focus();
            SelectedDocument.Editor.FileChanged = false;
            UpdateButtons();
        }

        private void CloseDocument(DocumentTab file)
        {
            file.UnSubscribeEvents();
            TabCell.Items.Remove(file);
            ResizeTabs(TabCell);
            UpdateUI();
        }

        #endregion

        private void ResizeTabs(TabControl Tab)
        {
            if (gip.core.reporthandlerwpf.Properties.Settings.Default.Options_Tabs_SizeMode == 0)
            {
                foreach (DocumentTab i in Tab.Items)
                {
                    if (TabCell.Items.Count > 8)
                    {
                        double d = new double();
                        double d2 = new double();
                        d = TabCell.ActualWidth - 8;
                        d2 = TabCell.Items.Count * 2;
                        i.Width = d / d2;
                    }
                    else
                    {
                        double d = new double();
                        d = Tab.ActualWidth;
                        i.Width = d / 8;
                    }
                }
            }
        }

        private void RunPlugin(object sender, System.Windows.RoutedEventArgs e)
        {
            //Fluent.Button p = e.Source as Fluent.Button;
            //Plugins plugins = new Plugins();
            //object i = plugins.Build(p.Header, My.Computer.FileSystem.ReadAllText(p.Tag));
            //if (i.GetType.Name.ToString == Const.TNameString)
            //{
            //    SelectedDocument.Editor.CaretPosition.InsertTextInRun(i);
            //}
        }

        #region "RibbonBar"

        private void UpdateSelected()
        {
            UpdateContextualTabs();
            MainBar.Title = SelectedDocument.DocName;
            if (File.Exists(SelectedDocument.Editor.DocumentName))
            {
                FileInfo info = new FileInfo(SelectedDocument.Editor.DocumentName);
                double kb = info.Length / 1024;
                int inter = Convert.ToInt32(kb);
                FileSizeTextBlock.Text = "Size: " + inter.ToString() + " KB";
            }
            else
            {
                FileSizeTextBlock.Text = "Size: " + "0 KB";
            }
            //LinesTextBlock.Text = "Line: " + SelectedDocument.Editor.SelectedLineNumber.ToString() + " of " + SelectedDocument.Editor.LineCount.ToString();
            //ColumnsTextBlock.Text = "Column: " + SelectedDocument.Editor.SelectedColumnNumber.ToString() + " of " + SelectedDocument.Editor.ColumnCount.ToString();
            //WordCountTextBlock.Text = "Word Count: " + SelectedDocument.Editor.WordCount.ToString();
            if (SelectedDocument.Editor.CanUndo)
            {
                UndoButton.IsEnabled = true;
            }
            else
            {
                UndoButton.IsEnabled = false;
            }
            if (SelectedDocument.Editor.CanRedo)
            {
                RedoButton.IsEnabled = true;
            }
            else
            {
                RedoButton.IsEnabled = false;
            }
            if (Clipboard.ContainsText() || Clipboard.ContainsImage())
            {
                PasteButton.IsEnabled = true;
                if (Clipboard.ContainsText())
                {
                    PasteTextButton.IsEnabled = true;
                }
                else
                {
                    PasteTextButton.IsEnabled = false;
                }
                if (Clipboard.ContainsImage())
                {
                    PasteImageMenuItem.IsEnabled = true;
                }
                else
                {
                    PasteImageMenuItem.IsEnabled = false;
                }
            }
            else
            {
                PasteButton.IsEnabled = false;
                PasteTextButton.IsEnabled = false;
                PasteImageMenuItem.IsEnabled = false;
            }

            UpdatingFont = true;
            if (FontComboBox.IsLoaded)
            {
                object value = SelectedDocument.Editor.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
                FontFamily currentfontfamily = value as FontFamily;
                if (currentfontfamily != null)
                {
                    FontComboBox.SelectedItem = currentfontfamily;
                }
            }
            if (FontSizeComboBox.IsLoaded)
            {
                try
                {
                    double sizevalue = Convert.ToDouble(SelectedDocument.Editor.Selection.GetPropertyValue(TextElement.FontSizeProperty));
                    FontSizeComboBox.SelectedValue = sizevalue;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    this.Root().Messages.LogException("Document.Editor.FlowDocEditor", "UpdateSelected", msg);
                }
            }
            UpdatingFont = false;
            Run r = SelectedDocument.Editor.CaretPosition.Parent as Run;
            if (r != null)
            {
                if (r.FontWeight == FontWeights.Bold)
                {
                    BoldButton.IsChecked = true;
                }
                else
                {
                    BoldButton.IsChecked = false;
                }
                if (r.FontStyle == FontStyles.Italic)
                {
                    ItalicButton.IsChecked = true;
                }
                else
                {
                    ItalicButton.IsChecked = false;
                }
                TextDecorationCollection td = SelectedDocument.Editor.Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
                UnderlineButton.IsChecked = false;
                StrikethroughButton.IsChecked = false;
                if (object.ReferenceEquals(td, TextDecorations.Underline))
                {
                    UnderlineButton.IsChecked = true;
                }
                if (object.ReferenceEquals(td, TextDecorations.Strikethrough))
                {
                    StrikethroughButton.IsChecked = true;
                }
                if (r.BaselineAlignment == BaselineAlignment.Subscript)
                {
                    SubscriptButton.IsChecked = true;
                    SuperscriptButton.IsChecked = false;
                }
                else if (r.BaselineAlignment == BaselineAlignment.Superscript)
                {
                    SubscriptButton.IsChecked = false;
                    SuperscriptButton.IsChecked = true;
                }
                else
                {
                    SubscriptButton.IsChecked = false;
                    SuperscriptButton.IsChecked = false;
                }
                Paragraph runparent = r.Parent as Paragraph;
                if (runparent != null)
                {
                    if (runparent.TextAlignment == TextAlignment.Left)
                    {
                        AlignLeftButton.IsChecked = true;
                        AlignCenterButton.IsChecked = false;
                        AlignRightButton.IsChecked = false;
                        AlignJustifyButton.IsChecked = false;
                    }
                    else if (runparent.TextAlignment == TextAlignment.Center)
                    {
                        AlignLeftButton.IsChecked = false;
                        AlignCenterButton.IsChecked = true;
                        AlignRightButton.IsChecked = false;
                        AlignJustifyButton.IsChecked = false;
                    }
                    else if (runparent.TextAlignment == TextAlignment.Right)
                    {
                        AlignLeftButton.IsChecked = false;
                        AlignCenterButton.IsChecked = false;
                        AlignRightButton.IsChecked = true;
                        AlignJustifyButton.IsChecked = false;
                    }
                    else if (runparent.TextAlignment == TextAlignment.Justify)
                    {
                        AlignLeftButton.IsChecked = false;
                        AlignCenterButton.IsChecked = false;
                        AlignRightButton.IsChecked = false;
                        AlignJustifyButton.IsChecked = true;
                    }
                    ListItem listitem = runparent.Parent as ListItem;
                    if (listitem != null)
                    {
                        List list = listitem.Parent as List;
                        if (list != null)
                        {
                            if (list.MarkerStyle == TextMarkerStyle.Disc || list.MarkerStyle == TextMarkerStyle.Circle || list.MarkerStyle == TextMarkerStyle.Box || list.MarkerStyle == TextMarkerStyle.Square)
                            {
                                BulletListButton.IsChecked = true;
                            }
                            else
                            {
                                BulletListButton.IsChecked = false;
                            }
                            if (list.MarkerStyle == TextMarkerStyle.Decimal || list.MarkerStyle == TextMarkerStyle.UpperLatin || list.MarkerStyle == TextMarkerStyle.LowerLatin || list.MarkerStyle == TextMarkerStyle.UpperRoman || list.MarkerStyle == TextMarkerStyle.LowerRoman)
                            {
                                NumberListButton.IsChecked = true;
                            }
                            else
                            {
                                NumberListButton.IsChecked = false;
                            }
                        }
                        else
                        {
                            BulletListButton.IsChecked = false;
                            NumberListButton.IsChecked = false;
                        }
                    }
                    else
                    {
                        BulletListButton.IsChecked = false;
                        NumberListButton.IsChecked = false;
                    }
                }
            }
            else
            {
                Paragraph p = SelectedDocument.Editor.CaretPosition.Parent as Paragraph;
                if (p != null)
                {
                    if (p.FontWeight == FontWeights.Bold)
                    {
                        BoldButton.IsChecked = true;
                    }
                    else
                    {
                        BoldButton.IsChecked = false;
                    }
                    if (p.FontStyle == FontStyles.Italic)
                    {
                        ItalicButton.IsChecked = true;
                    }
                    else
                    {
                        ItalicButton.IsChecked = false;
                    }
                    TextDecorationCollection td = SelectedDocument.Editor.Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
                    UnderlineButton.IsChecked = false;
                    StrikethroughButton.IsChecked = false;
                    if (td == TextDecorations.Underline)
                    {
                        UnderlineButton.IsChecked = true;
                    }
                    if (td == TextDecorations.Strikethrough)
                    {
                        StrikethroughButton.IsChecked = true;
                    }
                    Paragraph runparent = p as Paragraph;
                    if (runparent != null)
                    {
                        if (runparent.TextAlignment == TextAlignment.Left)
                        {
                            AlignLeftButton.IsChecked = true;
                            AlignCenterButton.IsChecked = false;
                            AlignRightButton.IsChecked = false;
                            AlignJustifyButton.IsChecked = false;
                        }
                        else if (runparent.TextAlignment == TextAlignment.Center)
                        {
                            AlignLeftButton.IsChecked = false;
                            AlignCenterButton.IsChecked = true;
                            AlignRightButton.IsChecked = false;
                            AlignJustifyButton.IsChecked = false;
                        }
                        else if (runparent.TextAlignment == TextAlignment.Right)
                        {
                            AlignLeftButton.IsChecked = false;
                            AlignCenterButton.IsChecked = false;
                            AlignRightButton.IsChecked = true;
                            AlignJustifyButton.IsChecked = false;
                        }
                        else if (runparent.TextAlignment == TextAlignment.Justify)
                        {
                            AlignLeftButton.IsChecked = false;
                            AlignCenterButton.IsChecked = false;
                            AlignRightButton.IsChecked = false;
                            AlignJustifyButton.IsChecked = true;
                        }
                    }
                    ListItem listitem = p.Parent as ListItem;
                    if (listitem != null)
                    {
                        List list = listitem.Parent as List;
                        if (list != null)
                        {
                            if (list.MarkerStyle == TextMarkerStyle.Disc || list.MarkerStyle == TextMarkerStyle.Circle || list.MarkerStyle == TextMarkerStyle.Box || list.MarkerStyle == TextMarkerStyle.Square)
                            {
                                BulletListButton.IsChecked = true;
                            }
                            else
                            {
                                BulletListButton.IsChecked = false;
                            }
                            if (list.MarkerStyle == TextMarkerStyle.Decimal || list.MarkerStyle == TextMarkerStyle.UpperLatin || list.MarkerStyle == TextMarkerStyle.LowerLatin || list.MarkerStyle == TextMarkerStyle.UpperRoman || list.MarkerStyle == TextMarkerStyle.LowerRoman)
                            {
                                NumberListButton.IsChecked = true;
                            }
                            else
                            {
                                NumberListButton.IsChecked = false;
                            }
                        }
                        else
                        {
                            BulletListButton.IsChecked = false;
                            NumberListButton.IsChecked = false;
                        }
                    }
                    else
                    {
                        BulletListButton.IsChecked = false;
                        NumberListButton.IsChecked = false;
                    }
                }
                else
                {
                    BulletListButton.IsChecked = false;
                    NumberListButton.IsChecked = false;
                }
            }
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
            {
                LineSpacing1Point0.IsChecked = false;
                LineSpacing1Point15.IsChecked = false;
                LineSpacing1Point5.IsChecked = false;
                LineSpacing2Point0.IsChecked = false;
                LineSpacing2Point5.IsChecked = false;
                LineSpacing3Point0.IsChecked = false;
                if (SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight == 1.0)
                {
                    LineSpacing1Point0.IsChecked = true;
                }
                else if (SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight == 1.15)
                {
                    LineSpacing1Point15.IsChecked = true;
                }
                else if (SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight == 1.5)
                {
                    LineSpacing1Point5.IsChecked = true;
                }
                else if (SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight == 2.0)
                {
                    LineSpacing2Point0.IsChecked = true;
                }
                else if (SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight == 2.5)
                {
                    LineSpacing2Point5.IsChecked = true;
                }
                else if (SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight == 3.0)
                {
                    LineSpacing3Point0.IsChecked = true;
                }
            }
        }

        private void UpdateButtons(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (!(TabCell.SelectedIndex == -1))
            {
                if (SelectedDocument.Editor.Document.Blocks.Count == 0)
                {
                    SelectedDocument.Editor.Document.Blocks.Add(new Paragraph());
                }
                UpdateSelected();
                foreach (DocumentTab i in TabCell.Items)
                {
                    i.ShowCloseButton();
                }
                BackgroundColorMenuItem.IsEnabled = true;
                CommonEditGroup.IsEnabled = true;
                CommonViewGroup.IsEnabled = true;
                zoomSlider.Value = SelectedDocument.Editor.ZoomLevel * 100;
                ZoomGroup.IsEnabled = true;
                ZoomInButton.IsEnabled = true;
                CommonZoomInButton.IsEnabled = true;
                if (zoomSlider.Value > 20)
                {
                    ZoomOutButton.IsEnabled = true;
                    CommonZoomOutButton.IsEnabled = true;
                }
                else
                {
                    ZoomOutButton.IsEnabled = false;
                    CommonZoomOutButton.IsEnabled = false;
                }
                if (zoomSlider.Value < 500)
                {
                    ZoomInButton.IsEnabled = true;
                    CommonZoomInButton.IsEnabled = true;
                }
                else
                {
                    ZoomInButton.IsEnabled = false;
                    CommonZoomInButton.IsEnabled = false;
                }
                ResetZoomButton.IsEnabled = true;
                CommonResetZoomButton.IsEnabled = true;
                zoomSlider.IsEnabled = true;
                InsertTab.Visibility = Visibility.Visible;
                TableButton.IsEnabled = true;
                DateButton.IsEnabled = true;
                TimeButton.IsEnabled = true;
                ImageButton.IsEnabled = true;
                VideoButton.IsEnabled = true;
                ObjectButton.IsEnabled = true;
                ShapeButton.IsEnabled = true;
                LinkButton.IsEnabled = true;
                TextFileButton.IsEnabled = true;
                HorizontalLineButton.IsEnabled = true;
                HeaderButton.IsEnabled = true;
                FooterButton.IsEnabled = true;
                ClearFormattingButton.IsEnabled = true;
                FontColorButton.IsEnabled = true;
                HighlightColorButton.IsEnabled = true;
                BoldButton.IsEnabled = true;
                ItalicButton.IsEnabled = true;
                UnderlineButton.IsEnabled = true;
                StrikethroughButton.IsEnabled = true;
                SubscriptButton.IsEnabled = true;
                SuperscriptButton.IsEnabled = true;
                IndentMoreButton.IsEnabled = true;
                IndentLessButton.IsEnabled = true;
                BulletListButton.IsEnabled = true;
                NumberListButton.IsEnabled = true;
                AlignLeftButton.IsEnabled = true;
                AlignCenterButton.IsEnabled = true;
                AlignRightButton.IsEnabled = true;
                AlignJustifyButton.IsEnabled = true;
                LineSpacingButton.IsEnabled = true;
                LeftToRightButton.IsEnabled = true;
                RightToLeftButton.IsEnabled = true;
                PageLayoutTab.Visibility = Visibility.Visible;
                LeftMarginBox.Value = SelectedDocument.Editor.docpadding.Left;
                TopMarginBox.Value = SelectedDocument.Editor.docpadding.Top;
                RightMarginBox.Value = SelectedDocument.Editor.docpadding.Right;
                BottomMarginBox.Value = SelectedDocument.Editor.docpadding.Bottom;
                PageHeightBox.Value = SelectedDocument.Editor.Document.PageHeight;
                PageWidthBox.Value = SelectedDocument.Editor.Document.PageWidth;
                NavigationTab.Visibility = Visibility.Visible;
                LineDownButton.IsEnabled = true;
                LineUpButton.IsEnabled = true;
                LineLeftButton.IsEnabled = true;
                LineRightButton.IsEnabled = true;
                PageDownButton.IsEnabled = true;
                PageUpButton.IsEnabled = true;
                PageLeftButton.IsEnabled = true;
                PageRightButton.IsEnabled = true;
                StartButton.IsEnabled = true;
                EndButton.IsEnabled = true;
                CommonEditGroup.IsEnabled = true;
                CommonInsertGroup.IsEnabled = true;
                CommonFormatGroup.IsEnabled = true;
                CommonToolsGroup.IsEnabled = true;
                SpellCheckButton.IsEnabled = true;
                TextToSpeechButton.IsEnabled = true;
                TranslateButton.IsEnabled = true;
                DefinitionsButton.IsEnabled = true;
                if (StatusbarButton.IsChecked.Value)
                {
                    StatusBar.Visibility = Visibility.Visible;
                }
            }
            else if (TabCell.SelectedIndex == -1)
            {
                SelectedDocument = null;
                MainBar.Title = Process.GetCurrentProcess().ProcessName;
                MainBar.SelectedTabIndex = 0;
                BackgroundColorMenuItem.IsEnabled = false;
                CommonEditGroup.IsEnabled = false;
                CommonViewGroup.IsEnabled = false;
                ZoomGroup.IsEnabled = false;
                ZoomInButton.IsEnabled = false;
                CommonZoomInButton.IsEnabled = false;
                ZoomOutButton.IsEnabled = false;
                CommonZoomOutButton.IsEnabled = false;
                ResetZoomButton.IsEnabled = false;
                CommonResetZoomButton.IsEnabled = false;
                zoomSlider.IsEnabled = false;
                InsertTab.Visibility = Visibility.Collapsed;
                TableButton.IsEnabled = false;
                DateButton.IsEnabled = false;
                TimeButton.IsEnabled = false;
                ImageButton.IsEnabled = false;
                VideoButton.IsEnabled = false;
                ObjectButton.IsEnabled = false;
                ShapeButton.IsEnabled = false;
                LinkButton.IsEnabled = false;
                TextFileButton.IsEnabled = false;
                HorizontalLineButton.IsEnabled = false;
                HeaderButton.IsEnabled = false;
                FooterButton.IsEnabled = false;
                ClearFormattingButton.IsEnabled = false;
                FontColorButton.IsEnabled = false;
                HighlightColorButton.IsEnabled = false;
                BoldButton.IsEnabled = false;
                BoldButton.IsChecked = false;
                ItalicButton.IsEnabled = false;
                ItalicButton.IsChecked = false;
                UnderlineButton.IsEnabled = false;
                UnderlineButton.IsChecked = false;
                StrikethroughButton.IsEnabled = false;
                StrikethroughButton.IsChecked = false;
                SubscriptButton.IsEnabled = false;
                SubscriptButton.IsChecked = false;
                SuperscriptButton.IsEnabled = false;
                SuperscriptButton.IsChecked = false;
                IndentMoreButton.IsEnabled = false;
                IndentLessButton.IsEnabled = false;
                BulletListButton.IsEnabled = false;
                BulletListButton.IsChecked = false;
                NumberListButton.IsEnabled = false;
                NumberListButton.IsChecked = false;
                AlignLeftButton.IsEnabled = false;
                AlignLeftButton.IsChecked = false;
                AlignCenterButton.IsEnabled = false;
                AlignCenterButton.IsChecked = false;
                AlignRightButton.IsEnabled = false;
                AlignRightButton.IsChecked = false;
                AlignJustifyButton.IsEnabled = false;
                AlignJustifyButton.IsChecked = false;
                LineSpacingButton.IsEnabled = false;
                LeftToRightButton.IsEnabled = false;
                RightToLeftButton.IsEnabled = false;
                PageLayoutTab.Visibility = Visibility.Collapsed;
                NavigationTab.Visibility = Visibility.Collapsed;
                LineDownButton.IsEnabled = false;
                LineUpButton.IsEnabled = false;
                LineLeftButton.IsEnabled = false;
                LineRightButton.IsEnabled = false;
                PageDownButton.IsEnabled = false;
                PageUpButton.IsEnabled = false;
                PageLeftButton.IsEnabled = false;
                PageRightButton.IsEnabled = false;
                StartButton.IsEnabled = false;
                EndButton.IsEnabled = false;
                EditTableTab.Visibility = Visibility.Collapsed;
                EditTableCellTab.Visibility = Visibility.Collapsed;
                EditListTab.Visibility = Visibility.Collapsed;
                EditImageTab.Visibility = Visibility.Collapsed;
                EditVideoTab.Visibility = Visibility.Collapsed;
                EditObjectTab.Visibility = Visibility.Collapsed;
                CommonEditGroup.IsEnabled = false;
                CommonInsertGroup.IsEnabled = false;
                CommonFormatGroup.IsEnabled = false;
                CommonToolsGroup.IsEnabled = false;
                SpellCheckButton.IsEnabled = false;
                TextToSpeechButton.IsEnabled = false;
                TranslateButton.IsEnabled = false;
                DefinitionsButton.IsEnabled = false;
                StatusBar.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        public void UpdateUI()
        {
            if (SelectedDocument != null)
            {
                if (gip.core.reporthandlerwpf.Properties.Settings.Default.MainWindow_ShowRuler)
                {
                    if (TabCell.Items.Count >= 2)
                    {
                        foreach (DocumentTab t in TabCell.Items)
                        {
                            t.Ruler.Margin = new Thickness(-23, 2, 0, 0);
                            t.VSV.Margin = new Thickness(-6, 26, 0, 0);
                        }
                    }
                    else
                    {
                        foreach (DocumentTab t in TabCell.Items)
                        {
                            t.Ruler.Margin = new Thickness(-23, 0, 0, 0);
                            t.VSV.Margin = new Thickness(-6, 24, 0, 0);
                        }
                    }
                }
                else
                {
                    if (TabCell.Items.Count >= 2)
                    {
                        foreach (DocumentTab t in TabCell.Items)
                        {
                            t.VSV.Margin = new Thickness(-6, 0, 0, 0);
                        }
                    }
                    else
                    {
                        foreach (DocumentTab t in TabCell.Items)
                        {
                            t.VSV.Margin = new Thickness(-6, -1, 0, 0);
                        }
                    }
                }
            }
        }

        #endregion

        #region "MainWindow"

        #region "Add Handlers"

        private void addhandlers()
        {
            this.AddHandler(TabHeader.CloseTabEvent, new RoutedEventHandler(CloseDoc));
            this.AddHandler(DocumentTab.UpdateSelected, new RoutedEventHandler(UpdateButtons));
            this.AddHandler(DocumentTab.InsertObjectEvent, new RoutedEventHandler(ObjectButton_Click));
            this.AddHandler(DocumentTab.InsertShapeEvent, new RoutedEventHandler(ShapeButton_Click));
            this.AddHandler(DocumentTab.InsertImageEvent, new RoutedEventHandler(ImageButton_Click));
            this.AddHandler(DocumentTab.InsertLinkEvent, new RoutedEventHandler(LinkMenuItem_Click));
            this.AddHandler(DocumentTab.InsertFlowDocumentEvent, new RoutedEventHandler(InsertFlowDocumentButton_Click));
            this.AddHandler(DocumentTab.InsertRichTextFileEvent, new RoutedEventHandler(InsertRichTextDocumentButton_Click));
            this.AddHandler(DocumentTab.InsertTextFileEvent, new RoutedEventHandler(TextFileButton_Click));
            this.AddHandler(DocumentTab.InsertSymbolEvent, new RoutedEventHandler(SymbolContextMenuItem_Click));
            this.AddHandler(DocumentTab.InsertTableEvent, new RoutedEventHandler(TableMenuItem_Click));
            this.AddHandler(DocumentTab.InsertVideoEvent, new RoutedEventHandler(VideoButton_Click));
            this.AddHandler(DocumentTab.InsertHorizontalLineEvent, new RoutedEventHandler(HorizontalLineButton_Click));
            this.AddHandler(DocumentTab.InsertHeaderEvent, new RoutedEventHandler(HeaderButton_Click));
            this.AddHandler(DocumentTab.InsertFooterEvent, new RoutedEventHandler(FooterButton_Click));
            this.AddHandler(DocumentTab.InsertDateEvent, new RoutedEventHandler(DateMenuItem_Click));
            this.AddHandler(DocumentTab.InsertTimeEvent, new RoutedEventHandler(TimeMenuItem_Click));
            this.AddHandler(DocumentTab.ClearFormattingEvent, new RoutedEventHandler(ClearFormattingButton_Click));
            this.AddHandler(DocumentTab.FontEvent, new RoutedEventHandler(FontContextMenuItem_Click));
            this.AddHandler(DocumentTab.FontSizeEvent, new RoutedEventHandler(FontSizeContextMenuItem_Click));
            this.AddHandler(DocumentTab.FontColorEvent, new RoutedEventHandler(FontColorContextMenuItem_Click));
            this.AddHandler(DocumentTab.HighlightColorEvent, new RoutedEventHandler(FontHighlightColorContextMenuItem_Click));
            this.AddHandler(DocumentTab.BoldEvent, new RoutedEventHandler(BoldMenuItem_Click));
            this.AddHandler(DocumentTab.ItalicEvent, new RoutedEventHandler(ItalicMenuItem_Click));
            this.AddHandler(DocumentTab.UnderlineEvent, new RoutedEventHandler(UnderlineMenuItem_Click));
            this.AddHandler(DocumentTab.StrikethroughEvent, new RoutedEventHandler(StrikethroughButton_Click));
            this.AddHandler(DocumentTab.SubscriptEvent, new RoutedEventHandler(SubscriptButton_Click));
            this.AddHandler(DocumentTab.SuperscriptEvent, new RoutedEventHandler(SuperscriptButton_Click));
            this.AddHandler(DocumentTab.IndentMoreEvent, new RoutedEventHandler(IndentMoreButton_Click));
            this.AddHandler(DocumentTab.IndentLessEvent, new RoutedEventHandler(IndentLessButton_Click));
            this.AddHandler(DocumentTab.BulletListEvent, new RoutedEventHandler(BulletListMenuItem_Click));
            this.AddHandler(DocumentTab.NumberListEvent, new RoutedEventHandler(NumberListMenuItem_Click));
            this.AddHandler(DocumentTab.AlignLeftEvent, new RoutedEventHandler(AlignLeftMenuItem_Click));
            this.AddHandler(DocumentTab.AlignCenterEvent, new RoutedEventHandler(AlignCenterMenuItem_Click));
            this.AddHandler(DocumentTab.AlignRightEvent, new RoutedEventHandler(AlignRightMenuItem_Click));
            this.AddHandler(DocumentTab.AlignJustifyEvent, new RoutedEventHandler(AlignJustifyMenuItem_Click));
            this.AddHandler(DocumentTab.LineSpacingEvent, new RoutedEventHandler(CustomLineSpacingMenuItem_Click));
            this.AddHandler(DocumentTab.LeftToRightEvent, new RoutedEventHandler(LeftToRightButton_Click));
            this.AddHandler(DocumentTab.RightToLeftEvent, new RoutedEventHandler(RightToLeftButton_Click));
            this.AddHandler(DocumentTab.UndoEvent, new RoutedEventHandler(UndoMenuItem_Click));
            this.AddHandler(DocumentTab.RedoEvent, new RoutedEventHandler(RedoMenuItem_Click));
            this.AddHandler(DocumentTab.CutEvent, new RoutedEventHandler(CutMenuItem_Click));
            this.AddHandler(DocumentTab.CopyEvent, new RoutedEventHandler(CopyMenuItem_Click));
            this.AddHandler(DocumentTab.PasteEvent, new RoutedEventHandler(PasteMenuItem_Click));
            this.AddHandler(DocumentTab.DeleteEvent, new RoutedEventHandler(DeleteMenuItem_Click));
            this.AddHandler(DocumentTab.SelectAllEvent, new RoutedEventHandler(SelectAllMenuItem_Click));
            this.AddHandler(DocumentTab.FindEvent, new RoutedEventHandler(FindButton_Click));
            this.AddHandler(DocumentTab.ReplaceEvent, new RoutedEventHandler(ReplaceButton_Click));
            this.AddHandler(DocumentTab.GoToEvent, new RoutedEventHandler(GoToButton_Click));
        }

        private void SymbolContextMenuItem_Click(object sender, EventArgs e)
        {
            MainBar.SelectedTabItem = InsertTab;
            InsertSymbolButton.IsDropDownOpen = true;
        }

        private void FontContextMenuItem_Click(object sender, EventArgs e)
        {
            MainBar.SelectedTabItem = HomeTabItem;
            FontComboBox.IsDropDownOpen = true;
        }

        private void FontSizeContextMenuItem_Click(object sender, EventArgs e)
        {
            MainBar.SelectedTabItem = HomeTabItem;
            FontSizeComboBox.IsDropDownOpen = true;
        }

        private void FontColorContextMenuItem_Click(object sender, EventArgs e)
        {
            MainBar.SelectedTabItem = HomeTabItem;
            FontColorButton.IsDropDownOpen = true;
        }

        private void FontHighlightColorContextMenuItem_Click(object sender, EventArgs e)
        {
            MainBar.SelectedTabItem = HomeTabItem;
            HighlightColorButton.IsDropDownOpen = true;
        }

        #endregion

        #region "Activated"


        #endregion

        #region "Key Down"

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (TabCell.SelectedContent != null)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) && !Keyboard.IsKeyDown(Key.LeftShift))
                {
                    if (e.Key == Key.F)
                    {
                        e.Handled = true;
                        FindButton_Click(null, null);
                    }
                    else if (e.Key == Key.G)
                    {
                        e.Handled = true;
                        GoToButton_Click(null, null);
                    }
                    else if (e.Key == Key.H)
                    {
                        e.Handled = true;
                        ReplaceButton_Click(null, null);
                    }
                }
                if (Keyboard.IsKeyDown(Key.Insert))
                {
                    if (e.Key == Key.O)
                    {
                        e.Handled = true;
                        ObjectButton_Click(null, null);
                    }
                    else if (e.Key == Key.H)
                    {
                        e.Handled = true;
                        HorizontalLineButton_Click(null, null);
                    }
                    else if (e.Key == Key.I)
                    {
                        e.Handled = true;
                        ImageButton_Click(null, null);
                    }
                    else if (e.Key == Key.L)
                    {
                        e.Handled = true;
                        LinkMenuItem_Click(null, null);
                    }
                    else if (e.Key == Key.S)
                    {
                        e.Handled = true;
                        ShapeButton_Click(null, null);
                    }
                    else if (e.Key == Key.T)
                    {
                        e.Handled = true;
                        TableMenuItem_Click(null, null);
                    }
                    else if (e.Key == Key.V)
                    {
                        e.Handled = true;
                        VideoButton_Click(null, null);
                    }
                    else if (e.Key == Key.X)
                    {
                        e.Handled = true;
                        TextFileButton_Click(null, null);
                    }
                }
            }
        }

        #endregion

        #region "Initialized"

        private void MainWindow_Initialized(object sender, System.EventArgs e)
        {
            System.Threading.Thread t = new System.Threading.Thread(addhandlers);
            //Plugins plugins = new Plugins();
            t.Start();
        }

        #endregion

        #region "Loaded"

        private bool _Loaded = false;
        private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_Loaded)
                return;
            DocPreviewScrollViewer.Content = new Canvas();
            MainGrid.Children.Add(DocPreviewScrollViewer);
            FontComboBox.SelectedItem = gip.core.reporthandlerwpf.Properties.Settings.Default.Options_DefaultFont;
            var _with4 = FontSizeComboBox;
            _with4.Items.Add(Convert.ToDouble(8));
            _with4.Items.Add(Convert.ToDouble(9));
            _with4.Items.Add(Convert.ToDouble(10));
            _with4.Items.Add(Convert.ToDouble(11));
            for (int i = 12; i <= 28; i += 2)
            {
                _with4.Items.Add(Convert.ToDouble(i));
            }
            _with4.Items.Add(Convert.ToDouble(36));
            _with4.Items.Add(Convert.ToDouble(48));
            _with4.Items.Add(Convert.ToDouble(72));
            _with4.SelectedItem = gip.core.reporthandlerwpf.Properties.Settings.Default.Options_DefaultFontSize;
            if (ControlManager.WpfTheme == eWpfTheme.Gip)
            {
                LinesTextBlock.Foreground = Brushes.LightGray;
                LinesTextBlock1.Foreground = Brushes.LightGray;
                LinesTextBlock2.Foreground = Brushes.LightGray;
                LinesTextBlock3.Foreground = Brushes.LightGray;
                ColumnsTextBlock.Foreground = Brushes.LightGray;
                ColumnsTextBlock1.Foreground = Brushes.LightGray;
                ColumnsTextBlock2.Foreground = Brushes.LightGray;
                ColumnsTextBlock3.Foreground = Brushes.LightGray;
                WordCountTextBlock.Foreground = Brushes.LightGray;
                WordCountTextBlock1.Foreground = Brushes.LightGray;
                FileSizeTextBlock.Foreground = Brushes.LightGray;
                ZoomTextBlock.Foreground = Brushes.LightGray;
            }
            if (!gip.core.reporthandlerwpf.Properties.Settings.Default.ShowCommonEditGroup)
            {
                CommonEditGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowCommonEditGroupMenuItem.IsChecked = true;
            }
            if (!gip.core.reporthandlerwpf.Properties.Settings.Default.ShowCommonViewGroup)
            {
                CommonViewGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowCommonViewGroupMenuItem.IsChecked = true;
            }
            if (!gip.core.reporthandlerwpf.Properties.Settings.Default.ShowCommonInsertGroup)
            {
                CommonInsertGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowCommonInsertGroupMenuItem.IsChecked = true;
            }
            if (!gip.core.reporthandlerwpf.Properties.Settings.Default.ShowCommonFormatGroup)
            {
                CommonFormatGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowCommonFormatGroupMenuItem.IsChecked = true;
            }
            if (!gip.core.reporthandlerwpf.Properties.Settings.Default.ShowCommonToolsGroup)
            {
                CommonToolsGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowCommonToolsGroupMenuItem.IsChecked = true;
            }

            TabCell.TabStripPlacement = Dock.Top;
            if (gip.core.reporthandlerwpf.Properties.Settings.Default.MainWindow_ShowRuler)
            {
                HRulerButton.IsChecked = true;
            }
            if (gip.core.reporthandlerwpf.Properties.Settings.Default.MainWindow_ShowStatusBar)
            {
                StatusbarButton.IsChecked = true;
            }
            else
            {
                StatusBar.Visibility = Visibility.Collapsed;
                TabCell.Margin = new Thickness(TabCell.Margin.Left, TabCell.Margin.Top, TabCell.Margin.Right, -5);
                StatusbarButton.IsChecked = false;
            }

            if (!_Loaded)
            {
                if (TabCell.Items.Count <= 0)
                {
                    NewDocument("New Document",null);
                    SelectedDocument.HeaderContent.FileTypeImage.ToolTip = ".xaml";
                }
            }
            _Loaded = true;
        }

        #endregion

        #region "Closing"


        #endregion

        #endregion

        #region "Ribbon"

        #region "--Common"

        private void CommonViewGroup_LauncherClick(object sender, RoutedEventArgs e)
        {
            MainBar.SelectedTabItem = ViewTab;
        }

        private void CommonInsertGroup_LauncherClick(object sender, RoutedEventArgs e)
        {
            MainBar.SelectedTabItem = InsertTab;
        }

        private void ShowCommonEditGroupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ShowCommonEditGroupMenuItem.IsChecked)
            {
                ShowCommonEditGroupMenuItem.IsChecked = false;
                CommonEditGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowCommonEditGroupMenuItem.IsChecked = true;
                CommonEditGroup.Visibility = Visibility.Visible;
            }
        }

        private void ShowCommonViewGroupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ShowCommonViewGroupMenuItem.IsChecked)
            {
                ShowCommonViewGroupMenuItem.IsChecked = false;
                CommonViewGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowCommonViewGroupMenuItem.IsChecked = true;
                CommonViewGroup.Visibility = Visibility.Visible;
            }
        }

        private void ShowCommonInsertGroupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ShowCommonInsertGroupMenuItem.IsChecked)
            {
                ShowCommonInsertGroupMenuItem.IsChecked = false;
                CommonInsertGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowCommonInsertGroupMenuItem.IsChecked = true;
                CommonInsertGroup.Visibility = Visibility.Visible;
            }
        }

        private void ShowCommonFormatGroupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ShowCommonFormatGroupMenuItem.IsChecked)
            {
                ShowCommonFormatGroupMenuItem.IsChecked = false;
                CommonFormatGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowCommonFormatGroupMenuItem.IsChecked = true;
                CommonFormatGroup.Visibility = Visibility.Visible;
            }
        }

        private void ShowCommonToolsGroupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ShowCommonToolsGroupMenuItem.IsChecked)
            {
                ShowCommonToolsGroupMenuItem.IsChecked = false;
                CommonToolsGroup.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowCommonToolsGroupMenuItem.IsChecked = true;
                CommonToolsGroup.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region "--DocumentMenu"

        #region "New"


        private void NewFromTemplate(object sender, System.Windows.RoutedEventArgs e)
        {
            Fluent.Button t = e.Source as Fluent.Button;
            FileInfo template = new FileInfo(t.Tag as String);
            FileStream fs = new FileStream(template.FullName, FileMode.Open);
            FlowDocument flow = new FlowDocument();
            flow = XamlReader.Load(fs) as FlowDocument;
            fs.Close();
            NewDocument("New Document", flow);
            SelectedDocument.HeaderContent.FileTypeImage.ToolTip = ".xaml";
            Thickness thi = flow.PagePadding;
            SelectedDocument.Editor.Document = flow;
            SelectedDocument.Editor.Focus();
            SelectedDocument.Editor.DocumentName = null;
            SelectedDocument.Editor.FileChanged = false;
            SelectedDocument.Editor.Height = SelectedDocument.Editor.Document.PageHeight;
            SelectedDocument.Editor.Width = SelectedDocument.Editor.Document.PageWidth;
            SelectedDocument.Ruler.Width = SelectedDocument.Editor.Width;
            Semagsoft.DocRuler.Ruler ch = SelectedDocument.Ruler.Children[0] as Semagsoft.DocRuler.Ruler;
            ch.Width = SelectedDocument.Editor.Width;
            try
            {
                double leftmargin = thi.Left;
                double topmargin = thi.Top;
                double rightmargin = thi.Right;
                double bottommargin = thi.Bottom;
                SelectedDocument.Editor.SetPageMargins(new Thickness(leftmargin, topmargin, rightmargin, bottommargin));
            }
            catch (Exception ec)
            {
                SelectedDocument.Editor.SetPageMargins(new Thickness(0, 0, 0, 0));

                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                this.Root().Messages.LogException("Document.Editor.FlowDocEditor", "NewFromTemplate", msg);
            }
        }

        #endregion

        #region "Open"

        //private void OpenMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        //{
        //    Microsoft.Win32.OpenFileDialog openfile = new Microsoft.Win32.OpenFileDialog();
        //    openfile.Multiselect = true;
        //    openfile.Filter = "Supported Documents(*.xamlpackage;*.xaml,*.docx,*.html,*.htm,*.rtf,*.txt)|*.xamlpackage;*.xaml;*.docx;*.html;*.htm;*.rtf;*.txt|XAML Packages(*.xamlpackage)|*.xamlpackage|FlowDocuments(*.xaml)|*.xaml|OpenXML Documents(*.docx)|*.docx|HTML Documents(*.html;*.htm)|*.html;*.htm|Rich Text Documents(*.rtf)|*.rtf|Plan Text Documents(*.txt)|*.txt|All Files(*.*)|*.*";
        //    //if (My.Computer.Info.OSVersion >= "6.1")
        //    //{
        //    //    this.TaskbarItemInfo.Overlay = (ImageSource)Resources("OpenOverlay");
        //    //}
        //    if (openfile.ShowDialog() == true)
        //    {
        //        //if (My.Computer.Info.OSVersion >= "6.1")
        //        //{
        //        //    this.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
        //        //}
        //        int itemcount = 0;
        //        LoadFileDialog load = new LoadFileDialog();
        //        //int @int = 0;
        //        foreach (string s in openfile.FileNames)
        //        {
        //            itemcount += 1;
        //        }
        //        this.IsEnabled = false;
        //        load.Show();
        //        foreach (string i in openfile.FileNames)
        //        {
        //            FileInfo f = new FileInfo(i);
        //            NewDocument(f.Name);
        //            SelectedDocument.Editor.LoadDocument(f.FullName);
        //            SelectedDocument.SetFileType(f.Extension);
        //            SelectedDocument.Editor.FileChanged = false;
        //            SelectedDocument.Editor.Height = SelectedDocument.Editor.Document.PageHeight;
        //            SelectedDocument.Editor.Width = SelectedDocument.Editor.Document.PageWidth;
        //            SelectedDocument.Ruler.Width = SelectedDocument.Editor.Width;
        //            Semagsoft.DocRuler.Ruler ch = SelectedDocument.Ruler.Children[0] as Semagsoft.DocRuler.Ruler;
        //            ch.Width = SelectedDocument.Editor.Width;
        //            if (!Properties.Settings.Default.Options_RecentFiles.Contains(SelectedDocument.Editor.DocumentName) && Properties.Settings.Default.Options_RecentFiles.Count < 13)
        //            {
        //                Properties.Settings.Default.Options_RecentFiles.Add(SelectedDocument.Editor.DocumentName);
        //            }
        //            //if (My.Computer.Info.OSVersion >= "6.1")
        //            //{
        //            //    TaskbarItemInfo.ProgressValue = Convert.ToDouble(@int) / itemcount;
        //            //    @int += 1;
        //            //}
        //        }
        //        load.i = true;
        //        load.Close();
        //        this.IsEnabled = true;
        //    }
        //    //if (My.Computer.Info.OSVersion >= "6.1")
        //    //{
        //    //    this.TaskbarItemInfo.Overlay = null;
        //    //    this.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
        //    //}
        //}

        #endregion


        #region "Save/Save As/Save Copy/Save All"

        //private void SaveMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        //{
        //    if (SelectedDocument.Editor.DocumentName == null)
        //    {
        //        SaveAsMenuItem_Click(this, null);
        //    }
        //    else if (File.Exists(SelectedDocument.Editor.DocumentName))
        //    {
        //        SelectedDocument.Editor.SaveDocument(SelectedDocument.Editor.DocumentName);
        //    }
        //    UpdateButtons();
        //}


        #endregion


        #region "Export"

        #region "Export Xps"

        private void ExportXPSMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateDocumentPreview();
            Microsoft.Win32.SaveFileDialog save = new Microsoft.Win32.SaveFileDialog();
            save.Title = "Export XPS";
            save.Filter = "XPS Document(*.xps)|*.xps|All Files(*.*)|*.*";
            save.AddExtension = true;
            if (save.ShowDialog().Value)
            {
                XpsDocument NewXpsDocument = new XpsDocument(save.FileName, FileAccess.ReadWrite);
                XpsDocumentWriter xpsw = XpsDocument.CreateXpsDocumentWriter(NewXpsDocument);
                xpsw.Write(DocumentPreview);
                NewXpsDocument.Close();
                xpsw = null;
            }
        }

        #endregion



        #endregion

        #region "Properties"

        private void ReadOnlyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument.Editor.IsReadOnly)
            {
                if (SelectedDocument.Editor.DocumentName != null)
                {
                    FileInfo f = new FileInfo(SelectedDocument.Editor.DocumentName);
                    f.IsReadOnly = false;
                }
                SelectedDocument.Editor.IsReadOnly = false;
            }
            else
            {
                if (SelectedDocument.Editor.DocumentName != null)
                {
                    FileInfo f = new FileInfo(SelectedDocument.Editor.DocumentName);
                    f.IsReadOnly = true;
                }
                SelectedDocument.Editor.IsReadOnly = true;
            }
        }

        #endregion

        #endregion

        #region "-Edit"

        #region "Undo/Redo/Cut/Copy/Paste/Delete/Select All"

        private void UndoMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Focus();
            SelectedDocument.Editor.Undo();
        }

        private void RedoMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Focus();
            SelectedDocument.Editor.Redo();
        }

        private void CutMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Focus();
            SelectedDocument.Editor.Cut();
        }

        private void CutParagraphMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (SelectedDocument.Editor.Selection.IsEmpty)
                {
                    TextRange TRange = new TextRange(SelectedDocument.Editor.CaretPosition.Paragraph.ElementStart, SelectedDocument.Editor.CaretPosition.Paragraph.ElementEnd);
                    if (!TRange.IsEmpty)
                    {
                        SelectedDocument.Editor.Selection.Select(TRange.Start, TRange.End);
                        SelectedDocument.Editor.Cut();
                    }
                }
                else
                {
                    EditingCommands.MoveToLineStart.Execute(null, SelectedDocument.Editor);
                    TextRange TRange = new TextRange(SelectedDocument.Editor.CaretPosition.Paragraph.ElementStart, SelectedDocument.Editor.CaretPosition.Paragraph.ElementEnd);
                    if (!TRange.IsEmpty)
                    {
                        SelectedDocument.Editor.Selection.Select(TRange.Start, TRange.End);
                        SelectedDocument.Editor.Cut();
                    }
                }
            }
            catch (Exception ex)
            {
                VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = ex.Message, MessageLevel = eMsgLevel.Error }, eMsgButton.OK, null);
                vbMessagebox.ShowMessageBox();
            }
            e.Handled = true;
        }

        private void CutLineMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.MoveToLineStart.Execute(null, SelectedDocument.Editor);
            EditingCommands.SelectToLineEnd.Execute(null, SelectedDocument.Editor);
            SelectedDocument.Editor.Cut();
            e.Handled = true;
        }

        private void CopyMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Focus();
            SelectedDocument.Editor.Copy();
        }

        private void CopyParagraphMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (SelectedDocument.Editor.Selection.IsEmpty)
                {
                    TextRange TRange = new TextRange(SelectedDocument.Editor.CaretPosition.Paragraph.ElementStart, SelectedDocument.Editor.CaretPosition.Paragraph.ElementEnd);
                    if (!TRange.IsEmpty)
                    {
                        SelectedDocument.Editor.Selection.Select(TRange.Start, TRange.End);
                        SelectedDocument.Editor.Copy();
                    }
                }
                else
                {
                    EditingCommands.MoveToLineStart.Execute(null, SelectedDocument.Editor);
                    TextRange TRange = new TextRange(SelectedDocument.Editor.CaretPosition.Paragraph.ElementStart, SelectedDocument.Editor.CaretPosition.Paragraph.ElementEnd);
                    if (!TRange.IsEmpty)
                    {
                        SelectedDocument.Editor.Selection.Select(TRange.Start, TRange.End);
                        SelectedDocument.Editor.Copy();
                    }
                }
            }
            catch (Exception ex)
            {
                VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = ex.Message, MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
                vbMessagebox.ShowMessageBox();
            }
            e.Handled = true;
        }

        private void CopyLineMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.MoveToLineStart.Execute(null, SelectedDocument.Editor);
            EditingCommands.SelectToLineEnd.Execute(null, SelectedDocument.Editor);
            SelectedDocument.Editor.Copy();
            e.Handled = true;
        }

        #region "Paste"

        private void PasteMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Focus();
            SelectedDocument.Editor.Paste();
        }

        private void PasteTextButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.CaretPosition.InsertTextInRun(Clipboard.GetText());
            e.Handled = true;
        }

        private void PasteImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //TextPointer t = SelectedDocument.Editor.CaretPosition;
            //Image img = new Image();
            //BitmapImage b = new BitmapImage();
            //MemoryStream ms = new MemoryStream();
            //// no using here! BitmapImage will dispose the stream after loading
            //Clipboard.GetImage().Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            //b.BeginInit();
            //b.CacheOption = BitmapCacheOption.OnLoad;
            //b.StreamSource = ms;
            //b.EndInit();
            //img.Tag = new Thickness(0, 1, 1, 0);
            //TransformGroup trans = new TransformGroup();
            //trans.Children.Add(new RotateTransform(0));
            //trans.Children.Add(new ScaleTransform(1, 1));
            //img.LayoutTransform = trans;
            //img.Stretch = Stretch.Fill;
            //img.Height = b.Height;
            //img.Width = b.Width;
            //img.Source = b;
            //InlineUIContainer inline = new InlineUIContainer(img);
            //if (object.ReferenceEquals(t.Parent.GetType, typeof(TableCell)))
            //{
            //    TableCell cell = t.Parent;
            //    cell.Blocks.Add(new Paragraph(inline));
            //}
            //else
            //{
            //    t.Paragraph.Inlines.Add(inline);
            //}
            e.Handled = true;
        }

        #endregion

        private void DeleteMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.Delete.Execute(null, SelectedDocument.Editor);
        }

        private void DeleteParagraphMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (SelectedDocument.Editor.Selection.IsEmpty)
                {
                    TextRange TRange = new TextRange(SelectedDocument.Editor.CaretPosition.Paragraph.ElementStart, SelectedDocument.Editor.CaretPosition.Paragraph.ElementEnd);
                    if (!TRange.IsEmpty)
                    {
                        SelectedDocument.Editor.Selection.Select(TRange.Start, TRange.End);
                        EditingCommands.Delete.Execute(null, SelectedDocument.Editor);
                    }
                }
                else
                {
                    EditingCommands.MoveToLineStart.Execute(null, SelectedDocument.Editor);
                    TextRange TRange = new TextRange(SelectedDocument.Editor.CaretPosition.Paragraph.ElementStart, SelectedDocument.Editor.CaretPosition.Paragraph.ElementEnd);
                    if (!TRange.IsEmpty)
                    {
                        SelectedDocument.Editor.Selection.Select(TRange.Start, TRange.End);
                        EditingCommands.Delete.Execute(null, SelectedDocument.Editor);
                    }
                }
            }
            catch (Exception ex)
            {
                VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = ex.Message, MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
                vbMessagebox.ShowMessageBox();
            }
            e.Handled = true;
        }

        private void DeleteLineMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.MoveToLineStart.Execute(null, SelectedDocument.Editor);
            EditingCommands.SelectToLineEnd.Execute(null, SelectedDocument.Editor);
            EditingCommands.Delete.Execute(null, SelectedDocument.Editor);
            e.Handled = true;
        }

        private void SelectAllMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Focus();
            SelectedDocument.Editor.SelectAll();
        }

        private void SelectParagraphMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (SelectedDocument.Editor.Selection.IsEmpty)
                {
                    TextRange TRange = new TextRange(SelectedDocument.Editor.CaretPosition.Paragraph.ElementStart, SelectedDocument.Editor.CaretPosition.Paragraph.ElementEnd);
                    if (!TRange.IsEmpty)
                    {
                        SelectedDocument.Editor.Selection.Select(TRange.Start, TRange.End);
                    }
                }
                else
                {
                    EditingCommands.MoveToLineStart.Execute(null, SelectedDocument.Editor);
                    TextRange TRange = new TextRange(SelectedDocument.Editor.CaretPosition.Paragraph.ElementStart, SelectedDocument.Editor.CaretPosition.Paragraph.ElementEnd);
                    if (!TRange.IsEmpty)
                    {
                        SelectedDocument.Editor.Selection.Select(TRange.Start, TRange.End);
                    }
                }
            }
            catch (Exception ex)
            {
                VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = ex.Message, MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
                vbMessagebox.ShowMessageBox();
            }
            e.Handled = true;
        }

        private void SelectLineMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.MoveToLineStart.Execute(null, SelectedDocument.Editor);
            EditingCommands.SelectToLineEnd.Execute(null, SelectedDocument.Editor);
            e.Handled = true;
        }

        #endregion

        #region "Find/Replace/Go To"

        private void FindButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FindDialog findDialog = new FindDialog(SelectedDocument.Editor.Document, this);
            findDialog.ShowDialog();
            if (findDialog.Res == "OK")
            {
                TextRange p = SelectedDocument.Editor.FindWordFromPosition(SelectedDocument.Editor.CaretPosition, findDialog.TextBox1.Text);
                try
                {
                    SelectedDocument.Editor.Selection.Select(p.Start, p.End);
                }
                catch (Exception ex)
                {
                    VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = "Word not found.", MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
                    vbMessagebox.ShowMessageBox();

                    string msg = ex.Message;
                    if (ex.InnerException != null && ex.InnerException.Message != null)
                        msg += " Inner:" + ex.InnerException.Message;

                    this.Root().Messages.LogException("Document.Editor.FlowDocEditor", "FindButton_Click", msg);
                }
            }
        }

        private void ReplaceButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ReplaceDialog replaceDialog = new ReplaceDialog(this);
            replaceDialog.ShowDialog();
            if (replaceDialog.Res == "OK")
            {
                TextRange p = SelectedDocument.Editor.FindWordFromPosition(SelectedDocument.Editor.Document.ContentStart.DocumentStart, replaceDialog.TextBox1.Text);
                try
                {
                    SelectedDocument.Editor.Selection.Select(p.Start, p.End);
                    SelectedDocument.Editor.Selection.Text = replaceDialog.TextBox2.Text;
                }
                catch (Exception ex)
                {
                    VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = "Word not found.", MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
                    vbMessagebox.ShowMessageBox();

                    string msg = ex.Message;
                    if (ex.InnerException != null && ex.InnerException.Message != null)
                        msg += " Inner:" + ex.InnerException.Message;

                    this.Root().Messages.LogException("Document.Editor.FlowDocEditor", "ReplaceButton_Click", msg);
                }
            }
        }

        private void GoToButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GoToDialog gotodialog = new GoToDialog(this);
            gotodialog.ShowDialog();
            if (gotodialog.Res == "OK")
            {
                SelectedDocument.Editor.GoToLine(gotodialog.line);
            }
        }

        #endregion

        private void UppercaseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedDocument.Editor.Selection.Text = SelectedDocument.Editor.Selection.Text.ToUpper();
        }

        private void LowercaseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedDocument.Editor.Selection.Text = SelectedDocument.Editor.Selection.Text.ToLower();
        }

        #endregion

        #region "--ViewMenu"

        private void HRulerButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (DocumentTab t in TabCell.Items)
            {
                t.Ruler.Visibility = Visibility.Visible;
            }
            gip.core.reporthandlerwpf.Properties.Settings.Default.MainWindow_ShowRuler = true;
            UpdateUI();
        }

        private void HRulerButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (DocumentTab t in TabCell.Items)
            {
                t.Ruler.Visibility = Visibility.Collapsed;
            }
            gip.core.reporthandlerwpf.Properties.Settings.Default.MainWindow_ShowRuler = false;
            UpdateUI();
        }

        private void StatusbarButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            StatusBar.Visibility = Visibility.Visible;
            TabCell.Margin = new Thickness(TabCell.Margin.Left, TabCell.Margin.Top, TabCell.Margin.Right, 17);
        }

        private void StatusbarButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            StatusBar.Visibility = Visibility.Collapsed;
            TabCell.Margin = new Thickness(TabCell.Margin.Left, TabCell.Margin.Top, TabCell.Margin.Right, -5);
        }

        private void ZoomInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zoomSlider.Value += 10;
        }

        private void ZoomOutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zoomSlider.Value -= 10;
        }

        private void ResetZoomButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zoomSlider.Value = 100;
        }
        #endregion

        #region "Insert"

        #region "Tables"

        private void TableGrid_Click(int y, int x)
        {
            Table t = new Table();
            int @int = Convert.ToInt32(y);
            int int2 = Convert.ToInt32(x);
            while (!(@int == 0))
            {
                TableRowGroup trg = new TableRowGroup();
                TableRow tr = new TableRow();
                while (!(int2 == 0))
                {
                    TableCell tc = new TableCell();
                    tc.BorderBrush = Brushes.Black;
                    tc.BorderThickness = new Thickness(1, 1, 1, 1);
                    tr.Cells.Add(tc);
                    int2 -= 1;
                }
                int2 = Convert.ToInt32(x);
                trg.Rows.Add(tr);
                t.RowGroups.Add(trg);
                @int -= 1;
            }
            t.BorderThickness = new Thickness(1, 1, 1, 1);
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
            {
                TableCell tc = SelectedDocument.Editor.CaretPosition.Paragraph.Parent as TableCell;
                if (tc != null)
                {
                    tc.Blocks.InsertBefore(SelectedDocument.Editor.CaretPosition.Paragraph, t);
                }
                else
                {
                    ListItem listitem = SelectedDocument.Editor.CaretPosition.Paragraph.Parent as ListItem;
                    if (listitem != null)
                    {
                        List list = listitem.Parent as List;
                        if (list != null)
                        {
                            SelectedDocument.Editor.Document.Blocks.InsertAfter(list, t);
                        }
                    }
                    else
                    {
                        SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.CaretPosition.Paragraph, t);
                    }
                }
            }
            else
            {
                TableCell tc = SelectedDocument.Editor.CaretPosition.Parent as TableCell;
                if (tc != null)
                {
                    tc.Blocks.Add(t);
                }
                else
                {
                    TableRow tr = SelectedDocument.Editor.CaretPosition.Parent as TableRow;
                    if (tr != null)
                    {
                        TableRowGroup trg = tr.Parent as TableRowGroup;
                        Table table = trg.Parent as Table;
                        SelectedDocument.Editor.Document.Blocks.InsertAfter(table as Block, t);
                    }
                    else
                    {
                        FlowDocument flow = SelectedDocument.Editor.CaretPosition.Parent as FlowDocument;
                        if (flow != null)
                        {
                            SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.Document.Blocks.FirstBlock, t);
                        }
                    }
                }
            }
            UpdateButtons();
        }

        private void TableMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TableDialog tableDialog = new TableDialog(this);
            tableDialog.ShowDialog();
            if (tableDialog.Res == "OK")
            {
                Table t = new Table();
                int @int = Convert.ToInt32(tableDialog.RowsTextBox.Value);
                int int2 = Convert.ToInt32(tableDialog.CellsTextBox.Value);
                while (!(@int == 0))
                {
                    TableRowGroup trg = new TableRowGroup();
                    TableRow tr = new TableRow();
                    while (!(int2 == 0))
                    {
                        TableCell tc = new TableCell();
                        tc.Background = tableDialog.CellBackgroundColor;
                        tc.BorderBrush = tableDialog.CellBorderColor;
                        tc.BorderThickness = new Thickness(1, 1, 1, 1);
                        tr.Cells.Add(tc);
                        int2 -= 1;
                    }
                    int2 = Convert.ToInt32(tableDialog.CellsTextBox.Value);
                    trg.Rows.Add(tr);
                    t.RowGroups.Add(trg);
                    @int -= 1;
                }
                t.Background = tableDialog.BackgroundColor;
                t.BorderBrush = tableDialog.BorderColor;
                t.BorderThickness = new Thickness(1, 1, 1, 1);
                if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
                {
                    TableCell tc = SelectedDocument.Editor.CaretPosition.Paragraph.Parent as TableCell;
                    if (tc != null)
                    {
                        tc.Blocks.InsertBefore(SelectedDocument.Editor.CaretPosition.Paragraph, t);
                    }
                    else
                    {
                        ListItem listitem = SelectedDocument.Editor.CaretPosition.Paragraph.Parent as ListItem;
                        if (listitem != null)
                        {
                            List list = listitem.Parent as List;
                            if (list != null)
                            {
                                SelectedDocument.Editor.Document.Blocks.InsertAfter(list, t);
                            }
                        }
                        else
                        {
                            SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.CaretPosition.Paragraph, t);
                        }
                    }
                }
                else
                {
                    TableCell tc = SelectedDocument.Editor.CaretPosition.Parent as TableCell;
                    if (tc != null)
                    {
                        tc.Blocks.Add(t);
                    }
                    else
                    {
                        TableRow tr = SelectedDocument.Editor.CaretPosition.Parent as TableRow;
                        if (tr != null)
                        {
                            TableRowGroup trg = tr.Parent as TableRowGroup;
                            Table table = trg.Parent as Table;
                            SelectedDocument.Editor.Document.Blocks.InsertAfter(table as Block, t);
                        }
                        else
                        {
                            FlowDocument flow = SelectedDocument.Editor.CaretPosition.Parent as FlowDocument;
                            if (flow != null)
                            {
                                SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.Document.Blocks.FirstBlock, t);
                            }
                        }
                    }
                }
                UpdateButtons();
            }
        }

        #endregion

        #region "Image"

        private void ImageButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog();
            open.Multiselect = true;
            open.Title = "Add Images";
            open.Filter = "Supported Images(*.bmp;*.gif;*.jpeg;*.jpg;*.png)|*.bmp;*.gif;*.jpeg;*.jpg;*.png|Bitmap Images(*.bmp)|*.bmp|GIF Images(*.gif)|*.gif|JPEG Images(*.jpeg;*.jpg)|*.jpeg;*.jpg|PNG Images(*.png)|*.png|All Files(*.*)|*.*";
            if (open.ShowDialog().Value == true)
            {
                SelectedDocument.Editor.Focus();
                foreach (string i in open.FileNames)
                {
                    TextPointer t = SelectedDocument.Editor.CaretPosition;
                    Image img = new Image();
                    BitmapImage b = new BitmapImage();
                    b.BeginInit();
                    b.UriSource = new Uri(i);
                    b.EndInit();
                    img.Tag = new Thickness(0, 1, 1, 0);
                    TransformGroup trans = new TransformGroup();
                    trans.Children.Add(new RotateTransform(0));
                    trans.Children.Add(new ScaleTransform(1, 1));
                    img.LayoutTransform = trans;
                    img.Stretch = Stretch.Fill;
                    img.Height = b.Height;
                    img.Width = b.Width;
                    img.Source = b;
                    InlineUIContainer inline = new InlineUIContainer(img);
                    if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
                    {
                        t.Paragraph.Inlines.Add(inline);
                    }
                    else
                    {
                        TableCell tc = SelectedDocument.Editor.CaretPosition.Parent as TableCell;
                        if (tc != null)
                        {
                            tc.Blocks.Add(new Paragraph(inline));
                        }
                        else
                        {
                            TableRow tr = SelectedDocument.Editor.CaretPosition.Parent as TableRow;
                            if (tr != null)
                            {
                                TableRowGroup trg = tr.Parent as TableRowGroup;
                                Table table = trg.Parent as Table;
                                SelectedDocument.Editor.Document.Blocks.InsertAfter(table as Block, new Paragraph(inline));
                            }
                            else
                            {
                                FlowDocument flow = SelectedDocument.Editor.CaretPosition.Parent as FlowDocument;
                                if (flow != null)
                                {
                                    SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.Document.Blocks.FirstBlock, new Paragraph(inline));
                                }
                            }
                        }
                    }
                }
                UpdateSelected();
            }
        }

        #endregion

        #region "Object"

        private void ObjectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ObjectDialog od = new ObjectDialog(this);
            od.ShowDialog();
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
            {
                if (od.Res == "button")
                {
                    TextPointer t = SelectedDocument.Editor.CaretPosition;
                    System.Windows.Controls.Button b = new System.Windows.Controls.Button();
                    InlineUIContainer i = new InlineUIContainer(b);
                    b.Width = od.OW;
                    b.Height = od.OH;
                    b.Content = od.OT;
                    t.Paragraph.Inlines.Add(i);
                }
                else if (od.Res == "radiobutton")
                {
                    TextPointer t = SelectedDocument.Editor.CaretPosition;
                    System.Windows.Controls.RadioButton b = new System.Windows.Controls.RadioButton();
                    InlineUIContainer i = new InlineUIContainer(b);
                    b.Width = od.OW;
                    b.Height = od.OH;
                    b.Content = od.OT;
                    t.Paragraph.Inlines.Add(i);
                }
                else if (od.Res == "checkbox")
                {
                    TextPointer t = SelectedDocument.Editor.CaretPosition;
                    System.Windows.Controls.CheckBox b = new System.Windows.Controls.CheckBox();
                    InlineUIContainer i = new InlineUIContainer(b);
                    b.Width = od.OW;
                    b.Height = od.OH;
                    b.Content = od.OT;
                    t.Paragraph.Inlines.Add(i);
                }
                else if (od.Res == "textblock")
                {
                    TextPointer t = SelectedDocument.Editor.CaretPosition;
                    TextBlock b = new TextBlock();
                    InlineUIContainer i = new InlineUIContainer(b);
                    b.Width = od.OW;
                    b.Height = od.OH;
                    b.Text = od.OT;
                    t.Paragraph.Inlines.Add(i);
                }
            }
            else
            {
                TableCell tc = SelectedDocument.Editor.CaretPosition.Parent as TableCell;
                if (tc != null)
                {
                    if (od.Res == "button")
                    {
                        TextPointer t = SelectedDocument.Editor.CaretPosition;
                        System.Windows.Controls.Button b = new System.Windows.Controls.Button();
                        InlineUIContainer i = new InlineUIContainer(b);
                        b.Width = od.OW;
                        b.Height = od.OH;
                        b.Content = od.OT;
                        tc.Blocks.Add(new Paragraph(i));
                    }
                    else if (od.Res == "radiobutton")
                    {
                        TextPointer t = SelectedDocument.Editor.CaretPosition;
                        System.Windows.Controls.RadioButton b = new System.Windows.Controls.RadioButton();
                        InlineUIContainer i = new InlineUIContainer(b);
                        b.Width = od.OW;
                        b.Height = od.OH;
                        b.Content = od.OT;
                        tc.Blocks.Add(new Paragraph(i));
                    }
                    else if (od.Res == "checkbox")
                    {
                        TextPointer t = SelectedDocument.Editor.CaretPosition;
                        System.Windows.Controls.CheckBox b = new System.Windows.Controls.CheckBox();
                        InlineUIContainer i = new InlineUIContainer(b);
                        b.Width = od.OW;
                        b.Height = od.OH;
                        b.Content = od.OT;
                        tc.Blocks.Add(new Paragraph(i));
                    }
                    else if (od.Res == "textblock")
                    {
                        TextPointer t = SelectedDocument.Editor.CaretPosition;
                        TextBlock b = new TextBlock();
                        InlineUIContainer i = new InlineUIContainer(b);
                        b.Width = od.OW;
                        b.Height = od.OH;
                        b.Text = od.OT;
                        tc.Blocks.Add(new Paragraph(i));
                    }
                }
                else
                {
                    TableRow tr = SelectedDocument.Editor.CaretPosition.Parent as TableRow;
                    if (tr != null)
                    {
                        TableRowGroup trg = tr.Parent as TableRowGroup;
                        Table table = trg.Parent as Table;
                        if (od.Res == "button")
                        {
                            TextPointer t = SelectedDocument.Editor.CaretPosition;
                            System.Windows.Controls.Button b = new System.Windows.Controls.Button();
                            InlineUIContainer i = new InlineUIContainer(b);
                            b.Width = od.OW;
                            b.Height = od.OH;
                            b.Content = od.OT;
                            SelectedDocument.Editor.Document.Blocks.InsertAfter(table as Block, new Paragraph(i));
                        }
                        else if (od.Res == "radiobutton")
                        {
                            TextPointer t = SelectedDocument.Editor.CaretPosition;
                            System.Windows.Controls.RadioButton b = new System.Windows.Controls.RadioButton();
                            InlineUIContainer i = new InlineUIContainer(b);
                            b.Width = od.OW;
                            b.Height = od.OH;
                            b.Content = od.OT;
                            SelectedDocument.Editor.Document.Blocks.InsertAfter(table as Block, new Paragraph(i));
                        }
                        else if (od.Res == "checkbox")
                        {
                            TextPointer t = SelectedDocument.Editor.CaretPosition;
                            System.Windows.Controls.CheckBox b = new System.Windows.Controls.CheckBox();
                            InlineUIContainer i = new InlineUIContainer(b);
                            b.Width = od.OW;
                            b.Height = od.OH;
                            b.Content = od.OT;
                            SelectedDocument.Editor.Document.Blocks.InsertAfter(table as Block, new Paragraph(i));
                        }
                        else if (od.Res == "textblock")
                        {
                            TextPointer t = SelectedDocument.Editor.CaretPosition;
                            TextBlock b = new TextBlock();
                            InlineUIContainer i = new InlineUIContainer(b);
                            b.Width = od.OW;
                            b.Height = od.OH;
                            b.Text = od.OT;
                            SelectedDocument.Editor.Document.Blocks.InsertAfter(table as Block, new Paragraph(i));
                        }
                    }
                    else
                    {
                        FlowDocument fd = SelectedDocument.Editor.CaretPosition.Parent as FlowDocument;
                        if (fd != null)
                        {
                            if (od.Res == "button")
                            {
                                TextPointer t = SelectedDocument.Editor.CaretPosition;
                                System.Windows.Controls.Button b = new System.Windows.Controls.Button();
                                InlineUIContainer i = new InlineUIContainer(b);
                                b.Width = od.OW;
                                b.Height = od.OH;
                                b.Content = od.OT;
                                SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.Document.Blocks.FirstBlock, new Paragraph(i));
                            }
                            else if (od.Res == "radiobutton")
                            {
                                TextPointer t = SelectedDocument.Editor.CaretPosition;
                                System.Windows.Controls.RadioButton b = new System.Windows.Controls.RadioButton();
                                InlineUIContainer i = new InlineUIContainer(b);
                                b.Width = od.OW;
                                b.Height = od.OH;
                                b.Content = od.OT;
                                SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.Document.Blocks.FirstBlock, new Paragraph(i));
                            }
                            else if (od.Res == "checkbox")
                            {
                                TextPointer t = SelectedDocument.Editor.CaretPosition;
                                System.Windows.Controls.CheckBox b = new System.Windows.Controls.CheckBox();
                                InlineUIContainer i = new InlineUIContainer(b);
                                b.Width = od.OW;
                                b.Height = od.OH;
                                b.Content = od.OT;
                                SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.Document.Blocks.FirstBlock, new Paragraph(i));
                            }
                            else if (od.Res == "textblock")
                            {
                                TextPointer t = SelectedDocument.Editor.CaretPosition;
                                TextBlock b = new TextBlock();
                                InlineUIContainer i = new InlineUIContainer(b);
                                b.Width = od.OW;
                                b.Height = od.OH;
                                b.Text = od.OT;
                                SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.Document.Blocks.FirstBlock, new Paragraph(i));
                            }
                        }
                    }
                }
            }
            UpdateButtons();
        }

        #endregion

        #region "Shape"

        private void ShapeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShapeDialog sd = new ShapeDialog(this);
            sd.ShowDialog();
            if (sd.Res == "OK")
            {
                Shape s = null;
                if (sd.TypeComboBox.SelectedIndex == 0)
                {
                    s = new Ellipse();
                }
                else if (sd.TypeComboBox.SelectedIndex == 1)
                {
                    s = new Rectangle();
                }
                s.Height = sd.Shape.RenderSize.Height;
                s.Width = sd.Shape.RenderSize.Width;
                s.Stroke = sd.Shape.Stroke;
                s.StrokeThickness = sd.Shape.StrokeThickness;
                InlineUIContainer i = new InlineUIContainer();
                i.Child = s;
                SelectedDocument.Editor.CaretPosition.Paragraph.Inlines.Add(i);
                UpdateButtons();
            }
        }

        #endregion

        #region "Chart"

        private void ChartButton_Click(object sender, RoutedEventArgs e)
        {
            //ChartDialog d = new ChartDialog(this);
            //d.ShowDialog();
            //if (d.Res == "OK")
            //{
            //    RenderTargetBitmap render = new RenderTargetBitmap(Convert.ToInt32(d.ChartWidth.Value), Convert.ToInt32(d.ChartHight.Value), 96, 96, PixelFormats.Default);
            //    render.Render(d.PreviewChart);
            //    BitmapSource bsource = render;
            //    Image img = new Image();
            //    img.Source = bsource;
            //    img.Height = d.ChartHight.Value;
            //    img.Width = d.ChartWidth.Value;
            //    InlineUIContainer i = new InlineUIContainer();
            //    i.Child = img;
            //    SelectedDocument.Editor.CaretPosition.Paragraph.Inlines.Add(i);
            //    UpdateButtons();
            //}
        }

        #endregion

        #region "Link"

        private void LinkMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LinkDialog l = new LinkDialog(this);
            l.ShowDialog();
            if (!(l.Link == null))
            {
                try
                {
                    SelectedDocument.Editor.Focus();
                    Hyperlink li = new Hyperlink(SelectedDocument.Editor.CaretPosition.DocumentStart, SelectedDocument.Editor.CaretPosition.DocumentEnd);
                    Uri u = new Uri(l.Link);
                    li.NavigateUri = u;
                    UpdateButtons();
                }
                catch (Exception ex)
                {
                    VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = ex.Message, MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
                    vbMessagebox.ShowMessageBox();
                }
            }
        }

        #endregion

        #region "FlowDocument"

        private void InsertFlowDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog();
            open.Title = "Insert FlowDocument";
            open.Filter = "FlowDocument(*.xaml)|*.xaml";
            if (open.ShowDialog().Value)
            {
                FileStream fs = File.Open(open.FileName, FileMode.Open, FileAccess.Read);
                FlowDocument content = XamlReader.Load(fs) as FlowDocument;
                fs.Close();
                fs = null;
                foreach (Block b in content.Blocks)
                {
                    Block newblock = null;
                    string xaml = XamlWriter.Save(b);
                    using (var sr = new StringReader(xaml))
                    using (var tr = new XmlTextReader(sr))
                    {
                        newblock = XamlReader.Load(tr) as Block;
                    }
                    SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.CaretPosition.Paragraph, newblock);
                }
            }
        }

        #endregion

        #region "Rich Text Document"

        private void InsertRichTextDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog();
            open.Title = "Insert Rich Text Document";
            open.Filter = "Rich Text Document(*.rtf)|*.rtf";
            if (open.ShowDialog().Value)
            {
                FileStream st = new FileStream(open.FileName, FileMode.Open, FileAccess.Read);
                SelectedDocument.Editor.Selection.Load(st, DataFormats.Rtf);
                st.Close();
            }
        }

        #endregion

        #region "Text File"

        private void TextFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog o = new Microsoft.Win32.OpenFileDialog();
            o.Title = "Insert Text File";
            o.Filter = "Text Document(*.txt)|*.txt|All Files(*.*)|*.*";
            if (o.ShowDialog().Value)
            {
                SelectedDocument.Editor.CaretPosition.InsertTextInRun(File.ReadAllText(o.FileName));
                UpdateButtons();
            }
        }

        #endregion

        private void EmoticonGrid_Click(BitmapSource img)
        {
            SelectedDocument.Editor.Focus();
            TextPointer t = SelectedDocument.Editor.CaretPosition;
            Image image = new Image();
            image.Tag = new Thickness(0, 1, 1, 0);
            TransformGroup trans = new TransformGroup();
            trans.Children.Add(new RotateTransform(0));
            trans.Children.Add(new ScaleTransform(1, 1));
            image.LayoutTransform = trans;
            image.Stretch = Stretch.Fill;
            image.Height = img.Height;
            image.Width = img.Width;
            image.Source = img;
            InlineUIContainer inline = new InlineUIContainer(image);
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
            {
                t.Paragraph.Inlines.Add(inline);
            }
            else
            {
                TableCell tc = SelectedDocument.Editor.CaretPosition.Parent as TableCell;
                if (tc != null)
                {
                    tc.Blocks.Add(new Paragraph(inline));
                }
                else
                {
                    TableRow tr = SelectedDocument.Editor.CaretPosition.Parent as TableRow;
                    if (tr != null)
                    {
                        TableRowGroup trg = tr.Parent as TableRowGroup;
                        Table table = trg.Parent as Table;
                        SelectedDocument.Editor.Document.Blocks.InsertAfter(table as Block, new Paragraph(inline));
                    }
                    else
                    {
                        FlowDocument flow = SelectedDocument.Editor.CaretPosition.Parent as FlowDocument;
                        if (flow != null)
                        {
                            SelectedDocument.Editor.Document.Blocks.InsertBefore(SelectedDocument.Editor.Document.Blocks.FirstBlock, new Paragraph(inline));
                        }
                    }
                }
            }
            UpdateSelected();
        }

        #region "Symbol"

        private void InsertSymbolGallery_Click(string symbol)
        {
            SelectedDocument.Editor.CaretPosition.InsertTextInRun(symbol);
            UpdateButtons();
        }

        #endregion

        #region "Video"

        private void VideoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "Insert Video";
            dialog.Filter = "Supported Videos(*.avi;*.mpeg;*.mpg;*.wmv)|*.avi;*.mpeg;*.mpg;*.wmv|AVI Videos(*.avi)|*.avi|MPEG Videos(*.mpeg;*.mpg)|*.mpeg;*.mpg|WMV Videos(*.wmv)|*.wmv|All Files(*.*)|*.*";
            if (dialog.ShowDialog().Value)
            {
                try
                {
                    MediaElement m = new MediaElement();
                    m.Width = 320;
                    m.Height = 240;
                    m.Source = new Uri(dialog.FileName);
                    m.LoadedBehavior = MediaState.Manual;
                    InlineUIContainer i = new InlineUIContainer();
                    i.Child = m;
                    SelectedDocument.Editor.CaretPosition.Paragraph.Inlines.Add(i);
                    UpdateSelected();
                }
                catch (Exception ex)
                {
                    VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = ex.Message, MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
                    vbMessagebox.ShowMessageBox();
                }
            }
        }

        #endregion

        #region "Horizontal Line"

        private void HorizontalLineButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            InsertLineDialog d = new InsertLineDialog(this);
            d.ShowDialog();
            if (d.Res == "OK")
            {
                Separator line = new Separator();
                InlineUIContainer inline = new InlineUIContainer();
                line.Background = Brushes.Gray;
                line.Width = d.h;
                line.HorizontalAlignment = HorizontalAlignment.Stretch;
                inline.Child = line;
                SelectedDocument.Editor.CaretPosition.Paragraph.Inlines.Add(inline);
                UpdateButtons();
            }
        }

        #endregion

        #region "Header/Footer"

        private void HeaderButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Document.Blocks.FirstBlock.ContentStart.InsertParagraphBreak();
            SelectedDocument.Editor.Document.Blocks.FirstBlock.ContentStart.InsertTextInRun("Header");
            UpdateButtons();
        }

        private void FooterButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Document.ContentEnd.InsertParagraphBreak();
            SelectedDocument.Editor.Document.ContentEnd.InsertTextInRun("Footer");
            UpdateButtons();
        }

        #endregion

        #region "Date/Time"

        private void DateMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.CaretPosition.InsertTextInRun(System.DateTime.Now.ToString("M/dd/yyyy"));
            UpdateButtons();
        }

        private void MoreDateMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DateDialog d = new DateDialog(this);
            d.ShowDialog();
            if (d.Res == "OK")
            {
                SelectedDocument.Editor.CaretPosition.InsertTextInRun(d.ListBox1.SelectedItem as String);
                UpdateButtons();
            }
            e.Handled = true;
        }

        private void TimeMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.CaretPosition.InsertTextInRun(System.DateTime.Now.ToString("h:mm tt"));
            UpdateButtons();
        }

        private void MoreTimeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TimeDialog d = new TimeDialog(this);
            d.ShowDialog();
            if (d.Res == "OK")
            {
                if (d.RadioButton12.IsChecked.Value)
                {
                    if (d.AMPMCheckBox.IsChecked.Value)
                    {
                        if (d.SecCheckBox.IsChecked.Value)
                        {
                            SelectedDocument.Editor.CaretPosition.InsertTextInRun(System.DateTime.Now.ToString("h:mm:ss tt"));
                        }
                        else
                        {
                            SelectedDocument.Editor.CaretPosition.InsertTextInRun(System.DateTime.Now.ToString("h:mm tt"));
                        }
                    }
                    else
                    {
                        if (d.SecCheckBox.IsChecked.Value)
                        {
                            SelectedDocument.Editor.CaretPosition.InsertTextInRun(System.DateTime.Now.ToString("h:mm:ss"));
                        }
                        else
                        {
                            SelectedDocument.Editor.CaretPosition.InsertTextInRun(System.DateTime.Now.ToString("h:mm"));
                        }
                    }
                }
                else
                {
                    if (d.SecCheckBox.IsChecked.Value)
                    {
                        SelectedDocument.Editor.CaretPosition.InsertTextInRun(System.DateTime.Now.ToString("HH:mm:ss"));
                    }
                    else
                    {
                        SelectedDocument.Editor.CaretPosition.InsertTextInRun(System.DateTime.Now.ToString("HH:mm"));
                    }
                }
                UpdateButtons();
            }
            e.Handled = true;
        }

        #endregion

        #endregion

        #region "-Format"

        private void ClearFormattingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Selection.ClearAllProperties();
            UpdateSelected();
        }

        #region "Font/Font Size/Font Color/Hightlight Color"

        private bool UpdatingFont = false;
        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontComboBox.IsLoaded && !UpdatingFont)
            {
                SelectedDocument.Editor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, FontComboBox.SelectedItem);
                SelectedDocument.Editor.Focus();
                UpdateSelected();
            }
        }

        private void FontSizeComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectedDocument.Editor.Focus();
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.IsLoaded && !UpdatingFont)
            {
                try
                {
                    double val = Convert.ToDouble(FontSizeComboBox.SelectedValue);
                    SelectedDocument.Editor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, val);
                    UpdateSelected();
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    this.Root().Messages.LogException("Document.Editor.FlowDocEditor", "FontSizeComboBox_SelectionChanged", msg);
                }
            }
        }

        private void FontColorGallery_SelectedColorChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (FontColorGallery.SelectedColor != null && FontColorGallery.SelectedColor.HasValue)
            {
                SolidColorBrush c = new SolidColorBrush();
                c.Color = FontColorGallery.SelectedColor.Value;
                SelectedDocument.Editor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, c);
                FontColorGallery.SelectedColor = null;
            }
        }

        private void HighlightColorGallery_SelectedColorChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (HighlightColorGallery.SelectedColor != null && HighlightColorGallery.SelectedColor.HasValue)
            {
                SolidColorBrush c = new SolidColorBrush();
                c.Color = HighlightColorGallery.SelectedColor.Value;
                SelectedDocument.Editor.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, c);
                HighlightColorGallery.SelectedColor = null;
            }
        }

        #endregion

        #region "Blod/Italic/Underline/Strikethrough"

        private void BoldMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.ToggleBold.Execute(null, SelectedDocument.Editor);
            UpdateSelected();
        }

        private void ItalicMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.ToggleItalic.Execute(null, SelectedDocument.Editor);
            UpdateSelected();
        }

        private void UnderlineMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.ToggleUnderline.Execute(null, SelectedDocument.Editor);
            UpdateSelected();
        }

        private void StrikethroughButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.ToggleStrikethrough();
            UpdateSelected();
        }

        #endregion

        #region "Subscript/Superscript"

        private void SubscriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.ToggleSubscript();
            UpdateSelected();
        }

        private void SuperscriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.ToggleSuperscript();
            UpdateSelected();
        }

        #endregion

        #region "Indent More/Indent Less"

        private void IndentMoreButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.IncreaseIndentation.Execute(null, SelectedDocument.Editor);
        }

        private void IndentLessButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.DecreaseIndentation.Execute(null, SelectedDocument.Editor);
        }

        #endregion

        #region "Bullet List/Number List"

        private void SetListStyle(TextMarkerStyle s)
        {
            Run run = SelectedDocument.Editor.CaretPosition.Parent as Run;
            if (run != null)
            {
                ListItem li = run.Parent as ListItem;
                if (li != null)
                {
                    MessageBox.Show(li.Parent.ToString());
                }
                else
                {
                    Paragraph p = run.Parent as Paragraph;
                    if (p != null)
                    {
                        ListItem listitem = p.Parent as ListItem;
                        if (listitem != null)
                        {
                            List list2 = listitem.Parent as List;
                            if (list2 != null)
                            {
                                list2.MarkerStyle = s;
                            }
                        }
                        else
                        {
                            EditingCommands.ToggleBullets.Execute(null, SelectedDocument.Editor);
                            Paragraph p2 = SelectedDocument.Editor.CaretPosition.Parent as Paragraph;
                            if (p2 != null)
                            {
                                ListItem listitem2 = p2.Parent as ListItem;
                                if (listitem2 != null)
                                {
                                    List list2 = listitem2.Parent as List;
                                    if (list2 != null)
                                    {
                                        list2.MarkerStyle = s;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Paragraph p = SelectedDocument.Editor.CaretPosition.Parent as Paragraph;
                if (p != null)
                {
                    ListItem listitem = p.Parent as ListItem;
                    if (listitem != null)
                    {
                        List list1 = listitem.Parent as List;
                        if (list1 != null)
                        {
                            list1.MarkerStyle = s;
                        }
                    }
                    else
                    {
                        EditingCommands.ToggleBullets.Execute(null, SelectedDocument.Editor);
                        Paragraph p2 = SelectedDocument.Editor.CaretPosition.Parent as Paragraph;
                        if (p2 != null)
                        {
                            ListItem listitem2 = p2.Parent as ListItem;
                            if (listitem2 != null)
                            {
                                List list2 = listitem2.Parent as List;
                                if (list2 != null)
                                {
                                    list2.MarkerStyle = s;
                                }
                            }
                        }
                    }
                }
                else
                {
                    TableCell tablecell = SelectedDocument.Editor.CaretPosition.Parent as TableCell;
                    if (tablecell != null)
                    {
                        Paragraph par = new Paragraph();
                        tablecell.Blocks.Add(par);
                        EditingCommands.ToggleBullets.Execute(null, SelectedDocument.Editor);
                        Paragraph p2 = SelectedDocument.Editor.CaretPosition.Parent as Paragraph;
                        if (p2 != null)
                        {
                            ListItem listitem2 = p2.Parent as ListItem;
                            if (listitem2 != null)
                            {
                                List list2 = listitem2.Parent as List;
                                if (list2 != null)
                                {
                                    list2.MarkerStyle = s;
                                }
                            }
                        }
                    }
                }
            }
            UpdateSelected();
        }

        private void BulletListMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.ToggleBullets.Execute(null, SelectedDocument.Editor);
            UpdateSelected();
        }

        private void DiskBulletButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetListStyle(TextMarkerStyle.Disc);
        }

        private void CircleBulletButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetListStyle(TextMarkerStyle.Circle);
        }

        private void BoxBulletButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetListStyle(TextMarkerStyle.Box);
        }

        private void SquareBulletButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetListStyle(TextMarkerStyle.Square);
        }

        private void NumberListMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.ToggleNumbering.Execute(null, SelectedDocument.Editor);
            UpdateSelected();
        }

        private void DecimalListButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetListStyle(TextMarkerStyle.Decimal);
        }

        private void UpperLatinListButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetListStyle(TextMarkerStyle.UpperLatin);
        }

        private void LowerLatinListButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetListStyle(TextMarkerStyle.LowerLatin);
        }

        private void UpperRomanListButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetListStyle(TextMarkerStyle.UpperRoman);
        }

        private void LowerRomanListButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetListStyle(TextMarkerStyle.LowerRoman);
        }

        #endregion

        #region "Align Left/Center/Right/Justify"

        private void AlignLeftMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.AlignLeft.Execute(null, SelectedDocument.Editor);
            UpdateSelected();
        }

        private void AlignCenterMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.AlignCenter.Execute(null, SelectedDocument.Editor);
            UpdateSelected();
        }

        private void AlignRightMenuItem_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.AlignRight.Execute(null, SelectedDocument.Editor);
            UpdateSelected();
        }

        private void AlignJustifyMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.AlignJustify.Execute(null, SelectedDocument.Editor);
            UpdateSelected();
        }

        #endregion

        #region "Line Spacing"

        private void LineSpacing1Point0_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
            {
                SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight = 1.0;
                UpdateSelected();
            }
        }

        private void LineSpacing1Point15_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
            {
                SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight = 1.15;
                UpdateSelected();
            }
        }

        private void LineSpacing1Point5_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
            {
                SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight = 1.5;
                UpdateSelected();
            }
        }

        private void LineSpacing2Point0_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
            {
                SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight = 2.0;
                UpdateSelected();
            }
        }

        private void LineSpacing2Point5_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
            {
                SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight = 2.5;
                UpdateSelected();
            }
        }

        private void LineSpacing3Point0_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
            {
                SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight = 3.0;
                UpdateSelected();
            }
        }

        private void CustomLineSpacingMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LineSpacingDialog d = new LineSpacingDialog(this);
            if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
                d.number = SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight;
            d.ShowDialog();
            if (d.Res == "OK")
            {
                try
                {
                    SelectedDocument.Editor.CaretPosition.Paragraph.LineHeight = d.number;
                    UpdateSelected();
                }
                catch (Exception ex)
                {
                    VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = ex.Message, MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
                    vbMessagebox.ShowMessageBox();
                }
            }
        }

        #endregion

        #region "ltr/rtl"

        private void LeftToRightButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Document.FlowDirection = FlowDirection.LeftToRight;
        }

        private void RightToLeftButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.Document.FlowDirection = FlowDirection.RightToLeft;
        }

        #endregion

        #endregion

        #region "--Page Layout"

        #region "Margins"

        private void NoneMarginsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedDocument.Editor.SetPageMargins(new Thickness(0, 0, 0, 0));
            UpdateButtons();
        }

        private void NormalMarginsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedDocument.Editor.SetPageMargins(new Thickness(96, 96, 96, 96));
            UpdateButtons();
        }

        private void NarrowMarginsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedDocument.Editor.SetPageMargins(new Thickness(48, 48, 48, 48));
            UpdateButtons();
        }

        private void LeftMarginBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                SelectedDocument.Editor.SetPageMargins(new Thickness(LeftMarginBox.Value, SelectedDocument.Editor.docpadding.Top, SelectedDocument.Editor.docpadding.Right, SelectedDocument.Editor.docpadding.Bottom));
                UpdateButtons();
            }
        }

        private void TopMarginBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                SelectedDocument.Editor.SetPageMargins(new Thickness(SelectedDocument.Editor.docpadding.Left, TopMarginBox.Value, SelectedDocument.Editor.docpadding.Right, SelectedDocument.Editor.docpadding.Bottom));
                UpdateButtons();
            }
        }

        private void RightMarginBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                SelectedDocument.Editor.SetPageMargins(new Thickness(SelectedDocument.Editor.docpadding.Left, SelectedDocument.Editor.docpadding.Top, RightMarginBox.Value, SelectedDocument.Editor.docpadding.Bottom));
                UpdateButtons();
            }
        }

        private void BottomMarginBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                SelectedDocument.Editor.SetPageMargins(new Thickness(SelectedDocument.Editor.docpadding.Left, SelectedDocument.Editor.docpadding.Top, SelectedDocument.Editor.docpadding.Right, BottomMarginBox.Value));
                UpdateButtons();
            }
        }

        #endregion

        #region "Size"

        private void NormalPageSizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedDocument.SetPageSize(1056, 816);
            UpdateButtons();
        }

        private void WidePageSizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedDocument.SetPageSize(816, 1056);
            UpdateButtons();
        }

        private void PageHeightBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                SelectedDocument.SetPageSize(PageHeightBox.Value, SelectedDocument.Editor.Document.PageWidth);
                UpdateButtons();
            }
        }

        private void PageWidthBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                SelectedDocument.SetPageSize(SelectedDocument.Editor.Document.PageHeight, PageWidthBox.Value);
                UpdateButtons();
            }
        }

        #endregion

        private void BackgroundColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            SolidColorBrush b = new SolidColorBrush(BackgroundColorGallery.SelectedColor.Value);
            SelectedDocument.Editor.Document.Background = b;
        }

        #endregion

        #region "--NavigationMenuItem"

        private void LineDownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.LineDown();
        }

        private void LineUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.LineUp();
        }

        private void LineLeftButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.LineLeft();
        }

        private void LineRightButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.LineRight();
        }

        private void PageDownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.PageDown();
        }

        private void PageUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.PageUp();
        }

        private void PageLeftButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.PageLeft();
        }

        private void PageRightButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedDocument.Editor.PageRight();
        }

        private void StartButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.MoveToDocumentStart.Execute(null, SelectedDocument.Editor);
        }

        private void EndButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EditingCommands.MoveToDocumentEnd.Execute(null, SelectedDocument.Editor);
        }

        #endregion

        #region "--ToolsMenuItem"

        private void SpellCheckButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SpellCheckDialog d = new SpellCheckDialog(this);
            TextPointer cp = SelectedDocument.Editor.Selection.Start.GetPositionAtOffset(1);
            SpellingError sp = SelectedDocument.Editor.GetSpellingError(cp);
            if (sp != null)
            {
                foreach (string i in sp.Suggestions)
                {
                    d.WordListBox.Items.Add(i);
                }
                d.WordListBox.SelectedIndex = 0;
                d.WordListBox.Focus();
            }
            d.ShowDialog();
            if (d.Res == "OK")
            {
                sp.Correct(d.WordListBox.SelectedItem as String);
            }
        }

        private void PreviousSpellingErrorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextPointer tp = SelectedDocument.Editor.GetNextSpellingErrorPosition(SelectedDocument.Editor.Selection.Start, LogicalDirection.Backward);
                TextRange tr = SelectedDocument.Editor.GetSpellingErrorRange(tp);
                SelectedDocument.Editor.Selection.Select(tr.Start, tr.End);
            }
            catch (Exception ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                this.Root().Messages.LogException("Document.Editor.FlowDocEditor", "PreviousSpellingErrorMenuItem_Click", msg);
            }
        }

        private void NextSpellingErrorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextPointer tp = SelectedDocument.Editor.GetNextSpellingErrorPosition(SelectedDocument.Editor.Selection.End, LogicalDirection.Forward);
                TextRange tr = SelectedDocument.Editor.GetSpellingErrorRange(tp);
                SelectedDocument.Editor.Selection.Select(tr.Start, tr.End);
            }
            catch (Exception ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                this.Root().Messages.LogException("Document.Editor.FlowDocEditor", "NextSpellingErrorMenuItem_Click", msg);
            }
        }

        private void IgnoreSpellingErrorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TextPointer cp = SelectedDocument.Editor.Selection.Start.GetPositionAtOffset(1);
            SpellingError sp = SelectedDocument.Editor.GetSpellingError(cp);
            if (sp != null)
            {
                TextRange r = SelectedDocument.Editor.GetSpellingErrorRange(cp);
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\spellcheck_ignorelist.lex"))
                {
                    StreamWriter sr = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "\\spellcheck_ignorelist.lex");
                    sr.Close();
                }
                StreamReader fileIn = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\spellcheck_ignorelist.lex");
                string strData = "";
                long lngCount = 1;
                bool canadd = true;
                while ((!(fileIn.EndOfStream)))
                {
                    strData = fileIn.ReadLine();
                    if (object.ReferenceEquals(strData, r.Text))
                    {
                        canadd = false;
                    }
                    lngCount = lngCount + 1;
                }
                fileIn.Close();
                if (canadd)
                {
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\spellcheck_ignorelist.lex", "\n");
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\spellcheck_ignorelist.lex", r.Text);
                }
                sp.IgnoreAll();
            }
        }

        private void CorrectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                TextPointer sp = SelectedDocument.Editor.GetNextSpellingErrorPosition(SelectedDocument.Editor.Document.ContentStart.DocumentStart, LogicalDirection.Forward);
                if (sp == null)
                {
                    break; // TODO: might not be correct. Was : Exit While
                }
                SpellingError se = SelectedDocument.Editor.GetSpellingError(SelectedDocument.Editor.GetNextSpellingErrorPosition(SelectedDocument.Editor.Document.ContentStart.DocumentStart, LogicalDirection.Forward));
                if (se != null)
                {
                    foreach (string i in se.Suggestions)
                    {
                        se.Correct(i);
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }
                else
                {
                    break; // TODO: might not be correct. Was : Exit While
                }
            }
        }

        private void TextToSpeechButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Rate = Properties.Settings.Default.Options_TTSS;
            //if (State == System.SynthesizerState.Speaking)
            //{
            //    SpeakAsyncCancelAll();
            //}
            //else
            //{
            //    try
            //    {
            //        SelectVoice(GetInstalledVoices.Item(Properties.Settings.Default.Options_TTSV).VoiceInfo.Name);
            //        SpeakAsync(SelectedDocument.Editor.Selection.Text.ToString);
            //    }
            //    catch (Exception ex)
            //    {
            //VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = ex.Message, MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
            //    }
            //}
        }

        private void TranslateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TranslateDialog trans = new TranslateDialog(SelectedDocument.Editor.Selection.Text.ToString(), this);
            trans.ShowDialog();
            if (trans.res == true)
            {
                SelectedDocument.Editor.Selection.Text = trans.TranslatedText.Content as string;
            }
        }

        private void DefinitionsButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (SelectedDocument.Editor.Selection.Text.Length > 0)
            {
                Process.Start("http://www.bing.com/Dictionary/search?q=define+" + SelectedDocument.Editor.Selection.Text);
            }
            else
            {
                VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = "Select a word first.", MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
                vbMessagebox.ShowMessageBox();
            }
        }


        #endregion

        #region "Contextual Groups"

        #region "Update"

        #region "Table Cell"

        private void SetSelectedTableCell(TableCell tablecell)
        {
            SelectedDocument.Editor.SelectedTableCell = tablecell;
            TableRow tr = SelectedDocument.Editor.SelectedTableCell.Parent as TableRow;
            TableRowGroup trg = tr.Parent as TableRowGroup;
            Table table = trg.Parent as Table;
            TableBorderSizeBox.Value = table.BorderThickness.Left;
            TableCellSpacingBox.Value = table.CellSpacing;
            TableCellBorderSizeBox.Value = tablecell.BorderThickness.Left;
            EditTableCellGroup.Visibility = Visibility.Visible;
        }

        #endregion

        #region "Image"

        private void SetSelectedImage(Image Img)
        {
            SelectedDocument.Editor.SelectedImage = new Image();
            ImageHeightBox.Value = Img.Height;
            ImageWidthBox.Value = Img.Width;
            if (Img.Stretch == Stretch.Fill)
            {
                ImageResizeModeComboBox.SelectedIndex = 0;
            }
            else if (Img.Stretch == Stretch.Uniform)
            {
                ImageResizeModeComboBox.SelectedIndex = 1;
            }
            else if (Img.Stretch == Stretch.UniformToFill)
            {
                ImageResizeModeComboBox.SelectedIndex = 2;
            }
            else if (Img.Stretch == Stretch.None)
            {
                ImageResizeModeComboBox.SelectedIndex = 3;
            }
            SelectedDocument.Editor.SelectedImage = Img;
            EditTableCellGroup.Visibility = Visibility.Collapsed;
            EditImageGroup.Visibility = Visibility.Visible;
            EditVideoGroup.Visibility = Visibility.Collapsed;
            EditShapeGroup.Visibility = Visibility.Collapsed;
            EditObjectGroup.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region "Video"

        private void SetSelectedVideo(MediaElement vid)
        {
            SelectedDocument.Editor.SelectedVideo = vid;
            VideoHeightBox.Value = vid.Height;
            VideoWidthBox.Value = vid.Width;
            if (vid.Stretch == Stretch.Fill)
            {
                VideoResizeModeComboBox.SelectedIndex = 0;
            }
            else if (vid.Stretch == Stretch.Uniform)
            {
                VideoResizeModeComboBox.SelectedIndex = 1;
            }
            else if (vid.Stretch == Stretch.UniformToFill)
            {
                VideoResizeModeComboBox.SelectedIndex = 2;
            }
            else if (vid.Stretch == Stretch.None)
            {
                VideoResizeModeComboBox.SelectedIndex = 3;
            }
            //TODO:
            EditTableCellGroup.Visibility = Visibility.Collapsed;
            EditImageGroup.Visibility = Visibility.Collapsed;
            EditVideoGroup.Visibility = Visibility.Visible;
            EditShapeGroup.Visibility = Visibility.Collapsed;
            EditObjectGroup.Visibility = Visibility.Collapsed;
        }

        #endregion
        //TODO:
        #region "Shape"

        private void SetSelectedShape(Shape s)
        {
            SelectedDocument.Editor.SelectedShape = s;
            ShapeHeightBox.Value = s.Height;
            ShapeWidthBox.Value = s.Width;
            //If vid.Stretch = Stretch.Fill Then
            //    VideoResizeModeComboBox.SelectedIndex = 0
            //ElseIf vid.Stretch = Stretch.Uniform Then
            //    VideoResizeModeComboBox.SelectedIndex = 1
            //ElseIf vid.Stretch = Stretch.UniformToFill Then
            //    VideoResizeModeComboBox.SelectedIndex = 2
            //ElseIf vid.Stretch = Stretch.None Then
            //    VideoResizeModeComboBox.SelectedIndex = 3
            //End If
            //TODO:
            EditTableCellGroup.Visibility = Visibility.Collapsed;
            EditImageGroup.Visibility = Visibility.Collapsed;
            EditVideoGroup.Visibility = Visibility.Collapsed;
            EditShapeGroup.Visibility = Visibility.Visible;
            EditObjectGroup.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region "Object"

        private void SetSelectedObject(UIElement uielement)
        {
            if (uielement is System.Windows.Controls.Button || uielement is System.Windows.Controls.RadioButton || uielement is System.Windows.Controls.CheckBox || uielement is System.Windows.Controls.TextBlock)
            {
                SelectedDocument.Editor.SelectedObject = uielement;
                ObjectBorderGroup.Visibility = Visibility.Visible;
                System.Windows.Controls.Button button = uielement as System.Windows.Controls.Button;
                if (button != null)
                {
                    ObjectHeightBox.Value = button.Height;
                    ObjectWidthBox.Value = button.Width;
                    ObjectBorderSizeBox.Value = button.BorderThickness.Left;
                    ObjectTextBox.Text = button.Content.ToString();
                    if (button.IsEnabled)
                    {
                        ObjectEnabledCheckBox.IsChecked = true;
                    }
                    else
                    {
                        ObjectEnabledCheckBox.IsChecked = false;
                    }
                }
                else
                {
                    System.Windows.Controls.RadioButton radiobutton = uielement as System.Windows.Controls.RadioButton;
                    if (radiobutton != null)
                    {
                        ObjectHeightBox.Value = radiobutton.Height;
                        ObjectWidthBox.Value = radiobutton.Width;
                        ObjectBorderSizeBox.Value = radiobutton.BorderThickness.Left;
                        ObjectTextBox.Text = radiobutton.Content.ToString();
                        if (radiobutton.IsEnabled)
                        {
                            ObjectEnabledCheckBox.IsChecked = true;
                        }
                        else
                        {
                            ObjectEnabledCheckBox.IsChecked = false;
                        }
                    }
                    else
                    {
                        System.Windows.Controls.CheckBox checkbox = uielement as System.Windows.Controls.CheckBox;
                        if (checkbox != null)
                        {
                            ObjectHeightBox.Value = checkbox.Height;
                            ObjectWidthBox.Value = checkbox.Width;
                            ObjectBorderSizeBox.Value = checkbox.BorderThickness.Left;
                            ObjectTextBox.Text = checkbox.Content.ToString();
                            if (checkbox.IsEnabled)
                            {
                                ObjectEnabledCheckBox.IsChecked = true;
                            }
                            else
                            {
                                ObjectEnabledCheckBox.IsChecked = false;
                            }
                        }
                        else
                        {
                            TextBlock textblock = uielement as TextBlock;
                            if (textblock != null)
                            {
                                ObjectHeightBox.Value = textblock.Height;
                                ObjectWidthBox.Value = textblock.Width;
                                ObjectBorderGroup.Visibility = Visibility.Collapsed;
                                ObjectTextBox.Text = textblock.Text;
                                if (textblock.IsEnabled)
                                {
                                    ObjectEnabledCheckBox.IsChecked = true;
                                }
                                else
                                {
                                    ObjectEnabledCheckBox.IsChecked = false;
                                }
                            }
                        }
                    }
                }
                EditTableCellGroup.Visibility = Visibility.Collapsed;
                EditImageGroup.Visibility = Visibility.Collapsed;
                EditVideoGroup.Visibility = Visibility.Collapsed;
                EditObjectGroup.Visibility = Visibility.Visible;
            }
        }

        #endregion

        private void UpdateContextualTabs()
        {
            if (!SelectedDocument.Editor.Selection.IsEmpty)
            {
                foreach (Block block in SelectedDocument.Editor.Document.Blocks)
                {
                    BlockUIContainer blockui = block as BlockUIContainer;
                    if (blockui != null)
                    {
                        UIElement uielement = blockui.Child as UIElement;
                        if (uielement != null)
                        {
                            if (uielement is Image)
                            {
                                SetSelectedImage(uielement as Image);
                                return;
                            }
                            else
                            {
                                SetSelectedObject(uielement);
                                return;
                            }
                        }
                    }
                    else
                    {
                        List list = block as List;
                        if (list != null)
                        {
                            foreach (ListItem litem in list.ListItems)
                            {
                                foreach (Block b in litem.Blocks)
                                {
                                    BlockUIContainer bui = block as BlockUIContainer;
                                    if (bui != null)
                                    {
                                        //TODO: (20xx.xx) add support for blockui inside of listitems
                                    }
                                    else
                                    {
                                        Paragraph p = b as Paragraph;
                                        if (p != null)
                                        {
                                            foreach (Inline i in p.Inlines)
                                            {
                                                InlineUIContainer iui = i as InlineUIContainer;
                                                if (iui != null)
                                                {
                                                    if (SelectedDocument.Editor.Selection.Start.CompareTo(iui.ElementStart) == 0 && SelectedDocument.Editor.Selection.End.CompareTo(iui.ElementEnd) == 0)
                                                    {
                                                        if (iui.Child is Image)
                                                        {
                                                            SetSelectedImage(iui.Child as Image);
                                                            return;
                                                        }
                                                        else if (iui.Child is MediaElement)
                                                        {
                                                            SetSelectedVideo(iui.Child as MediaElement);
                                                            return;
                                                        }
                                                        else if (iui.Child is Shape)
                                                        {
                                                            SetSelectedShape(iui.Child as Shape);
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            SetSelectedObject(iui.Child);
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //TODO: (20xx.xx) add suport for the rest of the "Container" types
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Paragraph par = block as Paragraph;
                            if (par != null)
                            {
                                foreach (Inline inline in par.Inlines)
                                {
                                    InlineUIContainer inlineui = inline as InlineUIContainer;
                                    if (inlineui != null)
                                    {
                                        if (SelectedDocument.Editor.Selection.Start.CompareTo(inlineui.ElementStart) == 0 && SelectedDocument.Editor.Selection.End.CompareTo(inlineui.ElementEnd) == 0)
                                        {
                                            if (inlineui.Child is Image)
                                            {
                                                SetSelectedImage(inlineui.Child as Image);
                                                return;
                                            }
                                            else if (inlineui.Child is MediaElement)
                                            {
                                                SetSelectedVideo(inlineui.Child as MediaElement);
                                                return;
                                            }
                                            else if (inlineui.Child is Shape)
                                            {
                                                SetSelectedShape(inlineui.Child as Shape);
                                                return;
                                            }
                                            else
                                            {
                                                SetSelectedObject(inlineui.Child);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Section sec = block as Section;
                                if (sec != null)
                                {
                                    //TODO: (20xx.xx) add support for sections
                                }
                                else
                                {
                                    Table table = block as Table;
                                    if (table != null)
                                    {
                                        foreach (TableRowGroup rowgroup in table.RowGroups)
                                        {
                                            foreach (TableRow row in rowgroup.Rows)
                                            {
                                                foreach (TableCell cell in row.Cells)
                                                {
                                                    foreach (Block b in cell.Blocks)
                                                    {
                                                        BlockUIContainer bui = b as BlockUIContainer;
                                                        if (bui != null)
                                                        {
                                                            UIElement uie = bui.Child as UIElement;
                                                            if (uie != null)
                                                            {
                                                                if (uie is Image)
                                                                {
                                                                    SetSelectedImage(uie as Image);
                                                                    return;
                                                                }
                                                                else if (uie is MediaElement)
                                                                {
                                                                    SetSelectedVideo(uie as MediaElement);
                                                                    return;
                                                                }
                                                                else if (uie is Shape)
                                                                {
                                                                    SetSelectedShape(uie as Shape);
                                                                    return;
                                                                }
                                                                else
                                                                {
                                                                    SetSelectedObject(uie);
                                                                    return;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            List l = b as List;
                                                            if (l != null)
                                                            {
                                                                //For Each litem As ListItem In l.ListItems
                                                                //    For Each b2 As Block In litem.Blocks
                                                                //        Dim bui2 As BlockUIContainer = TryCast(b2, BlockUIContainer)
                                                                //        If bui2 IsNot Nothing Then
                                                                //            'TODO: add support for blockui inside of listitems inside of tables
                                                                //        Else
                                                                //            Dim p As Paragraph = TryCast(b2, Paragraph)
                                                                //            If p IsNot Nothing Then
                                                                //                For Each i As Inline In p.Inlines
                                                                //                    Dim iui As InlineUIContainer = TryCast(i, InlineUIContainer)
                                                                //                    If iui IsNot Nothing Then
                                                                //                        If SelectedDocument.Editor.Selection.Start.CompareTo(iui.ElementStart) = 0 AndAlso SelectedDocument.Editor.Selection.End.CompareTo(iui.ElementEnd) = 0 Then
                                                                //                            If iui.Child.GetType Is GetType(Image) Then
                                                                //                                SetSelectedImage(iui.Child)
                                                                //                                Exit Sub
                                                                //                            Else
                                                                //                                SetSelectedObject(iui.Child)
                                                                //                                Exit Sub
                                                                //                            End If
                                                                //                        End If
                                                                //                    End If
                                                                //                Next
                                                                //            Else
                                                                //                'TODO: add suport for the rest of the "Container" types
                                                                //            End If
                                                                //        End If
                                                                //    Next
                                                                //Next
                                                            }
                                                            else
                                                            {
                                                                Paragraph p = b as Paragraph;
                                                                if (p != null)
                                                                {
                                                                    foreach (Inline inl in p.Inlines)
                                                                    {
                                                                        InlineUIContainer inlui = inl as InlineUIContainer;
                                                                        if (inlui != null)
                                                                        {
                                                                            if (SelectedDocument.Editor.Selection.Start.CompareTo(inlui.ElementStart) == 0 && SelectedDocument.Editor.Selection.End.CompareTo(inlui.ElementEnd) == 0)
                                                                            {
                                                                                if (inlui.Child is Image)
                                                                                {
                                                                                    SetSelectedImage(inlui.Child as Image);
                                                                                    return;
                                                                                }
                                                                                else if (inlui.Child is MediaElement)
                                                                                {
                                                                                    SetSelectedVideo(inlui.Child as MediaElement);
                                                                                    return;
                                                                                }
                                                                                else if (inlui.Child is Shape)
                                                                                {
                                                                                    SetSelectedShape(inlui.Child as Shape);
                                                                                    return;
                                                                                }
                                                                                else
                                                                                {
                                                                                    SetSelectedObject(inlui.Child);
                                                                                    return;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Section s = b as Section;
                                                                    if (s != null)
                                                                    {
                                                                        //TODO:(20xx.xx) add support for sections inside of tables
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                EditTableCellGroup.Visibility = Visibility.Collapsed;
                EditImageGroup.Visibility = Visibility.Collapsed;
                EditVideoGroup.Visibility = Visibility.Collapsed;
                EditShapeGroup.Visibility = Visibility.Collapsed;
                EditObjectGroup.Visibility = Visibility.Collapsed;
                if (SelectedDocument.Editor.CaretPosition.Paragraph != null)
                {
                    Paragraph p = SelectedDocument.Editor.CaretPosition.Paragraph;
                    if (p.Parent is TableCell)
                    {
                        SetSelectedTableCell(p.Parent as TableCell);
                    }
                }
                if (SelectedDocument.Editor.CaretPosition.Parent is TableCell)
                {
                    SetSelectedTableCell(SelectedDocument.Editor.CaretPosition.Parent as TableCell);
                }
                //Dim m As New MessageBoxDialog(SelectedDocument.Editor.CaretPosition.GetType.ToString, "Info!", Nothing, Nothing)
                //m.MessageImage.Source = New BitmapImage(New Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/info32.png", UriKind.Relative))
                //m.Owner = Me
                //m.ShowDialog()
            }
        }

        #endregion

        #region "EditImageTab"

        #region "Resize"

        private void ImageHeightBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                SelectedDocument.Editor.SelectedImage.Height = ImageHeightBox.Value;
            }
        }

        private void ImageWidthBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                SelectedDocument.Editor.SelectedImage.Width = ImageWidthBox.Value;
            }
        }

        private void ImageResizeModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageResizeModeComboBox.SelectedIndex == 0)
            {
                SelectedDocument.Editor.SelectedImage.Stretch = Stretch.Fill;
            }
            else if (ImageResizeModeComboBox.SelectedIndex == 1)
            {
                SelectedDocument.Editor.SelectedImage.Stretch = Stretch.Uniform;
            }
            else if (ImageResizeModeComboBox.SelectedIndex == 2)
            {
                SelectedDocument.Editor.SelectedImage.Stretch = Stretch.UniformToFill;
            }
            else if (ImageResizeModeComboBox.SelectedIndex == 3)
            {
                SelectedDocument.Editor.SelectedImage.Stretch = Stretch.None;
            }
        }

        #endregion

        #region "Rotate"

        private void RotateImageLeftMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Thickness imgprops = (Thickness)SelectedDocument.Editor.SelectedImage.Tag;
            imgprops.Left -= 90;
            SelectedDocument.Editor.SelectedImage.Tag = imgprops;
            TransformGroup transform = SelectedDocument.Editor.SelectedImage.LayoutTransform as TransformGroup;
            RotateTransform rotate = new RotateTransform(imgprops.Left);
            transform.Children[0] = rotate;
            SelectedDocument.Editor.SelectedImage.LayoutTransform = transform;
        }

        private void RotateImageRightMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Thickness imgprops = (Thickness)SelectedDocument.Editor.SelectedImage.Tag;
            imgprops.Left += 90;
            SelectedDocument.Editor.SelectedImage.Tag = imgprops;
            TransformGroup transform = SelectedDocument.Editor.SelectedImage.LayoutTransform as TransformGroup;
            RotateTransform rotate = new RotateTransform(imgprops.Left);
            transform.Children[0] = rotate;
            SelectedDocument.Editor.SelectedImage.LayoutTransform = transform;
        }

        #endregion

        #region "Flip"

        private void FlipImageHorizontalMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //TODO: (20xx.xx) add flip image horizontal support
            Thickness imgprops = (Thickness)SelectedDocument.Editor.SelectedImage.Tag;
            if (imgprops.Top == 1)
            {
                imgprops.Top = -1;
            }
            else
            {
                imgprops.Top = 1;
            }
            SelectedDocument.Editor.SelectedImage.Tag = imgprops;
            TransformGroup transform = SelectedDocument.Editor.SelectedImage.LayoutTransform as TransformGroup;
            ScaleTransform hflip = new ScaleTransform(imgprops.Top, imgprops.Right);
            transform.Children[1] = hflip;
            SelectedDocument.Editor.SelectedImage.LayoutTransform = transform;
        }

        private void FlipImageVerticalMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //TODO: (20xx.xx) add flip image vertical support
            Thickness imgprops = (Thickness)SelectedDocument.Editor.SelectedImage.Tag;
            if (imgprops.Right == 1)
            {
                imgprops.Right = -1;
            }
            else
            {
                imgprops.Right = 1;
            }
            SelectedDocument.Editor.SelectedImage.Tag = imgprops;
            TransformGroup transform = SelectedDocument.Editor.SelectedImage.LayoutTransform as TransformGroup;
            ScaleTransform vflip = new ScaleTransform(imgprops.Top, imgprops.Right);
            transform.Children[2] = vflip;
            SelectedDocument.Editor.SelectedImage.LayoutTransform = vflip;
        }

        #endregion

        #endregion

        #region "EditVideoTab"

        #region "Resize"

        private void VideoHeightBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                SelectedDocument.Editor.SelectedVideo.Height = VideoHeightBox.Value;
            }
        }

        private void VideoWidthBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                SelectedDocument.Editor.SelectedVideo.Width = VideoWidthBox.Value;
            }
        }

        private void VideoResizeModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VideoResizeModeComboBox.SelectedIndex == 0)
            {
                SelectedDocument.Editor.SelectedVideo.Stretch = Stretch.Fill;
            }
            else if (VideoResizeModeComboBox.SelectedIndex == 1)
            {
                SelectedDocument.Editor.SelectedVideo.Stretch = Stretch.Uniform;
            }
            else if (VideoResizeModeComboBox.SelectedIndex == 2)
            {
                SelectedDocument.Editor.SelectedVideo.Stretch = Stretch.UniformToFill;
            }
            else if (VideoResizeModeComboBox.SelectedIndex == 3)
            {
                SelectedDocument.Editor.SelectedVideo.Stretch = Stretch.None;
            }
        }

        #endregion

        private void VideoPlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedDocument.Editor.SelectedVideo.Play();
        }

        #endregion

        #region "EditTableCellTab"

        //private int LastSelectedTableEditingTab = 0;
        private void MainBar_SelectedTabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (object.ReferenceEquals(MainBar.SelectedTabItem, EditTableTab))
            {
                //LastSelectedTableEditingTab = 0;
            }
            else if (object.ReferenceEquals(MainBar.SelectedTabItem, EditTableCellTab))
            {
                //LastSelectedTableEditingTab = 1;
            }
        }

        #region "EditTableTab"

        private void TableBackgroundColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            TableRow tr = SelectedDocument.Editor.SelectedTableCell.Parent as TableRow;
            TableRowGroup trg = tr.Parent as TableRowGroup;
            Table table = trg.Parent as Table;
            table.Background = new SolidColorBrush(TableBackgroundColorGallery.SelectedColor.Value);
        }

        private void TableBorderColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            TableRow tr = SelectedDocument.Editor.SelectedTableCell.Parent as TableRow;
            TableRowGroup trg = tr.Parent as TableRowGroup;
            Table table = trg.Parent as Table;
            table.BorderBrush = new SolidColorBrush(TableBorderColorGallery.SelectedColor.Value);
        }

        private void TableBorderSizeBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                TableRow tr = SelectedDocument.Editor.SelectedTableCell.Parent as TableRow;
                TableRowGroup trg = tr.Parent as TableRowGroup;
                Table table = trg.Parent as Table;
                int size = Convert.ToInt32(TableBorderSizeBox.Value);
                table.BorderThickness = new Thickness(size, size, size, size);
            }
        }

        private void TableCellSpacingBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                TableRow tr = SelectedDocument.Editor.SelectedTableCell.Parent as TableRow;
                TableRowGroup trg = tr.Parent as TableRowGroup;
                Table table = trg.Parent as Table;
                table.CellSpacing = TableCellSpacingBox.Value;
            }
        }

        #endregion

        private void TableCellBackgroundColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            SelectedDocument.Editor.SelectedTableCell.Background = new SolidColorBrush(TableCellBackgroundColorGallery.SelectedColor.Value);
        }

        private void TableCellBorderColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            SelectedDocument.Editor.SelectedTableCell.BorderBrush = new SolidColorBrush(TableCellBorderColorGallery.SelectedColor.Value);
        }

        private void TableCellBorderSizeBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                double size = TableCellBorderSizeBox.Value;
                SelectedDocument.Editor.SelectedTableCell.BorderThickness = new Thickness(size, size, size, size);
            }
        }

        #endregion

        #region "EditObjectTab"

        private void ObjectHeightBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                System.Windows.Controls.Button button = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.Height = ObjectHeightBox.Value;
                }
                else
                {
                    System.Windows.Controls.RadioButton radiobutton = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.RadioButton;
                    if (radiobutton != null)
                    {
                        radiobutton.Height = ObjectHeightBox.Value;
                    }
                    else
                    {
                        System.Windows.Controls.CheckBox checkbox = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.CheckBox;
                        if (checkbox != null)
                        {
                            checkbox.Height = ObjectHeightBox.Value;
                        }
                        else
                        {
                            TextBlock textblock = SelectedDocument.Editor.SelectedObject as TextBlock;
                            if (textblock != null)
                            {
                                textblock.Height = ObjectHeightBox.Value;
                            }
                        }
                    }
                }
            }
        }

        private void ObjectWidthBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                System.Windows.Controls.Button button = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.Width = ObjectWidthBox.Value;
                }
                else
                {
                    System.Windows.Controls.RadioButton radiobutton = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.RadioButton;
                    if (radiobutton != null)
                    {
                        radiobutton.Width = ObjectWidthBox.Value;
                    }
                    else
                    {
                        System.Windows.Controls.CheckBox checkbox = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.CheckBox;
                        if (checkbox != null)
                        {
                            checkbox.Width = ObjectWidthBox.Value;
                        }
                        else
                        {
                            TextBlock textblock = SelectedDocument.Editor.SelectedObject as TextBlock;
                            if (textblock != null)
                            {
                                textblock.Width = ObjectWidthBox.Value;
                            }
                        }
                    }
                }
            }
        }

        private void ObjectBackgroundColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.Button;
            if (button != null)
            {
                button.Background = new SolidColorBrush(ObjectBackgroundColorGallery.SelectedColor.Value);
            }
            else
            {
                System.Windows.Controls.RadioButton radiobutton = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.RadioButton;
                if (radiobutton != null)
                {
                    radiobutton.Background = new SolidColorBrush(ObjectBackgroundColorGallery.SelectedColor.Value);
                }
                else
                {
                    System.Windows.Controls.CheckBox checkbox = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.CheckBox;
                    if (checkbox != null)
                    {
                        checkbox.Background = new SolidColorBrush(ObjectBackgroundColorGallery.SelectedColor.Value);
                    }
                    else
                    {
                        TextBlock textblock = SelectedDocument.Editor.SelectedObject as TextBlock;
                        if (textblock != null)
                        {
                            textblock.Background = new SolidColorBrush(ObjectBackgroundColorGallery.SelectedColor.Value);
                        }
                    }
                }
            }
        }

        private void ObjectBorderColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button button = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.Button;
            if (button != null)
            {
                button.BorderBrush = new SolidColorBrush(ObjectBorderColorGallery.SelectedColor.Value);
            }
            else
            {
                System.Windows.Controls.RadioButton radiobutton = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.RadioButton;
                if (radiobutton != null)
                {
                    radiobutton.BorderBrush = new SolidColorBrush(ObjectBorderColorGallery.SelectedColor.Value);
                }
                else
                {
                    System.Windows.Controls.CheckBox checkbox = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.CheckBox;
                    if (checkbox != null)
                    {
                        checkbox.BorderBrush = new SolidColorBrush(ObjectBorderColorGallery.SelectedColor.Value);
                    }
                }
            }
        }

        private void ObjectBorderSizeBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                int size = Convert.ToInt32(ObjectBorderSizeBox.Value);
                System.Windows.Controls.Button button = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.BorderThickness = new Thickness(size, size, size, size);
                }
                else
                {
                    System.Windows.Controls.RadioButton radiobutton = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.RadioButton;
                    if (radiobutton != null)
                    {
                        radiobutton.BorderThickness = new Thickness(size, size, size, size);
                    }
                    else
                    {
                        System.Windows.Controls.CheckBox checkbox = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.CheckBox;
                        if (checkbox != null)
                        {
                            checkbox.BorderThickness = new Thickness(size, size, size, size);
                        }
                    }
                }
            }
        }

        private void ObjectTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.Button button = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.Button;
            if (button != null)
            {
                button.Content = ObjectTextBox.Text;
            }
            else
            {
                System.Windows.Controls.RadioButton radiobutton = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.RadioButton;
                if (radiobutton != null)
                {
                    radiobutton.Content = ObjectTextBox.Text;
                }
                else
                {
                    System.Windows.Controls.CheckBox checkbox = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.CheckBox;
                    if (checkbox != null)
                    {
                        checkbox.Content = ObjectTextBox.Text;
                    }
                    else
                    {
                        System.Windows.Controls.TextBlock textblock = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.TextBlock;
                        if (textblock != null)
                        {
                            textblock.Text = ObjectTextBox.Text;
                        }
                    }
                }
            }
        }

        private void ObjectEnabledCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (ObjectEnabledCheckBox.IsChecked.Value)
            {
                System.Windows.Controls.Button button = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = true;
                }
                else
                {
                    System.Windows.Controls.RadioButton radiobutton = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.RadioButton;
                    if (radiobutton != null)
                    {
                        radiobutton.IsEnabled = true;
                    }
                    else
                    {
                        System.Windows.Controls.CheckBox checkbox = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.CheckBox;
                        if (checkbox != null)
                        {
                            checkbox.IsEnabled = true;
                        }
                        else
                        {
                            System.Windows.Controls.TextBlock textblock = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.TextBlock;
                            if (textblock != null)
                            {
                                textblock.IsEnabled = true;
                            }
                        }
                    }
                }
            }
            else
            {
                System.Windows.Controls.Button button = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.Button;
                if (button != null)
                {
                    button.IsEnabled = false;
                }
                else
                {
                    System.Windows.Controls.RadioButton radiobutton = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.RadioButton;
                    if (radiobutton != null)
                    {
                        radiobutton.IsEnabled = false;
                    }
                    else
                    {
                        System.Windows.Controls.CheckBox checkbox = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.CheckBox;
                        if (checkbox != null)
                        {
                            checkbox.IsEnabled = false;
                        }
                        else
                        {
                            System.Windows.Controls.TextBlock textblock = SelectedDocument.Editor.SelectedObject as System.Windows.Controls.TextBlock;
                            if (textblock != null)
                            {
                                textblock.IsEnabled = false;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region "Toolbar Items"

        #region "--HelpMenuItem"

        #endregion

        #endregion

        #endregion

        #region "TabCell"

        private void CloseDoc(object sender, System.Windows.RoutedEventArgs e)
        {
            TabHeader b = e.Source as TabHeader;
            DocumentTab i = b.Parent as DocumentTab;
            //if (i.Editor.FileChanged)
            //{
            //    SaveFileDialog SaveDialog = new SaveFileDialog(this);
            //    FileStream fs = File.Open(System.IO.Path.GetTempPath() + "\\TVPre.xaml", FileMode.Create);
            //    TextRange tr = new TextRange(i.Editor.Document.ContentStart, i.Editor.Document.ContentEnd);
            //    SaveDialog.SetFileInfo(i.DocName, i.Editor);
            //    XamlWriter.Save(i.Editor.Document, fs);
            //    fs.Close();
            //    SaveDialog.ShowDialog();
            //    if (SaveDialog.Res == "Yes")
            //    {
            //        SaveMenuItem_Click(this, null);
            //        CloseDocument(i);
            //    }
            //    else if (SaveDialog.Res == "No")
            //    {
            //        CloseDocument(i);
            //    }
            //}
            //else
            //{
            //    CloseDocument(i);
            //}
        }

        DocumentTab t;
        int oldPosition = -1;
        private void TabCell_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.Source.ToString() == "Document.Editor.TabHeader")
            {
                TabHeader h = e.Source as TabHeader;
                t = h.Parent as DocumentTab;
                object o = TabCell.ItemContainerGenerator.IndexFromContainer(h.Parent as DocumentTab);
                oldPosition = Int16.Parse(o.ToString());
            }
        }

        private void TabCell_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.Source.ToString() == "Document.Editor.TabHeader")
            {
                TabHeader h = e.Source as TabHeader;
                object o = TabCell.ItemContainerGenerator.IndexFromContainer(h.Parent as DocumentTab);
                int i = Int16.Parse(o.ToString());
                if (o != null && !(oldPosition == i))
                {
                    TabCell.Items.RemoveAt(oldPosition);
                    TabCell.Items.Insert(i, t);
                    TabCell.SelectedItem = t;
                }
            }
        }

        private void TabCell_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TabCell.SelectedIndex == -1)
            {
                SelectedDocument = null;
                UpdateButtons();
            }
            else
            {
                if (SelectedDocument != null)
                {
                    SelectedDocument.HeaderContent.Padding = new Thickness(4, 5, 4, 5);
                    SelectedDocument.HeaderContent.FileTypeImage.Margin = new Thickness(0, 1, 0, 0);
                    SelectedDocument.HeaderContent.TabTitle.Margin = new Thickness(0, 0, 0, 0);
                    SelectedDocument.HeaderContent.CloseButton.Margin = new Thickness(0, 1, 0, 0);
                }
                SelectedDocument = TabCell.SelectedItem as DocumentTab;
                if (SelectedDocument != null)
                {
                    SelectedDocument.HeaderContent.Padding = new Thickness(4, 4, 4, 5);
                    SelectedDocument.HeaderContent.FileTypeImage.Margin = new Thickness(0, 1, 0, 0);
                    SelectedDocument.HeaderContent.TabTitle.Margin = new Thickness(0, 1, 0, 0);
                    SelectedDocument.HeaderContent.CloseButton.Margin = new Thickness(0, 3, 0, 0);
                }
                UpdateUI();
                UpdateButtons();
            }
        }

        #endregion

        #region "Statusbar"

        private void StatusbarInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
            }
        }

        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (zoomSlider.IsLoaded)
            {
                ScaleTransform t = new ScaleTransform(zoomSlider.Value / 100, zoomSlider.Value / 100);
                ScaleTransform rulerzoom = new ScaleTransform(zoomSlider.Value / 100, 1);
                SelectedDocument.Ruler.LayoutTransform = rulerzoom;
                SelectedDocument.Editor.LayoutTransform = t;
                SelectedDocument.Editor.ZoomLevel = zoomSlider.Value / 100;
                UpdateButtons();
            }
        }

        #endregion

        #region "Misc"

        private void DocPreviewScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            DocPreviewScrollViewer.Visibility = Visibility.Collapsed;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CommonViewGroup_LauncherClick()
        {

        }
    }
}
