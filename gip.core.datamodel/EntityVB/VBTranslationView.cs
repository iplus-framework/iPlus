using System;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTranslationView'}de{'VBTranslationView'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "ACProjectName", "en{'Parent'}de{'Parent'}", "", "", true)]
    [ACPropertyEntity(2, "TableName", "en{'Table name'}de{'Tabellename'}", "", "", true)]
    [ACPropertyEntity(3, "MandatoryID", "en{'MandatoryID'}de{'MandatoryID'}", "", "", true)]
    [ACPropertyEntity(3, "MandatoryACIdentifier", "en{'MandatoryACIdentifier'}de{'MandatoryACIdentifier'}", "", "", true)]
    [ACPropertyEntity(3, "ID", "en{'ID'}de{'ID'}", "", "", true)]
    [ACPropertyEntity(4, "ACIdentifier", "en{'ACIdentifier'}de{'ACIdentifier'}", "", "", true)]
    [ACPropertyEntity(5, "TranslationValue", "en{'Translation'}de{'Übersetzung'}", "", "", true)]
    [ACPropertyEntity(498, Const.EntityUpdateName, Const.EntityTransUpdateName)]
    [ACPropertyEntity(499, Const.EntityUpdateDate, Const.EntityTransUpdateDate)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBTranslationView.ClassName, "en{'VBTranslationView'}de{'VBTranslationView'}", typeof(VBTranslationView), VBTranslationView.ClassName, "ACIdentifier", "ACIdentifier")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBTranslationView>) })]
    public partial class VBTranslationView
    {

        public const string ClassName = "VBTranslationView";
    }
}
