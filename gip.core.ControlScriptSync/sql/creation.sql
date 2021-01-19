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