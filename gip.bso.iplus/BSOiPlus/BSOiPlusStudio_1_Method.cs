// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-09-2012
// ***********************************************************************
// <copyright file="BSOiPlusStudio_1_Method.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.manager;
using gip.core.autocomponent;
using System.IO;
using System.Reflection;
using se = System.Environment;
using System.Text.RegularExpressions;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOiPlusStudio
    /// </summary>
    public partial class BSOiPlusStudio
    {
        #region BSO->ACProperty
        #region 1.1.1 ACClassMethod

        /// <summary>
        /// The _ access AC class method
        /// </summary>
        ACAccess<ACClassMethod> _AccessACClassMethod;
        /// <summary>
        /// Gets the access AC class method.
        /// </summary>
        /// <value>The access AC class method.</value>
        [ACPropertyAccess(9999, "ACClassMethod")]
        public override ACAccess<ACClassMethod> AccessACClassMethod
        {
            get
            {
                if (_AccessACClassMethod == null && ACType != null)
                {
                    ACQueryDefinition acQueryDefinition = AccessACClass.NavACQueryDefinition.ACUrlCommand(Const.QueryPrefix + ACClassMethod.ClassName) as ACQueryDefinition;
                    _AccessACClassMethod = acQueryDefinition.NewAccess<ACClassMethod>(ACClassMethod.ClassName, this);
                }
                return _AccessACClassMethod;
            }
        }

        /// <summary>
        /// The _ current AC class method
        /// </summary>
        ACClassMethod _CurrentACClassMethod;
        /// <summary>
        /// Gets or sets the current AC class method.
        /// </summary>
        /// <value>The current AC class method.</value>
        [ACPropertyCurrent(9999, "ACClassMethod")]
        public override ACClassMethod CurrentACClassMethod
        {
            get
            {
                return _CurrentACClassMethod;
            }
            set
            {
                if (_CurrentACClassMethod != value || value == null)
                {
                    _CurrentACClassMethod = value;

                    VBPresenterMethod vbPresenterMethod = this.ACUrlCommand("VBPresenterMethod(CurrentDesign)") as VBPresenterMethod;
                    if (vbPresenterMethod == null)
                    {
                        Messages.Error(this, "This user has no rights for viewing workflows. Assign rights for VBPresenterMethod in the group management!", true);
                        return;
                    }
                    vbPresenterMethod.Load(value);

                    _ACClassMethodControlList = null;
                    _ACClassMethodIconList = null;
                    if (value == null)
                    {
                        _CurrentACClassMethodIcon = null;
                        _CurrentACClassMethodControl = null;
                    }
                    else
                    {
                        ACClassDesign tempDesign = _CurrentACClassMethod.GetDesign(_CurrentACClassMethod, Global.ACUsages.DUIcon, Global.ACKinds.DSBitmapResource);
                        if (tempDesign != null)
                            _CurrentACClassMethodIcon = ACClassMethodIconList.FirstOrDefault(c => c.ACClassDesignID == tempDesign.ACClassDesignID);
                        else
                            _CurrentACClassMethodIcon = null;
                        _CurrentACClassMethodControl = _CurrentACClassMethod.GetDesign(_CurrentACClassMethod, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                    }

                    if (_CurrentACClassMethod != null && _CurrentACClassMethod.IsInteraction && _CurrentACClassMethod.ContextMenuCategoryIndex != null)
                        CurrentContextMenuCategory = ContextMenuCategoryList.FirstOrDefault(c => (short)c.Value == _CurrentACClassMethod.ContextMenuCategoryIndex);
                    else
                    {
                        CurrentContextMenuCategory = ContextMenuCategoryList.FirstOrDefault();
                        CurrentContextMenuCategory = null;
                    }

                    OnPropertyChanged("CurrentACClassMethod");
                    OnPropertyChanged("CurrentPWRoot");
                    OnPropertyChanged("CurrentMethodLayout");
                    OnPropertyChanged("IsMethodInfoVisible");
                    OnPropertyChanged("IsScriptEditorVisible");
                    OnPropertyChanged("IsWFEditorVisible");
                    OnPropertyChanged("CurrentACClassMethodControl");
                    OnPropertyChanged("CurrentACClassMethodIcon");
                    OnPropertyChanged("ACClassMethodControlList");
                    OnPropertyChanged("ACClassMethodIconList");
                    OnPropertyChanged("ConfigACClassMethodList");
                    OnPropertyChanged("CurrentContextMenuCategory");
                    string acObjectMode = ACState;
                    ACState = Const.SMReadOnly;
                    ACState = acObjectMode;
                }
            }
        }

        /// <summary>
        /// The _ selected AC class method
        /// </summary>
        ACClassMethod _SelectedACClassMethod;
        /// <summary>
        /// Gets or sets the selected AC class method.
        /// </summary>
        /// <value>The selected AC class method.</value>
        [ACPropertySelected(9999, "ACClassMethod")]
        public override ACClassMethod SelectedACClassMethod
        {
            get
            {
                return _SelectedACClassMethod;
            }
            set
            {
                _SelectedACClassMethod = value;
                OnPropertyChanged("SelectedACClassMethod");
            }
        }

        /// <summary>
        /// Gets the AC class method list.
        /// </summary>
        /// <value>The AC class method list.</value>
        [ACPropertyList(9999, "ACClassMethod")]
        public override IEnumerable<ACClassMethod> ACClassMethodList
        {
            get
            {
                if (CurrentACClass == null)
                    return null;

                if (_ACClassMethodList == null)
                    _ACClassMethodList = CurrentACClass.GetMethods();
                return _ACClassMethodList;
            }
        }

        public IEnumerable<IACConfigStore> ConfigStoreStages
        {
            get
            {
                List<IACConfigStore> stages = new List<IACConfigStore>();
                stages.Add(CurrentACClassMethod);
                return stages;
            }
        }
        #endregion
        #endregion

        #region BSO->ACMethod
        #region 1.1.1 ACClassMethod
        /// <summary>
        /// Loads the AC class method.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'Load Method'}de{'Methode laden'}", (short)MISort.Load, false, "SelectedACClassMethod", Global.ACKinds.MSMethodPrePost)]
        public void LoadACClassMethod()
        {
            if (!PreExecute("LoadACClassMethod")) return;
            CurrentACClassMethod = SelectedACClassMethod;
            PostExecute("LoadACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled load AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled load AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledLoadACClassMethod()
        {
            return SelectedACClassMethod != null;
        }

        /// <summary>
        /// News the work AC class method.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'New Workflow'}de{'Neuer Workflow'}", (short)MISort.New, true, "SelectedACClassMethod", Global.ACKinds.MSMethodPrePost)]
        public void NewWorkACClassMethod()
        {
            if (!PreExecute("NewWorkACClassMethod")) return;

            CurrentNewACClassMethod = ACClassMethod.NewWorkACClassMethod(Database.ContextIPlus, CurrentACClass);

            ShowDialog(this, "WorkACClassMethodNew");
            PostExecute("NewWorkACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled new work AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new work AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewWorkACClassMethod()
        {
            if (Database.ContextIPlus.IsChanged)
                return false;

            return ProjectManager.IsEnabledNewWorkACClassMethod(CurrentACClass);
        }


        /// <summary>
        /// News the script AC class method.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'New C#-Script'}de{'Neues C#-Script'}", (short)MISort.New, true, "SelectedACClassMethod", Global.ACKinds.MSMethodPrePost)]
        public void NewScriptACClassMethod()
        {
            if (!PreExecute("NewACClassMethod")) return;
            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            ACClassMethod acClassMethod = ACClassMethod.NewScriptACClassMethod(Database.ContextIPlus, CurrentACClass, Global.ACKinds.MSMethodExt);
            acClassMethod.SortIndex = Convert.ToInt16(CurrentACClass.ACClassMethod_ACClass.Count + 1);
            CurrentACClass.AddNewACClassMethod(acClassMethod);
            _ACClassMethodList = null;
            OnPropertyChanged("ACClassMethodList");
            SelectedACClassMethod = acClassMethod;
            CurrentACClassMethod = acClassMethod;
            PostExecute("NewACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled new script AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new script AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewScriptACClassMethod()
        {
            return ProjectManager.IsEnabledNewScriptACClassMethod(CurrentACClass);
        }

        /// <summary>
        /// News the script client AC class method.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'New Clientscript'}de{'Neues Clientscript'}", (short)MISort.New, true, "SelectedACClassMethod", Global.ACKinds.MSMethodPrePost)]
        public void NewScriptClientACClassMethod()
        {
            if (!PreExecute("NewACClassMethod")) return;
            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            ACClassMethod acClassMethod = ACClassMethod.NewScriptACClassMethod(Database.ContextIPlus, CurrentACClass, Global.ACKinds.MSMethodExtClient);
            acClassMethod.SortIndex = Convert.ToInt16(CurrentACClass.ACClassMethod_ACClass.Count + 1);
            CurrentACClass.AddNewACClassMethod(acClassMethod);
            _ACClassMethodList = null;
            OnPropertyChanged("ACClassMethodList");
            SelectedACClassMethod = acClassMethod;
            CurrentACClassMethod = acClassMethod;
            PostExecute("NewACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled new script client AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new script client AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewScriptClientACClassMethod()
        {
            return ProjectManager.IsEnabledNewScriptClientACClassMethod(CurrentACClass);
        }

        /// <summary>
        /// News the pre AC class method.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'New Pre-Method'}de{'Neue Pre-Methode'}", (short)MISort.New, true, "SelectedACClassMethod", Global.ACKinds.MSMethodPrePost)]
        public void NewPreACClassMethod()
        {
            if (!PreExecute("NewPreACClassMethod")) return;
            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            ACClassMethod acClassMethod = ACClassMethod.NewPreACClassMethod(Database.ContextIPlus, CurrentACClass, CurrentACClassMethod);
            CurrentACClass.AddNewACClassMethod(acClassMethod);
            _ACClassMethodList = null;
            OnPropertyChanged("ACClassMethodList");
            CurrentACClassMethod = acClassMethod;
            PostExecute("NewPreACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled new pre AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new pre AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewPreACClassMethod()
        {
            return ProjectManager.IsEnabledNewPreACClassMethod(CurrentACClass, CurrentACClassMethod);
        }

        /// <summary>
        /// News the post AC class method.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'New Post-Method'}de{'Neue Post-Methode'}", (short)MISort.New, true, "SelectedACClassMethod", Global.ACKinds.MSMethodPrePost)]
        public void NewPostACClassMethod()
        {
            if (!PreExecute("NewPostACClassMethod")) return;
            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            ACClassMethod acClassMethod = ACClassMethod.NewPostACClassMethod(Database.ContextIPlus, CurrentACClass, CurrentACClassMethod);
            CurrentACClass.AddNewACClassMethod(acClassMethod);
            _ACClassMethodList = null;
            OnPropertyChanged("ACClassMethodList");
            CurrentACClassMethod = acClassMethod;
            PostExecute("NewPostACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled new post AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new post AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewPostACClassMethod()
        {
            return ProjectManager.IsEnabledNewPostACClassMethod(CurrentACClass, CurrentACClassMethod);
        }

        /// <summary>
        /// News the on set property AC class method.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'New OnSetProperty-Method'}de{'Neue OnSetProperty-Methode'}", (short)MISort.New, true, "CurrentACClassProperty", Global.ACKinds.MSMethodPrePost)]
        public void NewOnSetPropertyACClassMethod()
        {
            if (!IsEnabledNewOnSetPropertyACClassMethod())
                return;
            if (!PreExecute("NewOnSetPropertyACClassMethod")) return;
            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            ACClassMethod acClassMethod = ACClassMethod.NewScriptTriggerACClassMethod(ScriptTrigger.Type.OnSetACProperty, Database.ContextIPlus, CurrentACClass, CurrentACClassProperty.ACIdentifier);
            CurrentACClass.AddNewACClassMethod(acClassMethod);
            _ACClassMethodList = null;
            OnPropertyChanged("ACClassMethodList");
            CurrentACClassMethod = acClassMethod;
            PostExecute("NewOnSetPropertyACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled new on set property AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new on set property AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewOnSetPropertyACClassMethod()
        {
            return ProjectManager.IsEnabledNewOnSetPropertyACClassMethod(CurrentACClass, CurrentACClassProperty);
        }


        /// <summary>
        /// News the on set property net AC class method.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'New OnSetPropertyNet-Method'}de{'Neue OnSetPropertyNet-Methode'}", (short)MISort.New, true, "CurrentACClassProperty", Global.ACKinds.MSMethodPrePost)]
        public void NewOnSetPropertyNetACClassMethod()
        {
            if (!IsEnabledNewOnSetPropertyNetACClassMethod())
                return;
            if (!PreExecute("NewOnSetPropertyNetACClassMethod")) return;
            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            ACClassMethod acClassMethod = ACClassMethod.NewScriptTriggerACClassMethod(ScriptTrigger.Type.OnSetACPropertyNet, Database.ContextIPlus, CurrentACClass, CurrentACClassProperty.ACIdentifier);
            CurrentACClass.AddNewACClassMethod(acClassMethod);
            _ACClassMethodList = null;
            OnPropertyChanged("ACClassMethodList");
            CurrentACClassMethod = acClassMethod;
            PostExecute("NewOnSetPropertyNetACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled new on set property net AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new on set property net AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewOnSetPropertyNetACClassMethod()
        {
            return ProjectManager.IsEnabledNewOnSetPropertyNetACClassMethod(CurrentACClass, CurrentACClassProperty);
        }

        /// <summary>
        /// News the on set point AC class method.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'New OnSetPoint-Method'}de{'Neue OnSetPoint-Methode'}", (short)MISort.New, true, "CurrentACClassProperty", Global.ACKinds.MSMethodPrePost)]
        public void NewOnSetPointACClassMethod()
        {
            if (!IsEnabledNewOnSetPointACClassMethod())
                return;
            if (!PreExecute("NewOnSetPointACClassMethod")) return;
            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            ACClassMethod acClassMethod = ACClassMethod.NewScriptTriggerACClassMethod(ScriptTrigger.Type.OnSetACPoint, Database.ContextIPlus, CurrentACClass, CurrentACClassProperty.ACIdentifier);
            CurrentACClass.AddNewACClassMethod(acClassMethod);
            _ACClassMethodList = null;
            OnPropertyChanged("ACClassMethodList");
            CurrentACClassMethod = acClassMethod;
            PostExecute("NewOnSetPointACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled new on set point AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new on set point AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewOnSetPointACClassMethod()
        {
            return ProjectManager.IsEnabledNewOnSetPointACClassMethod(CurrentACClass, CurrentACClassProperty);
        }


        /// <summary>
        /// Deletes the AC class method.
        /// </summary>
        [ACMethodInteraction("ACClassMethod", "en{'Delete Method'}de{'Methode löschen'}", (short)MISort.Delete, true, "CurrentACClassMethod", Global.ACKinds.MSMethodPrePost)]
        public void DeleteACClassMethod()
        {
            if (!PreExecute("DeleteACClassMethod")) return;
            Msg msg = CurrentACClassMethod.DeleteACObject(Database.ContextIPlus, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }

            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }
            _ACClassMethodList = null;
            CurrentACClass.RefreshCachedMethods();
            OnPropertyChanged("ACClassMethodList");
            PostExecute("DeleteACClassMethod");
        }

        /// <summary>
        /// Determines whether [is enabled delete AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteACClassMethod()
        {
            return ProjectManager.IsEnabledDeleteACClassMethod(CurrentACClass, CurrentACClassMethod);
        }

        /// <summary>
        /// Compiles the AC class method.
        /// </summary>
        [ACMethodCommand("ACClassMethod", "en{'Compile'}de{'Kompilieren'}", 9999, true)]
        public void CompileACClassMethod()
        {
            if (!IsEnabledCompileACClassMethod())
                return;

            if (Type.GetType(CurrentACClass.BaseClassWithASQN.AssemblyQualifiedName) == null)
            {
                Messages.Error(this, "Error00006");
                return;
            }
            ScriptEngine _ScriptEngine = new ScriptEngine(CurrentACClass);

            _ScriptEngine.RegisterScript(CurrentACClassMethod.ACIdentifier, CurrentACClassMethod.Sourcecode, CurrentACClassMethod.ContinueByError);

            //foreach (ACClassMethod acClassMethod in query)
            //{
            //    //_ScriptEngine.RegisterEventScript(acClassMethod.ACMethodName, acClassMethod.Sourcecode, acClassMethod.EventTrigger, acClassMethod.EventParameter, acClassMethod.SortIndex, acClassMethod.ContinueByError);
            //    _ScriptEngine.RegisterScript(acClassMethod.ACMethodName, acClassMethod.Sourcecode, acClassMethod.ContinueByError);
            //}

            try
            {
                if (!_ScriptEngine.Compile())
                {
                    // TODO: Im Grid darstellen
                    if (_ScriptEngine.CompileErrors.Any())
                    {
                        string message = string.Format("{0}\n{1}\n\nZeile {2}\tSpalte {3}",
                            _ScriptEngine.CompileErrors[0].ACIdentifier,
                            _ScriptEngine.CompileErrors[0].Message,
                            _ScriptEngine.CompileErrors[0].Row,
                            _ScriptEngine.CompileErrors[0].Column);
                        Messages.Error(this, "Message00001", false, message);
                    }
                }
            }
            catch (Exception e)
            {
                Messages.Exception(this, "Exception", true, e.Message + "(" + e.Source + ")");
            }
        }

        /// <summary>
        /// Determines whether [is enabled compile AC class method].
        /// </summary>
        /// <returns><c>true</c> if [is enabled compile AC class method]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledCompileACClassMethod()
        {
            return CurrentACClass != null && CurrentACClassMethod != null &&
                (CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodExt ||
                CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodExtClient ||
                CurrentACClassMethod.ACKind == Global.ACKinds.MSMethodExtTrigger);
        }

        #endregion
        #endregion

        #region ContextMenuCategory

        private ACValueItem _CurrentContextMenuCategory;
        [ACPropertyCurrent(999,"ContextMenuCategory","en{'Category'}de{'Category'}")]
        public ACValueItem CurrentContextMenuCategory
        {
            get 
            {
                return _CurrentContextMenuCategory;
            }
            set
            {
                if (CurrentACClassMethod != null && CurrentACClassMethod.IsInteraction && CurrentACClassMethod.ContextMenuCategoryIndex != null)
                {
                    _CurrentContextMenuCategory = value;
                    if (value != null)
                    {
                        short contextMenuCatIndex = short.Parse(_CurrentContextMenuCategory.Value.ToString());
                        if (contextMenuCatIndex != CurrentACClassMethod.ContextMenuCategoryIndex)
                            CurrentACClassMethod.ContextMenuCategoryIndex = contextMenuCatIndex;
                    }
                    else
                        CurrentACClassMethod.ContextMenuCategoryIndex = (short)Global.ContextMenuCategory.NoCategory;
                }
                OnPropertyChanged("ContextMenuCategory");
            }
        }

        [ACPropertyList(999,"ContextMenuCategory")]
        public ACValueItemList ContextMenuCategoryList
        {
            get
            {
                return Global.ContextMenuCategoryList;
            }
        }

        #endregion

        #region Layout und Propertychanged
        #endregion

        #region GenerateHandleExecuteACMethods

        [ACMethodInfo("", "en{'Create Method-Handler'}de{'Generiere Methodenhandler'}", 999)]
        public void GenerateExecuteHandler()
        {
            if (CurrentACClass != null)
                GenerateExecuteHandlerInternal(CurrentACClass);
        }

        private static string _4spaces = "    ";
        private static string _8spaces = _4spaces + _4spaces;
        private static string _12spaces = _8spaces + _4spaces;
        private static string _16spaces = _12spaces + _4spaces;
        private static string _20spaces = _16spaces + _4spaces;

        private static string _MethodSignature = "protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName," +
                                                                                      " core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)";

        private static string _MethodStartPart = string.Format("{0}{1}{2}{3}{{{4}{5}result = null;{6}{7}switch (acMethodName){8}{9}{{{10}",
                                                        _8spaces, _MethodSignature, se.NewLine, _8spaces, se.NewLine, _12spaces, se.NewLine, _12spaces, se.NewLine, _12spaces, se.NewLine);

        private static string _MethodEndPart = string.Format("{0}}}{1}{2}return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);{3}{4}}}",
                                                       _12spaces, se.NewLine, _16spaces, se.NewLine, _8spaces);

        private static List<Type> _MethodAttributes;
        private static List<Type> MethodAttributes
        {
            get
            {
                if (_MethodAttributes == null)
                {
                    _MethodAttributes = new List<Type>();
                    _MethodAttributes.AddRange(new Type[5] { typeof(ACMethodInfo), typeof(ACMethodInteraction), typeof(ACMethodCommand), typeof(ACMethodState), typeof(ACMethodAsync) });
                }
                return _MethodAttributes;
            }
        }

        private string GenerateExecuteHandlerInternal(ACClass currentACClass, string classCode = null)
        {
            Type voidTyp = typeof(void);
            string methodText = "";
            methodText = _MethodStartPart;

            bool check = false;

            if (currentACClass.ACKind != Global.ACKinds.TACUndefined)
            {
                Type ClassType = currentACClass.ObjectFullType;
                List<string> ClassMethodsList = new List<string>();
                if (typeof(IACComponent).IsAssignableFrom(ClassType))
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(ClassType.TypeHandle); // Invoke Static Constructor, to get virtual method infos

                foreach (MethodInfo method in ClassType.GetMethods())
                {
                    if (ClassType != method.DeclaringType) continue;

                    object info = null;
                    foreach (Type attribute in MethodAttributes)
                    {
                        info = method.GetCustomAttributes(attribute, false).FirstOrDefault();
                        if (info != null)
                            break;
                    }

                    info = method.GetCustomAttributes<ACMethodInfo>(false).FirstOrDefault();
                    if (method.IsStatic || (info == null && !(method.Name.StartsWith("IsEnabled") && method.ReturnType == typeof(bool)))) continue;

                    string result = "";
                    if (method.ReturnType != voidTyp)
                        result = "result = ";

                    string paramString = "";
                    var parameters = method.GetParameters();
                    int paramCount = parameters.Count();
                    if (paramCount > 0)
                    {
                        for (int i = 0; i < paramCount; i++)
                        {
                            ParameterInfo pInfo = parameters[i];

                            string defaultValueStart = "";
                            string defaultValueEnd = "";

                            if (pInfo.HasDefaultValue)
                            {
                                defaultValueStart = string.Format("acParameter.Count() == {0} ? ", i + 1);
                                defaultValueEnd = string.Format(" : {0}", pInfo.DefaultValue == null ? "null" : pInfo.DefaultValue.ToString().ToLower());
                            }

                            if (pInfo.ParameterType.IsGenericType)
                            {
                                var gParameters = pInfo.ParameterType.GetGenericArguments();
                                int gParamCount = gParameters.Count();
                                string genericParam = "";
                                string gParamReplaced = Regex.Replace(pInfo.ParameterType.Name, "[`0-9]{2,}", "<");

                                if (!(classCode != null && classCode.Contains("using " + pInfo.ParameterType.Namespace)))
                                    genericParam = pInfo.ParameterType.Namespace + ".";

                                genericParam += gParamReplaced;
                                for (int j = 0; j < gParamCount; j++)
                                {
                                    genericParam += gParameters[j].FullName;
                                    if (j < gParamCount - 1)
                                        genericParam += ", ";
                                }
                                genericParam += ">";

                                paramString += string.Format("{3}({1})acParameter[{0}]{2}", i, genericParam, defaultValueEnd, defaultValueStart);
                                if (i < paramCount - 1)
                                    paramString += ", ";
                            }
                            else if (pInfo.ParameterType.IsEnum)
                            {
                                string enumParam = "";
                                if (!(classCode != null && classCode.Contains("using " + pInfo.ParameterType.Namespace)))
                                    enumParam = pInfo.ParameterType.Namespace + ".";

                                if (!(classCode != null && pInfo.ParameterType.DeclaringType != null &&
                                    classCode.Contains("using " + pInfo.ParameterType.Namespace + "." + pInfo.ParameterType.DeclaringType.Name)))
                                    enumParam += "Global.";

                                enumParam += pInfo.ParameterType.Name;
                                paramString += string.Format("{3}({1})acParameter[{0}]{2}", i, enumParam, defaultValueEnd, defaultValueStart);
                                if (i < paramCount - 1)
                                    paramString += ", ";
                            }
                            else
                            {
                                string pType = pInfo.ParameterType.ToString();

                                if (classCode != null && classCode.Contains("using " + pInfo.ParameterType.Namespace))
                                    pType = pInfo.ParameterType.Name;

                                paramString += string.Format("{3}({1})acParameter[{0}]{2}", i, pType, defaultValueEnd, defaultValueStart);
                                if (i < paramCount - 1)
                                    paramString += ", ";
                            }
                        }
                    }

                    methodText += string.Format("{0}case\"{1}\":{2}{3}{4}{5}({6});{7}{8}return true;{9}",
                                                       _16spaces, method.Name, se.NewLine, _20spaces, result, method.Name, paramString, se.NewLine, _20spaces, se.NewLine);
                    check = true;
                }

            }

            methodText += _MethodEndPart;
            System.Windows.Clipboard.SetText(methodText);
            if (check)
                return methodText;
            return null;
        }
        #endregion

    }
}