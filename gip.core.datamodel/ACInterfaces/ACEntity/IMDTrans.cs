// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for entity framework classes, that are used for wrapping and translating enums that are stored in the database.
    /// Other Tables can reference these entries by foreign keys.
    /// MD-Tables are mostly displayed as a Combobox on the GUI to assign e.g. states or features to another Entity-Object.
    /// </summary>
    public interface IMDTrans
    {
        /// <summary>
        /// Database-Field that stores the tanslated texts for an enum-Value.
        /// Structure of TranslationTuple: languageCode1{'translatedText1'}languageCode2{'translatedText2'}..
        /// The gip.core.datamodel.Translator is used for reading and writing the translation-tuples.
        /// The Entity-Classes, that implements this interface should provide a Property "MDName" that invokes the GetTranslation() and SetTranslation()-Methods of the Translator-Class.
        /// </summary>
        /// <value>
        /// Translation-Tuple
        /// </value>
        string MDNameTrans { get; set; }


        /// <summary>
        /// Secondary Key of the the MD-Table.
        /// </summary>
        /// <value>
        /// Secondary Key
        /// </value>
        string MDKey { get; set; }
    }
}
