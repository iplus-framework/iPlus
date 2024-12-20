// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace gip.tool.devLicenseProvider
{
    using System;
    using System.Collections.Generic;
    
    public partial class License
    {
        public System.Guid LicenseID { get; set; }
        public int LicenseNo { get; set; }
        public string ProjectNo { get; set; }
        public System.Guid CustomerID { get; set; }
        public string PackagePrivateKey { get; set; }
        public string RemotePrivateKey { get; set; }
        public byte[] DongleKey { get; set; }
        public string VBSystemKey { get; set; }
    
        public virtual Customer Customer { get; set; }
    }
}
