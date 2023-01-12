using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
            var myObjectState = context.ChangeTracker.Entries().Where(c => c.Entity == entityObject);
            //var modifiedProperties = myObjectState.GetModifiedProperties();
            var modifiedProperties = context.Entry(entityObject).Properties.Where(c => c.IsModified).Select(c => c.Metadata.Name);


            bool localWithGreaterUpdateDate = false;
            bool isSame = false;
            DateTime localRecordTime = new DateTime();
            DateTime importedRecordTime = new DateTime();


            if (modifiedProperties.Contains(Property_UpdateDate))
            {
                //localRecordTime = (DateTime)myObjectState.OriginalValues[Property_UpdateDate];
                localRecordTime = (DateTime)myObjectState.Select(c => c.OriginalValues[Property_UpdateDate]).FirstOrDefault();
                //importedRecordTime = (DateTime)myObjectState.CurrentValues[Property_UpdateDate];
                importedRecordTime = (DateTime)myObjectState.Select(c => c.OriginalValues[Property_UpdateDate]).FirstOrDefault();

                isSame = Math.Abs((localRecordTime - importedRecordTime).TotalMinutes) < 1;

                localWithGreaterUpdateDate = !isSame && localRecordTime > importedRecordTime;
            }

            if (localWithGreaterUpdateDate)
            {
                msg = new Msg()
                {
                    Source = entityObject.GetACUrl(),
                    MessageLevel = eMsgLevel.Error,
                    Message = string.Format("[ControlSync/Import] [Type: {0}] [File: {1}] Item not imported, local UpdateDate {2} is greater as {3} from import!",
                   entityTypeName, importFileName, localRecordTime, importedRecordTime)
                };
            }
            else if(isSame)
            {
                msg = new Msg()
                {
                    Source = entityObject.GetACUrl(),
                    MessageLevel = eMsgLevel.Info,
                    Message = string.Format(@"[ControlSync/Import] [Type: {0}] [File: {1}] Item not imported, UpdateDate values is same!", entityTypeName, importFileName)
                };
            }

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
                IACEntityObjectContext context = (aCFSItem.ACObject as VBEntityObject).Context;
                if (context == null)
                    context = ACObjectContextManager.GetContextFromACUrl(null, aCFSItem.ACObject.ACType.ObjectFullType.FullName);
                EntityState objectEntityState = entityObject.EntityState;
                if (context != null)
                {
                    if (aCFSItem.IsChecked && objectEntityState == EntityState.Detached && (!checkUpdateDate || !aCFSItem.UpdateDateFail))
                    {
                        if (entityObject.EntityKey != null)
                        {
                            VBEntityObject tempObject = (context as IACEntityObjectContext).GetObjectByKey(entityObject.EntityKey) as VBEntityObject;
                            if (tempObject != null)
                                context.Detach(tempObject);
                            context.Attach(entityObject);
                        }
                    }
                    else if (objectEntityState != EntityState.Detached && (!aCFSItem.IsChecked || (checkUpdateDate && aCFSItem.UpdateDateFail)))
                    {
                        context.Detach(aCFSItem.ACObject);
                    }
                    throw new NotImplementedException();
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

#region Validation

        public static void ReferenceValidation(ACFSItem aCFSItem, object[] args)
        {
            List<Msg> messages = args[0] as List<Msg>;

            if(aCFSItem.ACObject != null)
            {
                if(aCFSItem.ACObject is ACClassDesign)
                {
                    ACClassDesign acClassDesign = aCFSItem.ACObject as ACClassDesign;
                    if(acClassDesign.ACClassID == Guid.Empty)
                    {
                        Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = $"ACClassDesign {acClassDesign.ACUrl} don't have referenced ACClass!" };
                        messages.Add(errMsg);
                        aCFSItem.IsChecked = false;
                    }
                }
                else if (aCFSItem.ACObject is ACClassProperty)
                {
                    ACClassProperty aCClassProperty = aCFSItem.ACObject as ACClassProperty;
                    if(aCClassProperty.ACClassID == Guid.Empty)
                    {
                        Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = $"ACClassProperty {aCClassProperty.ACUrl} don't have referenced ACClass!" };
                        messages.Add(errMsg);
                        aCFSItem.IsChecked = false;
                    }
                    if (aCClassProperty.BasedOnACClassPropertyID == Guid.Empty)
                    {
                        Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = $"ACClassProperty {aCClassProperty.ACUrl} don't have referenced BasedOnACClassProperty!" };
                        messages.Add(errMsg);
                        aCFSItem.IsChecked = false;
                    }
                    if (aCClassProperty.ValueTypeACClassID == Guid.Empty)
                    {
                        Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = $"ACClassProperty {aCClassProperty.ACUrl} don't have referenced ValueTypeACClass!" };
                        messages.Add(errMsg);
                        aCFSItem.IsChecked = false;
                    }
                }
                else if (aCFSItem.ACObject is ACClassMessage)
                {
                    ACClassMessage aCClassMessage = aCFSItem.ACObject as ACClassMessage;
                    if (aCClassMessage.ACClassID == Guid.Empty)
                    {
                        Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = $"ACClassMessage {aCClassMessage.ACUrl} don't have referenced ACClass!" };
                        messages.Add(errMsg);
                        aCFSItem.IsChecked = false;
                    }
                }
                else if (aCFSItem.ACObject is ACClassText)
                {
                    ACClassText aCClassText = aCFSItem.ACObject as ACClassText;
                    if (aCClassText.ACClassID == Guid.Empty)
                    {
                        Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = $"ACClassText {aCClassText.ACUrl} don't have referenced ACClass!" };
                        messages.Add(errMsg);
                        aCFSItem.IsChecked = false;
                    }
                }
                else if (aCFSItem.ACObject is ACClassPropertyRelation)
                {
                    ACClassPropertyRelation aCClassPropertyRelation = aCFSItem.ACObject as ACClassPropertyRelation;
                    if(
                            aCClassPropertyRelation.SourceACClassID == Guid.Empty 
                            || aCClassPropertyRelation.SourceACClassPropertyID == Guid.Empty 
                            || aCClassPropertyRelation.TargetACClassID == Guid.Empty
                            || aCClassPropertyRelation.TargetACClassPropertyID == Guid.Empty
                      )
                    {

                        string propertyRelationMsg = $"ACClassPropertyRelation ACIdentifier: {aCClassPropertyRelation.ACIdentifier} SourceACUrl: ";
                        if (aCClassPropertyRelation.SourceACClass != null && aCClassPropertyRelation.SourceACClassProperty != null)
                        {
                            propertyRelationMsg += aCClassPropertyRelation.SourceACUrl;
                        }
                        propertyRelationMsg += " TargetACUrl: ";
                        if (aCClassPropertyRelation.TargetACClass != null && aCClassPropertyRelation.TargetACClassProperty != null)
                        {
                            propertyRelationMsg += aCClassPropertyRelation.TargetACUrl;
                        }
                        
                        if (aCClassPropertyRelation.SourceACClassID == Guid.Empty)
                        {
                            Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = propertyRelationMsg + " don't have referenced SourceACClass!" };
                            messages.Add(errMsg);
                            aCFSItem.IsChecked = false;
                        }
                        
                        if (aCClassPropertyRelation.SourceACClassPropertyID == Guid.Empty)
                        {
                            Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = propertyRelationMsg + " don't have referenced SourceACClassProperty!" };
                            messages.Add(errMsg);
                            aCFSItem.IsChecked = false;
                        }

                        if (aCClassPropertyRelation.TargetACClassID == Guid.Empty)
                        {
                            Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = propertyRelationMsg + " don't have referenced TargetACClass!" };
                            messages.Add(errMsg);
                            aCFSItem.IsChecked = false;
                        }
                        
                        if (aCClassPropertyRelation.TargetACClassPropertyID == Guid.Empty)
                        {
                            Msg errMsg = new Msg() { MessageLevel = eMsgLevel.Error, Message = propertyRelationMsg + " don't have referenced TargetACClassProperty!" };
                            messages.Add(errMsg);
                            aCFSItem.IsChecked = false;
                        }
                    }
                }
            }
            
        }

#endregion

#region Test

        public static void RunSomeCheck(ACFSItem aCFSItem, object[] args)
        {
            if(aCFSItem != null && aCFSItem.ACObject != null && aCFSItem.ACObject is ACClassDesign)
            {
                ACClassDesign aCClassDesign = aCFSItem.ACObject as ACClassDesign;
                if(aCClassDesign.ACClass == null)
                {
                    throw new Exception("ACClassDesign.ACClass == null!");
                }
            }
        }

#endregion

    }
}
