CREATE TABLE dbo.ACProgramLogPropertyLog
	(
	ACProgramLogPropertyLogID uniqueidentifier NOT NULL,
	ACProgramLogID uniqueidentifier NOT NULL,
	ACPropertyLogID uniqueidentifier NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ACProgramLogPropertyLog ADD CONSTRAINT
	PK_ACProgramLogPropertyLog PRIMARY KEY CLUSTERED 
	(
	ACProgramLogPropertyLogID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.ACProgramLogPropertyLog ADD CONSTRAINT
	FK_ACProgramLogPropertyLog_ACPropertyLogID FOREIGN KEY
	(
	ACPropertyLogID
	) REFERENCES dbo.ACPropertyLog
	(
	ACPropertyLogID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
