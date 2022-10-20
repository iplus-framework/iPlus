// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACObjectEntity.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for Entity-Objects with translated Labels/Descriptions
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACObjectEntity" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACObjectEntityWithCheckTrans'}de{'IACObjectEntityWithCheckTrans'}", Global.ACKinds.TACInterface)]
    public interface IACObjectEntityWithCheckTrans : IACObjectEntity
    {
        /// <summary>
        /// String with translation-tuples for displaying ACCaption in the User-Language
        /// </summary>
        /// <value>String with translation-tuples</value>
        [ACPropertyInfo(3, "", "en{''}de{''}")]
        string ACCaptionTranslation { get; set; }


        /// <summary>
        /// Method for getting the translated text from ACCaptionTranslation
        /// </summary>
        /// <param name="VBLanguageCode">I18N-code</param>
        /// <returns>Translated text</returns>
        string GetTranslation(string VBLanguageCode);
    }
}
