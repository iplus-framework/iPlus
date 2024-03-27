CREATE TABLE dbo.ACClassRouteUsage
	(
	ACClassRouteUsageID uniqueidentifier NOT NULL,
	ACClassID uniqueidentifier NOT NULL,
	UseFactor int NOT NULL,
	InsertName varchar(20) NOT NULL,
	InsertDate datetime NOT NULL,
	UpdateName varchar(20) NOT NULL,
	UpdateDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ACClassRouteUsage ADD CONSTRAINT
	PK_ACClassRouteUsage PRIMARY KEY CLUSTERED 
	(
	ACClassRouteUsageID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE TABLE dbo.ACClassRouteUsagePos
	(
	ACClassRouteUsagePosID uniqueidentifier NOT NULL,
	ACClassRouteUsageID uniqueidentifier NOT NULL,
	HashCode int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ACClassRouteUsagePos ADD CONSTRAINT
	PK_ACClassRouteUsagePos PRIMARY KEY CLUSTERED 
	(
	ACClassRouteUsagePosID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

ALTER TABLE dbo.ACClassRouteUsagePos ADD CONSTRAINT
	FK_ACClassRouteUsagePos_ACClassRouteUsage FOREIGN KEY
	(
	ACClassRouteUsageID
	) REFERENCES dbo.ACClassRouteUsage
	(
	ACClassRouteUsageID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 



