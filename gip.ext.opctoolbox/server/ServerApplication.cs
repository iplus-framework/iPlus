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
//  Filename    : ServerApplication.cs                                        |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : OPC Server generic Entry point                              |
//                                                                            |
//-----------------------------------------------------------------------------
using System;
using System.Collections;
using System.Runtime.InteropServices;
using Softing.OPCToolbox;
using Softing.OPCToolbox.OTB;

namespace Softing.OPCToolbox.Server {
	
	#region // Public Delegates
	//-------------------------
		
	/// <summary>
	/// <para>
	/// This will be called when the server life is ended. This is only happening when the server has been started by 
	/// a client and the last client has closed the connection to the server.
	/// </para>
	/// <para lang="Unmanaged C++">
	/// <line>Delegate is not available for Unmanaged C++</line>
	/// </para>
	/// <para lang="Unmanaged C++">
	/// <line>A function pointer definition is used for the calback receive purpose in C++</line>	
	/// <line>typedef long (API_CALL *ShutdownRequestHandler)(void);</line>
	/// </para>
	/// <para>Callback triggered from within the OPC toolbox internals when the OPC server needs to shutdown
	/// </para>
	/// </summary>	
	public delegate int ShutdownHandler();
	
	//-
	#endregion

	/// <summary>
	/// The Application class singleton provides the main functionality of the OPC server interface
	/// </summary>
	/// <include 
	///  file='TBNS.doc.xml' 
	///  path='//class[@name="Application"]/doc/*'
	/// />
	public sealed class Application {

		#region //	Public Singleton Accessor
		//-----------------------------------
		
		/// <summary>
		/// Singleton OPC server's instance public accessor
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="Instance"]/doc/*'
		/// />
		public static Application Instance = new Application();
		
		///<summary>
		///<para lang="Unmanaged C++">Releases all the resources allocated by the application.</para>
		///<para lang="Visual Basic, C#, C++"> The method is not available.</para>
		///</summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="Application"]/method[@name="Release"]/doc/*'
		/// />
		public void Release(){			
			
			Instance.Terminate();
			Instance = null;

		}	//	end Release

		//-
		#endregion
		
		#region //	Public Events
		//-----------------------
		
		/// <summary>
		/// Triggered by the toolbox internal shutdown sequence. E.g. when the last session was closed by the last OPC client.
		/// Normally this will terminate the server application
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  event[@name="ShutdownRequest"]/doc/*'
		/// />
		public event ShutdownHandler ShutdownRequest = null;
		
		/// <summary>
		/// Triggered for each trace output line made by the OPC server application.
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  event[@name="TraceOutput"]/doc/*'
		/// />
		public event TraceEventHandler TraceOutput = null;

		//-
		#endregion
		
		#region //	Public Properties
		//---------------------------

		/// <summary>
		/// The version of the Softing OPC Toolbox core library (OTB.dll)
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="VersionOtb"]/doc/*'
		/// />		
		public short VersionOtb{
			get	{ return m_initData.m_versionOTS; }
			set	{ m_initData.m_versionOTS = value; }
		}	//	end VersionOts

		/// <summary>
		/// The pplication type of OPC server library or executable
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="AppType"]/doc/*'
		/// />
		public EnumApplicationType AppType{
			get	{ return (EnumApplicationType)m_initData.m_appType; }
			set	{ m_initData.m_appType = (byte)value; }
		}	//	end AppType

		/// <summary>
		/// NT Service name of the application running. Left empty for a normal outproc application 
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="ServiceName"]/doc/*'
		/// />		
		public string ServiceName{
			get	{ return m_initData.m_serviceName; }
			set	{ m_initData.m_serviceName = value; }
		}	//	end ServiceName

		/// <summary>
		/// Class Id of COM-DA OPC server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="ClsidDa"]/doc/*'
		/// />				
		public string ClsidDa{
			get	{ return m_initData.m_clsidDA; }
			set	{ m_initData.m_clsidDA = value; }
		}	//	end ClsidDa
		
		/// <summary>
		/// Prog ID of COM-DA OPC server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="ProgIdDa"]/doc/*'
		/// />		
		public string ProgIdDa{
			get	{ return m_initData.m_progIdDA; }
			set	{ m_initData.m_progIdDA = value; }
		}	//	end ProgIdDa

		/// <summary>
		/// Version independent prog ID COM-DA OPC server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="VerIndProgIdDa"]/doc/*'
		/// />		
		public string VerIndProgIdDa{
			get	{ return m_initData.m_verIndProgIdDA; }
			set	{ m_initData.m_verIndProgIdDA = value; }
		}	//	end VerIndProgIdDa
		
		/// <summary>
		/// Description of DA server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="Description"]/doc/*'
		/// />		
		public string Description{
			get	{ return m_initData.m_description; }
			set	{ m_initData.m_description = value; }
		}	//	end Description

		/// <summary>
		/// Class ID of the COM-AE OPC server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="ClsidAe"]/doc/*'
		/// />				
		public string ClsidAe{
			get	{ return m_initData.m_clsidAE; }
			set	{ m_initData.m_clsidAE = value; }
		}	//	end ClsidAe
		
		/// <summary>
		/// Prog ID of the COM-AE OPC server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="ProgIdAe"]/doc/*'
		/// />				
		public string ProgIdAe{
			get	{ return m_initData.m_progIdAE; }
			set	{ m_initData.m_progIdAE = value; }
		}	//	end ProgIdAE

		/// <summary>
		/// Version independent prog ID of the COM-AE OPC server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="VerIndProgIdAe"]/doc/*'
		/// />				
		public string VerIndProgIdAe{
			get	{ return m_initData.m_verIndProgIdAE; }
			set	{ m_initData.m_verIndProgIdAE = value; }
		}	//	end VerIndProgIdAe

		/// <summary>
		/// IP port number of HTTP server. The HTTP server will be used for the XML-DA 
		/// communication or for the Softing's enabled Web server of the OPC Toolbox application.
		/// Left 0 when not in use.
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="IpPortHTTP"]/doc/*'
		/// />				
		public int IpPortHTTP{
			get	{ return m_initData.m_ipPortHTTP; }
			set	{ m_initData.m_ipPortHTTP = value; }
		}	//	end IpPortHTTP

		/// <summary>
		/// URL for the XML-DA server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="UrlDa"]/doc/*'
		/// />				
		public string UrlDa{
			get	{ return m_initData.m_urlDA; }
			set	{ m_initData.m_urlDA = value; }
		}	//	end UrlDa

		/// <summary>
		/// Major version number of the server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="MajorVersion"]/doc/*'
		/// />				
		public short MajorVersion{
			get	{ return m_initData.m_majorVersion; }
			set	{ m_initData.m_majorVersion = value; }
		}	//	end MajorVersion

		/// <summary>
		/// Minor version number of the server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="MinorVersion"]/doc/*'
		/// />				
		public short MinorVersion{
			get	{ return m_initData.m_minorVersion; }
			set	{ m_initData.m_minorVersion = value; }
		}	//	end MinorVersion

		/// <summary>
		/// Patch version number of the server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="PatchVersion"]/doc/*'
		/// />				
		public short PatchVersion
		{
			get	{ return m_initData.m_patchVersion; }
			set	{ m_initData.m_patchVersion = value; }
		}	//	end MinorVersion

		/// <summary>
		/// Build number of the server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="BuildNumber"]/doc/*'
		/// />		
		public short BuildNumber
		{
			get	{ return m_initData.m_buildNumber; }
			set	{ m_initData.m_buildNumber = value; }
		}	//	end BuildNumber

		/// <summary>
		/// Vendor info of the server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="VendorInfo"]/doc/*'
		/// />				
		public string VendorInfo{
			get	{ return m_initData.m_vendorInfo; }
			set	{ m_initData.m_vendorInfo = value; }
		}	//	end VendorInfo

		/// <summary>
		/// Delimiter sign for addressspace levels in the string based ItemId
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="AddressSpaceDelimiter"]/doc/*'
		/// />				
		public char AddressSpaceDelimiter{
			get	{ return m_initData.m_addressSpaceDelimiter; }
			set	{ m_initData.m_addressSpaceDelimiter = value; }
		}	//	end AddressSpaceDelimiter

		/// <summary>
		/// Period in ms to monotor the connections to the OPC clients
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="ClientCheckPeriod"]/doc/*'
		/// />				
		public int ClientCheckPeriod{
			get	{ return m_initData.m_clientCheckPeriod; }
			set	{ m_initData.m_clientCheckPeriod = value; }
		}	//	end ClientCheckPeriod

		/// <summary>
		/// Delimiter sign for DA properties in the Item ID
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="PropertyDelimiter"]/doc/*'
		/// />				
		public char PropertyDelimiter{
			get	{ return m_initData.m_propertyDelimiter; }
			set	{ m_initData.m_propertyDelimiter = value; }
		}	//	end PropertyDelimiter
		
		/// <summary>
		/// Minimal update rate for a group (ms)
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="MinUpdateRateDa"]/doc/*'
		/// />				
		public int MinUpdateRateDa{
			get	{ return m_initData.m_minUpdateRateDA; }
			set	{ m_initData.m_minUpdateRateDA = value; }
		}	//	end MinUpdateRateDa

		/// <summary>
		/// Emables or deisables the AE conditions support
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="SupportDisableConditions"]/doc/*'
		/// />				
		public bool SupportDisableConditions{
			get	{ return System.Convert.ToBoolean(m_initData.m_supportDisableConditions); }
			set	{ m_initData.m_supportDisableConditions = System.Convert.ToByte(value); }
		}	//	end SupportDisableConditions
		
		
		/// <summary>
		/// Used to provide the web interface folder
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="WebRootDirectory"]/doc/*'
		/// />				
		public string WebRootDirectory
		{
			get	{ return m_initData.m_webRootDirectory; }
			set	{ m_initData.m_webRootDirectory = value; }
		}	//	end WebRootDirectory
		
		
		/// <summary>
		/// Provide the web interface root file
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="WebRootFile"]/doc/*'
		/// />				
		public string WebRootFile{
			get	{ return m_initData.m_webRootFile; }
			set	{ m_initData.m_webRootFile = value; }
		}	//	end WebRootFile


		/// <summary>
		/// used to handle the operator password the web interface
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="WebAdministratorPassword"]/doc/*'
		/// />				
		public string WebAdministratorPassword{
			get	{ return m_initData.m_webAdministratorPassword; }
			set	{ m_initData.m_webAdministratorPassword = value; }
		}	//	end WebAdministratorPassword
		

		/// <summary>
		/// used to handle the operator password of the web interface
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="WebOperatorPassword"]/doc/*'
		/// />				
		public string WebOperatorPassword{
			get	{ return m_initData.m_webOperatorPassword; }
			set	{ m_initData.m_webOperatorPassword = value; }
		}	//	end WebOperatorPassword

		/// <summary>
		/// The port number the application listenes on the Softing Tunnel protocol.
		/// When 0 is provided, the the Softing Tunnel protocol is desabled.
		/// The Softing Tunnel functionality is not active in the commercial version of the OPC Toolbox.
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="TpPort"]/doc/*'
		/// />						
		public ushort TpPort
		{
			get { return m_initData.m_tpPort; }
			set { m_initData.m_tpPort = value; }
		}	//	end TpPort

		/// <summary>
		/// Gets the address space root object entry
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="DaAddressSpaceRoot"]/doc/*'
		/// />
		public DaAddressSpaceRoot DaAddressSpaceRoot{
			get	{ return m_DaAddressSpaceRoot; }
		}	//	end DaAddressSpaceRoot

		/// <summary>
		/// Gets the Namespaceroot
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="AeAddressSpaceRoot"]/doc/*'
		/// />		
		public AeAddressSpaceRoot AeAddressSpaceRoot{
			get	{ return m_AeAddressSpaceRoot; }			
		}	//	AeAddressSpaceRoot

		/// <summary>
		/// The Application`s creator for namespace elements, the namespace root, requests, transactions
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="Creator"]/doc/*'
		/// />				
		public Creator Creator{
			get	{ return m_creator; }			
		}	//	end Creator
		
		/// <summary>
		/// returns the Wrb server handling class associated to the OPC server
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="WebTemplate"]/doc/*'
		/// />				
		public WebTemplate WebTemplate{
			get{ return m_webTemplate; }
		}	//	end WebTemplate

		//-
		#endregion
		
		#region //	Public Methods
		//------------------------
		
		/// <summary>
		/// Activates the speciffied Softing OPC functionality according to the Feature provided
		/// </summary>
		/// <param name="aProduct">A features the licensed product. <see cref="EnumFeature"/> </param> 
		/// <param name="aKey">Design Time License Key in string format : "XXXX-XXXX-XXXX-XXXX-XXXX".</param> 
		/// <returns>
		/// S_OK Everything was OK
		/// E_FAIL Actrivation has failed. Please note the provided key must match the feature
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="Activate"]/doc/*'
		/// />
		public int Activate(EnumFeature aProduct, string aKey){
			return OTBFunctions.OTActivate((byte)aProduct, aKey, 0);
		}	//	end Activate
		
		
		/// <summary>
		/// Forces the target application to run as demo even if a proper designtime or runtime license was activated
		/// </summary>
		/// <returns>
		/// S_OK Everything was OK
		/// E_FAIL Actrivation has failed
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="ForceDemoVersion"]/doc/*'
		/// />		
		public int ForceDemoVersion()
		{
			return OTBFunctions.OTActivate(0, string.Empty, 1);
		}	//	end ForceDemoVersion


		/// <summary>
		/// Initializes the OPC server core functionality
		/// </summary>
		/// <returns>
		/// S_OK - Everything was OK
		/// E_FAIL - Failure returned
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>
		/// <include
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="Initialize"]/doc/*'
		/// />
		public int Initialize(Creator aCreator)
		{
			try{
				m_creator = aCreator;
				m_DaAddressSpaceRoot = m_creator.CreateDaAddressSpaceRoot();
				m_AeAddressSpaceRoot = m_creator.CreateAeAddressSpaceRoot();
				m_webTemplate = m_creator.CreateWebTemplate();
				return OTBFunctions.OTSInitialize(m_initData);
			}
			catch(Exception e){
				Application.Instance.Trace(
					EnumTraceLevel.ERR, 
					EnumTraceGroup.OPCSERVER, 
					"Application.Initialize", 
					e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end try ... catch
		}	//	end Initialize
		
		/// <summary>
		/// Terminates the OPC sever application's core functionality
		/// </summary>
		/// <returns>
		/// S_OK - Everything was OK
		/// S_FALSE - Everything was OK    
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="Terminate"]/doc/*'
		/// />
		public int Terminate(){
			
			m_initData.Release();
			return OTBFunctions.OTSTerminate();
		}	//	end Terminate

		/// <summary>
		/// Processes the given parameters concerning registering or unregistering the server;
		/// </summary>
		/// <param name="aCommandLine">the entire commandline to be parsed</param>
		/// <returns>
		/// S_OK - verything was OK - no argument list was provided
		/// S_FALSE - Everything was OK - argument list successfully parsed and executed
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>
		/// <include
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/ 
		///  method[@name="ProcessCommandLine"]/doc/*'
		/// />
		public int ProcessCommandLine(string aCommandLine){
			return OTBFunctions.OTSProcessCommandLine(aCommandLine);
		}	//	end ProcessCommandLine

		/// <summary>
		/// Advises the OTS callbacks 
		/// this method should only be called once internally within TBC or TBN
		/// </summary>
		/// <returns>
		/// S_OK - Everything was OK
		/// </returns>	
		private int Advise(){
			
			m_OTNSCallBacks.m_OTOnTrace = new OTOnTrace(HandleTrace);
			m_OTNSCallBacks.m_OTSShutdown = new OTSShutdown(HandleShutdown);
			m_OTNSCallBacks.m_OTSHandleReadRequests = new OTSHandleReadRequests(HandleReadRequests);
			m_OTNSCallBacks.m_OTSHandleWriteRequests = new OTSHandleWriteRequests(HandleWriteRequests);
			m_OTNSCallBacks.m_OTSQueryProperties = new OTSQueryProperties(HandleQueryProperties);
			m_OTNSCallBacks.m_OTSQueryAddressSpaceElementChildren = new OTSQueryAddressSpaceElementChildren(HandleQueryAddressSpaceElementChildren);
			m_OTNSCallBacks.m_OTSQueryAddressSpaceElementData = new OTSQueryAddressSpaceElementData(HandleQueryAddressSpaceElementData);
						
			m_OTNSCallBacks.m_OTSCreateAddressSpaceElement = new OTSCreateAddressSpaceElement(HandleCreateAddressSpaceElement);
			m_OTNSCallBacks.m_OTSDestroyAddressSpaceElement = new OTSDestroyAddressSpaceElement(HandleDestroyAddressSpaceElement);
						
			m_OTNSCallBacks.m_OTSChangeSessionState = new OTSChangeSessionState(HandleChangeSessionState);
			m_OTNSCallBacks.m_OTSChangeItems = new OTSChangeItems(HandleChangeItems);
			m_OTNSCallBacks.m_OTSQueryCacheValue = new OTSQueryCacheValue(HandleQueryCacheValue);
			
			m_OTNSCallBacks.m_OTSQueryConditions = new OTSQueryConditions(HandleQueryConditions);
			m_OTNSCallBacks.m_OTSAcknowledgeCondition = new OTSAcknowledgeCondition(HandleAcknowledgeCondition);
			m_OTNSCallBacks.m_OTSQueryConditionDefinition = new OTSQueryConditionDefinition(HandleQueryConditionDefinition);
			m_OTNSCallBacks.m_OTSEnableConditions = new OTSEnableConditions(HandleEnableConditions);
			m_OTNSCallBacks.m_OTSWebHandleTemplate = new OTSWebHandleTemplate(HandleWebTemplate);
			
			return OTBFunctions.OTSAdvise(m_OTNSCallBacks);

		}	//	end Advise
		

		/// <summary>
		/// Starts the OPC server's Toolkit internals. The namespace should be created after this method is called.
		/// </summary>
		/// <returns>
		/// S_OK - Everything was OK
		/// S_FALSE - Everything was OK
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="Start"]/doc/*'
		/// />		
		public int Start(){
			return OTBFunctions.OTSStart();
		}	//	end Start
		

		/// <summary>
		/// Should be called after address space(s) initialization. This prepares the internal toolkit components for the IO startup
		/// </summary>
		/// <returns>
		/// S_OK - Everything was OK
		/// S_FALSE - Everything was OK
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="Ready"]/doc/*'
		/// />
		public int Ready(){
			return OTBFunctions.OTSReady();
		}	//	end Ready
		

		/// <summary>
		/// Stops the OPC server's IO and prepares the Toolkit internals for server's shutdown.
		/// </summary>
		/// <returns>
		/// S_OK Everything was OK
		/// S_FALSE Everything was OK    
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="Stop"]/doc/*'
		/// />
		public int Stop(){
			return OTBFunctions.OTSStop();
		}	//	end Stop
				
		/// <summary>
		/// Adds a new AE cathegory to the AE component of the OPC server application instance
		/// </summary>
		/// <param name="aCategory"></param>
		/// <returns>
		/// 
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="AddAeCategory"]/doc/*'
		/// />
		public int AddAeCategory(AeCategory aCategory){
			
			try{
				
				IntPtr categoryHandle = IntPtr.Zero;
				int result = OTBFunctions.OTSAddEventCategory(
					aCategory.Id,
					aCategory.Name,
					(uint)aCategory.EventType, 
					out categoryHandle);

				if (ResultCode.SUCCEEDED(result)){
					aCategory.Handle = categoryHandle;
					Hashtable syncHashtable = Hashtable.Synchronized(this.m_categoryList);
					syncHashtable.Add(categoryHandle, aCategory);
				}	//	end if

				return result;
			}
			catch(Exception e){
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.AddAeCategory",
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	enc try ... catch

		}	//	end AddAeCategory
		
		/// <summary>
		/// Retrieves the Ae Cathegory list tored in the OPC server application
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="AeCategoryList"]/doc/*'
		/// />
		public ICollection AeCategoryList {
			get{
				Hashtable syncHashtable = Hashtable.Synchronized(this.m_categoryList);
				return syncHashtable.Values;
			}	//	end get
		}	//	end AeCategoryList
		
		/// <summary>
		/// Trigger the change of a single AE condition probided as a parameter
		/// </summary>
		/// <param name="aCondition"></param>
		/// <returns>
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="ChangeCondition"]/doc/*'
		/// />
		public int ChangeCondition(AeCondition aCondition){
			
			try{
				if (aCondition.UserData != 0){					
					//	this is not accepted
					return (int)EnumResultCode.E_FAIL;
				}	//	end if

				return this.ChangeConditions(new AeCondition[]{aCondition});
			}
			catch(Exception e){
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.AddCondition",
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	try ... catch

		}	//	end AddCondition
		
		
		/// <summary>
		/// Triggers the change of several AE conditions provided in the parameters list
		/// </summary>
		/// <param name="aConditionList"></param>
		/// <returns></returns>
		/// <include
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="ChangeConditions"]/doc/*'
		/// />
		public int ChangeConditions(ICollection aConditionList){

			try{

				uint count = (uint)aConditionList.Count;
				if (count == 0){
					return (int)EnumResultCode.S_FALSE;
				}	//	end if
				
				uint[] inHandles = new uint[count];
				OTSConditionData[] conditionData = new OTSConditionData[count];
				uint[] outHandles = new uint[count];
				
				int index = 0;
				IntPtr conditionsData = Marshal.AllocCoTaskMem((int)count * Marshal.SizeOf(typeof(OTSConditionData)));
				IntPtr indexPointer = conditionsData;
				foreach(AeCondition condition in aConditionList){
					
					conditionData[index] = new OTSConditionData();
					condition.FillOTSConditionData(ref conditionData[index]);
					Marshal.StructureToPtr(conditionData[index], indexPointer, false);

					inHandles[index] = condition.Handle;
					outHandles[index] = 0;
					
					indexPointer = new IntPtr(indexPointer.ToInt64() + Marshal.SizeOf(typeof(OTSConditionData)));
					index++;
				}	//	end for
				
				int result = OTBFunctions.OTSConditionsChanged(
					count, 
					inHandles, 
					conditionsData, 
					outHandles);
				
				for(index = 0; index < count; index++){
					conditionData[index].Release();
				}	//	end for
				
				Marshal.FreeCoTaskMem(conditionsData);
			
				if (ResultCode.SUCCEEDED(result)){
			
					Hashtable syncConditions = Hashtable.Synchronized(this.m_conditionList);
					index = 0;
					foreach(AeCondition condition in aConditionList){
						if (inHandles[index] == 0){
							if (outHandles[index] == 0){
								//	strange situation, new condition to add, but not accepted
								result = (int)EnumResultCode.E_FAIL;
							}
							else{
								condition.Handle = outHandles[index];
								syncConditions.Add(outHandles[index], condition);
							}	//	end if ... else
						}
						else{
							if (outHandles[index] == 0){
								//	remove the conditon
								condition.Handle = 0;
								syncConditions.Remove(inHandles[index]);
							}
							else{
								//	normal situation for a update only action
							}	//	end if ... else
						}	//	end if ... else
						index++;
					}	//	end for
				}	//	end if
				
				return result;
			}
			catch(Exception e){
				Application.Instance.Trace(	
					EnumTraceLevel.ERR, 
					EnumTraceGroup.OPCSERVER, 
					"Application.ConditionsChanged", 
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	try ... catch

		}	//	end ConditionsChanged
		
		/// <summary>
		/// releases a condition from the OPC server's internal conditions list
		/// </summary>
		/// <param name="aConditionHandle"></param>
		/// <returns></returns>
		/// <include
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="ReleaseCondition"]/doc/*'
		/// />
		public int ReleaseCondition(uint aConditionHandle){
			try{
				Hashtable syncConditions = Hashtable.Synchronized(this.m_conditionList);
				syncConditions.Remove(aConditionHandle);
				return (int)EnumResultCode.S_OK;
			}
			catch(Exception e){
				Application.Instance.Trace(
					EnumTraceLevel.ERR, 
					EnumTraceGroup.OPCSERVER, 
					"Application.ReleaseCondition", 
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end try catch 

		}	//	end ReleaseCondition
		

		/// <summary>
		/// Fires the AE events provided by the parameter
		/// </summary>
		/// <param name="anEventList"></param>
		/// <returns></returns>
		/// <include
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="FireEvents"]/doc/*'
		/// />
		public int FireEvents(ArrayList anEventList){
			
			try{

				uint count = (uint)anEventList.Count;								
				if (count == 0){
					return (int)EnumResultCode.S_OK;
				}	//	end if
				
				IntPtr pEventData = Marshal.AllocCoTaskMem((int)count*Marshal.SizeOf(typeof(OTSEventData)));
				IntPtr pIndex = pEventData;
				
				OTSEventData[] eventData = new OTSEventData[count];
				
				int index = 0;
				foreach (AeEvent eventObject in anEventList){
					
					eventData[index] = eventObject.CreateOTSEventData();
					Marshal.StructureToPtr(eventData[index], pIndex, false);
					pIndex = new IntPtr(pIndex.ToInt64() + Marshal.SizeOf(typeof(OTSEventData)));
					index++;
				}	//	end foreach
				
				int result = OTBFunctions.OTSFireEvents(count, pEventData);				
				for(index = 0; index < count; index++){
					eventData[index].Release();
				}	//	end for
				Marshal.FreeCoTaskMem(pEventData);
				return result;
			}
			catch(Exception e){
				
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.FireEvents",
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end try ... catch

		}	//	end Fire

		/// <summary>
		/// Retrieves the current AE conditions list stored in the OPC server's application
		/// instance
		/// </summary>
		/// <include
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  property[@name="AeConditionList"]/doc/*'
		/// />
		public ICollection AeConditionList {
			get{
				Hashtable syncHashtable = Hashtable.Synchronized(this.m_conditionList);
				return syncHashtable.Values;
			}	//	end get
		}	//	end AeConditionList

		
		/// <summary>
		/// Message trace interface 
		/// </summary>
		/// <param name="aLevel"></param>
		/// <param name="aMask"></param>
		/// <param name="anObjectID"></param>
		/// <param name="aMessage"></param>
		/// <include
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="Trace"]/doc/*'
		/// />	
		public void Trace(EnumTraceLevel aLevel, EnumTraceGroup aMask, string anObjectID, string aMessage){			
			OPCToolbox.Trace.WriteLine((byte)aLevel, (uint)aMask, anObjectID, aMessage);
		}	//	end Trace
		

		/// <summary>
		/// Enables tracing for the entire application.
		/// </summary>		
		/// <param name="aDebugLevelMask">
		/// A trace filter at debug level.
		/// </param>
		/// <param name="anInfoLevelMask">
		/// A trace filter at informative level.
		/// </param>
		/// <param name="anWarningLevelMask">
		/// A trace filter at warning level.
		/// </param>
		/// <param name="anErrorLevelMask">
		/// A trace filter at error level.
		/// </param>
		/// <param name="aFileMaxSize">
		/// Maximum file size.Specifying -1 for maximum file size will determine tracing in a single file. The file in which 
		/// the tracing messages will be written is the file specified by the first parameter.
		/// </param>
		/// <param name="aFileName">
		/// A name of a file in which the tracing messages are to be written. When this parameter is empty
		/// the file tracing will be disabled
		/// </param>
		///<param name="maximumBackups">
		/// The maximum number of backups Trace files created. Every new application restart will stored the existing trace file 
		/// if any in backup. When this max umber is reached the older will be removed so that the new one is created.
		///</param>
		/// <include
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="EnableTracing"]/doc/*'
		/// />
		public void EnableTracing(	
			EnumTraceGroup anErrorLevelMask,
			EnumTraceGroup anWarningLevelMask,
			EnumTraceGroup anInfoLevelMask,
			EnumTraceGroup aDebugLevelMask,
			string aFileName,
			uint aFileMaxSize, 						
			uint maximumBackups){
			
			if (aFileName != string.Empty){
				OPCToolbox.Trace.EnableTraceToFile = true;
				OPCToolbox.Trace.MaximumBackups = maximumBackups;
				OPCToolbox.Trace.FileName = aFileName;				
			}
			else{
				OPCToolbox.Trace.EnableTraceToFile = false;
			}	//	end if ... else

			OPCToolbox.Trace.DebugLevelMask = (uint)aDebugLevelMask;
			OPCToolbox.Trace.ErrorLevelMask = (uint)anErrorLevelMask;
			OPCToolbox.Trace.InfoLevelMask = (uint)anInfoLevelMask;
			OPCToolbox.Trace.WarningLevelMask = (uint)anWarningLevelMask;
			OPCToolbox.Trace.FileMaxSize = aFileMaxSize;			

		}	//	end EnableTracing

		/// <summary>
		/// Adds the Softing Tunnel Protocol credential. 
		/// The Softing Tunnel functionality is not active in the commercial version of the OPC Toolbox.
		/// </summary>
		/// <param name="users"></param>
		/// <param name="passwords"></param>
		/// <returns></returns>
		/// <include
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="Application"]/
		///  method[@name="AddTpCredentials"]/doc/*'
		/// />
		public int AddTpCredentials(
			string[] users,
			string[] passwords)
		{
			if (users.Length != passwords.Length){
				return (int)EnumResultCode.E_INVALIDARG;
			}	//	end if

			m_initData.m_tpCredentialsNumber = users.Length;
			
			if (m_initData.m_tpCredentialsNumber > 0){

				m_initData.m_tpUsers = OTBFunctions.OTAllocateMemory(users.Length * Marshal.SizeOf(typeof(IntPtr)));
				m_initData.m_tpPasswords = OTBFunctions.OTAllocateMemory(passwords.Length * Marshal.SizeOf(typeof(IntPtr)));

				for (int index = 0; index < m_initData.m_tpCredentialsNumber; index++)
				{
					IntPtr userPointer = OTBFunctions.AllocateOTBString(users[index]);
					Marshal.WriteIntPtr(m_initData.m_tpUsers, 
						index * Marshal.SizeOf(typeof(IntPtr)),
						userPointer);

					IntPtr passwordPointer = OTBFunctions.AllocateOTBString(passwords[index]);
					Marshal.WriteIntPtr(m_initData.m_tpPasswords, 
						index * Marshal.SizeOf(typeof(IntPtr)),
						passwordPointer);
				}	//	end for
			}	//	end if

			return (int)EnumResultCode.S_OK;
		}	//	end AddTpCredentials

		//-
		#endregion
		
		#region //	Internal Members
		//--------------------------

		/// <summary>
		/// Get Client by the handle
		/// </summary>
		/// <param name="aSessionHandle"></param>
		/// <returns></returns>
		internal DaSession GetSession(IntPtr aSessionHandle){
			
			try{
				Hashtable syncSessionList = Hashtable.Synchronized(this.m_sessionList);
				return syncSessionList[aSessionHandle] as DaSession;
			}
			catch(Exception e){
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER, 
					"Application.GetSession", 
					"Exception caught:" + e.ToString());				
				return null;
			}	//	end try ... catch

		}	//	end GetSession
		
		internal void AddSession(DaSession aSession, IntPtr aSessionHandle){
			Hashtable syncSessionList = Hashtable.Synchronized(this.m_sessionList);
			syncSessionList.Add(aSessionHandle, aSession);
		}	//	end AddSession

		internal void RemoveSession(IntPtr aSessionHandle){
			Hashtable syncSessionList = Hashtable.Synchronized(this.m_sessionList);
			syncSessionList.Remove(aSessionHandle);
		}	//	end AddSession
		
		internal void AddTransaction(uint aKey, DaTransaction aTransaction){			
			Hashtable syncSessionList = Hashtable.Synchronized(this.m_sessionList);
			m_transactionList.Add(aKey, aTransaction);
		}	//	end AddTransaction

		internal DaTransaction FindTransaction(uint aKey){			
			return m_transactionList[aKey] as DaTransaction;
		}	//	end FindTransaction

		internal void ReleaseTransaction(uint aKey){
			
			DaTransaction transaction = FindTransaction(aKey);

			if (transaction != null && transaction.IsEmpty){				
				m_transactionList.Remove(aKey);
			}	//	end if

		}	//	end  ReleaseTransaction


		//-
		#endregion
		
		#region //	Private Constructor
		//-----------------------------
		
		/// <summary>
		/// private constructor in order to assure the singleton design
		/// </summary>
		private Application(){

			m_initData = new OTSInitData();

			m_initData.m_versionOTS			= 422;
			m_initData.m_appType			= 1;
			m_initData.m_clsidDA			= string.Empty;
			m_initData.m_progIdDA			= string.Empty;
			m_initData.m_verIndProgIdDA		= string.Empty;
			m_initData.m_clsidAE			= string.Empty;
			m_initData.m_progIdAE			= string.Empty;
			m_initData.m_verIndProgIdAE		= string.Empty;
			m_initData.m_description		= string.Empty;
			m_initData.m_ipPortHTTP			= 0;
			m_initData.m_urlDA				= string.Empty;
			m_initData.m_majorVersion		= 0;
			m_initData.m_minorVersion		= 0;
			m_initData.m_buildNumber		= 0;
			m_initData.m_vendorInfo			= string.Empty;;
			m_initData.m_minUpdateRateDA	= 100;
			m_initData.m_serviceName		= string.Empty;
			m_initData.m_addressSpaceDelimiter	= '.';
			m_initData.m_clientCheckPeriod	= 1000; 
			m_initData.m_propertyDelimiter	= '#';  
			m_initData.m_supportDisableConditions = 0;
			
			m_initData.m_webRootDirectory			= string.Empty;
			m_initData.m_webRootFile				= string.Empty;
			m_initData.m_webAdministratorPassword	= string.Empty;
			m_initData.m_webOperatorPassword		= string.Empty;
			
			m_initData.m_tpPort = 0;
			m_initData.m_tpCredentialsNumber = 0;
			m_initData.m_tpUsers = IntPtr.Zero;
			m_initData.m_tpPasswords = IntPtr.Zero;
			
			int result = Advise();

			if (ResultCode.FAILED(result)){
				
				Trace(EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.ctr", 
					"Callback advise failure");

				throw new ApplicationException("Callback advise failure");

			}	//	end if

		}	//	end constructor
		
		//-
		#endregion
		
		#region //	Private Members
		//---------------------------
		
		/// <summary>
		/// Terminates the application
		/// </summary>
		/// <returns>
		/// S_OK Everything was OK
		/// S_FALSE Everything was OK    
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>		
		private int HandleShutdown(){
			
			try{
				if (ShutdownRequest != null){
					return ShutdownRequest();	
				}	//	end if
				return (int)EnumResultCode.S_OK;
			}
			catch(Exception e){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.HandleShutdown", 
					e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end try ... catch

		}	//	end HandleShutdown
		
		/// <summary>
		/// HandleTrace
		/// </summary>
		private void HandleTrace(
			string aString,
			ushort aLevel,
			uint aMask, 
			string anObjId, 
			string aMessage){
			
			try{
				if (TraceOutput != null){
					TraceOutput(
						aString,
						(EnumTraceLevel)aLevel,
						(EnumTraceGroup)aMask,
						anObjId,
						aMessage);				
				}	//	end if
			}
			catch(Exception e){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.HandleTrace", 
					e.ToString());
			}	//	end try ... catch

		}	//	end HandleTrace

		/// <summary>
		/// HandleReadRequests
		/// </summary>
		/// <param name="aCount"></param>
		/// <param name="aRequestDataList"></param>
		/// <returns></returns>
		private int HandleReadRequests(
			int aCount, 
			OTSRequestData[] aRequestDataList){
			
			try{

				if (aCount == 0){
					return (int)EnumResultCode.E_INVALIDARG;
				}	//	end if

				DaRequest[] requests = new DaRequest[aCount];

				IntPtr sessionHandle = aRequestDataList[0].m_sessionHandle;

				for(int i = 0; i < aCount; i++){
					
					DaAddressSpaceElement element = DaAddressSpaceRoot.GetElementFromArray(aRequestDataList[i].m_object.m_userData) as DaAddressSpaceElement;
					
					requests[i] =  m_creator.CreateRequest(
						EnumTransactionType.READ,
						sessionHandle,
						element,
						aRequestDataList[i].m_propertyID,
						aRequestDataList[i].m_requestHandle);

					if (aRequestDataList[i].m_object.m_objectHandle != requests[i].AddressSpaceElement.ObjectHandle){
						//	this should not happen, it is an error and must be logged
						Application.Instance.Trace(EnumTraceLevel.ERR,EnumTraceGroup.OPCSERVER,
							"Application.HandleReadRequests", "Request object handle is different from the AddressSpaceElement object handle");
					}	//	end if
				}	//	end for

				DaTransaction transaction = m_creator.CreateTransaction(EnumTransactionType.READ, requests, sessionHandle);
				if (transaction == null){
					return (int)EnumResultCode.E_NOTIMPL;
				}	//	end if
				
				Instance.AddTransaction(transaction.Key, transaction);

				return transaction.HandleReadRequests();
			}
			catch(Exception e){
				Application.Instance.Trace(EnumTraceLevel.ERR,EnumTraceGroup.OPCSERVER,
					"Application.HandleReadRequests", e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end try ... catch

		}	//	end HandleReadRequests

		/// <summary>
		/// HandleWriteRequests
		/// </summary>
		/// <param name="aCount"></param>
		/// <param name="aRequestDataList"></param>
		/// <param name="aValues"></param>
		/// <returns></returns>
		private int HandleWriteRequests(
			int aCount,
			OTSRequestData[] aRequestDataList,
			OTValueData[] aValues){
			try{
			
				if (aCount == 0){
					return (int)EnumResultCode.E_INVALIDARG;
				}	//	end if

				DaRequest[] requests = new DaRequest[aCount];
				IntPtr sessionHandle = aRequestDataList[0].m_sessionHandle;

				for(int i=0; i < aCount; i++){
										
					DaAddressSpaceElement element = DaAddressSpaceRoot.GetElementFromArray(aRequestDataList[i].m_object.m_userData) as DaAddressSpaceElement;
					
					requests[i] =  m_creator.CreateRequest(
						EnumTransactionType.WRITE,
						sessionHandle,
						element,
						aRequestDataList[i].m_propertyID,
						aRequestDataList[i].m_requestHandle);

					requests[i].Value = new ValueQT(ref aValues[i]);
					
					if (aRequestDataList[i].m_object.m_objectHandle != requests[i].AddressSpaceElement.ObjectHandle){
						//	this should not happen, it is an error and must be logged
						Application.Instance.Trace(EnumTraceLevel.ERR,EnumTraceGroup.OPCSERVER,
							"Application.HandleWriteRequests", "Request object handle is different from the AddressSpaceElement object handle");
					}	//	end if

				}	//	end for
				
				DaTransaction transaction = m_creator.CreateTransaction(
					EnumTransactionType.WRITE, 
					requests,
					sessionHandle);

				if (transaction == null){
					return (int)EnumResultCode.E_NOTIMPL;
				}	//	end if
				
				Instance.AddTransaction(transaction.Key, transaction);				
				return transaction.HandleWriteRequests();
			}
			catch(Exception e){
				Application.Instance.Trace(EnumTraceLevel.ERR,EnumTraceGroup.OPCSERVER,
					"Application.HandleWriteRequests", e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end try ... catch
		
		}	//	end HandleWriteRequests
		
		/// <summary>
		/// HandleQueryProperties
		/// </summary>
		private int HandleQueryProperties(
			IntPtr pObjectData,
			string anItemId, 
			int aPropertyId, 
			out uint aPropertiesCount,
			out IntPtr pPropertiesList){

			aPropertiesCount = 0;
			pPropertiesList = IntPtr.Zero;
			
			try{
				int result = (int)EnumResultCode.E_NOTIMPL;
				ArrayList propertyList = null;
			
				if (pObjectData != IntPtr.Zero){
				
					OTObjectData objectData = (OTObjectData)Marshal.PtrToStructure(pObjectData, typeof(OTObjectData));
				
					if (pObjectData != IntPtr.Zero){
				
						DaAddressSpaceElement addressElement = 
							DaAddressSpaceRoot.GetElementFromArray(objectData.m_userData) as DaAddressSpaceElement;
					
						if (addressElement != null) {
							result = addressElement.QueryProperties(out propertyList);
						}	//	end if
					}	//	end if
				}	//	end if
				
				if (propertyList == null){
					//	try to get the property list using the item ID string
					result = DaAddressSpaceRoot.QueryAddressSpaceElementProperties(anItemId, out propertyList);
				}	//	end if
				
				if (aPropertyId > 0 && aPropertyId < 7) {
					return (int)EnumResultCode.S_FALSE;
				}	//	end if
				
				if (propertyList != null && propertyList.Count > 0){
					
					aPropertiesCount = (uint)propertyList.Count;

					OTSPropertyData data = new OTSPropertyData();
					pPropertiesList = OTBFunctions.OTAllocateMemory((int)aPropertiesCount * Marshal.SizeOf(typeof(OTSPropertyData)));

					IntPtr currentPtr = pPropertiesList;
					foreach (DaProperty property in propertyList){

						data.m_pid			= property.Id;
						data.m_name			= OTBFunctions.AllocateOTBString(property.Name);
						data.m_itemID		= OTBFunctions.AllocateOTBString(property.ItemId);
						data.m_descr		= OTBFunctions.AllocateOTBString(property.Description);
						data.m_datatype		= ValueQT.GetVartype(property.Datatype);
						
						data.m_accessRights	= (byte)property.AccessRights;							
					
						Marshal.StructureToPtr(data, currentPtr, false);
						currentPtr = new IntPtr(currentPtr.ToInt64() + Marshal.SizeOf(typeof(OTSPropertyData)));
					}	//	end foreach

					result = (int)EnumResultCode.S_OK;
			
				}	//	end if
				
				return result;
			}
			catch(Exception e){
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER, 
					"Application.HandleQueryProperties", 
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end catch

		}	//	end HandleQueryProperties
		
		/// <summary>
		/// HandleQueryAddressSpaceElementData
		/// </summary>
		private int HandleQueryAddressSpaceElementData(
			string aPath,
			byte anElementType,
			IntPtr aData){
			
			try{
				if (aData == IntPtr.Zero || aPath == string.Empty){
					return (int)EnumResultCode.E_INVALIDARG;
				}	//	end if

				EnumAddressSpaceElementType elementType = (EnumAddressSpaceElementType)anElementType;				
				AddressSpaceElement element = null;
				int result = (int)EnumResultCode.E_FAIL;

				switch(elementType){				
				
					case EnumAddressSpaceElementType.DA:
					
						result = DaAddressSpaceRoot.QueryAddressSpaceElementData(aPath, out element);
				
						if (ResultCode.SUCCEEDED(result)){
							DaAddressSpaceElement daElement = element as DaAddressSpaceElement;
						
							if (daElement != null){

								OTSAddressSpaceElementData aDataRet = new OTSAddressSpaceElementData();
								aDataRet.m_accessRights = (byte)daElement.AccessRights;
								aDataRet.m_datatype = ValueQT.GetVartype(daElement.Datatype) ;
								aDataRet.m_hasChildren = Convert.ToByte(daElement.HasChildren);
								aDataRet.m_isBrowsable = Convert.ToByte(daElement.IsBrowsable);
								aDataRet.m_ioMode = (byte)daElement.IoMode ;
								aDataRet.m_userData = daElement.UserData;								
								aDataRet.m_itemID = OTBFunctions.AllocateOTBString(daElement.ItemId);
								aDataRet.m_name = OTBFunctions.AllocateOTBString(daElement.Name);
							
								Marshal.StructureToPtr(aDataRet, aData, false);
							}	//	end if
						}	//	end if
						break;

					case EnumAddressSpaceElementType.AE:
						result = AeAddressSpaceRoot.QueryAddressSpaceElementData(aPath, out element);
				
						if (ResultCode.SUCCEEDED(result)){
						
							AeAddressSpaceElement aeElement = element as AeAddressSpaceElement;
						
							if (aeElement != null){

								OTSAddressSpaceElementData aDataRet = new OTSAddressSpaceElementData();
								aDataRet.m_name = OTBFunctions.AllocateOTBString(aeElement.Name);
								aDataRet.m_hasChildren = Convert.ToByte(aeElement.HasChildren);
								aDataRet.m_isBrowsable = Convert.ToByte(aeElement.IsBrowsable);
								aDataRet.m_userData = aeElement.UserData;
								Marshal.StructureToPtr(aDataRet, aData, false);
							}	//	end if
						}	//	end if
						break;

					default:
						Application.Instance.Trace(	EnumTraceLevel.ERR,
							EnumTraceGroup.OPCSERVER, 
							"Application.QueryAddressSpaceElementData", 
							"Invalid element type received" + anElementType.ToString());
						break;
				}	//	end switch

				return result;
			}
			catch(Exception e){
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER, 
					"Application.HandleQueryAddressSpaceElementData", 
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end catch
						
		}	//	end HandleQueryAddressSpaceElementData

		/// <summary>
		/// HandleQueryAddressSpaceElementChildren
		/// </summary>
		private int HandleQueryAddressSpaceElementChildren(			
			string aPath,
			byte anElementType,
			out uint pCount,
			out IntPtr aChildrenList){
			
			try{
				
				pCount = 0;
				aChildrenList = IntPtr.Zero;
				int res = (int)EnumResultCode.E_FAIL;
				EnumAddressSpaceElementType elementType = (EnumAddressSpaceElementType)anElementType;
				switch(elementType){

					case EnumAddressSpaceElementType.DA:

						ArrayList childrenDa = new ArrayList();
						res = (int)DaAddressSpaceRoot.QueryAddressSpaceElementChildren(aPath, childrenDa);
					
						if (ResultCode.SUCCEEDED(res)){
						
							pCount = (uint)childrenDa.Count;
							if (pCount > 0){
							
								OTSAddressSpaceElementData data = new OTSAddressSpaceElementData();
								aChildrenList = OTBFunctions.OTAllocateMemory((int)pCount * Marshal.SizeOf(typeof(OTSAddressSpaceElementData)));

								IntPtr currentPtr = aChildrenList;
								for (int i = 0; i < pCount; i++){
							
									DaAddressSpaceElement elementDA = childrenDa[i] as DaAddressSpaceElement;
									data.m_elementType = (byte)EnumAddressSpaceElementType.DA;
									data.m_accessRights = (byte)elementDA.AccessRights;
									data.m_datatype = ValueQT.GetVartype(elementDA.Datatype);
									data.m_hasChildren = Convert.ToByte(elementDA.HasChildren);
									data.m_isBrowsable = Convert.ToByte(elementDA.IsBrowsable);
									data.m_ioMode = (byte)elementDA.IoMode;														
									data.m_itemID = OTBFunctions.AllocateOTBString(elementDA.ItemId);
									data.m_name = OTBFunctions.AllocateOTBString(elementDA.Name);														
									data.m_userData = elementDA.UserData;
								
									Marshal.StructureToPtr(data, currentPtr, false);
									currentPtr = new IntPtr(currentPtr.ToInt64() + Marshal.SizeOf(typeof(OTSAddressSpaceElementData)));
								}	//	end for
							}	//	end if
						}	//	end if
						break;

					case EnumAddressSpaceElementType.AE:

						ArrayList childrenAe = new ArrayList();
						res = (int)AeAddressSpaceRoot.QueryAddressSpaceElementChildren(aPath, childrenAe);
					
						if (ResultCode.SUCCEEDED(res)){
						
							pCount = (uint)childrenAe.Count;
							if (pCount > 0){
							
								OTSAddressSpaceElementData data = new OTSAddressSpaceElementData();
								aChildrenList = OTBFunctions.OTAllocateMemory((int)pCount * Marshal.SizeOf(typeof(OTSAddressSpaceElementData)));

								IntPtr currentPtr = aChildrenList;
								for (int i = 0; i < pCount; i++){
									
									AeAddressSpaceElement elementAe = childrenAe[i] as AeAddressSpaceElement;
									data.m_elementType = (byte)EnumAddressSpaceElementType.AE;
									data.m_name = OTBFunctions.AllocateOTBString(elementAe.Name);
									data.m_hasChildren = Convert.ToByte(elementAe.HasChildren);
									data.m_isBrowsable = Convert.ToByte(elementAe.IsBrowsable);
									data.m_userData = elementAe.UserData;
									Marshal.StructureToPtr(data, currentPtr, false);
									currentPtr = new IntPtr(currentPtr.ToInt64() + Marshal.SizeOf(typeof(OTSAddressSpaceElementData)));
								}	//	end for
							}	//	end if
						}	//	end if
						break;
				}	//	end switch
							
				return res;
			}	//	end try
			catch(Exception e){
				
				pCount = 0;
				aChildrenList = IntPtr.Zero;
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER, 
					"Application.HandleQueryAddressSpaceElementChildren", 
					"Exception caught:" + e.ToString());
				
				return (int)EnumResultCode.E_FAIL;
			}	//	end catch

		}	//	end HandleQueryAddressSpaceElementChildren
		
		/// <summary>
		/// HandleCreateAddressSpaceElement
		/// </summary>
		private int HandleCreateAddressSpaceElement(
			IntPtr hParent, 
			IntPtr hObject, 
			OTSAddressSpaceElementData aDataIn, 
			IntPtr aDataOut){
			
			try{
				if (aDataIn == null || aDataOut == IntPtr.Zero){
					return (int)EnumResultCode.E_INVALIDARG;
				}	//	end if
					
				EnumAddressSpaceElementType elementType = (EnumAddressSpaceElementType)aDataIn.m_elementType;
				string name = Marshal.PtrToStringUni(aDataIn.m_name);
				
				switch (elementType){

					case EnumAddressSpaceElementType.DA:
						
						string itemId = Marshal.PtrToStringUni(aDataIn.m_itemID);						
						DaAddressSpaceElement daElem  = null;
						
						//	Check if the user data was already added
						daElem = DaAddressSpaceRoot.GetElementFromArray(aDataIn.m_userData) as DaAddressSpaceElement;
						if (daElem != null){
							daElem.ItemId = itemId;
							aDataIn.m_itemID = IntPtr.Zero;
							aDataIn.m_name = IntPtr.Zero;
							aDataIn.m_userData = daElem.UserData;
							aDataIn.m_ioMode = (byte)daElem.IoMode;

							Marshal.StructureToPtr(aDataIn, aDataOut, false);

							return (int)EnumResultCode.S_OK;
						}	//	end if

						daElem = Creator.CreateInternalDaAddressSpaceElement(itemId, name, aDataIn.m_userData, hObject, hParent);						
						
						daElem.HasChildren = (aDataIn.m_hasChildren != 0);
						daElem.IsBrowsable = (aDataIn.m_isBrowsable != 0);
						daElem.AccessRights = (EnumAccessRights)aDataIn.m_accessRights;
						daElem.Datatype = ValueQT.GetSysType(aDataIn.m_datatype) ;						
						daElem.IoMode = (EnumIoMode)aDataIn.m_ioMode;
												
						DaAddressSpaceElement daParent = DaAddressSpaceRoot.GetParent(hObject) as DaAddressSpaceElement;
						if (daParent != null){
						
							daParent.AddChild(daElem);							
							aDataIn.m_itemID = IntPtr.Zero;
							aDataIn.m_name = IntPtr.Zero;
							aDataIn.m_userData = daElem.UserData;
							
							Marshal.StructureToPtr(aDataIn, aDataOut, false);
							return (int)EnumResultCode.S_OK;
						}
						else{
							return (int)EnumResultCode.E_FAIL;
						}	//	end if ... else

						
					case EnumAddressSpaceElementType.AE:
						
						AeAddressSpaceElement aeElem = null;

						//	Check if the user data was already added
						aeElem = AeAddressSpaceRoot.GetElementFromArray(aDataIn.m_userData) as AeAddressSpaceElement;
						if (aeElem != null){
							aDataIn.m_itemID = IntPtr.Zero;
							aDataIn.m_name = IntPtr.Zero;
							aDataIn.m_userData = aeElem.UserData;							
							Marshal.StructureToPtr(aDataIn, aDataOut, false);
							
							return (int)EnumResultCode.S_OK;
						}	//	end if
						
						aeElem = Creator.CreateInternalAeAddressSpaceElement(name, aDataIn.m_userData, hObject, hParent);
						aeElem.HasChildren = (aDataIn.m_hasChildren != 0);
						aeElem.IsBrowsable = (aDataIn.m_isBrowsable != 0);

						AeAddressSpaceElement aeParent = AeAddressSpaceRoot.GetParent(hObject) as AeAddressSpaceElement;
						if (aeParent != null){

							aeParent.AddChild(aeElem);
							aDataIn.m_itemID = IntPtr.Zero;
							aDataIn.m_name = IntPtr.Zero;
							aDataIn.m_userData = aeElem.UserData;
							
							Marshal.StructureToPtr(aDataIn, aDataOut, false);
							return (int)EnumResultCode.S_OK;
						}
						else{
							return (int)EnumResultCode.E_FAIL;
						}	//	end if ... else

					default:

						return (int)EnumResultCode.E_FAIL;
				}	//	end switch
			}
			catch(Exception e){
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER, 
					"Application.HandleCreateAddressSpaceElement", 
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	enc try ... catch
			
		}	//	end HandleCreateAddressSpaceElement
		

		/// <summary>
		/// HandleDestroyAddressSpaceElement
		/// </summary>
		private int HandleDestroyAddressSpaceElement(OTObjectData anObjectData){
			
			try{
				AddressSpaceElement element = DaAddressSpaceRoot.GetElementFromArray(anObjectData.m_userData) as AddressSpaceElement;
			
				if (element != null){
					DaAddressSpaceRoot.RemoveElementFromArray(element);					
					element.TriggerRemovedFromAddressSpace();
					return (int)EnumResultCode.S_OK;
				}	//	end if

				element = AeAddressSpaceRoot.GetElementFromArray(anObjectData.m_userData) as AddressSpaceElement;			
				if (element != null){
					AeAddressSpaceRoot.RemoveElementFromArray(element);					
					element.TriggerRemovedFromAddressSpace();
					return (int)EnumResultCode.S_OK;

				}	//	end if
			
				return (int)EnumResultCode.S_FALSE;
			}
			catch(Exception e){
				Application.Instance.Trace(EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER, 
					"Application.DestroyAddressSpaceElement", 
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end try ... catch

		}	//	end HandleDestroyAddressSpaceElement
		
		/// <summary>
		/// HandleChangeSessionState
		/// </summary>
		private int HandleChangeSessionState(
			IntPtr aSessionHandle, 
			OTSSessionData aSessionStateData){
			
			try{

				if (aSessionHandle == IntPtr.Zero || aSessionStateData == null){
					return (int)EnumResultCode.E_INVALIDARG;
				}	//	end if
				
				DaSession session = null;

				switch ((EnumSessionState)aSessionStateData.m_state){

					case EnumSessionState.CREATE:
						session = Creator.CreateSession((EnumSessionType)aSessionStateData.m_type, aSessionHandle);
						if (session != null){
							AddSession(session, aSessionHandle);
							session.ConnectSession();
							return (int)EnumResultCode.S_OK;
						}
						break;
					
					case EnumSessionState.DESTROY:					
						session = GetSession(aSessionHandle);
						if (session != null){							
							session.DisconnectSession();
							RemoveSession(aSessionHandle);
							return (int)EnumResultCode.S_OK;
						}
						break;
					
					case EnumSessionState.LOGOFF:
						session = GetSession(aSessionHandle);
						if (session != null){
							return session.Logoff();
						}
						break;
					
					case EnumSessionState.LOGON:
						session = GetSession(aSessionHandle);
						if (session != null){
							return session.Logon(aSessionStateData.m_userName, aSessionStateData.m_password);
						}
						break;
				}	//	end switch
				
				return (int)EnumResultCode.E_UNEXPECTED;
			}
			catch(Exception e){

				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER, 
					"Application.HandleChangeSessionState", 
					"Exception caught:" + e.ToString());

				return (int)EnumResultCode.E_FAIL;				
			}	//	end try ... catch

		}	//	end HandleChangeSessionState
		
		
		/// <summary>
		/// HandleQueryCacheValue
		/// </summary>
		private int HandleQueryCacheValue(
			IntPtr aSessionHandle,
			OTObjectData anObjectData,
			IntPtr pOutValue){

			try{
				
				DaAddressSpaceElement element = DaAddressSpaceRoot.GetElementFromArray(anObjectData.m_userData) as DaAddressSpaceElement;
				DaSession session = this.GetSession(aSessionHandle);

				ValueQT cacheValue = null;
				int result = element.QueryCacheValue(session, ref cacheValue);
				
				if (!ResultCode.SUCCEEDED(result) ){
					return result;
				}	//	end if
				
				if (cacheValue == null){
					return (int)EnumResultCode.E_FAIL;
				}	//	end if

				OTValueData valueData = (OTValueData)Marshal.PtrToStructure(pOutValue, typeof(OTValueData));
				valueData.m_quality = (ushort)cacheValue.Quality;			
				valueData.m_timestamp = new OTDateTime(cacheValue.TimeStamp);				
				Marshal.GetNativeVariantForObject(cacheValue.Data, valueData.m_value);				
				Marshal.StructureToPtr(valueData, pOutValue, false);

				return (int)EnumResultCode.S_OK;
			}
			catch(Exception e){

				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.HandleQueryCacheValue",
					"Exception caught:" + e.ToString());

				return (int)EnumResultCode.E_FAIL;
			}	//	end try ... catch

		}	//	end HandleQueryCacheValue
		
		/// <summary>
		/// HandleChangeItems
		/// </summary>
		private int HandleChangeItems(uint aCount, IntPtr anItemDataPointer){
			
			try{

				if ((anItemDataPointer == IntPtr.Zero) || (aCount == 0))
				{
					return (int)EnumResultCode.S_FALSE;
				}	//	end if

				bool errorFound = false;
				DaAddressSpaceRoot root = Application.Instance.DaAddressSpaceRoot;
				DaAddressSpaceElement[] elements = new DaAddressSpaceElement[aCount];
				
				OTSItemData itemData = new OTSItemData();
				int structSize = Marshal.SizeOf(itemData);

				for (int i = 0; i < aCount; i++)
				{
					itemData = (OTSItemData)Marshal.PtrToStructure((IntPtr)((int)anItemDataPointer + (structSize * i)), typeof(OTSItemData));
					DaAddressSpaceElement element = (DaAddressSpaceElement)root.GetElementFromArray(itemData.m_object.m_userData);

					if (element != null)
					{
						element.Change(itemData.m_active != 0, itemData.m_sampleRate);
						elements[i] = element;
					}
					else
					{
						errorFound = true;
					}	//	end if...else
				}	//	end for

				root.ChangeItems(aCount, anItemDataPointer);
				return (errorFound ? (int)EnumResultCode.E_FAIL  : (int)EnumResultCode.S_OK);

			}
			catch(Exception e){				
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.HandleChangeItems",
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end catch

		}	//	end HandleChangeItems


		/// <summary>
		/// HandleQueryConditions
		/// </summary>
		private int HandleQueryConditions(
			IntPtr pObjectData,
			string aSourcePath,
			out uint aConditionCount,
			out IntPtr pConditionNames){

			aConditionCount = 0;
			pConditionNames = IntPtr.Zero;

			try{
				
				int result = (int)EnumResultCode.E_FAIL;

				if (pObjectData != IntPtr.Zero){
					
					OTObjectData objectData = (OTObjectData)Marshal.PtrToStructure(pObjectData, typeof(OTObjectData));
				
					string [] conditionNames = null;

					result = AeAddressSpaceRoot.QueryConditions(
						objectData.m_userData,
						aSourcePath, 
						out conditionNames);
				
					if (conditionNames != null){
						aConditionCount = (uint)conditionNames.Length;
					}	//	end if

					if (aConditionCount != 0){
					
						pConditionNames = OTBFunctions.OTAllocateMemory((int)aConditionCount*Marshal.SizeOf(typeof(IntPtr)));
						for(int index = 0; index < aConditionCount; index++){

							//	Allocate the string into the unmanaged memory
							IntPtr conditionName = OTBFunctions.AllocateOTBString(conditionNames[index]);
							Marshal.WriteIntPtr(pConditionNames, index*Marshal.SizeOf(typeof(IntPtr)), conditionName);
						}	//	end for
					}	//	end if 
				}
				return result;
			}
			catch(Exception e){

				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.HandleQueryConditions",
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end try ... catch

		}	//	end HandleQueryConditions
		

		/// <summary>
		/// HandleAcknowledgeCondition
		/// </summary>
		private int HandleAcknowledgeCondition(
			OTObjectData pConditionData,
			string anAckId,
			string anAckComment){
			
			try{
				
				Hashtable syncConditions = Hashtable.Synchronized(this.m_conditionList);
				AeCondition condition = syncConditions[(uint)pConditionData.m_objectHandle] as AeCondition;					
				if (condition != null){
					condition.Acknowledge(anAckId, anAckComment, DateTime.Now);
					return ChangeConditions(new AeCondition[]{condition});
				}	//	end if
				
				return (int)EnumResultCode.E_FAIL;
			}
			catch(Exception e){

				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.AcknowledgeCondition",
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;

			}	//	end try ... catch

		}	//	AcknowledgeCondition
		

		/// <summary>
		/// HandleQueryConditionDefinition
		/// </summary>
		private int HandleQueryConditionDefinition(
			OTObjectData pConditionData,
			IntPtr aConditionDefinitionData){
			
			try{
								
				Hashtable syncConditions = Hashtable.Synchronized(this.m_conditionList);
				AeCondition condition = syncConditions[(uint)pConditionData.m_objectHandle] as AeCondition;
				
				if (condition == null){
					return (int)EnumResultCode.E_FAIL;
				}	//	end if

				AeConditionDefinition conditionDefinition = condition.Definition;

				//	check if condition definition found
				if (conditionDefinition != null){
					
					OTSConditionDefinitionData data = conditionDefinition.CreateOTSConditionDefinitionData();
					
					//	copy the structure's to the the in/out parameter
					//	NOTE: the caller is responsible for releasing the memory allocated within this method
					Marshal.StructureToPtr(data, aConditionDefinitionData, false);
					return (int)EnumResultCode.S_OK;
				}	//	end if
				return (int)EnumResultCode.S_FALSE;
			}
			catch(Exception e){
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.HandleQueryConditionDefinition",
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end try ... catch

		}	//	end HandleQueryConditionDefinition
		

		/// <summary>
		/// HandleEnableConditions
		/// </summary>
		private int HandleEnableConditions(
			byte isEnable,
			string anAddressSpaceElementPath){
			
			try{

				Hashtable syncConditios = Hashtable.Synchronized(m_conditionList);
				int result = (int)EnumResultCode.S_OK;
				foreach (AeCondition condition in syncConditios.Values){
					
					if (condition.SourcePath == anAddressSpaceElementPath){
						if (isEnable != 0){
							result |= condition.Enable(anAddressSpaceElementPath);
						}
						else{
							result |= condition.Disable(anAddressSpaceElementPath);
						}	//	end if ... else
					}	//	end if
				}	//	end foreach

				return result;
			}
			catch(Exception e){

				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.HandleEnableConditions",
					"Exception caught:" + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end try ... catch

		}	//	end HandleEnableConditions
		

		/// <summary>
		/// HandleWebTemplate
		/// </summary>
		private int HandleWebTemplate(
			string aTemplateName,
			uint aNumArgs,
			string[] pArgs,
			out IntPtr pResult){
			pResult = IntPtr.Zero;

			try{
				
				string resultString;
				int result = m_webTemplate.HandleWebTemplate(aTemplateName, aNumArgs, pArgs, out resultString);
				
				if (ResultCode.SUCCEEDED(result) && resultString != string.Empty){					
					pResult = OTB.OTBFunctions.AllocateOTBString(resultString);					
				}	//	end if

				return (int)result;
			}
			catch(Exception e){
				
				Application.Instance.Trace(	EnumTraceLevel.ERR,
					EnumTraceGroup.OPCSERVER,
					"Application.HandleWebTemplate",
					"Exception caught:" + e.ToString());
				pResult = IntPtr.Zero;
				return (int)EnumResultCode.E_FAIL;	
			}	//	end try ... catch

		}	//	end HandleWebTemplate
		
		//-
		#endregion
		
		#region //	Private Attributes
		//----------------------------
		
		private OTSCallbackFunctions m_OTNSCallBacks = new OTSCallbackFunctions();
		
		private OTSInitData m_initData = new OTSInitData();
		
		private DaAddressSpaceRoot m_DaAddressSpaceRoot = null; 
		
		private AeAddressSpaceRoot m_AeAddressSpaceRoot = null; 
		
		private Creator m_creator = null;
		
		private Hashtable m_sessionList = new Hashtable();
		
		private Hashtable m_transactionList = new Hashtable();

		private Hashtable m_categoryList = new Hashtable();
		
		private Hashtable m_conditionList = new Hashtable();
		
		private WebTemplate m_webTemplate = null;

		//-
		#endregion

	}	//	end class Application

}	//	end namespace Softing.OPCToolbox.Server 

