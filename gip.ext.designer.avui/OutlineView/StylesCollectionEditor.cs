using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace gip.ext.designer.avui.OutlineView
{
    public partial class StylesCollectionEditor : TemplatedControl, ITypeEditorInitItem
    {
        private readonly ObservableCollection<StyleListItem> _styles = new ObservableCollection<StyleListItem>();
        private readonly List<string> _selectorSuggestionCatalog = new List<string>();
        private string[] _selectorSuggestionsSnapshot = Array.Empty<string>();
        private bool _isNavigatingSuggestions;

        private DesignItem _designObject;
        private DesignItemProperty _stylesProperty;
        private bool _isUpdatingSelection;

        private ListBox _stylesList;
        private AutoCompleteBox _selectorTextBox;
        private TextBlock _selectorStatusText;
        private Button _applySelectorButton;
        private Button _addStyleButton;
        private Button _removeStyleButton;
        private Button _moveUpButton;
        private Button _moveDownButton;
        private SettersCollectionEditor _setterEditor;

        public StylesCollectionEditor()
        {
        }

        public void LoadItemsCollection(DesignItem designObject)
        {
            if (designObject == null || designObject.View == null)
                return;

            _designObject = designObject;
            _stylesProperty = ResolveStylesProperty(designObject);

            ReloadStyles();
            SelectFirstStyle();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            WireControls(e);
        }

        private void WireControls(TemplateAppliedEventArgs e)
        {
            _stylesList = e.NameScope.Find<ListBox>("PART_StylesList");
            _selectorTextBox = e.NameScope.Find<AutoCompleteBox>("PART_SelectorTextBox");
            _selectorStatusText = e.NameScope.Find<TextBlock>("PART_SelectorStatusText");
            _applySelectorButton = e.NameScope.Find<Button>("PART_ApplySelectorButton");
            _addStyleButton = e.NameScope.Find<Button>("PART_AddStyleButton");
            _removeStyleButton = e.NameScope.Find<Button>("PART_RemoveStyleButton");
            _moveUpButton = e.NameScope.Find<Button>("PART_MoveUpButton");
            _moveDownButton = e.NameScope.Find<Button>("PART_MoveDownButton");
            _setterEditor = e.NameScope.Find<SettersCollectionEditor>("PART_SetterEditor");

            if (_stylesList != null)
            {
                _stylesList.ItemsSource = _styles;
                _stylesList.SelectionChanged += OnStyleSelectionChanged;
            }

            if (_selectorTextBox != null)
            {
                _selectorTextBox.ItemsSource = Array.Empty<string>();
                _selectorTextBox.MinimumPrefixLength = 0;
                _selectorTextBox.MinimumPopulateDelay = TimeSpan.Zero;
                _selectorTextBox.FilterMode = AutoCompleteFilterMode.Contains;
                _selectorTextBox.IsTextCompletionEnabled = false;
                _selectorTextBox.SelectionChanged += OnSelectorSelectionChanged;
                _selectorTextBox.TextChanged += OnSelectorTextChanged;
                _selectorTextBox.KeyDown += OnSelectorTextBoxKeyDown;
                _selectorTextBox.LostFocus += OnSelectorTextBoxLostFocus;
                _selectorTextBox.PropertyChanged += OnSelectorPropertyChanged;
            }

            if (_applySelectorButton != null)
                _applySelectorButton.Click += OnApplySelectorClicked;

            if (_addStyleButton != null)
                _addStyleButton.Click += OnAddStyleClicked;

            if (_removeStyleButton != null)
                _removeStyleButton.Click += OnRemoveStyleClicked;

            if (_moveUpButton != null)
                _moveUpButton.Click += OnMoveStyleUpClicked;

            if (_moveDownButton != null)
                _moveDownButton.Click += OnMoveStyleDownClicked;

            if (_setterEditor != null)
                _setterEditor.TrackDesignerSelection = false;

            RebuildSelectorSuggestionCatalog();
            SetSelectorStatus("Type selector. Suggestions appear while typing.", Brushes.Gray);
            UpdateButtons();
        }

        private DesignItemProperty ResolveStylesProperty(DesignItem designObject)
        {
            return designObject.Properties.GetProperty("Styles");
        }

        private void ReloadStyles()
        {
            _styles.Clear();

            if (_stylesProperty?.CollectionElements == null)
            {
                if (_selectorTextBox != null)
                    _selectorTextBox.Text = string.Empty;

                BindSettersEditor(null);
                RebuildSelectorSuggestionCatalog();
                UpdateButtons();
                return;
            }

            int index = 1;
            foreach (var styleItem in _stylesProperty.CollectionElements)
            {
                if (styleItem == null)
                    continue;

                _styles.Add(new StyleListItem(styleItem, index++, GetSelectorText(styleItem)));
            }

            RebuildSelectorSuggestionCatalog();
            UpdateButtons();
        }

        private void SelectFirstStyle()
        {
            if (_stylesList == null)
                return;

            _stylesList.SelectedItem = _styles.Count > 0 ? _styles[0] : null;
        }

        private StyleListItem SelectedStyle => _stylesList?.SelectedItem as StyleListItem;

        private static string GetSelectorText(DesignItem styleItem)
        {
            var selectorProperty = styleItem?.Properties?.HasProperty("Selector");
            if (selectorProperty == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(selectorProperty.TextValue))
                return selectorProperty.TextValue;

            var selectorValue = selectorProperty.ValueOnInstance;
            return selectorValue?.ToString() ?? string.Empty;
        }

        private bool ApplySelector(DesignItem styleItem, string selectorText)
        {
            if (styleItem == null)
                return false;

            var selectorProperty = styleItem.Properties.HasProperty("Selector");
            if (selectorProperty == null)
                return false;

            try
            {
                if (string.IsNullOrWhiteSpace(selectorText))
                    selectorProperty.Reset();
                else
                    selectorProperty.SetValue(selectorText.Trim());

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void OnStyleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshStyleSelection();
        }

        protected void RefreshStyleSelection()
        {
            if (_isUpdatingSelection)
                return;

            var selected = SelectedStyle;

            if (_selectorTextBox != null)
                _selectorTextBox.Text = selected?.SelectorText ?? string.Empty;

            BindSettersEditor(selected?.StyleItem);
            UpdateButtons();
        }

        private void BindSettersEditor(DesignItem styleItem)
        {
            if (_setterEditor == null)
                return;

            var settersProperty = styleItem?.Properties?.HasProperty("Setters");
            if (settersProperty == null)
            {
                _setterEditor.IsEnabled = false;
                return;
            }

            _setterEditor.TrackDesignerSelection = false;
            _setterEditor.InitEditor(_designObject, settersProperty);
            _setterEditor.IsEnabled = true;
        }

        private void OnAddStyleClicked(object sender, RoutedEventArgs e)
        {
            if (_designObject?.Services?.Component == null || _stylesProperty?.CollectionElements == null)
                return;

            var newStyleItem = _designObject.Services.Component.RegisterComponentForDesigner(new Style());
            _stylesProperty.CollectionElements.Add(newStyleItem);

            var defaultSelector = BuildDefaultSelector();
            if (!string.IsNullOrWhiteSpace(defaultSelector))
                ApplySelector(newStyleItem, defaultSelector);

            ReloadStyles();
            SelectStyle(newStyleItem);
            RefreshStyleSelection();
        }

        private void OnRemoveStyleClicked(object sender, RoutedEventArgs e)
        {
            if (_stylesProperty?.CollectionElements == null)
                return;

            var selected = SelectedStyle;
            if (selected?.StyleItem == null)
                return;

            _stylesProperty.CollectionElements.Remove(selected.StyleItem);
            ReloadStyles();
            SelectFirstStyle();
            RefreshStyleSelection();
        }

        private void OnMoveStyleUpClicked(object sender, RoutedEventArgs e)
        {
            MoveSelectedStyle(up: true);
        }

        private void OnMoveStyleDownClicked(object sender, RoutedEventArgs e)
        {
            MoveSelectedStyle(up: false);
        }

        private void MoveSelectedStyle(bool up)
        {
            if (_stylesProperty?.CollectionElements == null)
                return;

            var selected = SelectedStyle;
            if (selected?.StyleItem == null)
                return;

            if (up)
                _stylesProperty.CollectionElements.MoveBackward(selected.StyleItem);
            else
                _stylesProperty.CollectionElements.MoveForward(selected.StyleItem);

            ReloadStyles();
            SelectStyle(selected.StyleItem);
            RefreshStyleSelection();
        }

        private void OnApplySelectorClicked(object sender, RoutedEventArgs e)
        {
            ApplySelectorFromTextBox();
        }

        private void OnSelectorTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            ApplySelectorFromTextBox();
        }

        private void OnSelectorPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            // Reset navigation guard when the dropdown closes (Escape, click outside, etc.)
            if (e.Property == AutoCompleteBox.IsDropDownOpenProperty && !(bool)e.NewValue)
            {
                _isNavigatingSuggestions = false;
            }
        }

        private void OnSelectorTextChanged(object sender, TextChangedEventArgs e)
        {
            SetSelectorStatus("Press Enter or Apply to set selector.", Brushes.Gray);
        }

        private void OnSelectorTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _isNavigatingSuggestions = false;
                ApplySelectorFromTextBox();
                e.Handled = true;
            }
            else if (e.Key >= Key.Down && e.Key <= Key.End)
            {
                // Arrow keys, Home, End, Page Up/Down — mark as navigating so
                // SelectionChanged does not commit prematurely.
                _isNavigatingSuggestions = true;
            }
        }

        private void OnSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Only commit when the user confirms the selection (Enter or mouse click).
            // Arrow-key navigation should not trigger ApplySelectorFromTextBox.
            if (_isNavigatingSuggestions)
                return;

            // Only apply when items are added (user confirmed a selection).
            // RemovedItems events come from AutoCompleteBox internal state resets
            // and should not trigger ApplySelectorFromTextBox.
            if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;

            ApplySelectorFromTextBox();
        }

        private bool _isApplyingSelector;

        private void ApplySelectorFromTextBox()
        {
            if (_isApplyingSelector)
                return;

            var selected = SelectedStyle;
            if (selected?.StyleItem == null || _selectorTextBox == null)
                return;

            var previousSelector = selected.SelectorText;
            var nextSelector = _selectorTextBox.Text ?? string.Empty;
            if (string.Equals(previousSelector, nextSelector, StringComparison.Ordinal))
                return;

            _isApplyingSelector = true;
            try
            {
                if (!ApplySelector(selected.StyleItem, nextSelector))
                {
                    SetSelectorStatus("Invalid selector syntax.", Brushes.IndianRed);
                    return;
                }

                selected.SelectorText = GetSelectorText(selected.StyleItem);
                SetSelectorStatus("Selector applied.", Brushes.ForestGreen);
                // Defer catalog rebuild to avoid re-entering AutoCompleteBox's
                // selection model during the apply flow.
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    RebuildSelectorSuggestionCatalog();
                    RefreshStyleSelection();
                    _isApplyingSelector = false;
                });
            }
            catch
            {
                SetSelectorStatus("Selector application failed.", Brushes.IndianRed);
                _isApplyingSelector = false;
            }
        }

        private void SelectStyle(DesignItem styleItem)
        {
            if (_stylesList == null || styleItem == null)
                return;

            _isUpdatingSelection = true;
            try
            {
                foreach (var style in _styles)
                {
                    if (ReferenceEquals(style.StyleItem, styleItem))
                    {
                        _stylesList.SelectedItem = style;
                        return;
                    }
                }

                _stylesList.SelectedItem = null;
            }
            finally
            {
                _isUpdatingSelection = false;
            }

            UpdateButtons();
        }

        private string BuildDefaultSelector()
        {
            var typeName = _designObject?.ComponentType?.Name;
            return string.IsNullOrWhiteSpace(typeName) ? string.Empty : typeName;
        }

        private void RebuildSelectorSuggestionCatalog()
        {
            _selectorSuggestionCatalog.Clear();

            var typeName = BuildDefaultSelector();
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                _selectorSuggestionCatalog.Add(typeName);
                _selectorSuggestionCatalog.Add(typeName + ":pointerover");
                _selectorSuggestionCatalog.Add(typeName + ":pressed");
                _selectorSuggestionCatalog.Add(typeName + ":disabled");
                _selectorSuggestionCatalog.Add(typeName + ":focus");
                _selectorSuggestionCatalog.Add(typeName + " > Border");
                _selectorSuggestionCatalog.Add(typeName + " TextBlock");
                _selectorSuggestionCatalog.Add(":is(" + typeName + ")");
            }

            if (_designObject?.View is Avalonia.StyledElement styledElement)
            {
                foreach (var cls in styledElement.Classes)
                {
                    if (string.IsNullOrWhiteSpace(cls))
                        continue;

                    _selectorSuggestionCatalog.Add("." + cls);
                    if (!string.IsNullOrWhiteSpace(typeName))
                        _selectorSuggestionCatalog.Add(typeName + "." + cls);
                }
            }

            _selectorSuggestionCatalog.Add(":pointerover");
            _selectorSuggestionCatalog.Add(":pressed");
            _selectorSuggestionCatalog.Add(":disabled");
            _selectorSuggestionCatalog.Add(":focus");
            _selectorSuggestionCatalog.Add(":focus-within");
            _selectorSuggestionCatalog.Add(":checked");
            _selectorSuggestionCatalog.Add(":selected");
            _selectorSuggestionCatalog.Add(":not(:disabled)");
            _selectorSuggestionCatalog.Add(".primary");
            _selectorSuggestionCatalog.Add(".accent");
            _selectorSuggestionCatalog.Add(".warning");
            _selectorSuggestionCatalog.Add("#MyControl");
            _selectorSuggestionCatalog.Add("StackPanel > Button");
            _selectorSuggestionCatalog.Add("Grid TextBlock");

            foreach (var style in _styles)
            {
                if (style == null || string.IsNullOrWhiteSpace(style.SelectorText))
                    continue;
                _selectorSuggestionCatalog.Add(style.SelectorText);
            }

            _selectorSuggestionsSnapshot = _selectorSuggestionCatalog
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(200)
                .ToArray();

            ApplySelectorSuggestionSource();
        }

        private void ApplySelectorSuggestionSource()
        {
            if (_selectorTextBox == null)
                return;

            var wasOpen = _selectorTextBox.IsDropDownOpen;
            if (wasOpen)
                _selectorTextBox.IsDropDownOpen = false;

            // Do NOT clear SelectedItem here — doing so fires SelectionChanged
            // with RemovedItems, which triggers ApplySelectorFromTextBox with
            // an empty/previous value after the user just committed a selection.
            _selectorTextBox.ItemsSource = _selectorSuggestionsSnapshot;
        }

        private void SetSelectorStatus(string message, IBrush foreground)
        {
            if (_selectorStatusText == null)
                return;

            _selectorStatusText.Text = message ?? string.Empty;
            if (foreground != null)
                _selectorStatusText.Foreground = foreground;
        }

        private void UpdateButtons()
        {
            var selected = SelectedStyle;
            int selectedIndex = selected == null ? -1 : _styles.IndexOf(selected);
            bool hasSelection = selected != null;
            bool canAdd = _designObject?.Services?.Component != null && _stylesProperty?.CollectionElements != null;

            if (_addStyleButton != null)
                _addStyleButton.IsEnabled = canAdd;

            if (_removeStyleButton != null)
                _removeStyleButton.IsEnabled = hasSelection;

            if (_moveUpButton != null)
                _moveUpButton.IsEnabled = selectedIndex > 0;

            if (_moveDownButton != null)
                _moveDownButton.IsEnabled = hasSelection && selectedIndex >= 0 && selectedIndex < _styles.Count - 1;

            if (_selectorTextBox != null)
                _selectorTextBox.IsEnabled = hasSelection && selected.StyleItem.Properties.HasProperty("Selector") != null;

            if (_applySelectorButton != null)
                _applySelectorButton.IsEnabled = _selectorTextBox != null && _selectorTextBox.IsEnabled;
        }

        private sealed class StyleListItem : INotifyPropertyChanged
        {
            public StyleListItem(DesignItem styleItem, int index, string selectorText)
            {
                StyleItem = styleItem;
                Index = index;
                _selectorText = selectorText ?? string.Empty;
            }

            public DesignItem StyleItem { get; }
            public int Index { get; }

            private string _selectorText;
            public string SelectorText
            {
                get => _selectorText;
                set
                {
                    var next = value ?? string.Empty;
                    if (string.Equals(_selectorText, next, StringComparison.Ordinal))
                        return;

                    _selectorText = next;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                }
            }

            public string DisplayName
            {
                get
                {
                    var selector = string.IsNullOrWhiteSpace(SelectorText) ? "(no selector)" : SelectorText;
                    return $"Style {Index}: {selector}";
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}