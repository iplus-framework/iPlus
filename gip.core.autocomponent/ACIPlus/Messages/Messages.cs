// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.core.datamodel;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using System.Threading;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Manager für Meldungen, Eingabedialoge und Protokollierung
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Message-Manager'}de{'Meldungs-Manager'}", Global.ACKinds.TACMessages, Global.ACStorableTypes.Required, false, false)]
    public class Messages : ACComponent, IMessages
    {
        private List<LogFile> _ListLogFile;
        private List<LoggingType> _ListLoggingType;
        public const string ClassName = "Messages";

        #region c´tors
        public Messages(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ListLogFile = new List<LogFile>();
            _ListLoggingType = new List<LoggingType>();
            _LogFilePath = Path.GetTempPath();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Dialogs

        /// <summary>Opens a Messagebox with a OK-Button and a Information-Icon</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        [ACMethodInfo("Message", "en{'Infomessage'}de{'Infomeldung'}", 9999)]
        public Global.MsgResult Info(IACObject acObject, string acIdentifierOrText, bool ignoreTranslation = false, params object[] parameter)
        {
            return ConvertToMsgAndDisplay(acObject, eMsgButton.OK, eMsgLevel.Info, acIdentifierOrText, Global.MsgResult.OK, ignoreTranslation, parameter);
        }


        /// <summary>Opens a Messagebox with a OK-Button and a Warning-Icon
        /// Usage: "It could indicate a problem that needs to be fixed."</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">
        /// Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.
        /// </param>
        /// <param name="ignoreTranslation">
        /// If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).
        /// </param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        public Global.MsgResult Warning(IACObject acObject, string acIdentifierOrText, bool ignoreTranslation = false, params object[] parameter)
        {
            return ConvertToMsgAndDisplay(acObject, eMsgButton.OK, eMsgLevel.Warning, acIdentifierOrText, Global.MsgResult.OK, ignoreTranslation, parameter);
        }


        /// <summary>Opens a Messagebox with a Yes + No-Button and a Questionmark-Icon.</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="defaultResult">The default result.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        [ACMethodInfo("Message", "en{'Questionmessage'}de{'Fragemeldung'}", 9999)]
        public Global.MsgResult Question(IACObject acObject, string acIdentifierOrText, Global.MsgResult defaultResult = Global.MsgResult.Yes, bool ignoreTranslation = false, params object[] parameter)
        {
            return ConvertToMsgAndDisplay(acObject, eMsgButton.YesNo, eMsgLevel.Question, acIdentifierOrText, defaultResult, ignoreTranslation, parameter);
        }


        /// <summary>Opens a Messagebox with a Yes + No + Cancel-Button and a Questionmark-Icon.</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="defaultResult">The default result.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        [ACMethodInfo("Message", "en{'Yes/No-Mesage'}de{'Ja/Nein-Meldung'}", 9999)]
        public Global.MsgResult YesNoCancel(IACObject acObject, string acIdentifierOrText, Global.MsgResult defaultResult = Global.MsgResult.Yes, bool ignoreTranslation = false, params object[] parameter)
        {
            return ConvertToMsgAndDisplay(acObject, eMsgButton.YesNoCancel, eMsgLevel.Question, acIdentifierOrText, defaultResult, ignoreTranslation, parameter);
        }


        /// <summary>Opens a Messagebox with a OK-Button and a Error-Icon.
        /// Usage: "An failure occurred that could be successful if you try again."</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        [ACMethodInfo("Message", "en{'Failuremessage'}de{'Ausfallmeldung'}", 9999)]
        public Global.MsgResult Failure(IACObject acObject, string acIdentifierOrText, bool ignoreTranslation = false, params object[] parameter)
        {
            return ConvertToMsgAndDisplay(acObject, eMsgButton.OK, eMsgLevel.Failure, acIdentifierOrText, Global.MsgResult.OK, ignoreTranslation, parameter);
        }


        /// <summary>Opens a Messagebox with a OK-Button and a Error-Icon.
        /// Usage: There is an error that needs to be fixed.</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        [ACMethodInfo("Message", "en{'Errormesage'}de{'Fehlermeldung'}", 9999)]
        public Global.MsgResult Error(IACObject acObject, string acIdentifierOrText, bool ignoreTranslation = false, params object[] parameter)
        {
            return ConvertToMsgAndDisplay(acObject, eMsgButton.OK, eMsgLevel.Error, acIdentifierOrText, Global.MsgResult.OK, ignoreTranslation, parameter);
        }


        /// <summary>Opens a Messagebox with a OK-Button and a Exception-Icon.
        /// Usage: "It could indicate a technical problem that may recur and the cause should be investigated."</summary>
        /// <param name="acObject">Reference to a ACComponent that called this method and where the passed acIdentifierOrText should be searched in the class-hierarchy to find the translation.</param>
        /// <param name="acIdentifierOrText">Unique Identifier of a message that should be retrieved form the databasetable ACClassMessage an should be translated to the VBUser-language. I parameter ignoreTranslation is set to false, this parameter will be treated as a text.</param>
        /// <param name="ignoreTranslation">If true, the passed acIdentifierOrText-Parameter will be treated as a text. (No lookup in the translation-table by the Message-ACIdentifer to retrieve a translated text).</param>
        /// <param name="parameter">Parameterlist that is passed to String.Format()-Method to insert the dynamic values in the placeholders of the translated text.</param>
        /// <returns>Global.MsgResult.</returns>
        [ACMethodInfo("Message", "en{'Exceptionmessage'}de{'Ausnahmemeldung'}", 9999)]
        public Global.MsgResult Exception(IACObject acObject, string acIdentifierOrText, bool ignoreTranslation = false, params object[] parameter)
        {
            return ConvertToMsgAndDisplay(acObject, eMsgButton.OK, eMsgLevel.Exception, acIdentifierOrText, Global.MsgResult.OK, ignoreTranslation, parameter);
        }


        private Global.MsgResult ConvertToMsgAndDisplay(IACObject acObject, eMsgButton msgButton, eMsgLevel msgLevel, string acIdentifierOrText, Global.MsgResult defaultResult, bool ignoreTranslation = false, params object[] parameter)
        {
            Msg msg = new Msg { ACIdentifier = acIdentifierOrText, Message = ignoreTranslation ? acIdentifierOrText : Root.Environment.TranslateMessage(acObject, acIdentifierOrText, parameter), MessageLevel = msgLevel };
            return Msg(msg, defaultResult, msgButton);
        }


        /// <summary>Opens a Messagebox and shows the passed message (Default: With a OK-Button and a Information-Icon.). 
        /// The displayed icon depends on the MessageLevel-Property in msg.</summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="defaultResult">The default result.</param>
        /// <param name="msgButton">The MSG button.</param>
        /// <returns>Global.MsgResult.</returns>
        [ACMethodInfo("Message", "en{'Message'}de{'Meldung'}", 9999)]
        public Global.MsgResult Msg(Msg msg, Global.MsgResult defaultResult = Global.MsgResult.OK, eMsgButton msgButton = eMsgButton.OK)
        {
            Global.MsgResult msgResult = defaultResult;
            if (Root.RootPageWPF != null)
                msgResult = Root.RootPageWPF.ShowMsgBox(msg, msgButton);

            Messages.LogMessage(msg.MessageLevel, "MsgHandler", msg.ACIdentifier, string.Format("{0}\r\nResult:{1}", msg.Message, msgResult.ToString()));
            return msgResult;
        }


        /// <summary>Opens a Input-Box for one value.</summary>
        /// <param name="header">Text, that is displayed on the top of the dialog</param>
        /// <param name="value">The default-value in the Input-Field</param>
        /// <param name="designXML">XAML-String for a individual layout of the the Input-Form.</param>
        /// <returns>System.String.</returns>
        [ACMethodInfo("Input", "en{'String-Inputbox'}de{'Zeichen-Eingabebox'}", 9999)]
        public string InputBox(String header, string value, string designXML = null)
        {
            if (Root.RootPageWPF != null)
            {
                // TODO: Aufruf wird ersetzt durch BSO mit generischem Layout
                IACComponent acComponent = Root.ACUrlCommand("Businessobjects#VBBSOInputBox") as IACComponent;
                var result = acComponent.ACUrlCommand("!ShowInputBox", new object[] { header, value, designXML }) as string;
                acComponent.Stop();
                acComponent = null;
                return result;
            }
            // Falls kein WPF-Client, dann wird Originalwert zurückgegeben
            return value;
        }


        /// <summary>Opens a Input-Box for mupltiple values.</summary>
        /// <param name="header">Text, that is displayed on the top of the dialog</param>
        /// <param name="valueList">The default-values in the Input-Field-List</param>
        /// <param name="captionList">List of labels for the input fields.</param>
        /// <param name="designXML">XAML-String for a individual layout of the the Input-Form.</param>
        /// <returns>System.Object[][].</returns>
        [ACMethodInfo("Input", "en{'Inputbox'}de{'Eingabebox'}", 9999)]
        public object[] InputBoxValues(String header, object[] valueList, string[] captionList, string designXML = null)
        {
            IACComponent acComponent = Root.ACUrlCommand("Businessobjects#VBBSOInputBox") as IACComponent;
            var result = acComponent.ACUrlCommand("!ShowInputBoxValues", new object[] { header, valueList, captionList, designXML }) as object[];
            acComponent.Stop();
            acComponent = null;
            return result;
        }

        #endregion

        #region Logging

        /// <summary>Writes a Debug-Message to the Logfile. Usage: "Messages that are used to track a problem."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        [ACMethodInfo("Logging", "en{'Log-Debug'}de{'Log-Debug'}", 9999)]
        public void LogDebug(string source, string acName, string message)
        {
            LogMessage(eMsgLevel.Debug, source, acName, message);
        }


        /// <summary>Writes a general information to the Logifle.</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        [ACMethodInfo("Logging", "en{'Log-Info'}de{'Log-Info'}", 9999)]
        public void LogInfo(string source, string acName, string message)
        {
            LogMessage(eMsgLevel.Info, source, acName, message);
        }


        /// <summary>Writes a Warning-Message to the Logfile. Usage: "It could indicate a problem that needs to be fixed."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        [ACMethodInfo("Logging", "en{'Log-Warning'}de{'Log-Warnung'}", 9999)]
        public void LogWarning(string source, string acName, string message)
        {
            LogMessage(eMsgLevel.Warning, source, acName, message);
        }


        /// <summary>Writes a Failure-Message to the Logfile. Usage: "An failure occurred that could be successful if you try again."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        [ACMethodInfo("Logging", "en{'Log-Failure'}de{'Log-Ausfall'}", 9999)]
        public void LogFailure(string source, string acName, string message)
        {
            LogMessage(eMsgLevel.Failure, source, acName, message);
        }


        /// <summary>Writes a Error-Message to the Logfile. Usage: "There is an error that needs to be fixed."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        [ACMethodInfo("Logging", "en{'Log-Error'}de{'Log-Fehler'}", 9999)]
        public void LogError(string source, string acName, string message)
        {
            LogMessage(eMsgLevel.Error, source, acName, message);
        }


        /// <summary>Writes a Exception-Message to the Logfile. Usage: "It could indicate a technical problem that may recur and the cause should be investigated."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        [ACMethodInfo("Logging", "en{'Log-Exception'}de{'Log-Ausnahme'}", 9999)]
        public void LogException(string source, string acName, string message)
        {
            LogMessage(eMsgLevel.Exception, source, acName, message);
        }


        /// <summary>Writes a Exception-Message to the Logfile. Usage: "It could indicate a technical problem that may recur and the cause should be investigated."</summary>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="ex">Exception</param>
        /// <param name="withStackTrace">With Stack Trace</param>
        public void LogException(string source, string acName, Exception ex, bool withStackTrace = false)
        {
            StringBuilder sb = new StringBuilder();
            Exception tmpEc = ex;
            while (tmpEc != null)
            {
                sb.AppendLine(tmpEc.Message);
                tmpEc = tmpEc.InnerException;
            }
            if (withStackTrace)
                sb.AppendLine(ex.StackTrace);
            LogMessage(eMsgLevel.Exception, source, acName, sb.ToString());
        }


        [ACMethodInfo("Console", "en{'Log-Message'}de{'Log-Meldung'}", 9999)]
        public static void ConsoleMsg(string source, string message)
        {
            System.Console.WriteLine(message);
            GlobalMsg.AddDetailMessage(new Msg() { Source = source, Message = message, MessageLevel = eMsgLevel.Info });
        }

        [ACMethodInfo("Console", "en{'Clear Console'}de{'Console leeren'}", 9999)]
        public static void ConsoleClear()
        {
            GlobalMsg.ClearMsgDetails();
        }


        /// <summary>Writes a message to the logfile.</summary>
        /// <param name="messageLevel">The message level.</param>
        /// <param name="source">Information about the source where this method was called. Always pass the ACUrl of a ACComponent.</param>
        /// <param name="acName">Unique information about the position in the code where this method was called. Recommendation: "Methodname(Number)"</param>
        /// <param name="message">The message.</param>
        [ACMethodInfo("Logging", "en{'Log-Message'}de{'Log-Meldung'}", 9999)]
        public void LogMessage(eMsgLevel messageLevel, string source, string acName, string message)
        {
            // Abfangen, wenn Exception bei LogMessage auftaucht
            try
            {
                string messagetext = String.Format("{0:yyyy-MM-dd-HH:mm:ss.ffff} {1} {2} {3} {4}", DateTime.Now, messageLevel, source, acName, message);

                foreach (LoggingType loggingType in _ListLoggingType)
                {
                    if ((loggingType.MessageLevel == messageLevel
                             || loggingType.MessageLevel == eMsgLevel.Default)
                        && (loggingType.Source == "default"
                             || loggingType.Source == "*"
                             || loggingType.Source == source
                             || (!String.IsNullOrEmpty(loggingType.ACName) && loggingType.ACName == acName)
                           )
                        )
                    {
                        foreach (LogFile logFile in _ListLogFile)
                        {
                            if (logFile.FileType == loggingType.FileType)
                            {
                                logFile.Write(messagetext, loggingType.DumpThreadID);
                                if ((loggingType.Source == source || (!String.IsNullOrEmpty(loggingType.ACName) && loggingType.ACName == acName))
                                    && loggingType.SmtpServerHost != null && !String.IsNullOrEmpty(loggingType.SmtpReceipients) && !String.IsNullOrEmpty(loggingType.SmtpFrom))
                                {
                                    try
                                    {
                                        using (SmtpClient client = new SmtpClient(loggingType.SmtpServerHost))
                                        {
                                            if (loggingType.SmtpServerPort > 0 && loggingType.SmtpServerPort != 25)
                                                client.Port = loggingType.SmtpServerPort;
                                            if (loggingType.SmtpUseSSL)
                                                client.EnableSsl = true;
                                            else
                                                client.EnableSsl = false;
                                            if (!String.IsNullOrEmpty(loggingType.SmtpAuthUser) && !String.IsNullOrEmpty(loggingType.SmtpAuthPassword))
                                                client.Credentials = new NetworkCredential(loggingType.SmtpAuthUser, loggingType.SmtpAuthPassword);
                                            client.Send(loggingType.SmtpFrom, loggingType.SmtpReceipients, GetACUrl(), messagetext);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        string msg = e.Message;
                                        if (e.InnerException != null && e.InnerException.Message != null)
                                            msg += " Inner:" + e.InnerException.Message;
                                        msg += System.Environment.NewLine + "Source log message:" + message;

                                        LogToWindowsEvent(msg, 10000);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string msg = "Messages logging exception: " + e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;
                msg += System.Environment.NewLine + "Source log message:" + message;

                LogToWindowsEvent(msg, 10010);

                // Ignorieren, wird in diesem Sonderfall eben nicht geloggt
                return;
            }
        }


        /// <summary>Writes a message to the logfile.</summary>
        /// <param name="msg"></param>
        [ACMethodInfo("Logging", "en{'Log-Message'}de{'Log-Meldung'}", 9999)]
        public void LogMessageMsg(Msg msg)
        {
            LogMessage(msg.MessageLevel, msg.Source, msg.ACIdentifier, String.Format("{0} [{1},{2}]", msg.Message, msg.Row, msg.Column));
            if (msg is MsgWithDetails)
            {
                MsgWithDetails msgDet = msg as MsgWithDetails;
                if (msgDet.MsgDetailsCount > 0)
                {
                    foreach (var detailMsg in msgDet.MsgDetails)
                    {
                        LogMessageMsg(detailMsg);
                    }
                }
            }
        }

        public List<LogFile> LogFileDefinitions
        {
            get
            {
                return _ListLogFile;
            }
        }

        private string _LogFilePath;
        /// <summary>
        /// Directory where Logs are written.
        /// </summary>
        /// <value>Directory where Logs are written.</value>
        public string LogFilePath
        {
            get
            {
                return _LogFilePath;
            }
        }

        public void AddLogFile(string fileType, string fileName, int maxSizeMB, int archiveAfterDays = 0)
        {
            _ListLogFile.Add(new LogFile(fileType, fileName, maxSizeMB, LogFilePath, archiveAfterDays));
        }

        public void AddLoggingType(string fileType, string messageType, string source, bool dumpThreadID, string acName = "", string smtp = "")
        {
            eMsgLevel messageLevel = eMsgLevel.Default;
            if (!Enum.TryParse<eMsgLevel>(messageType, out messageLevel))
                messageLevel = eMsgLevel.Default;
            _ListLoggingType.Add(new LoggingType(messageLevel, source, fileType, dumpThreadID, acName, smtp));
        }
        #endregion


        #region Global Messages
        private static MsgWithDetails _GlobalMsg;
        public static MsgWithDetails GlobalMsg
        {
            get
            {
                if (_GlobalMsg == null)
                {
                    _GlobalMsg = new MsgWithDetails();
                    _GlobalMsg.AddDetailMessage(new Msg() { });
                }
                return _GlobalMsg;
            }
        }
        #endregion


        #region Internal classes
        public class LogFile
        {
            private string _FileType;
            private string _FileName;
            private long _MaxLength;
            private int _MaxSizeMB;
            private int _ArchiveAfterDays;
            private string _RootPath;
            private StreamWriter _StreamWriter;
            private bool _HasDate;
            private bool _HasProcessId;
            private DateTime _Today;
            private int _FileCounter;
            public readonly ACMonitorObject _9000_WriteFileLock = new ACMonitorObject(9000);

            public LogFile(string fileType, string fileName, int maxSizeMB, string rootPath, int archiveAfterDays = 0)
            {
                _FileType = fileType;
                _FileName = fileName;
                _MaxLength = ((long)maxSizeMB) * 1024 * 1024;
                _MaxSizeMB = maxSizeMB;
                _RootPath = rootPath;
                _HasDate = _FileName.IndexOf("%Date%") > 0;
                _HasProcessId = _FileName.IndexOf("%ProcessId%") > 0;
                _ArchiveAfterDays = archiveAfterDays;

                Open();
            }

            private void Open()
            {
                if (_StreamWriter != null)
                    _StreamWriter.Close();

                CheckLogFileAndZip(_RootPath, _FileName);

                string fileNamePath = _FileName;
                if (_HasDate)
                {
                    _Today = DateTime.Today;
                    string date = _Today.ToString("yyyyMMdd");
                    fileNamePath = fileNamePath.Replace("%Date%", date);
                }
                if (_HasProcessId)
                {
                    Process thisProcess = Process.GetCurrentProcess();
                    fileNamePath = fileNamePath.Replace("%ProcessId%", thisProcess.Id.ToString());
                }

                //Prüfen ob schon mehrere Log-Dateien vorhanden sind. Wenn ja, dann letzte öffnen.
                //if (_FileCounter == 0)
                //{
                string testFileName = "";
                do
                {
                    testFileName = _RootPath + fileNamePath + "." + _FileCounter + ".txt";
                    if (File.Exists(testFileName) && !(new FileInfo(testFileName)).IsReadOnly)
                    {
                        try
                        {
                            _StreamWriter = new StreamWriter(testFileName, true);
                            _StreamWriter.Close();
                            testFileName = "";
                            break;
                        }
                        catch (Exception e)
                        {
                            _FileCounter++;
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            LogToWindowsEvent(msg, 10030);
                        }
                    }
                    else
                    {
                        testFileName = "";
                        break;
                    }
                }
                while (testFileName != "");
                //}

                //if (_FileCounter != 0)
                //{
                fileNamePath = fileNamePath + "." + _FileCounter + ".txt";
                //}

                _StreamWriter = new StreamWriter(_RootPath + fileNamePath, true);
            }

            private void CheckLogFileAndZip(string logFilesDir, string fileName)
            {
                if (_ArchiveAfterDays <= 0)
                    return;

                var fileNameSplitted = fileName.Split('%');
                string fileNamePattern = fileNameSplitted.FirstOrDefault();
                string endFileName = fileNameSplitted.LastOrDefault();

                foreach (string logFile in Directory.EnumerateFiles(logFilesDir, fileNamePattern + "*"))
                {
                    string fileNameExtracted = logFile.Split('\\').Last();
                    if (!fileNameExtracted.EndsWith(".zip"))
                    {
                        string dateTimeString = fileNameExtracted.Split('.').FirstOrDefault().TrimStart(fileNamePattern.ToCharArray()).TrimEnd(endFileName.Split('.').FirstOrDefault().ToCharArray());
                        DateTime dateTime = DateTime.ParseExact(dateTimeString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        if ((DateTime.Now - dateTime).Days > _ArchiveAfterDays)
                        {
                            using (FileStream fs = new FileStream(logFilesDir + "\\" + fileNamePattern + dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0') + endFileName + ".zip", FileMode.OpenOrCreate))
                            {
                                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
                                {
                                    zip.CreateEntryFromFile(logFile, fileNameExtracted);
                                    File.Delete(logFile);
                                }
                            }
                        }
                    }
                }

            }

            public void Write(string messageText, bool dumpThreadID = false)
            {
                using (ACMonitor.Lock(_9000_WriteFileLock))
                {
                    //Datum prüfen
                    if (_HasDate && DateTime.Today != _Today)
                    {
                        _FileCounter = 0;
                        Open();
                    }
                    else if (((FileStream)_StreamWriter.BaseStream).Length + messageText.Length > _MaxLength)
                    {
                        _FileCounter++;
                        Open();
                    }

                    if (dumpThreadID)
                        _StreamWriter.WriteLine(messageText + String.Format(" TID:{0}", Thread.CurrentThread.ManagedThreadId));
                    else
                        _StreamWriter.WriteLine(messageText);
                    _StreamWriter.Flush();
                }
            }

            #region ILogFileDefinition Members

            public string FileType
            {
                get { return _FileType; }
            }

            public string FileName
            {
                get { return _FileName; }
            }

            public int MaxSizeMB
            {
                get { return _MaxSizeMB; }
            }

            #endregion
        };

        private class LoggingType
        {
            public LoggingType(eMsgLevel messageLevel, string source, string fileType, bool dumpThreadID, string acName = "", string smtp = "")
            {
                MessageLevel = messageLevel;
                Source = source;
                ACName = acName;
                FileType = fileType;
                DumpThreadID = dumpThreadID;
                _Smtp = null;
                if (!String.IsNullOrEmpty(smtp))
                    _Smtp = smtp.Split(';');
            }

            public eMsgLevel MessageLevel
            {
                get;
                private set;
            }

            public string Source
            {
                get;
                private set;
            }

            public string ACName
            {
                get;
                private set;
            }

            public string FileType
            {
                get;
                private set;
            }

            public bool DumpThreadID
            {
                get;
                private set;
            }

            private string[] _Smtp;

            public String SmtpServerHost
            {
                get
                {
                    return GetSmtpValue("SmtpServerHost");
                }
            }

            public int SmtpServerPort
            {
                get
                {
                    string value = GetSmtpValue("SmtpServerPort");
                    if (String.IsNullOrEmpty(value))
                        return 25;
                    return Convert.ToInt32(value);
                }
            }

            public Boolean SmtpUseSSL
            {
                get
                {
                    string value = GetSmtpValue("SmtpUseSSL");
                    if (String.IsNullOrEmpty(value))
                        return false;
                    return Convert.ToBoolean(value);
                }
            }

            public Boolean IgnoreInvalidCertificate
            {
                get
                {
                    string value = GetSmtpValue("IgnoreInvalidCertificate");
                    if (String.IsNullOrEmpty(value))
                        return false;
                    return Convert.ToBoolean(value);
                }
            }

            public String SmtpAuthUser
            {
                get
                {
                    return GetSmtpValue("SmtpAuthUser");
                }
            }

            public String SmtpAuthPassword
            {
                get
                {
                    return GetSmtpValue("SmtpAuthPassword");
                }
            }

            public String SmtpFrom
            {
                get
                {
                    return GetSmtpValue("SmtpFrom");
                }
            }

            public String SmtpReceipients
            {
                get
                {
                    return GetSmtpValue("SmtpReceipients");
                }
            }

            private string GetSmtpValue(string key)
            {
                if (_Smtp == null || !_Smtp.Any())
                    return null;
                string value = _Smtp.Where(c => c.StartsWith(key)).FirstOrDefault();
                if (String.IsNullOrEmpty(value))
                    return null;
                string[] tuple = value.Split('=');
                if (tuple == null || tuple.Count() < 2)
                    return null;
                return tuple[1];
            }
        }
        #endregion


        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Info":
                    result = Info((IACObject)acParameter[0], (String)acParameter[1], acParameter.Count() == 3 ? (Boolean)acParameter[2] : false, (Object[])acParameter[3]);
                    return true;
                case "Warning":
                    result = Warning((IACObject)acParameter[0], (String)acParameter[1], acParameter.Count() == 3 ? (Boolean)acParameter[2] : false, (Object[])acParameter[3]);
                    return true;
                case "Question":
                    result = Question((IACObject)acParameter[0], (String)acParameter[1], acParameter.Count() == 3 ? (Global.MsgResult)acParameter[2] : Global.MsgResult.Yes, acParameter.Count() == 4 ? (Boolean)acParameter[3] : false, (Object[])acParameter[4]);
                    return true;
                case "YesNoCancel":
                    result = YesNoCancel((IACObject)acParameter[0], (String)acParameter[1], acParameter.Count() == 3 ? (Global.MsgResult)acParameter[2] : Global.MsgResult.Yes, acParameter.Count() == 4 ? (Boolean)acParameter[3] : false, (Object[])acParameter[4]);
                    return true;
                case "Failure":
                    result = Failure((IACObject)acParameter[0], (String)acParameter[1], acParameter.Count() == 3 ? (Boolean)acParameter[2] : false, (Object[])acParameter[3]);
                    return true;
                case "Error":
                    result = Error((IACObject)acParameter[0], (String)acParameter[1], acParameter.Count() == 3 ? (Boolean)acParameter[2] : false, (Object[])acParameter[3]);
                    return true;
                case "Exception":
                    result = Exception((IACObject)acParameter[0], (String)acParameter[1], acParameter.Count() == 3 ? (Boolean)acParameter[2] : false, (Object[])acParameter[3]);
                    return true;
                case "Msg":
                    result = Msg((Msg)acParameter[0], acParameter.Count() == 2 ? (Global.MsgResult)acParameter[1] : Global.MsgResult.OK, acParameter.Count() == 3 ? (eMsgButton)acParameter[2] : eMsgButton.OK);
                    return true;
                case "InputBox":
                    result = InputBox((String)acParameter[0], (String)acParameter[1], acParameter.Count() == 3 ? (String)acParameter[2] : null);
                    return true;
                case "InputBoxValues":
                    result = InputBoxValues((String)acParameter[0], (Object[])acParameter[1], (String[])acParameter[2], acParameter.Count() == 4 ? (String)acParameter[3] : null);
                    return true;
                case "LogDebug":
                    LogDebug((String)acParameter[0], (String)acParameter[1], (String)acParameter[2]);
                    return true;
                case "LogInfo":
                    LogInfo((String)acParameter[0], (String)acParameter[1], (String)acParameter[2]);
                    return true;
                case "LogWarning":
                    LogWarning((String)acParameter[0], (String)acParameter[1], (String)acParameter[2]);
                    return true;
                case "LogFailure":
                    LogFailure((String)acParameter[0], (String)acParameter[1], (String)acParameter[2]);
                    return true;
                case "LogError":
                    LogError((String)acParameter[0], (String)acParameter[1], (String)acParameter[2]);
                    return true;
                case "LogException":
                    LogException((String)acParameter[0], (String)acParameter[1], (String)acParameter[2]);
                    return true;
                case "LogMessage":
                    LogMessage((eMsgLevel)acParameter[0], (String)acParameter[1], (String)acParameter[2], (String)acParameter[3]);
                    return true;
                case "LogMessageMsg":
                    LogMessageMsg((Msg)acParameter[0]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


        public void LoadLoggingConfiguration()
        {
            LoggingConfiguration loggingConfiguration =
                    (LoggingConfiguration)CommandLineHelper.ConfigCurrentDir.GetSection("Logging/LoggingConfiguration");
            if(loggingConfiguration != null)
            {
                _LogFilePath = loggingConfiguration.LogFilePath;
                if (!_LogFilePath.EndsWith("\\"))
                    _LogFilePath += "\\";
                if (loggingConfiguration != null)
                {
                    foreach (LogFileElement files in loggingConfiguration.LogFiles)
                    {
                        AddLogFile(files.FileType, files.FileName, files.MaxSizeMB, files.ArchiveAfterDays);
                    }
                    foreach (LoggingTypeElement types in loggingConfiguration.LoggingTypes)
                    {
                        AddLoggingType(types.FileType, types.MessageType, types.Source, types.DumpThreadID, types.ACName, types.Smtp);
                    }
                }
            }
        }

        /// <summary>
        /// Logs message to Windows event log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="eventID">The event ID parameter.</param>
        public static void LogToWindowsEvent(string message, int eventID)
        {
            try
            {
#pragma warning disable CA1416
                if (!EventLog.SourceExists("iPlus"))
                    EventLog.CreateEventSource("iPlus", "Application");
                EventLog.WriteEntry("iPlus", message, EventLogEntryType.Error, eventID);
#pragma warning restore CA1416
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(message);
            }
        }
    }
}
