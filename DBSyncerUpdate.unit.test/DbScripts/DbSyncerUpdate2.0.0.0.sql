delete from [dbo].[@DbSyncerInfo];
alter table [dbo].[@DbSyncerInfo] drop column [Version];
GO
CREATE UNIQUE NONCLUSTERED INDEX SyncerScriptUniqueTime ON dbo.[@DbSyncerInfo]
(
	DbSyncerInfoContextID,
	ScriptDate
) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
if((select
	COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS vv
WHERE  
	TABLE_SCHEMA='dbo'  
	and vv.COLUMN_NAME = N'Order'
	and table_name = N'@DbSyncerInfoContext') = 0)
begin
exec('ALTER TABLE [dbo].[@DbSyncerInfoContext] ADD [Order] int null;');

	exec('update [dbo].[@DbSyncerInfoContext] set [Order] = 1 where DbSyncerInfoContextID = N''varioiplus'';
	update [dbo].[@DbSyncerInfoContext] set [Order] = 2 where DbSyncerInfoContextID = N''variobatch'';
	update [dbo].[@DbSyncerInfoContext] set [Order] = 3 where DbSyncerInfoContextID = N''vbcoffee'';');
	
	exec('ALTER TABLE [dbo].[@DbSyncerInfoContext] ALTER COLUMN [Order] int not null;');
end