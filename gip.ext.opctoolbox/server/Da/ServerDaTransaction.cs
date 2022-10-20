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
//  Filename    : ServerDaTransaction.cs                                      |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : OPC DA transaction handling class                           |
//                                                                            |
//-----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Runtime.InteropServices;
using Softing.OPCToolbox.OTB;

namespace Softing.OPCToolbox.Server{

	/// <summary>
	/// Transaction class: Used for managing the transaction collection requests
	/// </summary>
	/// <include 
	///  file='TBNS.doc.xml' 
	///  path='//class[@name="DaTransaction"]/doc/*'
	/// />
	public class DaTransaction{
		
		#region //	Constructors
		//----------------------

		/// <summary>
		/// Default Constructor.
		/// </summary>
		/// <param name="aTransactionType">Client wants to read or to write</param>
		/// <param name="aRequestList">Array of requests contained in the transaction</param>
		/// <param name="aSessionHandle">The session's key</param>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  ctor[@name="DaTransaction"]/doc/*'
		/// />
		public DaTransaction(
			EnumTransactionType aTransactionType, 
			DaRequest[] aRequestList, 
			IntPtr aSessionHandle){
			
			this.m_type = aTransactionType;						
			this.m_requestList.AddRange(aRequestList);

			this.m_sessionHandle = aSessionHandle;
			this.m_key = KeyBuilder++;
			
			foreach(DaRequest request in aRequestList){
				request.TransactionKey = this.m_key;
			}	//	end foreach

		}	//	end public constructor
				
		//-
		#endregion
		
		#region	//	Public Virtual Methods
		//--------------------------------

		/// <summary>
		/// Called if a read transaction occurs
		/// </summary>
		/// <returns>
		/// EnumResultCode.E_NOTIMPL. Must be overridden
		///	</returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  method[@name="HandleReadRequests"]/doc/*'
		/// />
		virtual public int HandleReadRequests(){
			return (int)EnumResultCode.E_NOTIMPL;
		}	//	end HandleReadRequests

		/// <summary>
		/// Called if a write transaction occurs 
		/// </summary>
		/// <returns>
		/// EnumResultCode.E_NOTIMPL. Must be overridden
		///	</returns>	
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  method[@name="HandleWriteRequests"]/doc/*'
		/// />
		virtual public int HandleWriteRequests(){
			return (int)EnumResultCode.E_NOTIMPL;
		}	//	end HandleWriteRequests
		
		//-
		#endregion
		
		#region	//	Protected Attributes
		//----------------------------------
		
		/// <summary>
		/// incremented with each transaction created
		/// </summary>
		private static uint KeyBuilder = 1;

		/// <summary>
		/// Requests contained in this transaction
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  attribute[@name="m_requestList"]/doc/*'
		/// />
		protected ArrayList m_requestList = new ArrayList();
		
		/// <summary>
		/// Determines whether this is a read or a write Transaction
		/// </summary>
		private readonly EnumTransactionType m_type = EnumTransactionType.READ;
		
		/// <summary>
		///	the session's Handle
		/// </summary>
		private readonly IntPtr m_sessionHandle = IntPtr.Zero;
		
		/// <summary>
		/// the transaction unique key
		/// </summary>
		private readonly uint m_key = 0;
		
		//-
		#endregion
		
		#region //	Public Properties
		//---------------------------
		
		/// <summary>
		/// the transaction associated Key
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  properties[@name="Key"]/doc/*'
		/// />
		public uint Key{
			get{
				return m_key;
			}	//	end get
		}	//	end Key

		/// <summary>
		/// Determines whether this is a read or a write Transaction
		/// </summary>	
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  properties[@name="Type"]/doc/*'
		/// />		
		public EnumTransactionType Type{
			get	{
				return m_type;
			}	//	end get
		}	//	end Type

		/// <summary>
		/// Requests contained in this transaction
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  properties[@name="Requests"]/doc/*'
		/// />
		public ArrayList Requests{
			get	{
				return m_requestList;
			}	//	end get
		}	//	end Requests
		
		/// <summary>
		/// Is transaction empty?
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  properties[@name="IsEmpty"]/doc/*'
		/// />
		public bool IsEmpty{
			get{
				return (m_requestList.Count == 0);
			}	//	end get
		}	//	end IsEmpty
		
		/// <summary>
		/// Retrieves the session object owning this transaction
		/// </summary>
		/// <returns>
		/// null - Some error occurred
		/// DaSession the session that generated this transaction
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  properties[@name="Session"]/doc/*'
		/// />		
		public DaSession Session{
			get{
				if (m_sessionHandle != IntPtr.Zero){
					return (DaSession)Application.Instance.GetSession(m_sessionHandle);
				}	//	end if
				else{
					return null;
				}	//	end if ... else
			}	//	end get
		}	//	end Session
		
		//-
		#endregion
				
		#region //	Public Methods
		//------------------------

		/// <summary>
		/// Completes all requests must be called while or after "handling" requests
		/// </summary>
		/// <returns>
		/// S_OK - Everything was OK
		/// S_FALSE - Everything was OK    
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>		
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  method[@name="CompleteRequests"]/doc/*'
		/// />		
		public int CompleteRequests(){
			
			ArrayList syncArrayList = ArrayList.Synchronized(m_requestList);
			int count = syncArrayList.Count;
			
			if (count == 0){
				return (int)EnumResultCode.S_FALSE;
			}	//	end if

			OTValueData[] values = new OTValueData[count];
			OTSRequestData[] requests = new OTSRequestData[count];
			int[] results = new int[count];
			
			for(int i = 0; i < count; i++){
				
				DaRequest request =  syncArrayList[i] as DaRequest;
				if (request != null){

					requests[i].m_sessionHandle = request.SessionHandle;
					requests[i].m_propertyID    = request.PropertyId; 
					requests[i].m_requestHandle = request.RequestHandle;
					
					if (request.AddressSpaceElement != null){
						requests[i].m_object.m_userData		= (uint)request.AddressSpaceElement.UserData;
						requests[i].m_object.m_objectHandle	= request.AddressSpaceElement.ObjectHandle;
					}
					else{
						requests[i].m_object.m_userData		= 0;
						requests[i].m_object.m_objectHandle = IntPtr.Zero;
					}	//	end if ... else

					values[i] = new OTValueData();
					if	(request.Value != null){
						values[i].m_timestamp = new OTDateTime(request.Value.TimeStamp);
						values[i].m_quality = (ushort)request.Value.Quality;
						values[i].m_value = Marshal.AllocCoTaskMem(ValueQT.VARIANT_SIZE);
						Marshal.GetNativeVariantForObject(request.Value.Data, values[i].m_value);

						results[i] = (int)request.Result;					
					}
					else{
						values[i].m_quality = (ushort)EnumQuality.BAD;
						values[i].m_value = Marshal.AllocCoTaskMem(ValueQT.VARIANT_SIZE);
						Marshal.GetNativeVariantForObject(null, values[i].m_value);
						results[i] = (int)EnumResultCode.E_UNEXPECTED;
					}	//	end if ... else					
				}
				else{
					requests[i].m_sessionHandle = IntPtr.Zero;
					requests[i].m_propertyID    = 0; 
					requests[i].m_requestHandle = IntPtr.Zero;
					requests[i].m_object.m_userData = 0;
					requests[i].m_object.m_objectHandle = IntPtr.Zero;

					values[i] = new OTValueData();
					values[i].m_quality = (ushort)EnumQuality.BAD;
					values[i].m_value = Marshal.AllocCoTaskMem(ValueQT.VARIANT_SIZE);
					Marshal.GetNativeVariantForObject(null, values[i].m_value);
					results[i] = (int)EnumResultCode.E_UNEXPECTED;
				}	//	end if ... else
				
				request.RequestState = EnumRequestState.COMPLETED;

			}	//	end for
			
			int result = OTBFunctions.OTSCompleteRequests(count, requests, results, values);

			for (int i=0; i<count; i++){
				Win32Functions.VariantClear(values[i].m_value);
				Marshal.FreeCoTaskMem(values[i].m_value);
				syncArrayList.Clear();
			}	//	end for
			
			Application.Instance.ReleaseTransaction(m_key);

			return result;

		}	//	end CompleteRequests
		

		/// <summary>
		/// Completes a single request and removes it from the transaction
		/// </summary>
		/// <returns>
		/// S_OK - Everything was OK
		/// S_FALSE - Everything was OK
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>		
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  method[@name="CompleteRequest"]/doc/*'
		/// />		
		public int CompleteRequest(DaRequest aRequest){
			
			OTValueData[] values = new OTValueData[1];
			OTSRequestData[] requests = new OTSRequestData[1];
			int[] results = new int[1];
				
			DaRequest request =  aRequest as DaRequest;
			if (request != null){

				requests[0].m_sessionHandle = request.SessionHandle;
				requests[0].m_propertyID    = request.PropertyId ; 
				requests[0].m_requestHandle = request.RequestHandle;
				
				if (request.AddressSpaceElement != null){
					requests[0].m_object.m_userData		= (uint)request.AddressSpaceElement.UserData;
					requests[0].m_object.m_objectHandle	= request.AddressSpaceElement.ObjectHandle;
				}
				else{
					requests[0].m_object.m_userData		= 0;
					requests[0].m_object.m_objectHandle = IntPtr.Zero;
				}	//	end if ... else

				values[0] = new OTValueData();
				if	(request.Value != null){
					values[0].m_timestamp = new OTDateTime(request.Value.TimeStamp);
					values[0].m_quality = (ushort)request.Value.Quality;
					values[0].m_value = Marshal.AllocCoTaskMem(ValueQT.VARIANT_SIZE);
					Marshal.GetNativeVariantForObject(request.Value.Data, values[0].m_value);

					results[0] = (int)request.Result;					
				}
				else{
					values[0].m_quality = (ushort)EnumQuality.BAD;
					values[0].m_value = Marshal.AllocCoTaskMem(ValueQT.VARIANT_SIZE);
					Marshal.GetNativeVariantForObject(null, values[0].m_value);
					results[0] = (int)EnumResultCode.E_UNEXPECTED;
				}	//	end if ... else					
			}
			else{
				requests[0].m_sessionHandle = IntPtr.Zero;
				requests[0].m_propertyID    = 0; 
				requests[0].m_requestHandle = IntPtr.Zero;
				requests[0].m_object.m_userData = 0;
				requests[0].m_object.m_objectHandle = IntPtr.Zero;

				values[0] = new OTValueData();
				values[0].m_quality = (ushort)EnumQuality.BAD;
				values[0].m_value = Marshal.AllocCoTaskMem(ValueQT.VARIANT_SIZE);
				Marshal.GetNativeVariantForObject(null, values[0].m_value);
				results[0] = (int)EnumResultCode.E_UNEXPECTED;
			}	//	end if ... else
			
			request.RequestState = EnumRequestState.COMPLETED;
			
			int result = OTBFunctions.OTSCompleteRequests(1, requests, results, values);
			
			Win32Functions.VariantClear(values[0].m_value);
			Marshal.FreeCoTaskMem(values[0].m_value);
			
			RemoveRequest(aRequest);
			Application.Instance.ReleaseTransaction(m_key);

			return result;

		}	//	end CompleteRequest


		/// <summary>
		/// Changes the cache value for the Address space elements provided
		/// </summary>
		/// <returns>
		/// E_INVALIDARG - Invalid valueQT was passed
		/// S_OK - Value changed
		/// OTS_E_EXCEPTION - Unexpected error occurred    
		/// The Result should be checked with ResultCode.SUCCEEDED
		/// or with ResultCode.FAILED
		/// </returns>		
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  method[@name="ValuesChanged"]/doc/*'
		/// />
		public int ValuesChanged(){
			
			try{
				ArrayList syncArrayList = ArrayList.Synchronized(m_requestList);
				int count = syncArrayList.Count;
				
				OTValueData[] values = new OTValueData[count];
				IntPtr[] handles = new IntPtr[count];
				
				for(int i = 0; i < count; i++){
					
					OTValueData valueData = new OTValueData();
					DaRequest request = syncArrayList[i] as DaRequest;

					if (request != null){
												
						valueData.m_quality = (ushort)request.Value.Quality;
						valueData.m_timestamp = new OTDateTime(request.Value.TimeStamp);
						valueData.m_value = Marshal.AllocCoTaskMem(ValueQT.VARIANT_SIZE);
						
						Marshal.GetNativeVariantForObject(request.Value.Data, valueData.m_value);
						
						handles[i] = request.AddressSpaceElement.ObjectHandle;
					}
					else{						
						valueData.m_value = Marshal.AllocCoTaskMem(ValueQT.VARIANT_SIZE);
						Marshal.GetNativeVariantForObject(null, valueData.m_value);
						
						handles[i] = IntPtr.Zero;
					}	//	end if ... else
					request.Result = EnumResultCode.S_OK;
					values[i] = valueData;

				}	//	end for
				
				int result = OTBFunctions.OTSValuesChanged(count, handles, values);
				
				for (int i = 0; i < count; i++){
					Win32Functions.VariantClear(values[i].m_value);
					Marshal.FreeCoTaskMem(values[i].m_value);
				}	//	end for

				return result;
			}
			catch(Exception e){
				Application.Instance.Trace(
					EnumTraceLevel.ERR, EnumTraceGroup.OPCSERVER,
					"DaTransaction.ValuesChanged", "Exception caught: " + e.ToString());
				return (int)EnumResultCode.E_FAIL;
			}	//	end catch

		}	//	end ValuesChanged


		/// <summary>
		/// removes a specific request from the internal requests list
		/// </summary>
		/// <param name="aRequest"> 
		/// the request to be removed from the transactrion
		/// </param>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaTransaction"]/
		///  method[@name="RemoveRequest"]/doc/*'
		/// />
		public void RemoveRequest(DaRequest aRequest){

			if (aRequest == null){
				return;
			}	//	end if

			ArrayList syncRequestList = ArrayList.Synchronized(m_requestList);
			syncRequestList.Remove(aRequest);			

		}	//	end removeRequest

		//-
		#endregion
	
	}	//	end class Transaction

}	//	end namespace Softing.OPCToolbox.Server


