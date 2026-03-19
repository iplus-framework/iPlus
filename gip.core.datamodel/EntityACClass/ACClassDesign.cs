// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
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
using SkiaSharp;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
    [ACPropertyEntity(3, nameof(ACIdentifierKey), "en{'Key'}de{'Schlüssel'}","", "", true)]
    [ACPropertyEntity(4, ACClass.ClassName, "en{'Class'}de{'Klasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(5, Const.ACGroup, "en{'Group'}de{'Gruppe'}","", "", true)]
    [ACPropertyEntity(6, Const.ACKindIndex, "en{'Design Type'}de{'Designart'}", typeof(Global.ACKinds), Const.ContextDatabaseIPlus + "\\ACKindDSList", "", true)]
    [ACPropertyEntity(7, nameof(ACUsageIndex), "en{'Usage'}de{'Verwendung'}", typeof(Global.ACUsages), Const.ContextDatabaseIPlus + "\\ACUsageList", "", true)]
    [ACPropertyEntity(8, nameof(SortIndex), "en{'Sortindex'}de{'Sortierung'}","", "", true)]
    [ACPropertyEntity(9, nameof(IsRightmanagement), "en{'Rights Management'}de{'Rechteverwaltung'}","", "", true)]
    [ACPropertyEntity(10, nameof(IsDefault), "en{'Default'}de{'Standard'}","", "", true)]
    [ACPropertyEntity(11, nameof(Comment), "en{'Comment'}de{'Bemerkung'}","", "", true)]
    [ACPropertyEntity(12, nameof(ValueTypeACClass), "en{'Datatype'}de{'Datentyp'}", Const.ContextDatabaseIPlus + "\\VBControlACClassList", "", true)]
    [ACPropertyEntity(14, nameof(DesignerMaxRecursion), "en{'Max. Recursion Designerobjects'}de{'Max. Rekursionstiefe Designerobjekte'}","", "", true)]
    [ACPropertyEntity(9999, nameof(ACCaptionTranslation), "en{'Translation'}de{'Übersetzung'}","", "", true)]
    [ACPropertyEntity(9999, nameof(DesignBinary), "en{'Binary code'}de{'Binärcode'}","", "", true)]
    [ACPropertyEntity(9999, nameof(DesignNo), "en{'Design No.'}de{'Designnr.'}","", "", true)]
    [ACPropertyEntity(9999, nameof(XMLDesign), "en{'Design'}de{'Design'}")]
    [ACPropertyEntity(9999, nameof(XMLDesign2), "en{'Design Avalonia'}de{'Design Avalonia'}")]
    [ACPropertyEntity(9999, nameof(IsResourceStyle), "en{'Resourcestyle'}de{'WPF-Resource'}","", "", true)]
    [ACPropertyEntity(9999, nameof(VisualHeight), "en{'Height'}de{'Höhe'}","", "", true)]
    [ACPropertyEntity(9999, nameof(VisualWidth), "en{'Width'}de{'Breite'}","", "", true)]
    [ACPropertyEntity(9999, nameof(XMLDesignUpdateDate), "en{'XML Design Update Date'}de{'Datum der Aktualisierung des XML-Designs'}", "", "", true)]
    [ACPropertyEntity(9999, nameof(XMLDesign2UpdateDate), "en{'XML Design 2 Update Date'}de{'Datum der Aktualisierung des XML-Designs2'}", "", "", true)]
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
            entity.XMLDesign2 = "";
            entity.DesignNo = secondaryKey;
            entity.BranchNo = 0;

            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            if (parentACObject is ACClass)
            {
                entity.ACClassID = (parentACObject as ACClass).ACClassID;
                entity.ACClass = parentACObject as ACClass;
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
            entity.XAMLDesign = "<?xml version=\"1.0\" encoding=\"utf-16\"?><vb:VBScrollViewer  " + ACxmlnsResolver.XMLNamespaces + " VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Auto\"><vb:VBCanvas Height=\"100\" Width=\"100\" Background=\"#FF000000\"></vb:VBCanvas></vb:VBScrollViewer>";
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

            base.DeleteACObject(database, withCheck, softDelete);

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
                    using (var ms = new MemoryStream(DesignBinary))
                    {
                        using (SKBitmap image = SKBitmap.Decode(ms))
                        {
                            if (image != null)
                            {
                                switch (image.ColorType)
                                {
                                    case SKColorType.Rgba8888:
                                    case SKColorType.Bgra8888:
                                    case SKColorType.Rgb888x:
                                        return reportFileName + ".png";
                                    case SKColorType.Rgb565:
                                        return reportFileName + ".jpeg";
                                    case SKColorType.Gray8:
                                        return reportFileName + ".bmp";
                                }
                            }
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
                if (this.ACKind != Global.ACKinds.DSDesignMenu || string.IsNullOrEmpty(XMLDesign))
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
                    if (acClass.ACClassConfig_ACClass_IsLoaded && !acClass.Database.IsChanged)
                    {
                        //acClass.ACClassConfig_ACClass.AutoRefresh(acClass.ACClassConfig_ACClassReference, acClass.Database);
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
                if (this.ACClass.ACClassConfig_ACClass_IsLoaded)
                {
                    //this.ACClass.ACClassConfig_ACClass.AutoRefresh(this.ACClass.ACClassConfig_ACClassReference, this.ACClass.Database);
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

        [NotMapped]
        private bool _StoreDesignInXML2;
        [ACPropertyInfo(10000, "IsDesignCompiled", "en{'Edit Avalonia/WPF Design'}de{'Avalonia/WPF Design ändern'}", "", false, IsPersistable = false)]
        [NotMapped]
        public bool StoreDesignInXML2
        {
            get
            {
                return _StoreDesignInXML2;
            }
            set
            {
                // this can only be activated in WPF mode
                // if (Database.Root.IsAvaloniaUI)
                //     return;
                if (this.SetProperty(ref _StoreDesignInXML2, value))
                    OnPropertyChanged(nameof(XAMLDesign));
            }
        }

        [ACPropertyInfo(10000, "XAMLDesign", "en{'XAML Design'}de{'XAML Design'}", "", false, IsPersistable = false)]
        [NotMapped]
        public string XAMLDesign
        {
            get
            {
                // If UI is Avalonia 
                if (Database.Root.IsAvaloniaUI)
                {
                    // Return WPF Design
                    if (StoreDesignInXML2)
                        return XMLDesign;
                    else
                    {
                        // Return cached Avalonia XAML if available
                        if (!string.IsNullOrEmpty(XMLDesign2))
                            return XMLDesign2;
                        // Otherwise convert from WPF XAML
                        return ConvertWpfToAvaloniaXaml(XMLDesign);
                    }
                }
                // If UI is WPF
                else
                {
                    // Return Avalonia Design
                    if (StoreDesignInXML2)
                        return XMLDesign2;
                    // Return WPF Design
                    return XMLDesign;
                }
            }
            set
            {
                if (Database.Root.IsAvaloniaUI)
                {
                    // Store Changes in WPF Design
                    if (StoreDesignInXML2)
                    {
                        XMLDesign = value;
                        XMLDesignUpdateDate = DateTime.Now;
                    }
                    // Else Avalonia Design  (default)
                    else
                    {
                        XMLDesign2 = value;
                        XMLDesign2UpdateDate = DateTime.Now;
                    }
                }
                else
                {
                    // Store Changes in Avalonia Design
                    if (StoreDesignInXML2)
                    {
                        XMLDesign2 = value;
                        XMLDesign2UpdateDate = DateTime.Now;
                    }
                    // Else Changes in WPF Design (default)
                    else
                    {
                        XMLDesign = value;
                        XMLDesignUpdateDate = DateTime.Now;
                    }
                }
            }
        }

        [ACPropertyInfo(14, "AreDesignsSynchronized", "en{'Designs Synchronized'}de{'Designs Synchronisiert'}", "", false)]
        [NotMapped]
        public bool AreDesignsSynchronized
        {
            get
            {
                return XMLDesignUpdateDate.HasValue && XMLDesign2UpdateDate.HasValue && XMLDesignUpdateDate.Value == XMLDesign2UpdateDate.Value;
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(XMLDesign) || propertyName == nameof(XMLDesign2))
            {
                OnPropertyChanged(nameof(XAMLDesign));
                OnPropertyChanged(nameof(AreDesignsSynchronized));
            }
            if (propertyName == nameof(XMLDesignUpdateDate) || propertyName == nameof(XMLDesign2UpdateDate))
            {
                OnPropertyChanged(nameof(AreDesignsSynchronized));
            }
            base.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Converts WPF XAML to Avalonia XAML using namespace mappings and find/replace patterns.
        /// This is a unified conversion method used by both ACClassDesign and VBUserACClassDesign.
        /// </summary>
        /// <param name="wpfXaml">The WPF XAML string to convert</param>
        /// <returns>Converted Avalonia XAML string</returns>
        public static string ConvertWpfToAvaloniaXaml(string wpfXaml)
        {
            if (string.IsNullOrEmpty(wpfXaml))
                return wpfXaml;

            string avaloniaXAML = wpfXaml;
            
            // Apply namespace mappings
            foreach (var tuple in ACxmlnsResolver.C_AvaloniaNamespaceMapping)
            {
                avaloniaXAML = avaloniaXAML?.Replace(tuple.WpfNamespace, tuple.AvaloniaNamespace);
            }
            
            // Apply regex and string replacements
            foreach (var tuple in C_AvaloniaFindAndReplace)
            {
                if (tuple.IsRegex)
                {
                    avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", tuple.WpfPattern, tuple.AvaloniaReplacement, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                }
                else
                {
                    avaloniaXAML = avaloniaXAML?.Replace(tuple.WpfPattern, tuple.AvaloniaReplacement);
                }
            }
            
            // Handle xmlns removal: split at first root element close, process child content only
            // This preserves xmlns on root element but removes from child elements
            if (!string.IsNullOrEmpty(avaloniaXAML))
            {
                // Find the end of the first root element tag (after all xmlns declarations)
                // Pattern matches up to first real element tag (not XML declarations like <?xml...?>)
                var rootEndMatch = Regex.Match(avaloniaXAML, @"^([\s\S]*?<[a-zA-Z][\w:]*[^>]*?>)([\s\S]*)$");
                if (rootEndMatch.Success)
                {
                    string rootPart = rootEndMatch.Groups[1].Value;
                    string childContent = rootEndMatch.Groups[2].Value;
                    
                    // Remove all xmlns declarations from child content only
                    childContent = Regex.Replace(childContent, @"\s+xmlns:?\w*=""[^""]*""", "", RegexOptions.IgnoreCase);
                    
                    // Remove attributes' namespace prefix for prefixes used by element names (e.g. vb: in <vb:VBButton vb:VBContent="...">).
                    // This also covers unprefixed roots like <Grid ...> where the first prefixed element appears in child content.
                    // We skip x/xml/xmlns to preserve standard XAML/XML behavior.
                    var elementPrefixMatches = Regex.Matches(rootPart + childContent, @"<([a-zA-Z][\w.]*)\:[a-zA-Z][\w.]*");
                    var elementPrefixes = elementPrefixMatches
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Where(prefix =>
                            !string.Equals(prefix, "x", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(prefix, "xml", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(prefix, "xmlns", StringComparison.OrdinalIgnoreCase));

                    foreach (string elementPrefix in elementPrefixes)
                    {
                        // Pattern matches the prefix only if the attribute name does NOT contain a dot.
                        // This ensures simple attributes like vb:VBContent become VBContent,
                        // but attached properties like vb:VBDockingManager.IsCloseableBSORoot are preserved.
                        string prefixAttrPattern = @"(\s+)" + Regex.Escape(elementPrefix) + @":([\w]+)(=)";
                        rootPart = Regex.Replace(rootPart, prefixAttrPattern, "$1$2$3", RegexOptions.IgnoreCase);
                        childContent = Regex.Replace(childContent, prefixAttrPattern, "$1$2$3", RegexOptions.IgnoreCase);
                    }

                    avaloniaXAML = rootPart + childContent;
                }
            }

            // Convert decimal CenterX/Y to percentage (e.g., 0.669 -> 67)
            // This handles the conversion from WPF's 0.0-1.0 coordinate system to Avalonia's percentage-based values where required
            avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", @"\b(Center[XY])=""0\.(\d+)""", m =>
            {
                string attr = m.Groups[1].Value;
                string decimals = m.Groups[2].Value;
                if (double.TryParse("0." + decimals, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                {
                    int percent = (int)Math.Round(val * 100);
                    return $"{attr}=\"{percent}\"";
                }
                return m.Value;
            }, RegexOptions.IgnoreCase);

            // Convert decimal StartPoint/EndPoint to percentage (e.g., 0.46,1.0 -> 46%,100%)
            // This handles the conversion of relative points in brushes to percentage-based strings required for Avalonia
            avaloniaXAML = Regex.Replace(avaloniaXAML ?? "", @"\b(StartPoint|EndPoint)=""([^""]+)""", m =>
            {
                string attr = m.Groups[1].Value;
                string points = m.Groups[2].Value;
                var coords = points.Split(',');
                if (coords.Length == 2)
                {
                    if (double.TryParse(coords[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double x) &&
                        double.TryParse(coords[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double y))
                    {
                        int px = (int)Math.Round(x * 100);
                        int py = (int)Math.Round(y * 100);
                        return $"{attr}=\"{px}%,{py}%\"";
                    }
                }
                return m.Value;
            }, RegexOptions.IgnoreCase);

            // Avalonia expects RadialGradientBrush radii/origin/center in percent notation.
            avaloniaXAML = ConvertRadialGradientBrushValuesToPercent(avaloniaXAML);

            // Convert WPF resource dictionary includes to Avalonia ResourceInclude + avares:// URI syntax.
            avaloniaXAML = ConvertResourceDictionarySourceToResourceInclude(avaloniaXAML);

            // Avalonia parser is strict for VBBinding argument names.
            // Convert {vb:VBBinding vb:VBContent=...} -> {vb:VBBinding VBContent=...}.
            avaloniaXAML = RemovePrefixedVBBindingParameterNames(avaloniaXAML);

            // RelativeTransform is no longer supported for brushes in Avalonia.
            // Remove both attribute usage and *.RelativeTransform property elements.
            avaloniaXAML = RemoveRelativeTransforms(avaloniaXAML);

            // Convert WPF Line coordinates to Avalonia points:
            // <Line X1="0" Y1="6" X2="60" Y2="6" ... /> -> <Line StartPoint="0,6" EndPoint="60,6" ... />
            // Run after StartPoint/EndPoint percentage conversion to avoid treating line coordinates as percentages.
            avaloniaXAML = ConvertLineCoordinatesToStartEndPoint(avaloniaXAML);

            // Avalonia Shape has StrokeLineCap (single value) instead of WPF's
            // StrokeStartLineCap/StrokeEndLineCap pair.
            avaloniaXAML = ConvertStrokeStartEndLineCapToStrokeLineCap(avaloniaXAML);

            // Convert WPF-style trigger blocks embedded in *.Style property elements into
            // Xaml.Behaviors-based triggers and copy default Setter values to owner attributes.
            avaloniaXAML = ConvertControlThemeTriggersToBehaviors(avaloniaXAML);
            
            return avaloniaXAML;
        }

        private static string ConvertControlThemeTriggersToBehaviors(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                if (doc.DocumentElement == null)
                    return xaml;

                string xamlNs = GetDefaultXamlNamespace(doc);
                if (string.IsNullOrEmpty(xamlNs))
                    return xaml;

                var stylePropertyNodes = doc.SelectNodes("//*[contains(local-name(), '.Style')]");
                if (stylePropertyNodes == null || stylePropertyNodes.Count == 0)
                    return xaml;

                var styleProperties = stylePropertyNodes
                    .OfType<XmlNode>()
                    .OfType<XmlElement>()
                    .ToList();

                foreach (var styleProperty in styleProperties)
                {
                    if (!styleProperty.LocalName.EndsWith(".Style", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var ownerElement = styleProperty.ParentNode as XmlElement;
                    if (ownerElement == null)
                        continue;

                    var controlTheme = styleProperty
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme", StringComparison.OrdinalIgnoreCase));

                    if (controlTheme == null)
                        continue;

                    var triggersElement = controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme.Triggers", StringComparison.OrdinalIgnoreCase));

                    // Copy default setter values from ControlTheme root and optional ControlTheme.Setters block.
                    var defaultSetters = new List<XmlElement>();
                    defaultSetters.AddRange(controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)));

                    var settersContainer = controlTheme
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ControlTheme.Setters", StringComparison.OrdinalIgnoreCase));

                    if (settersContainer != null)
                    {
                        defaultSetters.AddRange(settersContainer
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)));
                    }

                    int movedDefaultSetterCount = 0;
                    foreach (var setter in defaultSetters)
                    {
                        var propertyName = setter.GetAttribute("Property");
                        var propertyValue = setter.GetAttribute("Value");
                        if (string.IsNullOrWhiteSpace(propertyName))
                            continue;

                        // Only convert plain properties to owner attributes.
                        if (propertyName.Contains(".") || propertyName.Contains(":"))
                            continue;

                        propertyName = NormalizeTriggerPropertyName(propertyName);
                        propertyValue = NormalizeTriggerPropertyValue(propertyName, propertyValue);

                        if (!ownerElement.HasAttribute(propertyName))
                        {
                            ownerElement.SetAttribute(propertyName, propertyValue);
                            movedDefaultSetterCount++;
                        }
                    }

                    if (triggersElement == null)
                    {
                        // Clean up redundant style wrappers:
                        // 1) setters were successfully moved to owner attributes, or
                        // 2) style has no default setters at all.
                        if (movedDefaultSetterCount > 0 || defaultSetters.Count == 0)
                        {
                            ownerElement.RemoveChild(styleProperty);
                        }

                        continue;
                    }

                    var interactionBehaviors = doc.CreateElement("Interaction.Behaviors", xamlNs);
                    bool hasBehavior = false;

                    foreach (var dataTrigger in triggersElement
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "DataTrigger", StringComparison.OrdinalIgnoreCase)))
                    {
                        var behavior = doc.CreateElement("DataTriggerBehavior", xamlNs);

                        foreach (var attr in dataTrigger.Attributes.OfType<XmlAttribute>())
                        {
                            behavior.SetAttribute(attr.Name, attr.Value);
                        }

                        int actionCount = 0;
                        foreach (var setter in dataTrigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)))
                        {
                            var propertyName = setter.GetAttribute("Property");
                            if (string.IsNullOrWhiteSpace(propertyName))
                                continue;

                            // ChangePropertyAction expects simple target property names.
                            if (propertyName.Contains(".") || propertyName.Contains(":"))
                                continue;

                            propertyName = NormalizeTriggerPropertyName(propertyName);
                            var propertyValue = NormalizeTriggerPropertyValue(propertyName, setter.GetAttribute("Value"));

                            var action = doc.CreateElement("ChangePropertyAction", xamlNs);
                            action.SetAttribute("PropertyName", propertyName);
                            action.SetAttribute("Value", propertyValue);
                            behavior.AppendChild(action);
                            actionCount++;
                        }

                        if (actionCount > 0)
                        {
                            interactionBehaviors.AppendChild(behavior);
                            hasBehavior = true;
                        }
                    }

                    foreach (var multiDataTrigger in triggersElement
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "MultiDataTrigger", StringComparison.OrdinalIgnoreCase)))
                    {
                        var conditionsElement = multiDataTrigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .FirstOrDefault(e => string.Equals(e.LocalName, "MultiDataTrigger.Conditions", StringComparison.OrdinalIgnoreCase));

                        if (conditionsElement == null)
                            continue;

                        var conditionElements = conditionsElement
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "Condition", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (conditionElements.Count == 0)
                            continue;

                        var behavior = doc.CreateElement("DataTriggerBehavior", xamlNs);
                        behavior.SetAttribute("Value", "True");

                        var behaviorBindingProperty = doc.CreateElement("DataTriggerBehavior.Binding", xamlNs);
                        var multiBinding = doc.CreateElement("MultiBinding", xamlNs);
                        multiBinding.SetAttribute("Converter", "{x:Static BoolConverters.And}");

                        bool allConditionsConverted = true;
                        foreach (var condition in conditionElements)
                        {
                            var conditionBinding = condition.GetAttribute("Binding");
                            if (string.IsNullOrWhiteSpace(conditionBinding))
                            {
                                allConditionsConverted = false;
                                break;
                            }

                            var bindingElement = CreateBindingElementFromMarkup(doc, xamlNs, conditionBinding);
                            if (bindingElement == null)
                            {
                                allConditionsConverted = false;
                                break;
                            }

                            string expectedValue = condition.GetAttribute("Value");
                            bool expectedIsTrue = IsTrueLiteral(expectedValue);
                            bool hasConverter = BindingElementHasConverter(bindingElement);

                            // Preserve existing converter behavior for boolean True checks.
                            // For value comparisons, attach ObjectEqualsConverter if none exists.
                            if (!expectedIsTrue)
                            {
                                if (hasConverter)
                                {
                                    allConditionsConverted = false;
                                    break;
                                }

                                var converterProperty = CreateElementWithResolvedNamespace(doc, xamlNs, $"{bindingElement.Name}.Converter");
                                var objectEqualsConverter = CreateElementWithResolvedNamespace(doc, xamlNs, "vb:ObjectEqualsConverter");
                                if (converterProperty == null || objectEqualsConverter == null)
                                {
                                    allConditionsConverted = false;
                                    break;
                                }

                                converterProperty.AppendChild(objectEqualsConverter);
                                bindingElement.AppendChild(converterProperty);

                                if (!string.IsNullOrWhiteSpace(expectedValue))
                                {
                                    bindingElement.SetAttribute("ConverterParameter", expectedValue);
                                }
                            }

                            multiBinding.AppendChild(bindingElement);
                        }

                        if (!allConditionsConverted)
                            continue;

                        behaviorBindingProperty.AppendChild(multiBinding);
                        behavior.AppendChild(behaviorBindingProperty);

                        int actionCount = 0;
                        foreach (var setter in multiDataTrigger
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "Setter", StringComparison.OrdinalIgnoreCase)))
                        {
                            var propertyName = setter.GetAttribute("Property");
                            if (string.IsNullOrWhiteSpace(propertyName))
                                continue;

                            if (propertyName.Contains(".") || propertyName.Contains(":"))
                                continue;

                            propertyName = NormalizeTriggerPropertyName(propertyName);
                            var propertyValue = NormalizeTriggerPropertyValue(propertyName, setter.GetAttribute("Value"));

                            var action = doc.CreateElement("ChangePropertyAction", xamlNs);
                            action.SetAttribute("PropertyName", propertyName);
                            action.SetAttribute("Value", propertyValue);
                            behavior.AppendChild(action);
                            actionCount++;
                        }

                        if (actionCount > 0)
                        {
                            interactionBehaviors.AppendChild(behavior);
                            hasBehavior = true;
                        }
                    }

                    if (!hasBehavior)
                        continue;

                    ownerElement.ReplaceChild(interactionBehaviors, styleProperty);
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string NormalizeTriggerPropertyName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return propertyName;

            if (string.Equals(propertyName, "Visibility", StringComparison.OrdinalIgnoreCase))
                return "IsVisible";

            return propertyName;
        }

        private static string NormalizeTriggerPropertyValue(string normalizedPropertyName, string propertyValue)
        {
            if (!string.Equals(normalizedPropertyName, "IsVisible", StringComparison.OrdinalIgnoreCase))
                return propertyValue;

            if (string.Equals(propertyValue, "Visible", StringComparison.OrdinalIgnoreCase))
                return "True";

            if (string.Equals(propertyValue, "Hidden", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(propertyValue, "Collapsed", StringComparison.OrdinalIgnoreCase))
                return "False";

            return propertyValue;
        }

        private static bool IsTrueLiteral(string value)
        {
            return string.IsNullOrWhiteSpace(value) ||
                   string.Equals(value, "True", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
        }

        private static bool BindingElementHasConverter(XmlElement bindingElement)
        {
            if (bindingElement == null)
                return false;

            if (bindingElement.HasAttribute("Converter"))
                return true;

            return bindingElement
                .ChildNodes
                .OfType<XmlElement>()
                .Any(e => e.LocalName.EndsWith(".Converter", StringComparison.OrdinalIgnoreCase));
        }

        private static XmlElement CreateBindingElementFromMarkup(XmlDocument doc, string xamlNs, string markup)
        {
            if (doc == null || string.IsNullOrWhiteSpace(markup))
                return null;

            var extension = ParseMarkupExtension(markup);
            if (extension == null || string.IsNullOrWhiteSpace(extension.TypeName))
                return null;

            var bindingElement = CreateElementWithResolvedNamespace(doc, xamlNs, extension.TypeName);
            if (bindingElement == null)
                return null;

            foreach (var kvp in extension.Properties)
            {
                if (string.Equals(kvp.Key, "Converter", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryAppendConverterFromMarkup(doc, xamlNs, bindingElement, kvp.Value))
                        return null;
                    continue;
                }

                bindingElement.SetAttribute(kvp.Key, kvp.Value);
            }

            return bindingElement;
        }

        private static bool TryAppendConverterFromMarkup(XmlDocument doc, string xamlNs, XmlElement owner, string converterMarkup)
        {
            if (doc == null || owner == null || string.IsNullOrWhiteSpace(converterMarkup))
                return false;

            var converterExtension = ParseMarkupExtension(converterMarkup);
            if (converterExtension == null || string.IsNullOrWhiteSpace(converterExtension.TypeName))
                return false;

            var converterProperty = CreateElementWithResolvedNamespace(doc, xamlNs, $"{owner.Name}.Converter");
            var converterElement = CreateElementWithResolvedNamespace(doc, xamlNs, converterExtension.TypeName);
            if (converterProperty == null || converterElement == null)
                return false;

            foreach (var kvp in converterExtension.Properties)
            {
                converterElement.SetAttribute(kvp.Key, kvp.Value);
            }

            converterProperty.AppendChild(converterElement);
            owner.AppendChild(converterProperty);
            return true;
        }

        private static XmlElement CreateElementWithResolvedNamespace(XmlDocument doc, string fallbackNamespace, string qualifiedName)
        {
            if (doc == null || string.IsNullOrWhiteSpace(qualifiedName))
                return null;

            string prefix = string.Empty;
            string localName = qualifiedName;

            int colonIndex = qualifiedName.IndexOf(':');
            if (colonIndex > 0 && colonIndex < qualifiedName.Length - 1)
            {
                prefix = qualifiedName.Substring(0, colonIndex);
                localName = qualifiedName.Substring(colonIndex + 1);
            }

            string namespaceUri = fallbackNamespace;
            if (doc.DocumentElement != null)
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    string defaultNamespace = GetDefaultXamlNamespace(doc);
                    if (!string.IsNullOrWhiteSpace(defaultNamespace))
                    {
                        namespaceUri = defaultNamespace;
                    }
                }
                else
                {
                    string mappedNamespace = doc.DocumentElement.GetNamespaceOfPrefix(prefix);
                    if (!string.IsNullOrWhiteSpace(mappedNamespace))
                    {
                        namespaceUri = mappedNamespace;
                    }
                }
            }

            return string.IsNullOrEmpty(prefix)
                ? doc.CreateElement(localName, namespaceUri)
                : doc.CreateElement(prefix, localName, namespaceUri);
        }

        private static string GetDefaultXamlNamespace(XmlDocument doc)
        {
            if (doc?.DocumentElement == null)
                return string.Empty;

            string defaultNamespace = doc.DocumentElement.GetNamespaceOfPrefix(string.Empty);
            if (!string.IsNullOrWhiteSpace(defaultNamespace))
                return defaultNamespace;

            return doc.DocumentElement.NamespaceURI ?? string.Empty;
        }

        private sealed class ParsedMarkupExtension
        {
            public string TypeName { get; set; }
            public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private static ParsedMarkupExtension ParseMarkupExtension(string markup)
        {
            if (string.IsNullOrWhiteSpace(markup))
                return null;

            string text = markup.Trim();
            if (!text.StartsWith("{", StringComparison.Ordinal) || !text.EndsWith("}", StringComparison.Ordinal))
                return null;

            text = text.Substring(1, text.Length - 2).Trim();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            int splitIndex = text.IndexOfAny(new[] { ' ', ',' });
            string typeName = splitIndex < 0 ? text : text.Substring(0, splitIndex).Trim();
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            var parsed = new ParsedMarkupExtension
            {
                TypeName = typeName
            };

            if (splitIndex < 0)
                return parsed;

            string propertyText = text.Substring(splitIndex).Trim();
            foreach (var part in SplitTopLevel(propertyText, ','))
            {
                string assignment = part?.Trim();
                if (string.IsNullOrWhiteSpace(assignment))
                    continue;

                int equalsIndex = assignment.IndexOf('=');
                if (equalsIndex <= 0 || equalsIndex >= assignment.Length - 1)
                    continue;

                string key = assignment.Substring(0, equalsIndex).Trim();
                string value = assignment.Substring(equalsIndex + 1).Trim();
                if (value.Length >= 2 && value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                if (!string.IsNullOrWhiteSpace(key))
                    parsed.Properties[key] = value;
            }

            return parsed;
        }

        private static List<string> SplitTopLevel(string input, char separator)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(input))
                return result;

            var sb = new StringBuilder();
            int braceDepth = 0;
            bool inQuotes = false;

            foreach (char ch in input)
            {
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    sb.Append(ch);
                    continue;
                }

                if (!inQuotes)
                {
                    if (ch == '{')
                        braceDepth++;
                    else if (ch == '}' && braceDepth > 0)
                        braceDepth--;

                    if (ch == separator && braceDepth == 0)
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }

                sb.Append(ch);
            }

            if (sb.Length > 0)
                result.Add(sb.ToString());

            return result;
        }

        private static string ConvertRadialGradientBrushValuesToPercent(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                var brushNodes = doc.SelectNodes("//*[local-name()='RadialGradientBrush']");
                if (brushNodes == null || brushNodes.Count == 0)
                    return xaml;

                foreach (var brush in brushNodes.OfType<XmlNode>().OfType<XmlElement>())
                {
                    ConvertPercentAttribute(brush, "RadiusX");
                    ConvertPercentAttribute(brush, "RadiusY");
                    ConvertPercentPointAttribute(brush, "Center");
                    ConvertPercentPointAttribute(brush, "GradientOrigin");
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string RemoveRelativeTransforms(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                // Remove property elements like <RadialGradientBrush.RelativeTransform>...</...>
                var relativeTransformElements = doc.SelectNodes("//*[contains(local-name(), '.RelativeTransform')]");
                if (relativeTransformElements != null)
                {
                    foreach (var element in relativeTransformElements.OfType<XmlNode>().OfType<XmlElement>().ToList())
                    {
                        if (element.LocalName.EndsWith(".RelativeTransform", StringComparison.OrdinalIgnoreCase))
                        {
                            element.ParentNode?.RemoveChild(element);
                        }
                    }
                }

                // Remove attribute usage: RelativeTransform="..."
                var allElements = doc.SelectNodes("//*");
                if (allElements != null)
                {
                    foreach (var element in allElements.OfType<XmlNode>().OfType<XmlElement>())
                    {
                        element.RemoveAttribute("RelativeTransform");
                    }
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ConvertResourceDictionarySourceToResourceInclude(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                string xamlNs = GetDefaultXamlNamespace(doc);
                if (string.IsNullOrEmpty(xamlNs))
                    return xaml;

                var resourcesNodes = doc.SelectNodes("//*[contains(local-name(), '.Resources')]");
                if (resourcesNodes == null || resourcesNodes.Count == 0)
                    return xaml;

                foreach (var resourcesElement in resourcesNodes.OfType<XmlNode>().OfType<XmlElement>())
                {
                    if (!resourcesElement.LocalName.EndsWith(".Resources", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var sourceDictionaries = resourcesElement
                        .ChildNodes
                        .OfType<XmlElement>()
                        .Where(e => string.Equals(e.LocalName, "ResourceDictionary", StringComparison.OrdinalIgnoreCase)
                                    && e.HasAttribute("Source"))
                        .ToList();

                    if (sourceDictionaries.Count == 0)
                        continue;

                    var includeSources = new List<string>();
                    foreach (var sourceDictionary in sourceDictionaries)
                    {
                        string source = sourceDictionary.GetAttribute("Source");
                        string avaresSource = ConvertWpfResourceSourceToAvares(source);
                        if (!string.IsNullOrWhiteSpace(avaresSource))
                        {
                            includeSources.Add(avaresSource);
                        }

                        resourcesElement.RemoveChild(sourceDictionary);
                    }

                    if (includeSources.Count == 0)
                        continue;

                    var targetDictionary = resourcesElement
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ResourceDictionary", StringComparison.OrdinalIgnoreCase));

                    if (targetDictionary == null)
                    {
                        targetDictionary = doc.CreateElement("ResourceDictionary", xamlNs);
                        resourcesElement.AppendChild(targetDictionary);
                    }

                    var mergedDictionaries = targetDictionary
                        .ChildNodes
                        .OfType<XmlElement>()
                        .FirstOrDefault(e => string.Equals(e.LocalName, "ResourceDictionary.MergedDictionaries", StringComparison.OrdinalIgnoreCase));

                    if (mergedDictionaries == null)
                    {
                        mergedDictionaries = doc.CreateElement("ResourceDictionary.MergedDictionaries", xamlNs);
                        targetDictionary.AppendChild(mergedDictionaries);
                    }

                    var existingSources = new HashSet<string>(
                        mergedDictionaries
                            .ChildNodes
                            .OfType<XmlElement>()
                            .Where(e => string.Equals(e.LocalName, "ResourceInclude", StringComparison.OrdinalIgnoreCase))
                            .Select(e => e.GetAttribute("Source"))
                            .Where(s => !string.IsNullOrWhiteSpace(s)),
                        StringComparer.OrdinalIgnoreCase);

                    foreach (string source in includeSources)
                    {
                        if (existingSources.Contains(source))
                            continue;

                        var resourceInclude = doc.CreateElement("ResourceInclude", xamlNs);
                        resourceInclude.SetAttribute("Source", source);
                        mergedDictionaries.AppendChild(resourceInclude);
                        existingSources.Add(source);
                    }
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ConvertWpfResourceSourceToAvares(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            string normalized = source.Trim().Replace('\\', '/');
            if (normalized.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeAvaresExtension(normalized);
            }

            const string packPrefix = "pack://application:,,,/";
            if (normalized.StartsWith(packPrefix, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(packPrefix.Length);
            }

            if (normalized.StartsWith("/", StringComparison.Ordinal))
            {
                normalized = normalized.Substring(1);
            }

            int componentIndex = normalized.IndexOf(";component/", StringComparison.OrdinalIgnoreCase);
            if (componentIndex <= 0)
                return null;

            string assemblyName = normalized.Substring(0, componentIndex);
            string path = normalized.Substring(componentIndex + ";component/".Length).TrimStart('/');

            if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(path))
                return null;

            if (!assemblyName.EndsWith(".avui", StringComparison.OrdinalIgnoreCase))
            {
                assemblyName += ".avui";
            }

            if (path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - ".xaml".Length) + ".axaml";
            }

            return $"avares://{assemblyName}/{path}";
        }

        private static string NormalizeAvaresExtension(string avaresUri)
        {
            if (string.IsNullOrWhiteSpace(avaresUri))
                return avaresUri;

            if (avaresUri.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                return avaresUri.Substring(0, avaresUri.Length - ".xaml".Length) + ".axaml";
            }

            return avaresUri;
        }

        private static string RemovePrefixedVBBindingParameterNames(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                // Match {vb:VBBinding ...} and keep nested single-level braces in arguments.
                return Regex.Replace(
                    xaml,
                    @"\{vb:VBBinding(?<body>(?:[^{}]|\{[^{}]*\})*)\}",
                    m =>
                    {
                        string body = m.Groups["body"].Value;
                        if (string.IsNullOrEmpty(body))
                            return m.Value;

                        // Remove namespace prefixes from argument names only:
                        // " vb:VBContent=..." -> " VBContent=..."
                        string normalizedBody = Regex.Replace(
                            body,
                            @"(^|[\s,])(?:[a-zA-Z_][\w.]*)\:([a-zA-Z_][\w.]*)\s*=",
                            "$1$2=",
                            RegexOptions.Singleline);

                        return "{vb:VBBinding" + normalizedBody + "}";
                    },
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static void ConvertPercentAttribute(XmlElement element, string attributeName)
        {
            if (element == null || string.IsNullOrWhiteSpace(attributeName) || !element.HasAttribute(attributeName))
                return;

            var raw = element.GetAttribute(attributeName);
            var converted = ConvertNumericValueToPercent(raw);
            if (!string.IsNullOrWhiteSpace(converted))
            {
                element.SetAttribute(attributeName, converted);
            }
        }

        private static void ConvertPercentPointAttribute(XmlElement element, string attributeName)
        {
            if (element == null || string.IsNullOrWhiteSpace(attributeName) || !element.HasAttribute(attributeName))
                return;

            var raw = element.GetAttribute(attributeName);
            if (string.IsNullOrWhiteSpace(raw))
                return;

            var parts = raw.Split(',');
            if (parts.Length != 2)
                return;

            var x = ConvertNumericValueToPercent(parts[0]);
            var y = ConvertNumericValueToPercent(parts[1]);
            if (string.IsNullOrWhiteSpace(x) || string.IsNullOrWhiteSpace(y))
                return;

            element.SetAttribute(attributeName, $"{x},{y}");
        }

        private static string ConvertNumericValueToPercent(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            string text = raw.Trim();
            if (text.EndsWith("%", StringComparison.Ordinal))
                return text;

            if (!double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value))
                return null;

            // WPF commonly stores these values as 0..1; Avalonia expects percent.
            // If the value already appears to be a percent number (e.g. 50), keep the magnitude.
            double percent = value <= 1d ? value * 100d : value;
            int rounded = (int)Math.Round(percent);
            return $"{rounded}%";
        }

        private static string ConvertLineCoordinatesToStartEndPoint(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                var lineNodes = doc.SelectNodes("//*[local-name()='Line']");
                if (lineNodes == null || lineNodes.Count == 0)
                    return xaml;

                foreach (var line in lineNodes.OfType<XmlNode>().OfType<XmlElement>())
                {
                    var x1 = line.GetAttribute("X1");
                    var y1 = line.GetAttribute("Y1");
                    var x2 = line.GetAttribute("X2");
                    var y2 = line.GetAttribute("Y2");

                    if (!line.HasAttribute("StartPoint") && line.HasAttribute("X1") && line.HasAttribute("Y1"))
                    {
                        line.SetAttribute("StartPoint", $"{x1},{y1}");
                    }

                    if (!line.HasAttribute("EndPoint") && line.HasAttribute("X2") && line.HasAttribute("Y2"))
                    {
                        line.SetAttribute("EndPoint", $"{x2},{y2}");
                    }

                    // Remove obsolete WPF line coordinate attributes after conversion.
                    line.RemoveAttribute("X1");
                    line.RemoveAttribute("Y1");
                    line.RemoveAttribute("X2");
                    line.RemoveAttribute("Y2");
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        private static string ConvertStrokeStartEndLineCapToStrokeLineCap(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                return xaml;

            try
            {
                var doc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(xaml);

                // Any element can derive from Shape, so inspect all elements for legacy attributes.
                var elements = doc.SelectNodes("//*");
                if (elements == null || elements.Count == 0)
                    return xaml;

                foreach (var element in elements.OfType<XmlNode>().OfType<XmlElement>())
                {
                    bool hasStart = element.HasAttribute("StrokeStartLineCap");
                    bool hasEnd = element.HasAttribute("StrokeEndLineCap");
                    if (!hasStart && !hasEnd)
                        continue;

                    // Keep existing StrokeLineCap if already present; otherwise map from legacy attributes.
                    if (!element.HasAttribute("StrokeLineCap"))
                    {
                        string start = element.GetAttribute("StrokeStartLineCap");
                        string end = element.GetAttribute("StrokeEndLineCap");

                        string mappedValue = !string.IsNullOrWhiteSpace(start) ? start : end;
                        if (!string.IsNullOrWhiteSpace(mappedValue))
                        {
                            element.SetAttribute("StrokeLineCap", mappedValue);
                        }
                    }

                    element.RemoveAttribute("StrokeStartLineCap");
                    element.RemoveAttribute("StrokeEndLineCap");
                }

                return doc.OuterXml;
            }
            catch
            {
                // Keep conversion resilient: if this pass fails, return the original text.
                return xaml;
            }
        }

        public static readonly (string WpfPattern, string AvaloniaReplacement, bool IsRegex)[] C_AvaloniaFindAndReplace = new[]
        {
            // Simple string replacements (fast, non-regex)
            ("<Style ", "<ControlTheme ", false),
            ("<Style.Setters>", "<ControlTheme.Setters>", false),
            ("</Style.Setters>", "</ControlTheme.Setters>", false),
            ("</Style>", "</ControlTheme>", false),
            ("ToolTip=", "ToolTip.Tip=", false),
            ("DataGrid.Columns", "vb:VBDataGrid.Columns", false),
            ("AllowDrop=", "DragDrop.AllowDrop=", false),
            ("<Style", "<ControlTheme", false),
            ("</Style", "</ControlTheme", false),    
            (" Style=", " Theme=", false),    
            (".TreeItemTemplate", ".ItemTemplate", false),
            ("VirtualizingStackPanel.IsVirtualizing=\"True\"", "", false),
            (" ScrollViewerVisibility=\"Hidden\"", " ScrollViewerVisibility=\"False\"", false),
            (" Visibility=\"Hidden\"", " IsVisible=\"False\"", false),
            (" Visibility=\"Collapsed\"", " IsVisible=\"False\"", false),
            (" Visibility=\"Visible\"", " IsVisible=\"True\"", false),
            ("EnableRowVirtualization=\"True\"", "", false),
            ("VirtualizingStackPanel.VirtualizationMode=\"TRecyclinge\"", "", false),
            (" Key=\"", " x:Key=\"", false),
            
            (" ColorInterpolationMode=\"SRgbLinearInterpolation\"", "", false),
            (" MappingMode=\"RelativeToBoundingBox\"", "", false),
            ("Property=\"X2\" Value=\"1\"", "Property=\"EndPoint\" Value=\"1,0\"", false),
            ("Property=\"Y2\" Value=\"1\"", "Property=\"EndPoint\" Value=\"0,1\"", false),
            ("RelativeSource={x:Static RelativeSource.Self}}", "RelativeSource={RelativeSource Self}}", false),
            ("StrokeLineJoin=", "StrokeJoin=", false),
            ("Transform=\"Identity\"", "", false),
            ("GlassEffect=\"Visible\"", "GlassEffect=\"True\"", false),
            ("GlassEffect=\"Hidden\"", "GlassEffect=\"False\"", false),
            ("GlassEffect=\"Collapsed\"", "GlassEffect=\"False\"", false),
            ("Rotor=\"Visible\"", "Rotor=\"True\"", false),
            ("Rotor=\"Hidden\"", "Rotor=\"False\"", false),
            ("Rotor=\"Collapsed\"", "Rotor=\"False\"", false),
            ("<vb:VBInstanceInfo x:Key=", "<vb:VBInstanceInfo Key=", false),
                        
            // Regex-based patterns for complex multi-line replacements
            (@"<vb:VBTreeView\.TreeItemTemplate>\s*<DataTemplate>", "<TreeView.ItemTemplate>\n    <TreeDataTemplate ItemsSource=\"{Binding VisibleItemsT}\">", true),
            (@"<vb:VBTreeView\.ItemTemplate>\s*<DataTemplate>", "<TreeView.ItemTemplate>\n    <TreeDataTemplate ItemsSource=\"{Binding VisibleItemsT}\">", true),
            (@"<TreeView\.TreeItemTemplate>\s*<DataTemplate>", "<TreeView.ItemTemplate>\n    <TreeDataTemplate ItemsSource=\"{Binding VisibleItemsT}\">", true),
            (@"<TreeView\.ItemTemplate>\s*<DataTemplate>", "<TreeView.ItemTemplate>\n    <TreeDataTemplate ItemsSource=\"{Binding VisibleItemsT}\">", true),
            (@"</DataTemplate>\s*</vb:VBTreeView\.TreeItemTemplate>", "</TreeDataTemplate>\n</TreeView.ItemTemplate>", true),
            (@"</DataTemplate>\s*</vb:VBTreeView\.ItemTemplate>", "</TreeDataTemplate>\n</TreeView.ItemTemplate>", true),
            (@"</DataTemplate>\s*</TreeView\.TreeItemTemplate>", "</TreeDataTemplate>\n</TreeView.ItemTemplate>", true),
            (@"</DataTemplate>\s*</TreeView\.ItemTemplate>", "</TreeDataTemplate>\n</TreeView.ItemTemplate>", true),
            
            // Remove CenterX and CenterY from SkewTransform
            (@"<SkewTransform\s+([^>]*?)CenterX=""[^""]*""\s*", @"<SkewTransform $1", true),
            (@"<SkewTransform\s+([^>]*?)CenterY=""[^""]*""\s*", @"<SkewTransform $1", true),

            // Remove obsolete Shape property not available in Avalonia.
            (@"\s+StrokeMiterLimit=""[^""]*""", "", true),

            // Convert Image with VBStaticResource to VBDynamicImage (with ResourceKey parameter)
            (@"<Image\s+([^>]*?)Source=""\{vb:VBStaticResource\s+ResourceKey=([^,}]+)[^}]*\}""([^>]*?)(/?>)", @"<vb:VBDynamicImage $1VBContent=""$2""$3$4", true),
            // Convert Image with VBStaticResource to VBDynamicImage (with positional ResourceKey)
            (@"<Image\s+([^>]*?)Source=""\{vb:VBStaticResource\s+([^,}]+)\}""([^>]*?)(/?>)", @"<vb:VBDynamicImage $1VBContent=""$2""$3$4", true),
            // Convert Image with VBStaticResource to VBDynamicImage (no ResourceKey - keep VBContent if already present)
            (@"<Image\s+([^>]*?)Source=""\{vb:VBStaticResource\s*\}""([^>]*?)(/?>)", @"<vb:VBDynamicImage $1$2$3", true),
            
            // Note: xmlns removal from child elements is handled separately in XAMLDesign property to preserve root element xmlns
        };

        #endregion

        #region Clone
        public object Clone()
        {
            ACClassDesign clonedObject = new ACClassDesign();
            clonedObject.ACClassDesignID = this.ACClassDesignID;
            clonedObject.ACClassID = this.ACClassID;
            CopyFieldsTo(clonedObject);
            return clonedObject;
        }

        public void CopyFieldsTo(ACClassDesign toDesign)
        {
            toDesign.ACIdentifier = this.ACIdentifier;
            toDesign.ACIdentifierKey = this.ACIdentifierKey;
            toDesign.ACCaptionTranslation = this.ACCaptionTranslation;
            toDesign.ACGroup = this.ACGroup;
            toDesign.XMLDesign = this.XMLDesign;
            toDesign.XMLDesign2 = this.XMLDesign2;
            toDesign.DesignBinary = this.DesignBinary;
            toDesign.DesignNo = this.DesignNo;
            toDesign.ValueTypeACClassID = this.ValueTypeACClassID;
            toDesign.ACKindIndex = this.ACKindIndex;
            toDesign.ACUsageIndex = this.ACUsageIndex;
            toDesign.SortIndex = this.SortIndex;
            toDesign.IsRightmanagement = this.IsRightmanagement;
            toDesign.Comment = this.Comment;
            toDesign.IsDefault = this.IsDefault;
            toDesign.IsResourceStyle = this.IsResourceStyle;
            toDesign.VisualHeight = this.VisualHeight;
            toDesign.VisualWidth = this.VisualWidth;
            toDesign.XMLConfig = this.XMLConfig;
            toDesign.BranchNo = this.BranchNo;
            toDesign.DesignerMaxRecursion = this.DesignerMaxRecursion;
            toDesign.BAMLDesign = this.BAMLDesign;
            toDesign.BAMLDate = this.BAMLDate;
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
