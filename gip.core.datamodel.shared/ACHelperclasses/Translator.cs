// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-17-2013
// ***********************************************************************
// <copyright file="Translator.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Class Translator
    /// </summary>
    public static class Translator
    {
        /// <summary>
        /// Gets or sets the MD language code.
        /// </summary>
        /// <value>The MD language code.</value>
        public static string VBLanguageCode { get; set; }
        /// <summary>
        /// Gets or sets the default MD language code.
        /// </summary>
        /// <value>The default MD language code.</value>
        public static string DefaultVBLanguageCode { get; set; }

#if NETFRAMEWORK
        /// <summary>
        /// The _ AC URL translation cache
        /// </summary>
        private static ACUrlTranslationCache _ACUrlTranslationCache = new ACUrlTranslationCache();
#endif

        /// <summary>
        /// Gets the translation.
        /// </summary>
        /// <param name="translationTuple">Structure of TranslationTuple: languageCode1{'translatedText1'}languageCode2{'translatedText2'}..</param>
        /// <returns>System.String.</returns>
        public static string GetTranslation(string translationTuple)
        {
            return GetTranslation("", translationTuple, VBLanguageCode);
        }

        /// <summary>
        /// Gets the translation.
        /// </summary>
        /// <param name="acName">Name of the ac.</param>
        /// <param name="translationTuple">Structure of TranslationTuple: languageCode1{'translatedText1'}languageCode2{'translatedText2'}..</param>
        /// <returns>System.String.</returns>
        public static string GetTranslation(string acName, string translationTuple)
        {
            return GetTranslation(acName, translationTuple, VBLanguageCode);
        }

        /// <summary>
        /// Gets the translation.
        /// </summary>
        /// <param name="acName">Name of the ac.</param>
        /// <param name="translationTuple">Structure of TranslationTuple: languageCode1{'translatedText1'}languageCode2{'translatedText2'}..</param>
        /// <param name="VBLanguageCode">The md language code.</param>
        /// <returns>System.String.</returns>
        public static string GetTranslation(string acName, string translationTuple, string VBLanguageCode)
        {
            TranslationTupleHelper translatorHelper = new TranslationTupleHelper(translationTuple);
            string translatedText = translatorHelper.GetValue(VBLanguageCode);
            if (String.IsNullOrEmpty(translatedText) && !String.IsNullOrEmpty(acName))
                return acName;
            else if (String.IsNullOrEmpty(translatedText))
            {
                if (translatorHelper.Values.Any())
                    return translatorHelper.Values.FirstOrDefault();
            }
            return translatedText;
        }

        /// <summary>
        /// Sets the translation.
        /// </summary>
        /// <param name="translationTuple">Structure of TranslationTuple: languageCode1{'translatedText1'}languageCode2{'translatedText2'}..</param>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public static string SetTranslation(string translationTuple, string value)
        {
            return SetTranslation(translationTuple, value, VBLanguageCode);
        }

        /// <summary>
        /// Sets the translation.
        /// </summary>
        /// <param name="translationTuple">Structure of TranslationTuple: languageCode1{'translatedText1'}languageCode2{'translatedText2'}..</param>
        /// <param name="value">The value.</param>
        /// <param name="VBLanguageCode">The md language code.</param>
        /// <returns>System.String.</returns>
        public static string SetTranslation(string translationTuple, string value, string VBLanguageCode)
        {
            TranslationTupleHelper translatorHelper = new TranslationTupleHelper(translationTuple);
            translatorHelper.SetValue(VBLanguageCode, value);
            return translatorHelper.CaptionLocalized;
        }

        public static bool IsTranslationTupleValid(string translationTuple)
        {
            TranslationTupleHelper translatorHelper = new TranslationTupleHelper(translationTuple);
            return translatorHelper.Any();
        }


#if NETFRAMEWORK
        /// <summary>
        /// Updates the translation.
        /// </summary>
        /// <param name="acTypeInfo">The ac type info.</param>
        /// <param name="newTranslationTuple">Structure of TranslationTuple: languageCode1{'translatedText1'}languageCode2{'translatedText2'}..</param>
        public static void UpdateTranslation(IACType acTypeInfo, string newTranslationTuple)
        {
            string updatedTuple = GetUpdatedTranslation(acTypeInfo.ACCaptionTranslation, newTranslationTuple);
            if (updatedTuple != acTypeInfo.ACCaptionTranslation)
                acTypeInfo.ACCaptionTranslation = updatedTuple;
        }

        public static string GetUpdatedTranslation(string prevTranslationTuple, string newTranslationTuple)
        {
            string newTuple = prevTranslationTuple;
            if (string.IsNullOrEmpty(newTranslationTuple))
                return newTuple;
            else if (String.IsNullOrEmpty(prevTranslationTuple))
                return newTranslationTuple;

            if (   !newTranslationTuple.Contains('{') 
                || !newTranslationTuple.Contains('}')
                || !prevTranslationTuple.Contains('{') 
                || !prevTranslationTuple.Contains('}'))
                return newTuple;
            try
            {
                TranslationTupleHelper translatorHelper = new TranslationTupleHelper(prevTranslationTuple);

                string temp = newTranslationTuple;
                string language = "";
                string caption = "";

                int i = 0;
                while (temp.Length > 0)
                {
                    switch (temp.Substring(i, 2))
                    {
                        case "{'":
                            language = temp.Substring(0, i);
                            temp = temp.Substring(i + 2);
                            i = 0;
                            break;
                        case "'}":
                            caption = temp.Substring(0, i);
                            temp = temp.Substring(i + 2);
                            i = 0;

                            string captionOld = "";
                            if (!translatorHelper.TryGetValue(language, out captionOld) || caption != captionOld)
                            {
                                translatorHelper.SetValue(language, caption);
                                newTuple = translatorHelper.CaptionLocalized;
                            }

                            language = "";
                            caption = "";
                            break;
                        default:
                            i++;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Database.Root.Messages.LogException("Translator.UpdateTranslation(): ", "Translationtupel", String.Format("{0}: {1}", e.Message, newTranslationTuple));
                return prevTranslationTuple;
            }
            return newTuple;
        }

        /// <summary>
        /// For classes extends ACValueItemList write in-code (ACValueItem) defined translations into ACClassText
        /// </summary>
        /// <param name="acClass"></param>
        public static void UpdateTranslationACValueItemList(ACClass acClass)
        {
            ACValueItemList aCValueItems = Activator.CreateInstance(acClass.ObjectType, false) as ACValueItemList;
            foreach (ACValueItem aCValueItem in aCValueItems)
            {
                string acIdentifier = aCValueItem.Value.ToString();
                ACClassText aCClassText = acClass.ACClassText_ACClass.FirstOrDefault(c => c.ACIdentifier == acIdentifier);
                if (aCClassText == null)
                {
                    aCClassText = ACClassText.NewACObject(acClass.GetObjectContext<Database>(), acClass);
                    aCClassText.ACIdentifier = acIdentifier;
                    acClass.ACClassText_ACClass.Add(aCClassText);
                }
                string updatedTuple = GetUpdatedTranslation(aCClassText.ACCaptionTranslation, aCValueItem.ACCaptionTranslation);
                if (updatedTuple != aCClassText.ACCaptionTranslation)
                    aCClassText.ACCaptionTranslation = updatedTuple;
            }
        }

        /// <summary>
        /// Get's a translated ACCaption of a Property of a given "Live"-ACUrl
        /// </summary>
        /// <param name="acUrlComponent">The ac URL component.</param>
        /// <param name="acIdentifierProperty">The ac identifier property.</param>
        /// <returns>System.String.</returns>
        public static string GetACPropertyCaption(string acUrlComponent, string acIdentifierProperty)
        {
            return _ACUrlTranslationCache.GetACPropertyCaption(acUrlComponent, acIdentifierProperty, VBLanguageCode);
        }

        /// <summary>
        /// Get's a translated ACCaption of a Property of a given "Live"-ACUrl
        /// </summary>
        /// <param name="acUrlComponent">The ac URL component.</param>
        /// <returns>System.String.</returns>
        public static string GetACComponentCaption(string acUrlComponent)
        {
            return _ACUrlTranslationCache.GetACComponentCaption(acUrlComponent, VBLanguageCode);
        }

        public static string GetACComponentComment(string acUrlComponent)
        {
            return _ACUrlTranslationCache.GetACComponentComment(acUrlComponent, VBLanguageCode);
        }
#endif

        /// <summary>
        /// Class that manages a TranslationTuple
        /// Structure of TranslationTuple: languageCode1{'translatedText1'}languageCode2{'translatedText2'}...
        /// Example:
        /// TranslationTuple:    en{'Group'}de{'Gruppe'}
        /// 1. TranslationPair:  en{'Group'}
        /// LanguageCode:        en
        /// TranslatedText       Group
        /// 2. TranslationPair:  de{'Gruppe'}
        /// LanguageCode:        de
        /// TranslatedText       Gruppe
        /// </summary>
        public class TranslationTupleHelper : Dictionary<string, string>
        {
            /// <summary>
            /// The string separators
            /// </summary>
            public  string[] stringSeparators = new string[] { "{'" };
            /// <summary>
            /// The string separators2
            /// </summary>
            static string[] stringSeparators2 = new string[] { "'}" };

            /// <summary>
            /// Initializes a new instance of the <see cref="TranslationTupleHelper"/> class.
            /// </summary>
            /// <param name="captionLocalized">The caption localized.</param>
            public TranslationTupleHelper(string captionLocalized)
            {
                if (string.IsNullOrEmpty(captionLocalized))
                    return;
                if (!captionLocalized.Contains(stringSeparators[0]) && !captionLocalized.Contains(stringSeparators2[0]))
                    captionLocalized = Translator.VBLanguageCode + stringSeparators[0] + captionLocalized + stringSeparators2[0];
                string[] translationTuple = captionLocalized.Split(stringSeparators2, StringSplitOptions.RemoveEmptyEntries);
                foreach (var tupleObject in translationTuple)
                {
                    string[] translationPair = tupleObject.Split(stringSeparators, StringSplitOptions.None);
                    if (translationPair.Count() == 2)
                    {
                        // translationPair[0] is languageCode, 
                        // translationPair[1] is translatedText
                        this[translationPair[0]] = translationPair[1];
                    }
                }
            }

            /// <summary>
            /// Gets a text under a Language-Code
            /// </summary>
            /// <param name="VBLanguageCode">The md language code.</param>
            /// <returns>System.String.</returns>
            public string GetValue(string VBLanguageCode)
            {
                string translatedText;
                if (TryGetValue(VBLanguageCode, out translatedText))
                    return translatedText;
                if (VBLanguageCode != Translator.VBLanguageCode && TryGetValue(Translator.VBLanguageCode, out translatedText))
                    return translatedText;
                if (Translator.VBLanguageCode != Translator.DefaultVBLanguageCode && TryGetValue(Translator.DefaultVBLanguageCode, out translatedText))
                    return translatedText;
                return translatedText;
            }

            /// <summary>
            /// Enter or update a Text under a LanguageCode
            /// </summary>
            /// <param name="VBLanguageCode">Default Language Code when user is logged on</param>
            /// <param name="value">Entered Text. If Text has a languageCode as Prefix, then store Text under the language Code</param>
            public void SetValue(string VBLanguageCode, string value)
            {
                if (value == null)
                {
                    Remove(VBLanguageCode);
                    return;
                }
                int indexP = value.IndexOf(':');

                // Falls Benutzer übersetzten Text in Eingabefeld mit Ländercode eingegeben hat. z.B. de:Hallo
                if (indexP > 0)
                {
                    string VBLanguageCode1 = value.Substring(0, indexP);
                    string translatedText = value.Substring(indexP + 1);

#if NETFRAMEWORK
                    using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                    {
                        if (Database.GlobalDatabase.VBLanguage.Where(c => c.VBLanguageCode == VBLanguageCode1).Any())
                        {
                            this[VBLanguageCode1] = translatedText;
                            return;
                        }
                    }
#else
                    this[VBLanguageCode1] = translatedText;
                    return;
#endif
                }
                // Sonst wurde Text ohne Ländercode eingegeben
                this[VBLanguageCode] = value;

            }

            /// <summary>
            /// Builds a TranslationTuple as String
            /// </summary>
            /// <value>The caption localized.</value>
            public string CaptionLocalized
            {
                get
                {
                    string captionLocalized = "";

                    foreach (var item in this)
                    {
                        captionLocalized += item.Key + "{'" + item.Value + "'}";
                    }
                    return captionLocalized;
                }
            }
        }

#if NETFRAMEWORK
        /// <summary>
        /// Key is a Database-ACUrl
        /// Value is a TranslationTupleHelper
        /// </summary>
        class ACUrlTranslationCache : Dictionary<string, TranslationTupleHelper>
        {
            /// <summary>
            /// Gets the AC component caption.
            /// </summary>
            /// <param name="acUrlComponent">The ac URL component.</param>
            /// <param name="VBLanguageCode">The md language code.</param>
            /// <returns>System.String.</returns>
            public string GetACComponentCaption(string acUrlComponent, string VBLanguageCode)
            {
                return GetACComponentProperty(acUrlComponent, VBLanguageCode, Subkey.ACCaption);
            }

            public string GetACComponentComment(string acUrlComponent, string VBLanguageCode)
            {
                return GetACComponentProperty(acUrlComponent, VBLanguageCode, Subkey.Comment);
            }

            private enum Subkey
            {
                ACCaption = 0,
                Comment = 1
            }

            private string GetACComponentProperty(string acUrlComponent, string VBLanguageCode, Subkey subkey)
            {
                string acDBUrl = ConvertACUrlComponentToDBUrl(acUrlComponent);
                if (String.IsNullOrEmpty(acDBUrl))
                    return acUrlComponent;
                string acDBUrlWithSubkey = acDBUrl + "\\" + subkey.ToString();
                string translatedText;
                TranslationTupleHelper translTupleHelper;
                if (TryGetValue(acDBUrlWithSubkey, out translTupleHelper))
                {
                    translatedText = translTupleHelper.GetValue(VBLanguageCode);
                    if (!String.IsNullOrEmpty(translatedText))
                        return translatedText;
                }
                IACObjectEntityWithCheckTrans entityObj = Database.GlobalDatabase.ACUrlCommand(acDBUrl, Const.ParamInheritedMember) as IACObjectEntityWithCheckTrans;
                if (entityObj == null)
                    return acUrlComponent;

                string dictKey = String.Format("{0}\\{1}", acDBUrl, Subkey.ACCaption.ToString());
                translTupleHelper = new TranslationTupleHelper(entityObj.ACCaptionTranslation);
                this[dictKey] = translTupleHelper;

                dictKey = String.Format("{0}\\{1}", acDBUrl, Subkey.Comment.ToString());
                translTupleHelper = new TranslationTupleHelper((entityObj as ACClass).Comment);
                this[dictKey] = translTupleHelper;

                if (TryGetValue(acDBUrlWithSubkey, out translTupleHelper))
                {
                    translatedText = translTupleHelper.GetValue(VBLanguageCode);
                    if (!String.IsNullOrEmpty(translatedText))
                        return translatedText;
                }

                return "";
            }


            /// <summary>
            /// Gets the AC property caption.
            /// </summary>
            /// <param name="acUrlComponent">The ac URL component.</param>
            /// <param name="acIdentifierProperty">The ac identifier property.</param>
            /// <param name="VBLanguageCode">The md language code.</param>
            /// <returns>System.String.</returns>
            public string GetACPropertyCaption(string acUrlComponent, string acIdentifierProperty, string VBLanguageCode)
            {
                string acDBUrl = ConvertACUrlComponentToDBUrl(acUrlComponent, acIdentifierProperty);
                if (String.IsNullOrEmpty(acDBUrl))
                    return acIdentifierProperty;
                string translatedText;
                TranslationTupleHelper translTupleHelper;
                if (TryGetValue(acDBUrl, out translTupleHelper))
                {
                    translatedText = translTupleHelper.GetValue(VBLanguageCode);
                    if (!String.IsNullOrEmpty(translatedText))
                        return translatedText;
                }
                IACObjectEntityWithCheckTrans entityObj = Database.GlobalDatabase.ACUrlCommand(acDBUrl, Const.ParamInheritedMember) as IACObjectEntityWithCheckTrans;
                if (entityObj == null)
                    return acIdentifierProperty;
                translTupleHelper = new TranslationTupleHelper(entityObj.ACCaptionTranslation);
                this[acDBUrl] = translTupleHelper;
                translatedText = translTupleHelper.GetValue(VBLanguageCode);
                if (!String.IsNullOrEmpty(translatedText))
                    return translatedText;
                return acIdentifierProperty;
            }

            /// <summary>
            /// Converts a "Live"-ACUrl of a Component with a ACIdentifier of a Property to a Database-Url
            /// </summary>
            /// <param name="acUrlComponent">The ac URL component.</param>
            /// <returns>System.String.</returns>
            public static string ConvertACUrlComponentToDBUrl(string acUrlComponent)
            {
                // TODO: Convert URL of Workflow-Instances
                if (String.IsNullOrEmpty(acUrlComponent))
                    return "";
                //AnwDef: Database\ACProject(BackDef)\ACClass(BackDef)\ACClass(SH)\ACClass(FIVE)\ACClass(CONV)\ACClassProperty(AggrNo)
                //Anw:    Database\ACProject(Back)\ACClass(Back)\ACClass(SH01)\ACClass(FIVE3)\ACClass(CONV)\ACClassProperty(AggrNo)
                ACUrlHelper acUrlHelper = new ACUrlHelper(acUrlComponent);
                if (acUrlHelper.UrlKey != ACUrlHelper.UrlKeys.Root)
                    return "";
                //string dbACUrl = Const.ContextDatabase;
                string dbACUrl = "";
                string nextACUrl = acUrlHelper.NextACUrl;
                if (String.IsNullOrEmpty(nextACUrl))
                    return "";
                int i = 0;
                do
                {
                    acUrlHelper = new ACUrlHelper(nextACUrl);
                    nextACUrl = acUrlHelper.NextACUrl;
                    if (i == 0)
                        dbACUrl += ACProject.ClassName + "(" + acUrlHelper.ACUrlPart + ")\\" + ACClass.ClassName + "(" + acUrlHelper.ACUrlPart + ")";
                    else
                        dbACUrl += "\\" + ACClass.ClassName + "(" + acUrlHelper.ACUrlPart + ")";
                    i++;
                }
                while (!String.IsNullOrEmpty(nextACUrl));
                return dbACUrl;
            }

            /// <summary>
            /// Converts a "Live"-ACUrl of a Component with a ACIdentifier of a Property to a Database-Url
            /// </summary>
            /// <param name="acUrlComponent">The ac URL component.</param>
            /// <param name="acIdentifierProperty">The ac identifier property.</param>
            /// <returns>System.String.</returns>
            public static string ConvertACUrlComponentToDBUrl(string acUrlComponent, string acIdentifierProperty)
            {
                string dbACUrl = ConvertACUrlComponentToDBUrl(acUrlComponent);
                if (String.IsNullOrEmpty(dbACUrl))
                    return dbACUrl;
                dbACUrl += "\\" + ACClassProperty.ClassName + "(" + acIdentifierProperty + ")";
                return dbACUrl;
            }
        }
#endif

    }
}
