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
//  Filename    : ServerDaSession.cs                                          |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : OPC DA Session opened by client handler class               |
//                                                                            |
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Softing.OPCToolbox.Server{
	
	#region	//	Public Delegates
	//--------------------------
	
	/// <summary>
	/// <para lang="Unmanaged C++">
	/// <line>Delegate is not available for Unmanaged C++.</line>
	/// </para>
	/// <para lang="Unmanaged C++">
	/// <line>This functionality is available overriding the DaSession::handleConnected() virtual method. </line>
	/// </para>
	/// <para lang="Visual Basic, C#, C++">
	/// Delegate will be triggered when a the session was connected.
	/// </para>	
	/// </summary>
	public delegate void SessionConnected();
	
	/// <summary>
	/// <para lang="Unmanaged C++">
	/// <line>Delegate is not available for Unmanaged C++.</line>
	/// <line>This functionality is available overriding the handleDisconnected() virtual method .</line>
	/// </para>
	/// <para lang="Visual Basic, C#, C++">
	/// Delegate will be triggered when a the session was disconnected.
	/// </para>
	/// </summary>
	public delegate void SessionDisconnected();
	
	//-
	#endregion

	#region //	Public Classes
	//------------------------

	/// <summary>
	///	Represents an OPC client communication session
	/// </summary>
	/// <include 
	///  file='TBNS.doc.xml' 
	///  path='//class[@name="DaSession"]/doc/*'
	/// />
	public class DaSession{
		
		#region //	Public Constructors
		//-----------------------------
		
		/// <summary>
		/// Default session constructor
		/// </summary>
		/// <param name="aType">
		/// the type of the client session
		/// </param>
		/// <param name="aHandle">
		/// OTB internal session identifier.
		/// </param>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaSession"]/
		///  ctor[@name="DaSession"]/doc/*'
		/// />
		public DaSession(EnumSessionType aType, IntPtr aHandle){
			m_type = aType;
			m_objectHandle = aHandle;
		}	//	end ctr

		//-	
		#endregion
		
		#region //	Private Attributes
		//------------------------------

		private readonly EnumSessionType m_type = EnumSessionType.DA;
		private readonly IntPtr m_objectHandle = IntPtr.Zero;
		
		//-
		#endregion
		
		#region //	Public Events
		//-----------------------
		
		/// <summary>
		/// Event triggered when the OPC client set this session into the connected state.
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaSession"]/
		///  event[@name="Connected"]/doc/*'
		/// />
		public event SessionConnected Connected = null;

		/// <summary>
		/// Event triggered when the OPC client set this session into the disconnected state.
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaSession"]/
		///  event[@name="Disconnected"]/doc/*'
		/// />
		public event SessionDisconnected Disconnected = null;
		
		//-
		#endregion
		
		#region	//	Protected Properties
		//-----------------------------

		/// <summary>
		/// OTB Internal handler
		/// </summary>
		/// <include
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaSession"]/
		///  property[@name="ObjectHandle"]/doc/*'
		/// />
		protected IntPtr ObjectHandle{
			get	{ return m_objectHandle; }			
		}	//	end 
		
		//-
		#endregion
		
		#region //	Public Properties
		//---------------------------

		/// <summary>
		/// Type of client
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaSession"]/
		///  property[@name="Type"]/doc/*'
		/// />
		public virtual EnumSessionType Type {
			get	{ return m_type; }			
		}	//	end Type
		
		//-
		#endregion
		
		#region //	Public Methods
		//------------------------

		/// <summary>
		/// Notifies this session is switched to connected state
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaSession"]/
		///  method[@name="ConnectSession"]/doc/*'
		/// />
		public virtual void ConnectSession(){
			
			if (this.Connected != null){
				this.Connected();
			}	//	end if

		}	//	end ConnectSession

		/// <summary>
		/// Notifies this session is switched to disconnected state
		/// </summary>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaSession"]/
		///  method[@name="DisconnectSession"]/doc/*'
		/// />
		public virtual void DisconnectSession(){
			if (this.Disconnected != null){
				this.Disconnected();
			}	//	end if
		}	//	end DisconnectSession

		/// <summary>
		/// Called when an OPC client attempts an autentication to this session. This method needs to be overriden.
		/// </summary>
		/// <param name="anUserName">Username for login attempt</param>
		/// <param name="aPassword">Password for login attempt</param>
		/// <returns>
		/// EnumResultCode.E_NOTIMPL. Must be overridden. Should return S_OK when success or E_ACCESSDENIED when invalid credentials provided
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaSession"]/
		///  method[@name="Logon"]/doc/*'
		/// />
		public virtual int Logon (string  anUserName, string aPassword){
			return (int)EnumResultCode.E_NOTIMPL;
		}	//	end Logon
		
		/// <summary>
		/// Notify that this OPC Client logged off from the session.
		/// </summary>
		/// <returns>
		/// EnumResultCode.E_NOTIMPL. Must be overridden.
		/// </returns>
		/// <include 
		///  file='TBNS.doc.xml' 
		///  path='//class[@name="DaSession"]/
		///  method[@name="Logoff"]/doc/*'
		/// />
		public virtual int Logoff (){
			return (int)EnumResultCode.E_NOTIMPL;;
		}	//	end Logoff
		
		//-
		#endregion
		
 	}	//	end class Session
	
	//-
	#endregion

}	//	end namespace Softing.OPCToolbox.Server
