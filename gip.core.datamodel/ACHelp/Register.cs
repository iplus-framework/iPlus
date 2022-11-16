using System;

namespace gip.core.datamodel
{
    [Serializable]
#if !EFCR
    [JsonObject(MemberSerialization.OptOut)]
#endif
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Register'}de{'Register'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class Register
    {
        [ACPropertyInfo(9999, "Email", "en{'Email'}de{'Email'}")]
        public string email { get; set; }

        [ACPropertyInfo(9999, "Password", "en{'Password'}de{'Kennwort'}")]
        public string password { get; set; }

        [ACPropertyInfo(9999, "PasswordRepeat", "en{'Repeat password'}de{'Kennwort wiederholen'}")]
        public string passwordRepeated { get; set; }

        [ACPropertyInfo(9999, "FirstName", "en{'First name'}de{'Vorname'}")]
        public string FirstName { get; set; }

        [ACPropertyInfo(9999, "LastName", "en{'Last name'}de{'Nachname'}")]
        public string LastName { get; set; }

        [ACPropertyInfo(9999, "Gender", "en{'Gender'}de{'Geschlecht'}")]
        public string Gender { get; set; }

        [ACPropertyInfo(9999, "Langauge", "en{'Langauge'}de{'Sprache'}")]
        public string LangaugeCode { get; set; }

        [ACPropertyInfo(9999, "AddressType", "en{'Address type'}de{'Adresstyp'}")]
        public short AddressTypeID { get; set; }

#if !EFCR
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
#endif
        [ACPropertyInfo(9999, "Address", "en{'Address'}de{'Adresse'}")]
        public string Address { get; set; }

        [ACPropertyInfo(9999, "PhoneType", "en{'Phone type'}de{'Telefon-Typ'}")]
        public short PhoneType { get; set; }

        [ACPropertyInfo(9999, "PhonePrefix", "en{'Call number'}de{'Rufnummer'}")]
        public string PhonePrefix { get; set; }

        [ACPropertyInfo(9999, "PhoneNumber", "en{'Phone number'}de{'Telefonnummer'}")]
        public string Phone { get; set; }

        #region Address part will be populated from Map (Bing or Google)
        public string StreetNo { get; set; }
        public string Street { get; set; }
        public string PostCode { get; set; }
        public string PlaceName { get; set; }
        public string CountryCode { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        #endregion
    }
}
