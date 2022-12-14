using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using gipCoreData = gip.core.datamodel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Wrapper class for IACConfig for presentation on the GUI.
    /// It helps to display the overridden according the IACConfigStore.OverridingOrder 
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACConfigParam'}de{'ACConfigParam'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACConfigParam 
    {
        [ACPropertyInfo(1, Const.ACIdentifierPrefix, "en{'Name'}de{'Name'}")]
        [NotMapped]
        public string ACIdentifier { get; set; }

        [ACPropertyInfo(2, Const.ACCaptionPrefix, "en{'Description'}de{'Beschreibung'}")]
        [NotMapped]
        public string ACCaption { get; set; }

        [ACPropertyInfo(3, "DefaultConfiguration", "en{'DefaultConfiguration'}de{'DefaultConfiguration'}")]
        [NotMapped]
        public IACConfig DefaultConfiguration { get; set; }

        public ACClassWF ACClassWF
        {
            get;
            set;
        }

        public Guid ValueTypeACClassID { get; set; }

        private List<IACConfig> _ConfigurationList;
        [ACPropertyInfo(9999, "ConfigurationList")]
        [NotMapped]
        public List<IACConfig> ConfigurationList
        {
            get
            {
                if (_ConfigurationList == null)
                    _ConfigurationList = new List<IACConfig>();
                return _ConfigurationList;
            }
            set
            {
                _ConfigurationList = value;
            }
        }

        [ACPropertyInfo(105)]
        [NotMapped]
        public gipCoreData.ACClass VBACClass { get; set; }

        public Guid? VBiACClassID { get; set; }

    }
}
