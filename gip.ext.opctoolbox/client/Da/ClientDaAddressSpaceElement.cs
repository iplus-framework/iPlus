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
//  Filename    : ClientDaAddressSpaceElement.cs                              |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : Address space element in a DA Server address space.         |
//                                                                            |
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using Softing.OPCToolbox.Client;
using Softing.OPCToolbox.OTB;

namespace Softing.OPCToolbox.Client{

	/// <summary>
	/// Represents an element in an OPC <b>Data Access Server</b>`s address space.
	/// </summary>
	/// <include 
	///  file='TBNC.doc.xml' 
	///  path='//class[@name="DaAddressSpaceElement"]/doc/*'
	/// />
	public class DaAddressSpaceElement : AddressSpaceElement {
		
		#region //	Protected Attributes
		//------------------------------

		/// <summary>
		/// A second identifier for the element in the Data Access Server`s address space.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElement"]/field[@name="m_itemPath"]/doc/*'
		/// />		
		protected string m_itemPath = null;

		/// <summary>
		/// The <see cref="DaAddressSpaceElement"/> object to whom the element belongs.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElement"]/field[@name="m_session"]/doc/*'
		/// />
		protected DaSession m_session = null;		

		//-
		#endregion
		
		#region	//	Public Constructors
		//-----------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="DaAddressSpaceElement">DaAddressSpaceElement</see> class.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElement"]/constructor[@name="DaAddressSpaceElement"]/doc/*'
		/// />
		public DaAddressSpaceElement() 
		{			
			m_itemPath = String.Empty;			
		}

		internal DaAddressSpaceElement(
			string name,
			string itemId,
			string itemPath,
			IntPtr objectHandle,
			DaSession session)
		{
			m_name = name;
			m_qName = itemId;
			m_itemPath = itemPath;
			m_objectElementHandle = objectHandle;
			m_session = session;
		}

		//-
		#endregion
		
		#region	//	Public Methods
		//---------------------------------
		/// <summary>
		/// Browses the Data Access Server`s address space starting from the element`s position in the address space.
		/// </summary>
		/// <param name="browseOptions">Specifies the options of the browsing.</param>
		/// <param name="addressSpaceElements">A list with the element`s children.</param>
		/// <param name="executionOptions">Specifies the modality of execution for browsing the server's address space.</param>
		/// <returns>The result of browsing the address space.</returns>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElement"]/method[@name="Browse"]/doc/*'
		/// />
		public int Browse(
			DaAddressSpaceElementBrowseOptions browseOptions, 
			out DaAddressSpaceElement[] addressSpaceElements,
			ExecutionOptions executionOptions)
		{	
			addressSpaceElements = null;
			OTCAddressSpaceBrowseOptions OTCBrowseOptions = new OTCAddressSpaceBrowseOptions();
            OTCBrowseOptions.m_accessRightsFilter = 0;
			int res = (int)EnumResultCode.E_FAIL;
			
			if(this.Session == null){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"DaAddressSpaceElement.Browse", 
					"The Session property of the AddressSpaceElement cannot be null! Set the property to a value before calling Browse!");
				return res;
			}
			try{
				OTCExecutionOptions options = new OTCExecutionOptions();

				if(executionOptions != null){
						options.m_executionType = (byte)executionOptions.ExecutionType;
						options.m_executionContext = (uint)executionOptions.ExecutionContext;
				}
				else{
					options.m_executionType = (byte)EnumExecutionType.SYNCHRONOUS;
					options.m_executionContext = 0;
				}
				
				IntPtr pOTCaddressSpaceElements = new IntPtr();
				uint addressSpaceElementDataCount = 0;
				
				OTCBrowseOptions.m_type = (byte)browseOptions.ElementTypeFilter;
				OTCBrowseOptions.m_elementNameFilter = Marshal.StringToCoTaskMemUni(browseOptions.ElementNameFilter);
				OTCBrowseOptions.m_vendorFilter = Marshal.StringToCoTaskMemUni(browseOptions.VendorFilter);
				OTCBrowseOptions.m_accessRightsFilter = (uint)browseOptions.AccessRightsFilter;               
				OTCBrowseOptions.m_dataTypeFilter = System.Convert.ToUInt16(browseOptions.DataTypeFilter);
				OTCBrowseOptions.m_maxElements = browseOptions.MaxElements;
				OTCBrowseOptions.m_retrieveItemID = (byte)(browseOptions.RetrieveItemId ? 1:0);
				OTCBrowseOptions.m_retrieveProperties = (byte)(browseOptions.ReturnProperties ? 1:0);
				OTCBrowseOptions.m_retrievePropertyValues = (byte)(browseOptions.ReturnPropertyValues ? 1:0);
			
				res = OTBFunctions.OTCBrowseAddressSpace(
					this.Session.Handle,
					this.m_objectElementHandle,
					this.ItemId,
					this.m_itemPath,
					ref OTCBrowseOptions,
					ref addressSpaceElementDataCount,
					out pOTCaddressSpaceElements,
					ref options);

				addressSpaceElements = new DaAddressSpaceElement[addressSpaceElementDataCount];
				
				if (options.m_executionType == (byte)EnumExecutionType.SYNCHRONOUS){
					if(ResultCode.SUCCEEDED(res)){

						OTCAddressSpaceElementData typeOfAddressSpaceElement = new OTCAddressSpaceElementData();	
						for (int i = 0; i< addressSpaceElementDataCount; i++){
					
							int structSize = Marshal.SizeOf(typeOfAddressSpaceElement);
							OTCAddressSpaceElementData myData = (OTCAddressSpaceElementData)Marshal.PtrToStructure((IntPtr)((int)pOTCaddressSpaceElements+(structSize*i)),typeof(OTCAddressSpaceElementData));
							addressSpaceElements[i] = new DaAddressSpaceElement(
								Marshal.PtrToStringUni(myData.m_name),
								Marshal.PtrToStringUni(myData.m_itemID),
								Marshal.PtrToStringUni(myData.m_itemPath),
								myData.m_objectHandle,
								this.Session);
							
							OTBFunctions.OTFreeMemory(myData.m_itemID);
						
							OTBFunctions.OTFreeMemory(myData.m_name);
						
							OTBFunctions.OTFreeMemory(myData.m_itemPath);
						}

						if(pOTCaddressSpaceElements != IntPtr.Zero){
							OTBFunctions.OTFreeMemory(pOTCaddressSpaceElements);
						}
					}
					else{
						Application.Instance.Trace(
							EnumTraceLevel.ERR,
							EnumTraceGroup.CLIENT,
							"DaAddressSpaceElement.Browse", 
							"Browsing failed! Result: " + res);
					}
				}//if executionOptions Synchronous
				
				Marshal.FreeCoTaskMem(OTCBrowseOptions.m_elementNameFilter);
				
				Marshal.FreeCoTaskMem(OTCBrowseOptions.m_vendorFilter);
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"DaAddressSpaceElement.Browse", 
					exc.ToString());
			
			}
			return res;
		}

		
		/// <summary>
		/// Retrieves the element`s property list based on some filter criteria.
		/// </summary>
		/// <param name="aDaGetPropertiesOptions">The filter criteria for retrieving the element's properties.</param>
		/// <param name="someDaProperties">A list with the property data for the current element.</param>
		/// <param name="executionOptions">Specifies the modality of execution for getting the element's properties.</param>
		///<returns>The result of retrieving the element's properties.</returns>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElement"]/method[@name="GetDaProperties_1"]/doc/*'
		/// />		
		public virtual int GetDaProperties(
			DaGetPropertiesOptions aDaGetPropertiesOptions, 
			out DaProperty[] someDaProperties,
			ExecutionOptions executionOptions)
		{
			int res = (int)EnumResultCode.E_FAIL;	
			try{
				res = this.Session.GetDaProperties(
					this.ItemId, 
					this.ItemPath,
					aDaGetPropertiesOptions,
					out someDaProperties,
					executionOptions);

				if (ResultCode.FAILED(res)){
					Application.Instance.Trace(
						EnumTraceLevel.ERR,
						EnumTraceGroup.CLIENT,
						"DaAddressSpaceElement.GetDaProperties", 
						"Getting properties failed! Result: " + res);
				}
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"DaAddressSpaceElement.GetDaProperties", 
					exc.ToString());

				someDaProperties = null;
			}
			return res;
		}	//	end GetProperties
		
		/// <summary>
		/// Retrieves the element`s property list.
		/// </summary>
		/// <param name="someDaProperties">A list with the property data for the current element.</param>
		/// <param name="executionOptions">Specifies the modality of execution for getting the element's properties.</param>
		///<returns>The result of retrieving the element's properties.</returns>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElement"]/method[@name="GetDaProperties_2"]/doc/*'
		/// />	
		public virtual int GetDaProperties( out DaProperty[] someDaProperties, ExecutionOptions executionOptions)
		{
			int res = (int)EnumResultCode.E_FAIL;			
			try{
				res = this.Session.GetDaProperties(
					this,
					this.ItemId, 
					this.ItemPath, 
					null, 
					out someDaProperties,
					executionOptions);

				if (ResultCode.FAILED(res)){
					Application.Instance.Trace(
						EnumTraceLevel.ERR,
						EnumTraceGroup.CLIENT,
						"DaAddressSpaceElement.GetDaProperties", 
						"Getting properties failed! Result: " + res);
				}
			}
			catch(Exception exc){
				Application.Instance.Trace(
					EnumTraceLevel.ERR,
					EnumTraceGroup.CLIENT,
					"DaAddressSpaceElement.GetDaProperties", 
					exc.ToString());
	
				someDaProperties = null;
			}
			return res;
		}	//	end GetDaProperties
		//-
		#endregion

		#region	//	Public Properties
		//---------------------------
		
		
		/// <summary>
		/// Gets or sets an unique identifier for the element in the Data Access Server's address space.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElement"]/property[@name="ItemId"]/doc/*'
		/// />
		public string ItemId
		{
			get
			{
				return m_qName;
			}
			set
			{
				m_qName = value;
			}
		}


		/// <summary>
		/// Gets or sets a second identifier for the element in the Data Access Server`s address space.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElement"]/property[@name="ItemPath"]/doc/*'
		/// />
		public string ItemPath
		{
			get
			{
				return m_itemPath;
			}
			set
			{
				m_itemPath = value;
			}
		}


		/// <summary>
		/// Get or sets the <see cref="Softing.OPCToolbox.Client.DaSession"/> object to whom the element belongs.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElement"]/property[@name="Session"]/doc/*'
		/// />
		public DaSession Session
		{
			get
			{
				return m_session;
			}
			set
			{
				m_session = value;
			}
		}

		//-
		#endregion
	
	}	//	end DaAddressSpaceElement

}	//	end namespace Softing.OPCToolbox.Client
