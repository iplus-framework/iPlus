using System;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>Table that stores license informations.</summary>
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'VB License'}de{'VB License'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "LicenseNo", "en{'License No'}de{'Lizenz Nr.'}", "", "", true)]
    [ACPropertyEntity(2, "ProjectNo", "en{'Project number'}de{'Projekt Nummer'}", "", "", true)]
    [ACPropertyEntity(3, "CustomerName", "en{'Customer name'}de{'Name des Kunden'}", "", "", true)]
    [ACPropertyEntity(4, "SystemDB", "en{'Unique customer code'}de{'Eindeutiger Kundencode'}", "", "", true)]
    [ACPropertyEntity(5, "SystemDS", "en{'Number of licensed sessions'}de{'Anzahl lizenzierter Sitzungen'}", "", "", true)]
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
            entity.SetInsertAndUpdateInfo(database.UserName, database);
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

        [ACPropertyInfo(200, "", "en{'Readable License Key'}de{'Lesbarer Lizenzschlüssel'}")]
        public string ReadableLicenseKey
        {
            get
            {
                if (SystemCommon == null || SystemCommon.Length <= 0)
                    return null;
                return BitConverter.ToString(SystemCommon);
            }
        }

        [ACPropertyInfo(201, "", "en{'Assigned Packages'}de{'Zugewiesen Pakete'}")]
        public string ReadablePackages
        {
            get
            {
                if (PackageSystem == null || PackageSystem.Length <= 0)
                    return "";
                return Encoding.UTF8.GetString(PackageSystem);
            }
        }
    }
}
