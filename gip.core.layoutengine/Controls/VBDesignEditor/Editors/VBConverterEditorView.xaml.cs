// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
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
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using gip.core.datamodel;
using gip.ext.design.PropertyGrid;
using gip.ext.design;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Folding;

namespace gip.core.layoutengine.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a view editor for convertes.
    /// </summary>
    public partial class VBConverterEditorView : INotifyPropertyChanged //: ITypeEditorInitItem
	{
		/// <summary>
        /// Create a new VBConverterEditorView instance.
		/// </summary>
		public VBConverterEditorView()
		{
            // C#-Highlighting
            IHighlightingDefinition customHighlighting;
            string xshdFile = "gip.core.layoutengine.Controls.VBDesignEditor.Editors.Highlighting.CSharp-ModeStyleGip.xshd";
            if (ControlManager.WpfTheme == eWpfTheme.Aero)
                xshdFile = "gip.core.layoutengine.Controls.VBDesignEditor.Editors.Highlighting.CSharp-ModeStyleAero.xshd";

            using (Stream s = this.GetType().Assembly.GetManifestResourceStream(xshdFile))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("C#", new string[] { ".cs" }, customHighlighting);

            InitializeComponent();
            DataContext = this;

            DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
            foldingUpdateTimer.Start();

            ComboConverter.ItemsSource = ConverterList;
            ComboConverter.DisplayMemberPath = "ConverterName";
            ComboConverter.SelectionChanged += new SelectionChangedEventHandler(ComboConverter_SelectionChanged);

            List<ConverterBase.ConvType> convTypes = new List<ConverterBase.ConvType>();
            foreach (ConverterBase.ConvType value in Enum.GetValues(typeof(ConverterBase.ConvType)))
            {
                convTypes.Add(value);
            }
            ComboConversionBy.ItemsSource = convTypes;

            ucAvalonTextEditor.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(ucAvalonTextEditor_LostKeyboardFocus);
        }


        IPropertyNode property;
        public IPropertyNode Property
        {
            get
            {
                return property;
            }
            set
            {
                property = value;
                ComboConverter.ItemsSource = ConverterList;
                if (DesignObject != null && DesignObject.Component != null)
                    _ConversionBy = (DesignObject.Component as ConverterBase).ConversionBy;
                else
                    _ConversionBy = ConverterBase.ConvType.Direct;
                if (ucAvalonTextEditor != null)
                {
                    _OnTargetUpdated = true;
                    ucAvalonTextEditor.Text = Expression;
                    _OnTargetUpdated = false;
                }
                RefreshView();
                UpdateVisibility();
            }
        }

        public IEnumerable<ConverterManager.ConverterEntry> ConverterList
        {
            get
            {
                return ConverterManager.GetConverterList(IsForMultibinding);
            }
        }

        public bool IsForMultibinding
        {
            get
            {
                if (Property == null)
                    return false;
                if (Property.FirstProperty.DesignItem.Component is MultiBinding)
                    return true;
                else
                    return false;
            }
        }

        protected DesignItem DesignObject
        {
            get
            {
                if (Property != null)
                    return Property.ValueItem;
                return null;
            }
        }

        //public void InitEditor(DesignItem designObject)
        //{
        //    DesignObject = designObject;
        //}

        public bool GlobalFunction
        {
            get
            {
                if (DesignObject == null)
                    return false;
                object result = DesignObject.Properties["GlobalFunction"].ValueOnInstance;
                if (result == null)
                    return false;
                return (bool)result;
            }
            set
            {
                if (DesignObject != null)
                    DesignObject.Properties["GlobalFunction"].SetValue(value);
            }
        }

        public string ACUrlCommand
        {
            get
            {
                if (DesignObject == null)
                    return "";
                object result = DesignObject.Properties["ACUrlCommand"].ValueOnInstance;
                if (result == null)
                    return "";
                return (string)result;
            }
            set
            {
                if (DesignObject != null)
                    DesignObject.Properties["ACUrlCommand"].SetValue(value);
            }
        }

        public string Expression
        {
            get
            {
                if (DesignObject == null)
                    return "";
                object result = DesignObject.Properties["Expression"].ValueOnInstance;
                if (result == null)
                    return "";
                return (string)result;
            }
            set
            {
                if (DesignObject != null)
                    DesignObject.Properties["Expression"].SetValue(value);
            }
        }

        private ConverterBase.ConvType _ConversionBy;
        public ConverterBase.ConvType ConversionBy
        {
            get
            {
                return _ConversionBy;
            }
            set
            {
                _ConversionBy = value;
                if (DesignObject != null)
                {
                    if (value == ConverterBase.ConvType.ScriptEngine)
                    {
                        DesignObject.Properties["Expression"].Reset();
                        _OnTargetUpdated = true;
                        ucAvalonTextEditor.Text = "";
                        _OnTargetUpdated = false;
                    }
                    else if (value == ConverterBase.ConvType.Expression)
                    {
                        DesignObject.Properties["ACUrlCommand"].Reset();
                        DesignObject.Properties["GlobalFunction"].Reset();
                        _OnTargetUpdated = true;
                        ucAvalonTextEditor.Text = Expression;
                        _OnTargetUpdated = false;
                    }
                    else
                    {
                        DesignObject.Properties["Expression"].Reset();
                        _OnTargetUpdated = true;
                        ucAvalonTextEditor.Text = "";
                        _OnTargetUpdated = false;
                        DesignObject.Properties["ACUrlCommand"].Reset();
                        DesignObject.Properties["GlobalFunction"].Reset();
                    }
                }
                RefreshView();
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            if (ConversionBy == ConverterBase.ConvType.ScriptEngine)
            {
                ucAvalonTextEditor.Visibility = System.Windows.Visibility.Collapsed;
                tbACUrlCmd.Visibility = System.Windows.Visibility.Visible;
                cbGlobal.Visibility = System.Windows.Visibility.Visible;
            }
            else if (ConversionBy == ConverterBase.ConvType.Expression)
            {
                ucAvalonTextEditor.Visibility = System.Windows.Visibility.Visible;
                tbACUrlCmd.Visibility = System.Windows.Visibility.Collapsed;
                cbGlobal.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ucAvalonTextEditor.Visibility = System.Windows.Visibility.Collapsed;
                tbACUrlCmd.Visibility = System.Windows.Visibility.Collapsed;
                cbGlobal.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (Property != null)
            {
                _InSelectionUpdate = true;
                if (DesignObject == null)
                {
                    ComboConversionBy.Visibility = System.Windows.Visibility.Collapsed;
                    ButtonReset.IsEnabled = false;
                    ComboConverter.IsEnabled = true;
                    ComboConverter.SelectedValue = null;
                }
                else
                {
                    ComboConversionBy.Visibility = System.Windows.Visibility.Visible;
                    ButtonReset.IsEnabled = true;
                    ComboConverter.SelectedIndex = ConverterList.IndexWhere(c => c.ConverterType == DesignObject.ComponentType);
                    ComboConverter.IsEnabled = false;
                }
                _InSelectionUpdate = false;
            }
        }

        private void RefreshView()
        {
            RaisePropertyChanged("GlobalFunction");
            RaisePropertyChanged("ACUrlCommand");
            RaisePropertyChanged("Expression");
            RaisePropertyChanged("ConversionBy");
        }

        private bool _OnTargetUpdated = false;
        private bool _EditorTextChanged = false;
        private void ucAvalonTextEditor_TextChanged(object sender, EventArgs e)
        {
            if (_OnTargetUpdated == true)
                return;
            _EditorTextChanged = true;
        }

        void ucAvalonTextEditor_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_EditorTextChanged)
            {
                Expression = ucAvalonTextEditor.Text;
            }
            _EditorTextChanged = false;
        }

        public void UpdateExpression(string newExpression)
        {
            ucAvalonTextEditor.Text = newExpression;
            Expression = newExpression;
        }


        #region Folding
        FoldingManager foldingManager;
        VBScriptEditorBraceFoldingStrategy foldingStrategy;

        //void HighlightingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        void ChangeSyntaxHighlighting()
        {
            if (ucAvalonTextEditor.SyntaxHighlighting == null)
            {
                foldingStrategy = null;
            }
            else
            {
                switch (ucAvalonTextEditor.SyntaxHighlighting.Name)
                {
                    case "C#":
                        foldingStrategy = new VBScriptEditorBraceFoldingStrategy();
                        ucAvalonTextEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(ucAvalonTextEditor.Options);
                        break;
                    default:
                        ucAvalonTextEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        foldingStrategy = null;
                        break;
                }
            }
            if (foldingStrategy != null)
            {
                if (foldingManager == null)
                    foldingManager = FoldingManager.Install(ucAvalonTextEditor.TextArea);
                int firstErrorOffset;
                IEnumerable<NewFolding> foldings = foldingStrategy.CreateNewFoldings(ucAvalonTextEditor.Document, out firstErrorOffset);
                foldingManager.UpdateFoldings(foldings, firstErrorOffset);
            }
            else
            {
                if (foldingManager != null)
                {
                    FoldingManager.Uninstall(foldingManager);
                    foldingManager = null;
                }
            }
        }

        void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (foldingStrategy != null)
            {
                int firstErrorOffset;
                IEnumerable<NewFolding> foldings = foldingStrategy.CreateNewFoldings(ucAvalonTextEditor.Document, out firstErrorOffset);
                foldingManager.UpdateFoldings(foldings, firstErrorOffset);
            }
        }
        #endregion

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            if (Property == null)
                return;
            Property.Reset();
            _ConversionBy = ConverterBase.ConvType.Direct;
            _OnTargetUpdated = true;
            ucAvalonTextEditor.Text = "";
            _OnTargetUpdated = false;
            RefreshView();
            UpdateVisibility();
        }

        private bool _InSelectionUpdate = false;
        void ComboConverter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Property.Value != null || _InSelectionUpdate)
                return;
            ConverterManager.ConverterEntry converterEntry = ComboConverter.SelectedItem as ConverterManager.ConverterEntry;
            if (converterEntry == null)
                return;
            if (IsForMultibinding)
            {
                IMultiValueConverter converter = Activator.CreateInstance(converterEntry.ConverterType) as IMultiValueConverter;
                if (converter == null)
                    converter = new ConverterObjectMulti();
                Property.Value = converter;
            }
            else
            {
                IValueConverter converter = Activator.CreateInstance(converterEntry.ConverterType) as IValueConverter;
                if (converter == null)
                    converter = new ConverterObject();
                Property.Value = converter;
            }
            RefreshView();
            UpdateVisibility();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
