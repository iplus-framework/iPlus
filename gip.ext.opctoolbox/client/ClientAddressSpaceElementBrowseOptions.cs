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
//  Filename    : ClientAddressSpaceElementBrowseOptions.cs                   |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : - AddressSpaceElementBrowseOptions						  |
//					  Options class for browsing an OPC Server's address space|						  
//				  - DaAddressSpaceElementBrowseOptions						  |
//                    Options class for browsing a DA Server's address space  |
//-----------------------------------------------------------------------------
using System;

namespace Softing.OPCToolbox.Client{

	/// <summary>
	/// Specifies the criteria on which the browsing of an OPC server address space is made.
	/// </summary>
	/// <include 
	///  file='TBNC.doc.xml' 
	///  path='//class[@name="AddressSpaceElementBrowseOptions"]/doc/*'
	/// />	
	public class AddressSpaceElementBrowseOptions{

		#region //Public Contructor
		//------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="AddressSpaceElementBrowseOptions"/> class.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElementBrowseOptions"]/constructor[@name="AddressSpaceElementBrowseOptions"]/doc/*'
		/// />
		public AddressSpaceElementBrowseOptions(){}
		//-
		#endregion

		#region //Protected Attributes
		//--------------------------
		/// <summary>
		/// Specifies the type of the <see cref="Softing.OPCToolbox.Client.AddressSpaceElement"/>s which will be returned as result of browsing.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElementBrowseOptions"]/field[@name="m_elementTypeFilter"]/doc/*'
		/// />
		protected EnumAddressSpaceElementType m_elementTypeFilter = EnumAddressSpaceElementType.ALL;
		/// <summary>
		/// Specifies the name of the <see cref="Softing.OPCToolbox.Client.AddressSpaceElement"/>s which will be returned as result of browsing.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElementBrowseOptions"]/field[@name="m_elementNameFilter"]/doc/*'
		/// />
		protected string m_elementNameFilter = null;

		//-
		#endregion

		#region //Public Properties
		//-------------------------

		/// <summary>
		/// Gets or sets the name of the <see cref="AddressSpaceElement"> AddressSpaceElement</see>s which will be returned as result of browsing the server's address space.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElementBrowseOptions"]/property[@name="ElementNameFilter"]/doc/*'
		/// />			
		public string  ElementNameFilter{
			get{ 
				return m_elementNameFilter;
			}
			set{
				m_elementNameFilter = value;
			}
		}
		/// <summary>
		/// Gets or sets the type of the <see cref="AddressSpaceElement"> AddressSpaceElement</see>s which will be returned as result of browsing the server's address space.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="AddressSpaceElementBrowseOptions"]/property[@name="ElementTypeFilter"]/doc/*'
		/// />			
		public EnumAddressSpaceElementType ElementTypeFilter{
			get{
				return m_elementTypeFilter;
			}
			set{
				m_elementTypeFilter = value;
			}
		}
		
		//-
		#endregion
	}

	/// <summary>
	/// Specifies the criteria on which the browsing of an OPC Data Access Server address space is made.
	/// </summary>
	/// <include 
	///  file='TBNC.doc.xml' 
	///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/doc/*'
	/// />	
	public class DaAddressSpaceElementBrowseOptions : AddressSpaceElementBrowseOptions {
	
		#region //Public Contructor
		//------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="DaAddressSpaceElementBrowseOptions"/> class.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/constructor[@name="DaAddressSpaceElementBrowseOptions"]/doc/*'
		/// />
		public DaAddressSpaceElementBrowseOptions(){}
		//-
		#endregion

		#region //Protected Attributes
		//--------------------------
		
		///<summary>
		///The maximum number of elements to return.
		///</summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/field[@name="m_maxElements"]/doc/*'
		/// />
		protected uint m_maxElements = 0;
		///<summary>
		///Indicates whether all supported properties should be returned with each element.
		///</summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/field[@name="m_returnProperties"]/doc/*'
		/// />
		protected bool m_returnProperties = false;
		///<summary>
		///Indicates whether property values should be returned with the properties.
		///</summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/field[@name="m_returnPropertyValues"]/doc/*'
		/// />
		protected bool m_returnPropertyValues = false;
		///<summary>
		///Indicates whether to retrieve the item identifier of the returned elements..
		///</summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/field[@name="m_retrieveItemID"]/doc/*'
		/// />
		protected bool m_retrieveItemID = true;
		///<summary>
		///Indicates the server manufacturer who defined the properties to be returned.
		///</summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/field[@name="m_vendorFilter"]/doc/*'
		/// />
		protected string m_vendorFilter = null;
		///<summary>
		///Indicates that only elements the values of which have specific data type will be returned.
		///</summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/field[@name="m_dataTypeFilter"]/doc/*'
		/// />
		protected System.Type m_dataTypeFilter = null;
		///<summary>
		///Indicates that only elements with specific access rights will be returned.
		///</summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/field[@name="m_accessRightsFilter"]/doc/*'
		/// />
		protected EnumAccessRights m_accessRightsFilter = 0;

		//-
		#endregion

		#region //Public Properties
		//-------------------------
		/// <summary>
		/// Gets or sets the maximum number of elements to return.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/property[@name="MaxElements"]/doc/*'
		/// />	
		public uint MaxElements{
			get{
				return m_maxElements;
			}
			set{
				m_maxElements = value;
			}
		}
		/// <summary>		
		/// Gets or sets the server manufacturer who defined properties that need to be returned.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/property[@name="VendorFilter"]/doc/*'
		/// />	
		public string VendorFilter{
			get{
				return m_vendorFilter;
			}
			set{
				m_vendorFilter = value;
			}
		}
		/// <summary>
		/// Gets or sets the access rights of the elements to be returned.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/property[@name="AccessRightsFilter"]/doc/*'
		/// />			
		public EnumAccessRights AccessRightsFilter{
			get{
				return m_accessRightsFilter;
			}
			set{
				m_accessRightsFilter = value;
			}
		}
		/// <summary>
		/// Gets or sets the data type for the values of the elements to be returned.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/property[@name="DataTypeFilter"]/doc/*'
		/// />	
		public System.Type DataTypeFilter{
			get{
				return m_dataTypeFilter;
			}
			set{
				m_dataTypeFilter = value;
			}
		}
		/// <summary>
		/// Gets or sets whether property values should be returned with the properties.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/property[@name="ReturnPropertyValues"]/doc/*'
		/// />	
		public bool ReturnPropertyValues{
			get{
				return m_returnPropertyValues;
			}
			set{
				m_returnPropertyValues = value;
			}
		}
		/// <summary>
		///  Gets or sets whether all supported properties should be returned with each element.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/property[@name="ReturnProperties"]/doc/*'
		/// />	
		public bool ReturnProperties{
			get{
				return m_returnProperties;
			}
			set{
				m_returnProperties = value;
			}
		}
		/// <summary>
		/// Gets or sets whether to retrieve the item identifier of the returned elements.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaAddressSpaceElementBrowseOptions"]/property[@name="RetrieveItemId"]/doc/*'
		/// />	
		public bool RetrieveItemId{
			get{
				return m_retrieveItemID;
			}
			set{
				m_retrieveItemID = value;
			}
		}
		
		//-
		#endregion
	}
}
