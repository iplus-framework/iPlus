// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACValueItemList.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACValueItemList
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACEnumObjectList'}de{'ACEnumObjectList'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACValueItemList : List<ACValueItem>
    {
        /// <summary>
        /// The _ property
        /// </summary>
        string _Property;

        /// <summary>
        /// Initializes a new instance of the <see cref="ACValueItemList"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        public ACValueItemList(string property)
        {
            _Property = property;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACValueItemList"/> class.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="propertyType">Type of the property.</param>
        public ACValueItemList(Type enumType, Type propertyType)
        {
            Type type = Enum.GetUnderlyingType(enumType);
            _Property = enumType.Name;

            var values = Enum.GetValues(enumType);

            var x = type.BaseType;
            foreach (var value in values)
            {
                if (propertyType != null && enumType != propertyType)
                {
                    AddEntry(Convert.ChangeType(value, propertyType), value.ToString());
                }
                else
                {
                    AddEntry(value, value.ToString());
                }
            }
        }

        /// <summary>
        /// Hinzufügen eines neuen Eintrags
        /// </summary>
        /// <param name="acValue">The ac value.</param>
        /// <param name="acCaption">The ac caption.</param>
        public void AddEntry(object acValue, string acCaption)
        {
            Add(new ACValueItem(acCaption, acValue, null));
        }

        /// <summary>
        /// Adds the entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void AddEntry(ACValueItem entry)
        {
            Add(entry);
        }

        /// <summary>
        /// Gets the index of the entry by.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>ACValueItem.</returns>
        public ACValueItem GetEntryByIndex(short index)
        {
            try
            {
                return this.Where(c => (short)c.Value == index).FirstOrDefault();
            }
            catch(Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACValueItemList", "GetEntryByIndex", msg);

                return null;
            }
        }

        /// <summary>
        /// Prüft die Gültigkeit eine Index
        /// </summary>
        /// <param name="index">The index.</param>
        public void CheckIndex(short index)
        {

            if (!this.Where(c => (short)c.Value == index).Any())
            {
                string message = string.Format("Der Index {0} ist für die Eigenschaft {1} nicht erlaubt!",
                    index, _Property);
                //throw new EntityCheckException(message, false);
            }
        }
    }

}
