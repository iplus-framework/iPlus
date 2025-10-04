// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using gip.ext.design.avui;
using gip.ext.designer.avui.OutlineView;
using System.Collections.Generic;
using System.ComponentModel;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a tranform outline node.
    /// </summary>
    public class TransformOutlineNode : OutlineNodeBase
    {
        public TransformOutlineNode(DesignItem designItem)
            : base(designItem)
        {
        }

        protected override void Initialize()
        {
            DesignItem.PropertyChanged += new PropertyChangedEventHandler(DesignItem_PropertyChanged);
        }

        public void Replace(DesignItem designItem)
        {
            DesignItem.PropertyChanged -= DesignItem_PropertyChanged;
            DesignItem = designItem;
            DesignItem.PropertyChanged += DesignItem_PropertyChanged;
            RaisePropertyChanged("Name");
        }

        protected static Dictionary<DesignItem, TransformOutlineNode> outlineNodes = new Dictionary<DesignItem, TransformOutlineNode>();

        public static TransformOutlineNode Create(DesignItem designItem)
        {
            TransformOutlineNode node;
            if (!outlineNodes.TryGetValue(designItem, out node))
            {
                node = new TransformOutlineNode(designItem);
                outlineNodes[designItem] = node;
            }
            return node;
        }

        protected override OutlineNodeBase OnCreateChildrenNode(DesignItem child)
        {
            return TransformOutlineNode.Create(child);
        }

    }
}
