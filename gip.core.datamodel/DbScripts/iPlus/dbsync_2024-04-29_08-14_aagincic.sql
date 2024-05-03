alter table [dbo].[ACClassProperty] add IsCaptionCustomized bit null;
GO
update [dbo].[ACClassProperty] set IsCaptionCustomized = 0
alter table [dbo].[ACClassProperty] alter column IsCaptionCustomized bit not null;