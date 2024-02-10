CREATE TABLE dbo.ACClassRouteUsageGroup
	(
	ACClassRouteUsageGroupID uniqueidentifier NOT NULL,
	ACClassRouteUsageID uniqueidentifier NOT NULL,
	GroupID uniqueidentifier NOT NULL,
	UseFactor int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ACClassRouteUsageGroup ADD CONSTRAINT
	PK_ACClassRouteUsageGroup PRIMARY KEY CLUSTERED 
	(
	ACClassRouteUsageGroupID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

ALTER TABLE dbo.ACClassRouteUsageGroup ADD CONSTRAINT
	FK_ACClassRouteUsageGroup_ACClassRouteUsage FOREIGN KEY
	(
	ACClassRouteUsageID
	) REFERENCES dbo.ACClassRouteUsage
	(
	ACClassRouteUsageID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 