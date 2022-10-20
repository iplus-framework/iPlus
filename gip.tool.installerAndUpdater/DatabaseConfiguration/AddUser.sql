USE[-databaseName-]
GO

if database_principal_id('-username-') is not null
    drop user [-username-];
GO

CREATE USER [-username-] FOR LOGIN [-username-]
GO

ALTER ROLE [db_datareader] ADD MEMBER [-username-]
GO

ALTER ROLE [db_datawriter] ADD MEMBER [-username-]