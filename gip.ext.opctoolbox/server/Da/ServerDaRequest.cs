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
//  Filename    : ServerDaRequest.cs                                          |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : OPC DA client generated request handler class               |
//                                                                            |
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using Softing.OPCToolbox.OTB;

namespace Softing.OPCToolbox.Server{

	/// <summary>
	/// Stores data concerning an OPC read or write request.
	/// </summary>
	/// <include 
	///		file='TBNS.doc.xml' 
	///		path='//class[@name="DaRequest"]/doc/*' 
	///	/>
	public class DaRequest{
		
		#region //	Public Constructors
		//-----------------------------
		
		/// <summary>
		/// Default public constructor
		/// </summary>
		/// <include
		///		file='TBNS.doc.xml' 
		///		path='//class[@name="DaRequest"]/
		///		ctor[@name="DaRequest"]/doc/*'
		///	/>
		public DaRequest(
			EnumTransactionType aTransactionType,
			IntPtr aSessionHandle,
			DaAddressSpaceElement anElement,
			int aPropertyId,
			IntPtr aRequestHandle){
			
			m_transactionType = aTransactionType;
			m_sessionHandle = aSessionHandle;
			m_requestHandle = aRequestHandle;
			m_propertyId = aPropertyId;
			m_addressSpaceElement = anElement;
			m_transactionKey = 0;
			m_result = EnumResultCode.E_NOTIMPL;
			m_requestState = EnumRequestState.CREATED;
			
		}	//	end ctr

		//-	
		#endregion

		#region	//	Public Properties
		//---------------------------
		
		/// <summary>
		/// The property ID the request is about (0 means no property request)
		/// </summary>		
		/// <include
		///		file='TBNS.doc.xml' 
		///		path='//class[@name="DaRequest"]/
		///		property[@name="PropertyId"]/doc/*'
		///	/>
		public int PropertyId{
			get	{ return m_propertyId; }			
		}	//	end PropertyId
				
		/// <summary>
		/// The namespace element the request is about
		/// </summary>
		/// <include
		///		file='TBNS.doc.xml' 
		///		path='//class[@name="DaRequest"]/
		///		property[@name="AddressSpaceElement"]/doc/*'
		///	/>
		public DaAddressSpaceElement AddressSpaceElement{
			get	{ return m_addressSpaceElement; }
		}	//	end AddressSpaceElement
		
		/// <summary>
		/// The result of the request
		/// </summary>
		/// <include
		///		file='TBNS.doc.xml' 
		///		path='//class[@name="DaRequest"]/
		///		property[@name="Result"]/doc/*'
		///	/>
		public EnumResultCode Result{
			get	{ return m_result; }
			set	{ m_result = value; }
		}	//	end Result

		/// <summary>
		/// the state of the request
		/// </summary>
		/// <include
		///		file='TBNS.doc.xml' 
		///		path='//class[@name="DaRequest"]/
		///		property[@name="RequestState"]/doc/*'
		///	/>
		public EnumRequestState RequestState{
			get	{ return m_requestState; }
			set	{ m_requestState = value; }
		}	//	end RequestState

		/// <summary>
		/// the transaction the request is contained in 
		/// </summary>
		/// <include
		///		file='TBNS.doc.xml' 
		///		path='//class[@name="DaRequest"]/
		///		property[@name="TransactionKey"]/doc/*'
		///	/>
		public uint TransactionKey{
			get	{ return m_transactionKey; }
			set	{ m_transactionKey = value; }
		}	//	end TransactionKey
		
		/// <summary>
		/// The type of transaction to be performed
		/// </summary>
		/// <include
		///		file='TBNS.doc.xml' 
		///		path='//class[@name="DaRequest"]/
		///		property[@name="TransactionType"]/doc/*'
		///	/>
		public EnumTransactionType TransactionType{
			get{return m_transactionType;}
		}	//	TransactionType
		
		/// <summary>
		/// This requests value with quality and timestamp.
		/// </summary>
		/// <include
		///		file='TBNS.doc.xml' 
		///		path='//class[@name="DaRequest"]/
		///		property[@name=Const.Value]/doc/*'
		///	/>
		public ValueQT Value{
			get	{return m_value;}
			set	{m_value = value;}
		}	//	end Value
		
		//-
		#endregion
		
		#region //	Internal Properties
		//-----------------------------

		/// <summary>
		/// The handle of the request itself
		/// </summary>
		internal IntPtr RequestHandle {
			get	{ return m_requestHandle;}
		}	//	end RequestHandle
		
		/// <summary>
		/// Returns the handle of the owning session
		/// </summary>
		internal IntPtr SessionHandle {
			get	{ return m_sessionHandle;}
		}	//	end SessionHandle

		//-
		#endregion 

		#region	//	Public Methods
		//------------------------

		/// <summary>
		/// Complete only this request
		/// </summary>
		/// <include
		///		file='TBNS.doc.xml' 
		///		path='//class[@name="DaRequest"]/
		///		method[@name="Complete"]/doc/*'
		///	/>		
		public virtual int Complete(){

			int result = (int)EnumResultCode.S_FALSE;
			
			DaTransaction transaction = Application.Instance.FindTransaction(m_transactionKey);
			if (transaction != null){
				result = transaction.CompleteRequest(this);
			}	//	end if

			return result;
		
		}	//	end Complete
		
		//-
		#endregion
		
		#region //	Private Attributes
		//------------------------------
		
		private readonly IntPtr m_sessionHandle = IntPtr.Zero;
		private readonly IntPtr m_requestHandle = IntPtr.Zero;
		
		/// <summary>
		/// The owning transaction key 
		/// </summary>
		private uint m_transactionKey = 0;
		
		/// <summary>
		/// The associated transaction type
		/// </summary>
		private EnumTransactionType m_transactionType;

		/// <summary>
		/// Property assigned to the request
		/// </summary>
		private int m_propertyId = 0;
		
		/// <summary>
		/// Address space element assigned to the request
		/// </summary>
		private DaAddressSpaceElement m_addressSpaceElement = null;

		/// <summary>
		/// Result code to be filled in when request resolved
		/// </summary>
		private EnumResultCode m_result = EnumResultCode.E_NOTIMPL;

		/// <summary>
		/// Current cempletion state
		/// </summary>
		private EnumRequestState m_requestState = EnumRequestState.CREATED;
		
		/// <summary>
		/// Value carried by the request as a reply or as a write request
		/// </summary>
		private ValueQT m_value = null;
		
		//-
		#endregion
		
	}	//	end DaRequest

}	//	end Softing.OPCToolbox.Server
