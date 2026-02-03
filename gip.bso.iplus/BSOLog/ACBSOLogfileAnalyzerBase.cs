// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gip.bso.iplus
{
    /// <summary>
    /// Abstract base class for log file analyzers. Cannot be instantiated and used directly.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Log File Analyzer Base'}de{'Log-Datei-Analysierer Basis'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Description = "Abstract base class for log file analyzers. Cannot be instantiated and used directly.")]
    public abstract class ACBSOLogfileAnalyzerBase : ACBSO
    {
        #region c'tors

        /// <summary>
        /// Creates a new instance of ACBSOLogfileAnalyzerBase.
        /// </summary>
        protected ACBSOLogfileAnalyzerBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        /// <summary>
        /// Initializes this component.
        /// </summary>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        /// <summary>
        /// Deinitializes this component.
        /// </summary>
        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            _LogLines?.Clear();
            _OutputFilePath = null;

            return await base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Properties

        private List<LogfileReaderLine> _LogLines;
        /// <summary>
        /// Gets the log lines to analyze.
        /// </summary>
        protected List<LogfileReaderLine> LogLines
        {
            get => _LogLines;
        }

        private string _OutputFilePath;
        /// <summary>
        /// Gets or sets the output file path.
        /// </summary>
        [ACPropertyInfo(401, "", "en{'Output File Path'}de{'Ausgabedatei-Pfad'}")]
        public string OutputFilePath
        {
            get => _OutputFilePath;
            set
            {
                _OutputFilePath = value;
                OnPropertyChanged("OutputFilePath");
            }
        }

        private int _AnalysisProgress;
        /// <summary>
        /// Gets or sets the analysis progress percentage.
        /// </summary>
        [ACPropertyInfo(402, "", "en{'Analysis Progress'}de{'Analyse-Fortschritt'}")]
        public int AnalysisProgress
        {
            get => _AnalysisProgress;
            set
            {
                _AnalysisProgress = value;
                OnPropertyChanged("AnalysisProgress");
            }
        }

        private bool _IsAnalyzing;
        /// <summary>
        /// Gets or sets whether analysis is currently running.
        /// </summary>
        [ACPropertyInfo(403, "", "en{'Is Analyzing'}de{'Analysiert gerade'}")]
        public bool IsAnalyzing
        {
            get => _IsAnalyzing;
            set
            {
                _IsAnalyzing = value;
                OnPropertyChanged("IsAnalyzing");
            }
        }

        private string _AnalysisResult;
        /// <summary>
        /// Gets or sets the analysis result summary.
        /// </summary>
        [ACPropertyInfo(404, "", "en{'Analysis Result'}de{'Analyse-Ergebnis'}")]
        public string AnalysisResult
        {
            get => _AnalysisResult;
            set
            {
                _AnalysisResult = value;
                OnPropertyChanged("AnalysisResult");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the log lines to be analyzed.
        /// </summary>
        /// <param name="logLines">The log lines to analyze.</param>
        public virtual void SetLogLines(List<LogfileReaderLine> logLines)
        {
            _LogLines = logLines ?? new List<LogfileReaderLine>();
        }

        /// <summary>
        /// Starts the analysis process.
        /// </summary>
        public virtual void StartAnalysis(Action<int, int> progressCallback)
        {
            if (LogLines == null || LogLines.Count == 0)
            {
                Root.Messages.WarningAsync(this, "No log lines to analyze.", true);
                return;
            }

            try
            {
                PerformAnalysis(progressCallback);
                Root.Messages.InfoAsync(this, "Analysis completed successfully.", true);
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "StartAnalysis", ex);
                Root.Messages.ErrorAsync(this, "Error during analysis: " + ex.Message, true);
            }
        }

        /// <summary>
        /// Checks if the Start Analysis command is enabled.
        /// </summary>
        public virtual bool IsEnabledStartAnalysis()
        {
            return !String.IsNullOrEmpty(OutputFilePath);
        }

        /// <summary>
        /// Selects the output file path.
        /// </summary>
        [ACMethodInfo("", "en{'Select Output File'}de{'Ausgabedatei auswählen'}", 402)]
        public virtual void SelectOutputFile()
        {
            try
            {
                string filePath = Root.RootPageWPF.SaveFileDialog(GetOutputFileFilter());
                if (filePath != null)
                {
                    OutputFilePath = filePath;
                }
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "SelectOutputFile", ex);
                Root.Messages.ErrorAsync(this, "Error selecting output file: " + ex.Message, true);
            }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Performs the actual analysis. Must be implemented by derived classes.
        /// </summary>
        protected abstract void PerformAnalysis(Action<int, int> progressCallback);

        /// <summary>
        /// Gets the file filter for the output file dialog.
        /// </summary>
        /// <returns>File filter string.</returns>
        protected abstract string GetOutputFileFilter();

        /// <summary>
        /// Generates a default file name for the output file.
        /// </summary>
        /// <returns>Default file name.</returns>
        protected abstract string GenerateDefaultFileName();

        #endregion

        #region Protected Methods

        /// <summary>
        /// Logs a message during analysis.
        /// </summary>
        /// <param name="message">The message to log.</param>
        protected void LogAnalysisMessage(string message)
        {
            Messages.LogInfo(this.GetACUrl(), "Analysis", message);
        }

        /// <summary>
        /// Ensures the output directory exists.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        protected void EnsureOutputDirectoryExists(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }

        #endregion

        #region HandleExecuteACMethod

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(IsEnabledStartAnalysis):
                    result = IsEnabledStartAnalysis();
                    return true;
                case nameof(SelectOutputFile):
                    SelectOutputFile();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}