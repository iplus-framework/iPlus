// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.ext.design;
using System.Collections.ObjectModel;
using System.Collections;
using gip.ext.designer;
using gip.ext.xamldom;
using gip.ext.design.PropertyGrid;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Markup;
using gip.ext.designer.OutlineView;

namespace gip.core.layoutengine.PropertyGrid.Editors
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
