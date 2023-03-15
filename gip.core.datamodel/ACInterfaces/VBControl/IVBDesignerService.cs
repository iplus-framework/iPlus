// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IVBSource.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace gip.core.datamodel
{
    public interface IVBComponentDesignManagerProxy
    {
        void UpdateVisual();
        void CloseDockableWindow(IACObject window);
        void OnDesignerLoaded(IVBContent designEditor, bool reverseToXaml);
        void RecalcEdgeRouting();
        void ACActionToTargetLogic(IACInteractiveObject oldTargetVBDataObject, ACActionArgs oldActionArgs, out IACInteractiveObject targetVBDataObject, out ACActionArgs actionArgs);
        IEnumerable<IACObject> GetAvailableTools();
        IACInteractiveObject GetVBDesignEditor(IACComponent component);
    }

    public interface IVBDesignerService
    {
        #region VBPresenterMethod
        Msg GetPresenterElements(out List<string> result, string xaml);
        #endregion

        #region VBDesigner
        IVBComponentDesignManagerProxy GetDesignMangerProxy(IACComponent component);
        void RemoveDesignMangerProxy(IACComponent component);
        #endregion
    }
}
