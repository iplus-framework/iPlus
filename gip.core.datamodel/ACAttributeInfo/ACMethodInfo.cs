// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-15-2012
// ***********************************************************************
// <copyright file="ACMethodInfo.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    /// <summary>
    /// Base attribute class for publishing methods for the iPlus-Framework.<para />
    /// Use this class for Methods with return values and which can be called locally or remote (Server-Side).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ACMethodInfo : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="acGroup">Name of Group if this method is in a relationship with other properties from the same acGroup</param>
        /// <param name="acCaptionTranslation">Translation tuple. Format: en{'xxx'}de{'xxx'}..</param>
        /// <param name="sortIndex">Sort order of the methodlist in the development enviroment</param>
        /// <param name="isRightmanagement">If set to true, this method appears in the Group-Management (BSOGroup) and rights can be assigned.</param>
        /// <param name="acKind">Category of the method</param>
        /// <param name="isAsyncProcess">True, if method is asynchronous</param>
        /// <param name="isPeriodic">Flag if Method will be called cyclic when it's subscribed to the project work cycle</param>
        public ACMethodInfo(string acGroup, string acCaptionTranslation, Int16 sortIndex, bool isRightmanagement = false, Global.ACKinds acKind = Global.ACKinds.MSMethod, bool isAsyncProcess = false, bool isPeriodic = false)
        {
            ACGroup = acGroup;
            ACCaptionTranslation = acCaptionTranslation;
            IsRightmanagement = isRightmanagement;
            PWClassMethod = "";
            IsInteraction = false;
            IsCommand = false;
            IsAsyncProcess = isAsyncProcess;
            IsPeriodic = isPeriodic;
            SortIndex = sortIndex;
            ACKind = acKind;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="acGroup">Name of Group if this method is in a relationship with other properties from the same acGroup</param>
        /// <param name="acCaptionTranslation">Translation tuple. Format: en{'xxx'}de{'xxx'}..</param>
        /// <param name="sortIndex">Sort order of the methodlist in the development enviroment.</param>
        /// <param name="isRightmanagement">If set to true, this method appears in the Group-Management (BSOGroup) and rights can be assigned.</param>
        /// <param name="isInteraction">If set to true, method is displayed in the context menu.</param>
        /// <param name="isCommand">If set to true, method is a void-method and has no parameters.</param>
        /// <param name="acKind">Category of the method.</param>
        /// <param name="pwClassMethod">ACIdentifier of a Workflow-Class which is able to invoke this method.</param>
        /// <param name="interactionVBContent">This method only appears in the context-menu if the VBContent-Property of a VB-UI-Control is the same like this VBContent.</param>
        /// <param name="isAsyncProcess">True, if method is asynchronous.</param>
        /// <param name="isPeriodic">Flag if Method will be called cyclic when it's subscribed to the project work cycle.</param>
        public ACMethodInfo(string acGroup, string acCaptionTranslation, Int16 sortIndex, bool isRightmanagement, bool isInteraction, bool isCommand, Global.ACKinds acKind = Global.ACKinds.MSMethod, string pwClassMethod = "", string interactionVBContent = "", bool isAsyncProcess = false, bool isPeriodic = false)
        {
            ACGroup = acGroup;
            ACCaptionTranslation = acCaptionTranslation;
            IsRightmanagement = isRightmanagement;
            PWClassMethod = pwClassMethod;
            IsInteraction = isInteraction;
            IsCommand = isCommand;
            IsAsyncProcess = isAsyncProcess;
            IsPeriodic = isPeriodic;
            SortIndex = sortIndex;
            InteractionVBContent = interactionVBContent;
            ACKind = acKind;
        }

        #region public Member

        /// <summary>
        /// If set to true, this method appears in the Group-Management (BSOGroup) and rights can be assigned.
        /// </summary>
        public bool IsRightmanagement { get; set; }

        /// <summary>
        /// Name of Group if this method is in a relationship with other properties from the same acGroup.
        /// </summary>
        public string ACGroup { get; set; }

        /// <summary>
        /// Translation tuple. Format: en{'xxx'}de{'xxx'}..
        /// </summary>
        public string ACCaptionTranslation { get; set; }


        /// <summary>
        /// ACIdentifier of a Workflow-Class which is able to invoke this method.
        /// </summary>
        public string PWClassMethod { get; set; }

        /// <summary>
        /// If set to true, method is displayed in the context menu
        /// </summary>
        public bool IsInteraction { get; set; }

        /// <summary>
        /// If set to true, method is a void-method and has no parameters.
        /// </summary>
        public bool IsCommand { get; set; }

        /// <summary>
        /// True, if method is asynchronous.
        /// </summary>
        public bool IsAsyncProcess { get; set; }

        /// <summary>
        /// This method only appears in the context-menu if the VBContent-Property of a VB-UI-Control is the same like this VBContent.
        /// </summary>
        public string InteractionVBContent { get; set; }

        /// <summary>
        /// Sort order of the methodlist in the development enviroment.
        /// </summary>
        public Int16 SortIndex { get; set; }

        /// <summary>
        /// Category of the method.
        /// </summary>
        public Global.ACKinds ACKind { get; set; }

        /// <summary>
        /// Flag if Method will be called cyclic when it's subscribed to the project work cycle.
        /// </summary>
        public bool IsPeriodic { get; set; }

        /// <summary>
        /// Category where this method belongs to.
        /// </summary>
        public short ContextMenuCategoryIndex { get; set; }

        /// <summary>
        /// Defines if Method will be delegated to a remote Device
        /// </summary>
        public bool IsRPCEnabled { get; set; }

        /// <summary>
        /// System.Type of the Class where this method should be attached. (Extension-Method)
        /// </summary>
        public Type AttachToClass { get; set; }

        #endregion
    }


    /// <summary>
    /// For publishing methods which can be invoked ONLY on CLIENT-Side.<para />
    /// The method MUST be STATIC and the FIRST PARAMETER of the method mus alway be "IACComponent acComponent".
    /// The iPlus-Framework automatically passes a instance of ACComponentProxy to this parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ACMethodClient : ACMethodInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="acGroup">Name of Group if this method is in a relationship with other properties from the same acGroup</param>
        /// <param name="acCaptionTranslation">Translation tuple. Format: en{'xxx'}de{'xxx'}..</param>
        /// <param name="sortIndex">Sort order of the methodlist in the development enviroment</param>
        /// <param name="isRightmanagement">If set to true, this method appears in the Group-Management (BSOGroup) and rights can be assigned.</param>
        /// <param name="isAsyncProcess">True, if method is asynchronous</param>
        /// <param name="isPeriodic">Flag if Method will be called cyclic when it's subscribed to the project work cycle</param>
        public ACMethodClient(string acGroup, string acCaptionTranslation, Int16 sortIndex, bool isRightmanagement = false, bool isAsyncProcess = false, bool isPeriodic = false) :
            base(acGroup, acCaptionTranslation, sortIndex, isRightmanagement, false, true, Global.ACKinds.MSMethodClient, "", "", isAsyncProcess)
        {
        }
    }


    /// <summary>
    /// For publishing methods which DOESN'T have a RETURN VALUE (void) and are PARAMETERLESS.<para />
    /// This is mostly used for Methods which appears as a button on the GUI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ACMethodCommand : ACMethodInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="acGroup">Name of Group if this method is in a relationship with other properties from the same acGroup</param>
        /// <param name="acCaptionTranslation">Translation tuple. Format: en{'xxx'}de{'xxx'}..</param>
        /// <param name="sortIndex">Sort order of the methodlist in the development enviroment</param>
        /// <param name="isRightmanagement">If set to true, this method appears in the Group-Management (BSOGroup) and rights can be assigned.</param>
        /// <param name="acKind">Category of the method</param>
        /// <param name="isAsyncProcess">True, if method is asynchronous</param>
        public ACMethodCommand(string acGroup, string acCaptionTranslation, Int16 sortIndex, bool isRightmanagement = false, Global.ACKinds acKind = Global.ACKinds.MSMethod, bool isAsyncProcess = false) :
            base(acGroup, acCaptionTranslation, sortIndex, isRightmanagement, false, true, acKind, "", "", isAsyncProcess)
        {
        }
    }


    /// <summary>
    /// For publishing methods which doesn't have a return value (void) and are parameterless and should appear in the CONTEXT MENU.<para />
    /// This is mostly used for Methods which can be invoked via context menu. You can also use it with a button.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ACMethodInteraction : ACMethodInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="acGroup">Name of Group if this method is in a relationship with other properties from the same acGroup</param>
        /// <param name="acCaptionTranslation">Translation tuple. Format: en{'xxx'}de{'xxx'}..</param>
        /// <param name="sortIndex">Sort order of the methodlist in the development enviroment</param>
        /// <param name="isRightmanagement">If set to true, this method appears in the Group-Management (BSOGroup) and rights can be assigned.</param>
        /// <param name="interactionVBContent">This method only appears in the context-menu if the VBContent-Property of a VB-UI-Control is the same like this VBContent</param>
        /// <param name="acKind">Category of the method</param>
        /// <param name="isAsyncProcess">True, if method is asynchronous</param>
        /// <param name="contextMenuCategory">Category where this method belongs to.</param>
        public ACMethodInteraction(string acGroup, string acCaptionTranslation, Int16 sortIndex, bool isRightmanagement, string interactionVBContent = "", Global.ACKinds acKind = Global.ACKinds.MSMethod, bool isAsyncProcess = false, Global.ContextMenuCategory contextMenuCategory = Global.ContextMenuCategory.NoCategory) :
            base(acGroup, acCaptionTranslation, sortIndex, isRightmanagement, true, true, acKind, "", interactionVBContent, isAsyncProcess)
        {
            ContextMenuCategoryIndex = (short)contextMenuCategory;
        }
    }


    /// <summary>
    /// For publishing methods which doesn't have a RETURN VALUE (void) and are PARAMETERLESS and should appear in the CONTEXT MENU but ONLY on CLIENT-Side.<para />
    /// The method  MUST be STATIC and the FIRST PARAMETER of the method mus alway be "IACComponent acComponent".
    /// This is mostly used for Methods which can be invoked via context menu. You can also use it with a button.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ACMethodInteractionClient : ACMethodInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="acGroup">Name of Group if this method is in a relationship with other properties from the same acGroup</param>
        /// <param name="acCaptionTranslation">Translation tuple. Format: en{'xxx'}de{'xxx'}..</param>
        /// <param name="sortIndex">Sort order of the methodlist in the development enviroment</param>
        /// <param name="isRightmanagement">If set to true, this method appears in the Group-Management (BSOGroup) and rights can be assigned.</param>
        /// <param name="interactionVBContent">This method only appears in the context-menu if the VBContent-Property of a VB-UI-Control is the same like this VBContent</param>
        /// <param name="isAsyncProcess">True, if method is asynchronous</param>
        /// <param name="contextMenuCategory">Category where this method belongs to</param>
        public ACMethodInteractionClient(string acGroup, string acCaptionTranslation, Int16 sortIndex, bool isRightmanagement, string interactionVBContent = "", bool isAsyncProcess = false, Global.ContextMenuCategory contextMenuCategory = Global.ContextMenuCategory.NoCategory) :
            base(acGroup, acCaptionTranslation, sortIndex, isRightmanagement, true, true, Global.ACKinds.MSMethodClient, "", interactionVBContent, isAsyncProcess)
        {
            ContextMenuCategoryIndex = (short)contextMenuCategory;
        }
    }


    /// <summary>
    /// For publishing methods, which should appear in the CONTEXT MENU in other classes. 
    /// The method  MUST be STATIC and the FIRST PARAMETER of the method must always be "IACComponent acComponent".
    /// Pass the TYPE of the class in the parameter "attachToClass" where this method should be ATTACHED (Similar concept as Extension-Methods in .net)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ACMethodAttached : ACMethodInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="acGroup">Name of Group if this method is in a relationship with other properties from the same acGroup</param>
        /// <param name="acCaptionTranslation">Translation tuple. Format: en{'xxx'}de{'xxx'}..</param>
        /// <param name="sortIndex">Sort order of the methodlist in the development enviroment</param>
        /// <param name="attachToClass">System.Type of the Class where this method should be attached.</param>
        /// <param name="isRightmanagement">If set to true, this method appears in the Group-Management (BSOGroup) and rights can be assigned.</param>
        /// <param name="interactionVBContent">This method only appears in the context-menu if the VBContent-Property of a VB-UI-Control is the same like this VBContent</param>
        /// <param name="isAsyncProcess">True, if method is asynchronous</param>
        /// <param name="contextMenuCategory">Category where this method belongs to</param>
        public ACMethodAttached(string acGroup, string acCaptionTranslation, Int16 sortIndex, Type attachToClass, bool isRightmanagement, string interactionVBContent = "", bool isAsyncProcess = false, Global.ContextMenuCategory contextMenuCategory = Global.ContextMenuCategory.NoCategory) :
            base(acGroup, acCaptionTranslation, sortIndex, isRightmanagement, true, true, Global.ACKinds.MSMethodClient, "", interactionVBContent, isAsyncProcess)
        {
            ContextMenuCategoryIndex = (short)contextMenuCategory;
            AttachToClass = attachToClass;
        }
    }


    /// <summary>
    /// For publishing methods which are ASYNCHRONOUS an can start a Task ath the parent Component which is IACComponentTaskExec or has a IACPointAsyncRMI-Point which returns a IACTask.
    /// The method should always have this signature: ACMethodEventArgs MethodName(ACMethod acMethod)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ACMethodAsync : ACMethodInfo
    {
        /// <summary>
        /// </summary>
        /// <param name="acGroup">Name of Group if this method is in a relationship with other properties from the same acGroup</param>
        /// <param name="acCaptionTranslation">Translation tuple. Format: en{'xxx'}de{'xxx'}..</param>
        /// <param name="sortIndex">Sort order of the methodlist in the development enviroment</param>
        /// <param name="isRightmanagement">If set to true, this method appears in the Group-Management (BSOGroup) and rights can be assigned.</param>
        /// <param name="pwClassMethod">ACIdentifier of a Workflow-Class which is able to invoke this method.</param>
        public ACMethodAsync(string acGroup, string acCaptionTranslation, Int16 sortIndex, bool isRightmanagement = false, string pwClassMethod = "") :
            base(acGroup, acCaptionTranslation, sortIndex, isRightmanagement, false, true, Global.ACKinds.MSMethod, pwClassMethod, "", true)
        {
        }
    }


    /// <summary>
    /// For publishing methods which DOESN'T have a RETURN VALUE (void) and are PARAMETERLESS and are related to a corresponding State-Property.<para />
    /// This is mostly used for Methods which Component has a State-Machine (ACStateEnum ACState)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ACMethodState : ACMethodInfo
    {
        /// <summary>
        /// </summary>
        /// <param name="acCaptionTranslation">Translation tuple. Format: en{'xxx'}de{'xxx'}..</param>
        /// <param name="sortIndex">Sort order of the methodlist in the development enviroment</param>
        /// <param name="isPeriodic">Flag if Method will be called cyclic when it's subscribed to the project work cycle</param>
        public ACMethodState(string acCaptionTranslation, Int16 sortIndex, bool isPeriodic = false) :
            base(Const.ACState, acCaptionTranslation, sortIndex, false, false, true, Global.ACKinds.MSMethod, "", "", false, isPeriodic)
        {
        }
    }
}
