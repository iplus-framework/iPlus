using System;
using System.Collections.Generic;
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
        public string ACIdentifier { get; set; }

        [ACPropertyInfo(2, Const.ACCaptionPrefix, "en{'Description'}de{'Beschreibung'}")]
        public string ACCaption { get; set; }

        [ACPropertyInfo(3, "DefaultConfiguration", "en{'DefaultConfiguration'}de{'DefaultConfiguration'}")]
        public IACConfig DefaultConfiguration { get; set; }

        public ACClassWF ACClassWF
        {
            get;
            set;
        }

        public Guid ValueTypeACClassID { get; set; }

        private List<IACConfig> _ConfigurationList;
        [ACPropertyInfo(9999, "ConfigurationList")]
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
        public gipCoreData.ACClass VBACClass { get; set; }

        public Guid? VBiACClassID { get; set; }


        #region For mass preview - params from many nodes

        [ACPropertyInfo(100, Const.PN_LocalConfigACUrl, Const.LocalConfigACUrl)]
        public string LocalConfigACUrl { get; set; }


        [ACPropertyInfo(99, Const.PN_PreConfigACUrl, Const.PreConfigACUrl)]
        public string PreConfigACUrl { get; set; }


        public string ACMehtodACIdentifier { get; set; }

        #endregion

    }
}
