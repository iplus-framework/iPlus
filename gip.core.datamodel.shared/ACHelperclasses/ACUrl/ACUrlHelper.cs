// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-08-2012
// ***********************************************************************
// <copyright file="ACUrlHelper.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Class ACUrlValidation
    /// </summary>
    public static class ACUrlValidation
    {
        /// <summary>
        /// Determines whether [contains AC URL delmiters] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if [contains AC URL delmiters] [the specified value]; otherwise, <c>false</c>.</returns>
        public static bool ContainsACUrlDelimiters(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;
            if (   value.IndexOfAny(ACUrlHelper.DelimitersReserved) >= 0
                || Char.IsNumber(value,0)
                )
                return true;
            return false;
        }
    }

    /// <summary>
    /// Class ACUrlHelper
    /// </summary>
    public class ACUrlHelper
    {
        #region Delimiter

        /// <summary>
        /// Path to a Child
        /// </summary>
        public const char Delimiter_DirSeperator = '\\';

        /// <summary>
        /// Relative Path
        /// </summary>
        public const char Delimiter_RelativePath = '.';
        public const string DelString_RelativeParent = "..";
        public const string DelString_RelativeParentChild = "..\\";
        public const string DelString_RelativeChild = ".\\";

        /// <summary>
        /// Relative Path
        /// </summary>
        public const char Delimiter_Exists = '?';

        /// <summary>
        /// Start ACComponent (If not Autostart)
        /// </summary>
        public const char Delimiter_Start = '#';

        /// <summary>
        /// Stop ACComponent
        /// </summary>
        public const char Delimiter_Stop = '~';

        /// <summary>
        /// Invoke Method
        /// </summary>
        public const char Delimiter_InvokeMethod = '!';

        /// <summary>
        /// Get a Translated text through an ACIdentifier
        /// </summary>
        public const char Delimiter_Translate = '§';

        /// <summary>
        /// Custom Messages, which are additionally in ACUrlCommand implemented and sended through ACUrlCmdMessage
        /// </summary>
        public const char Delimiter_CustomMessage = '^';

        /// <summary>
        /// '(' - Open-Bracket for indexing/identifying a dynamic instance (BSO oder Workflow)
        /// </summary>
        public const char Delimiter_InstanceNoOpen = '(';

        /// <summary>
        /// ')' - Close-Bracket for indexing/identifying a dynamic instance (BSO oder Workflow)
        /// </summary>
        public const char Delimiter_InstanceNoClose = ')';

        /// <summary>
        /// "||" - Relationship-Delimiter for separating two or more ACUrl for describing an relationship between them.
        /// </summary>
        public const string Delimiter_Relationship = "||";

        /// <summary>
        /// Symbol that is used to concatenate Identifiers or Captions for attached methods
        /// </summary>
        public const char AttachedMethodIDConcatenator = '@';

        public const char OpeningBrace = '{';

        public const char ClosingBrace = '}';

        /// <summary>
        /// All reserved Characters, which are not allowed to use in a ACIdentifer
        /// </summary>
        public static readonly char[] DelimitersReserved = new char[] {
            Delimiter_DirSeperator,
            Delimiter_RelativePath,
            Delimiter_Exists,
            Delimiter_Start,
            Delimiter_Stop,
            Delimiter_InvokeMethod,
            Delimiter_Translate,
            Delimiter_CustomMessage,
            Delimiter_InstanceNoOpen,
            Delimiter_InstanceNoClose,
            '<', '>', '+', '-', '/', '*', '|', '$', '%', '%', '&', '{', '}', '[', ']' };

        /// <summary>
        /// All delimiters which are used for separating Instances inside a ACURl
        /// </summary>
        public static readonly char[] ACUrlKeyDelimiters = new char[] {
            Delimiter_DirSeperator,
            Delimiter_Exists,
            Delimiter_Start,
            Delimiter_Stop,
            Delimiter_InvokeMethod,
            Delimiter_Translate,
            Delimiter_CustomMessage };
        #endregion



        /// <summary>
        /// Initializes a new instance of the <see cref="ACUrlHelper"/> class.
        /// </summary>
        /// <param name="acUrl">The ac URL.</param>
        public ACUrlHelper(string acUrl)
        {
            char delimiter;
            _UrlKey = AnalyzeSegment(acUrl, out _ACUrlPart, out _NextACUrl, out delimiter, out _CmdDelimiter);
        }

        public static UrlKeys AnalyzeSegment(string acUrl, out string acUrlPart, out string nextACUrl, out char delimiter, out char cmdDelimiter)
        {
            UrlKeys urlKey = UrlKeys.Unknown;
            cmdDelimiter = Char.MinValue;
            acUrlPart = "";
            nextACUrl = "";
            delimiter = !String.IsNullOrEmpty(acUrl) ? acUrl[0] : Char.MinValue;
            if (delimiter == Delimiter_Exists)
            {
                cmdDelimiter = delimiter;
                acUrl = acUrl.Substring(1);
            }

            if (String.IsNullOrEmpty(acUrl))
            {
                urlKey = UrlKeys.Unknown;
                acUrlPart = "";
                nextACUrl = "";
            }
            else
            {
                switch (acUrl[0])
                {
                    case Delimiter_DirSeperator:
                        urlKey = UrlKeys.Root;
                        acUrlPart = "";
                        nextACUrl = cmdDelimiter != Char.MinValue ? cmdDelimiter + acUrl.Substring(1) : acUrl.Substring(1);
                        break;
                    case Delimiter_InvokeMethod:
                        urlKey = UrlKeys.InvokeMethod;
                        acUrlPart = acUrl.Substring(1);
                        nextACUrl = "";
                        break;
                    case Delimiter_CustomMessage:
                        urlKey = UrlKeys.CustomMessage;
                        acUrlPart = acUrl.Substring(1);
                        nextACUrl = "";
                        break;
                    case Delimiter_Stop:
                        urlKey = UrlKeys.Stop;
                        acUrlPart = acUrl.Substring(1);
                        nextACUrl = "";
                        break;
                    case Delimiter_Start:
                        urlKey = UrlKeys.Start;
                        acUrlPart = acUrl.Substring(1);
                        nextACUrl = "";
                        break;
                    case Delimiter_Translate:
                        urlKey = UrlKeys.TranslationText;
                        acUrlPart = acUrl.Substring(1);
                        nextACUrl = "";
                        break;
                    case Delimiter_RelativePath:
                        if (acUrl.StartsWith(DelString_RelativeParent))
                        {
                            urlKey = UrlKeys.Parent;
                            acUrlPart = "";
                            if (acUrl.StartsWith(DelString_RelativeParentChild))
                            {
                                nextACUrl = cmdDelimiter != Char.MinValue ? cmdDelimiter + acUrl.Substring(3) : acUrl.Substring(3);
                            }
                            else
                            {
                                nextACUrl = cmdDelimiter != Char.MinValue ? cmdDelimiter + acUrl.Substring(2) : acUrl.Substring(2);
                            }
                        }
                        else if (acUrl.StartsWith(DelString_RelativeChild))
                        {
                            acUrl = acUrl.Substring(2);
                            urlKey = UrlKeys.Child;
                            int pos = acUrl.IndexOfAny(ACUrlKeyDelimiters, 0);

                            if (pos == -1)
                            {
                                acUrlPart = acUrl;
                                nextACUrl = "";
                            }
                            else
                            {
                                acUrlPart = acUrl.Substring(0, pos);
                                if (acUrl[pos] == Delimiter_DirSeperator)
                                    nextACUrl = acUrl.Substring(pos + 1);
                                else
                                    nextACUrl = acUrl.Substring(pos);
                                if (!String.IsNullOrEmpty(nextACUrl) && cmdDelimiter != Char.MinValue)
                                    nextACUrl = cmdDelimiter + nextACUrl;
                            }
                        }
                        break;
                    default:    // Auflösung über ACObjectChilds
                        {
                            urlKey = UrlKeys.Child;
                            int pos = acUrl.IndexOfAny(ACUrlKeyDelimiters, 0);

                            if (pos == -1)
                            {
                                acUrlPart = acUrl;
                                nextACUrl = "";
                            }
                            else
                            {
                                acUrlPart = acUrl.Substring(0, pos);
                                if (acUrl[pos] == Delimiter_DirSeperator)
                                    nextACUrl = acUrl.Substring(pos + 1);
                                else
                                    nextACUrl = acUrl.Substring(pos);
                                if (!String.IsNullOrEmpty(nextACUrl) && cmdDelimiter != Char.MinValue)
                                    nextACUrl = cmdDelimiter + nextACUrl;
                            }
                        }
                        break;
                }
            }
            return urlKey;
        }

        /// <summary>
        /// Enum UrlKeys
        /// </summary>
        public enum UrlKeys : short
        {
            /// <summary>
            /// Unkown Command
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Get the child Component: ".\\"
            /// </summary>
            Child = 1,

            /// <summary>
            /// Get the root Component (Application): "\\"
            /// </summary>
            Root = 2,

            /// <summary>
            /// Get the parent Component:  "..\\"
            /// </summary>
            Parent = 3,

            /// <summary>
            /// Invoke a Method: '!'
            /// </summary>
            InvokeMethod = 4,

            /// <summary>
            /// Instance a new Component: '#'
            /// </summary>
            Start = 6,

            /// <summary>
            /// Stop a component: '~'
            /// </summary>
            Stop = 7,

            /// <summary>
            /// Get a Translated text through an ACIdentifier: '§'
            /// </summary>
            TranslationText = 11,

            /// <summary>
            /// Custom Messages, which are additionally in ACUrlCommand implemente and sended through ACUrlCmdMessage: '^'
            /// </summary>
            CustomMessage = 12
        }

        /// <summary>
        /// Enum UrlTypes
        /// </summary>
        public enum UrlTypes : short
        {
            /// <summary>
            /// The value type
            /// </summary>
            ValueType = 0,
            /// <summary>
            /// The query type
            /// </summary>
            QueryType = 1,
        }

        /// <summary>
        /// The _ parse char
        /// </summary>
        private char _CmdDelimiter = Char.MinValue;

        /// <summary>
        /// The _ URL key
        /// </summary>
        UrlKeys _UrlKey = UrlKeys.Unknown;
        /// <summary>
        /// Gets the URL key.
        /// </summary>
        /// <value>The URL key.</value>
        public UrlKeys UrlKey
        {
            get
            {
                return _UrlKey;
            }
        }

        /// <summary>
        /// Gets the type of the URL.
        /// </summary>
        /// <value>The type of the URL.</value>
        public UrlTypes UrlType
        {
            get
            {
                switch (_CmdDelimiter)
                {
                    case Delimiter_Exists:
                        return UrlTypes.QueryType;
                    default:
                        return UrlTypes.ValueType;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is query.
        /// </summary>
        /// <value><c>true</c> if this instance is query; otherwise, <c>false</c>.</value>
        public bool IsQuery
        {
            get
            {
                return _CmdDelimiter == Delimiter_Exists;
            }
        }

        /// <summary>
        /// The _ AC URL part
        /// </summary>
        string _ACUrlPart = "";
        /// <summary>
        /// Gets the AC URL part.
        /// </summary>
        /// <value>The AC URL part.</value>
        public string ACUrlPart
        {
            get
            {
                return _ACUrlPart;
            }
        }

        /// <summary>
        /// The _ next AC URL
        /// </summary>
        string _NextACUrl = "";
        /// <summary>
        /// Gets the next AC URL.
        /// </summary>
        /// <value>The next AC URL.</value>
        public string NextACUrl
        {
            get
            {
                return _NextACUrl;
            }
        }

        // ACIdentifier of ACComponent: "Type(Instance)"
        /// <summary>
        /// Extracts the name of the type.
        /// </summary>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <returns>System.String.</returns>
        static public string ExtractTypeName(string acIdentifier) // ExtractTypeName GetTypePartOfACIdentifier
        {
            if (String.IsNullOrEmpty(acIdentifier))
                return acIdentifier;
            int pos = acIdentifier.IndexOf(Delimiter_InstanceNoOpen);
            if (pos == -1)
            {
                return acIdentifier;
            }
            else
            {
                return acIdentifier.Substring(0, pos);
            }
        }

        /// <summary>
        /// Extracts the name of the instance.
        /// </summary>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <returns>System.String.</returns>
        static public string ExtractInstanceName(string acIdentifier) // ExtractInstance GetInstancePartOfACIdentifier
        {
            if (String.IsNullOrEmpty(acIdentifier))
                return acIdentifier;
            int pos = acIdentifier.IndexOf(Delimiter_InstanceNoOpen);
            int pos2 = acIdentifier.IndexOf(Delimiter_InstanceNoClose);
            if (pos == -1 || pos2 <= pos)
            {
                return null;
            }
            else
            {
                return acIdentifier.Substring(pos + 1, pos2 - pos - 1);
            }
        }

        /// <summary>
        /// Gets the filter values.
        /// </summary>
        /// <param name="acName">Name of the ac.</param>
        /// <returns>System.String[][].</returns>
        static public string[] GetFilterValues(string acName)
        {
            int pos = acName.IndexOf(Delimiter_InstanceNoOpen);

            int pos2 = acName.IndexOf(Delimiter_InstanceNoClose);
            if (pos2 != -1)
            {
                pos2 = -1;
                pos2 = acName.ToDictionary(key => ++pos2, val => val).Where(x => x.Value == Delimiter_InstanceNoClose).Last().Key;
            }

            if (pos == -1 || pos2 <= pos)
            {
                return null;
            }
            else
            {
                string filter = acName.Substring(pos + 1, pos2 - pos - 1);

                return filter.Split(',');
            }
        }

        /// <summary>
        /// Structure of ACIdentifier at VBDocking/GUI-Objects
        /// GUI:VBControlClassName:FrameworkElementName:VBContent:ACNameOfComponent
        /// </summary>
        /// <param name="acNameOfVBContent">Content of the ac name of VB.</param>
        /// <param name="filterVBControlClassName">ACClassName of</param>
        /// <param name="filterFrameworkElementName">Name-Property of FrameworkElement</param>
        /// <param name="filterVBContent">VBContent of IACInteractiveObject, which is most the Name of the XMLDesign (VBContent-Member of IACInteractiveObject)</param>
        /// <param name="filterACNameOfComponent">ACIdentifier of Component, which is set as DataContext (ContextACObject-Member of IACInteractiveObject)</param>
        /// <returns><c>true</c> if [is searched GUI instance] [the specified ac name of VB content]; otherwise, <c>false</c>.</returns>
        static public bool IsSearchedGUIInstance(string acNameOfVBContent,
            string filterVBControlClassName = "",
            string filterFrameworkElementName = "",
            string filterVBContent = "",
            string filterACNameOfComponent = "")
        {
            if (String.IsNullOrEmpty(acNameOfVBContent))
                return false;
            bool filterVBControlClassNameSet = !String.IsNullOrEmpty(filterVBControlClassName);
            bool filterFrameworkElementNameSet = !String.IsNullOrEmpty(filterFrameworkElementName);
            bool filterACNameOfComponentSet = !String.IsNullOrEmpty(filterACNameOfComponent);
            bool filterVBContentSet = !String.IsNullOrEmpty(filterVBContent);
            string[] splittedACName = acNameOfVBContent.Split(new char[] { ':' });
            if (splittedACName.Count() < 5)
                return false;
            if (splittedACName[0] != "GUI")
                return false;
            if (!filterVBControlClassNameSet && !filterFrameworkElementNameSet && !filterACNameOfComponentSet && !filterVBContentSet)
                return true;

            if (!filterVBContentSet && filterVBControlClassNameSet && filterFrameworkElementNameSet && filterACNameOfComponentSet)
            {
                if ((splittedACName[1] == filterVBControlClassName)
                    && (splittedACName[2] == filterFrameworkElementName)
                    && (splittedACName[4] == filterACNameOfComponent))
                    return true;
                return false;
            }
            else if (!filterVBContentSet && filterVBControlClassNameSet && !filterFrameworkElementNameSet && filterACNameOfComponentSet)
            {
                if ((splittedACName[1] == filterVBControlClassName)
                    && (splittedACName[4] == filterACNameOfComponent))
                    return true;
                return false;
            }
            else if (!filterVBContentSet && filterVBControlClassNameSet && filterFrameworkElementNameSet && !filterACNameOfComponentSet)
            {
                if ((splittedACName[1] == filterVBControlClassName)
                    && (splittedACName[2] == filterFrameworkElementName))
                    return true;
                return false;
            }
            else if (!filterVBContentSet && filterVBControlClassNameSet && !filterFrameworkElementNameSet && !filterACNameOfComponentSet)
            {
                if (splittedACName[1] == filterVBControlClassName)
                    return true;
                return false;
            }
            else if (!filterVBContentSet && !filterVBControlClassNameSet && filterFrameworkElementNameSet && filterACNameOfComponentSet)
            {
                if ((splittedACName[2] == filterFrameworkElementName)
                    && (splittedACName[4] == filterACNameOfComponent))
                    return true;
                return false;
            }
            else if (!filterVBContentSet && !filterVBControlClassNameSet && filterFrameworkElementNameSet && !filterACNameOfComponentSet)
            {
                if (splittedACName[2] == filterFrameworkElementName)
                    return true;
                return false;
            }
            else if (!filterVBContentSet && filterVBControlClassNameSet && !filterFrameworkElementNameSet && filterACNameOfComponentSet)
            {
                if ((splittedACName[1] == filterVBControlClassName)
                    && (splittedACName[4] == filterACNameOfComponent))
                    return true;
                return false;
            }
            else if (!filterVBContentSet && !filterVBControlClassNameSet && !filterFrameworkElementNameSet && filterACNameOfComponentSet)
            {
                if (splittedACName[4] == filterACNameOfComponent)
                    return true;
                return false;
            }
            else if (filterVBContentSet && filterVBControlClassNameSet && filterFrameworkElementNameSet && filterACNameOfComponentSet)
            {
                if ((splittedACName[1] == filterVBControlClassName)
                    && (splittedACName[2] == filterFrameworkElementName)
                    && (splittedACName[3] == filterVBContent)
                    && (splittedACName[4] == filterACNameOfComponent))
                    return true;
                return false;
            }
            else if (filterVBContentSet && filterVBControlClassNameSet && !filterFrameworkElementNameSet && filterACNameOfComponentSet)
            {
                if ((splittedACName[1] == filterVBControlClassName)
                    && (splittedACName[3] == filterVBContent)
                    && (splittedACName[4] == filterACNameOfComponent))
                    return true;
                return false;
            }
            else if (filterVBContentSet && filterVBControlClassNameSet && filterFrameworkElementNameSet && !filterACNameOfComponentSet)
            {
                if ((splittedACName[1] == filterVBControlClassName)
                    && (splittedACName[2] == filterFrameworkElementName)
                    && (splittedACName[3] == filterVBContent))
                    return true;
                return false;
            }
            else if (filterVBContentSet && filterVBControlClassNameSet && !filterFrameworkElementNameSet && !filterACNameOfComponentSet)
            {
                if ((splittedACName[1] == filterVBControlClassName)
                    && (splittedACName[3] == filterVBContent))
                    return true;
                return false;
            }
            else if (filterVBContentSet && !filterVBControlClassNameSet && filterFrameworkElementNameSet && filterACNameOfComponentSet)
            {
                if ((splittedACName[2] == filterFrameworkElementName)
                    && (splittedACName[3] == filterVBContent)
                    && (splittedACName[4] == filterACNameOfComponent))
                    return true;
                return false;
            }
            else if (filterVBContentSet && !filterVBControlClassNameSet && filterFrameworkElementNameSet && !filterACNameOfComponentSet)
            {
                if ((splittedACName[2] == filterFrameworkElementName)
                    && (splittedACName[3] == filterVBContent))
                    return true;
                return false;
            }
            else if (filterVBContentSet && filterVBControlClassNameSet && !filterFrameworkElementNameSet && filterACNameOfComponentSet)
            {
                if ((splittedACName[1] == filterVBControlClassName)
                    && (splittedACName[3] == filterVBContent)
                    && (splittedACName[4] == filterACNameOfComponent))
                    return true;
                return false;
            }
            else if (filterVBContentSet && !filterVBControlClassNameSet && !filterFrameworkElementNameSet && filterACNameOfComponentSet)
            {
                if ((splittedACName[4] == filterACNameOfComponent)
                    && (splittedACName[3] == filterVBContent))
                    return true;
                return false;
            }
            else if (filterVBContentSet && !filterVBControlClassNameSet && !filterFrameworkElementNameSet && !filterACNameOfComponentSet)
            {
                if (splittedACName[3] == filterVBContent)
                    return true;
                return false;
            }

            return false;
        }

#if NETFRAMEWORK
/// <summary>
        /// Structure of ACIdentifier at VBDocking/GUI-Objects
        /// GUI:VBControlClassName:FrameworkElementName:VBContent:ACNameOfComponent
        /// </summary>
        /// <param name="guiObject">The GUI object.</param>
        /// <param name="frameworkElementName">Name of the framework element.</param>
        /// <returns>System.String.</returns>
        static public string BuildACNameForGUI(IACInteractiveObject guiObject, string frameworkElementName)
        {
            if (guiObject == null)
                return null;
            string objectFullName = guiObject.GetType().FullName;
            string vbControlClassName = guiObject.GetType().Name;
            string acNameOfComponent = "";
            if (guiObject.ContextACObject != null)
                acNameOfComponent = guiObject.ContextACObject.ACIdentifier;
            return "GUI:" + vbControlClassName + ":" + frameworkElementName + ":" + guiObject.VBContent + ":" + acNameOfComponent;
        }
#endif

        static public List<string> ResolveParents(string acUrl, bool breakOnCommand = false)
        {
            List<string> parts = new List<string>();
            string subACUrl = acUrl;
            string lastParent = "";

            UrlKeys urlKey = UrlKeys.Unknown;
            string acUrlPart = null;
            string nextACUrl = null;
            char cmdDelimiter = Char.MinValue;
            char delimiter = Char.MinValue;
            while (!String.IsNullOrEmpty(subACUrl))
            {
                urlKey = AnalyzeSegment(subACUrl, out acUrlPart, out nextACUrl, out delimiter, out cmdDelimiter);
                subACUrl = nextACUrl;
                if (breakOnCommand && urlKey != UrlKeys.Root && urlKey != UrlKeys.Child)
                    break;
                if (urlKey == UrlKeys.Root)
                    continue;
                else if (urlKey == UrlKeys.Child)
                {
                    lastParent += Delimiter_DirSeperator + acUrlPart;
                    parts.Add(lastParent);
                }
                else if (delimiter != Char.MinValue)
                {
                    lastParent += delimiter + acUrlPart;
                    parts.Add(lastParent);
                }
                else
                    break;
            }
            return parts;
        }

        static public List<string> SplitSegments(string acUrl, bool breakOnCommand = false)
        {
            List<string> parts = new List<string>();
            string subACUrl = acUrl;

            UrlKeys urlKey = UrlKeys.Unknown;
            string acUrlPart = null;
            string nextACUrl = null;
            char cmdDelimiter = Char.MinValue;
            char delimiter = Char.MinValue;
            while (!String.IsNullOrEmpty(subACUrl))
            {
                urlKey = AnalyzeSegment(subACUrl, out acUrlPart, out nextACUrl, out delimiter, out cmdDelimiter);
                subACUrl = nextACUrl;
                if (breakOnCommand && urlKey != UrlKeys.Root && urlKey != UrlKeys.Child)
                    break;
                if (urlKey == UrlKeys.Root)
                    continue;
                else if (urlKey == UrlKeys.Child)
                {
                    parts.Add(acUrlPart);
                }
                else if (delimiter != Char.MinValue)
                {
                    parts.Add(acUrlPart);
                }
                else
                    break;
            }
            return parts;
        }

        public static int CalcDistance(string acUrl1, string acUrl2)
        {
            if (String.IsNullOrEmpty(acUrl1) || String.IsNullOrEmpty(acUrl2))
                return -1;
            if (acUrl1 == acUrl2)
                return 0;
            if (acUrl1.Length == acUrl2.Length)
                return -1;
            List<string> hier1 = ResolveParents(acUrl1, true);
            List<string> hier2 = ResolveParents(acUrl2, true);
            if (!hier1.Any() || !hier2.Any())
                return -1;
            if (hier1.Count > hier2.Count)
            {
                if (!hier1.Contains(hier2.Last()))
                    return -1;
                return hier1.Count - hier2.Count;
            }
            else
            {
                if (!hier2.Contains(hier1.Last()))
                    return -1;
                return hier2.Count - hier1.Count;
            }
        }

        public static bool IsUrlDynamicInstance(string acUrl)
        {
            if (String.IsNullOrEmpty(acUrl))
                return false;
            return acUrl.IndexOf(Delimiter_InstanceNoOpen) > 0 || acUrl.IndexOf(Delimiter_InstanceNoClose) > 0;
        }

        public static string GetTrimmedName(string vbContent)
        {
            String str = vbContent;
            foreach (char c in DelimitersReserved)
            {
                str = str.Replace(c.ToString(), String.Empty);
            }
            return str;
        }

#if NETFRAMEWORK
        public static string BuildConfigACUrl(IACConfig config)
        {
            if (config == null)
                return null;
            if (String.IsNullOrEmpty(config.PreConfigACUrl))
            {
                if (String.IsNullOrEmpty(config.LocalConfigACUrl))
                    return config.LocalConfigACUrl;
                //else if (!config.LocalConfigACUrl.StartsWith("\\"))
                //    return "\\" + config.LocalConfigACUrl;
                return config.LocalConfigACUrl;
            }
            else if (config.PreConfigACUrl.EndsWith("\\"))
            {
                if (!config.LocalConfigACUrl.StartsWith("\\"))
                    return config.PreConfigACUrl + config.LocalConfigACUrl;
                else
                    return config.PreConfigACUrl + config.LocalConfigACUrl.Substring(1);
            }
            else
            {
                if (!config.LocalConfigACUrl.StartsWith("\\"))
                    return config.PreConfigACUrl + "\\" + config.LocalConfigACUrl;
                else
                    return config.PreConfigACUrl + config.LocalConfigACUrl;
            }
        }

        public static string BuildLocalConfigACUrl(IACConfigURL configUrl)
        {
            if (configUrl == null)
                return null;
            if (String.IsNullOrEmpty(configUrl.PreValueACUrl))
                return configUrl.ConfigACUrl;
            else
            {
                return configUrl.ConfigACUrl.Replace(configUrl.PreValueACUrl, "");
            }
        }
#endif

        public static string GetParentACUrl(string acURL)
        {
            if (String.IsNullOrEmpty(acURL))
                return acURL;
            int indexOfLastSegment = acURL.LastIndexOf(ACUrlHelper.Delimiter_DirSeperator);
            if (indexOfLastSegment > 0)
                return acURL.Substring(0, indexOfLastSegment);
            return acURL;
        }
    }
}
