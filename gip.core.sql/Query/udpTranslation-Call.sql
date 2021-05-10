
DECLARE @RC int
DECLARE @onlyClassTables bit
DECLARE @onlyMDTables bit
DECLARE @searchClassACIdentifier varchar(20)
DECLARE @searchACIdentifier varchar(20)
DECLARE @searchTranslation varchar(20)


set @searchACIdentifier = 'search';

-- TODO: Set parameter values here.

EXECUTE @RC = [dbo].[udpTranslation] 
   @onlyClassTables
  ,@onlyMDTables
  ,@searchClassACIdentifier
  ,@searchACIdentifier
  ,@searchTranslation
GO


