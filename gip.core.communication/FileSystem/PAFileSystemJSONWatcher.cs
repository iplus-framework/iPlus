using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace gip.core.communication
{
    /// <summary>
    /// File-System-Watcher for JSON Files
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'JSON Watcher'}de{'JSON Überwacher'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class PAFileSystemJSONWatcher : PAFileSystemWatcherBase
    {
        #region c´tors
        public PAFileSystemJSONWatcher(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            _DelegateQueue = new ACDelegateQueue(this.GetACUrl());
            _DelegateQueue.StartWorkerThread();
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_DelegateQueue != null)
            {
                _DelegateQueue.StopWorkerThread();
                _DelegateQueue = null;
            }
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion

        #region Properties
        [ACPropertyInfo(true, 300, "Configuration", "en{'DeleteUnknownFile'}de{'DeleteUnknownFile'}", "", true)]
        [DefaultValue(true)]
        public bool DeleteUnknownFile
        {
            get;
            set;
        }

        private ACDelegateQueue _DelegateQueue = null;
        public ACDelegateQueue DelegateQueue
        {
            get
            {
                return _DelegateQueue;
            }
        }
        #endregion

        #region Methods

        #region Overridden Methods
        protected override bool AnalyzeContentBeforeRaising(ACEventArgs eventArgs)
        {
            WatcherChangeTypes watcherChangeType = (WatcherChangeTypes)eventArgs["WatcherChangeType"];
            string rootPropertyName = "";
            bool importerFound = false;
            JsonElement rootElement = default;
            JsonDocument jsonDocument = null;
            JToken newtonsoftToken = null;
            string fileName = eventArgs["FullPath"] as string;

            if (watcherChangeType != WatcherChangeTypes.Deleted)
            {
                try
                {
                    string jsonContent = File.ReadAllText(fileName, Encoding.UTF8);
                    
                    // Try System.Text.Json first
                    try
                    {
                        jsonDocument = JsonDocument.Parse(jsonContent);
                        rootElement = jsonDocument.RootElement;

                        // Determine the root property name for JSON object identification
                        if (rootElement.ValueKind == JsonValueKind.Object)
                        {
                            // Get the first property name as identifier
                            var firstProperty = rootElement.EnumerateObject().FirstOrDefault();
                            if (firstProperty.Name != null)
                            {
                                rootPropertyName = firstProperty.Name;
                            }
                            else
                            {
                                // If no properties, use the object type name as fallback
                                rootPropertyName = "JsonObject";
                            }
                        }
                        else if (rootElement.ValueKind == JsonValueKind.Array)
                        {
                            // For arrays, check if we can get type info from first element
                            var firstElement = rootElement.EnumerateArray().FirstOrDefault();
                            if (firstElement.ValueKind == JsonValueKind.Object)
                            {
                                var firstProperty = firstElement.EnumerateObject().FirstOrDefault();
                                rootPropertyName = firstProperty.Name ?? "JsonArray";
                            }
                            else
                            {
                                rootPropertyName = "JsonArray";
                            }
                        }
                        else
                        {
                            rootPropertyName = rootElement.ValueKind.ToString();
                        }
                    }
                    catch (JsonException)
                    {
                        // Fallback to Newtonsoft.Json if System.Text.Json fails
                        newtonsoftToken = JToken.Parse(jsonContent);
                        
                        if (newtonsoftToken is JObject jObject)
                        {
                            var firstProperty = jObject.Properties().FirstOrDefault();
                            rootPropertyName = firstProperty?.Name ?? "JsonObject";
                        }
                        else if (newtonsoftToken is JArray jArray && jArray.Count > 0)
                        {
                            if (jArray[0] is JObject firstElement)
                            {
                                var firstProperty = firstElement.Properties().FirstOrDefault();
                                rootPropertyName = firstProperty?.Name ?? "JsonArray";
                            }
                            else
                            {
                                rootPropertyName = "JsonArray";
                            }
                        }
                        else
                        {
                            rootPropertyName = newtonsoftToken.Type.ToString();
                        }
                        
                        // Create a dummy JsonElement for compatibility (since we're using Newtonsoft)
                        rootElement = default;
                    }

                    eventArgs.GetACValue("FileContentInfo").Value = rootPropertyName;

                    if (!String.IsNullOrEmpty(rootPropertyName))
                    {
                        foreach (PAJSONDocImporterBase importer in FindChildComponents<PAJSONDocImporterBase>(c => c is PAJSONDocImporterBase, null, 2))
                        {
                            if (importer.IsImporterForJSONDocType(eventArgs, rootElement))
                            {
                                importerFound = true;
                                Messages.LogDebug("PAFileSystemJSONWatcher.AnalyzeContentBeforeRaising(): ", "Importer Found", fileName);
                                _FilesInProcess.Add(fileName);
                                DelegateQueue.Add(
                                    delegate()
                                    {
                                        importer.DoImportAndArchive(eventArgs);
                                    }
                                    );
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Messages.LogException("PAFileSystemJSONWatcher.AnalyzeContentBeforeRaising(): ", "JSON Parse Error", e.Message);
                }
                finally
                {
                    if (jsonDocument != null)
                    {
                        jsonDocument.Dispose();
                        jsonDocument = null;
                    }
                }
            }

            if (!importerFound)
            {
                if (!String.IsNullOrWhiteSpace(ForwardDir))
                    ForwardFile(fileName, ForwardDir);

                Messages.LogDebug("PAFileSystemJSONWatcher.AnalyzeContentBeforeRaising(): ", "Importer Not Found", fileName);
                if (!DeleteUnknownFile)
                {
                    // Verschiebe in Mülleimer
                    string movePath = FindAndCreateTrashPath(fileName);
                    MoveOrDeleteFile(true, movePath, fileName);
                }
                else
                {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception e)
                    {
                        Messages.LogException("PAFileSystemJSONWatcher.AnalyzeContentBeforeRaising(): ", "Delete File", e.Message);
                    }
                }
                return false;
            }
            return true;
        }
        #endregion

        #endregion
    }
}