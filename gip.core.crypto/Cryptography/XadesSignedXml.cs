﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACCrypt.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace gip.core.crypto
{

    public class XadesSignedXml : SignedXml
    {
        #region Public fields
        public const string XmlDsigSignatureProperties = "http://uri.etsi.org/01903#SignedProperties";
        public const string XadesProofOfApproval = "http://uri.etsi.org/01903/v1.2.2#ProofOfApproval";
        public const string XadesPrefix = "xades";
        public const string XadesNamespaceUrl = "http://uri.etsi.org/01903/v1.3.2#";
        public XmlElement PropertiesNode { get; set; }
        #endregion Public fields

        #region Private fields
        private readonly List<DataObject> _dataObjects = new List<DataObject>();
        #endregion Private fields

        #region Constructor
        public XadesSignedXml(XmlDocument document) : base(document) { }
        #endregion Constructor

        #region SignedXml
        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            if (string.IsNullOrEmpty(idValue))
                return null;

            var xmlElement = base.GetIdElement(document, idValue);
            if (xmlElement != null)
                return xmlElement;

            if (_dataObjects.Count == 0)
                return null;

            foreach (var dataObject in _dataObjects)
            {
                var nodeWithSameId = XmlHelper.FindNodeWithAttributeValueIn(dataObject.Data, "Id", idValue);
                if (nodeWithSameId != null)
                    return nodeWithSameId;
            }

            return null;
        }

        public new void AddObject(DataObject dataObject)
        {
            base.AddObject(dataObject);
            _dataObjects.Add(dataObject);
        }
        #endregion SignedXml
    }
}
