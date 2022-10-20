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

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents the outline node for trigger property.
    /// </summary>
    public class VBPropertyTriggerOutlineNode : PropertyTriggerOutlineNode
    {
        //new public static VBPropertyTriggerOutlineNode Create(DesignItem trigger, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(trigger, out node))
        //    {
        //        node = new VBPropertyTriggerOutlineNode(trigger, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as VBPropertyTriggerOutlineNode;
        //}

        /// <summary>
        /// Creates a new instance of VBPropertyTriggerOutlineNode.
        /// </summary>
        /// <param name="trigger">The trigger parameter.</param>
        /// <param name="designItem">The design item parameter.</param>
        public VBPropertyTriggerOutlineNode(DesignItem trigger, DesignItem designItem)
            : base(trigger, designItem)
        {
        }

        /// <summary>
        /// Gets or sets the editor.
        /// </summary>
        public override FrameworkElement Editor
        {
            get
            {
                if (_Editor == null)
                {
                    _Editor = new VBPropertyTriggerEditor();
                    (_Editor as VBPropertyTriggerEditor).InitEditor(_DesignObject, this);
                }
                return _Editor;
            }

            protected set
            {
                _Editor = value;
            }
        }

    }

    /// <summary>
    /// Represents the outline node for data triggers.
    /// </summary>
    public class VBDataTriggerOutlineNode : DataTriggerOutlineNode
    {
        //new public static VBDataTriggerOutlineNode Create(DesignItem trigger, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(trigger, out node))
        //    {
        //        node = new VBDataTriggerOutlineNode(trigger, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as VBDataTriggerOutlineNode;
        //}

        /// <summary>
        /// Create a new instance of VBDataTriggerOutlineNode.
        /// </summary>
        /// <param name="trigger">The trigger parameter.</param>
        /// <param name="designItem">The design item parameter.</param>
        public VBDataTriggerOutlineNode(DesignItem trigger, DesignItem designItem)
            : base(trigger, designItem)
        {
        }

        /// <summary>
        /// Gets or sets the editor.
        /// </summary>
        public override FrameworkElement Editor
        {
            get
            {
                if (_Editor == null)
                {
                    _Editor = new VBDataTriggerEditor();
                    (_Editor as VBDataTriggerEditor).InitEditor(_DesignObject, this);
                }
                return _Editor;
            }

            protected set
            {
                _Editor = value;
            }
        }

        internal void ChangeTypeOfTriggerValue(Type typeOfTriggerValue)
        {
            TypeOfTriggerValue = typeOfTriggerValue;
        }

    }

    /// <summary>
    /// Represents the outline node for event triggers.
    /// </summary>
    public class VBEventTriggerOutlineNode : EventTriggerOutlineNode
    {
        //new public static VBEventTriggerOutlineNode Create(DesignItem trigger, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(trigger, out node))
        //    {
        //        node = new VBEventTriggerOutlineNode(trigger, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as VBEventTriggerOutlineNode;
        //}

        /// <summary>
        /// Creates a new instance of VBEventTriggerOutlineNode.
        /// </summary>
        /// <param name="trigger">The trigger parameter.</param>
        /// <param name="designItem">The design item parameter.</param>
        public VBEventTriggerOutlineNode(DesignItem trigger, DesignItem designItem)
            : base(trigger, designItem)
        {
        }

        /// <summary>
        /// Gets or sets the editor.
        /// </summary>
        public override FrameworkElement Editor
        {
            get
            {
                if (_Editor == null)
                {
                    _Editor = new VBEventTriggerEditor();
                    (_Editor as VBEventTriggerEditor).InitEditor(_DesignObject, this);
                }
                return _Editor;
            }

            protected set
            {
                _Editor = value;
            }
        }

    }

    /// <summary>
    /// Represents the outline node for multidata triggers.
    /// </summary>
    public class VBMultiDataTriggerOutlineNode : MultiDataTriggerOutlineNode
    {
        //new public static VBMultiDataTriggerOutlineNode Create(DesignItem trigger, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(trigger, out node))
        //    {
        //        node = new VBMultiDataTriggerOutlineNode(trigger, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as VBMultiDataTriggerOutlineNode;
        //}

        /// <summary>
        /// Creates a new instace of VBMultiDataTriggerOutlineNode
        /// </summary>
        /// <param name="trigger">The trigger parameter.</param>
        /// <param name="designItem">The design item parameter.</param>
        public VBMultiDataTriggerOutlineNode(DesignItem trigger, DesignItem designItem)
            : base(trigger, designItem)
        {
        }

        /// <summary>
        /// Gets or sets the editor.
        /// </summary>
        public override FrameworkElement Editor
        {
            get
            {
                if (_Editor == null)
                {
                    _Editor = new VBMultiTriggerEditor();
                    (_Editor as VBMultiTriggerEditor).InitEditor(_DesignObject, this);
                }
                return _Editor;
            }

            protected set
            {
                _Editor = value;
            }
        }
    }

    /// <summary>
    /// Represents the outline node for multi triggers.
    /// </summary>
    public class VBMultiTriggerOutlineNode : MultiTriggerOutlineNode
    {
        //new public static VBMultiTriggerOutlineNode Create(DesignItem trigger, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(trigger, out node))
        //    {
        //        node = new VBMultiTriggerOutlineNode(trigger, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as VBMultiTriggerOutlineNode;
        //}

        /// <summary>
        /// Creates a new instance of VBMultiTriggerOutlineNode.
        /// </summary>
        /// <param name="trigger">The trigger parameter.</param>
        /// <param name="designItem">The design item.</param>
        public VBMultiTriggerOutlineNode(DesignItem trigger, DesignItem designItem)
            : base(trigger, designItem)
        {
        }

        /// <summary>
        /// Gets or sets the editor.
        /// </summary>
        public override FrameworkElement Editor
        {
            get
            {
                if (_Editor == null)
                {
                    _Editor = new VBMultiTriggerEditor();
                    (_Editor as VBMultiTriggerEditor).InitEditor(_DesignObject, this);
                }
                return _Editor;
            }

            protected set
            {
                _Editor = value;
            }
        }

    }
}
