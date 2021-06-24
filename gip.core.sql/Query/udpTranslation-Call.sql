
DECLARE @RC int
declare @mandatoryID uniqueidentifier
DECLARE @onlyClassTables bit
DECLARE @onlyMDTables bit
DECLARE @searchClassACIdentifier varchar(20)
DECLARE @searchACIdentifier varchar(20)
DECLARE @searchTranslation varchar(20)
DECLARE @notHaveInTranslation varchar(20);


--set @searchClassACIdentifier = 'BSOFacility'
--set @mandatoryID = '96335789-DA73-4909-8F48-F0CF089CF27B';
--set @searchTranslation = 'Zulieferer';
set @notHaveInTranslation = 'en{';

--set @onlyClassTables = 0;

-- TODO: Set parameter values here.

EXECUTE @RC = [dbo].[udpTranslation] 
   @mandatoryID
   ,@onlyClassTables
  ,@onlyMDTables
  ,@searchClassACIdentifier
  ,@searchACIdentifier
  ,@searchTranslation
  ,@notHaveInTranslation
GO


