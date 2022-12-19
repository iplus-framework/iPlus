using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>Table that stores the rights for using ACComponents an their Properties, Methods and Designs.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Usergroupright'}de{'Benutzergruppenrechte'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "VBGroup", "en{'Usergroup'}de{'Benutzergruppe'}", Const.ContextDatabase + "\\" + VBGroup.ClassName, "", true)]
    [ACPropertyEntity(2, ACClass.ClassName, "en{'Class'}de{'Klasse'}", Const.ContextDatabase + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(3, "ControlModeIndex", "en{'ControlModeIndex'}de{'de-ControlModeIndex'}", typeof(Global.ControlModes), "", "", true)]
    [ACPropertyEntity(9999, "ACClassDesign", "en{'Design'}de{'Design'}", Const.ContextDatabase + "\\" + ACClassDesign.ClassName + "List", "", true)]
    [ACPropertyEntity(9999, "ACClassMethod", "en{'Method'}de{'Methode'}", Const.ContextDatabase + "\\" + ACClassMethod.ClassName, "", true)]
    [ACPropertyEntity(9999, ACClassProperty.ClassName, "en{'ACClassProperty'}de{'de-ACClassProperty'}", Const.ContextDatabase + "\\" + ACClassProperty.ClassName, "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBGroupRight.ClassName, "en{'Usergroupright'}de{'Benutzergruppenrechte'}", typeof(VBGroupRight), VBGroupRight.ClassName, VBGroup.ClassName + "\\VBGroupName", VBGroup.ClassName + "\\VBGroupName")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBGroupRight>) })]
    [NotMapped]
    public partial class VBGroupRight : IACObjectEntity
    {
        public const string ClassName = "VBGroupRight";

        #region New/Delete
        public static VBGroupRight NewACObject(Database database, IACObject parentACObject)
        {
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            VBGroupRight entity = new VBGroupRight();
            entity.VBGroupRightID = Guid.NewGuid();
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            if (parentACObject is VBGroup)
            {
                entity.VBGroup = parentACObject as VBGroup;
            }
            entity.SetInsertAndUpdateInfo(database.UserName, database);
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
                if ( ACClassProperty != null )
                    return ACClass.ACCaption + "->" + ACClassProperty.ACCaption;
                if (ACClassMethod != null)
                    return ACClass.ACCaption + "->" + ACClassMethod.ACCaption;
                if (ACClassDesign != null)
                    return ACClass.ACCaption + "->" + ACClassDesign.ACCaption;

                return ACClass.ACCaption;
            }
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            if (ACClass == null || VBGroup == null)
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "Key",
                    Message = "Key",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "Key"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            base.EntityCheckAdded(user, context);
            return null;
        }


        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "VBGroup\\VBGroupName";
            }
        }
        
        [NotMapped]
        public Global.ControlModes ControlMode
        {
            get
            {
                return (Global.ControlModes)ControlModeIndex;
            }
            set
            {
                ControlModeIndex = (short)value;
            }
        }
        #endregion
    }
}
