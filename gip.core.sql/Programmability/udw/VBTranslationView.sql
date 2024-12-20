﻿IF EXISTS (SELECT * FROM sysobjects WHERE type = 'V' AND name = 'VBTranslationView')
	BEGIN
		DROP  View dbo.[VBTranslationView]
	END
GO
CREATE VIEW [dbo].[VBTranslationView]
	AS 
select
pr.ACProjectName as							ACProjectName,
'000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000

' as								
											TableName,
cl.ACClassID as								MandatoryID,
cl.ACIdentifier as							MandatoryACIdentifier,
cl.ACURLCached as							MandatoryACURLCached,
cl.ACClassID as								ID,
cl.ACIdentifier as							ACIdentifier,
cl.ACCaptionTranslation as					TranslationValue,
isnull(cl.UpdateName, '') as				UpdateName,
isnull(cl.UpdateDate, getdate()) as			UpdateDate
from ACClass cl
inner join ACProject pr on pr.ACProjectID = cl.ACProjectID