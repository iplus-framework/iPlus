using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Xaml.Interactivity;
using gip.ext.design.avui;
using System;

namespace gip.ext.designer.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Alternative behavior-based approach for template selection in Avalonia
    /// This can be used as an alternative to the IDataTemplate approach
    /// </summary>
    public class CollectionTemplateSelectionBehavior : Behavior<ListBox>
    {
        public static readonly StyledProperty<IDataTemplate> PointTemplateProperty =
            AvaloniaProperty.Register<CollectionTemplateSelectionBehavior, IDataTemplate>(nameof(PointTemplate));

        public static readonly StyledProperty<IDataTemplate> StringTemplateProperty =
            AvaloniaProperty.Register<CollectionTemplateSelectionBehavior, IDataTemplate>(nameof(StringTemplate));

        public static readonly StyledProperty<IDataTemplate> DefaultTemplateProperty =
            AvaloniaProperty.Register<CollectionTemplateSelectionBehavior, IDataTemplate>(nameof(DefaultTemplate));

        public IDataTemplate PointTemplate
        {
            get => GetValue(PointTemplateProperty);
            set => SetValue(PointTemplateProperty, value);
        }

        public IDataTemplate StringTemplate
        {
            get => GetValue(StringTemplateProperty);
            set => SetValue(StringTemplateProperty, value);
        }

        public IDataTemplate DefaultTemplate
        {
            get => GetValue(DefaultTemplateProperty);
            set => SetValue(DefaultTemplateProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.ItemTemplate = new CollectionDataTemplateSelector(this);
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.ItemTemplate = null;
            }
            base.OnDetaching();
        }

        private class CollectionDataTemplateSelector : IDataTemplate
        {
            private readonly CollectionTemplateSelectionBehavior _behavior;

            public CollectionDataTemplateSelector(CollectionTemplateSelectionBehavior behavior)
            {
                _behavior = behavior;
            }

            public Control Build(object param)
            {
                if (param is not DesignItem designItem)
                    return new TextBlock { Text = "No data" };

                IDataTemplate template = designItem.Component switch
                {
                    Point => _behavior.PointTemplate,
                    string => _behavior.StringTemplate,
                    _ => _behavior.DefaultTemplate
                };

                return template?.Build(param) ?? new TextBlock { Text = designItem.Component?.ToString() ?? "No data" };
            }

            public bool Match(object data)
            {
                return data is DesignItem;
            }
        }
    }
}