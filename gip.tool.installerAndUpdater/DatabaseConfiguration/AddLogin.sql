USE[-databaseName-]
GO
If not Exists (select loginname from master.dbo.syslogins where name = '-username-')
	create login [-username-] from windows
