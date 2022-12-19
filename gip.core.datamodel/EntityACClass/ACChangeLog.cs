using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace gip.core.datamodel
{
    /// <summary>Table the logs changes in application-tables or persistable properties of ACComponents.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "", Global.ACKinds.TACDBA)]
    [ACPropertyEntity(999, VBUser.ClassName, "en{'Changed by'}de{'Geändert von'}")]
    [ACPropertyEntity(999, "ChangeDate", "en{'Changed on'}de{'Geändert am'}")]
    [NotMapped]
    public partial class ACChangeLog
    {
        public static ACChangeLog NewACObject()
        {
            ACChangeLog aCChangeLog = new ACChangeLog();
            aCChangeLog.ACChangeLogID = Guid.NewGuid();
            return aCChangeLog;
        }

        public static ACChangeLog NewACObject(Database db)
        {
            ACChangeLog aCChangeLog = NewACObject();
            db.ACChangeLog.Add(aCChangeLog);
            return aCChangeLog;
        }

        [NotMapped]
        private ACValue _ChangeLogValue;
        [ACPropertyInfo(999, "", "en{'Value'}de{'Wert'}")]
        [NotMapped]
        public ACValue ChangeLogValue
        {
            get
            {
                return _ChangeLogValue;

                //if (string.IsNullOrEmpty(XMLValue))
                //    return null;

                //var value = ACConvert.XMLToObject(ACClassProperty.ObjectType, XMLValue, true, Database.GlobalDatabase);

                ////ACValueItem changeLogValue = new ACValueItem(null, null, ACClassProperty.ValueTypeACClass);
                ////changeLogValue.SetValueFromString(XMLValue);
                //return new ACValue(ACClassProperty.ACIdentifier, value);
            }
            set
            {
                _ChangeLogValue = value;
                OnPropertyChanged("ChangeLogValue");
            }
        }
    }

    [DataContract]
    public struct ACChangeLogInfo
    {
        [DataMember]
        [NotMapped]
        public string Info
        {
            get;
            set;
        }

        [DataMember]
        [NotMapped]
        public string ACUrl
        {
            get;
            set;
        }

        [IgnoreDataMember]
        [NotMapped]
        public string XMLValue
        {
            get
            {
                string xml;
                using (StringWriter sw = new StringWriter())
                using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(ACChangeLogInfo));
                    serializer.WriteObject(xmlWriter, this);
                    xml = sw.ToString();
                }

                return xml;
            }
            set
            {
                ACChangeLogInfo? info;
                using (StringReader sr = new StringReader(value))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(sr))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(ACChangeLogInfo));
                        info = serializer.ReadObject(xmlReader) as ACChangeLogInfo?;
                    }
                }

                if(info.HasValue)
                {
                    this.ACUrl = info.Value.ACUrl;
                    this.Info = info.Value.Info;
                }

            }
        }
    }
}
