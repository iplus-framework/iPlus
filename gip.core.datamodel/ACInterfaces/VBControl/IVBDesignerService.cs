// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
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
using System.Xml.Linq;

namespace gip.core.datamodel
{
    public interface IVBComponentDesignManagerProxy
    {

        #region Methods
        void UpdateVisual();
        void CloseDockableWindow(IACObject window);
        void OnDesignerLoaded(IVBContent designEditor, bool reverseToXaml);
        void RecalcEdgeRouting();
        void ACActionToTargetLogic(IACInteractiveObject oldTargetVBDataObject, ACActionArgs oldActionArgs, out IACInteractiveObject targetVBDataObject, out ACActionArgs actionArgs);

        void CurrentAvailableElement(object ToolService, ACObjectItem _CurrentAvailableProperty, ACObjectItem value);

        void ShowDesignManager(string dockingManagerName = "");
        void HideDesignManager();

        #region ToolService
        void OnCurrentToolChanged(object sender, EventArgs e);
        void ReloadToolService();
        object GetToolService();
        void DeInitToolService();
        void OnCurrentAvailableToolChanged();
        void CurrentAvailableElementIsToolSelection(object ToolService, ACObjectItem _CurrentAvailableProperty, ACObjectItem value);
        IEnumerable<IACObject> GetAvailableTools();
        #endregion

        void ClearVisualChangeList();
        void AddToVisualChangeList(IACObject visualObject, short layoutAction, string acUrl = "", string acUrl2 = "");
        IACInteractiveObject GetVBDesignEditor(IACComponent component);
        #endregion
    }

    public interface IVBComponentDesignManagerXAML : IVBComponentDesignManagerProxy
    {
        IEnumerable<ACClass> DesignManagerToolGetVBControlList(Database context);
        XElement LayoutGeneratorLoadLayoutAsXElement(string xmlLayout);
        bool IsEnabledACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs);
        void CurrentAvailableElementClicked(object ToolService, ACObjectItem _CurrentAvailableProperty);

        #region Designer Methods
        void DesignerRotateR90();
        bool IsEnabledDesignerRotateR90();
        void DesignerFlipHorz();
        void DesignerFlipVert();
        bool IsEnabledDesignerFlipVert();
        bool IsEnabledDesignerFlipHorz();
        void DesignerResetTransform();
        bool IsEnabledDesignerResetTransform();
        #endregion

        void ParentComponent_ACSaveChangesExecuted(object sender, EventArgs e);
        void ParentACComponentDatabase_ACChangesExecuted(object sender, ACChangesEventArgs e);
    }

    public interface IVBComponentDesignManagerWorkflowMethod : IVBComponentDesignManagerProxy
    {
        bool DoInsertRoot(IACWorkflowDesignContext vbWorkflow, ACClass methodACClass);
        void AddToVisualChangeListRectParam(IACObject visualObject, short layoutAction, double width, double height, string acUrl = "", string acUrl2 = "");
    }

    public interface IVBDesignerService
    {
        #region VBPresenter
        void GenerateNewRoutingLogic();
        object GetVBRoutingLogic();
        #endregion

        #region VBPresenterMethod
        Msg GetPresenterElements(out List<string> result, string xaml);
        #endregion

        #region VBDesigner
        IVBComponentDesignManagerProxy GetDesignMangerProxy(IACComponent component);
        void RemoveDesignMangerProxy(IACComponent component);
        #endregion
    }

    public interface IVBWFLayoutCalculatorService
    {
        IVBWFLayoutCalculatorProxy GetWFLayoutCalculatorProxy(IACComponent component);
        void RemoveWFLayoutCalculatorProxy(IACComponent component);
    }

    public interface IVBWFLayoutCalculatorProxy
    {
        void WFLayoutGroup(short layoutAction, object designContext, object designItemGroup, object designItem);
        void LayoutMaterialWF(IACWorkflowContext wfContext, object parent, object designItem, short layoutAction);
    }
}
