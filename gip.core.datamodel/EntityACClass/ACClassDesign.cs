// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-14-2013
// ***********************************************************************
// <copyright file="ACClassDesign.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACClassDesign is used to store elements that are responsible for presenting a class. 
    /// The XMLDesign field stores XAML code compiled at run time by the XAML parser. 
    /// XAML code is used for both UI presentation and reports. 
    /// The DesignBinary field stores images (*.jpg, *.png...).
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Design'}de{'Design'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, Const.ACIdentifierPrefix, "en{'Design Name/ID'}de{'Designname/ID'}", "", "", true)]
    [ACPropertyEntity(3, "ACIdentifierKey", "en{'Key'}de{'Schlüssel'}","", "", true)]
    [ACPropertyEntity(4, ACClass.ClassName, "en{'Class'}de{'Klasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(5, Const.ACGroup, "en{'Group'}de{'Gruppe'}","", "", true)]
    [ACPropertyEntity(6, Const.ACKindIndex, "en{'Design Type'}de{'Designart'}", typeof(Global.ACKinds), Const.ContextDatabaseIPlus + "\\ACKindDSList", "", true)]
    [ACPropertyEntity(7, "ACUsageIndex", "en{'Usage'}de{'Verwendung'}", typeof(Global.ACUsages), Const.ContextDatabaseIPlus + "\\ACUsageList", "", true)]
    [ACPropertyEntity(8, "SortIndex", "en{'Sortindex'}de{'Sortierung'}","", "", true)]
    [ACPropertyEntity(9, "IsRightmanagement", "en{'Rights Management'}de{'Rechteverwaltung'}","", "", true)]
    [ACPropertyEntity(10, "IsDefault", "en{'Default'}de{'Standard'}","", "", true)]
    [ACPropertyEntity(11, "Comment", "en{'Comment'}de{'Bemerkung'}","", "", true)]
    [ACPropertyEntity(12, "ValueTypeACClass", "en{'Datatype'}de{'Datentyp'}", Const.ContextDatabaseIPlus + "\\VBControlACClassList", "", true)]
    [ACPropertyEntity(13, "IsSystem", "en{'System'}de{'System'}","", "", true)]
    [ACPropertyEntity(14, "DesignerMaxRecursion", "en{'Max. Recursion Designerobjects'}de{'Max. Rekursionstiefe Designerobjekte'}","", "", true)]
    [ACPropertyEntity(9999, "ACCaptionTranslation", "en{'Translation'}de{'Übersetzung'}","", "", true)]
    [ACPropertyEntity(9999, "DesignBinary", "en{'Binary code'}de{'Binärcode'}","", "", true)]
    [ACPropertyEntity(9999, "DesignNo", "en{'Design No.'}de{'Designnr.'}","", "", true)]
    [ACPropertyEntity(9999, "XMLDesign", "en{'Design'}de{'Design'}")]
    [ACPropertyEntity(9999, "FilterIndex", "en{'Filter'}de{'Filter'}","", "", true)]
    [ACPropertyEntity(9999, "IsResourceStyle", "en{'Resourcestyle'}de{'WPF-Resource'}","", "", true)]
    [ACPropertyEntity(9999, "ACValueType", "en{'Valuetype'}de{'Werttyp'}","", "", true)]
    [ACPropertyEntity(9999, "VisualHeight", "en{'Height'}de{'Höhe'}","", "", true)]
    [ACPropertyEntity(9999, "VisualWidth", "en{'Width'}de{'Breite'}","", "", true)]
    [ACDeleteAction("ACClassMapDesign_ACClassDesign", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("VBGroupRight_ACClassDesign", Global.DeleteAction.CascadeManual)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassDesign.ClassName, "en{'Design'}de{'Design'}", typeof(ACClassDesign), ACClassDesign.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassDesign>) })]
    [NotMapped]
    public partial class ACClassDesign : IACObjectEntityWithCheckTrans, IACEntityProperty, IACType, IACObjectDesign, IACClassEntity, IACConfigStore, ICloneable
    {
        public const string ClassName = "ACClassDesign";
        public const string NoColumnName = "LayoutNo";
        public const string FormatNewNo = null;

        public readonly ACMonitorObject _10020_LockValue = new ACMonitorObject(10020);


        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="secondaryKey">secondaryKey</param>
        /// <returns>ACClassDesign.</returns>
        public static ACClassDesign NewACObject(Database database, IACObject parentACObject, string secondaryKey)
        {
            ACClassDesign entity = new ACClassDesign();
            entity.Context = database;
            entity.ACClassDesignID = Guid.NewGuid();
            entity.ACIdentifier = "NewDesign";
            entity.IsRightmanagement = false;
            entity.IsDefault = false;
            entity.IsResourceStyle = false;
            entity.VisualHeight = 0;
            entity.VisualWidth = 0;
            entity.ACKind = Global.ACKinds.DSDesignLayout;
            entity.ACUsage = Global.ACUsages.DUControl;
            entity.UpdateVBControlACClass(database);
            entity.SortIndex = 999;
            entity.XMLDesign = "";
            entity.DesignNo = secondaryKey;
            entity.BranchNo = 0;

            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            if (parentACObject is ACClass)
            {
                entity.ACClassID = (parentACObject as ACClass).ACClassID;
            }

            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        /// <summary>
        /// News the AC class design visualisation.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="secondaryKey">secondaryKey</param>
        /// <returns>ACClassDesign.</returns>
        public static ACClassDesign NewACClassDesignVisualisation(Database database, IACObject parentACObject, string secondaryKey)
        {
            ACClassDesign entity = ACClassDesign.NewACObject(database, parentACObject, secondaryKey);
            entity.ACKind = Global.ACKinds.DSDesignLayout;
            entity.ACUsage = Global.ACUsages.DUVisualisation;
//            entity.XMLDesign = "<?xml version=\"1.0\" encoding=\"utf-16\"?><Grid " + ACxmlnsResolver.XMLNamespaces + "><vb:VBScrollViewer VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Auto\"><vb:VBCanvas Background=\"#FF000000\"></vb:VBCanvas></vb:VBScrollViewer></Grid>";
            entity.XMLDesign = "<?xml version=\"1.0\" encoding=\"utf-16\"?><vb:VBScrollViewer  " + ACxmlnsResolver.XMLNamespaces + " VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Auto\"><vb:VBCanvas Height=\"100\" Width=\"100\" Background=\"#FF000000\"></vb:VBCanvas></vb:VBScrollViewer>";
            return entity;
        }

        /// <summary>
        /// Deletes this entity-object from the database
        /// UNSAFE, use (QueryLock_1X000)
        /// </summary>
        /// <param name="database">Entity-Framework databasecontext</param>
        /// <param name="withCheck">If set to true, a validation happens before deleting this EF-object. If Validation fails message ís returned.</param>
        /// <param name="softDelete">If set to true a delete-Flag is set in the dabase-table instead of a physical deletion. If  a delete-Flag doesn't exit in the table the record will be deleted.</param>
        /// <returns>If a validation or deletion failed a message is returned. NULL if sucessful.</returns>
        public override MsgWithDetails DeleteACObject(IACEntityObjectContext database, bool withCheck, bool softDelete = false)
        {
            if (withCheck)
            {
                MsgWithDetails msg = IsEnabledDeleteACObject(database);
                if (msg != null)
                    return msg;
            }


            var query2 = (database as Database).VBGroupRight.Where(c => c.ACClassDesign.ACClassDesignID == this.ACClassDesignID).ToList();
            foreach (var vbGroupRight in query2)
            {
                vbGroupRight.DeleteACObject(database, withCheck);
            }

            database.Remove(this);

            return null;
        }

        #endregion

        #region IACUrl Member

        /// <summary>
        /// ACs the URL AC type signature.
        /// </summary>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="attachToObject">attachToObject</param>
        /// <returns>ACMethod.</returns>
        public ACMethod ACUrlACTypeSignature(string acUrl, IACObject attachToObject = null)
        {
            return null;
        }

        /// <summary>
        /// Types the AC signature.
        /// </summary>
        /// <returns>ACMethod.</returns>
        public ACMethod TypeACSignature()
        {
            return null;
        }

        /// <summary>
        /// Gets the AC URL.
        /// </summary>
        /// <value>The AC URL.</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public string ACUrl
        {
            get
            {
                return GetACUrl();
            }
        }

        /// <summary>
        /// Returns ACClass, where this design belongs to
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>Reference to ACClass</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return ACClass;
                }
            }
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            if (string.IsNullOrEmpty(ACIdentifier))
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = Const.ACIdentifierPrefix,
                    Message = "ACIdentifier is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", Const.ACIdentifierPrefix), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            base.EntityCheckAdded(user, context);
            return null;
        }

        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for changed entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckModified(string user, IACEntityObjectContext context)
        {
            base.EntityCheckModified(user, context);
            return null;
        }

        /// <summary>
        /// Updates the VB control AC class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool UpdateVBControlACClass(Database database)
        {
            switch (ACUsage)
            {
                case Global.ACUsages.DUControl:
                    if (ValueTypeACClass == null || ValueTypeACClass.ACIdentifier != Const.VBVisual_ClassName || ValueTypeACClass.ACIdentifier != Const.VBVisualGroup_ClassName)
                    {
                        ValueTypeACClass = database.ACClass.Where(c => c.ACIdentifier == Const.VBVisual_ClassName).First();
                        return true;
                    }
                    break;
                case Global.ACUsages.DUMain:
                case Global.ACUsages.DULayout:
                case Global.ACUsages.DUControlDialog:
                case Global.ACUsages.DUDiagnostic:
                case Global.ACUsages.DUVisualisation:
                    if (IsResourceStyle == false)
                    {
                        if (ValueTypeACClass == null || ValueTypeACClass.ACIdentifier != Const.VBDesign_ClassName)
                        {
                            ValueTypeACClass = database.ACClass.Where(c => c.ACIdentifier == Const.VBDesign_ClassName).First();
                            return true;
                        }
                    }
                    else
                    {
                        if (ValueTypeACClass == null || ValueTypeACClass.ACIdentifier != Const.VBBorder_ClassName)
                        {
                            ValueTypeACClass = database.ACClass.Where(c => c.ACIdentifier == Const.VBBorder_ClassName).First();
                            return true;
                        }
                    }
                    break;
                case Global.ACUsages.DUMainmenu:
                    if (ValueTypeACClass != null)
                    {
                        ValueTypeACClass = null;
                        return true;
                    }
                    break;
                case Global.ACUsages.DUBitmap:
                case Global.ACUsages.DUIcon:
                    if (ValueTypeACClass == null || ValueTypeACClass.ACIdentifier != Const.VBImage_ClassName)
                    {
                        ValueTypeACClass = database.ACClass.Where(c => c.ACIdentifier == Const.VBImage_ClassName).First();
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return Const.ACIdentifierPrefix;
            }
        }
        #endregion

        #region IACType Member

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        [NotMapped]
        public Type ObjectType
        {
            get
            {
                if (ValueTypeACClass == null)
                    return null;
                return ValueTypeACClass.ObjectType;
            }
        }

        /// <summary>
        /// Gets the full type of the object.
        /// </summary>
        /// <value>The full type of the object.</value>
        [NotMapped]
        public Type ObjectFullType
        {
            get
            {
                return ObjectType;
            }
        }

        /// <summary>
        /// Gets the object type parent.
        /// </summary>
        /// <value>The object type parent.</value>
        [NotMapped]
        public Type ObjectTypeParent
        {
            get
            {
                return ACClass.ObjectType;
            }
        }

        /// <summary>
        /// Gets the kind of the AC.
        /// </summary>
        /// <value>The kind of the AC.</value>
        [NotMapped]
        public Global.ACKinds ACKind
        {
            get
            {
                return (Global.ACKinds)ACKindIndex;
            }
            set
            {
                ACKindIndex = (short)value;
            }
        }

        /// <summary>
        /// Gets or sets the AC usage.
        /// </summary>
        /// <value>The AC usage.</value>
        [NotMapped]
        public Global.ACUsages ACUsage
        {
            get
            {
                return (Global.ACUsages)ACUsageIndex;
            }
            set
            {
                ACUsageIndex = (short)value;
            }
        }

        #endregion

#region IEntityProperty Members

        [NotMapped]
        bool bRefreshConfig = false;
        protected override void OnPropertyChanging<T>(T newValue, string propertyName, bool afterChange)
        {
            if (propertyName == nameof(XMLConfig))
            {
                string xmlConfig = newValue as string;
                if (afterChange)
                {
                    if (bRefreshConfig)
                        ACProperties.Refresh();
                }
                else
                {
                    bRefreshConfig = false;
                    if (!(String.IsNullOrEmpty(xmlConfig) && String.IsNullOrEmpty(XMLConfig)) && xmlConfig != XMLConfig)
                        bRefreshConfig = true;
                }
            }
            base.OnPropertyChanging(newValue, propertyName, afterChange);
        }

        /// <summary>
        /// Gets the type AC sort columns.
        /// </summary>
        /// <value>The type AC sort columns.</value>
        [NotMapped]
        public static string TypeACSortColumns
        {
            get
            {
                ACClass ReflectedACType = Database.GlobalDatabase.GetACType(typeof(ACClassDesign)) as ACClass;
                if (string.IsNullOrEmpty(ReflectedACType.ACSortColumns))
                {
                    return ReflectedACType.GetColumns().First().PropertyName;
                }
                return ReflectedACType.ACSortColumns;
            }
        }

#endregion
        
#region ReportHandling
        /// <summary>
        /// Gets the name of the report file.
        /// </summary>
        /// <value>The name of the report file.</value>
        [NotMapped]
        public string ReportFileName
        {
            get
            {
                string reportFileName = ACIdentifier + "_" + DesignNo;

                if (DesignBinary != null)
                {
                    using(var ms = new MemoryStream(DesignBinary))
                    {
                        System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
                        if (image != null)
                        {
                            if (System.Drawing.Imaging.ImageFormat.Png.Equals(image.RawFormat))
                                return reportFileName += ".png";
                            if (System.Drawing.Imaging.ImageFormat.Jpeg.Equals(image.RawFormat))
                                return reportFileName += ".jpeg";
                            if (System.Drawing.Imaging.ImageFormat.Bmp.Equals(image.RawFormat))
                                return reportFileName += ".bmp";
                        }
                    }
                }
                switch (ACUsage)
                {
                    case Global.ACUsages.DULLReport:
                        reportFileName += ".lst";
                        break;
                    case Global.ACUsages.DULLOverview:
                        reportFileName += ".lst";
                        break;
                    case Global.ACUsages.DULLList:
                        reportFileName += ".lst";
                        break;
                    case Global.ACUsages.DULLLabel:
                        reportFileName += ".lbl";
                        break;
                    case Global.ACUsages.DULLFilecard:
                        reportFileName += ".crd";
                        break;
                    default:
                        reportFileName += ".lst";
                        break;
                }
                
                return reportFileName;
            }
        }

        /// <summary>
        /// Uploads the report file.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        public void UploadReportFile(string folderPath = "")
        {
            if (ReportFileName != null)
            {
                if (string.IsNullOrEmpty(folderPath))
                    DesignBinary = File.ReadAllBytes(ReportFileName);
                else
                    DesignBinary = File.ReadAllBytes(folderPath + "\\" + ReportFileName);
            }
        }

        /// <summary>
        /// Downloads the report file.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        public void DownloadReportFile(string folderPath = "")
        {
            if (ReportFileName != null && DesignBinary != null)
            {
                if ( string.IsNullOrEmpty(folderPath))
                    File.WriteAllBytes(ReportFileName, DesignBinary);
                else
                    File.WriteAllBytes(folderPath + "\\" + ReportFileName, DesignBinary);
            }
        }
#endregion

#region MenuHandling
        /// <summary>
        /// Gets or sets the menu entry.
        /// </summary>
        /// <value>The menu entry.</value>
        [NotMapped]
        public ACMenuItem MenuEntry
        {
            get
            {
                if (this.ACKind != Global.ACKinds.DSDesignMenu || XMLDesign == null || XMLDesign == "")
                {
                    return new ACMenuItem(null, "", "", 0, null);
                }
                ACMenuItem menuEntry = null;
                // Einmal mit DataContractSerializer serialisieren
                using (StringReader ms = new StringReader(XMLDesign))
                using (XmlTextReader xmlReader = new XmlTextReader(ms))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(ACMenuItem));
                    menuEntry = (ACMenuItem)serializer.ReadObject(xmlReader);
                }

                // Damit das einfügen richtig funktioniert, muß immer der Parent bekannt sein.
                SetMenuEntryParentsAndGuid(menuEntry);

                return menuEntry;
            }
            set
            {
                if (this.ACKind != Global.ACKinds.DSDesignMenu)
                    return;
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(ACMenuItem));
                    serializer.WriteObject(xmlWriter, value);

                    string menuXML = sw.ToString();
                    if (XMLDesign != menuXML)
                        XMLDesign = menuXML;
                }
            }
        }

        /// <summary>
        /// Gets the menu entry with check.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <returns>ACMenuItem.</returns>
        public ACMenuItem GetMenuEntryWithCheck(IACObject acObject)
        {
            ACMenuItem menuEntry = MenuEntry;
            CheckMenuRights(menuEntry, acObject);
            if (!UpdateMenuItems(menuEntry))
                return null;
            return menuEntry;
        }

        /// <summary>
        /// Checks the menu rights.
        /// </summary>
        /// <param name="menuEntry">The menu entry.</param>
        /// <param name="acObject">The ac object.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private bool CheckMenuRights(ACMenuItem menuEntry, IACObject acObject)
        {
            if (menuEntry.Items == null || menuEntry.Items.Count == 0)
            {
                return false;
            }
            for (int i = menuEntry.Items.Count - 1; i >= 0; i--)
            {
                ACMenuItem menuEntryChild = menuEntry.Items[i];
                if (!string.IsNullOrEmpty(menuEntryChild.GetACUrl()))
                {
                    Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

                    if (menuEntryChild.ACUrlCommandString.StartsWith(Const.BusinessobjectsACUrl))
                    {
                        string acClassName = menuEntryChild.ACUrlCommandString.Substring(Const.BusinessobjectsACUrl.Length);
                        if (String.IsNullOrEmpty(acClassName))
                            dcRightControlMode = Global.ControlModes.Hidden;
                        else
                        {
                            if (ACUrlHelper.DelimitersReserved.Contains(acClassName[0]))
                                acClassName = acClassName.Substring(1);
                            int nextDelimiter = acClassName.IndexOfAny(ACUrlHelper.DelimitersReserved);
                            if (nextDelimiter > 0)
                                acClassName = acClassName.Substring(0, nextDelimiter);

                            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                            {
                                try
                                {
                                    ACClass acClass = this.Database.ACClass.Where(c => c.ACIdentifier == acClassName).FirstOrDefault();
                                    if (acClass != null)
                                    {
                                        if (acClass.IsLicensed
                                            || (Database.Root != null
                                                && (Database.Root.Environment.License.MayUserDevelop
                                                    || (Database.Root.Environment.License.IsTrial && !Database.Root.Environment.License.IsTrialTimeExpired))))
                                        {
                                            dcRightControlMode = acClass.GetRight(acClass);
                                        }
                                        else
                                        {
                                            dcRightControlMode = Global.ControlModes.Hidden;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    dcRightControlMode = Global.ControlModes.Hidden;

                                    string msg = e.Message;
                                    if (e.InnerException != null && e.InnerException.Message != null)
                                        msg += " Inner:" + e.InnerException.Message;

                                    if (Database.Root != null && Database.Root.Messages != null)
                                        Database.Root.Messages.LogException("ACClassDesign", "CheckMenuRights", msg);
                                }
                            }
                        }
                    }
                    else if (menuEntryChild.ACUrlCommandString.IndexOf(ACUrlHelper.Delimiter_InvokeMethod) != -1)
                    {
                        dcRightControlMode = Global.ControlModes.Enabled;
                    }

                    if (dcRightControlMode < Global.ControlModes.Disabled)
                    {
                        menuEntry.Items.Remove(menuEntryChild);
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(menuEntryChild.ACCaptionTranslation) && menuEntryChild.ACCaption != "-")
                    {
                        if (!menuEntryChild.HasTranslations)
                            menuEntryChild.ACCaption = Database.Root.Environment.TranslateText(Database.Root, menuEntryChild.ACCaption);
                    }
                }
                if (menuEntryChild.Items != null && menuEntryChild.Items.Count > 0)
                {
                    if (!CheckMenuRights(menuEntryChild, acObject))
                    {
                        menuEntry.Items.RemoveAt(i);
                    }
                }

            }
            return menuEntry.Items.Count > 0;
        }

        /// <summary>
        /// Updates the menu items.
        /// </summary>
        /// <param name="menuEntry">The menu entry.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private bool UpdateMenuItems(ACMenuItem menuEntry)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(menuEntry.ACUrlCommandString))
            {
                return true;
            }

            // Erst mal alle nicht benötigten leeren Untermenüs entfernen
            foreach (var menuItem in menuEntry.Items.ToList())
            {
                if (menuItem.ACCaption == "-")
                    continue;
                if (!UpdateMenuItems(menuItem))
                {
                    menuEntry.Items.Remove(menuItem);
                }
                else
                {
                    result = true;
                }
            }

            // Nun die Seperatoren "-" entfernen
            int lastType = 0; // 0 = kein Vorgänger, 1 = Seperatur, 2 = Menüpunkt
            foreach (var menuItem in menuEntry.Items.ToList())
            {
                if (menuItem.ACCaption == "-")
                {
                    if (lastType == 0 || lastType == 1) // Wenn kein Eintrag oder vorheriger ein Seperator ist, dann entfernen
                    {
                        menuEntry.Items.Remove(menuItem);
                    }
                    else
                    {
                        lastType = 1;
                    }
                    continue;
                }
                else if (!string.IsNullOrEmpty(menuEntry.ACUrlCommandString) || !string.IsNullOrEmpty(menuEntry.ACCaption))
                {
                    lastType = 2;
                }
                else
                {
                    menuEntry.Items.Remove(menuItem);
                }
            }

            if (menuEntry.Items.Any())
            {
                if (menuEntry.Items.Last().ACCaption == "-")
                {
                    menuEntry.Items.Remove(menuEntry.Items.Last());
                }
            }

            return result;
        }
#endregion

#region private methods
        /// <summary>
        /// Sets the menu entry parents and GUID.
        /// </summary>
        /// <param name="menuEntry">The menu entry.</param>
        void SetMenuEntryParentsAndGuid(ACMenuItem menuEntry)
        {
            //menuEntry.MenuEntryID = Guid.NewGuid();
            if (menuEntry.Items != null && menuEntry.Items.Count > 0)
            {
                foreach (ACMenuItem menuEntryChild in menuEntry.Items)
                {
                    menuEntryChild.ParentMenuEntry = menuEntry;
                    SetMenuEntryParentsAndGuid(menuEntryChild);
                }
            }
        }
#endregion

#region IRightItem Member
        /// <summary>
        /// Primary Key of a Entity in the Database/Table
        /// (Uniqued Identifier of a type in the iPlus-Framework)
        /// </summary>
        [NotMapped]
        public Guid ACTypeID
        {
            get { return ACClassDesignID; }
        }

        /// <summary>
        /// Gets the AC path.
        /// </summary>
        /// <param name="first">if set to <c>true</c> [first].</param>
        /// <returns>String.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetACPath(bool first)
        {
            return null;
        }

        /// <summary>
        /// Gets the member.
        /// </summary>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <param name="forceRefreshFromDB">forceRefreshFromDB</param>
        /// <returns>IACType.</returns>
        public IACType GetMember(string acIdentifier, bool forceRefreshFromDB = false)
        {
            return null;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(2, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return Translator.GetTranslation(ACIdentifier, ACCaptionTranslation);
            }
            set
            {
                this.OnACCaptionChanging(value);
                // this.ReportPropertyChanging(Const.ACCaptionPrefix);
                ACCaptionTranslation = Translator.SetTranslation(ACCaptionTranslation, value);
                //this.ReportPropertyChanged(Const.ACCaptionPrefix);
                OnPropertyChanged(Const.ACCaptionPrefix);
                this.OnACCaptionChanged();
            }
        }
        partial void OnACCaptionChanging(string value);
        partial void OnACCaptionChanged();

        /// <summary>
        /// Gets the tooltip.
        /// </summary>
        /// <value>The tooltip.</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public string Tooltip
        {
            get
            {
                string tooltip = ACCaption;
                if (!string.IsNullOrEmpty(Comment))
                {
                    tooltip += "\n" + Comment;
                }
                return tooltip;
            }
        }

        /// <summary>
        /// Method for getting the translated text from ACCaptionTranslation
        /// </summary>
        /// <param name="VBLanguageCode">I18N-code</param>
        /// <returns>Translated text</returns>
        public string GetTranslation(string VBLanguageCode)
        {
            return Translator.GetTranslation(ACIdentifier, ACCaptionTranslation, VBLanguageCode);
        }


        /// <summary>
        /// Mies the AC columns.
        /// </summary>
        /// <param name="maxColumns">The max columns.</param>
        /// <param name="acColumns">The ac columns.</param>
        /// <returns>List{ACColumnItem}.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public List<ACColumnItem> GetColumns(int maxColumns = 9999, string acColumns = null)
        {
            return null;
        }
#endregion

#region IACValueType Member
        /// <summary>
        /// Gets the design.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="acUsage">The ac usage.</param>
        /// <param name="acKind">Kind of the ac.</param>
        /// <param name="vbDesignName">Name of the vb design.</param>
        /// <param name="msg">msg</param>
        /// <returns>ACClassDesign.</returns>
        public ACClassDesign GetDesign(IACObject acObject, Global.ACUsages acUsage, Global.ACKinds acKind, string vbDesignName = "", MsgWithDetails msg = null)
        {
            if ((this.ACUsage != acUsage) || (this.ACKind != acKind))
                return null;
            if (String.IsNullOrEmpty(vbDesignName) && (this.ACIdentifier != vbDesignName))
                return null;
            return this;
        }

        /// <summary>
        /// Gets my AC class design list.
        /// </summary>
        /// <value>My AC class design list.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        [ACPropertyInfo(9999, "", "", "", true)]
        [NotMapped]
        public IEnumerable<ACClassDesign> Designs
        {
            get
            {
                return null;
                //throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Mies the AC class design.
        /// </summary>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <param name="forceRefreshFromDB"></param>
        /// <returns>ACClassDesign.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public ACClassDesign GetDesign(string acIdentifier, bool forceRefreshFromDB = false)
        {
            return this;
        }

        //public IEnumerable<ACConfiguration> LayoutConfiguration(IACInteractiveObject acElement)
        //{
        //    throw new NotImplementedException();
        //}
#endregion

#region IACConfigStore

        [NotMapped]
        public string ConfigStoreName
        {
            get
            {
                ACClassInfo acClassInfo = (ACClassInfo)Safe_ACClass.GetType().GetCustomAttributes(typeof(ACClassInfo), false)[0];
                string caption = Translator.GetTranslation(acClassInfo.ACCaptionTranslation);
                return caption;
            }
        }

        /// <summary>
        /// ACConfigKeyACUrl returns the relative Url to the "main table" in group a group of semantically related tables.
        /// This property is used when NewACConfig() is called. NewACConfig() creates a new IACConfig-Instance and set the IACConfig.KeyACUrl-Property with this ACConfigKeyACUrl.
        /// </summary>
        [NotMapped]
        public string ACConfigKeyACUrl
        {
            get
            {
                return ".\\ACClassDesign(" + this.ACIdentifier + ")";
            }
        }


        /// <summary>
        /// Creates and adds a new IACConfig-Entry to ConfigItemsSource.
        /// The implementing class creates a new entity object an add it to its "own Configuration-Table".
        /// It sets automatically the IACConfig.KeyACUrl-Property with this ACConfigKeyACUrl.
        /// </summary>
        /// <param name="acObject">Optional: Reference to another Entity-Object that should be related for this new configuration entry.</param>
        /// <param name="valueTypeACClass">The iPlus-Type of the "Value"-Property.</param>
        /// <param name="localConfigACUrl"></param>
        /// <returns>IACConfig as a new entry</returns>
        public IACConfig NewACConfig(IACObjectEntity acObject = null, gip.core.datamodel.ACClass valueTypeACClass = null, string localConfigACUrl = null)
        {
            ACClass acClass = acObject as ACClass;
            if (acClass == null)
                acClass = this.Safe_ACClass;

            ACClassConfig acConfig = null;
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                acConfig = ACClassConfig.NewACObject(this.Database, acClass);
                acConfig.KeyACUrl = ACConfigKeyACUrl;
                acConfig.LocalConfigACUrl = localConfigACUrl;
                acConfig.ValueTypeACClass = valueTypeACClass;
                acClass.ACClassConfig_ACClass.Add(acConfig);
            }
            ACConfigListCache.Add(acConfig);
            return acConfig;
        }


        /// <summary>Removes a configuration from ConfigurationEntries and the database-context.</summary>
        /// <param name="acObject">Entry as IACConfig</param>
        public void RemoveACConfig(IACConfig acObject)
        {
            ACClassConfig acConfig = acObject as ACClassConfig;
            if (acConfig == null)
                return;
            if (ACConfigListCache.Contains(acConfig))
                ACConfigListCache.Remove(acConfig);
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                if (acConfig.ACClass == this.Safe_ACClass)
                    this.Safe_ACClass.ACClassConfig_ACClass.Remove(acConfig);
                else
                    acConfig.ACClass.ACClassConfig_ACClass.Remove(acConfig);
                if (acConfig.EntityState != EntityState.Detached)
                    acConfig.DeleteACObject(this.Database, false);
            }
        }

        /// <summary>
        /// Deletes all IACConfig-Entries in the Database-Context as well as in ConfigurationEntries.
        /// </summary>
        public void DeleteAllConfig()
        {
            if (!ACConfigListCache.Any())
                return;
            // Clear for reloading from database
            ClearCacheOfConfigurationEntries();
            var list = ACConfigListCache.ToArray();
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                foreach (var acConfig in list)
                {
                    (acConfig as ACClassConfig).DeleteACObject(this.Database, false);
                }
            }
            ClearCacheOfConfigurationEntries();
        }

        [NotMapped]
        public decimal OverridingOrder { get; set; }

        [NotMapped]
        private SafeList<IACConfig> _ACConfigListCache;
        [NotMapped]
        private SafeList<IACConfig> ACConfigListCache
        {
            get
            {
                using (ACMonitor.Lock(_10020_LockValue))
                {
                    if (_ACConfigListCache != null)
                        return _ACConfigListCache;
                }
                GetConfigListOfType();
                return _ACConfigListCache;
            }
        }

        /// <summary>
        /// A thread-safe and cached list of Configuration-Values of type IACConfig.
        /// </summary>
        [NotMapped]
        public IEnumerable<IACConfig> ConfigurationEntries
        {
            get
            {
                return ACConfigListCache;
            }
        }

        /// <summary>Clears the cache of configuration entries. (ConfigurationEntries)
        /// Re-accessing the ConfigurationEntries property rereads all configuration entries from the database.</summary>
        public void ClearCacheOfConfigurationEntries()
        {
            using (ACMonitor.Lock(_10020_LockValue))
            {
                _ACConfigListCache = null;
            }
        }

        /// <summary>
        /// Checks if cached configuration entries are loaded from database successfully
        /// </summary>
        public bool ValidateConfigurationEntriesWithDB(ConfigEntriesValidationMode mode = ConfigEntriesValidationMode.AnyCheck)
        {
            if (mode == ConfigEntriesValidationMode.AnyCheck)
            {
                if (ConfigurationEntries.Any())
                    return true;
            }
            using (Database database = new Database())
            {
                var query = database.ACClassConfig.Where(c => c.ACClassID == this.ACClassID && c.KeyACUrl == ACConfigKeyACUrl);
                if (mode == ConfigEntriesValidationMode.AnyCheck)
                {
                    if (query.Any())
                        return false;
                }
                else if (mode == ConfigEntriesValidationMode.CompareCount
                         || mode == ConfigEntriesValidationMode.CompareContent)
                    return query.Count() == ConfigurationEntries.Count();
            }
            return true;
        }

        /// <summary>
        /// Gets the class config list.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <returns>IEnumerable{IACConfig}.</returns>
        public IEnumerable<IACConfig> GetConfigListOfType(IACObjectEntity acObject = null)
        {
            ACClass acClass = acObject as ACClass;
            if (acClass == null)
                acClass = this.Safe_ACClass;
            if (acClass != this.Safe_ACClass)
            {
                using (ACMonitor.Lock(acClass.Database.QueryLock_1X000))
                {
                    if (acClass.ACClassConfig_ACClassReference.IsLoaded)
                    {
                        acClass.ACClassConfig_ACClass.AutoRefresh(acClass.ACClassConfig_ACClassReference, acClass.Database);
                        acClass.ACClassConfig_ACClass.AutoLoad(ACClass.ACClassConfig_ACClassReference, this);
                    }
                    return acClass.ACClassConfig_ACClass.ToList().Select(x => (IACConfig)x).Where(c => c.KeyACUrl == ACConfigKeyACUrl);
                }
            }

            using (ACMonitor.Lock(_10020_LockValue))
            {
                if (_ACConfigListCache != null)
                    return _ACConfigListCache;
            }

            SafeList<IACConfig> newSafeList = new SafeList<IACConfig>();
            using (ACMonitor.Lock(this.ACClass.Database.QueryLock_1X000))
            {
                if (this.ACClass.ACClassConfig_ACClassReference.IsLoaded)
                {
                    this.ACClass.ACClassConfig_ACClass.AutoRefresh(this.ACClass.ACClassConfig_ACClassReference, this.ACClass.Database);
                    this.ACClass.ACClassConfig_ACClass.AutoLoad(this.ACClass.ACClassConfig_ACClassReference, this);
                }
                newSafeList = new SafeList<IACConfig>(this.ACClass.ACClassConfig_ACClass.ToList().Select(x => (IACConfig)x).Where(c => c.KeyACUrl == ACConfigKeyACUrl));
            }
            using (ACMonitor.Lock(_10020_LockValue))
            {
                _ACConfigListCache = newSafeList;
                return _ACConfigListCache;
            }
        }


#endregion

#region Convention memebers implementation

        public override string ToString()
        {
            return ACCaption;
        }

#endregion

#region Others

        [NotMapped]
        public Database Database
        {
            get
            {
                return Context as Database;
            }
        }

        /// <summary>
        /// Checks if design compiled (compares BAMLDate with UpdateDate).  
        /// </summary>
        [ACPropertyInfo(14, "IsDesignCompiled", "en{'Compiled'}de{'Kompilieren'}", "", false)]
        [NotMapped]
        public bool IsDesignCompiled
        {
            get
            {
                if (BAMLDate != null && BAMLDesign != null)
                {
                    Nullable<TimeSpan> diff = UpdateDate - BAMLDate;
                    if (((TimeSpan)diff).Seconds == 0)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }

#endregion

#region Clone
        public object Clone()
        {
            ACClassDesign clonedObject = new ACClassDesign();
            clonedObject.ACClassDesignID = this.ACClassDesignID;
            clonedObject.ACClassID = this.ACClassID;
            clonedObject.ACIdentifier = this.ACIdentifier;
            clonedObject.ACIdentifierKey = this.ACIdentifierKey;
            clonedObject.ACCaptionTranslation = this.ACCaptionTranslation;
            clonedObject.ACGroup = this.ACGroup;
            clonedObject.XMLDesign = this.XMLDesign;
            clonedObject.DesignBinary = this.DesignBinary;
            clonedObject.DesignNo = this.DesignNo;
            clonedObject.ValueTypeACClassID = this.ValueTypeACClassID;
            clonedObject.ACKindIndex = this.ACKindIndex;
            clonedObject.ACUsageIndex = this.ACUsageIndex;
            clonedObject.SortIndex = this.SortIndex;
            clonedObject.IsRightmanagement = this.IsRightmanagement;
            clonedObject.Comment = this.Comment;
            clonedObject.IsDefault = this.IsDefault;
            clonedObject.IsResourceStyle = this.IsResourceStyle;
            clonedObject.VisualHeight = this.VisualHeight;
            clonedObject.VisualWidth = this.VisualWidth;
            clonedObject.XMLConfig = this.XMLConfig;
            clonedObject.BranchNo = this.BranchNo;
            clonedObject.DesignerMaxRecursion = this.DesignerMaxRecursion;
            clonedObject.BAMLDesign = this.BAMLDesign;
            clonedObject.BAMLDate = this.BAMLDate;
            return clonedObject;
        }
#endregion


        internal ACClass Safe_ACClass
        {
            get
            {

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return this.ACClass;
                }
            }
        }
    }
}
