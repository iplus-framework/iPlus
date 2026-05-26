using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using Avalonia.Controls;

namespace gip.ext.designer.avui.OutlineView
{
    public partial class StyleSelectorWindow : Window, ITypeEditorInitItem
    {
        public StyleSelectorWindow()
        {
            InitializeComponent();
        }

        public void LoadItemsCollection(DesignItem designObject)
        {
            if (designObject == null)
                return;

            var stylesEditor = this.FindControl<StylesCollectionEditor>("StylesEditor");
            stylesEditor?.LoadItemsCollection(designObject);
        }
    }
}