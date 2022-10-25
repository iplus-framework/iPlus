// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ScriptTrigger.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ScriptTrigger
    /// </summary>
    public class ScriptTrigger
    {
        /// <summary>
        /// The method signature this param
        /// </summary>
        public const string MethodSignatureThisParam = "IACComponent acComponent";

        /// <summary>
        /// The _ script triggers
        /// </summary>
        private static ScriptTrigger[] _ScriptTriggers = new ScriptTrigger[5] 
        {
            new ScriptTrigger { TriggerType = Type.PreExecute, 
                                         MethodNamePrefix = "_Pre_", 
                                         MethodSignature = "(" + MethodSignatureThisParam + ")",
                                         MethodReturnSignature = "Msg"},
            new ScriptTrigger { TriggerType = Type.PostExecute,
                                         MethodNamePrefix = "_Post_", 
                                         MethodSignature = "(" + MethodSignatureThisParam + ")" },
            new ScriptTrigger { TriggerType = Type.OnSetACProperty,
                                         MethodNamePrefix = "_OnSetACProperty_", 
                                         MethodSignature = "(" + MethodSignatureThisParam + ", object value, IACMember callingProperty)" },
            new ScriptTrigger { TriggerType = Type.OnSetACPropertyNet,
                                         MethodNamePrefix = "_OnSetACPropertyNet_", 
                                         MethodSignature = "(" + MethodSignatureThisParam + ", IACPropertyNetValueEvent valueEvent, IACPropertyNetBase callingProperty)" },
            new ScriptTrigger { TriggerType = Type.OnSetACPoint,
                                         MethodNamePrefix = "_OnSetACPoint_", 
                                         MethodSignature = "(" + MethodSignatureThisParam + ", IACPointNetBase changedPoint)" }        
        };

        /// <summary>
        /// Gets the script triggers.
        /// </summary>
        /// <value>The script triggers.</value>
        public static ScriptTrigger[] ScriptTriggers
        {
            get
            {
                return _ScriptTriggers;
            }
        }

        /// <summary>
        /// Enum Type
        /// </summary>
        public enum Type : short
        {
            /// <summary>
            /// The pre execute
            /// </summary>
            PreExecute = 0,
            /// <summary>
            /// The post execute
            /// </summary>
            PostExecute = 1,
            /// <summary>
            /// The on set AC property
            /// </summary>
            OnSetACProperty = 2,
            /// <summary>
            /// The on set AC property net
            /// </summary>
            OnSetACPropertyNet = 3,
            /// <summary>
            /// The on set AC point
            /// </summary>
            OnSetACPoint = 4,
        }

        /// <summary>
        /// Gets or sets the type of the trigger.
        /// </summary>
        /// <value>The type of the trigger.</value>
        public Type TriggerType { get; set; }
        /// <summary>
        /// Gets or sets the method name prefix.
        /// </summary>
        /// <value>The method name prefix.</value>
        public string MethodNamePrefix { get; set; }
        /// <summary>
        /// Gets or sets the method signature.
        /// </summary>
        /// <value>The method signature.</value>
        public string MethodSignature { get; set; }
        /// <summary>
        /// Gets or sets the method return signature.
        /// </summary>
        /// <value>The method return signature.</value>
        public string MethodReturnSignature { get; set; }

#if !EFCR
        public string GetMethodSignatureForACClass(ACClass acClass)
        {
            string name = acClass.ACIdentifier.Substring(0, 1).ToLower() + acClass.ACIdentifier.Substring(1);
            return MethodSignature.Replace(MethodSignatureThisParam, acClass.ObjectType.Name + " " + name);
        }
#endif

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        /// <param name="MethodNamePostFix">The method name post fix.</param>
        /// <returns>System.String.</returns>
        public string GetMethodName(string MethodNamePostFix)
        {
            return MethodNamePrefix + MethodNamePostFix;
        }
    }
}
