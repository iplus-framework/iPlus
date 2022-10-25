using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace gip.core.datamodel
{
    public delegate void ExportErrorDelegate(ExportErrorEventArgs exportErrorEventArgs);
    public delegate void ExportReportProgressDelegate(int currentItem, string progressMessage);

    public class ExportCommand
    {
        public event ExportErrorDelegate ExportErrorEvent;
        public event ExportReportProgressDelegate ExportProgressEvent;

        #region Filters

        public static string PackageExportFolderTemplate = @"{datetime}_{user}";
        public bool IsExportACClass { get; set; }
        public bool IsExportACClassProperty { get; set; }
        public bool IsExportACClassMethod { get; set; }
        public bool IsExportACClassPropertyRelation { get; set; }
        public bool IsExportACClassConfig { get; set; }
        public bool IsExportACClassMessage { get; set; }
        public bool IsExportACClassText { get; set; }
        public bool IsExportACClassDesign { get; set; }
        public bool UseExportFromTime { get; set; }

        public DateTime ExportFromTime { get; set; }
        #endregion

        public ACBackgroundWorker BackgroundWorker { get; set; }
        public DoWorkEventArgs DoWorkEventArgs { get; set; }

        #region project
#if !EFCR
        public string ExportProject(ACEntitySerializer aCEntitySerializer, ACProject aCProject, ACQueryDefinition qryACProject, string rootFolder)
        {
            string folderPath = rootFolder + "\\" + aCProject.ACIdentifier;
            if (!CheckOrCreateDirectory(folderPath))
                return "";

            XElement element = aCEntitySerializer.SerializeACObject(aCProject, qryACProject, folderPath, false);
            string xmlACProject = element != null ? element.ToString() : "";

            WriteAllText(folderPath + "\\" + aCProject.ACIdentifier + Const.ACQueryExportFileType, xmlACProject, aCProject.GetACUrl());

            return folderPath;
        }

        public void DoExport(ACBackgroundWorker worker, DoWorkEventArgs doWorkEventArgs, ACEntitySerializer aCEntitySerializer, ACQueryDefinition qryACProject, ACQueryDefinition qryACClass, ACProject aCProject, ACClassInfoWithItems exportProjectItemRoot, string rootFolder, int currentItem, int totalItems)
        {
            BackgroundWorker = worker;
            DoWorkEventArgs = doWorkEventArgs;
            string folderPath = ExportProject(aCEntitySerializer, aCProject, qryACProject, rootFolder);
            ExportACClass(aCEntitySerializer, qryACClass, exportProjectItemRoot, folderPath, ref currentItem, totalItems);
        }

        public string DoFolder(ACBackgroundWorker worker, DoWorkEventArgs doWorkEventArgs, ACEntitySerializer aCEntitySerializer, ACQueryDefinition qryACProject, ACQueryDefinition qryACClass, ACProject aCProject, ACClassInfoWithItems exportProjectItemRoot, string rootFolder, int currentItem, int totalItems, string userName)
        {
            string subExportFolderName = PackageExportFolderTemplate.Replace("{datetime}", DateTime.Now.ToString("yyyy-MM-dd HH-mm")).Replace("{user}", userName);
            var arhiveDir = Path.Combine(rootFolder, subExportFolderName);
            Directory.CreateDirectory(arhiveDir);
            DoExport(worker, doWorkEventArgs, aCEntitySerializer, qryACProject, qryACClass, aCProject, exportProjectItemRoot, arhiveDir, currentItem, totalItems);
            return subExportFolderName;
        }
#endif
        public void DoPackage(string rootFolder, string subExportFolderName)
        {
            var arhiveDir = Path.Combine(rootFolder, subExportFolderName);
            string destinationArchiveName = rootFolder + @"\" + subExportFolderName + ".zip";
            ZipFile.CreateFromDirectory(arhiveDir, destinationArchiveName, CompressionLevel.Optimal, true, Encoding.UTF8);
            Directory.Delete(arhiveDir, true);
        }

        #endregion

        #region ACClassInfoWithItems - export item
        /// <summary>
        /// Exports the AC class.
        /// </summary>
        /// <param name="aCEntitySerializer">aCEntitySerializer</param>
        /// <param name="projectItem">The project item.</param>
        /// <param name="qryACClass">The qry AC class.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="currentItem"></param>
        /// <param name="totalItems"></param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
#if !EFCR
        public bool ExportACClass(ACEntitySerializer aCEntitySerializer, ACQueryDefinition qryACClass, ACClassInfoWithItems projectItem, string folderPath, ref int currentItem, int totalItems)
        {
            if (BackgroundWorker != null && DoWorkEventArgs != null)
            {
                if (BackgroundWorker.CancellationPending == true)
                {
                    DoWorkEventArgs.Cancel = true;
                    return false;
                }
            }

            if (projectItem.Value == null)
                folderPath += "\\" + projectItem.ACIdentifier;
            else
                folderPath += "\\" + projectItem.ValueT.ACIdentifier;

            if (projectItem.IsChecked && projectItem.ValueT != null)
            {
                ExportACClass(aCEntitySerializer, projectItem.ValueT, qryACClass, folderPath);
                currentItem++;
                string progressMessage = string.Format("Exporting {0} / {1} items...[{2}]", currentItem, totalItems, projectItem.ValueT.ACIdentifier);
                OnExportReportProgress(currentItem, progressMessage);
            }

            bool returnChild = true;
            foreach (var childProjectItem in projectItem.Items)
            {
                // Doing something for cancel
                returnChild = ExportACClass(aCEntitySerializer, qryACClass, childProjectItem as ACClassInfoWithItems, folderPath, ref currentItem, totalItems);
                if (!returnChild)
                    break;
            }

            return returnChild;
        }
#endif        
        #endregion

        #region export class parts

        /// <summary>
        /// Exports the AC class.
        /// </summary>
        /// <param name="aCEntitySerializer"></param>
        /// <param name="aCClass"></param>
        /// <param name="qryACClass"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
#if !EFCR
        public bool ExportACClass(ACEntitySerializer aCEntitySerializer, ACClass aCClass, ACQueryDefinition qryACClass, string folderPath)
        {
            if (!CheckOrCreateDirectory(folderPath))
                return false;


            string acClassTaskName = (string.Format("Export ACClass: {0}", aCClass.ACIdentifier));

            if (IsExportACClass)
            {
                XElement element = aCEntitySerializer.SerializeACObject(aCClass, qryACClass, folderPath, false);
                string xmlACClass = element != null ? element.ToString() : "";

                WriteAllText(folderPath + "\\" + ACClass.ClassName + Const.ACQueryExportFileType, xmlACClass, aCClass.GetACUrl());
            }
            if (IsExportACClassProperty)
            {
                ACQueryDefinition qryACClassProperty = qryACClass.ACUrlCommand(Const.QueryPrefix + ACClassProperty.ClassName) as ACQueryDefinition;
                if (qryACClassProperty != null)
                {
                    ACQueryApplyUseExportFromTime(qryACClassProperty);
                    ExportACClassProperty(aCEntitySerializer, aCClass, qryACClassProperty, folderPath);
                }
            }
            if (IsExportACClassMethod)
            {
                ACQueryDefinition qryACClassMethod = qryACClass.ACUrlCommand(Const.QueryPrefix + ACClassMethod.ClassName) as ACQueryDefinition;
                if (qryACClassMethod != null)
                {
                    ACQueryApplyUseExportFromTime(qryACClassMethod);
                    ExportACClassMethod(aCEntitySerializer, aCClass, qryACClassMethod, folderPath);
                }
            }
            if (IsExportACClassDesign)
            {
                ACQueryDefinition qryACClassDesign = qryACClass.ACUrlCommand(Const.QueryPrefix + ACClassDesign.ClassName) as ACQueryDefinition;
                if (qryACClassDesign != null)
                {
                    ACQueryApplyUseExportFromTime(qryACClassDesign);
                    ExportACClassDesign(aCEntitySerializer, aCClass, qryACClassDesign, folderPath);
                }
            }

            if (IsExportACClassText)
            {
                ACQueryDefinition qryACClassText = qryACClass.ACUrlCommand(Const.QueryPrefix + ACClassText.ClassName) as ACQueryDefinition;
                if (qryACClassText != null)
                {
                    ACQueryApplyUseExportFromTime(qryACClassText);
                    ExportACClassText(aCEntitySerializer, aCClass, qryACClassText, folderPath);
                }
            }

            if (IsExportACClassMessage)
            {
                ACQueryDefinition qryACClassMessage = qryACClass.ACUrlCommand(Const.QueryPrefix + ACClassMessage.ClassName) as ACQueryDefinition;
                if (qryACClassMessage != null)
                {
                    ACQueryApplyUseExportFromTime(qryACClassMessage);
                    ExportACClassMessage(aCEntitySerializer, aCClass, qryACClassMessage, folderPath);
                }
            }

            if (IsExportACClassConfig)
            {
                ACQueryDefinition qryACClassConfig = qryACClass.ACUrlCommand(Const.QueryPrefix + ACClassConfig.ClassName) as ACQueryDefinition;
                if (qryACClassConfig != null)
                {
                    ACQueryApplyUseExportFromTime(qryACClassConfig);
                    ExportACClassConfig(aCEntitySerializer, aCClass, qryACClassConfig, folderPath);
                }
            }

            if (IsExportACClassPropertyRelation)
            {
                ACQueryDefinition qryACClassPropertyRelation = qryACClass.ACUrlCommand(Const.QueryPrefix + ACClassPropertyRelation.ClassName) as ACQueryDefinition;
                if (qryACClassPropertyRelation != null)
                {
                    ACQueryApplyUseExportFromTime(qryACClassPropertyRelation);
                    ExportACClassPropertyRelation(aCEntitySerializer, aCClass, qryACClassPropertyRelation, folderPath);
                }
            }

            return true;
        }

        /// <summary>
        /// Exports Properties of a class
        /// </summary>
        /// <param name="aCEntitySerializer"></param>
        /// <param name="acClass"></param>
        /// <param name="qryACClassProperty"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public bool ExportACClassProperty(ACEntitySerializer aCEntitySerializer, ACClass acClass, ACQueryDefinition qryACClassProperty, string folderPath)
        {
            System.Xml.Linq.XElement element = aCEntitySerializer.Serialize(acClass, qryACClassProperty, folderPath, true);
            string xmlACClassProperty = element != null ? element.ToString() : "";

            string propertyPath = folderPath + "\\" + ACClassProperty.ClassName + Const.ACQueryExportFileType;
            if (string.IsNullOrEmpty(xmlACClassProperty))
            {
                if (IsPathValid(propertyPath) && File.Exists(propertyPath))
                {
                    File.Delete(propertyPath);
                }
            }
            else
            {
                WriteAllText(propertyPath, xmlACClassProperty, acClass.GetACUrl());
            }
            return true;
        }

        /// <summary>
        /// Exports all methods of a class
        /// </summary>
        /// <param name="aCEntitySerializer"></param>
        /// <param name="acClass"></param>
        /// <param name="qryACClassMethod"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public bool ExportACClassMethod(ACEntitySerializer aCEntitySerializer, ACClass acClass, ACQueryDefinition qryACClassMethod, string folderPath)
        {
            // 1. Exportieren aller Assembly-Methoden (MSMethod) einer ACClass in eine Datei
            qryACClassMethod.ClearFilter();
            qryACClassMethod.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, "11110" /*MSMethod*/, false));
            qryACClassMethod.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.and, "11111" /*MSMethodPrePost*/, false));
            System.Xml.Linq.XElement element = aCEntitySerializer.Serialize(acClass, qryACClassMethod, folderPath, true);
            string xmlACClassMethod = element != null ? element.ToString() : "";

            string methodPath = folderPath + "\\" + ACClassMethod.ClassName + Const.ACQueryExportFileType;
            if (string.IsNullOrEmpty(xmlACClassMethod))
            {
                if (IsPathValid(methodPath) && File.Exists(methodPath))
                {
                    File.Delete(methodPath);
                }
            }
            else
            {
                WriteAllText(methodPath, xmlACClassMethod, acClass.GetACUrl());
            }

            // 2. Exportieren aller Script-Methoden (MSMethodExt,MSMethodExtTrigger,MSMethodExtClient) einer ACClass und ACClassMethodMapDesign in getrennten Dateien
            qryACClassMethod.ClearFilter();
            qryACClassMethod.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.parenthesisOpen, null, Global.LogicalOperators.none, Global.Operators.and, null, false));
            qryACClassMethod.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, "11120" /*MSMethodExt       */, false));
            qryACClassMethod.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, "11121" /*MSMethodExtTrigger*/, false));
            qryACClassMethod.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.or, "11122" /*MSMethodExtClient */, false));
            qryACClassMethod.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.parenthesisClose, null, Global.LogicalOperators.none, Global.Operators.none, null, false));

            foreach (var acClassMethod in acClass.ACSelect(qryACClassMethod))
            {
                OnExportReportProgress(0, string.Format(@"Export ACClassMethod: {0}", (acClassMethod as ACClassMethod).ACIdentifier));
                XElement xmlACClassMethod2Element = aCEntitySerializer.SerializeACObject(acClassMethod as ACClassMethod, qryACClassMethod, folderPath, true);
                string xmlACClassMethod2 = xmlACClassMethod2Element != null ? xmlACClassMethod2Element.ToString() : "";
                if (!string.IsNullOrEmpty(xmlACClassMethod2))
                {
                    WriteAllText(folderPath + "\\" + ACClassMethod.ClassName + "_" + (acClassMethod as ACClassMethod).ACIdentifier + Const.ACQueryExportFileType, xmlACClassMethod2, (acClassMethod as IACObject).GetACUrl());
                }
            }

            // 3. Exportieren aller Workflow-Methoden (MSWorkflow) einer ACClass und ACClassMethodMapDesign in getrennten Dateien
            qryACClassMethod.ClearFilter();
            qryACClassMethod.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, Const.ACKindIndex, Global.LogicalOperators.equal, Global.Operators.and, "11130" /*MSWorkflow*/, false));

            foreach (var acClassMethod in acClass.ACSelect(qryACClassMethod))
            {
                XElement xmlACClassMethod3Element = aCEntitySerializer.SerializeACObject(acClassMethod as ACClassMethod, qryACClassMethod, folderPath, true);
                string xmlACClassMethod3 = xmlACClassMethod3Element != null ? xmlACClassMethod3Element.ToString() : "";
                if (!string.IsNullOrEmpty(xmlACClassMethod3))
                {
                    WriteAllText(folderPath + "\\" + ACClassMethod.ClassName + "_" + (acClassMethod as ACClassMethod).ACIdentifier + Const.ACQueryExportFileType, xmlACClassMethod3, (acClassMethod as IACObject).GetACUrl());
                }
            }

            return true;
        }


        /// <summary>
        /// Exports all designs of a class
        /// </summary>
        /// <param name="aCEntitySerializer"></param>
        /// <param name="acClass"></param>
        /// <param name="qryACClassDesign"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public virtual bool ExportACClassDesign(ACEntitySerializer aCEntitySerializer, ACClass acClass, ACQueryDefinition qryACClassDesign, string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath, ACClassDesign.ClassName + "_*" + Const.ACQueryExportFileType);
            foreach (var file in files)
            {
                File.Delete(file);
            }
            foreach (var acClassDesign in acClass.ACSelect(qryACClassDesign))
            {
                OnExportReportProgress(0, string.Format(@"Export ACClassDesign: {0}", (acClassDesign as ACClassDesign).ACIdentifier));
                XElement element = aCEntitySerializer.SerializeACObject(acClassDesign as IACObject, qryACClassDesign, folderPath, false);
                string xmlACClassDesign = element != null ? element.ToString() : "";
                if (!string.IsNullOrEmpty(xmlACClassDesign))
                {
                    WriteAllText(folderPath + "\\" + ACClassDesign.ClassName + "_" + (acClassDesign as IACObject).ACIdentifier + Const.ACQueryExportFileType, xmlACClassDesign, (acClassDesign as IACObject).GetACUrl());
                }
            }
            return true;
        }

        /// <summary>
        /// Exports all Texts of a class
        /// </summary>
        /// <param name="aCEntitySerializer"></param>
        /// <param name="acClass"></param>
        /// <param name="qryACClassText"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public bool ExportACClassText(ACEntitySerializer aCEntitySerializer, ACClass acClass, ACQueryDefinition qryACClassText, string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath, ACClassText.ClassName + "_*" + Const.ACQueryExportFileType);
            foreach (var file in files)
            {
                File.Delete(file);
            }
            foreach (var acClassText in acClass.ACSelect(qryACClassText))
            {
                OnExportReportProgress(0, string.Format(@"Export ACClassText: {0}", (acClassText as ACClassText).ACIdentifier));
                XElement element = aCEntitySerializer.SerializeACObject(acClassText as IACObject, qryACClassText, folderPath, false);
                string xmlACClassText = element != null ? element.ToString() : "";
                if (!string.IsNullOrEmpty(xmlACClassText))
                {
                    WriteAllText(folderPath + "\\" + ACClassText.ClassName + "_" + (acClassText as IACObject).ACIdentifier + Const.ACQueryExportFileType, xmlACClassText, (acClassText as IACObject).GetACUrl());
                }
            }

            return true;
        }

        /// <summary>
        /// Exports all messages of a class
        /// </summary>
        /// <param name="aCEntitySerializer"></param>
        /// <param name="acClass"></param>
        /// <param name="qryACClassMessage"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public bool ExportACClassMessage(ACEntitySerializer aCEntitySerializer, ACClass acClass, ACQueryDefinition qryACClassMessage, string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath, ACClassMessage.ClassName + "_*" + Const.ACQueryExportFileType);
            foreach (var file in files)
            {
                File.Delete(file);
            }
            foreach (var acClassMessage in acClass.ACSelect(qryACClassMessage))
            {
                OnExportReportProgress(0, string.Format(@"Export ACClassMessage: {0}", (acClassMessage as ACClassMessage).ACIdentifier));
                XElement element = aCEntitySerializer.SerializeACObject(acClassMessage as IACObject, qryACClassMessage, folderPath, false);
                string xmlACClassMessage = element != null ? element.ToString() : "";
                if (!string.IsNullOrEmpty(xmlACClassMessage))
                {
                    WriteAllText(folderPath + "\\" + ACClassMessage.ClassName + "_" + (acClassMessage as IACObject).ACIdentifier + Const.ACQueryExportFileType, xmlACClassMessage, (acClassMessage as IACObject).GetACUrl());
                }
            }

            return true;
        }

        /// <summary>
        /// Exports all config-entries of a class
        /// </summary>
        /// <param name="aCEntitySerializer"></param>
        /// <param name="acClass"></param>
        /// <param name="qryACClassComposition"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public bool ExportACClassConfig(ACEntitySerializer aCEntitySerializer, ACClass acClass, ACQueryDefinition qryACClassComposition, string folderPath)
        {
            System.Xml.Linq.XElement element = aCEntitySerializer.Serialize(acClass, qryACClassComposition, folderPath, true);
            string xmlACClassConfig = element != null ? element.ToString() : "";

            string configPaht = folderPath + "\\" + ACClassConfig.ClassName + Const.ACQueryExportFileType;
            if (string.IsNullOrEmpty(xmlACClassConfig))
            {
                if (IsPathValid(configPaht) && File.Exists(configPaht))
                {
                    File.Delete(configPaht);
                }
            }
            else
            {
                WriteAllText(configPaht, xmlACClassConfig, acClass.GetACUrl());
            }
            return true;
        }

        /// <summary>
        /// Exports all relations of a class
        /// </summary>
        /// <param name="aCEntitySerializer"></param>
        /// <param name="acClass"></param>
        /// <param name="qryACClassPropertyRelation"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public bool ExportACClassPropertyRelation(ACEntitySerializer aCEntitySerializer, ACClass acClass, ACQueryDefinition qryACClassPropertyRelation, string folderPath)
        {
            System.Xml.Linq.XElement element = aCEntitySerializer.Serialize(acClass, qryACClassPropertyRelation, folderPath, true);
            string xmlACClassPropertyRelation = element != null ? element.ToString() : "";

            string propertyRelationPath = folderPath + "\\" + ACClassPropertyRelation.ClassName + Const.ACQueryExportFileType;
            if (string.IsNullOrEmpty(xmlACClassPropertyRelation))
            {
                if (File.Exists(propertyRelationPath))
                {
                    File.Delete(propertyRelationPath);
                }
            }
            else
            {
                WriteAllText(propertyRelationPath, xmlACClassPropertyRelation, acClass.GetACUrl());
            }
            return true;
        }
#endif
        #endregion

        #region Helper methods
#if !EFCR
        public void ACQueryApplyUseExportFromTime(ACQueryDefinition queryDefinition)
        {
            ACFilterItem updateDateFilterItem = queryDefinition.ACFilterColumns.FirstOrDefault(x => x.ACIdentifier == Const.EntityUpdateDate);
            if (updateDateFilterItem != null)
            {
                queryDefinition.ACFilterColumns.Remove(updateDateFilterItem);
            }
            if (UseExportFromTime)
            {
                updateDateFilterItem = new ACFilterItem(Global.FilterTypes.filter, Const.EntityUpdateDate, Global.LogicalOperators.greaterThanOrEqual, Global.Operators.and, "", true);
                updateDateFilterItem.SearchWord = ExportFromTime.ToString("o");
                queryDefinition.ACFilterColumns.Add(updateDateFilterItem);
            }
        }
#endif
        public void WriteAllText(string path, string contents, string acUrl)
        {
            if (IsPathValid(path))
            {
                try
                {
                    File.WriteAllText(path, contents);
                }
                catch (Exception ec)
                {
                    OnExportError(ExportErrosEnum.ErrorWriteFile, acUrl, ec, null);
                }
            }
            else
            {
                OnExportError(ExportErrosEnum.MissingPath, "", null, path);
            }
        }
        public bool IsPathValid(string path)
        {
            bool isValid = false;
            try
            {
                Path.GetFullPath(path);
                isValid = true;
            }
            catch (Exception)
            {

            }
            return isValid;
        }

        /// <summary>
        /// Checks the or create directory.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool CheckOrCreateDirectory(string folder)
        {
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch (Exception e)
            {
                OnExportError(ExportErrosEnum.MissingPath, "", e, folder);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Export folder path
        /// </summary>
        /// <param name="currentExportFolder"></param>
        /// <param name="acObject"></param>
        /// <returns></returns>
        public string GetExportFolderPath(string currentExportFolder, IACObject acObject)
        {
            string folderPath = acObject.GetACUrl();
            return currentExportFolder + folderPath.Substring(8);
        }

        /// <summary>
        /// Gets the file name XML.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <returns>System.String.</returns>
        public string GetFileNameXML(IACObject acObject)
        {
            return acObject.ACIdentifier + Const.ACQueryExportFileType;
        }

        #endregion

        #region Event callers

        /// <summary>
        /// Logs errors
        /// </summary>
        /// <param name="exportErrorType"></param>
        /// <param name="acUrl"></param>
        /// <param name="ec"></param>
        /// <param name="path"></param>
        public void OnExportError(ExportErrosEnum exportErrorType, string acUrl, Exception ec, string path)
        {
            if (ExportErrorEvent != null)
                ExportErrorEvent(
                        new ExportErrorEventArgs()
                        {
                            Exception = ec,
                            ExportErrorType = exportErrorType,
                            ACUrl = acUrl,
                            Path = path
                        });
        }

        public void OnExportReportProgress(int currentItem, string progressMessage)
        {
            if (ExportProgressEvent != null)
                ExportProgressEvent(currentItem, progressMessage);
        }

        #endregion
    }
}
