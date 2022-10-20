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
//  Filename    : ClientValueEditor.cs                                        |
//  Version     : 4.22.release                                                |
//  Date        : 15-December-2008                                            |
//                                                                            |
//  Description : Class used to design value editors.                         |
//                                                                            |
//-----------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Softing.OPCToolbox.Client;

namespace Softing.OPCToolbox.Client.Control{

	/// <summary>
	/// Represents and edits the values of an ControlDaSession object.
	/// </summary>
	internal class ValueEditor : System.Drawing.Design.UITypeEditor {

		#region //	Public Methods
		//----------------------

		public ValueEditor()
		{
		}
		[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")] 
		
		public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context) {
			return UITypeEditorEditStyle.Modal;
		}
		
		
		[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
		
		public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value) {            
		
			IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if( edSvc != null ) {				
				InfoCollector infoCollector = new InfoCollector((ControlDaSession)value);
				edSvc.ShowDialog( infoCollector);
				return infoCollector.InfoCollected;
			}
			return value;
		}

		//-
		#endregion

	}
}
