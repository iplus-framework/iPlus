CREATE TABLE [dbo].[@ControlScriptSyncInfo](
	[ControlScriptSyncInfoID] [int] IDENTITY(1,1) NOT NULL,
	[VersionTime] [datetime] NOT NULL,
	[UpdateTime] [datetime] NOT NULL,
	[UpdateAuthor] [varchar](40) NOT NULL,
 CONSTRAINT [PK_ControlScriptSyncInfo] PRIMARY KEY CLUSTERED 
(
	[ControlScriptSyncInfoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[@DbSyncerInfo](
	[DbSyncerInfoID] [int] IDENTITY(1,1) NOT NULL,
	[DbSyncerInfoContextID] [nvarchar](10) NOT NULL,
	[Version] [int] NOT NULL,
	[ScriptDate] [datetime] NOT NULL,
	[UpdateDate] [datetime] NOT NULL,
	[UpdateAuthor] [varchar](40) NOT NULL,
 CONSTRAINT [PK_DbSyncerInfo] PRIMARY KEY CLUSTERED 
(
	[DbSyncerInfoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[@DbSyncerInfoContext](
	[DbSyncerInfoContextID] [nvarchar](10) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[ConnectionName] [nvarchar](150) NOT NULL,
	[Order] [int] NOT NULL,
 CONSTRAINT [PK_DbSyncerInfoContext] PRIMARY KEY CLUSTERED 
(
	[DbSyncerInfoContextID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[@DbSyncerInfo]  WITH CHECK ADD  CONSTRAINT [FK_DbSyncerInfo_DbSyncerInfoContext] FOREIGN KEY([DbSyncerInfoContextID]) REFERENCES [dbo].[@DbSyncerInfoContext] ([DbSyncerInfoContextID])
GO

INSERT INTO [dbo].[@DbSyncerInfoContext]
(
	[DbSyncerInfoContextID]
	,[Name]
	,[ConnectionName]
	,[Order]
)
SELECT 
	[DbInfoContextID] as DbSyncerInfoContextID
    ,[Name] 
    ,[ConnectionName]
	,case when [DbInfoContextID] = 'varioiplus' then 1 else 2 end [Order]
FROM [dbo].[DbInfoContext]

INSERT INTO [dbo].[@DbSyncerInfo]
(
	[DbSyncerInfoContextID]
	,[Version]
	,[ScriptDate]
	,[UpdateDate]
	,[UpdateAuthor]
)
SELECT [DbInfoContextId] as [DbSyncerInfoContextID]
      ,[Version]
      ,[ScriptDate]
      ,[UpdateDate]
      ,[UpdateAuthor]
  FROM [dbo].[DbInfo]

INSERT INTO [dbo].[@ControlScriptSyncInfo]
(
	[VersionTime]
	,[UpdateTime]
	,[UpdateAuthor]
)
SELECT 
      [VersionTime]
      ,[UpdateTime]
      ,[UpdateAuthor]
  FROM [dbo].[VBControlScriptInfo]

GO
drop table [dbo].[VBControlScriptInfo];
drop table [dbo].[DbInfo];
drop table [dbo].[DbInfoContext];