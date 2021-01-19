IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'udpTranslation')
	BEGIN
		DROP  procedure  dbo.[udpTranslation]
	END
GO
CREATE PROCEDURE [dbo].[udpTranslation]
	@onlyClassTables bit,
	@onlyMDTables bit,
	@searchClassACIdentifier varchar(20),
	@searchACIdentifier varchar(20),
	@searchTranslation varchar(20)
AS
begin
	begin tran
		
		-- # ACClass Tables
		if @onlyClassTables is null or @onlyClassTables = 1
		begin
			-- declare internal variables
			IF OBJECT_ID('dbo.#translationViewResults') IS NULL
				begin
					CREATE TABLE #translationViewResults
					(
						JobID					uniqueidentifier,
						ACProjectName			varchar(200),
						TableName				varchar(150),
						MandatoryID				uniqueidentifier,
						MandatoryACIdentifier	varchar(200),
						ID						uniqueidentifier,
						ACIdentifier			varchar(200),
						TranslationValue		varchar(max),
						UpdateName				varchar(5),
						UpdateDate				datetime
					);
				end
				declare @jobID uniqueidentifier;
				declare @cnt int;
				-- start process
				set @jobID = NEWID();

				-- ACClass
				insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
				select
					@jobID as					JobID,
					Pr.ACProjectName as			ACProjectName,
					N'ACClass' as				TableName,
					cl.ACClassID as				MandatoryID,
					cl.ACIdentifier as			MandatoryACIdentifier,
					cl.ACClassID as				ID,
					cl.ACIdentifier as			ACIdentifier,
					cl.ACCaptionTranslation as	TranslationValue,
					cl.UpdateName as			UpdateName,
					cl.UpdateDate as			UpdateDate
				from ACClass cl
				inner join ACProject pr on pr.ACProjectID = cl.ACProjectID
					where 
						(@searchClassACIdentifier is null or lower(cl.ACIdentifier) like '%' + @searchClassACIdentifier + '%') and
						(@searchACIdentifier is null or lower(cl.ACIdentifier) like '%' + @searchACIdentifier + '%') and
						(@searchTranslation is null or LOWER(cl.ACCaptionTranslation) like '%' + @searchTranslation + '%')
					order by cl.ACIdentifier

				--ACClassMessage
				insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
				select
					@jobID as					JobID,
					cl.ACIdentifier as			ACProjectName,
					'ACClassMessage' as			TableName,
					cl.ACClassID as				MandatoryID,
					cl.ACIdentifier as			MandatoryACIdentifier,
					msg.ACClassMessageID as		ID,
					msg.ACIdentifier as			ACIdentifier,
					msg.ACCaptionTranslation as TranslationValue,
					msg.UpdateName	as			UpdateName,
					msg.UpdateDate	as			UpdateDate
				from ACClassMessage msg
				inner join ACClass cl on cl.ACClassID = msg.ACClassID
					where 
						(@searchClassACIdentifier is null or lower(cl.ACIdentifier) like '%' + @searchClassACIdentifier + '%') and
						(@searchACIdentifier is null or lower(msg.ACIdentifier) like '%' + @searchACIdentifier + '%') and
						(@searchTranslation is null or LOWER(msg.ACCaptionTranslation) like '%' + @searchTranslation + '%')
					order by msg.ACIdentifier

				--ACClassMethod
				insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
				select
					@jobID as					JobID,
					cl.ACIdentifier as			ACProjectName,
					N'ACClassMethod' as			TableName,
					cl.ACClassID as				MandatoryID,
					cl.ACIdentifier as			MandatoryACIdentifier,
					mth.ACClassID as			ID,
					mth.ACIdentifier as			ACIdentifier,
					mth.ACCaptionTranslation as TranslationValue,
					mth.UpdateName as			UpdateName,
					mth.UpdateDate as			UpdateDate
				from ACClassMethod mth
				inner join ACClass cl on cl.ACClassID = mth.ACClassID
					where 
						(@searchClassACIdentifier is null or lower(cl.ACIdentifier) like '%' + @searchClassACIdentifier + '%') and
						(@searchACIdentifier is null or lower(mth.ACIdentifier) like '%' + @searchACIdentifier + '%') and
						(@searchTranslation is null or LOWER(mth.ACCaptionTranslation) like '%' + @searchTranslation + '%')
					order by mth.ACIdentifier
			
				--ACClassProperty
				insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
				select
					@jobID as					JobID,
					cl.ACIdentifier as			ACProjectName,
					N'ACClassProperty' as		TableName,
					cl.ACClassID as				MandatoryID,
					cl.ACIdentifier as			MandatoryACIdentifier,
					prop.ACClassID as			ID,
					prop.ACIdentifier as		ACIdentifier,
					prop.ACCaptionTranslation as TranslationValue,
					prop.UpdateName as			UpdateName,
					prop.UpdateDate as			UpdateDate
				from ACClassProperty prop
				inner join ACClass cl on cl.ACClassID = prop.ACClassID
					where 
						(@searchClassACIdentifier is null or lower(cl.ACIdentifier) like '%' + @searchClassACIdentifier + '%') and
						(@searchACIdentifier is null or lower(prop.ACIdentifier) like '%' + @searchACIdentifier + '%') and
						(@searchTranslation is null or LOWER(prop.ACCaptionTranslation) like '%' + @searchTranslation + '%')
					order by prop.ACIdentifier

				--ACClassText
				insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
				select
					@jobID as					JobID,
					cl.ACIdentifier as			ACProjectName,
					N'ACClassText' as			TableName,
					cl.ACClassID as				MandatoryID,
					cl.ACIdentifier as			MandatoryACIdentifier,
					txt.ACClassID as			ID,
					txt.ACIdentifier as			ACIdentifier,
					txt.ACCaptionTranslation as TranslationValue,
					txt.UpdateName as			UpdateName,
					txt.UpdateDate as			UpdateDate
				from ACClassText txt
				inner join ACClass cl on cl.ACClassID = txt.ACClassID
					where 
						(@searchClassACIdentifier is null or lower(cl.ACIdentifier) like '%' + @searchClassACIdentifier + '%') and
						(@searchACIdentifier is null or lower(txt.ACIdentifier) like '%' + @searchACIdentifier + '%') and
						(@searchTranslation is null or LOWER(txt.ACCaptionTranslation) like '%' + @searchTranslation + '%')
					order by txt.ACIdentifier
			
				--ACClassDesign
				insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
				select
					@jobID as					JobID,
					cl.ACIdentifier as			ACProjectName,
					N'ACClassDesign' as			TableName,
					cl.ACClassID as				MandatoryID,
					cl.ACIdentifier as			MandatoryACIdentifier,
					de.ACClassID as				ID,
					de.ACIdentifier as			ACIdentifier,
					de.ACCaptionTranslation as TranslationValue,
					de.UpdateName as			UpdateName,
					de.UpdateDate as			UpdateDate
				from ACClassDesign de
				inner join ACClass cl on cl.ACClassID = de.ACClassID
					where 
						(@searchClassACIdentifier is null or lower(cl.ACIdentifier) like '%' + @searchClassACIdentifier + '%') and
						(@searchACIdentifier is null or lower(de.ACIdentifier) like '%' + @searchACIdentifier + '%') and
						(@searchTranslation is null or LOWER(de.ACCaptionTranslation) like '%' + @searchTranslation + '%')
					order by de.ACIdentifier
			end -- end cause only class tables
			
		-- # MDTables
		if @onlyMDTables is null or @onlyMDTables = 1
		begin
				-- MDBalancingMode
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDBalancingMode' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDBalancingModeID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDBalancingMode md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDBookingNotAvailableMode
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as							JobID,
				null as							ACProjectName,
				'MDBookingNotAvailableMode' as		TableName,
				null as							MandatoryID,
				null as							MandatoryACIdentifier,
				md.MDBookingNotAvailableModeID as	ID,
				md.MDKey as						ACIdentifier,
				md.MDNameTrans as					TranslationValue,
				md.UpdateName as					UpdateName,
				md.UpdateDate as					UpdateDate
			from MDBookingNotAvailableMode md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDCostCenter
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as				JobID,
				null as				ACProjectName,
				'MDCostCenter' as		TableName,
				null as				MandatoryID,
				null as				MandatoryACIdentifier,
				md.MDCostCenterID as	ID,
				md.MDKey as			ACIdentifier,
				md.MDNameTrans as		TranslationValue,
				md.UpdateName as		UpdateName,
				md.UpdateDate as		UpdateDate
			from MDCostCenter md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey
					
				-- MDCountry
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as			JobID,
				null as			ACProjectName,
				'MDCountry' as		TableName,
				null as			MandatoryID,
				null as			MandatoryACIdentifier,
				md.MDCountryID as	ID,
				md.MDKey as		ACIdentifier,
				md.MDNameTrans as	TranslationValue,
				md.UpdateName as	UpdateName,
				md.UpdateDate as	UpdateDate
			from MDCountry md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDCountryLand
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as				JobID,
				null as				ACProjectName,
				'MDCountryLand' as		TableName,
				null as				MandatoryID,
				null as				MandatoryACIdentifier,
				md.MDCountryLandID as	ID,
				md.MDKey as			ACIdentifier,
				md.MDNameTrans as		TranslationValue,
				md.UpdateName as		UpdateName,
				md.UpdateDate as		UpdateDate
			from MDCountryLand md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDCountrySalesTax
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDCountrySalesTax' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDCountrySalesTaxID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDCountrySalesTax md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey
					
				-- MDCurrency
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as				JobID,
				null as				ACProjectName,
				'MDCurrency' as		TableName,
				null as				MandatoryID,
				null as				MandatoryACIdentifier,
				md.MDCurrencyID as		ID,
				md.MDKey as			ACIdentifier,
				md.MDNameTrans as		TranslationValue,
				md.UpdateName as		UpdateName,
				md.UpdateDate as		UpdateDate
			from MDCurrency md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDDelivNoteState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDDelivNoteState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDDelivNoteStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDDelivNoteState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDDelivPosLoadState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as						JobID,
				null as						ACProjectName,
				'MDDelivPosLoadState' as		TableName,
				null as						MandatoryID,
				null as						MandatoryACIdentifier,
				md.MDDelivPosLoadStateID as	ID,
				md.MDKey as					ACIdentifier,
				md.MDNameTrans as				TranslationValue,
				md.UpdateName as				UpdateName,
				md.UpdateDate as				UpdateDate
			from MDDelivPosLoadState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDDelivPosState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDDelivPosState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDDelivPosStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDDelivPosState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDDelivType
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as				JobID,
				null as				ACProjectName,
				'MDDelivType' as		TableName,
				null as				MandatoryID,
				null as				MandatoryACIdentifier,
				md.MDDelivTypeID as	ID,
				md.MDKey as			ACIdentifier,
				md.MDNameTrans as		TranslationValue,
				md.UpdateName as		UpdateName,
				md.UpdateDate as		UpdateDate
			from MDDelivType md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDDemandOrderState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as						JobID,
				null as						ACProjectName,
				'MDDemandOrderState' as		TableName,
				null as						MandatoryID,
				null as						MandatoryACIdentifier,
				md.MDDemandOrderStateID as		ID,
				md.MDKey as					ACIdentifier,
				md.MDNameTrans as				TranslationValue,
				md.UpdateName as				UpdateName,
				md.UpdateDate as				UpdateDate
			from MDDemandOrderState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDFacilityInventoryPosState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as JobID,
				null as								ACProjectName,
				'MDFacilityInventoryPosState' as		TableName,
				null as								MandatoryID,
				null as								MandatoryACIdentifier,
				md.MDFacilityInventoryPosStateID as	ID,
				md.MDKey as							ACIdentifier,
				md.MDNameTrans as						TranslationValue,
				md.UpdateName as						UpdateName,
				md.UpdateDate as						UpdateDate
			from MDFacilityInventoryPosState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDFacilityInventoryState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as							JobID,
				null as							ACProjectName,
				'MDFacilityInventoryState' as		TableName,
				null as							MandatoryID,
				null as							MandatoryACIdentifier,
				md.MDFacilityInventoryStateID as	ID,
				md.MDKey as						ACIdentifier,
				md.MDNameTrans as					TranslationValue,
				md.UpdateName as					UpdateName,
				md.UpdateDate as					UpdateDate
			from MDFacilityInventoryState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDFacilityManagementType
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as							JobID,
				null as							ACProjectName,
				'MDFacilityManagementType' as		TableName,
				null as							MandatoryID,
				null as							MandatoryACIdentifier,
				md.MDFacilityManagementTypeID as	ID,
				md.MDKey as						ACIdentifier,
				md.MDNameTrans as					TranslationValue,
				md.UpdateName as					UpdateName,
				md.UpdateDate as					UpdateDate
			from MDFacilityManagementType md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDFacilityType
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as				JobID,
				null as				ACProjectName,
				'MDFacilityType' as	TableName,
				null as				MandatoryID,
				null as				MandatoryACIdentifier,
				md.MDFacilityTypeID as ID,
				md.MDKey as			ACIdentifier,
				md.MDNameTrans as		TranslationValue,
				md.UpdateName as		UpdateName,
				md.UpdateDate as		UpdateDate
			from MDFacilityType md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDFacilityVehicleType
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as						JobID,
				null as						ACProjectName,
				'MDFacilityVehicleType' as		TableName,
				null as						MandatoryID,
				null as						MandatoryACIdentifier,
				md.MDFacilityVehicleTypeID as	ID,
				md.MDKey as					ACIdentifier,
				md.MDNameTrans as				TranslationValue,
				md.UpdateName as				UpdateName,
				md.UpdateDate as				UpdateDate
			from MDFacilityVehicleType md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDGMPAdditive
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as				JobID,
				null as				ACProjectName,
				'MDGMPAdditive' as		TableName,
				null as				MandatoryID,
				null as				MandatoryACIdentifier,
				md.MDGMPAdditiveID as	ID,
				md.MDKey as			ACIdentifier,
				md.MDNameTrans as		TranslationValue,
				md.UpdateName as		UpdateName,
				md.UpdateDate as		UpdateDate
			from MDGMPAdditive md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDGMPMaterialGroup
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDGMPMaterialGroup' as	TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDGMPMaterialGroupID as ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDGMPMaterialGroup md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDInOrderPosState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDInOrderPosState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDInOrderPosStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDInOrderPosState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDInOrderState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDInOrderState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDInOrderStateID as		ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDInOrderState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDInOrderType
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as				JobID,
				null as				ACProjectName,
				'MDInOrderType' as		TableName,
				null as				MandatoryID,
				null as				MandatoryACIdentifier,
				md.MDInOrderTypeID as	ID,
				md.MDKey as			ACIdentifier,
				md.MDNameTrans as		TranslationValue,
				md.UpdateName as		UpdateName,
				md.UpdateDate as		UpdateDate
			from MDInOrderType md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDInRequestState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDInRequestState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDInRequestStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDInRequestState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey
					
				-- MDInventoryManagementType
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as							JobID,
				null as							ACProjectName,
				'MDInventoryManagementType' as		TableName,
				null as							MandatoryID,
				null as							MandatoryACIdentifier,
				md.MDInventoryManagementTypeID as	ID,
				md.MDKey as						ACIdentifier,
				md.MDNameTrans as					TranslationValue,
				md.UpdateName as					UpdateName,
				md.UpdateDate as					UpdateDate
			from MDInventoryManagementType md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey
					
				-- MDLabOrderPosState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDLabOrderPosState' as	TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDLabOrderPosStateID as ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDLabOrderPosState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDLabOrderState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDLabOrderState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDLabOrderStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDLabOrderState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDLabTag
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as			JobID,
				null as			ACProjectName,
				'MDLabTag' as		TableName,
				null as			MandatoryID,
				null as			MandatoryACIdentifier,
				md.MDLabTagID as	ID,
				md.MDKey as		ACIdentifier,
				md.MDNameTrans as	TranslationValue,
				md.UpdateName as	UpdateName,
				md.UpdateDate as	UpdateDate
			from MDLabTag md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDMaintMode
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as				JobID,
				null as				ACProjectName,
				'MDMaintMode' as		TableName,
				null as				MandatoryID,
				null as				MandatoryACIdentifier,
				md.MDMaintModeID as	ID,
				md.MDKey as			ACIdentifier,
				md.MDNameTrans as		TranslationValue,
				md.UpdateName as		UpdateName,
				md.UpdateDate as		UpdateDate
			from MDMaintMode md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDMaintOrderPropertyState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as							JobID,
				null as							ACProjectName,
				'MDMaintOrderPropertyState' as		TableName,
				null as							MandatoryID,
				null as							MandatoryACIdentifier,
				md.MDMaintOrderPropertyStateID as	ID,
				md.MDKey as						ACIdentifier,
				md.MDNameTrans as					TranslationValue,
				md.UpdateName as					UpdateName,
				md.UpdateDate as					UpdateDate
			from MDMaintOrderPropertyState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDMaintOrderState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDMaintOrderState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDMaintOrderStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDMaintOrderState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDMaterialGroup
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDMaterialGroup' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDMaterialGroupID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDMaterialGroup md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDMaterialType
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDMaterialType' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDMaterialTypeID as		ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDMaterialType md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDMovementReason
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDMovementReason' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDMovementReasonID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDMovementReason md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDOutOfferingState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDOutOfferingState' as	TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDOutOfferingStateID as ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDOutOfferingState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDOutOrderPlanState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as						JobID,
				null as						ACProjectName,
				'MDOutOrderPlanState' as		TableName,
				null as						MandatoryID,
				null as						MandatoryACIdentifier,
				md.MDOutOrderPlanStateID as	ID,
				md.MDKey as					ACIdentifier,
				md.MDNameTrans as				TranslationValue,
				md.UpdateName as				UpdateName,
				md.UpdateDate as				UpdateDate
			from MDOutOrderPlanState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDOutOrderPosState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDOutOrderPosState' as	TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDOutOrderPosStateID as ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDOutOrderPosState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey
					
				-- MDOutOrderState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDOutOrderState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDOutOrderStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDOutOrderState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDOutOrderType
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDOutOrderType' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDOutOrderTypeID as		ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDOutOrderType md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDProcessErrorAction
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as						JobID,
				null as						ACProjectName,
				'MDProcessErrorAction' as		TableName,
				null as						MandatoryID,
				null as						MandatoryACIdentifier,
				md.MDProcessErrorActionID as	ID,
				md.MDKey as					ACIdentifier,
				md.MDNameTrans as				TranslationValue,
				md.UpdateName as				UpdateName,
				md.UpdateDate as				UpdateDate
			from MDProcessErrorAction md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDProdOrderPartslistPosState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as								JobID,
				null as								ACProjectName,
				'MDProdOrderPartslistPosState' as		TableName,
				null as								MandatoryID,
				null as								MandatoryACIdentifier,
				md.MDProdOrderPartslistPosStateID as	ID,
				md.MDKey as							ACIdentifier,
				md.MDNameTrans as						TranslationValue,
				md.UpdateName as						UpdateName,
				md.UpdateDate as						UpdateDate
			from MDProdOrderPartslistPosState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDProdOrderState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDProdOrderState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDProdOrderStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDProdOrderState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDRatingComplaintType
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as						JobID,
				null as						ACProjectName,
				'MDRatingComplaintType' as		TableName,
				null as						MandatoryID,
				null as						MandatoryACIdentifier,
				md.MDRatingComplaintTypeID as	ID,
				md.MDKey as					ACIdentifier,
				md.MDNameTrans as				TranslationValue,
				md.UpdateName as				UpdateName,
				md.UpdateDate as				UpdateDate
			from MDRatingComplaintType md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDReleaseState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDReleaseState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDReleaseStateID as		ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDReleaseState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDReservationMode
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDReservationMode' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDReservationModeID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDReservationMode md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDTermOfPayment
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDTermOfPayment' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDTermOfPaymentID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDTermOfPayment md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDTimeRange
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as				JobID,
				null as				ACProjectName,
				'MDTimeRange' as		TableName,
				null as				MandatoryID,
				null as				MandatoryACIdentifier,
				md.MDTimeRangeID as	ID,
				md.MDKey as			ACIdentifier,
				md.MDNameTrans as		TranslationValue,
				md.UpdateName as		UpdateName,
				md.UpdateDate as		UpdateDate
			from MDTimeRange md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDToleranceState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDToleranceState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDToleranceStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDToleranceState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDTour
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as			JobID,
				null as			ACProjectName,
				'MDTour' as		TableName,
				null as			MandatoryID,
				null as			MandatoryACIdentifier,
				md.MDTourID as		ID,
				md.MDKey as		ACIdentifier,
				md.MDNameTrans as	TranslationValue,
				md.UpdateName as	UpdateName,
				md.UpdateDate as	UpdateDate
			from MDTour md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDTourplanPosState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as						JobID,
				null as						ACProjectName,
				'MDTourplanPosState' as		TableName,
				null as						MandatoryID,
				null as						MandatoryACIdentifier,
				md.MDTourplanPosStateID as		ID,
				md.MDKey as					ACIdentifier,
				md.MDNameTrans as				TranslationValue,
				md.UpdateName as				UpdateName,
				md.UpdateDate as				UpdateDate
			from MDTourplanPosState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDTourplanState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDTourplanState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDTourplanStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDTourplanState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDTransportMode
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDTransportMode' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDTransportModeID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDTransportMode md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDVisitorCardState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as						JobID,
				null as						ACProjectName,
				'MDVisitorCardState' as		TableName,
				null as						MandatoryID,
				null as						MandatoryACIdentifier,
				md.MDVisitorCardStateID as		ID,
				md.MDKey as					ACIdentifier,
				md.MDNameTrans as				TranslationValue,
				md.UpdateName as				UpdateName,
				md.UpdateDate as				UpdateDate
			from MDVisitorCardState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDVisitorVoucherState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as						JobID,
				null as						ACProjectName,
				'MDVisitorVoucherState' as		TableName,
				null as						MandatoryID,
				null as						MandatoryACIdentifier,
				md.MDVisitorVoucherStateID as	ID,
				md.MDKey as					ACIdentifier,
				md.MDNameTrans as				TranslationValue,
				md.UpdateName as				UpdateName,
				md.UpdateDate as				UpdateDate
			from MDVisitorVoucherState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey

				-- MDZeroStockState
			insert into #translationViewResults(JobID, ACProjectName, TableName, MandatoryID, MandatoryACIdentifier, ID, ACIdentifier, TranslationValue, UpdateName, UpdateDate)
			select
				@jobID as					JobID,
				null as					ACProjectName,
				'MDZeroStockState' as		TableName,
				null as					MandatoryID,
				null as					MandatoryACIdentifier,
				md.MDZeroStockStateID as	ID,
				md.MDKey as				ACIdentifier,
				md.MDNameTrans as			TranslationValue,
				md.UpdateName as			UpdateName,
				md.UpdateDate as			UpdateDate
			from MDZeroStockState md
				where 
				(@searchACIdentifier is null or lower(md.MDKey) like '%' + @searchACIdentifier + '%') and
				(@searchTranslation is null or LOWER(md.MDNameTrans) like '%' + @searchTranslation + '%')
				order by md.MDKey
		-- ## end MDTables
		end
			-- output resutl
			select 
				ACProjectName,
				TableName,
				MandatoryID,
				MandatoryACIdentifier,
				ID, 
				ACIdentifier, 
				TranslationValue,
				isnull(UpdateName, '') as UpdateName,
				isnull(UpdateDate, getdate()) as UpdateDate
			from  #translationViewResults 
				where JobID = @jobID
			order by TableName, ACIdentifier;
			-- delete result of work
			delete from #translationViewResults where JobID = @jobID;
			
			set @cnt = (select count(*) from #translationViewResults);
			-- delete temp table if not more used
			if OBJECT_ID('dbo.#translationViewResults') IS not NULL and @cnt = 0
			begin
				drop table #translationViewResults;
			end
	commit tran;
end