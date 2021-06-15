namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'TranslationPair}de{'TranslationPair'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class TranslationPair
    {
        [ACPropertyInfo(1, "", "en{'Language'}de{'Sprache'}")]
        public string LangCode { get; set; }
        [ACPropertyInfo(2, "", "en{'Translation'}de{'Übersetzung'}")]
        public string Translation { get; set; }

        public string GetTranslationTuple()
        {
            return string.Format(@"{0}{{'{1}'}}", LangCode, Translation);
        }

        public override string ToString()
        {
            return GetTranslationTuple();
        }
    }
}
