alter table [dbo].[ACClassMethod] add HasRequiredParams bit null;
GO
update [dbo].[ACClassMethod] set HasRequiredParams = 0
alter table [dbo].[ACClassMethod] alter column HasRequiredParams bit not null;