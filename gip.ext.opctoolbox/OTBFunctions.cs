//-----------------------------------------------------------------------------
//                                                                            |
//                               Softing AG                                   |
//                        Richard-Reitzner-Allee 6                            |
//                           85540 Haar, Germany                              |
//                                                                            |
//                 This is a part of the Softing OPC Toolbox                  |
//                   Copyright (C) Softing AG 2005 - 2008                     |
//                           All Rights Reserved                              |
//                                                                            |
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
//                             OPC Toolbox .NET                               |
//                                                                            |
//  Filename    : OTBFunctions.cs                                             |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : OPC Toolbox Internal functions wrapper                      |
//                                                                            |
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Softing.OPCToolbox.OTB {	

	#region Win32 functions

	internal class Win32Functions {
		[DllImport("oleaut32.dll")]
		public static extern void VariantClear(IntPtr pVariant);
	}
	
	#endregion // Win32 functions

	#region OTB functions

	internal class OTBFunctions {
#if DEBUG
	//const string  OTB_DLL = "OTBuD.dll";
	const string  OTB_DLL = "OTBu.dll";
#elif NDEBUG 
	const string  OTB_DLL = "OTBu.dll";
#endif

		// common
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTActivate(
			byte feature, 
			String key,
			byte forceDemo);

		[DllImport(OTB_DLL)]
		public static extern int OTTrace( 
			byte traceLevel,
			uint traceMask, 
			[MarshalAs(UnmanagedType.LPWStr)]string ObjID, 
			[MarshalAs(UnmanagedType.LPWStr)]string Message);

		[DllImport(OTB_DLL)]
		public static extern int OTEnableTracing(
			uint aTraceDataMask,
			OTCTraceData aTraceData);

		[DllImport(OTB_DLL)]
		public static extern int OTFreeMemory(IntPtr Handle);

		[DllImport(OTB_DLL)]
		public static extern IntPtr OTAllocateMemory(int aSize);

		[DllImport(OTB_DLL, CharSet=CharSet.Unicode)]
		public static extern IntPtr OTVariantToString(								
								IntPtr pValue, //OTVariant*
								uint stringLength, 
								IntPtr input);		
		// client
		[DllImport(OTB_DLL)]
		public static extern int OTCInitialize(OTCInitData initData);

		[DllImport(OTB_DLL)]
		public static extern int OTCProcessCommandLine([MarshalAs(UnmanagedType.LPWStr)]string commandLine);


		[DllImport(OTB_DLL)] 
		public static extern int OTCTerminate();
		
		[DllImport(OTB_DLL,CallingConvention = CallingConvention.StdCall)]
		public static extern int OTCAdvise(OTCCallbackFunctions callbacks);	
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCAddDASession(
			string url,
			uint sessionUserData,  
			ref IntPtr objectHandle);
		
		[DllImport(OTB_DLL)]
		public static extern int OTCAddDASubscription( 
			IntPtr sessionHandle, 
			uint updateRate ,
			uint groupUserData,
			ref IntPtr objectHandle);	
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCAddDAItem(
			IntPtr groupHandle,string itemID, 
			uint itemUserData,
			ref IntPtr objectHandle);
		
		[DllImport(OTB_DLL)] 
		public static extern int OTCRemoveDASession(IntPtr objectHandle);
		
		[DllImport(OTB_DLL)]
		public static extern int OTCRemoveDASubscription(IntPtr objectHandle);

		[DllImport(OTB_DLL)]
		public static extern int OTCRemoveDAItem(IntPtr objectHandle);

		[DllImport(OTB_DLL)]
		public static extern int OTCGetAttributes(
			IntPtr objectHandle, 
			uint whatAttributes, 
			[In,Out]OTCObjectAttributes pObjectAttributes);
		
		[DllImport(OTB_DLL)]
		public static extern int OTCSetAttributes(
			IntPtr objectHandle, 
			uint whatAttributes,
			[In,Out]OTCObjectAttributes pObjectAttributes);

		[DllImport(OTB_DLL)]
		public static extern int OTCUpdateAttributes(
			IntPtr objectHandle, 
			byte fromServer, 
			uint attributeCount, 
			[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=2)]uint[] pWhatAttributes, 
			[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=2)][In,Out] int[] pResults,
			ref OTCExecutionOptions executionOptions);
		
		[DllImport(OTB_DLL)]
		public static extern int OTCUpdateDAItemAttributes(
			IntPtr subscriptionHandle, 
			int itemCount, 
			IntPtr[] itemHandles, 
			byte fromServer, 
			uint attributeCount, 
			[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=6)] uint[] whatAttributes,
			[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=6)][In,Out]int[] results,
			ref OTCExecutionOptions executionOptions);
		
		[DllImport(OTB_DLL)]
		public static extern int OTCPerformStateTransition(
			IntPtr objectHandle,
			byte deep,
			ref OTCExecutionOptions executionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCChangeTargetState(
			IntPtr objectHandle,
			byte state,
			byte deep);
		
		[DllImport(OTB_DLL)]
		public static extern int OTCGetState(
			IntPtr objectHandle,
			out byte currentState,
			out byte targetState);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCRead(
			IntPtr objectHandle,
			int count,
			[MarshalAs(UnmanagedType.LPArray)]IntPtr[] itemHandles,
			[MarshalAs(UnmanagedType.LPArray)]string[] itemIDs,
			[MarshalAs(UnmanagedType.LPArray)]string[] itemPaths,
			uint maxAge, 
			[MarshalAs(UnmanagedType.LPArray)][In,Out]OTValueData[] values,
			[In,Out]int[] results,
			ref OTCExecutionOptions executionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCWrite(
			IntPtr objectHandle,
			int count,
			[MarshalAs(UnmanagedType.LPArray)] IntPtr[] itemHandles,
			string[] itemIDs,
			string[] itemPaths,
			[MarshalAs(UnmanagedType.LPArray)]OTValueData[] values,
			[In,Out]int[] results,
			ref OTCExecutionOptions executionOptions);
		
		[DllImport(OTB_DLL)]
		public static extern int OTCGetServerStatus(
			IntPtr sessionHandle,
			[In,Out]OTCServerStatus serverStatus,
			ref OTCExecutionOptions executionOptions);
		
		[DllImport(OTB_DLL)] 
		public static extern int OTCActivateConnectionMonitor(
			IntPtr sessionHandle, 
			byte activate,
			uint checkCycle, 
			uint connect1Attempts, 
			uint connect1Cycle,
			uint connect2Cycle);
		
		[DllImport(OTB_DLL)]
		public static extern int OTCValidateDAItems(
			IntPtr subscriptionHandle,
			int itemCount, 
			IntPtr[] itemHandles,
			int[] results, 
			ref OTCExecutionOptions ExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCBrowseServer(
			string ipAddress,
			byte whatOPCSpec,
			byte whatServerData,
			ref uint serverDataCount,
			out IntPtr ppServerData,
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCBrowseAddressSpace(
			IntPtr sessionHandle,
			IntPtr addressSpaceElementHandle,
			string addressSpaceElementID,
			string addressSpaceElementPath, 
			ref OTCAddressSpaceBrowseOptions pBrowseOptions,
			ref uint pAddressSpaceElementDataCount,
			out IntPtr ppAddressSpaceElement,
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCReleaseAddressSpaceElement(
			IntPtr addressSpaceElementHandle,
			int deep);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCGetDAProperties(
			IntPtr aSessionHandle, 
			IntPtr anAddressSpaceElementHandle, 
			string addressSpaceElementID, 
			string addressSpaceElementPath, 
			OTCGetDAPropertiesOptions pGetPropertiesOptions, 
			out uint pPropertiesDataCount, 
			out IntPtr ppProperty, 
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCAddAESession( 
			string url, 
			uint sessionUserData, 
			ref IntPtr pObjectHandle);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCAddAESubscription(
			IntPtr sessionHandle,
			uint subscriptionUserData,
			ref IntPtr pSubscriptionHandle);
	
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCRefreshDAGroup(
			IntPtr subscriptionHandle, 
			uint maxAge, 
			ref OTCExecutionOptions executionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCRemoveAESession(IntPtr objectHandle);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCRemoveAESubscription(IntPtr objectHandle);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCRefreshAEConditions(
			IntPtr subscriptionHandle, 
			byte cancelRefresh, 
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCAcknowledgeAEConditions(
			IntPtr sessionHandle, 
			[MarshalAs(UnmanagedType.LPWStr)] string ackID, 
			[MarshalAs(UnmanagedType.LPWStr)] string ackComment, 
			uint count, 
			[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)] OTCEventData[] pEvents, 
			[In,Out]int[] results,
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCQueryAvailableAEFilters(
			IntPtr sessionHandle,
			out uint availableFilters,
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCQueryAECategories(
			IntPtr sessionHandle, 
			out uint categoryCount, 
			out IntPtr ppEventTypes, 
			out IntPtr ppCategoryIds, 
			out IntPtr ppCategoryDescriptions, 
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCQueryAEAttributes(
			IntPtr sessionHandle, 
			uint categoryId, 
			out uint pCount, 
			out IntPtr ppAttributeIds, 
			out IntPtr ppAttributeDescriptions, 
			out IntPtr ppAttributeDatatypes, 
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCQueryAEConditions(
			IntPtr sessionHandle, 
			uint categoryId, 
			out uint pCount, 
			out IntPtr ppConditionsNames, 
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCQueryAESubConditions(
			IntPtr sessionHandle, 
			[MarshalAs(UnmanagedType.LPWStr)]string conditionName, 
			out uint pCount,
			out IntPtr ppSubConditionsNames, 
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCQueryAESourceConditions(
			IntPtr sessionHandle, 
			[MarshalAs(UnmanagedType.LPWStr)]string sourcePath, 
			out uint pCount, 
			out IntPtr ppConditionsNames, 
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCGetAEConditionState(
			IntPtr sessionHandle, 
			[MarshalAs(UnmanagedType.LPWStr)]string sourcePath, 
			[MarshalAs(UnmanagedType.LPWStr)]string conditionName, 
			uint  attributeCount, 
			[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)] uint[] pAttributeIds, 
			[In,Out] OTCAEConditionStateData pConditionState, 
			//out IntPtr pConditionState,
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCEnableAEConditions(
			IntPtr sessionHandle, 
			byte enable, 
			byte areaOrSource, 
			[MarshalAs(UnmanagedType.LPWStr)]string path, 
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCGetErrorString(
			IntPtr sessionHandle, 
			int errorCode, 
			out IntPtr errorString, 
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCLogon(
			IntPtr sessionHandle, 
			[MarshalAs(UnmanagedType.LPWStr)]string userName, 
			[MarshalAs(UnmanagedType.LPWStr)]string password, 
			ref OTCExecutionOptions pExecutionOptions);
		
		[DllImport(OTB_DLL,CharSet=CharSet.Unicode)]
		public static extern int OTCLogoff(
			IntPtr sessionHandle, 
			ref OTCExecutionOptions pExecutionOptions);

		// server
		[DllImport(OTB_DLL)]
		public static extern int OTSInitialize(OTSInitData pInitData);

		[DllImport(OTB_DLL)]
		public static extern int OTSTerminate();
		
		[DllImport(OTB_DLL)]
		public static extern int OTSProcessCommandLine([MarshalAs(UnmanagedType.LPWStr)]string commandLine);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSAdvise(OTSCallbackFunctions callbacks);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSStart();
		
		[DllImport(OTB_DLL)]
		public static extern int OTSReady();
		
		[DllImport(OTB_DLL)]
		public static extern int OTSStop();
		
		[DllImport(OTB_DLL)]		
		public static extern int OTSAddAddressSpaceElement(
				IntPtr hParent,
				OTSAddressSpaceElementData elementData, 
				out IntPtr hObject);		

		[DllImport(OTB_DLL)]
		public static extern int OTSChangeUserdata(IntPtr hObject, int iUserData);

		[DllImport(OTB_DLL)]
		public static extern int OTSRemoveAddressSpaceElement(IntPtr hObject);

		[DllImport(OTB_DLL)]
		public static extern int OTSAddProperty(
				IntPtr pObject, 
				IntPtr hPropertyList,
				IntPtr pPropData);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSCompleteRequests(
				int count, 
				[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=0)]OTSRequestData[] aRequests,
				[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=0)]int[] aResult, 
				[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=0)]OTValueData[] aValues);

		[DllImport(OTB_DLL)]
		public static extern int OTSGetCacheValue(
				IntPtr hObject,
				ref OTValueData val);

		[DllImport(OTB_DLL)]
		public static extern int OTSValuesChanged(
				int count, 
				[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=0)]IntPtr[] aObjects, 
				[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=0)]OTValueData[] aValues);

		[DllImport(OTB_DLL)]
		public static extern int OTSGetParent(
				IntPtr hObject,
				IntPtr pParent);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSGetAddressSpaceElementData(
				IntPtr hObject, 
				out IntPtr pData);

		[DllImport(OTB_DLL)]
		public static extern int OTSInitAddressSpace(byte anAddressSpaceType);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSGetChildren(
				IntPtr hObject,
				byte elementType,
				out int pCount,
				out IntPtr ppChildren);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSAddEventCategory(
				uint aCategoryID,
				[MarshalAs(UnmanagedType.LPWStr)]string aDescription, 
				uint eventType, 
				out IntPtr pCatHandle);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSAddEventAttribute(
				IntPtr hCategory, 
				uint anAttributeID, 
				[MarshalAs(UnmanagedType.LPWStr)]string aDescription, 
				ushort aDataType);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSFireEvents(
				uint anEventCount,
				IntPtr pEventData);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSAddCondition(
				IntPtr hCategory,
				[MarshalAs(UnmanagedType.LPWStr)]string aConditionName);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSAddSubCondition(
				IntPtr hCategory, 
				[MarshalAs(UnmanagedType.LPWStr)]string aConditionName,
				[MarshalAs(UnmanagedType.LPWStr)]string aSubConditionName);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSConditionsChanged(
				uint aCount,
				[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=0)]uint[] pObjects,
				IntPtr pConditionData,
				[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=0)][In,Out]uint[] pObjectsOut);
		
		[DllImport(OTB_DLL)]
		public static extern int OTSGetConditionData(			
				IntPtr hObject,			
				[MarshalAs(UnmanagedType.LPStruct)] out OTSConditionData pData);
		
		public static IntPtr AllocateOTBString(string aSource){
			
			if (aSource == null || aSource == string.Empty){
				return IntPtr.Zero;
			}	//	end if
			
			IntPtr result = OTBFunctions.OTAllocateMemory((aSource.Length + 1) * Marshal.SizeOf(typeof(short)));
			if (result == IntPtr.Zero){
				return IntPtr.Zero;
			}	//	end if
			
			for (int index = 0; index < aSource.Length; index++){			
				Marshal.WriteInt16(result, index*Marshal.SizeOf(typeof(short)), (short)aSource[index]);
			}	//	end for

			return result;
			
		}	//	end AllocateOTBString
	}

	#endregion // OTB functions

	#region OTB callbacks
	//common

	internal delegate void OTOnTrace(
		[MarshalAs(UnmanagedType.LPWStr)]string traceString,
		ushort level, 
	    uint mask, 
		[MarshalAs(UnmanagedType.LPWStr)]string objId,
		[MarshalAs(UnmanagedType.LPWStr)]string text);

	internal delegate void OTCOnStateChange(
		OTCObjectContext objectData, 
		byte state);
	
	internal delegate void OTCOnDataChange(
		OTCObjectContext objectContext, 
		uint count ,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=1)]OTObjectData[] itemData,	
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=1)]OTValueData[] values,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=1)]int[] results);
	
	internal delegate void OTCOnReadComplete(
		uint executionContext,
		OTCObjectContext objectContext,
		int result,
		uint count,  
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)]OTObjectData[] itemData, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3, ArraySubType=UnmanagedType.LPWStr)]  String[] itemIDs,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3, ArraySubType=UnmanagedType.LPWStr)] String[] itemPaths,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)]OTValueData[] values, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)] int[] results);
	
	internal delegate void OTCOnWriteComplete(
		uint executionContext, 
		OTCObjectContext objectContext,
		int result,	
		uint count, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)]OTObjectData[] itemData, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3, ArraySubType=UnmanagedType.LPWStr)] String[] itemIDs, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3, ArraySubType=UnmanagedType.LPWStr)] String[] itemPaths, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)]OTValueData[] values, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)] int[]results);
	
	internal delegate void OTCOnPerfromStateTransition(
		uint executionContext, 
		OTCObjectContext objectContext, 
		int result);
	
	internal delegate bool OTCOnServerShutdown(
		OTObjectData sessionData,
		[MarshalAs(UnmanagedType.LPWStr)]string reason);
	
	internal delegate void OTCOnUpdateAttributes(
		uint executionContext, 
		OTCObjectContext objectContext, 
		int result, 
		byte fromServer, 
		uint attributesCount,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=4)] uint[] whatAttributes,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=4)] int[] results);
	
	internal delegate void OTCOnUpdateDAItemAttributes(
		uint executionContext,
		OTCObjectContext objectContext, 
		int result, 
		uint itemCount,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 3)] OTObjectData[] itemData, 
		byte fromServer, 
		uint attributesCount, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=6)] uint[] whatAttributes, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)]int[] results);
	
	internal delegate void OTCOnValidateDAItems(
		uint executionContext, 
		OTCObjectContext objectContext, 
		int result, 
		uint itemCount, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)]OTObjectData[] objectDataArray,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=3)]int[] results);
	
	internal delegate void OTCOnGetServerStatus(
		uint executionContext, 
		OTObjectData sessionData, 
		int result,
		[In,Out] OTCServerStatus serverStatus);
	
	internal delegate void OTCOnBrowseServer(
		uint executionContext, 
		int result, 
		[MarshalAs(UnmanagedType.LPWStr)]string ipAddress,
		byte whatOPCSpec, byte whatServerData,
		uint serverDataCount,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=5)]OTCServerData[] serverData);	
	
	internal delegate void OTCOnBrowseAddressSpace(
		uint executionContext,
		OTObjectData sessionData, 
		int result, 
		IntPtr addressSpaceElementHandle,		
		[MarshalAs(UnmanagedType.LPWStr)]String addressSpaceElementID, 
		[MarshalAs(UnmanagedType.LPWStr)]String addressSpaceElementPath,
		ref OTCAddressSpaceBrowseOptions browseOptions, 
		uint addressSpaceElementDataCount,  
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=7)]OTCAddressSpaceElementData[] addressSpaceElement);
	
	internal delegate void OTCOnGetDAProperties(
		uint executionContext, 
		OTObjectData sessionData, 
		int result,
		IntPtr addressSpaceElementHandle, 
		[MarshalAs(UnmanagedType.LPWStr)]String addressSpaceElementID, 
		[MarshalAs(UnmanagedType.LPWStr)]String addressSpaceElementPath, 
	    OTCGetDAPropertiesOptions getPropertiesOptions, 
		uint propertyDataCount,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=7)] OTCDAPropertyData[] propertyData);	
	
	internal delegate void OTCOnReceivedEvents (
		OTCObjectContext objectContext,
		uint count,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 1)]OTCEventData[] pEvents, 
		byte refresh,
		byte lastRefresh);
	
	internal delegate void OTCOnRefreshAEConditions(
		uint executionContext, 
		OTCObjectContext objectContext,
		int result,
		byte cancelRefresh);
	
	internal delegate void OTCOnAcknowledgeAEConditions(
		uint executionContext, 
		OTObjectData sessionData, 
		int result, 
		[MarshalAs(UnmanagedType.LPWStr)]string  ackID, 
		[MarshalAs(UnmanagedType.LPWStr)]string ackComment, 
		uint count,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 5)] OTCEventData[] pEvents,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 5)] int[] pResults);
	
	internal delegate void OTCOnQueryAvailableAEFilters(
		uint executionContext, 
		OTObjectData sessionData, 
		int result, 
		uint availableFilters);
	
	internal delegate void OTCOnQueryAECategories(
		uint executionContext, 
		OTObjectData sessionData, 
		int result, 
		uint count, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 3)]uint[] pEventTypes, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 3)]uint[] pCategoryIds,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 3, ArraySubType=UnmanagedType.LPWStr)] string[] pCategoryDescriptions);
	
	internal delegate void OTCOnQueryAEAttributes(
		uint executionContext,
		OTObjectData sessionData, 
		int result, 
		uint categoryId, 
		uint count, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 4)]uint[] pAttributeIds,
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 4,ArraySubType=UnmanagedType.LPWStr)]string[] pAttributeDescriptions, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 4)]ushort[] pAttributeDatatypes);
	
	internal delegate void OTCOnQueryAEConditions(
		uint executionContext,
		OTObjectData sessionData,
		int result, 
		uint categoryId, 
		uint count, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 4, ArraySubType=UnmanagedType.LPWStr)] string[] pConditionsNames);
	
	internal delegate void OTCOnQueryAESubConditions(
		uint executionContext, 
		OTObjectData sessionData, 
		int result, 
		[MarshalAs(UnmanagedType.LPWStr)]string conditionName, 
		uint count, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 4, ArraySubType=UnmanagedType.LPWStr)] string[] pSubConditionsNames);
	
	internal delegate void OTCOnQueryAESourceConditions(
		uint executionContext, 
		OTObjectData sessionData, 
		int result, 
		[MarshalAs(UnmanagedType.LPWStr)]string sourcePath, 
		uint count, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 4, ArraySubType=UnmanagedType.LPWStr)] string[] pConditionsNames);
	
	internal delegate void OTCOnGetAEConditionState(
		uint executionContext, 
		OTObjectData sessionData, 
		int result, 
		[MarshalAs(UnmanagedType.LPWStr)]string sourcePath, 
		[MarshalAs(UnmanagedType.LPWStr)]string conditionName, 
		uint attributeCount, 
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 5)]uint[] pAttributeIds, 
		OTCAEConditionStateData pConditionState);
	
	internal delegate void OTCOnEnableAEConditions(
		uint executionContext, 
		OTObjectData sessionData, 
		int result, 
		byte enable, 
		byte areaOrSource, 
		[MarshalAs(UnmanagedType.LPWStr)]string path);
	
	internal delegate void OTCOnGetErrorString(
		uint executionContext, 
		OTObjectData sessionData, 
		int result, 
		int errorCode,
		[MarshalAs(UnmanagedType.LPWStr)] string errorString);
	
	internal delegate void OTCOnLogon(
		uint executionContext,
		OTObjectData sessionData, 
		int result, 
		[MarshalAs(UnmanagedType.LPWStr)]string userName, 
		[MarshalAs(UnmanagedType.LPWStr)]string password);
	
	internal delegate void OTCOnLogoff(
		uint executionContext, 
		OTObjectData sessionData,
		int result);


	//server
	internal delegate int OTSShutdown();
	
	internal delegate int OTSHandleReadRequests(
		int count, 
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]OTSRequestData[] aRequests);
	
	internal delegate int OTSHandleWriteRequests(
		int count, 
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]OTSRequestData[] aRequests, 
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]OTValueData[] aValues);
	
	internal delegate int OTSQueryProperties(
		IntPtr pObjectData, 
		[MarshalAs(UnmanagedType.LPWStr)]string anItemId, 
		int aPropertyId, 
		out uint aPropertiesCount, 
		out IntPtr pProperties);

	internal delegate int OTSQueryAddressSpaceElementData(
		[MarshalAs(UnmanagedType.LPWStr)]string path, 
		byte elementType, 
		IntPtr pData);

	internal delegate int OTSQueryAddressSpaceElementChildren(
		[MarshalAs(UnmanagedType.LPWStr)]string path, 
		byte elementType, 
		out uint pCount, 
		out IntPtr pElementData);

	internal delegate int OTSQueryAddressSpaceElementProperties(
		int aPropertyID, 
		IntPtr aPropertyList);
	
	internal delegate int OTSChangeSessionState(
		IntPtr aSessionHandle, 
		OTSSessionData pSessionStateData);
	
	internal delegate int OTSQueryCacheValue(
		IntPtr aSessionHandle,
		OTObjectData anObjectData,
		IntPtr pOutValue);
	
	internal delegate int OTSChangeItems(
		uint itemCnt,
		IntPtr pItemData);

	internal delegate int OTSCreateAddressSpaceElement(
		IntPtr hParent, 
		IntPtr hObject, 
		OTSAddressSpaceElementData pDataIn, 
		IntPtr pDataOut);

	internal delegate int OTSDestroyAddressSpaceElement(OTObjectData objectData);
	
	internal delegate int OTSQueryConditions(
		IntPtr pObjectData,
		[MarshalAs(UnmanagedType.LPWStr)]string aSourcePath,
		out uint aConditionCount,
		out IntPtr pConditionNames);
	
	internal delegate int OTSAcknowledgeCondition(
		OTObjectData pConditionData,
		[MarshalAs(UnmanagedType.LPWStr)]string anAckId,
		[MarshalAs(UnmanagedType.LPWStr)]string anAckComment);
	
	internal delegate int OTSQueryConditionDefinition(
		OTObjectData pConditionData,
		IntPtr aConditionDefinitionData);

	internal delegate int OTSEnableConditions(
		byte isEnable,
		[MarshalAs(UnmanagedType.LPWStr)]string anAddressSpaceElementPath);

	internal delegate int OTSWebHandleTemplate(
		[MarshalAs(UnmanagedType.LPWStr)]string aTemplateName,
		uint numArgs,		
		[MarshalAs(UnmanagedType.LPArray,SizeParamIndex = 1, ArraySubType=UnmanagedType.LPWStr)] string[] pArgs,
		out IntPtr pResult);

	#endregion // OTB callbacks
	
}	//	end namespace Softing.OPCToolbox
