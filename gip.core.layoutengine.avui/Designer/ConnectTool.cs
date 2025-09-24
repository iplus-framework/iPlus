// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Input;
using gip.core.datamodel;
using gip.ext.design.avui;
using gip.ext.designer.avui.Services;

namespace gip.core.layoutengine.avui
{
    public class ConnectTool : DrawingTool
    {
        public static readonly ConnectTool Instance = new ConnectTool();

        public ConnectTool()
        {
        }

        public ConnectTool(IACComponentDesignManager designManager)
            : base()
        {
            DesignManager = designManager;
        }

        public override void OnMouseDown(object sender, PointerPressedEventArgs e)
        {
            DrawVBEdgeAdorner._DesignManager = DesignManager;
            IDesignPanel designPanel = (IDesignPanel)sender;
            DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel as Visual), true, true);
            if ((result.AdornerHit != null) && (result.AdornerHit.AdornedDesignItem != null) && (result.AdornerHit.AdornedElement != null))
            {
                IHandleDrawToolMouseDown b = result.AdornerHit.AdornedDesignItem.GetBehavior<ConnectToolDrawingHandler>();
                if (b != null)
                {
                    b.HandleStartDrawingOnMouseDown(designPanel, e, result, this);
                }
            }
        }

        protected override void OnActivated(IDesignPanel designPanel)
        {
            DrawVBEdgeAdorner._DesignManager = DesignManager;
        }

        public IACComponentDesignManager DesignManager
        {
            get;
            set;
        }

    }
}
