
DECLARE @RC int
DECLARE @acProjectID uniqueidentifier
DECLARE @mandatoryID uniqueidentifier
DECLARE @onlyClassTables bit
DECLARE @onlyMDTables bit
DECLARE @searchClassACIdentifier varchar(150)
DECLARE @searchACIdentifier varchar(150)
DECLARE @searchTranslation varchar(150)
DECLARE @notHaveInTranslation varchar(150)

declare @jobID uniqueidentifier;

--set @searchClassACIdentifier = 'BSOTest'
--set @mandatoryID = '96335789-DA73-4909-8F48-F0CF089CF27B';
--set @searchTranslation = 'Zulieferer';
set @notHaveInTranslation = 'hr{';

set @onlyClassTables = 1;
set @onlyMDTables = 0;

-- TODO: Set parameter values here.


EXECUTE @RC = [dbo].[udpTranslation] 
   @acProjectID
  ,@mandatoryID
  ,@onlyClassTables
  ,@onlyMDTables
  ,@searchClassACIdentifier
  ,@searchACIdentifier
  ,@searchTranslation
  ,@notHaveInTranslation
GO


