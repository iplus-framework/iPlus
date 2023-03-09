// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACMsgHandler.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;


namespace gip.core.datamodel
{
    /// <summary>
    /// Interface IMessages
    /// </summary>
    public interface IMessages
    {
        #region Dialogs

        /// <summary>Opens a Messagebox with a OK-Button and a Information-Icon.</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        Global.MsgResult Info(IACObject acObject, string acIdentifierOrText, bool ignoreTranslation = false, params object[] parameter);


        /// <summary>Opens a Messagebox with a OK-Button and a Warning-Icon.
        /// Usage: "It could indicate a problem that needs to be fixed."</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        Global.MsgResult Warning(IACObject acObject, string acIdentifierOrText, bool ignoreTranslation = false, params object[] parameter);


        /// <summary>Opens a Messagebox with a Yes + No-Button and a Questionmark-Icon.</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="defaultResult">The default result.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        Global.MsgResult Question(IACObject acObject, string acIdentifierOrText, Global.MsgResult defaultResult = Global.MsgResult.Yes, bool ignoreTranslation = false, params object[] parameter);


        /// <summary>Opens a Messagebox with a Yes + No + Cancel-Button and a Questionmark-Icon.</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="defaultResult">The default result.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        Global.MsgResult YesNoCancel(IACObject acObject, string acIdentifierOrText, Global.MsgResult defaultResult = Global.MsgResult.Yes, bool ignoreTranslation = false, params object[] parameter);


        /// <summary>Opens a Messagebox with a OK-Button and a Error-Icon.
        /// Usage: "An failure occurred that could be successful if you try again."</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        Global.MsgResult Failure(IACObject acObject, string acIdentifierOrText, bool ignoreTranslation = false, params object[] parameter);


        /// <summary>Opens a Messagebox with a OK-Button and a Error-Icon.
        /// Usage: "There is an error that needs to be fixed."</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        Global.MsgResult Error(IACObject acObject, string acIdentifierOrText, bool ignoreTranslation = false, params object[] parameter);


        /// <summary>Opens a Messagebox with a OK-Button and a Exception-Icon.
        /// Usage: "It could indicate a technical problem that may recur and the cause should be investigated."</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        Global.MsgResult Exception(IACObject acObject, string acIdentifierOrText, bool ignoreTranslation = false, params object[] parameter);


        /// <summary>Opens a Messagebox and shows the passed message (Default: With a OK-Button and a Information-Icon.). 
        /// The displayed icon depends on the MessageLevel-Property in msg.</summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="defaultResult">The default result.</param>
        /// <param name="msgButton">The MSG button.</param>
        /// <returns>Global.MsgResult.</returns>
        Global.MsgResult Msg(Msg msg, Global.MsgResult defaultResult = Global.MsgResult.OK, eMsgButton msgButton = eMsgButton.OK);


        /// <summary>Opens a Input-Box for one value.</summary>
        /// <param name="header">Text, that is displayed on the top of the dialog</param>
        /// <param name="value">The default-value in the Input-Field</param>
        /// <param name="designXML">XAML-String for a individual layout of the the Input-Form.</param>
        /// <returns>System.String.</returns>
        string InputBox(String header, string value, string designXML = null);


        /// <summary>Opens a Input-Box for mupltiple values.</summary>
        /// <param name="header">Text, that is displayed on the top of the dialog</param>
        /// <param name="valueList">The default-values in the Input-Field-List</param>
        /// <param name="captionList">List of labels for the input fields.</param>
        /// <param name="designXML">XAML-String for a individual layout of the the Input-Form.</param>
        /// <returns>System.Object[][].</returns>
        object[] InputBoxValues(String header, object[] valueList, string[] captionList, string designXML = null);


        /// <summary>Opens the file dialog.</summary>
        /// <param name="filter">The filter.</param>
        /// <param name="initialDirectory">The initial directory.</param>
        /// <param name="restoreDirectory">if set to <c>true</c> [restore directory].</param>
        /// <returns>System.String.</returns>
        string OpenFileDialog(string filter, string initialDirectory = "", bool restoreDirectory = true);


        /// <summary>Opens a file dialog for saving.</summary>
        /// <param name="filter">The filter.</param>
        /// <param name="initialDirectory">The initial directory.</param>
        /// <param name="restoreDirectory">if set to <c>true</c> [restore directory].</param>
        /// <returns>System.String.</returns>
        string SaveFileDialog(string filter, string initialDirectory = "", bool restoreDirectory = true);


        /// <summary>Opens a file dialog for selecting a directory.</summary>
        /// <param name="initialDirectory">The initial directory.</param>
        /// <returns>System.String.</returns>
        string SelectFolderDialog(string initialDirectory);

        #endregion


        #region Logging

        /// <summary>Writes a Debug-Message to the Logfile. Usage: "Messages that are used to track a problem."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        void LogDebug(string source, string acName, string message);


        /// <summary>Writes a general information to the Logifle.</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        void LogInfo(string source, string acName, string message);


        /// <summary>Writes a Warning-Message to the Logfile. Usage: "It could indicate a problem that needs to be fixed."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        void LogWarning(string source, string acName, string message);


        /// <summary>Writes a Exception-Message to the Logfile. Usage: "It could indicate a technical problem that may recur and the cause should be investigated."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        void LogException(string source, string acName, string message);


        /// <summary>Writes a Exception-Message to the Logfile. Usage: "It could indicate a technical problem that may recur and the cause should be investigated."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="ex">Exception</param>
        /// <param name="withStackTrace">With Stack Trace</param>
        void LogException(string source, string acName, Exception ex, bool withStackTrace = false);


        /// <summary>Writes a Failure-Message to the Logfile. Usage: "An failure occurred that could be successful if you try again."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        void LogFailure(string source, string acName, string message);


        /// <summary>Writes a Error-Message to the Logfile. Usage: "There is an error that needs to be fixed."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        void LogError(string source, string acName, string message);


        /// <summary>Writes a message to the logfile.</summary>
        /// <param name="messageLevel">The message level.</param>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        void LogMessage(eMsgLevel messageLevel, string source, string acName, string message);


        /// <summary>Writes a message to the logfile.</summary>
        /// <param name="message">The message.</param>
        void LogMessageMsg(Msg message);


        /// <summary>
        /// Directory where Logs are written.
        /// </summary>
        /// <value>Directory where Logs are written.</value>
        string LogFilePath { get; }


        #endregion
    }
}
