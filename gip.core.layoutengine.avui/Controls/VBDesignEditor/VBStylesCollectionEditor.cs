using System;
using gip.ext.designer.avui.OutlineView;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents an editor for local styles and selectors.
    /// </summary>
    public class VBStylesCollectionEditor : StylesCollectionEditor, IDesignEditorWindow
    {
        protected override Type StyleKeyOverride => typeof(VBStylesCollectionEditor);
    }
}