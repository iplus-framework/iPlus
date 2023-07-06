-- author:		@aagincic
-- name:		udpACClassClone
-- desc:		clone acclass
-- created:		-
-- updated:		2020--06-03
-- deployed:	-

IF EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'udpACClassClone')
	BEGIN
		DROP  procedure  dbo.[udpACClassClone]
	END
GO

CREATE PROCEDURE [dbo].[udpACClassClone]
				
	@acClassID uniqueidentifier,
	@acIdentifier nvarchar(100),
	@IsCloneACClassProperty bit,
	@IsCloneACClassMethod bit,
	@IsCloneACClassDesign bit,
	@IsCloneACClassConfig bit,
	@IsCloneACClassText bit,
	@IsCloneACClassMessage bit,
	@IsCloneACClassPropertyRelation bit,
	@userID varchar(5)
	
AS
begin
	begin tran
	-- Inner PART
	---------------------

	-- 1.0 Declaration part
	declare @newId uniqueidentifier;
	declare @sourceTableTree table
	(
		ParentID uniqueidentifier,
		ID uniqueidentifier,
		Level int,
		ACIdentifer varchar(150),
		ParentACIdentifier varchar(150)
	);
	declare @classIDs table
	(
		oldID uniqueidentifier,
		[newID] uniqueidentifier,
		ACIdentifier nvarchar(100)
	);
	declare @propertyIDs table
	(
		oldID uniqueidentifier,
		[newID] uniqueidentifier
	);
	declare @level int;
	declare @operationTime datetime;
	-- 2 Basic setup part

	-- id for new class (known id - can be delete by testing)
	set @newId = NEWID();
	set @level = 0;
	set @operationTime = getdate();
	-- 3. Define a job
	insert into @sourceTableTree (ID, ParentID, [Level], ACIdentifer, ParentACIdentifier) 
		select top 1 
			cl.ACCLassiD, 
			cl.ParentACClassID, 
			@level,
			cl.ACIdentifier,
			pCl.ACIdentifier
		from [dbo].[ACClass] cl
		left join [dbo].[ACClass] pCl on pCl.ACClassID = cl.ParentACClassID
		where cl.ACClassID = @acClassID;

	insert into @classIDs(oldID,[newID])
	select ParentID, ParentID from @sourceTableTree

	while @level > -1
	begin
		set @level = @level + 1;
		insert into @sourceTableTree (ID, ParentID, [Level], ACIdentifer, ParentACIdentifier) 
		select
			cl.ACCLassiD, 
			cl.ParentACClassID, 
			@level,
			cl.ACIdentifier,
			pCl.ACIdentifier
		from [dbo].[ACClass] cl
		left join [dbo].[ACClass] pCl on pCl.ACClassID = cl.ParentACClassID
		where cl.ParentACClassID in
			(select ID from @sourceTableTree where [Level] = (@level - 1));
		if (select count(ID) from @sourceTableTree where [Level] = @level) = 0
		begin
			set @level = -1;
		end
	end
	insert into @classIDs (oldID, [newID], ACIdentifier)
		select Cs.ID, NEWID(), Cl.ACIdentifier  from @sourceTableTree Cs
		inner join ACClass Cl on Cl.ACClassID = Cs.ID

	update @classIDs
	set ACIdentifier = @acIdentifier
	where oldID = @acClassID

	update @classIDs
	set newID = @newId
	where oldID = @acClassID


	INSERT INTO [dbo].[ACClass]
		([ACClassID]
		,[ACProjectID]
		,[BasedOnACClassID]
		,[ParentACClassID]
		,[ACIdentifier]
		,[ACIdentifierKey]
		,[ACCaptionTranslation]
		,[ACKindIndex]
		,[SortIndex]
		,[ACPackageID]
		,[AssemblyQualifiedName]
		,[PWACClassID]
		,[PWMethodACClassID]
		,[Comment]
		,[IsAutostart]
		,[IsAbstract]
		,[ACStartTypeIndex]
		,[ACStorableTypeIndex]
		,[IsAssembly]
		,[IsMultiInstance]
		,[IsRightmanagement]
		,[ACSortColumns]
		,[ACFilterColumns]
		,[XMLConfig]
		,[XMLACClass]
		,[BranchNo]
		,[ChangeLogMax]
		--,[ACURLCached]
		--,[ACURLComponentCached]
		,[IsStatic]
		,[InsertName]
		,[InsertDate]
		,[UpdateName]
		,[UpdateDate])
	SELECT 
		NewIDs.newID as ACClassID
		,Cl.[ACProjectID]
		,Cl.[BasedOnACClassID]
		,ParentNewId.[newID] as ParentACClassID
		,NewIDs.ACIdentifier as ACIdentifier
		,Cl.[ACIdentifierKey]
		,Cl.[ACCaptionTranslation]
		,Cl.[ACKindIndex]
		,Cl.[SortIndex]
		,Cl.[ACPackageID]
		,Cl.[AssemblyQualifiedName]
		,Cl.[PWACClassID]
		,Cl.[PWMethodACClassID]
		,Cl.[Comment]
		,Cl.[IsAutostart]
		,Cl.[IsAbstract]
		,Cl.[ACStartTypeIndex]
		,Cl.[ACStorableTypeIndex]
		,Cl.[IsAssembly]
		,Cl.[IsMultiInstance]
		,Cl.[IsRightmanagement]
		,Cl.[ACSortColumns]
		,Cl.[ACFilterColumns]
		,Cl.[XMLConfig]
		,Cl.[XMLACClass]
		,Cl.[BranchNo]
		,Cl.[ChangeLogMax]
		--,Cl.[ACURLCached]
		--,Cl.[ACURLComponentCached]
		,Cl.[IsStatic]
		,@userID as InsertName
		,@operationTime as InsertDate
		,@userID as UpdateName
		,@operationTime as UpdateDate
	FROM [dbo].[ACClass] Cl
		inner join @sourceTableTree Cs on Cs.ID = cl.ACClassID
		left join @classIDs NewIDs on NewIDs.oldID = cl.ACClassID
		left join @classIDs ParentNewId on ParentNewId.oldID = cl.ParentACClassID
			--update Cl
			--set Cl.ACIdentifier = @acIdentifier
			--from ACClass Cl 
			--inner join @classIDs classIDs on classIDs.[newID] = Cl.ACClassID
			--where classIDs.oldID = @acClassID

	if @IsCloneACClassProperty = 1
	begin
		insert into @propertyIDs(oldID,[newID])
		select
			Pr.[ACClassPropertyID] as oldID,
			NEWID() as [newID]
		FROM [dbo].[ACClassProperty] Pr
		inner join @classIDs classIDs on classIDs.oldID = Pr.ACClassID

		INSERT INTO [dbo].[ACClassProperty]
			([ACClassPropertyID]
			,[ACClassID]
			,[ACIdentifier]
			,[ACIdentifierKey]
			,[ACCaptionTranslation]
			,[ACGroup]
			,[BasedOnACClassPropertyID]
			,[ACKindIndex]
			,[SortIndex]
			,[IsRightmanagement]
			,[ACSource]
			,[Comment]
			,[IsInteraction]
			,[ValueTypeACClassID]
			,[GenericType]
			,[ConfigACClassID]
			,[ACPropUsageIndex]
			,[DeleteActionIndex]
			,[IsBroadcast]
			,[ForceBroadcast]
			,[IsProxyProperty]
			,[IsInput]
			,[IsOutput]
			,[IsContent]
			,[IsPersistable]
			,[IsSerializable]
			,[IsEnumerable]
			,[ACPointCapacity]
			,[CallbackMethodName]
			,[ParentACClassPropertyID]
			,[DataTypeLength]
			,[IsNullable]
			,[InputMask]
			,[MinLength]
			,[MaxLength]
			,[MinValue]
			,[MaxValue]
			,[XMLValue]
			,[LogRefreshRateIndex]
			,[LogFilter]
			,[Precision]
			,[XMLACEventArgs]
			,[XMLConfig]
			,[BranchNo]
			,[InsertName]
			,[InsertDate]
			,[UpdateName]
			,[UpdateDate]
			,[IsRPCEnabled]
			,[RemotePropID]
			,[LogBufferSize]
			,[IsStatic])
		SELECT 
				propertyIDs.[newID] as [ACClassPropertyID]
			,classIDs.[newID] as [ACClassID]
			,Pr.[ACIdentifier]
			,Pr.[ACIdentifierKey]
			,Pr.[ACCaptionTranslation]
			,Pr.[ACGroup]
			,isnull(basedPrId.[newID], Pr.[BasedOnACClassPropertyID]) as BasedOnACClassPropertyID
			,Pr.[ACKindIndex]
			,Pr.[SortIndex]
			,Pr.[IsRightmanagement]
			,Pr.[ACSource]
			,Pr.[Comment]
			,Pr.[IsInteraction]
			,Pr.[ValueTypeACClassID]
			,Pr.[GenericType]
			,Pr.[ConfigACClassID]
			,Pr.[ACPropUsageIndex]
			,Pr.[DeleteActionIndex]
			,Pr.[IsBroadcast]
			,Pr.[ForceBroadcast]
			,Pr.[IsProxyProperty]
			,Pr.[IsInput]
			,Pr.[IsOutput]
			,Pr.[IsContent]
			,Pr.[IsPersistable]
			,Pr.[IsSerializable]
			,Pr.[IsEnumerable]
			,Pr.[ACPointCapacity]
			,Pr.[CallbackMethodName]
			,Pr.[ParentACClassPropertyID]
			,Pr.[DataTypeLength]
			,Pr.[IsNullable]
			,Pr.[InputMask]
			,Pr.[MinLength]
			,Pr.[MaxLength]
			,Pr.[MinValue]
			,Pr.[MaxValue]
			,Pr.[XMLValue]
			,Pr.[LogRefreshRateIndex]
			,Pr.[LogFilter]
			,Pr.[Precision]
			,Pr.[XMLACEventArgs]
			,Pr.[XMLConfig]
			,Pr.[BranchNo]
			,@userID as [InsertName]
			,@operationTime as [InsertDate]
			,@userID as [UpdateName]
			,@operationTime as [UpdateDate]
			,Pr.[IsRPCEnabled]
			,Pr.[RemotePropID]
			,Pr.[LogBufferSize]
			,Pr.[IsStatic]
		FROM [dbo].[ACClassProperty] Pr
			inner join @classIDs classIDs on classIDs.oldID = Pr.ACClassID
			inner join @propertyIDs propertyIDs on propertyIDs.oldID = Pr.ACClassPropertyID
			left join @propertyIDs basedPrId on basedPrId.oldID = Pr.ACClassPropertyID
		where classIDs.oldID <> classIDs.newID
		order by pr.ACIdentifier
	end

	if @IsCloneACClassMethod = 1
	begin
		INSERT INTO [dbo].[ACClassMethod]
			([ACClassMethodID]
			,[ACClassID]
			,[ACIdentifier]
			,[ACIdentifierKey]
			,[ACCaptionTranslation]
			,[ACGroup]
			,[Sourcecode]
			,[ACKindIndex]
			,[SortIndex]
			,[IsRightmanagement]
			,[Comment]
			,[IsCommand]
			,[IsInteraction]
			,[IsAsyncProcess]
			,[IsPeriodic]
			,[IsParameterACMethod]
			,[IsSubMethod]
			,[InteractionVBContent]
			,[IsAutoenabled]
			,[IsPersistable]
			,[PWACClassID]
			,[ContinueByError]
			,[ValueTypeACClassID]
			,[GenericType]
			,[ParentACClassMethodID]
			,[XMLACMethod]
			,[XMLDesign]
			,[XMLConfig]
			,[BranchNo]
			,[InsertName]
			,[InsertDate]
			,[UpdateName]
			,[UpdateDate]
			,[ContextMenuCategoryIndex]
			,[IsRPCEnabled]
			,[AttachedFromACClassID]
			,[IsStatic]
			,[ExecuteByDoubleClick])
		SELECT  
			newid() [ACClassMethodID]
			,classIDs.[newID] as [ACClassID]
			,Mth.[ACIdentifier]
			,Mth.[ACIdentifierKey]
			,Mth.[ACCaptionTranslation]
			,Mth.[ACGroup]
			,Mth.[Sourcecode]
			,Mth.[ACKindIndex]
			,Mth.[SortIndex]
			,Mth.[IsRightmanagement]
			,Mth.[Comment]
			,Mth.[IsCommand]
			,Mth.[IsInteraction]
			,Mth.[IsAsyncProcess]
			,Mth.[IsPeriodic]
			,Mth.[IsParameterACMethod]
			,Mth.[IsSubMethod]
			,Mth.[InteractionVBContent]
			,Mth.[IsAutoenabled]
			,Mth.[IsPersistable]
			,Mth.[PWACClassID]
			,Mth.[ContinueByError]
			,Mth.[ValueTypeACClassID]
			,Mth.[GenericType]
			,Mth.[ParentACClassMethodID]
			,Mth.[XMLACMethod]
			,Mth.[XMLDesign]
			,Mth.[XMLConfig]
			,Mth.[BranchNo]
			,@userID as [InsertName]
			,@operationTime as [InsertDate]
			,@userID as [UpdateName]
			,@operationTime as [UpdateDate]
			,Mth.[ContextMenuCategoryIndex]
			,Mth.[IsRPCEnabled]
			,Mth.[AttachedFromACClassID]
			,Mth.[IsStatic]
			,Mth.[ExecuteByDoubleClick]
		FROM [dbo].[ACClassMethod] Mth
		inner join @classIDs classIDs on classIDs.oldID = Mth.ACClassID
		where classIDs.oldID <> classIDs.newID
	end

	if @IsCloneACClassDesign = 1
	begin
		INSERT INTO [dbo].[ACClassDesign]
			([ACClassDesignID]
			,[ACClassID]
			,[ACIdentifier]
			,[ACIdentifierKey]
			,[ACCaptionTranslation]
			,[ACGroup]
			,[XMLDesign]
			,[DesignBinary]
			,[DesignNo]
			,[ValueTypeACClassID]
			,[ACKindIndex]
			,[ACUsageIndex]
			,[SortIndex]
			,[IsRightmanagement]
			,[Comment]
			,[IsDefault]
			,[IsResourceStyle]
			,[VisualHeight]
			,[VisualWidth]
			,[XMLConfig]
			,[BranchNo]
			,[DesignerMaxRecursion]
			,[InsertName]
			,[InsertDate]
			,[UpdateName]
			,[UpdateDate]
			,[BAMLDesign]
			,[BAMLDate])
		SELECT 
			newid() ACClassDesignID
			,classIDs.[newID] as [ACClassID]
			,De.[ACIdentifier]
			,De.[ACIdentifierKey]
			,De.[ACCaptionTranslation]
			,De.[ACGroup]
			,De.[XMLDesign]
			,De.[DesignBinary]
			,De.[DesignNo]
			,De.[ValueTypeACClassID]
			,De.[ACKindIndex]
			,De.[ACUsageIndex]
			,De.[SortIndex]
			,De.[IsRightmanagement]
			,De.[Comment]
			,De.[IsDefault]
			,De.[IsResourceStyle]
			,De.[VisualHeight]
			,De.[VisualWidth]
			,De.[XMLConfig]
			,De.[BranchNo]
			,De.[DesignerMaxRecursion]
			,@userID as [InsertName]
			,@operationTime as [InsertDate]
			,@userID as [UpdateName]
			,@operationTime as [UpdateDate]
			,De.[BAMLDesign]
			,De.[BAMLDate]
		FROM [dbo].[ACClassDesign] De
			inner join @classIDs classIDs on classIDs.oldID = De.ACClassID
		where classIDs.oldID <> classIDs.newID
	end

	if @IsCloneACClassConfig = 1
	begin
		INSERT INTO [dbo].[ACClassConfig]
			([ACClassConfigID]
			,[ACClassID]
			,[ACClassPropertyRelationID]
			,[ParentACClassConfigID]
			,[ValueTypeACClassID]
			,[KeyACUrl]
			,[PreConfigACUrl]
			,[LocalConfigACUrl]
			,[Expression]
			,[Comment]
			,[XMLConfig]
			,[BranchNo]
			,[InsertName]
			,[InsertDate]
			,[UpdateName]
			,[UpdateDate])
		SELECT 
			newid () as [ACClassConfigID]
			,classIDs.[newID] as [ACClassID]
			,Config.[ACClassPropertyRelationID]
			,Config.[ParentACClassConfigID]
			,Config.[ValueTypeACClassID]
			,Config.[KeyACUrl]
			,Config.[PreConfigACUrl]
			,Config.[LocalConfigACUrl]
			,Config.[Expression]
			,Config.[Comment]
			,Config.[XMLConfig]
			,Config.[BranchNo]
			,@userID as [InsertName]
			,@operationTime as [InsertDate]
			,@userID as [UpdateName]
			,@operationTime as [UpdateDate]
		FROM [dbo].[ACClassConfig] Config
			inner join @classIDs classIDs on classIDs.oldID = Config.ACClassID
		where classIDs.oldID <> classIDs.newID
	end

	if @IsCloneACClassText = 1
	begin
		INSERT INTO [dbo].[ACClassText]
			([ACClassTextID]
			,[ACClassID]
			,[ACIdentifier]
			,[ACIdentifierKey]
			,[ACCaptionTranslation]
			,[BranchNo]
			,[InsertName]
			,[InsertDate]
			,[UpdateName]
			,[UpdateDate])
		SELECT
			NEWID() as [ACClassTextID]
			,classIDs.[newID] as [ACClassID]
			,Txt.[ACIdentifier]
			,Txt.[ACIdentifierKey]
			,Txt.[ACCaptionTranslation]
			,Txt.[BranchNo]
			,@userID as [InsertName]
			,@operationTime as [InsertDate]
			,@userID as [UpdateName]
			,@operationTime as [UpdateDate]
		FROM [dbo].[ACClassText] Txt
			inner join @classIDs classIDs on classIDs.oldID = Txt.ACClassID
		where classIDs.oldID <> classIDs.newID
	end

	if @IsCloneACClassMessage = 1
	begin
		INSERT INTO [dbo].[ACClassMessage]
			([ACClassMessageID]
			,[ACClassID]
			,[ACIdentifier]
			,[ACIdentifierKey]
			,[ACCaptionTranslation]
			,[BranchNo]
			,[InsertName]
			,[InsertDate]
			,[UpdateName]
			,[UpdateDate])
		SELECT 
			newid() as [ACClassMessageID]
			,classIDs.[newID] as [ACClassID]
			,Msg.[ACIdentifier]
			,Msg.[ACIdentifierKey]
			,Msg.[ACCaptionTranslation]
			,Msg.[BranchNo]
			,@userID as [InsertName]
			,@operationTime as [InsertDate]
			,@userID as [UpdateName]
			,@operationTime as [UpdateDate]
		FROM [dbo].[ACClassMessage] Msg
			inner join @classIDs classIDs on classIDs.oldID = Msg.ACClassID
		where classIDs.oldID <> classIDs.newID
	end

	if @IsCloneACClassPropertyRelation = 1
	begin
		INSERT INTO [dbo].[ACClassPropertyRelation]
			([ACClassPropertyRelationID]
			,[SourceACClassID]
			,[SourceACClassPropertyID]
			,[TargetACClassID]
			,[TargetACClassPropertyID]
			,[ConnectionTypeIndex]
			,[DirectionIndex]
			,[XMLValue]
			,[LogicalOperationIndex]
			,[InsertName]
			,[InsertDate]
			,[UpdateName]
			,[UpdateDate]
			,[Multiplier]
			,[Divisor]
			,[ConvExpressionT]
			,[ConvExpressionS]
			,[IsDeactivated]
			,[DisplayGroup])
		SELECT
		newid() as [ACClassPropertyRelationID]
		,isnull(sourceCL.[newID], PRel.SourceACClassID) as [SourceACClassID]
		,isnull(sourcePr.[newID], PRel.SourceACClassPropertyID) as [SourceACClassPropertyID]
		,isnull(targetCL.[newID], PRel.TargetACClassID) as [TargetACClassID]
		,isnull(targetPr.[newID], PRel.TargetACClassPropertyID) as [TargetACClassPropertyID]
		,PRel.[ConnectionTypeIndex]
		,PRel.[DirectionIndex]
		,PRel.[XMLValue]
		,PRel.[LogicalOperationIndex]
		,@userID as [InsertName]
		,@operationTime as [InsertDate]
		,@userID as [UpdateName]
		,@operationTime as [UpdateDate]
		,PRel.[Multiplier]
		,PRel.[Divisor]
		,PRel.[ConvExpressionT]
		,PRel.[ConvExpressionS]
		,PRel.[IsDeactivated]
		,PRel.[DisplayGroup]
		FROM [dbo].[ACClassPropertyRelation] PRel
			left join @classIDs sourceCL on sourceCL.oldID = PRel.SourceACClassID
			left join @classIDs targetCL on targetCL.oldID = PRel.TargetACClassID
			left join @propertyIDs sourcePr on sourcePr.oldID = PRel.SourceACClassPropertyID
			left join @propertyIDs targetPr on targetPr.oldID = PRel.TargetACClassPropertyID
		where 
		sourceCL.oldID <> sourceCL.newID and
		PRel.ACClassPropertyRelationID in 
		(
			select distinct PRel.[ACClassPropertyRelationID] FROM [dbo].[ACClassPropertyRelation] PRel
			where 
				PRel.SourceACClassPropertyID in (select oldID from @propertyIDs) OR
				PRel.TargetACClassPropertyID in (select oldID from @propertyIDs)
		);
	end
	commit tran;
end