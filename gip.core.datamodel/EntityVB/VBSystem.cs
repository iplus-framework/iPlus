using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBSystem'}de{'VBSystem'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "CustomerName", "en{'Customer'}de{'Customer'}","","",true)]
    [ACPropertyEntity(2, "ProjectNo", "en{'Project number'}de{'Project number'}", "", "", true)]
    [ACPropertyEntity(3, "SystemName", "en{'Systemname'}de{'Systemname'}", "", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBSystem.ClassName, "en{'System'}de{'System'}", typeof(VBSystem), VBSystem.ClassName, "CustomerName", "CustomerName")]
    public partial class VBSystem : IACObjectEntity
    {
        public const string ClassName = "VBSystem";

        #region New/Delete
        public static VBSystem NewACObject(Database database, IACObject parentACObject)
        {
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            VBSystem entity = new VBSystem();
            entity.VBSystemID = Guid.NewGuid();
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            database.VBSystem.Add(entity);
            return entity;
        }

        #endregion

        #region IACUrl Member

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return SystemName;
            }
        }

        public override string ACIdentifier
        {
            get
            {
                return SystemName;
            }
            set
            {
                _ = value;
            }
        }

        #endregion

        #region IACObjectEntity Members

        static public string KeyACIdentifier
        {
            get
            {
                return "CustomerName";
            }
        }

        #endregion

        #region Helper

        public byte[] GetChecksum()
        {
            byte[] checkSum;
            using (SHA256 sha = SHA256.Create())
            {
                checkSum = sha.ComputeHash(Encoding.UTF8.GetBytes(string.Format("{1}{5}{0}{2}{4}{3}{7}{6}{8}", SystemInternal.ToByteString(), SystemPrivate, SystemRemote,
                                                                                                        SystemInternal1.ToByteString(), SystemInternal2, 
                                                                                                        SystemInternal3.ToByteString(), SystemName, CustomerName, Company)));
            }
            return checkSum;
        }

        public byte[] GetChecksumSigned()
        {
            byte[] checkSum;
            using (SHA256 sha = SHA256.Create())
            {
                checkSum = sha.ComputeHash(Encoding.UTF8.GetBytes(string.Format("{1}{5}{0}{2}{4}{3}{7}{6}{8}{9}", SystemInternal.ToByteString(), SystemPrivate, SystemRemote,
                                                                                                        SystemInternal1.ToByteString(), SystemInternal2, SystemCommon.ToByteStringKey(),
                                                                                                        SystemInternal3.ToByteString(), SystemName, CustomerName,Company)));
            }
            return checkSum;
        }

        #endregion
    }
}
