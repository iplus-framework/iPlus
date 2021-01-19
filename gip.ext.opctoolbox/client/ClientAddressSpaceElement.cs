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
//  Filename    : ClientAddressSpaceElement.cs                                |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : Generic object that represents AE and DA address space	  |	
//					elements											      |
//                                                                            |
//-----------------------------------------------------------------------------
using System;
using Softing.OPCToolbox.Client;
using System.Runtime.InteropServices;
using Softing.OPCToolbox;
using Softing.OPCToolbox.OTB;

namespace Softing.OPCToolbox.Client{

	/// <summary>
	/// <para>Represents an element in an OPC Server's address space.</para>
	/// <para>
	/// The elements in an OPC Server's address space have different signification depending on the OPC specification supported by the OPC Server.
	/// <br></br>The address space of an <b>Data Access Server</b> contains all the data sources and data sinks made available by that <b>Data Access Server.</b>
	/// <br></br>It can be a structure of any depth(hierarchical) or it can be flat.
	/// <br></br> In the flat adress space there are no nodes and all the leaves are on the same level.
	/// <br></br>A hierarchical presentation of the server address space would behave much like a file system, where the directories 
	/// are the branches or paths, and the files represent the leaves or items. 
	/// For example, a server could present a control system by showing all the control networks, then all of the devices 
	/// on a selected network, and then all of the classes of data within a device, then all of the data items of that class.
	/// In a hierachical adress space, nodes can be used as structuring means.The nodes can for example represent devices in an installation;
	/// leaves are available at the nodes,representing data sinks(e.g.controllers) and data sources(e.g.temperature sensors).
	/// These can be setpoints and measured variables of a device.
	/// </para>
	/// <para>The address space of an <b>Alarms and Events Server</b> is always hierarchical and is used for topographic organization of events.
	/// The nodes of the address space describe areas in which events may occur(e.g. room).The leaves describe events sources.
	/// Several events can be available at one event source (e.g monitoring of the temperature value and the temperature sensor). 
	/// </para>
	/// </summary>
	/// <include 
	///  file='TBNC.doc.xml' 
	///  path='//class[@name="AddressSpaceElement"]/doc/*'
	/// />	
	[Serializable]
	public class AddressSpaceElement : IDisposable{
		
		#region //Public Constructors
		//-----------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="AddressSpaceElement"/> class.
		/// </summary>
		/// <include
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElement"]/constructor[@name="AddressSpaceElement"]/doc/*'
		/// />
		public AddressSpaceElement()
		{			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AddressSpaceElement"/> class with 
		/// the value indicated by an element name and an element handle.
		/// </summary>
		/// <param name="aName">A name for the address space element.</param>
		/// <param name="aQualifiedName">The qualified name for the address space element.</param>
		/// <param name="aHandle">A handle for the address space element.</param>		
		internal AddressSpaceElement(
			string aName,
			string aQualifiedName,
			IntPtr aHandle)
		{		
			m_name = aName;
			m_qName = aQualifiedName;
			m_objectElementHandle = aHandle;
		}

		/// <summary>
		/// The <see cref="AddressSpaceElement">AddressSpaceElement</see> class destructor.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElement"]/method[@name="Finalize"]/doc/*'
		/// />
		~AddressSpaceElement()
		{
			Dispose();
		}

		/// <summary>
		/// Releases the resources used by the <see cref="AddressSpaceElement"/> object.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElement"]/method[@name="Dispose"]/doc/*'
		/// />
		public void Dispose()
		{   
			if (m_objectElementHandle != IntPtr.Zero)
			{
				OTBFunctions.OTCReleaseAddressSpaceElement(m_objectElementHandle,0);
			}
		}

		//-
		#endregion
	
		#region //Protected Attributes
		//----------------------------

		internal IntPtr m_objectElementHandle = IntPtr.Zero;

		/// <summary>
		/// A name for an element within the server's address space. 
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElement"]/field[@name="m_name"]/doc/*'
		/// />
		protected string m_name = string.Empty;
		
		/// <summary>
		/// A qualified name for an element within the server's address space. 
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElement"]/field[@name="m_qName"]/doc/*'
		/// />
		protected string m_qName = string.Empty;
		
		//-
		#endregion				

		#region //Public Properties
		//-------------------------
		
		///<summary>
		///Gets the handle of an <see cref="AddressSpaceElement"/> in the server's adress space.
		///</summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElement"]/property[@name="Handle"]/doc/*'
		/// />
		public IntPtr Handle
		{
			get
			{
				return m_objectElementHandle;
			}			
		}

		/// <summary>
		/// Gets or sets the name of an <see cref="AddressSpaceElement"/> within the server's adress space.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElement"]/property[@name="Name"]/doc/*'
		/// />	
		public virtual string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		/// <summary>
		/// Gets or sets the qualified name of an <see cref="AddressSpaceElement"/> within the server's adress space.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElement"]/property[@name="QualifiedName"]/doc/*'
		/// />	
		public virtual string QualifiedName
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

		//-
		#endregion
		
	}	//	end AddressSpaceElement

}	//	end Softing.OPCToolbox.Client
