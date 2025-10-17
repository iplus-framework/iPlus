using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control for connecting lines between two VBGraphItems.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBGraphEdge'}de{'VBGraphEdge'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBGraphEdge : VBEdge
    {
        #region c'tors

        public VBGraphEdge(IACEdge iACEdge, VBGraphSurface parent, string routeIdentifier) : base()
        {
            StrokeThickness = 1.5;
            if (parent.StrokeThickness > 0)
                StrokeThickness = parent.StrokeThickness;

            Focusable = parent.Focusable;

            Binding binding = new Binding();
            binding.Source = parent;
            this.Bind(ParentSurfaceProperty, binding);

            Binding binding2 = new Binding();
            binding2.Source = iACEdge;
            this.Bind(DataContextProperty, binding2);

            VBContent = routeIdentifier;
            ParentSurface.OnEdgesChanged += ParentSurface_OnEdgesChanged;

            if (ParentSurface.GraphEdgeStyle != null)
                this.Theme = ParentSurface.GraphEdgeStyle;
            else
                Stroke = Brushes.LightGoldenrodYellow;

        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            if(ParentSurface != null)
                ParentSurface.OnEdgesChanged -= ParentSurface_OnEdgesChanged;

            this.ClearAllBindings();

            base.DeInitVBControl(bso);
        }

        #endregion

        #region Properties



        public VBGraphSurface ParentSurface
        {
            get { return (VBGraphSurface)GetValue(ParentSurfaceProperty); }
            set { SetValue(ParentSurfaceProperty, value); }
        }

        public static readonly StyledProperty<VBGraphSurface> ParentSurfaceProperty = AvaloniaProperty.Register<VBGraphSurface, VBGraphSurface>(nameof(ParentSurface));


        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public override IEnumerable<IACObject> ACContentList
        {
            get { return null; }
        }

        public override Control SourceElement
        {
            get
            {
                if (Source != null)
                    return Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(Source, typeof(VBGraphItem)) as VBConnector;
                return null;
            }
        }

        public override Control TargetElement
        {
            get
            {
                if (Target != null)
                    return Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(Target, typeof(VBGraphItem)) as VBConnector;
                return null;
            }
        }
        #endregion

        #region Methods

        public void SetConnectorNames(string source, string target)
        {
            VBConnectorSource = source;
            VBConnectorTarget = target;
        }

        public void SetEdgeInRoute()
        {
            IsSelected = true;
            this.ZIndex = 2;
        }

        public void UnsetEdgeFromRoute()
        {
            IsSelected = false;
            this.ZIndex = 1;
        }

        private void ParentSurface_OnEdgesChanged(object sender, EventArgs e)
        {
            if (ParentSurface.ActiveEdges.Any(c => c == DataContext))
                SetEdgeInRoute();
            else
                UnsetEdgeFromRoute();
        }

        protected override void VBConnectPath_Loaded(object sender, RoutedEventArgs e)
        {
            base.VBConnectPath_Loaded(sender, e);
            if(ParentSurface != null)
                ParentSurface.OnVBGraphEdgeLoaded();
        }

        #endregion
    }
}
