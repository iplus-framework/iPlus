USE [master]

DECLARE @mdfPath nvarchar(max), @ldfPath nvarchar(max), @data nvarchar(max), @log nvarchar(max);

DECLARE @fileListTable TABLE (
    [LogicalName]           NVARCHAR(128),
    [PhysicalName]          NVARCHAR(260),
    [Type]                  CHAR(1),
    [FileGroupName]         NVARCHAR(128),
    [Size]                  NUMERIC(20,0),
    [MaxSize]               NUMERIC(20,0),
    [FileID]                BIGINT,
    [CreateLSN]             NUMERIC(25,0),
    [DropLSN]               NUMERIC(25,0),
    [UniqueID]              UNIQUEIDENTIFIER,
    [ReadOnlyLSN]           NUMERIC(25,0),
    [ReadWriteLSN]          NUMERIC(25,0),
    [BackupSizeInBytes]     BIGINT,
    [SourceBlockSize]       INT,
    [FileGroupID]           INT,
    [LogGroupGUID]          UNIQUEIDENTIFIER,
    [DifferentialBaseLSN]   NUMERIC(25,0),
    [DifferentialBaseGUID]  UNIQUEIDENTIFIER,
    [IsReadOnly]            BIT,
    [IsPresent]             BIT,
    [TDEThumbprint]         VARBINARY(32) -- remove this column if using SQL 2005
)
INSERT INTO @fileListTable EXEC('RESTORE FILELISTONLY FROM DISK = ''-location-''');
select @data = LogicalName from @fileListTable where Type = 'D';
select @log = LogicalName from @fileListTable where Type = 'L';
select @mdfPath = PhysicalName from @fileListTable where Type = 'D';
select @ldfPath = PhysicalName from @fileListTable where Type = 'L';

RESTORE DATABASE [-databaseName-] 
FROM  DISK = N'-location-' 
WITH  FILE = 1,
MOVE @data TO @mdfPath,  
MOVE @log TO @ldfPath,  
NOUNLOAD,  REPLACE,  STATS = 5


