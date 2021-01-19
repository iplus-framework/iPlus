using System;

namespace gip.core.datamodel
{
    /// <summary>Table that stores license informations.</summary>
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'VB License'}de{'VB License'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "LicenseNo", "en{'License No'}de{'Lizenz Nr.'}", "", "", true)]
    [ACPropertyEntity(2, "ProjectNo", "en{'Project number'}de{'Projekt Nummer'}", "", "", true)]
    [ACPropertyEntity(3, "CustomerName", "en{'Customer name'}de{'Name des Kunden'}", "", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBLicense.ClassName, "en{'License'}de{'Lizenz'}", typeof(VBLicense), VBLicense.ClassName, "CustomerName", "CustomerName")]
    public partial class VBLicense : IACObjectEntity
    {
        public const string ClassName = "VBLicense";
        public const string NoColumnName = "LicenseNo";
        public const string FormatNewNo = null;

        public static VBLicense NewACObject(Database database, object parent, string secondaryKey)
        {
            VBLicense entity = new VBLicense();
            entity.VBLicenseID = Guid.NewGuid();
            entity.LicenseNo = secondaryKey;
            entity.SetInsertAndUpdateInfo(Database.Initials, database);
            database.VBLicense.AddObject(entity);
            return entity;
        }


        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        public override string ACCaption
        {
            get
            {
                return ProjectNo;
            }
        }

        static public string KeyACIdentifier
        {
            get
            {
                return "CustomerName";
            }
        }
    }
}
