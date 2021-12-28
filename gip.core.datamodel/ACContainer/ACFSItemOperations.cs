using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;

namespace gip.core.datamodel
{
    public static class ACFSItemOperations
    {

        #region config
        public const string Property_UpdateDate = @"UpdateDate";
        #endregion


        #region Process Update Date
        /// <summary>
        /// Check items in tree and compare Update Dates - write report
        /// </summary>
        /// <param name="aCFSItem"></param>
        /// <param name="args"></param>
        public static void ProcessUpdateDate(ACFSItem aCFSItem, object[] args)
        {
            MsgWithDetails msgWithDetails = args[0] as MsgWithDetails;
            string importFileName = args[1].ToString();
            if (aCFSItem.ACObject != null && aCFSItem.ACObject is VBEntityObject)
            {
                VBEntityObject entityObject = aCFSItem.ACObject as VBEntityObject;
                IACEntityObjectContext context = (aCFSItem.ACObject as VBEntityObject).GetObjectContext();
                if (context == null)
                    context = ACObjectContextManager.GetContextFromACUrl(null, aCFSItem.ACObject.ACType.ObjectFullType.FullName);
                EntityState objectEntityState = entityObject.EntityState;
                if (objectEntityState != EntityState.Added && context != null)
                {
                    string entityTypeName = entityObject.GetType().Name;
                    Msg checkStateMsg = CheckUpdateDate(context, entityObject, importFileName, entityTypeName);
                    // Exclude broadcast messages for ACProject type
                    if (checkStateMsg != null && entityTypeName != ACProject.ClassName)
                        msgWithDetails.AddDetailMessage(checkStateMsg);
                    aCFSItem.UpdateDateFail = checkStateMsg != null;
                }
            }
        }


        private static Msg CheckUpdateDate(IACEntityObjectContext context, VBEntityObject entityObject, string importFileName, string entityTypeName)
        {
            Msg msg = null;
            var myObjectState = context.ObjectStateManager.GetObjectStateEntry(entityObject);
            var modifiedProperties = myObjectState.GetModifiedProperties();
            if (modifiedProperties.Contains(Property_UpdateDate))
            {
                DateTime localRecordTime = (DateTime)myObjectState.OriginalValues[Property_UpdateDate];
                DateTime importedRecordTime = (DateTime)myObjectState.CurrentValues[Property_UpdateDate];

                if (localRecordTime > importedRecordTime)
                    msg = new Msg()
                    {
                        Source = entityObject.GetACUrl(),
                        MessageLevel = eMsgLevel.Error,
                        Message = string.Format("[ControlSync/Import] [Type: {0}] [File: {1}] Item not imported, local UpdateDate {2} is greater as {3} from import!",
                        entityTypeName, importFileName, localRecordTime, importedRecordTime)
                    };
            }
            else
                msg = new Msg()
                {
                    Source = entityObject.GetACUrl(),
                    MessageLevel = eMsgLevel.Info,
                    Message = string.Format(@"[ControlSync/Import] [Type: {0}] [File: {1}] Item not imported, UpdateDate values is same!", entityTypeName, importFileName)
                };
            return msg;
        }

        #endregion


        #region ObjectContext Attach / Deattach
        /// <summary>
        /// Tree operation attach or de-attach to context 
        /// </summary>
        /// <param name="aCFSItem"></param>
        /// <param name="args"></param>
        public static void AttachOrDeattachToContext(ACFSItem aCFSItem, object[] args)
        {
            bool checkUpdateDate = (bool)args[0];
            if (aCFSItem.ACObject != null && aCFSItem.ACObject is VBEntityObject)
            {
                VBEntityObject entityObject = aCFSItem.ACObject as VBEntityObject;
                IACEntityObjectContext context = (aCFSItem.ACObject as VBEntityObject).GetObjectContext();
                if (context == null)
                    context = ACObjectContextManager.GetContextFromACUrl(null, aCFSItem.ACObject.ACType.ObjectFullType.FullName);
                EntityState objectEntityState = entityObject.EntityState;
                if (context != null)
                {
                    if (aCFSItem.IsChecked && objectEntityState == EntityState.Detached && (!checkUpdateDate || !aCFSItem.UpdateDateFail))
                    {
                        VBEntityObject tempObject = (context as ObjectContext).GetObjectByKey(entityObject.EntityKey) as VBEntityObject;
                        if (tempObject != null)
                            context.Detach(tempObject);
                        context.Attach(entityObject);
                    }
                    else if (objectEntityState != EntityState.Detached && (!aCFSItem.IsChecked || (checkUpdateDate && aCFSItem.UpdateDateFail)))
                    {
                        context.Detach(aCFSItem.ACObject);
                    }
                }
            }
        }

        #endregion

        #region Filter

        public static void Filter(ACFSItem aCFSItem, object[] args)
        {
            InstallImportTypeFilter filter = args[0] as InstallImportTypeFilter;
            if (aCFSItem.ResourceType == ResourceTypeEnum.IACObject)
                if (aCFSItem.ACObject.GetType() == filter.Type)
                    aCFSItem.IsChecked = filter.IsChecked;
        }

        public static void FilterParent(ACFSItem aCFSItem, object[] args)
        {
            if (aCFSItem.ResourceType != ResourceTypeEnum.IACObject && aCFSItem.ACObject == null)
                if (!aCFSItem.Items.Any(c => (c as ACFSItem).IsChecked))
                    aCFSItem.IsChecked = false;
                else
                    aCFSItem.IsChecked = true;
        }

        public static void CollectZipFileDates(ACFSItem aCFSItem, object[] args)
        {
            List<string> zipFileNames = args[0] as List<string>;
            if (aCFSItem.IsChecked && aCFSItem.ResourceType == ResourceTypeEnum.Zip)
            {
                string zipName = aCFSItem.ACCaption;
                if (!zipFileNames.Contains(zipName))
                    zipFileNames.Add(zipName);
            }
        }

        #endregion

    }
}
