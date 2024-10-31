// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections;
using System.Windows;
using System.Windows.Input;
//using gip.core.datamodel;
//using gip.core.autocomponent;
//using gip.core.layoutengine;
using gip.ext.design;
using gip.ext.designer.Services;
using gip.core.datamodel;

namespace gip.core.layoutengine
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

        public override void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DrawVBEdgeAdorner._DesignManager = DesignManager;
            IDesignPanel designPanel = (IDesignPanel)sender;
            DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel), true, true);
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
