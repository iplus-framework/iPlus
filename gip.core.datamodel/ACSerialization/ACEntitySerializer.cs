// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACEntitySerializer.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Collections;
using System.Runtime.Serialization;
using gip.core.datamodel;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel
{
    /// <summary>
    /// Serialisierung von Entitäten zum Export und Import in die Datenbank
    /// Nicht vorhandene Tabelleneinträge werden automatisch erzeugt, wenn das Parentobjekt vorhanden ist
    /// </summary>
    public class ACEntitySerializer
    {

        #region ctor's

        /// <summary>
        /// Initializes a new instance of the <see cref="ACEntitySerializer"/> class.
        /// </summary>
        public ACEntitySerializer()
        {

        }

        #endregion

        #region Progress and messages

        /// <summary>
        /// Gets or sets the background worker.
        /// </summary>
        /// <value>The background worker.</value>
        public IVBProgress VBProgress
        {
            get;
            set;
        }

        private List<Msg> msgList;
        public List<Msg> MsgList
        {
            get
            {
                if (msgList == null)
                    msgList = new List<Msg>();
                return msgList;
            }

        }

        #endregion

        #region Serialize

        /// <summary>
        /// Serializes the specified ac object.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="acQueryDefinition">The ac query definition.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="withChilds">if set to <c>true</c> [with childs].</param>
        /// <returns>System.String.</returns>
        public XElement SerializeACObject(IACObject acObject, ACQueryDefinition acQueryDefinition, string folderPath, bool withChilds = true)
        {
            if (acObject == null) 
                return null;

            // Define element header
            XElement xElement = new XElement(acObject.GetType().Name);
            xElement.Add(new XAttribute("Type", acObject.GetType().FullName));
            xElement.Add(new XAttribute("ACUrl", acObject.GetACUrl()));

            // Transpose element attributes into xml sub-elements
            PropertyInfo[] properties = acObject.GetType().GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                if (pi.GetIndexParameters().Length != 0)
                    continue;
                if (pi.PropertyType.IsGenericType)
                {
#if !EFCR
                    if (pi.PropertyType.GetInterfaces().Where(c => c.Name == "IEnumerable").Any() ||
                        pi.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityReference)
                        )
                    {
                        continue;
                    }
#endif
                }
                if (pi.GetCustomAttributes(typeof(DataMemberAttribute), true).Any() && pi.Name != "EntityKey")
                {
                    var value = pi.GetValue(acObject, null);
                    if (pi.PropertyType == typeof(Guid))
                    {
                        continue;
                    }
                    if (value == null)
                    {
                        xElement.Add(new XElement(pi.Name, "i:NULL"));
                        continue;
                    }

                    bool isNullablePrimitive =
                        pi.PropertyType.GetGenericArguments() != null &&
                        pi.PropertyType.GetGenericArguments().Any() &&
                        pi.PropertyType.GetGenericArguments().First().IsPrimitive;
                    if (pi.PropertyType.IsPrimitive || isNullablePrimitive)
                    {
                        xElement.Add(new XElement(pi.Name, value.ToString()));
                    }
                    else if (pi.PropertyType == typeof(string))
                    {
                        xElement.Add(new XElement(pi.Name, (string)value));
                    }
                    else if (pi.PropertyType == typeof(DateTime))
                    {
                        xElement.Add(new XElement(pi.Name, ((DateTime)value).ToString("o")));
                    }
                    else if (pi.PropertyType == typeof(System.Byte[]))
                    {
                        if (acObject is ACClassDesign)
                        {
                            ACClassDesign acClassDesign = acObject as ACClassDesign;
                            acClassDesign.DownloadReportFile(folderPath);

                            xElement.Add(new XElement(pi.Name, "\\Resources\\#" + folderPath + "\\" + acClassDesign.ReportFileName));
                        }
                    }
                    else if (value is IACObject)
                    {
                        // Falls ParentObjekt, dann nicht mit serialisieren
                        if (acObject.ParentACObject == value)
                            continue;
                        xElement.Add(new XElement(pi.Name, (value as IACObject).GetACUrl()));
                    }
                }
            }

            if (withChilds)
            {
                var childQuery =
                     acQueryDefinition
                     .ACQueryDefinitionChilds
                     .GroupBy(x => x.QueryType.ACIdentifier)
                     .Select(x => new { ACIdentifier = x.Key, QueryDef = x.FirstOrDefault() });
                foreach (var child in childQuery)
                {
                    XElement childElement = Serialize(acObject, child.QueryDef, folderPath, withChilds);
                    if (childElement != null)
                        xElement.Add(childElement);
                }
            }

            return xElement;
        }

        /// <summary>
        /// Serializes the childs.
        /// </summary>
        /// <param name="acObjectRoot">Hauptobject (meist Database) wird selbst nicht serialisiert</param>
        /// <param name="acQueryDefinition">Definition welche Eigenschaften, Entitäten und Unterentitäten zu serialisieren sind</param>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="withChilds">if set to <c>true</c> [with childs].</param>
        /// <returns>System.String.</returns>
        public XElement Serialize(IACObject acObjectRoot, ACQueryDefinition acQueryDefinition, string folderPath, bool withChilds = true)
        {
            IEnumerable query = acObjectRoot.ACSelect(acQueryDefinition);

            IEnumerable<IACObject> acObjectList = query as IEnumerable<IACObject>;
            if (acObjectList == null || !acObjectList.Any())
                return null;

            IACObject acObject = acObjectList.FirstOrDefault();

            // XML-Dokument erzeugen
            XElement xDoc = new XElement(acObject.GetType().Name + "List");

            // XML-Dokument erzeugen

            // Type-Attribut für die Haupt-IACObject
            string propertyTypeValue = "System.Collections.Generic.List`1[[" + acObject.GetType().FullName + "]]";
            xDoc.Add(new XAttribute("Type", propertyTypeValue));

            // Url für das Haupt-IACObject
            xDoc.Add(new XAttribute("ACUrl", acQueryDefinition.ChildACUrl));
            xDoc.Add(new XAttribute(ACQueryDefinition.ClassName, acQueryDefinition.ACIdentifier));

            foreach (IACObject childACObject in acObjectList)
            {
                string taskName = string.Format("Deserialize {0} ...", childACObject.ACIdentifier);
                if (VBProgress != null)
                {
                    VBProgress.AddSubTask(taskName, 0, 1);
                }
                XElement element = SerializeACObject(childACObject, acQueryDefinition, folderPath, withChilds);
                if (VBProgress != null)
                    VBProgress.ReportProgress(taskName, 1, string.Format("Deserialized {0}!", childACObject.ACIdentifier));
                if (element != null)
                    xDoc.Add(element);
            }

            return xDoc;
        }

#endregion

#region Derialize XML

        /// <summary>
        /// Deserializing XML file
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="acFSItemParent"></param>
        /// <param name="paht">file name</param>
        public void DeserializeXMLData(IResources resource, ACFSItem acFSItemParent, string paht)
        {
            XElement xDoc = XElement.Load(paht);
            DeserializeXMLData(resource, acFSItemParent, xDoc, paht);
        }

        /// <summary>
        /// Deserialize XML document
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="acFSItemParent"></param>
        /// <param name="xDoc"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public IACEntityObjectContext DeserializeXMLData(IResources resource, ACFSItem acFSItemParent, XElement xDoc, string path = null)
        {
            string acQueryACUrl = xDoc.Attribute(ACQueryDefinition.ClassName) != null ? xDoc.Attribute(ACQueryDefinition.ClassName).Value : "";
            string typeName = xDoc.Attribute("Type").Value;
            IACEntityObjectContext db = ACObjectContextManager.GetContextFromACUrl(acQueryACUrl, typeName);

            ACQueryDefinition navACQueryDefinition = null;
            if (!string.IsNullOrEmpty(acQueryACUrl))
            {
                ACClass acClass = db.ContextIPlus.ACClass.FirstOrDefault(x => x.ACIdentifier == acQueryACUrl);
                navACQueryDefinition = new ACQueryDefinition(acClass, acClass, db, null);
                navACQueryDefinition.ACInit(Global.ACStartTypes.Manually);
            }

            DeserializeXML(resource, db, acFSItemParent, xDoc, navACQueryDefinition, path);
            return db;
        }


        /// <summary>
        /// Deserializes the specified ac XML.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="db"></param>
        /// <param name="acFSItemParent"></param>
        /// <param name="xDoc"></param>
        /// <param name="acQueryDefinition"></param>
        /// <param name="path"></param>
        public void DeserializeXML(IResources resource, IACEntityObjectContext db, ACFSItem acFSItemParent, XElement xDoc, ACQueryDefinition acQueryDefinition, string path = null)
        {
            var typeValue = xDoc.Attribute("Type").Value;
            var acUrlValue = xDoc.Attribute("ACUrl").Value;

            Type mainType = Type.GetType(typeValue);
            if (mainType == null)
            {
                string embType = typeValue;
                if (embType.StartsWith("System.Collections.Generic.List"))
                {
                    string pattern = @"\[\[([\w.]*)\]\]";
                    Match test = Regex.Match(typeValue, pattern);
                    Group mthGroup = null;
                    if (test.Groups.Count > 1)
                        mthGroup = test.Groups[1];
                    if (mthGroup != null)
                        embType = mthGroup.Value;
                }
                string assemblyName = string.Join(".", (embType).Split('.').ToList().Take(3));
                var simpleInstance = AppDomain.CurrentDomain.CreateInstance(assemblyName, embType);
                mainType = simpleInstance.Unwrap().GetType();

                if (typeValue.StartsWith("System.Collections.Generic.List"))
                {
                    var listType = typeof(List<>);
                    mainType = listType.MakeGenericType(mainType);
                }
            }

            string taskName = acQueryDefinition != null ? acQueryDefinition.ACIdentifier : "DeserializeXML_" + (new Random()).Next().ToString();
            if (VBProgress != null)
                VBProgress.AddSubTask(taskName, 0, 1);
            if (mainType.Name == "List`1")
            {
                string listFileName = path.Substring(path.LastIndexOf("\\"));
                ACFSItem acFsListItem = new ACFSItem(resource, acFSItemParent.Container, null, listFileName, ResourceTypeEnum.List, path);
                acFSItemParent.Add(acFsListItem);
                Type acObjectType = mainType.GetGenericArguments()[0];
                List<XElement> listOfElements = xDoc.Elements(acObjectType.Name).ToList();

                if (listOfElements != null && listOfElements.Any())
                {
                    if (VBProgress != null)
                        VBProgress.AddSubTask(taskName, 0, listOfElements.Count);

                    foreach (var xElement in listOfElements)
                    {
                        DeserializeXMLRecursive(resource, db, acFsListItem, xElement, acQueryDefinition, path);
                        if (VBProgress != null)
                            VBProgress.ReportProgress(taskName, listOfElements.IndexOf(xElement), "Deserialized " + xElement.Name);
                    }

                    foreach (ACFSItem childItem in acFsListItem.Items)
                    {
                        DeserializeXMLChildren(resource, db, acQueryDefinition, path, childItem);
                    }

                    if (VBProgress != null)
                        VBProgress.ReportProgress(taskName, listOfElements.Count, "Deserialized " + taskName);
                }


            }
            else
            {
                DeserializeXMLRecursive(resource, db, acFSItemParent, xDoc, acQueryDefinition, path);
                foreach (var item in acFSItemParent.Items)
                {
                    ACFSItem childItem = item as ACFSItem;
                    DeserializeXMLChildren(resource, db, acQueryDefinition, path, childItem);
                }
                if (VBProgress != null)
                    VBProgress.ReportProgress(taskName, 1, "Deserialized " + taskName);
            }

        }

        /// <summary>
        /// Deserializes recursive.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="db"></param>
        /// <param name="acFSParentItem"></param>
        /// <param name="xNode"></param>
        /// <param name="acQueryDefinition"></param>
        /// <param name="path"></param>
        public void DeserializeXMLRecursive(IResources resource, IACEntityObjectContext db, ACFSItem acFSParentItem, XElement xNode, ACQueryDefinition acQueryDefinition, string path = null)
        {
            //Type objectType = ACObjectHelper.GetType(xNode.Attribute("Type").Value);
            Type objectType = TypeAnalyser.GetTypeInAssembly(xNode.Attribute("Type").Value);

            if (acQueryDefinition != null && acQueryDefinition.QueryType.ObjectType != objectType)
            {
                MsgList.Add(new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format("Wrong QueryDefinition for '{0}'!", objectType.FullName) });
                return;
            }

            var acUrl = xNode.Attribute("ACUrl").Value;


            IACObject acObject = ACObjectSerialHelper.FactoryACObject(acFSParentItem, acUrl);

            if (acObject == null)
            {
                string parentURL = "";
                if (acFSParentItem.ACObject != null)
                    parentURL = acFSParentItem.ACObject.GetACUrl();
                else if (acFSParentItem.Container != null && acFSParentItem.Container is IACObject)
                    parentURL = (acFSParentItem.Container as IACObject).GetACUrl();
                MsgList.Add(new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format("Can not Create Object '{0}', because parent does not exists !", parentURL + "\\" + acUrl) });
                return;
            }

            ACFSItem acFsItem = new ACFSItem(resource, acFSParentItem.Container, acObject, acObject.ACIdentifier, ResourceTypeEnum.IACObject, path);
            acFsItem.ACObjectACUrl = acUrl;
            acFSParentItem.Add(acFsItem);
            acFsItem.XNode = xNode;

            //// Place for define KeyACValue property
            if ((acObject as VBEntityObject).EntityState == EntityState.Added)
                ACObjectSerialHelper.SetIACObjectProperties(resource, db, acFSParentItem.Container, acFsItem, true, msgList);
            acFsItem.ACObjectACUrl = acObject.GetACUrl();
            acFsItem.ACCaption = acObject.ACIdentifier;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="db"></param>
        /// <param name="acQueryDefinition"></param>
        /// <param name="path"></param>
        /// <param name="acFsItem"></param>
        public void DeserializeXMLChildren(IResources resource, IACEntityObjectContext db, ACQueryDefinition acQueryDefinition, string path, ACFSItem acFsItem)
        {
            if (acQueryDefinition != null && acQueryDefinition.ACQueryDefinitionChilds.Any())
            {
                var preventACQueryDefinitionDuplicates =
                    acQueryDefinition
                    .ACQueryDefinitionChilds
                    .GroupBy(x => x.QueryType.ACIdentifier)
                    .Select(x => new { ACIdentifier = x.Key, QueryDef = x.FirstOrDefault() });
                foreach (var queryItem in preventACQueryDefinitionDuplicates)
                {
                    ACQueryDefinition queryDefinitionChild = queryItem.QueryDef;
                    XElement xChilds = acFsItem.XNode.Element(queryDefinitionChild.QueryType.ACIdentifier + "List");
                    if (xChilds != null)
                    {
                        List<XNode> nodes = xChilds.Nodes().Where(c => c is XElement).ToList();
                        if (nodes != null && nodes.Any())
                        {
                            string taskName = queryDefinitionChild.ACIdentifier;
                            if (VBProgress != null)
                                VBProgress.AddSubTask(taskName, 0, nodes.Count);
                            foreach (var xmlChild in nodes)
                            {
                                DeserializeXMLRecursive(resource, db, acFsItem, xmlChild as XElement, queryDefinitionChild, path);
                                if (VBProgress != null)
                                    VBProgress.ReportProgress(taskName, nodes.IndexOf(xmlChild), "Process node ..");
                            }
                            foreach (var item in acFsItem.Items)
                            {
                                ACFSItem childItem = item as ACFSItem;
                                DeserializeXMLChildren(resource, db, queryDefinitionChild, path, childItem);
                            }
                            if (VBProgress != null)
                                VBProgress.ReportProgress(taskName, nodes.Count, string.Format("Process finished: {0}", taskName));
                        }
                    }
                }
            }

        }

#endregion

#region Deserialize SQL

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlInstanceInfo"></param>
        /// <param name="acQueryDefinitionIdentifier"></param>
        /// <returns></returns>
        public DeserializedSQLDataModel GetDeserializeSQLDataModel(SQLInstanceInfo sqlInstanceInfo, string acQueryDefinitionIdentifier)
        {
            List<IACObject> acObjectList = null;
            bool iPlusContext = acQueryDefinitionIdentifier.StartsWith("acQueryACUrl");
            IACEntityObjectContext dbOuterDatabase = ACObjectContextManager.FactoryContext(sqlInstanceInfo, iPlusContext);
            ACClass qryACClass = dbOuterDatabase.ContextIPlus.ACClass.FirstOrDefault(x => x.ACIdentifier == acQueryDefinitionIdentifier);
            ACQueryDefinition acQueryDefinition = FactoryImportACQueryDefinition(dbOuterDatabase, qryACClass);
            IEnumerable query = dbOuterDatabase.ACSelect(acQueryDefinition);
            acObjectList = (query as IEnumerable<IACObject>).ToList();
            string contextName = iPlusContext ? Const.GlobalDatabase : RootDbOpQueue.ClassName + "." + RootDbOpQueue.AppContextPropName;
            IACEntityObjectContext db = ACObjectContextManager.GetContext(contextName);
            return new DeserializedSQLDataModel()
            {
                InnerDatabase = db,
                OuterDatabase = dbOuterDatabase,
                ACQueryDefinition = acQueryDefinition,
                DeserializedSQLData = acObjectList
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="db"></param>
        /// <param name="acFSParentItem"></param>
        /// <param name="outerACObjects"></param>
        /// <param name="aCQueryDefinition"></param>
        /// <param name="path"></param>
        public void DeserializeSQL(IResources resource, IACEntityObjectContext db, ACFSItem acFSParentItem, List<IACObject> outerACObjects, ACQueryDefinition aCQueryDefinition, string path = null)
        {
            string taskName = string.Format(@"ResourcesSQL.Dir(""{0}"")", path);
            foreach (IACObject outerAcObject in outerACObjects)
            {
                DeserializeSQLRecursive(resource, db, acFSParentItem, outerAcObject, aCQueryDefinition);

                if (VBProgress != null)
                    VBProgress.ReportProgress(taskName, outerACObjects.IndexOf(outerAcObject), "Deserialized item: " + outerAcObject.ACIdentifier);
            }
            if (acFSParentItem.Items != null && acFSParentItem.Items.Any())
            {
                foreach (IACObject childItem in acFSParentItem.Items)
                {
                    DeserializeSQLChildren(resource, db, childItem as ACFSItem, (childItem as ACFSItem).OuterIACObject, aCQueryDefinition);
                }
            }
        }

        /// <summary>
        /// DeserializeSQLRecursive
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="db"></param>
        /// <param name="acFSParentItem"></param>
        /// <param name="outerAcObject"></param>
        /// <param name="acQueryDefinition"></param>
        public void DeserializeSQLRecursive(IResources resource, IACEntityObjectContext db, ACFSItem acFSParentItem, IACObject outerAcObject, ACQueryDefinition acQueryDefinition)
        {

            if (acQueryDefinition.QueryType.ObjectType != outerAcObject.GetType())
            {
                MsgList.Add(new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format("Wrong QueryDefinition for '{0}'!", outerAcObject.GetType().FullName) });
                return;
            }

            var acUrl = outerAcObject.GetACUrl();

            IACObject acObject = ACObjectSerialHelper.FactoryACObject(acFSParentItem, acUrl);

            if (acObject == null)
            {
                MsgList.Add(new Msg()
                {
                    MessageLevel = eMsgLevel.Error,
                    Message = string.Format("Can not Create Object '{0}'!", db.GetACUrl() + "\\" + acUrl)
                });
                return;
            }

            ACFSItem acFsItem = new ACFSItem(resource, acFSParentItem.Container, acObject, acObject.ACIdentifier, ResourceTypeEnum.IACObject);
            acFSParentItem.Add(acFsItem);
            acFsItem.ACObjectACUrl = outerAcObject.GetACUrl();
            acFsItem.ACCaption = outerAcObject.ACIdentifier;
            acFsItem.OuterIACObject = outerAcObject;
            if ((acObject as VBEntityObject).EntityState == EntityState.Added)
                ACObjectSerialHelper.SetIACObjectProperties(resource, db, acFSParentItem.Container, acFsItem, true, msgList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="db"></param>
        /// <param name="outerAcObject"></param>
        /// <param name="acQueryDefinition"></param>
        /// <param name="acFSParentItem"></param>
        public void DeserializeSQLChildren(IResources resource, IACEntityObjectContext db, ACFSItem acFSParentItem, IACObject outerAcObject, ACQueryDefinition acQueryDefinition)
        {
            var preventACQueryDefinitionDuplicates =
                     acQueryDefinition
                     .ACQueryDefinitionChilds
                     .GroupBy(x => x.QueryType.ACIdentifier)
                     .Select(x => new { ACIdentifier = x.Key, QueryDef = x.FirstOrDefault() });
            foreach (var itemACQuery in preventACQueryDefinitionDuplicates)
            {
                ACQueryDefinition queryDefinitionChild = itemACQuery.QueryDef;
                var query = outerAcObject.ACSelect(queryDefinitionChild);
                if (!query.Any()) continue;
                IEnumerable<IACObject> outerChilds = (query as IEnumerable<IACObject>);
                if (outerChilds != null)
                {
                    foreach (var outerChildAcObject in outerChilds)
                    {
                        DeserializeSQLRecursive(resource, db, acFSParentItem, outerChildAcObject, queryDefinitionChild);
                    }
                    if (acFSParentItem.Items != null && acFSParentItem.Items.Any())
                    {
                        foreach (var innerChildAcObject in acFSParentItem.Items)
                        {
                            DeserializeSQLChildren(resource, db, innerChildAcObject as ACFSItem, (innerChildAcObject as ACFSItem).OuterIACObject, queryDefinitionChild);
                        }
                    }
                }
            }
        }

#endregion

#region Helper common mehtods

        /// <summary>
        /// Factory ACQueryDef with config defined for Import
        /// keyword: ImportData
        /// </summary>
        /// <param name="db"></param>
        /// <param name="acQueryDefACClass"></param>
        /// <returns></returns>
        public static ACQueryDefinition FactoryImportACQueryDefinition(IACEntityObjectContext db, ACClass acQueryDefACClass)
        {
            ACValueList acValueList = new ACValueList();
            acValueList.Add(new ACValue("IsLoadConfig", true));
            acValueList.Add(new ACValue(Const.PN_LocalConfigACUrl, "ImportData"));
            ACQueryDefinition acQueryDefinition = new ACQueryDefinition(acQueryDefACClass, acQueryDefACClass, db, acValueList);
            acQueryDefinition.ACInit(Global.ACStartTypes.Manually);
            return acQueryDefinition;
        }

#endregion

    }
}
