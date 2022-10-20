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
//  Filename    : ClientApplication.cs                                        |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : OPC Client entry point                                      |
//                                                                            |
//-----------------------------------------------------------------------------

using System;
using Softing.OPCToolbox;
using System.Runtime.InteropServices;
using System.Collections;
using System.Windows.Forms;
using System.Globalization;
using Softing.OPCToolbox.OTB;

namespace Softing.OPCToolbox.Client{
		
	#region //Public Delegates
	//------------------------	
	/// <summary>
	/// <para lang="Unmanaged C++">This is not available for Unmanaged C++.</para>
	/// <para lang="Visual Basic, C#, C++">Represents the method that will handle the <b>Shutdown</b> event of a <see cref="DaSession"/> or
	/// of an <see cref="AeSession"/>object.</para>
	/// </summary>
	///<param name="obj">The <see cref="ObjectSpaceElement"/> object to whom the <b>Shutdown</b> event belongs.</param>
	///<param name="reason">The reason of the shutdown.</param>
	///<returns>A value that indicates if the reconnection to the server will be attempted. For 
	///<b>true</b> the client will automatically connect to the server.</returns>
	/// <include 
	///  file='TBNC.doc.xml' 
	///  path='//delegate[@name="ShutdownEventHandler"]/doc/*'
	/// />
	public delegate bool ShutdownEventHandler(ObjectSpaceElement obj, string reason);
	
	//-
	#endregion
	
	/// <summary>
	/// Gives the client the possibility to choose the modality of executing operations. 
	/// Operations can be executed in two ways: synchronously and asynchronously.
	/// </summary>
	/// <include 
	///  file='TBNC.doc.xml' 
	///  path='//class[@name="ExecutionOptions"]/doc/*'
	/// />	
	public class ExecutionOptions {

		#region Protected Attributes
		//------------------------
			///<summary>
			///One of the <see cref="Softing.OPCToolbox.Client.EnumExecutionType"/> values that represent the mode of executing operations.
			///</summary>	
			/// <include 
			///  file='TBNC.doc.xml' 
			///  path='//class[@name="ExecutionOptions"]/field[@name="m_executionType"]/doc/*'
			/// />
			protected EnumExecutionType m_executionType;
			///<summary>
			///An identifier for an asynchronous call.
			///</summary>	
			/// <include 
			///  file='TBNC.doc.xml' 
			///  path='//class[@name="ExecutionOptions"]/field[@name="m_executionContext"]/doc/*'
			/// />
			protected uint m_executionContext;
		//-
		#endregion
	
		#region Constructors
		//--------------------

		/// <summary>
		/// Initializes a new instance of the ExecutionOptions class.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="ExecutionOptions"]/constructor[@name="ExecutionOptions"]/doc/*'
		/// />
		public ExecutionOptions(){

			m_executionContext = 0;
			m_executionType = EnumExecutionType.SYNCHRONOUS;
		}
		/// <summary>
		/// Initializes a new instance of the ExecutionOptions class to the value indicated by a 
		/// call type and an identifier for an asynchronous call.
		/// </summary>
		/// <param name="type">A call type.</param>
		/// <param name="executionContext">An identifier for an asynchronous call.</param>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="ExecutionOptions"]/constructor[@name="ExecutionOptions"]/doc/*'
		/// />
		public ExecutionOptions(EnumExecutionType type,uint executionContext){
			ExecutionType = type;
			ExecutionContext = executionContext;
		}	
		
		//-
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets or sets the operations executing type. 
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="ExecutionOptions"]/property[@name="ExecutionType"]/doc/*'
		/// />		
		public EnumExecutionType ExecutionType{
			get{
				return m_executionType;
			}
			set{
				m_executionType = value;
			}
		}
		
		/// <summary>
		/// Gets or sets an identifier for an asynchronous call.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="ExecutionOptions"]/property[@name="ExecutionContext"]/doc/*'
		/// />
		public uint ExecutionContext{
			get{
				return m_executionContext;
			}
			set{
				m_executionContext = value;
			}
		}

		#endregion

	}

	/// <summary>
	/// The client assembly's entry point.
	/// </summary>	
	/// <include 
	///  file='TBNC.doc.xml' 
	///  path='//class[@name="Application"]/doc/*'
	/// />
	public sealed class Application 
	{

		#region //Private Attributes
		//-----------------------
		
		private readonly Hashtable m_daSessionList = new Hashtable();	
		private readonly Hashtable m_aeSessionList = new Hashtable();
		private string m_badQualityValueString = "???";	
		private EnumUpdateAttributesBehavior m_updateAttributesBehavior = EnumUpdateAttributesBehavior.IMPLICIT_UPDATE;			
		internal readonly Hashtable m_serverBrowserList = new Hashtable();
		private static bool m_isInit = false;	
		private static bool m_isAdvised = false;
		private static  OTCCallbackFunctions m_OTNCCallBacks = new OTCCallbackFunctions();		
		private readonly OTCInitData m_initData = new OTCInitData();

		//-
		#endregion		
				
		#region //Constructors
		//--------------------
		/// <summary>
		/// Creates a new instance of the <see cref="Application"/> class.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="Instance"]/doc/*'
		/// />
		public static Application Instance = new Application();
		
		private Application()
		{						
			m_initData.m_versionOtb	= 422;
		}//	end ctr
		
		///<summary>
		///<para lang="Unmanaged C++">Releases all the resources allocated by the application.</para>
		///<para lang="Visual Basic, C#, C++"> The method is not available.</para>
		///</summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="ReleaseApplication"]/doc/*'
		/// />
		public void ReleaseApplication(){}
		//-
		#endregion

		#region //Public Properties
		//-------------------------

		/// <summary>
		/// Used OTS version 
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="VersionOts"]/doc/*'
		/// />		
		public short VersionOtb 
		{
			get 
			{
				return m_initData.m_versionOtb;
			}
			set 
			{
				m_initData.m_versionOtb = value;
			}
		}	//	end VersionOtb


		/// <summary>
		/// Service name which is and has to be set when the application runs as service 
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="ServiceName"]/doc/*'
		/// />		
		public string ServiceName 
		{
			get 
			{
				return m_initData.m_serviceName;
			}
			set 
			{
				m_initData.m_serviceName = value;
			}
		}	//	end ServiceName


		/// <summary>
		/// Gets an array with the <see cref="Softing.OPCToolbox.Client.DaSession"/> objects added to the application.
		/// </summary>				
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/property[@name="DaSessionList"]/doc/*'
		/// />	
		public  DaSession[] DaSessionList
		{
			get
			{
				
				Hashtable sessionTable = Hashtable.Synchronized(m_daSessionList);
				DaSession[] sessionVector = new DaSession[sessionTable.Count];
				int i = 0;

				foreach(DaSession daSession in sessionTable.Values)
				{

					sessionVector[i] = daSession;
					i++;

				} //end foreach

				return sessionVector;
			}
		}
		/// <summary>
		/// Gets an array with the <see cref="Softing.OPCToolbox.Client.AeSession"/> objects added to the application.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/property[@name="AeSessionList"]/doc/*'
		/// />	
		public  AeSession[] AeSessionList
		{
			get
			{

				Hashtable sessionTable = Hashtable.Synchronized(m_aeSessionList);
				AeSession[] sessionVector = new AeSession[sessionTable.Count];
				int i = 0;

				foreach(AeSession aeSession in sessionTable.Values)
				{

					sessionVector[i] = aeSession;
					i++;

				} //end foreach

				return sessionVector;
				
			}
		}
		/// <summary>
		/// <para lang="C#, Visual Basic, C++">Gets or sets the message to be displayed in case the quality of a value read from a server is not good.</para>
		///	<para lang="Unmanaged C++">This method is not available.</para>
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/property[@name="BadQualityValueString"]/doc/*'
		/// />	
		public string BadQualityValueString
		{
			get
			{
				return m_badQualityValueString;
			}
			set
			{
				m_badQualityValueString = value;
			}
		}
		/// <summary>
		///  Gets or sets the modality of updating the attributes of an object.
		/// </summary>		
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/property[@name="UpdateAttributesBehavior"]/doc/*'
		/// />	
		public EnumUpdateAttributesBehavior UpdateAttributesBehavior
		{
			get 
			{
				return m_updateAttributesBehavior;
			}
			set
			{
				m_updateAttributesBehavior = value;
			}
		}

		//-
		#endregion

		#region //Public Events
		//---------------------
		/// <summary>
		/// Occurs for each trace output line made by the Toolbox.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/event[@name="TraceOutput"]/doc/*'
		/// />	
		public event TraceEventHandler TraceOutput;
		//-
		#endregion

		#region //Internal Methods
		//------------------------

		/// <summary>
		/// Permits receiving callbacks from the server.
		/// </summary>
		/// <remarks>
		/// The method should be called only once!
		/// <note>The returned value should be checked with ResultCode.SUCCEEDED or with ResultCode.FAILED. </note>
		/// </remarks>
		/// <returns>The result of allowing receiving callbacks.</returns>	
		internal int Advise()
		{
			
			int res = (int)EnumResultCode.E_FAIL;
			try
			{
				m_OTNCCallBacks.m_trace					 += new OTOnTrace(HandleTrace);
				m_OTNCCallBacks.m_stateChange				 += new OTCOnStateChange(HandleStateChange);
				m_OTNCCallBacks.m_dataChange				 += new OTCOnDataChange(HandleDataChange);
				m_OTNCCallBacks.m_readComplete			 += new OTCOnReadComplete(HandleReadComplete);
				m_OTNCCallBacks.m_writeComplete			 += new OTCOnWriteComplete(HandleWriteComplete);
				m_OTNCCallBacks.m_perfromStateTransition	 += new OTCOnPerfromStateTransition(HandlePerformStateTransition);
				m_OTNCCallBacks.m_serverShutdown			 += new OTCOnServerShutdown(HandleServerShutdown);
				m_OTNCCallBacks.m_updateAttributes		 += new OTCOnUpdateAttributes(HandleUpdateAttributes);
				m_OTNCCallBacks.m_updateDaItemAttributes	 += new OTCOnUpdateDAItemAttributes(HandleUpdateDaItemAttributes);
				m_OTNCCallBacks.m_validateDaItems			 += new OTCOnValidateDAItems(HandleValidateDaItems);
				m_OTNCCallBacks.m_getServerStatus			 += new OTCOnGetServerStatus(HandleGetServerStatus);
				m_OTNCCallBacks.m_browseAddressSpace		 += new OTCOnBrowseAddressSpace(HandleBrowseAddressSpace);
				m_OTNCCallBacks.m_onGetDaProperties		 += new OTCOnGetDAProperties(HandleGetDaProperties);
				m_OTNCCallBacks.m_browseServer			 += new OTCOnBrowseServer(HandleBrowseServer);	
				m_OTNCCallBacks.m_receivedEvents			 += new OTCOnReceivedEvents(HandleEventsReceived);
				m_OTNCCallBacks.m_refreshConditions		 += new OTCOnRefreshAEConditions(HandleRefreshConditions);
				m_OTNCCallBacks.m_acknowledgeConditions	 += new OTCOnAcknowledgeAEConditions(HandleAcknowledgeConditions);	
				m_OTNCCallBacks.m_queryAvailableFilters	 += new OTCOnQueryAvailableAEFilters(HandleQueryAvailableAeFilters);
				m_OTNCCallBacks.m_queryCategories			 += new OTCOnQueryAECategories(HandleQueryAeCategories);
				m_OTNCCallBacks.m_queryAttributes			 += new OTCOnQueryAEAttributes(HandleQueryAeAttributes);
				m_OTNCCallBacks.m_queryConditons			 += new OTCOnQueryAEConditions(HandleQueryAeConditions);				
				m_OTNCCallBacks.m_querySubConditions		 += new OTCOnQueryAESubConditions(HandleQueryAeSubConditions);
				m_OTNCCallBacks.m_querySourceConditions	 += new OTCOnQueryAESourceConditions(HandleQuerySourceConditions);
				m_OTNCCallBacks.m_getConditionState		 += new OTCOnGetAEConditionState(HandleGetConditionState);
				m_OTNCCallBacks.m_enableConditions		 += new OTCOnEnableAEConditions(HandleEnableAeConditions);
				m_OTNCCallBacks.m_getErrorString			 += new OTCOnGetErrorString(HandleGetErrorString);
				m_OTNCCallBacks.m_logon					 += new OTCOnLogon(HandleLogon);
				m_OTNCCallBacks.m_logoff					 += new OTCOnLogoff(HandleLogoff);
				  
				if (!m_isAdvised)
				{
					res =	OTBFunctions.OTCAdvise(m_OTNCCallBacks);	
					if(ResultCode.SUCCEEDED(res))
					{
						m_isAdvised = true;
					}
					else
					{
						Application.Instance.Trace(
							EnumTraceLevel.ERR,
							EnumTraceGroup.CLIENT,
							"Application.Advise",
							" Application advise failed! Result: " + res);
					}
				}
				 
			}//try
			catch(Exception exc)
			{
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application.Advise",
					exc.ToString());
			}
			return res;
		}


		/// <summary>
		/// Adds a client - Data Access Server session (<see cref="Softing.OPCToolbox.Client.DaSession"/> object) to the application.
		/// </summary>
		/// <param name="url">The url of the OPC Data Access Server for which a session with a client is established.</param>
		/// <param name="aDaSession">The <b>DaSession</b> object to be added.</param>
		/// <returns>The result of adding a DaSession object.</returns>				
		internal int AddDaSession(string url, DaSession aDaSession)
		{		
			int res = (int)EnumResultCode.E_FAIL;
			try
			{
				if(aDaSession != null)
				{
					IntPtr sessionHandle = IntPtr.Zero;
					res = OTBFunctions.OTCAddDASession(
						url,
						(uint)aDaSession.GetHashCode(), 
						ref sessionHandle);	

					if(ResultCode.SUCCEEDED(res))
					{
						aDaSession.Handle = sessionHandle;						
						Hashtable synchronizedSessionList = Hashtable.Synchronized(this.m_daSessionList);
						synchronizedSessionList.Add(aDaSession.GetHashCode(),aDaSession);							
					}
					else
					{
						Application.Instance.Trace(
							EnumTraceLevel.ERR,
							EnumTraceGroup.CLIENT,
							"Application.AddDaSession", 
							" Add session failed! Result: " + res);
					}
				}	
				else
				{
					res = (int)EnumResultCode.E_INVALIDARG;
					Application.Instance.Trace(
						EnumTraceLevel.ERR,
						EnumTraceGroup.CLIENT,
						"Application.AddDaSession", 
						" The session argument is invalid! It cannot be null!");
				}	//	end if...else
			}
			catch(Exception exc)
			{
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application.AddDaSession",
					exc.ToString());
			}
			return res;
		}

		/// <summary>
		/// Adds a client - Alarms and Events Server session (<see cref="Softing.OPCToolbox.Client.AeSession"/> object) to the application.
		/// </summary>
		/// <param name="url">The url of the OPC Alarms and Events Server for which a session with a client is established.</param>
		/// <param name="anAeSession">The <b>AeSession</b> object to be added.</param>
		/// <returns>The result of adding an AeSession object.</returns>		
		internal int AddAeSession(string url, AeSession anAeSession)
		{
			int res = (int)EnumResultCode.E_FAIL;
			try
			{
				if (anAeSession != null)
				{
					IntPtr sessionHandle = IntPtr.Zero;
					res = OTBFunctions.OTCAddAESession(
						url,
						(uint)anAeSession.GetHashCode(),
						ref sessionHandle);	

					if(ResultCode.SUCCEEDED(res))
					{
						anAeSession.Handle = sessionHandle;
						Hashtable synchronizedSessionList = Hashtable.Synchronized(this.m_aeSessionList);
						synchronizedSessionList.Add(anAeSession.GetHashCode(),anAeSession);	
					}	
					else
					{
						Application.Instance.Trace(
							EnumTraceLevel.ERR,
							EnumTraceGroup.CLIENT,
							"Application.AddAeSession", 
							" Add session failed! Result: " + res);
					}
				}
				else
				{
					res = (int)EnumResultCode.E_INVALIDARG;
					Application.Instance.Trace(
						EnumTraceLevel.ERR,
						EnumTraceGroup.CLIENT,
						"Application.AddAeSession", 
						" The session argument is invalid! Session cannot be null");
				}
			}
			catch(Exception exc)
			{
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application.AddAeSession", 
					exc.ToString());				
			}
			return res;
		}
						

		#region //Internal Handle Callbacks
		//------------------------
		
		internal void HandleTrace(
			string traceString,
			ushort level, 
			uint mask, 
			string objId,
			string text)
		{
			try
			{
				if(TraceOutput != null)
				{
					TraceOutput(traceString,(EnumTraceLevel)level,(EnumTraceGroup)mask,objId,text);
				}
			}
			catch(Exception exc)
			{
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleTrace", 
					exc.ToString());
			}
		}


		internal void HandleStateChange(
			OTCObjectContext objectContext, 
			byte state)
		{
			try
			{
				EnumObjectState currentState = EnumObjectState.DISCONNECTED;
				switch (state)
				{
					case 0: currentState = EnumObjectState.DISCONNECTED;
						break;
					case 1: currentState = EnumObjectState.CONNECTED;
						break;
					case 3: currentState = EnumObjectState.ACTIVATED;
						break;
					default: currentState = EnumObjectState.DISCONNECTED;
						break;
				}		
				Hashtable synchronizedDaSessionList = Hashtable.Synchronized(m_daSessionList);
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);

				if(objectContext.m_objectType == (uint)EnumObjectType.DASESSION )
				{
					DaSession currentSession = (DaSession)synchronizedDaSessionList[(int)objectContext.m_objectData.m_userData];
					if(currentSession != null) {currentSession.OnStateChange(currentState);}
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.AESESSION )
				{
					AeSession currentSession = (AeSession)synchronizedAeSessionList[(int)objectContext.m_objectData.m_userData];
					if(currentSession != null) {currentSession.OnStateChange(currentState);}
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.DASUBSCRIPTION)
				{
					DaSession currentSession = (DaSession)synchronizedDaSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null)
					{
						
						DaSubscription currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
						if(currentSubscription != null) {currentSubscription.OnStateChange(currentState);}
					}
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.AESUBSCRIPTION)
				{
					AeSession currentSession = (AeSession)synchronizedAeSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null)
					{
						
						AeSubscription currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
						if(currentSubscription != null) currentSubscription.OnStateChange(currentState);
					} //end if
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.DAITEM )
				{
					DaSession currentSession = (DaSession)synchronizedDaSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null)
					{
						
						DaSubscription currentSubscription = currentSession.Find((int)objectContext.m_subscriptionData.m_userData);
						if(currentSubscription != null)
						{
							
							DaItem currentItem = currentSubscription.Find((int)objectContext.m_objectData.m_userData);
							if (currentItem != null) {currentItem.OnStateChange(currentState);}
						}
					}
				}	
			}
			catch(Exception exc)
			{
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleStateChange", 
					exc.ToString());
			}
		}//end handleStateChanged						

		internal void HandleDataChange(
			OTCObjectContext objectContext, 
			uint count, 
			OTObjectData[] itemData,
			OTValueData[] values,
			int[] results)
		{	
			try{

				DaSession currentSession = null;
				DaSubscription currentSubscription = null;
				DaItem[] items = new DaItem[count];	
				ValueQT[]newValues = new ValueQT[count];

				Hashtable synchronizedSessionList = Hashtable.Synchronized(m_daSessionList);				

				if(objectContext.m_objectType == (uint)EnumObjectType.DASUBSCRIPTION ){
					currentSession = (DaSession)synchronizedSessionList[(int)objectContext.m_sessionData.m_userData];
					if( currentSession != null){
						
						currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
						if(currentSubscription != null){
						
							for(int i = 0;i < count; i++){
								items[i] = currentSubscription.Find((int)itemData[i].m_userData);								
								newValues[i] = new ValueQT(ref values[i]);
							} //end for																					
							
							currentSubscription.OnDataChange(
								items,
								newValues,
								results);		
						}//end if
					} //end if
				} //end if															
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleDataChange", 
					exc.ToString());
				
			}
		}// end HandleDataChange
		
		
		internal void HandleUpdateAttributes(
			uint executionContext, 
			OTCObjectContext objectContext,
			int result,
			byte fromServer, 
			uint attributesCount, 
			uint[] whatAttributes, 
			int[] results)
		{	
			try{
				
				EnumObjectAttribute[] attributes = new EnumObjectAttribute[attributesCount];
	
				for(int ii = 0; ii < attributesCount; ii++){
					attributes[ii] = (EnumObjectAttribute)(whatAttributes[ii]);		
				}// for
		
				if(objectContext.m_objectType == (uint)EnumObjectType.DASESSION ){
					DaSession currentSession = (DaSession)m_daSessionList[objectContext.m_objectData.m_userData];
					if(currentSession != null){
						if(fromServer == 1){
							currentSession.OnGetAttributesFromServer(
								executionContext,
								attributes,
								results,
								result);
						}
						if(fromServer == 0){
							currentSession.OnSetAttributesToServer(
								executionContext,
								attributes,
								results,
								result);
						}
					} //end if
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.DASUBSCRIPTION)
				{
					Hashtable synchronizedSessionList = Hashtable.Synchronized(m_daSessionList);
					DaSession currentSession = (DaSession)synchronizedSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null){
						
						DaSubscription currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
						if(currentSubscription != null){
							if(fromServer == 1){
								currentSubscription.OnGetAttributesFromServer(
									executionContext,
									attributes,
									results,
									result);
							}
							if(fromServer == 0){
								currentSubscription.OnSetAttributesToServer(
									executionContext,
									attributes,
									results,
									result);
							}
						} //end if
					} //end if
				}				
			}catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleUpdateAttributes", 
					exc.ToString());
			}
		}
		
		
		internal void HandleValidateDaItems(
			uint executionContext,
			OTCObjectContext objectContext, 
			int result, 
			uint itemCount, 
			OTObjectData[] objectDataArray,
			int[] results)
		{
			try{								
				Hashtable synchonizedSessionList = Hashtable.Synchronized(m_daSessionList);
				DaSession currentSession = (DaSession)synchonizedSessionList[(int)objectContext.m_sessionData.m_userData];
				if (currentSession != null){
					
					DaSubscription currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
					if(currentSubscription != null){
						DaItem[] items = new DaItem[itemCount];
					
						for(int i = 0;i < itemCount; i++){
							items[i] = currentSubscription.Find((int)objectDataArray[i].m_userData);								
						}
						currentSubscription.OnValidateDaItems(
							executionContext,
							items,
							results,
							result);
					}
				}
				
			}catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleValidateDaItems",
					exc.ToString());
			}
		}// end HandleValidateDaItems	
	

		internal void HandleReadComplete(
			uint executionContext, 
			OTCObjectContext objectContext,
			int result,
			uint count,  
			OTObjectData[] itemData, 
			String[] itemIDs,
			String[] itemPaths,
			OTValueData[] values,
			int[] results)
		{	
			try{				
				DaSession currentSession;
				DaSubscription currentSubscription;
				DaItem[] items = new DaItem[count];
				ValueQT[] newValues = new ValueQT[count]; 

				//creates the values array
				for (int i = 0; i< count; i++){
					newValues[i] = new ValueQT(ref values[i]);
				}
				Hashtable synchronizedSessionList = Hashtable.Synchronized(m_daSessionList);
				if(objectContext.m_objectType == (uint)EnumObjectType.DASESSION ){
					currentSession = (DaSession)synchronizedSessionList[(int)objectContext.m_objectData.m_userData];
					if(currentSession != null){
						currentSession.OnReadComplete(
							executionContext,
							itemIDs,
							itemPaths,
							newValues,
							results,
							result);
					}
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.DASUBSCRIPTION){
					currentSession = (DaSession)synchronizedSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null){
						
						currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
						if(currentSubscription != null){
						
							for(int i = 0; i < count; i++){
								items[i] = currentSubscription.Find((int)itemData[i].m_userData);									
							} //end for
						} //end if
						currentSubscription.OnReadComplete(
							executionContext,
							items,
							newValues,
							results,
							result);
					} //end if
				}	
			}catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleReadComplete", 
					exc.ToString());
			}
		}// end HandleReadComplete
		

		internal void HandleWriteComplete(
			uint executionContext, 
			OTCObjectContext objectContext, 
			int result,
			uint count, 
			OTObjectData[] itemData, 
			String[] itemIDs,
			String[] itemPaths,
			OTValueData[] values,
			int[]results)
		{	
			try{				
				DaSession currentSession;
				DaSubscription currentSubscription;
				DaItem[] items = new DaItem[count];
				ValueQT[] newValues = new ValueQT[count]; 	

				// creates the values array
				if(values != null){
					for (int i = 0; i< count; i++){
						newValues[i] = new ValueQT(ref values[i]);	
					}	//	end for
				}else{
					newValues = null;
				}	//	end if...else

				Hashtable synchronizedSessionList = Hashtable.Synchronized(m_daSessionList);
				if(objectContext.m_objectType == (uint)EnumObjectType.DASESSION ){
					currentSession = (DaSession)synchronizedSessionList[(int)objectContext.m_objectData.m_userData];
					if(currentSession != null){
						currentSession.OnWriteComplete(
							executionContext,
							itemIDs,
							itemPaths,
							newValues,
							results,
							result);
					} //end if
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.DASUBSCRIPTION){
					currentSession = (DaSession)synchronizedSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null)
					{							
						currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
						if(currentSubscription != null){
							
							for(int i = 0;i < count; i++){
								items[i] = currentSubscription.Find((int)itemData[i].m_userData);									
							}
							currentSubscription.OnWriteComplete(
								executionContext,
								items,
								newValues,
								results,
								result);
						}
					} //end if
				}
			} //end if		
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleWriteComplete", 
					exc.ToString());
			}
		}//end HandleWriteComplete
	
	
		internal bool HandleServerShutdown(
			OTObjectData sessionData,
			String reason)
		{		
			bool res = true;//after server shutsdown the reconnection will be performed
			try{
				Hashtable synchronizedDaSessionList = Hashtable.Synchronized(m_daSessionList);
				DaSession daSession = (DaSession)synchronizedDaSessionList[(int)sessionData.m_userData];
				if(daSession != null){
					 res = daSession.OnShutdown(reason);
				}
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession aeSession = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
				if(aeSession != null){
					res = aeSession.OnShutdown(reason);
				}

			}catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleServerShutdown", 
					exc.ToString());				
			}
			return res;	
		}
		
	
		internal void HandleGetServerStatus(
			uint executionContext,
			OTObjectData sessionData,
			int result,
			OTCServerStatus currentStatus)
		{	
			try{
				uint state = currentStatus.m_state;
				DateTime startTime = currentStatus.m_startTime.ToDateTime();
				DateTime lastUpdateTime = currentStatus.m_lastUpdateTime.ToDateTime();
				DateTime currentTime = currentStatus.m_currentTime.ToDateTime();

				uint[] supportedLCIDs = new uint[currentStatus.m_countLCIDs];	
				string[] LCIDs = new string[currentStatus.m_countLCIDs];
				CultureInfo culture;

				for (int i = 0; i< currentStatus.m_countLCIDs; i++){
					uint myData = (uint)Marshal.PtrToStructure ((IntPtr)((uint)currentStatus.m_supportedLCIDs+(Marshal.SizeOf(typeof(uint))*i)),typeof(uint));
					supportedLCIDs[i] = myData;
					try{													
							switch(supportedLCIDs[i]){
								case 2048:		

									LCIDs[i] = "System Default";

									break;
								case 1024:									

									LCIDs[i] = "User Default";
									break;
								case 0:

									LCIDs[i] = "Neutral";
									break;
								default:
									culture = new CultureInfo((int)supportedLCIDs[i]);
									LCIDs[i] = culture.Name;	
									break;
							}						
					}
					catch(ArgumentException exc){
						LCIDs[i] = supportedLCIDs[i].ToString();
						Application.Instance.Trace(
							EnumTraceLevel.INF,
							EnumTraceGroup.CLIENT, 
							"Application.HandleGetServerStatus", 
							exc.Message);
					}
				}
				ServerStatus serverStatus = new ServerStatus(
					state,
					startTime,
					currentTime,
					Marshal.PtrToStringUni(currentStatus.m_vendorInfo),
					Marshal.PtrToStringUni(currentStatus.m_productVersion),
					LCIDs,
					lastUpdateTime,
					currentStatus.m_groupCount,
					currentStatus.m_bandwidth,
					Marshal.PtrToStringUni(currentStatus.m_statusInfo));				
			
				IntPtr handle = sessionData.m_objectHandle;
				Hashtable synchronizedDaSessionList = Hashtable.Synchronized(m_daSessionList);
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				DaSession currentDaSession = (DaSession)synchronizedDaSessionList[(int)sessionData.m_userData];
				if(currentDaSession != null){	
					currentDaSession.OnGetServerStatus(executionContext,serverStatus,result);
				}
				AeSession currentAeSession = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
				if(currentAeSession != null){
					currentAeSession.OnGetServerStatus(executionContext,serverStatus,result);
				}	
			
			}catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application.HandleGetServerStatus", 
					exc.ToString());
			}
		}// end HandleGetServerStatus
		
	
		internal void HandlePerformStateTransition(
			uint executionContext,
			OTCObjectContext objectContext, 
			int result)
		{
		try{
				Hashtable synchronizedDaSessionList = Hashtable.Synchronized(m_daSessionList);
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);

				if(objectContext.m_objectType == (uint)EnumObjectType.DASESSION ){
					DaSession currentSession = (DaSession)synchronizedDaSessionList[(int)objectContext.m_objectData.m_userData];
					if(currentSession != null){
						currentSession.OnPerformStateTransition(executionContext,result);
					} //end if
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.AESESSION ){
					AeSession currentSession = (AeSession)synchronizedAeSessionList[(int)objectContext.m_objectData.m_userData];
					if(currentSession != null){
						currentSession.OnPerformStateTransition(executionContext,result);
					} //end if
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.DASUBSCRIPTION){
					DaSession currentSession = (DaSession)synchronizedDaSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null){
						
						DaSubscription currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
						if(currentSubscription != null){
							currentSubscription.OnPerformStateTransition(executionContext, result);
						} //end if
					}
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.AESUBSCRIPTION){
					AeSession currentSession = (AeSession)synchronizedAeSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null){
					
						AeSubscription currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
						if(currentSubscription != null){
							currentSubscription.OnPerformStateTransition(executionContext,result);
						} //end if
					}
				}
				if(objectContext.m_objectType == (uint)EnumObjectType.DAITEM ){
					DaSession currentSession = (DaSession)synchronizedDaSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null){
					
						DaSubscription currentSubscription = currentSession.Find((int)objectContext.m_subscriptionData.m_userData);
						if(currentSubscription != null){
							
							DaItem currentItem = currentSubscription.Find((int)objectContext.m_objectData.m_userData);
							if(currentItem != null){
								currentItem.OnPerformStateTransition(executionContext,result);
							}
						} //end if
					} //end if
				}
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandlePerformStateTransition", 
					exc.ToString());
			}
		}// end HandlePerformStateTransition
	
	
		internal void HandleUpdateDaItemAttributes(
			uint executionContext,
			OTCObjectContext objectContext,  
			int result, 
			uint itemCount,
			OTObjectData[] itemData,
			byte fromServer,
			uint attributesCount, 
			uint[] whatAttributes, 
			int[] results)
		{	
			try{	
				DaItem[] items = new DaItem[itemCount];
				DaSession currentSession = null;
				DaSubscription currentSubscription = null;
				Hashtable synchronizedSessionList = Hashtable.Synchronized(m_daSessionList);
				if(objectContext.m_objectType == (uint)EnumObjectType.DASUBSCRIPTION){
					currentSession = (DaSession)synchronizedSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null){
						
						currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
						if(currentSubscription != null){
							
							for(int i = 0;i<itemData.Length; i++){
								items[i] = currentSubscription.Find((int)itemData[i].m_userData);									
							}
						} //end if
					} //end if
				}				
				EnumObjectAttribute[] whatOtherAttributes = new EnumObjectAttribute[attributesCount];

				for(int j = 0; j< attributesCount; j++){
					whatOtherAttributes[j] = (EnumObjectAttribute)whatAttributes[j];
				}
				if(currentSubscription != null){
					if(fromServer == 1){
						currentSubscription.OnGetDaItemAttributesFromServer(
							executionContext,
							items,
							whatOtherAttributes,
							results,
							result);
					}
					if(fromServer == 0){
						currentSubscription.OnSetDaItemAttributesToServer(
							executionContext,
							items,
							whatOtherAttributes,
							results,
							result);
					}
				} //end if
			} 	
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleUpdateDaItemAttributes", 
					exc.ToString());
			}
		}//end HandleUpdateDaItemAttributes
				
		internal void HandleBrowseAddressSpace(
			uint executionContext,
			OTObjectData sessionData, 
			int result,
			IntPtr addressSpaceElementHandle,
			[MarshalAs(UnmanagedType.LPWStr)]String addressSpaceElementID, 
			[MarshalAs(UnmanagedType.LPWStr)]String addressSpaceElementPath,
			ref OTCAddressSpaceBrowseOptions browseOptions, 
			uint addressSpaceElementDataCount,  
			[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=7)]OTCAddressSpaceElementData[] elements)
		{	
			try{	
				
				Hashtable synchronizedDaSessionList = Hashtable.Synchronized(m_daSessionList);				
				DaSession currentDaSession = (DaSession)synchronizedDaSessionList[(int)sessionData.m_userData];
				if(currentDaSession != null)
				{		
					if ( result == (int)EnumResultCode.S_OK)
					{
						DaAddressSpaceElement[] addressSpaceElements = new DaAddressSpaceElement[addressSpaceElementDataCount];	
						for (int i = 0; i< addressSpaceElementDataCount; i++){
							addressSpaceElements[i] = new DaAddressSpaceElement(
								Marshal.PtrToStringUni((IntPtr)elements[i].m_name),
								Marshal.PtrToStringUni((IntPtr)elements[i].m_itemID),
								Marshal.PtrToStringUni((IntPtr)elements[i].m_itemPath),
								elements[i].m_objectHandle,
								null);							
						}
						currentDaSession.OnBrowse(
							executionContext,
							addressSpaceElementHandle,
							addressSpaceElementID,
							addressSpaceElementPath,
							addressSpaceElements,
							result);
					}
					else
					{
						currentDaSession.OnBrowse(
							executionContext,
							addressSpaceElementHandle,
							string.Empty,
							string.Empty,
							null,
							result);
					}					

					Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
					AeSession currentAeSession = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
					if(currentAeSession != null)
					{
						if (result == (int)EnumResultCode.S_OK)
						{
							AddressSpaceElement[] addressSpaceElements = new AddressSpaceElement[addressSpaceElementDataCount];	
							for (int i = 0; i< addressSpaceElementDataCount; i++)
							{
								addressSpaceElements[i] = new AddressSpaceElement(
									Marshal.PtrToStringUni((IntPtr)elements[i].m_name),
									Marshal.PtrToStringUni((IntPtr)elements[i].m_itemID),
									elements[i].m_objectHandle);
							}
							currentAeSession.OnBrowse(
								executionContext,
								addressSpaceElementHandle,
								addressSpaceElements,
								result);
						}
						else{
							currentAeSession.OnBrowse(
								executionContext,
								addressSpaceElementHandle,
								null,
								result);
						}
					}	
				} //end if
			}catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleBrowseAddressSpace", 
					exc.ToString());
			}
		}// end HandleBrowseAddressSpace
		
	
		internal void HandleBrowseServer(
			uint executionContext, 
			int result, 
			String ipAddress, 
			byte whatOPCSpec, 
			byte whatServerData, 
			uint serverDataCount, 
			OTCServerData[] serverDataOTC)
		{
			try{				
				Hashtable synchronizedServerBrowerList = Hashtable.Synchronized(m_serverBrowserList);
				ServerBrowser serverBrowser = (ServerBrowser)synchronizedServerBrowerList[(int)executionContext];
				if(serverBrowser != null)
				{
					EnumOPCSpecification whatSpecification = (EnumOPCSpecification)whatOPCSpec;
					EnumServerBrowserData whatData		   = (EnumServerBrowserData)whatServerData;						
				
					ServerBrowserData[] serverData = new ServerBrowserData[serverDataCount];
					
					for(int i = 0; i < serverDataCount; i++)
					{
						serverData[i] = new ServerBrowserData(
							Marshal.PtrToStringUni(serverDataOTC[i].m_clsid),
							Marshal.PtrToStringUni(serverDataOTC[i].m_progId),
							Marshal.PtrToStringUni(serverDataOTC[i].m_description),
							Marshal.PtrToStringUni(serverDataOTC[i].m_vProgId),
							(EnumOPCSpecification)serverDataOTC[i].m_opcSpecification,
							Marshal.PtrToStringUni(serverDataOTC[i].m_url));									
					}// end for
					serverBrowser.OnBrowseServers(
						executionContext,
						ipAddress,
						serverData,
						result);
				} //end if										
			} 	
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleBrowseServer", 
					exc.ToString());
			}
		}//end HandleBrowseServer
			
	
		internal void HandleGetDaProperties(
			uint executionContext, 
			OTObjectData sessionData, 
			int result, 
			IntPtr addressSpaceElementHandle, 
			String addressSpaceElementId, 
			String addressSpaceElementPath, 
			OTCGetDAPropertiesOptions getPropertiesOptions, 
			uint propertiesDataCount,
			OTCDAPropertyData[] propertyData)
		{
			try{	
				DaProperty[] someProperties = new DaProperty[propertiesDataCount];						
				
				Hashtable synchronizedSessionList = Hashtable.Synchronized(m_daSessionList);
				DaSession currentSession = (DaSession)synchronizedSessionList[(int)sessionData.m_userData];	

				if (currentSession != null)
				{
					for(int i = 0; i < propertiesDataCount; i++)
					{
						someProperties[i] = new DaProperty(
							propertyData[i].m_ID,
							Marshal.PtrToStringUni(propertyData[i].m_name),
							Marshal.PtrToStringUni(propertyData[i].m_itemID),
							Marshal.PtrToStringUni(propertyData[i].m_itemPath),
							Marshal.PtrToStringUni(propertyData[i].m_description),
							ValueQT.GetSysType(propertyData[i].m_datatype),
							new ValueQT(ref propertyData[i].m_value),
							propertyData[i].m_result);
					} //end for

					currentSession.OnGetDaProperties(
						executionContext,
						addressSpaceElementHandle,
						addressSpaceElementId,
						addressSpaceElementPath,
						someProperties,
						result);
				} //end if											
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleGetDaProperties", 
					exc.ToString());
			}
		}
		
	
		private int ChangeCondition(
			bool conditionAlreadyExists,
			AeSubscription subscription,
			AeCondition anAeCondition,
			AeCondition newAeCondition)
		{
			try{
				if(!conditionAlreadyExists){
					if(((newAeCondition.State & EnumConditionState.ACTIVE) != EnumConditionState.ACTIVE)//not active
						&&																			//and 			
						((newAeCondition.State & EnumConditionState.ACKED) == EnumConditionState.ACKED)){//aknowledged
						return -1;
					}//end if
					
					//new condition
					anAeCondition.AckRequired = newAeCondition.AckRequired;
					anAeCondition.Category = newAeCondition.Category;
					anAeCondition.Attributes = newAeCondition.Attributes;
					anAeCondition.ChangeMask = newAeCondition.ChangeMask;
					anAeCondition.Quality = newAeCondition.Quality;
					anAeCondition.EventType = newAeCondition.EventType;
					anAeCondition.State = newAeCondition.State;
					anAeCondition.SourcePath = newAeCondition.SourcePath;
					anAeCondition.ConditionName = newAeCondition.ConditionName;
					anAeCondition.SubConditionName = newAeCondition.SubConditionName;
					anAeCondition.OccurenceTime = newAeCondition.OccurenceTime;
					anAeCondition.Severity = newAeCondition.Severity;
					anAeCondition.Message = newAeCondition.Message;
					anAeCondition.ActiveTime = newAeCondition.ActiveTime;
					anAeCondition.Cookie = newAeCondition.Cookie;
					anAeCondition.ActorId = newAeCondition.ActorId;
					anAeCondition.AeSession = newAeCondition.AeSession;					
				
					//add condition to conditionList
					subscription.m_conditionList.Add(anAeCondition);
					AeCondition[] conditions = new AeCondition[subscription.m_conditionList.Count];
					for(int i = 0; i< subscription.m_conditionList.Count; i++){
						conditions[i] = (AeCondition)subscription.AeConditionList[i];
					}//end for

					subscription.OnAeConditionsChanged(conditions);

				}//end if
				else{
					if(((newAeCondition.State & EnumConditionState.ACTIVE) != EnumConditionState.ACTIVE)//not active
						&&																			//and 	
						((newAeCondition.State & EnumConditionState.ACKED) == EnumConditionState.ACKED)){//aknowledged
						
						//remove condition from list
						subscription.m_conditionList.Remove(anAeCondition);
						AeCondition[] conditions = new AeCondition[subscription.m_conditionList.Count];
						for(int i = 0; i< subscription.m_conditionList.Count; i++){
							conditions[i] = (AeCondition)subscription.AeConditionList[i];
						}//end for

						subscription.OnAeConditionsChanged(conditions);
					}
					else{
						//set the changed values
						//new condition
						anAeCondition.AckRequired = newAeCondition.AckRequired;
						anAeCondition.Category = newAeCondition.Category;
						anAeCondition.Attributes = newAeCondition.Attributes;
						anAeCondition.ChangeMask = newAeCondition.ChangeMask;
						anAeCondition.Quality = newAeCondition.Quality;
						anAeCondition.EventType = newAeCondition.EventType;
						anAeCondition.State = newAeCondition.State;
						anAeCondition.SubConditionName = newAeCondition.SubConditionName;
						anAeCondition.OccurenceTime = newAeCondition.OccurenceTime;
						anAeCondition.Severity = newAeCondition.Severity;
						anAeCondition.Message = newAeCondition.Message;
						anAeCondition.ActiveTime = newAeCondition.ActiveTime;
						anAeCondition.Cookie = newAeCondition.Cookie;
						anAeCondition.ActorId = newAeCondition.ActorId;
						anAeCondition.AeSession = newAeCondition.AeSession;												
					
						AeCondition[] conditions = new AeCondition[subscription.m_conditionList.Count];
						for(int i = 0; i< subscription.m_conditionList.Count; i++){
							conditions[i] = (AeCondition)subscription.AeConditionList[i];
						}//end for
						subscription.OnAeConditionsChanged(conditions);
					}//end else
				}//end else
				
			}//try
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. ChangeCondition", 
					exc.ToString());
			}
			return 0;
		}
		
	
		internal void HandleEventsReceived(
			OTCObjectContext objectContext,
			uint count, 
			OTCEventData[] eventData,
			byte refresh, 
			byte lastRefresh)
		{
			try{			
				if (objectContext.m_objectType == (uint)EnumObjectType.AESUBSCRIPTION)
				{					
					Hashtable synchronizedAeServerList = Hashtable.Synchronized(m_aeSessionList);
					AeSession session = (AeSession)synchronizedAeServerList[(int)objectContext.m_sessionData.m_userData];
					
					if(session != null)
					{
						AeSubscription subscription = session.Find((int)objectContext.m_objectData.m_userData);
						
						if(subscription != null){
							AeEvent[] events = new Softing.OPCToolbox.Client.AeEvent[count];
							for(int i = 0; i< count; i++){																				
								bool ackRequired = (eventData[i].m_ackRequired == 1)? true:false;					
								ArrayList attributes = new ArrayList((int)eventData[i].m_eventAttrCount);								

								for(int j = 0; j< eventData[i].m_eventAttrCount; j++){
									IntPtr attribute = (IntPtr)((uint)eventData[i].m_pEventAttrs + ValueQT.VARIANT_SIZE * j);									
									object myData = (object)Marshal.GetObjectForNativeVariant(attribute);
									attributes.Add(myData);
								}

								if(((eventData[i].m_eventType) & ((uint)EnumEventType.CONDITION))!= (uint)EnumEventType.CONDITION){
									events[i] = new Softing.OPCToolbox.Client.AeEvent(
										(EnumEventType)eventData[i].m_eventType,
										eventData[i].m_eventCategory,
										eventData[i].m_severity,
										Marshal.PtrToStringUni(eventData[i].m_sourcePath),
										Marshal.PtrToStringUni(eventData[i].m_message),
										eventData[i].m_occurenceTime.ToDateTime(),
										attributes,
										Marshal.PtrToStringUni(eventData[i].m_actorID));				
								}
								else {
									//maintains the subscription's conditions list
									//the conditions list contains all active and/or not acknowledged condition-related events			
									events[i] = new Softing.OPCToolbox.Client.AeCondition(
										(EnumEventType)eventData[i].m_eventType,
										eventData[i].m_eventCategory,
										eventData[i].m_severity,
										Marshal.PtrToStringUni(eventData[i].m_sourcePath),
										Marshal.PtrToStringUni(eventData[i].m_message),
										eventData[i].m_occurenceTime.ToDateTime(),
										attributes,
										Marshal.PtrToStringUni(eventData[i].m_actorID),
										(EnumConditionChange)eventData[i].m_changeMask,
										(EnumConditionState)eventData[i].m_newState,
										ackRequired,
										(EnumQuality)eventData[i].m_quality,
										Marshal.PtrToStringUni(eventData[i].m_conditionName),
										Marshal.PtrToStringUni(eventData[i].m_subConditionName),
										eventData[i].m_activeTime.ToDateTime(),
										eventData[i].m_cookie);
									bool conditionExists = false;
									lock(subscription.AeConditionList.SyncRoot){
										AeCondition[] conditions = subscription.AeConditionList;
										foreach(AeCondition condition in conditions){
											if (( condition.ConditionName == ((AeCondition)events[i]).ConditionName)&&  (condition.SourcePath == events[i].SourcePath)){
												conditionExists = true;
												ChangeCondition(true,
													subscription,
													condition,
													(AeCondition)events[i]);
											}//end if
										}//end foreach
										// not found condition -> add it
										if(!conditionExists){
											AeCondition newCondition = new AeCondition();
											int result = ChangeCondition(false,subscription,newCondition,(AeCondition)events[i]);
										}//end if
									}//end lock
								}//end if events
							}//end for
				
							bool bRefresh = (refresh == 1)? true:false;
							bool bLastRefresh = (lastRefresh == 1)? true:false;

							subscription.OnAeEventsReceived(bRefresh,bLastRefresh,events);
						} //end if
					} //end if

				} //end if			
			}//try
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleEventsReceived", 
					exc.ToString());
			}//end try
		}//end HandleEventsReceived
		

		internal void HandleQueryAeCategories(
			uint executionContext, 
			OTObjectData sessionData, 
			int result, 
			uint count, 
			uint[] pEventTypes, 
			uint[] pCategoryIds,
			string[] pCategoryDescriptions)
		{
			try{											
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession session = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
				if(session != null)
				{
					AeCategory[] categories = new Softing.OPCToolbox.Client.AeCategory[count];
					for(int i = 0; i< count; i++)
					{
						categories[i] = new Softing.OPCToolbox.Client.AeCategory(
							(EnumEventType)pEventTypes[i],
							pCategoryIds[i],
							pCategoryDescriptions[i],
							session);
					}//end for								
					session.OnQueryAeCategories(
						executionContext,
						categories,
						result);
				}//end if			
			}//try
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleQueryAeCategories", 
					exc.ToString());
			}// end try
		}//end HandleQueryAeCategories
		
		
		internal void HandleQueryAeAttributes(
			uint executionContext,
			OTObjectData sessionData, 
			int result, 
			uint categoryId, 
			uint count, 
			uint[] pAttributeIds,
			string[] pAttributeDescriptions, 
			ushort[] pAttributeDatatypes)
		{
			try{				
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession session = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];

				if (session != null)
				{
					Softing.OPCToolbox.Client.AeAttribute[] attributes = new Softing.OPCToolbox.Client.AeAttribute[count];

					int uintSize = Marshal.SizeOf(typeof(uint));
					int ptrSize = Marshal.SizeOf(typeof(IntPtr));
					int ushortSize = Marshal.SizeOf(typeof(ushort));

					for (int i = 0; i < count; i++)
					{
						attributes[i] = new Softing.OPCToolbox.Client.AeAttribute(
							pAttributeIds[i],
							pAttributeDescriptions[i],
							ValueQT.GetSysType(pAttributeDatatypes[i]));
					}	//	end for									
					Hashtable synchronizedCategoryList = Hashtable.Synchronized(session.m_categoryList);
					foreach (AeCategory category in synchronizedCategoryList.Values)
					{
						if(category.Id == categoryId)
						{
							category.OnQueryAeAttributes(
								executionContext,
								categoryId,
								attributes,
								result);
						}	//	end if
					}	//	ebd foreach
				}	//	end if			
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleQueryAeAttributes", 
					exc.ToString());
			}//catch
		}//end HandleQueryAeAttributes
		
	
		internal void HandleQueryAeConditions(
			uint executionContext,
			OTObjectData sessionData,
			int result, 
			uint categoryId, 
			uint count, 
			string[] pConditionsNames)
		{
			try{				
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession session = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
				if(session != null){
					Hashtable synchronizedCategoryList = Hashtable.Synchronized(session.m_categoryList);
			
					foreach (AeCategory category in synchronizedCategoryList.Values)
					{
						if(category.Id == categoryId)
						{
							category.OnQueryAeConditionNames(
								executionContext,
								categoryId,
								pConditionsNames,
								result);
						}//end if
					}//end foreach
				} //end if				
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleQueryAeConditions", 
					exc.ToString());
			}	//	end try...catch
		}//end HandleQueryAeConditions

	
		internal void HandleQueryAeSubConditions(
			uint executionContext, 
			OTObjectData sessionData, 
			int result, 
			string conditionName, 
			uint count, 
			string[] pSubConditionsNames)
		{
			try{				
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession session = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
				if(session != null)
				{
					Hashtable synchronizedCategoryList = Hashtable.Synchronized(session.m_categoryList);
					foreach (AeCategory category in synchronizedCategoryList.Values)
					{
						for(int i = 0; i < category.m_conditionNameList.Length; i++)
						{
							if(category.m_conditionNameList[i] == conditionName){
								category.OnQueryAeSubConditionNames(
									executionContext,
									conditionName,
									pSubConditionsNames,
									result);
							}//end if
						}//end for
					}//end foreach
				} //end if				
			}//try
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleQueryAeSubConditions", 
					exc.ToString());
			}//catch
		}//end HandleQueryAeSubConditions

		
		internal void HandleQueryAvailableAeFilters(
			uint executionContext, 
			OTObjectData sessionData, 
			int result, 
			uint availableFilters)
		{	
			try{
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession session = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
				if(session != null)
				{
					session.OnQueryAvailableAeFilters(
						executionContext,
						(EnumFilterBy)availableFilters,
						result);
				} //end if
			}//try
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleQueryAvailableAeFilters", 
					exc.ToString());
			}//catch
		}//end HandleQueryAvailableAeFilters
		
	
		internal void HandleEnableAeConditions(
			uint executionContext, 
			OTObjectData sessionData, 
			int result, 
			byte enable, 
			byte areaOrSource, 
			string path)
		{	
			try{
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession session = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
				bool enabled = (enable == 1)? true:false;

				if (session != null)
				{
					if(areaOrSource == 1)
					{
						session.OnEnableAeConditionsByArea(
							executionContext,
							enabled,
							path,
							result);
					}//end if
					if(areaOrSource == 0)
					{
						session.OnEnableAeConditionsBySource(
							executionContext,
							enabled,
							path,
							result);
					}//end if
				}//end if
			}//try
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleEnableAeConditions", 
					exc.ToString());
			}//end try
		}//end HandleEnableAeConditions


		internal void HandleAcknowledgeConditions(
			uint executionContext, 
			OTObjectData sessionData, 
			int result, 
			string  ackID, 
			string ackComment, 
			uint count,
			OTCEventData[] pEvents,
			int[] pResults) 
		{
			try{				
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession session = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
				if(session != null)
				{					
					AeCondition[] conditions = new AeCondition[count]; 
				
					for(int i = 0; i< count; i++)
					{
						conditions[i] = new AeCondition();
						conditions[i].AckRequired = (pEvents[i].m_ackRequired == 1)? true:false;
						conditions[i].ActiveTime = pEvents[i].m_activeTime.ToDateTime();
						conditions[i].ActorId = Marshal.PtrToStringUni(pEvents[i].m_actorID);	
						conditions[i].Attributes = new ArrayList((int)pEvents[i].m_eventAttrCount);
						for(int j = 0; j<  pEvents[i].m_eventAttrCount; j++)
						{
							object myData = (uint)Marshal.GetObjectForNativeVariant((IntPtr)((uint) pEvents[i].m_pEventAttrs+(Marshal.SizeOf(typeof(object))*j)));
							conditions[i].Attributes[j] = myData;
						}//end for
						conditions[i].Category = pEvents[i].m_eventCategory;
						conditions[i].ChangeMask = (EnumConditionChange)pEvents[i].m_changeMask;
						conditions[i].ConditionName = Marshal.PtrToStringUni(pEvents[i].m_conditionName);
						conditions[i].Cookie = pEvents[i].m_cookie;
						conditions[i].Message = Marshal.PtrToStringUni(pEvents[i].m_message);
						conditions[i].State = (EnumConditionState)pEvents[i].m_newState;
						conditions[i].OccurenceTime = pEvents[i].m_occurenceTime.ToDateTime();
						conditions[i].Quality = (EnumQuality)pEvents[i].m_quality;
						conditions[i].Severity = pEvents[i].m_severity;
						conditions[i].SourcePath = Marshal.PtrToStringUni(pEvents[i].m_sourcePath);
						conditions[i].SubConditionName = Marshal.PtrToStringUni(pEvents[i].m_subConditionName);
						conditions[i].EventType = (EnumEventType)pEvents[i].m_eventType;
					}//end for
				
					session.OnAcknowledgeAeConditions(
						executionContext,
						ackID,
						ackComment,
						conditions,
						pResults,
						result);
				}				
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleAcknowledgeConditions", 
					exc.ToString());
			}//end try
		}//end HandleAcknowledgeConditions
				
		internal void HandleQuerySourceConditions(
			uint executionContext, 
			OTObjectData sessionData, 
			int result, 
			string sourcePath, 
			uint count, 
			string[] pConditionsNames)
		{
			try{				
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession session = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
				if(session != null)
				{
					session.OnQueryAeSourceConditions(
						executionContext,
						sourcePath,
						pConditionsNames,
						result);
				}				
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleQuerySourceConditions", 
					exc.ToString());
			}//end try	
		}// end HandleQuerySourceConditions
		
	
		internal void HandleGetConditionState(
			uint executionContext, 
			OTObjectData sessionData, 
			int result, 
			string sourcePath, 
			string conditionName, 
			uint attributeCount, 
			uint[] pAttributeIds, 
			OTCAEConditionStateData pConditionState)
		{	
			try{

				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession session = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];
				if(session != null)
				{
					AeConditionState conditionState = new AeConditionState();
					conditionState.AcknowledgerComment = Marshal.PtrToStringUni(pConditionState.m_ackComment);
					conditionState.AcknowledgerId = Marshal.PtrToStringUni(pConditionState.m_ackID);
					conditionState.ConditionAckTime = pConditionState.m_conditionAckTime.ToDateTime();
					conditionState.ConditionActiveTime = pConditionState.m_conditionActiveTime.ToDateTime();
					conditionState.ConditionInactiveTime =pConditionState.m_conditionInactiveTime.ToDateTime();
					conditionState.Quality = (EnumQuality)pConditionState.m_quality;
					conditionState.State = (EnumConditionState)pConditionState.m_state;
					conditionState.SubConditionActiveTime = pConditionState.m_subConditionActiveTime.ToDateTime();
					conditionState.ActiveSubConditionDefinition = Marshal.PtrToStringUni(pConditionState.m_actSubConditionDefinition);
					conditionState.ActiveSubConditionDescription = Marshal.PtrToStringUni(pConditionState.m_actSubConditionDescription);
					conditionState.ActiveSubConditionName = Marshal.PtrToStringUni(pConditionState.m_actSubConditionName);
					conditionState.ActiveSubConditionSeverity = pConditionState.m_actSubConditionSeverity;
					conditionState.SubConditionActiveTime = pConditionState. m_subConditionActiveTime.ToDateTime();
					
					uint countSubconditions = pConditionState.m_subConditionCount;
					conditionState.SubConditionsDefinitions = new string[countSubconditions];
					conditionState.SubConditionsDescriptions = new string[countSubconditions];
					conditionState.SubConditionsNames = new string[countSubconditions];
					conditionState.SubConditionsSeverities = new uint[countSubconditions];
					
					int ptrSize = Marshal.SizeOf(typeof(IntPtr));
					int uintSize = Marshal.SizeOf(typeof(uint));

					for(int i = 0; i < countSubconditions; i++)
					{
						IntPtr subConditionName = (IntPtr)Marshal.PtrToStructure((IntPtr)((uint)pConditionState.m_pSubConditionName + (i * ptrSize)) ,typeof(IntPtr));
						IntPtr subConditionDefinition = (IntPtr)Marshal.PtrToStructure((IntPtr)((uint)pConditionState.m_pSubConditionDefinition + (i * ptrSize)) ,typeof(IntPtr));
						IntPtr subConditionDescription = (IntPtr)Marshal.PtrToStructure((IntPtr)((uint)pConditionState.m_pSubConditionDescription + (i * ptrSize)) ,typeof(IntPtr));
						conditionState.SubConditionsSeverities[i] = (uint)Marshal.PtrToStructure((IntPtr)((uint)pConditionState.m_pSubConditionSeverity + (i * uintSize)) ,typeof(uint));
						
						conditionState.SubConditionsDefinitions [i] = Marshal.PtrToStringUni(subConditionDefinition);
						conditionState.SubConditionsDescriptions[i] =  Marshal.PtrToStringUni(subConditionDescription);
						conditionState.SubConditionsNames[i] =  Marshal.PtrToStringUni(subConditionName);
					}//end for

					uint countAttributes = pConditionState.m_eventAttrCount;
					conditionState.EventAttributes = new ArrayList((int)countAttributes);
					conditionState.AttributesErrors = new int[countAttributes];

					for(int i = 0; i< countAttributes; i++)
					{
						object eventAttributte = (object)Marshal.GetObjectForNativeVariant((IntPtr)((uint)pConditionState.m_pEventAttrs + (Marshal.SizeOf(typeof(object))*i)));
						conditionState.EventAttributes[i] = eventAttributte;
						conditionState.AttributesErrors[i] = (int)Marshal.PtrToStructure((IntPtr)((uint)pConditionState.m_pAttrErrors + (i * Marshal.SizeOf(typeof(int)))) ,typeof(int));
						
					}//end for
				
					session.OnGetAeConditionState(
						executionContext,
						sourcePath,
						conditionName,
						pAttributeIds,
						conditionState,
						result);
				}
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleGetConditionState", 
					exc.ToString());
			}//end try
		}// end HandleGetConditionState
			
		
		internal void HandleRefreshConditions(
			uint executionContext, 
			OTCObjectContext objectContext,
			int result,
			byte cancelRefresh)
		{		
			try{
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				if(objectContext.m_objectType == (uint)EnumObjectType.AESUBSCRIPTION)
				{
					AeSession currentSession = (AeSession)synchronizedAeSessionList[(int)objectContext.m_sessionData.m_userData];
					if(currentSession != null)
					{						
						AeSubscription currentSubscription = currentSession.Find((int)objectContext.m_objectData.m_userData);
						if(currentSubscription != null)
						{
							if (cancelRefresh == 1)
							{
								currentSubscription.OnCancelRefreshAeConditions(executionContext,result);
							}
							if(cancelRefresh == 0)
							{
								currentSubscription.OnRefreshAeConditions(executionContext,result);					
							}
						} //end if
					} //end if
				}
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleRefreshConditions", 
					exc.ToString());
			}//end try		
		}//end HandleRefreshConditions

		
		internal void HandleGetErrorString(
			uint executionContext, 
			OTObjectData sessionData, 
			int result, 
			int errorCode,
			string errorString)
		{
			try{
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession anAeSession = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];

				Hashtable synchronizedDaSessionList = Hashtable.Synchronized(m_daSessionList);
				DaSession aDaSession = (DaSession)synchronizedDaSessionList[(int)sessionData.m_userData];
							
				if(aDaSession != null)
				{
					aDaSession.OnGetErrorString(
						executionContext,
						errorCode,
						errorString,
						result);
				}
				if(anAeSession != null)
				{
					anAeSession.OnGetErrorString(
						executionContext,
						errorCode,
						errorString,
						result);
				}
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleGetErrorString", 
					exc.ToString());
			}
		}

	
		internal void HandleLogon(
			uint executionContext,
			OTObjectData sessionData, 
			int result, 
			string userName, 
			string password)
		{
			try{		
				Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
				AeSession anAeSession = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];

				Hashtable synchronizedDaSessionList = Hashtable.Synchronized(m_daSessionList);
				DaSession aSession = (DaSession)synchronizedDaSessionList[(int)sessionData.m_userData];
		
				if(aSession != null)
				{
					aSession.OnLogon(
						executionContext,
						userName,
						password,
						result);
				}//end if
				if(anAeSession != null)
				{
					anAeSession.OnLogon(
						executionContext,
						userName,
						password,
						result);
				}//end if
			}//try
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleLogon", 
					exc.ToString());
			}//end catch
		}//end HandleLogon


		internal void HandleLogoff(uint executionContext, OTObjectData sessionData,int result)
		{
			try{
				
					Hashtable synchronizedAeSessionList = Hashtable.Synchronized(m_aeSessionList);
					AeSession anAeSession = (AeSession)synchronizedAeSessionList[(int)sessionData.m_userData];

					Hashtable synchronizedDaSessionList = Hashtable.Synchronized(m_daSessionList);
					DaSession aSession = (DaSession)synchronizedDaSessionList[(int)sessionData.m_userData];
			
					if(aSession != null){
						aSession.OnLogoff(executionContext,result);
					}//end if
					if(anAeSession != null){
						anAeSession.OnLogoff(executionContext,result);
					}//end if

			}//try
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. HandleLogoff", 
					exc.ToString());
			}//catch
		}//end HandleLogoff

		//-
		#endregion

		//-
		#endregion

		#region //Public Methods
		//----------------------	
		
		/// <summary>
		/// Activates the speciffied Softing OPC functionality according to the Feature provided
		/// </summary>
		/// <param name="feature">A features the licensed product. <see cref="EnumFeature"/> </param> 
		/// <param name="key">Design Time License Key in string format : "XXXX-XXXX-XXXX-XXXX-XXXX".</param> 
		/// <returns>
		/// S_OK Everything was OK
		/// E_FAIL Actrivation has failed. Please note the provided key must match the feature
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="Activate"]/doc/*'
		/// />		
		public int Activate (EnumFeature feature, string key)
		{
			int res = (int)EnumResultCode.E_FAIL;
			try{
				res = OTBFunctions.OTActivate((byte)feature, key, 0);

				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application.Activate", 
					" Result of activating application:" + res);
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application.Activate", 
					exc.ToString());			
			}
			return res;
		}	//	end Activate


		/// <summary>
		/// Forces the target application to run as demo.
		/// </summary>
		/// <returns>
		/// S_OK Everything was OK
		/// E_FAIL Actrivation has failed
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED		
		///</returns>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="ForceDemoVersion"]/doc/*'
		/// />		
		public int ForceDemoVersion ()
		{
			return OTBFunctions.OTActivate(0, string.Empty, 1);
		}	//	end ForceDemoVersion



		/// <summary>
		/// Initializes the application.
		/// </summary>
		/// <returns>The result of initializing the application.</returns>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="Initialize"]/doc/*'
		/// />			
		public int Initialize()
		{
			int res = (int)EnumResultCode.E_FAIL;
			try{
				if (!m_isInit){

					res = OTBFunctions.OTCInitialize(m_initData);	
					if(ResultCode.SUCCEEDED(res)){
						m_isInit = true;						
					}	
					else{
						Application.Instance.Trace(
							EnumTraceLevel.ERR,
							EnumTraceGroup.CLIENT,
							"Application. Initialize", 
							" Application initialization failed! Result: " + res);
					}
				}
				Advise();
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application. Initialize", 
					exc.ToString());			
			}
			return res;
		}


		/// <summary>
		/// Processes the given parameters concerning registering or unregistering the application as a service
		/// </summary>
		/// <param name="aCommandLine">the entire commandline to be parsed</param>
		/// <returns>
		/// S_OK - verything was OK - no argument list was provided
		/// S_FALSE - Everything was OK - argument list successfully parsed and executed
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>
		/// <include
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/ 
		///  method[@name="ProcessCommandLine"]/doc/*'
		/// />
		public int ProcessCommandLine(string aCommandLine) {
			return OTBFunctions.OTCProcessCommandLine(aCommandLine);
		}	//	end ProcessCommandLine


		/// <summary>
		/// Removes a client - Data Access Server session (<see cref="Softing.OPCToolbox.Client.DaSession"/> object) from the application.
		/// </summary>
		/// <param name="aDaSession">A DaSession object to be removed.</param>
		/// <returns>The result of removing a DaSession object.</returns>		
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="RemoveDaSession"]/doc/*'
		/// />
		public int RemoveDaSession(DaSession aDaSession){	
			int res = (int)EnumResultCode.E_FAIL;
			try{
				if(aDaSession != null){
					res = OTBFunctions.OTCRemoveDASession(aDaSession.Handle);
					if(ResultCode.SUCCEEDED(res)){
						
						Hashtable synchronizedSessionList = Hashtable.Synchronized(m_daSessionList);
						synchronizedSessionList.Remove(aDaSession.GetHashCode());
					}
					else{
						Application.Instance.Trace(
						EnumTraceLevel.ERR,
						EnumTraceGroup.CLIENT,
						"Application.RemoveDasession", 
						"The session could not be removed from the application! Result: " + res);
					}	//	end if...else
				}
				else{
					res = (int)EnumResultCode.E_INVALIDARG;
					Application.Instance.Trace(
						EnumTraceLevel.ERR,
						EnumTraceGroup.CLIENT,
						"Application.RemoveDasession", 
						"The argument is invalid! The session cannot be null!");
				}
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application.RemoveDasession",
					exc.ToString());				
			}
			return res;
		}

		/// <summary>
		/// Removes a client - Alarms and Events Server session (<see cref="Softing.OPCToolbox.Client.AeSession"/> object) from the application.
		/// </summary>
		/// <param name="anAeSession">An AeSession object to be removed.</param>
		/// <returns>The result of removing an AeSession object.</returns>		
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="RemoveAeSession"]/doc/*'
		/// />
		public int RemoveAeSession(AeSession anAeSession){
			int res = (int)EnumResultCode.E_FAIL;
			try{
				if(anAeSession != null){
					res = OTBFunctions.OTCRemoveAESession(anAeSession.Handle);
					if(ResultCode.SUCCEEDED(res)){
						Hashtable synchronizedSessionList = Hashtable.Synchronized(m_aeSessionList);
						synchronizedSessionList.Remove(anAeSession.GetHashCode());
					}
					else{
						Application.Instance.Trace(
						EnumTraceLevel.ERR,
						EnumTraceGroup.CLIENT,
						"Application.RemoveAeSession", 
						"The session could not be removed from the application! Result: " + res);
					}	//	end if...else

				}
				else{
					res = (int)EnumResultCode.E_INVALIDARG;
					Application.Instance.Trace(
						EnumTraceLevel.ERR,
						EnumTraceGroup.CLIENT,
						"Application.RemoveAeSession", 
						"The argument is invalid! Session cannot be null!");
				}
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application.RemoveAesession",
					exc.ToString());				
			}
			return res;
		}
		/// <summary>
		/// Terminates the application.
		/// </summary>
		/// <returns>The result of terminating the application.</returns>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="Terminate"]/doc/*'
		/// />				
		public int Terminate(){
			int res = (int)EnumResultCode.E_FAIL;
			try{

				m_OTNCCallBacks.m_trace						-= new OTOnTrace(HandleTrace);
				m_OTNCCallBacks.m_stateChange				-= new OTCOnStateChange(HandleStateChange);
				m_OTNCCallBacks.m_dataChange				-= new OTCOnDataChange(HandleDataChange);
				m_OTNCCallBacks.m_readComplete				-= new OTCOnReadComplete(HandleReadComplete);
				m_OTNCCallBacks.m_writeComplete				-= new OTCOnWriteComplete(HandleWriteComplete);
				m_OTNCCallBacks.m_perfromStateTransition	-= new OTCOnPerfromStateTransition(HandlePerformStateTransition);
				m_OTNCCallBacks.m_serverShutdown			-= new OTCOnServerShutdown(HandleServerShutdown);
				m_OTNCCallBacks.m_updateAttributes			-= new OTCOnUpdateAttributes(HandleUpdateAttributes);
				m_OTNCCallBacks.m_updateDaItemAttributes	-= new OTCOnUpdateDAItemAttributes(HandleUpdateDaItemAttributes);
				m_OTNCCallBacks.m_validateDaItems			-= new OTCOnValidateDAItems(HandleValidateDaItems);
				m_OTNCCallBacks.m_getServerStatus			-= new OTCOnGetServerStatus(HandleGetServerStatus);
				m_OTNCCallBacks.m_browseAddressSpace		-= new OTCOnBrowseAddressSpace(HandleBrowseAddressSpace);
				m_OTNCCallBacks.m_onGetDaProperties			-= new OTCOnGetDAProperties(HandleGetDaProperties);
				m_OTNCCallBacks.m_browseServer				-= new OTCOnBrowseServer(HandleBrowseServer);	
				m_OTNCCallBacks.m_receivedEvents			-= new OTCOnReceivedEvents(HandleEventsReceived);
				m_OTNCCallBacks.m_refreshConditions			-= new OTCOnRefreshAEConditions(HandleRefreshConditions);
				m_OTNCCallBacks.m_acknowledgeConditions		-= new OTCOnAcknowledgeAEConditions(HandleAcknowledgeConditions);	
				m_OTNCCallBacks.m_queryAvailableFilters		-= new OTCOnQueryAvailableAEFilters(HandleQueryAvailableAeFilters);
				m_OTNCCallBacks.m_queryCategories			-= new OTCOnQueryAECategories(HandleQueryAeCategories);
				m_OTNCCallBacks.m_queryAttributes			-= new OTCOnQueryAEAttributes(HandleQueryAeAttributes);
				m_OTNCCallBacks.m_queryConditons			-= new OTCOnQueryAEConditions(HandleQueryAeConditions);				
				m_OTNCCallBacks.m_querySubConditions		-= new OTCOnQueryAESubConditions(HandleQueryAeSubConditions);
				m_OTNCCallBacks.m_querySourceConditions		-= new OTCOnQueryAESourceConditions(HandleQuerySourceConditions);
				m_OTNCCallBacks.m_getConditionState			-= new OTCOnGetAEConditionState(HandleGetConditionState);
				m_OTNCCallBacks.m_enableConditions			-= new OTCOnEnableAEConditions(HandleEnableAeConditions);
				m_OTNCCallBacks.m_getErrorString			-= new OTCOnGetErrorString(HandleGetErrorString);
				m_OTNCCallBacks.m_logon						-= new OTCOnLogon(HandleLogon);
				m_OTNCCallBacks.m_logoff					-= new OTCOnLogoff(HandleLogoff);

				if ((m_OTNCCallBacks.m_dataChange == null)){		
					res = OTBFunctions.OTCTerminate();
					Hashtable synchronizedServerList = Hashtable.Synchronized(m_daSessionList);
					synchronizedServerList.Clear();
					Hashtable synchronizedaeServerList = Hashtable.Synchronized(m_aeSessionList);
					synchronizedaeServerList.Clear();
					Hashtable synchronizedServerBrowserList = Hashtable.Synchronized(m_serverBrowserList);
					synchronizedServerBrowserList.Clear();
					if(ResultCode.SUCCEEDED(res)){
						if(m_isInit) 
							m_isInit = false;
						if(m_isAdvised) 
							m_isAdvised = false;
					} 
					else{
						Application.Instance.Trace(
							EnumTraceLevel.ERR,
							EnumTraceGroup.CLIENT,
							"Application.Terminate", 
							" Terminate application failed! Result: " + res);
					}
				}
			}catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"Application.Terminate", 
					exc.ToString());			
			}
			return res;
		}	

		/// <summary>
		/// Traces a line.
		/// </summary>
		/// <param name="traceLevel">A level of tracing.</param>
		/// <param name="traceMask">A group to be traced.</param>
		/// <param name="objectId">An identifier for the traced object.</param>
		/// <param name="message">A tracing message.</param>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="Trace"]/doc/*'
		/// />	 
		public void Trace(
			EnumTraceLevel traceLevel,
			EnumTraceGroup traceMask,
			string objectId,
			string message)
		{
			
			Softing.OPCToolbox.Trace.WriteLine((byte)traceLevel,(uint)traceMask,objectId,message);
		}
		/// <summary>
		/// Enables tracing messages for the entire application.
		/// </summary>
		/// <param name="fileName">A name of a file in which the tracing messages are to be written.</param>
		/// <param name="debugLevelMask">A group to be traced at debug level.</param>
		/// <param name="errorLevelMask">A group to be traced at error level.</param>
		/// <param name="infoLevelMask">A group to be traced at informative level.</param>
		/// <param name="warningLevelMask">A group to be traced at warning level.</param>
		/// <param name="fileMaxSize">Maximum file size. Specifying -1 for maximum file size will determine tracing in a single file. The file in which 
		/// the tracing messages will be written is the file specified by the first parameter.</param>		
		/// <param name="maximumBackups"> The maximum number of backups.</param> 
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="EnableTracing"]/doc/*'
		/// />
		public void EnableTracing(
			EnumTraceGroup errorLevelMask,
			EnumTraceGroup warningLevelMask,
			EnumTraceGroup infoLevelMask,
			EnumTraceGroup debugLevelMask,
			string fileName,			
			uint fileMaxSize, 			
			uint maximumBackups){
			
			if (fileName	!=	string.Empty){
				
				Softing.OPCToolbox.Trace.EnableTraceToFile	=	true;
				Softing.OPCToolbox.Trace.MaximumBackups		=	maximumBackups;
				Softing.OPCToolbox.Trace.FileMaxSize		=	1000000;				
				Softing.OPCToolbox.Trace.FileName			=	fileName;

			}	//	end if
			else{

				Softing.OPCToolbox.Trace.EnableTraceToFile	=	false;
			}			
						
			Softing.OPCToolbox.Trace.DebugLevelMask		=	(uint)debugLevelMask;
			Softing.OPCToolbox.Trace.ErrorLevelMask		=	(uint)errorLevelMask;
			Softing.OPCToolbox.Trace.InfoLevelMask		=	(uint)infoLevelMask;
			Softing.OPCToolbox.Trace.WarningLevelMask	=	(uint) warningLevelMask;						
			
		}

		
		//-
		#endregion
	}
}

