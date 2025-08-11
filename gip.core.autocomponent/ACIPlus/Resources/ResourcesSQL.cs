// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace gip.core.autocomponent
{
    public class ResourcesSQL : Resources
    {

        #region Constants

        public static string SQLConnectionList = "SQLConnectionList";
        public static string Hash = "_dlfjkder%%6E";

        #endregion

        #region ctor's

        public ResourcesSQL()
            : base()
        {
        }

        #endregion

        #region Dir

        public override ACFSItem Dir(IACEntityObjectContext db, ACFSItemContainer container, string path, bool recursive, bool withFiles = false)
        {
            ACFSItem rootACObjectItem = new ACFSItem(this, container, null, path, ResourceTypeEnum.SQL, "\\SQL\\" + path);
            ACEntitySerializer serializer = new ACEntitySerializer();
            // serializer.VBProgress = this.VBProgress;

            Tuple<SQLInstanceInfo, string> sqlServerInstanceAndQueryInfo = null;
            try
            {
                sqlServerInstanceAndQueryInfo = ResourcesSQL.DBUrlDecode(path);
            }
            catch (Exception ec)
            {
                if (Worker != null)
                {
                    Worker.ReportProgress(0, new Msg()
                    {
                        MessageLevel = eMsgLevel.Error,
                        Message = string.Format(@"ResourcesSQL({0}): Unable to decode path! Exception:{1}", path, ec.Message)
                    });
                }
                return rootACObjectItem;
            }
            SQLInstanceInfo sqlInstanceInfo = sqlServerInstanceAndQueryInfo.Item1;
            string acQueryDefinitionIdentifier = sqlServerInstanceAndQueryInfo.Item2;

            DeserializedSQLDataModel result = null;

            try
            {
                result = serializer.GetDeserializeSQLDataModel(sqlInstanceInfo, acQueryDefinitionIdentifier);
                if (Worker != null && serializer.MsgList.Any())
                {
                    foreach(Msg msg in serializer.MsgList)
                    {
                        Worker.ReportProgress(0, msg);
                    }
                }
            }
            catch (Exception ec)
            {
                if (Worker != null)
                {
                    Worker.ReportProgress(0, new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format(@"ResourcesSQL({0}): Unable to fetch external list! External connection string:{1}. Exception:{2}", path, result.OuterDatabase.Connection.ConnectionString, ec.Message) });
                }
                return rootACObjectItem;
            }
            string taskName = string.Format(@"ResourcesSQL.Dir(""{0}"")", path);
            if (result.DeserializedSQLData != null)
            {
                if (VBProgress != null)
                    VBProgress.AddSubTask(taskName, 0, result.DeserializedSQLData.Count());
                ACFSItem listFsItem = new ACFSItem(this, container, null, path, ResourceTypeEnum.List, "\\List\\" + path);
                rootACObjectItem.Add(listFsItem);
                serializer.DeserializeSQL(this, result.InnerDatabase, listFsItem, result.DeserializedSQLData, result.ACQueryDefinition);

                if (VBProgress != null)
                    VBProgress.ReportProgress(taskName, result.DeserializedSQLData.Count, "Deserialization finished!");
            }

            SetupProperties(rootACObjectItem);

            result.OuterDatabase.Dispose();
            return rootACObjectItem;
        }



        #endregion

        #region HelperMethods

        public static string DBUrlEncode(SQLInstanceInfo instanceInfo, ACClass acQueryDefinition)
        {
            return instanceInfo.ServerName + " | " + instanceInfo.Database + " | " + acQueryDefinition.ACIdentifier;
        }

        public static Tuple<SQLInstanceInfo, string> DBUrlDecode(string url)
        {
            List<string> elements = url.Split(" | ".ToCharArray()).ToList();
            elements.RemoveAll(x => string.IsNullOrEmpty(x));

            List<SQLInstanceInfo> result = new List<SQLInstanceInfo>();
            Database db = ACObjectContextManager.GetOrCreateContext<Database>(Const.GlobalDatabase);
            // Decclare key strings 
            ACClassConfig acClassConfig = db.ACClassConfig.Where(c => c.ACClass.ACIdentifier == "BSOiPlusResourceSelect" && c.KeyACUrl == null && c.LocalConfigACUrl == SQLConnectionList).AutoMergeOption(db).FirstOrDefault();
            object storedResult = acClassConfig[Const.Value];
            if (storedResult != null && !string.IsNullOrEmpty(storedResult.ToString()))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<SQLInstanceInfo>));
                using (TextReader reader = new StringReader(storedResult.ToString()))
                {
                    result = (List<SQLInstanceInfo>)serializer.Deserialize(reader);
                }
            }

            SQLInstanceInfo sqlInstanceInfo = result.FirstOrDefault(x => x.ServerName == elements[0] && x.Database == elements[1]);
            sqlInstanceInfo.Password = ACCrypt.AesDecrypt(sqlInstanceInfo.Password, ResourcesSQL.Hash);
            return new Tuple<SQLInstanceInfo, string>(sqlInstanceInfo, elements[2]);
        }

#endregion


#region overrides
        public override string ReadText(string filename)
        {
            return null;
        }
#endregion

    }
}
