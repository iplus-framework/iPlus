// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.dbsyncer.helper
{
    /// <summary>
    /// List of sql scripts
    /// </summary>
    public static class SQLScripts
    {
        public static string MaxScriptDate              = @"select max(ScriptDate) from [dbo].[@DbSyncerInfo] where rtrim(DbSyncerInfoContextID) = '{0}'";
        public static string CheckDbSyncerInfoExist     = @"select max(ScriptDate) from [dbo].[@DbSyncerInfo]";
        public static string CheckOldV4DbSyncerContext  = @"SELECT COUNT(*) as Count FROM [@DbSyncerInfoContext] WHERE ConnectionName LIKE '%V4%'";
        public static string UpdateV4ToV5References     = @"update [@DbSyncerInfoContext] set ConnectionName = 'iPlusV5_Entities' where ConnectionName like 'iPlusV4_Entities'
                                                          update [@DbSyncerInfoContext] set ConnectionName = 'iPlusMESV5_Entities' where ConnectionName like 'iPlusMESV4_Entities'";
        public static string CheckDbSyncerInfoExist_OLD = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DbInfo'";
        public static string DbSyncerInfoContextSelect  = @"select * from  [dbo].[@DbSyncerInfoContext] order by [Order]";
        public static string DbSyncerInfoContextInsert  = @"INSERT INTO [dbo].[@DbSyncerInfoContext] ([DbSyncerInfoContextID], [Name], [ConnectionName], [Order]) VALUES ('{0}', '{1}', '{2}',{3})";
        public static string DbSyncerInfoInsert         = @"INSERT INTO [dbo].[@DbSyncerInfo] ([DbSyncerInfoContextID], [ScriptDate], [UpdateDate], [UpdateAuthor]) VALUES ('{0}', {1}, {2}, '{3}')";
        public static string DbSyncerInfoDelete         = @"delete from [dbo].[@DbSyncerInfo] where [DbSyncerInfoContextID] = '{0}' and [ScriptDate] = {1}";
        public static string AllDbSyncerInfo            = @"select * from  [dbo].[@DbSyncerInfo]";
        public static string DBSyncerVersionSelect      = @"select * from  [dbo].[@DBSyncerVersion]";
        public static string DBSyncerVersionInsert      = @"INSERT INTO [dbo].[@DBSyncerVersion] ([Version], [UpdateDate]) VALUES ('{0}', getdate())";
        public static string DBSyncerVersionCreate      =
@"CREATE TABLE [dbo].[@DBSyncerVersion](
	[Version] [nvarchar](10) NOT NULL,
	[UpdateDate] [datetime] NOT NULL
 CONSTRAINT [PK_DBSyncerVersion] PRIMARY KEY CLUSTERED 
(
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT INTO [dbo].[@DBSyncerVersion] ([Version], [UpdateDate]) VALUES ('1.0.0.0', getdate())";
    }
}