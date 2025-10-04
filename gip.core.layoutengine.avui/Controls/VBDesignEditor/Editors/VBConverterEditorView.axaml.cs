// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.Indentation;
using AvaloniaEdit.Indentation.CSharp;
using gip.core.datamodel;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a view editor for convertes.
    /// </summary>
    public partial class VBConverterEditorView : UserControl, INotifyPropertyChanged //: ITypeEditorInitItem
    {
		/// <summary>
        /// Create a new VBConverterEditorView instance.
		/// </summary>
		public VBConverterEditorView()
		{
            // C#-Highlighting
            IHighlightingDefinition customHighlighting;
            string xshdFile = "gip.core.layoutengine.avui.Controls.VBDesignEditor.Editors.Highlighting.CSharp-ModeStyleGip.xshd";
            if (ControlManager.WpfTheme == eWpfTheme.Aero)
                xshdFile = "gip.core.layoutengine.avui.Controls.VBDesignEditor.Editors.Highlighting.CSharp-ModeStyleAero.xshd";

            using (Stream s = this.GetType().Assembly.GetManifestResourceStream(xshdFile))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    customHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
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
            ComboConverter.DisplayMemberBinding = new Avalonia.Data.Binding("ConverterName");
            ComboConverter.SelectionChanged += ComboConverter_SelectionChanged;

            List<ConverterBase.ConvType> convTypes = new List<ConverterBase.ConvType>();
            foreach (ConverterBase.ConvType value in Enum.GetValues(typeof(ConverterBase.ConvType)))
            {
                convTypes.Add(value);
            }
            ComboConversionBy.ItemsSource = convTypes;

            ucAvalonTextEditor.LostFocus += ucAvalonTextEditor_LostKeyboardFocus;
        }

        private void UcAvalonTextEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Defines the Property styled property.
        /// </summary>
        public static readonly StyledProperty<IPropertyNode> PropertyProperty =
            AvaloniaProperty.Register<VBConverterEditorView, IPropertyNode>(nameof(Property));

        /// <summary>
        /// Gets or sets the property node.
        /// </summary>
        public IPropertyNode Property
        {
            get => GetValue(PropertyProperty);
            set => SetValue(PropertyProperty, value);
        }

        /// <summary>
        /// Defines the GlobalFunction styled property.
        /// </summary>
        public static readonly StyledProperty<bool> GlobalFunctionProperty =
            AvaloniaProperty.Register<VBConverterEditorView, bool>(nameof(GlobalFunction));

        /// <summary>
        /// Gets or sets the global function value.
        /// </summary>
        public bool GlobalFunction
        {
            get => GetValue(GlobalFunctionProperty);
            set => SetValue(GlobalFunctionProperty, value);
        }

        /// <summary>
        /// Defines the ACUrlCommand styled property.
        /// </summary>
        public static readonly StyledProperty<string> ACUrlCommandProperty =
            AvaloniaProperty.Register<VBConverterEditorView, string>(nameof(ACUrlCommand), "");

        /// <summary>
        /// Gets or sets the AC URL command.
        /// </summary>
        public string ACUrlCommand
        {
            get => GetValue(ACUrlCommandProperty);
            set => SetValue(ACUrlCommandProperty, value);
        }

        /// <summary>
        /// Defines the Expression styled property.
        /// </summary>
        public static readonly StyledProperty<string> ExpressionProperty =
            AvaloniaProperty.Register<VBConverterEditorView, string>(nameof(Expression), "");

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        public string Expression
        {
            get => GetValue(ExpressionProperty);
            set => SetValue(ExpressionProperty, value);
        }

        /// <summary>
        /// Defines the ConversionBy styled property.
        /// </summary>
        public static readonly StyledProperty<ConverterBase.ConvType> ConversionByProperty =
            AvaloniaProperty.Register<VBConverterEditorView, ConverterBase.ConvType>(nameof(ConversionBy), ConverterBase.ConvType.Direct);

        /// <summary>
        /// Gets or sets the conversion type.
        /// </summary>
        public ConverterBase.ConvType ConversionBy
        {
            get => GetValue(ConversionByProperty);
            set => SetValue(ConversionByProperty, value);
        }

        static VBConverterEditorView()
        {
            // Register property changed handlers
            PropertyProperty.Changed.AddClassHandler<VBConverterEditorView>((sender, e) => sender.OnPropertyChanged(e.NewValue as IPropertyNode));
            GlobalFunctionProperty.Changed.AddClassHandler<VBConverterEditorView>((sender, e) => sender.OnGlobalFunctionChanged((bool)e.NewValue));
            ACUrlCommandProperty.Changed.AddClassHandler<VBConverterEditorView>((sender, e) => sender.OnACUrlCommandChanged((string)e.NewValue));
            ExpressionProperty.Changed.AddClassHandler<VBConverterEditorView>((sender, e) => sender.OnExpressionChanged((string)e.NewValue));
            ConversionByProperty.Changed.AddClassHandler<VBConverterEditorView>((sender, e) => sender.OnConversionByChanged((ConverterBase.ConvType)e.NewValue));
        }

        private void OnPropertyChanged(IPropertyNode newProperty)
        {
            ComboConverter.ItemsSource = ConverterList;
            if (DesignObject != null && DesignObject.Component != null)
                ConversionBy = (DesignObject.Component as ConverterBase).ConversionBy;
            else
                ConversionBy = ConverterBase.ConvType.Direct;
            if (ucAvalonTextEditor != null)
            {
                _OnTargetUpdated = true;
                ucAvalonTextEditor.Text = Expression;
                _OnTargetUpdated = false;
            }
            RefreshView();
            UpdateVisibility();
        }

        private void OnGlobalFunctionChanged(bool value)
        {
            if (DesignObject != null)
                DesignObject.Properties["GlobalFunction"].SetValue(value);
        }

        private void OnACUrlCommandChanged(string value)
        {
            if (DesignObject != null)
                DesignObject.Properties["ACUrlCommand"].SetValue(value);
        }

        private void OnExpressionChanged(string value)
        {
            if (DesignObject != null)
                DesignObject.Properties["Expression"].SetValue(value);
        }

        private void OnConversionByChanged(ConverterBase.ConvType value)
        {
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

        private void UpdateVisibility()
        {
            if (ConversionBy == ConverterBase.ConvType.ScriptEngine)
            {
                ucAvalonTextEditor.IsVisible = false;
                tbACUrlCmd.IsVisible = true;
                cbGlobal.IsVisible = true;
            }
            else if (ConversionBy == ConverterBase.ConvType.Expression)
            {
                ucAvalonTextEditor.IsVisible = true;
                tbACUrlCmd.IsVisible = false;
                cbGlobal.IsVisible = false;
            }
            else
            {
                ucAvalonTextEditor.IsVisible = false;
                tbACUrlCmd.IsVisible = false;
                cbGlobal.IsVisible = false;
            }
            if (Property != null)
            {
                _InSelectionUpdate = true;
                if (DesignObject == null)
                {
                    ComboConversionBy.IsVisible = false;
                    ButtonReset.IsEnabled = false;
                    ComboConverter.IsEnabled = true;
                    ComboConverter.SelectedValue = null;
                }
                else
                {
                    ComboConversionBy.IsVisible = true;
                    ButtonReset.IsEnabled = true;
                    ComboConverter.SelectedIndex = ConverterList.IndexWhere(c => c.ConverterType == DesignObject.ComponentType);
                    ComboConverter.IsEnabled = false;
                }
                _InSelectionUpdate = false;
            }
        }

        private void RefreshView()
        {
            // Update properties from DesignObject
            if (DesignObject != null)
            {
                // Update GlobalFunction
                object globalFunctionResult = DesignObject.Properties["GlobalFunction"].ValueOnInstance;
                GlobalFunction = globalFunctionResult != null ? (bool)globalFunctionResult : false;

                // Update ACUrlCommand
                object acUrlCommandResult = DesignObject.Properties["ACUrlCommand"].ValueOnInstance;
                ACUrlCommand = acUrlCommandResult != null ? (string)acUrlCommandResult : "";

                // Update Expression
                object expressionResult = DesignObject.Properties["Expression"].ValueOnInstance;
                Expression = expressionResult != null ? (string)expressionResult : "";
            }
            else
            {
                GlobalFunction = false;
                ACUrlCommand = "";
                Expression = "";
            }
        }

        private bool _OnTargetUpdated = false;
        private bool _EditorTextChanged = false;
        private void ucAvalonTextEditor_TextChanged(object sender, EventArgs e)
        {
            if (_OnTargetUpdated == true)
                return;
            _EditorTextChanged = true;
        }

        void ucAvalonTextEditor_LostKeyboardFocus(object sender, RoutedEventArgs e)
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
                        ucAvalonTextEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(ucAvalonTextEditor.Options);
                        break;
                    default:
                        ucAvalonTextEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
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
            ConversionBy = ConverterBase.ConvType.Direct;
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

        // Keep INotifyPropertyChanged for compatibility, but it's now handled by StyledProperties
        public new event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
