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
//  Filename    : ClientDaPropertiesGetOptions.cs                             |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : Options class for browsing an OPC Item's properties         |
//                                                                            |
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace  Softing.OPCToolbox.Client{

	/// <summary>
	///  Specifies the criteria on which the browsing of an <see cref="DaItem"/> properties is made.
	/// </summary>
	/// <include 
	///  file='TBNC.doc.xml' 
	///  path='//class[@name="DaGetPropertiesOptions"]/doc/*'
	/// />
	[SerializableAttribute]
	public class DaGetPropertiesOptions{

		#region // Public Constructors
		//----------------------------

		/// <summary>
		/// Initializes a new instance of the <see cref="DaGetPropertiesOptions"/> class.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaGetPropertiesOptions"]/constructor[@name="DaGetPropertiesOptions"]/doc/*'
		/// />
		public DaGetPropertiesOptions(){
		}	//	end constructor
	
		//-
		#endregion

		#region	// Protected Members
		//------------------------
		/// <summary>
		/// What property data to be retrieved.
		/// </summary>		
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaGetPropertiesOptions"]/field[@name="m_whatPropertyData"]/doc/*'
		/// />
		protected EnumPropertyData m_whatPropertyData = 0;
		/// <summary>
		/// A list of identifiers for the <see cref="DaItem"/>'s properties.
		/// </summary>		
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaGetPropertiesOptions"]/field[@name="m_pPropertyIds"]/doc/*'
		/// />
		protected int[] m_pPropertyIds = null;
		/// <summary>
		/// A list of names for the <see cref="DaItem"/>'s properties.		
		///</summary>	
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaGetPropertiesOptions"]/field[@name="m_pPropertyNames"]/doc/*'
		/// />
		protected string[] m_pPropertyNames = null;
		
		//-
		#endregion
		
		#region //	Public Properties
		//---------------------------
		/// <summary>
		/// Gets or sets the property data to be retrieved.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaGetPropertiesOptions"]/property[@name="WhatPropertyData"]/doc/*'
		/// />	
		public EnumPropertyData WhatPropertyData{			
			get{
				return this.m_whatPropertyData;
			}	//	end get
			set{
				this.m_whatPropertyData = value;
			}	//	end set
		}	//	end WhatPropertyData
		/// <summary>
		/// Gets or sets a list of identifiers for an <see cref="DaItem"/>'s properties.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaGetPropertiesOptions"]/property[@name="PropertyIds"]/doc/*'
		/// />
		public int[] PropertyIds{
			get{
				return m_pPropertyIds;
			}	//	end get
			set{
				m_pPropertyIds = value;
			}	//	end set
		}	//	end PropertyIDs
		
		/// <summary>
		/// Gets or sets a list of names for an <see cref="DaItem"/>'s properties.
		/// </summary>
		/// <include 
		///  file='TBNC.doc.xml' 
		///  path='//class[@name="DaGetPropertiesOptions"]/property[@name="PropertyNames"]/doc/*'
		/// />
		public string[] PropertyNames{
			get{
				return m_pPropertyNames;
			}	//	end get
			set{
				m_pPropertyNames = value;
			}	//	end set
		}	//	end PropertyNames
		
		//-
		#endregion

	}	//	end class PropertiesGetOptions

}	//	end namespace Client.Da
