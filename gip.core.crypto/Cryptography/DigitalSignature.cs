// ***********************************************************************
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
using System.Globalization;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography;
using IM.Xades;
using Egelke.EHealth.Client.Pki;
using Org.BouncyCastle.Crypto;
using Egelke.EHealth.Client.Pki.DSS;
using System.ServiceModel;
using IM.Xades.Extra;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Tsp;
using System.Text;
using Disig.TimeStampClient;

namespace gip.core.crypto
{
    public static class DigitalSignature
    {
        #region Private fields
        private const string C_SignatureId = "Signature";
        private const string C_SignaturePropertiesId = "SignedProperties";
        private const string C_SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
        private const string C_CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
        #endregion Private fields

        #region Public methods
        // https://www.w3.org/TR/XAdES/
        // https://www.w3.org/TR/xml-exc-c14n/
        // https://ec.europa.eu/digital-building-blocks/DSS/webapp-demo/validation
        // https://github.com/soonthana/XAdES/blob/master/Src/XAdES/XAdESSignature.cs
        // https://github.com/wjab/xadesnet/tree/master/XadesNetLib
        // https://stackoverflow.com/questions/50096199/c-sharp-how-to-properly-sign-message-with-xades-using-signedxml
        // https://www.scottbrady.io/c-sharp/xml-signing-dotnet

        public static void Sign(XmlDocument xmlDocument, X509Certificate2 signingCertificate, bool withNewerC14N = true, bool insertSignatureAtBeginning = false)
        {
            XmlElement signatureXML = DigitalSignature.Sign(signingCertificate, xmlDocument, withNewerC14N);
            XmlNode signatureNode = xmlDocument.ImportNode(signatureXML, true);
            XmlElement root = xmlDocument.DocumentElement;
            if (insertSignatureAtBeginning)
                root.InsertBefore(signatureNode, root.FirstChild);
            else
                root.AppendChild(signatureNode);
        }

        public static void SignWithXAdES(XmlDocument xmlDocument, X509Certificate2 signingCertificate, bool withNewerC14N = true, bool insertSignatureAtBeginning = false)
        {
            XmlElement signatureXML = DigitalSignature.SignWithXAdES(signingCertificate, xmlDocument, withNewerC14N);
            XmlNode signatureNode = signatureXML; // xmlDocument.ImportNode(signatureXML, true);
            XmlElement root = xmlDocument.DocumentElement;
            if (insertSignatureAtBeginning)
                root.InsertBefore(signatureNode, root.FirstChild);
            else
                root.AppendChild(signatureNode);
        }

        public static XmlElement SignWithXAdES(X509Certificate2 signingCertificate, XmlDocument xmlDocument, bool withNewerC14N = true)
        {
            var signedXml = new XadesSignedXml(xmlDocument);
            signedXml.Signature.Id = C_SignatureId;
            signedXml.SigningKey = signingCertificate.GetRSAPrivateKey();
            if (withNewerC14N)
                signedXml.SignedInfo.CanonicalizationMethod = C_CanonicalizationMethod;

            var signatureReference = new Reference { Uri = "" };
            signatureReference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            if (withNewerC14N)
                signatureReference.AddTransform(new XmlDsigExcC14NTransform());
            signedXml.AddReference(signatureReference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(signingCertificate));
            signedXml.KeyInfo = keyInfo;

            AddXAdESProperties(xmlDocument, signedXml, signingCertificate);

            signedXml.ComputeSignature();

            return signedXml.GetXml();
        }

        public static void SignWithImXAdES(XmlDocument xmlDocument, X509Certificate2 signingCertificate, bool insertSignatureAtBeginning = false, DateTime? timestamp = null)
        {
            XadesCreator creator = new XadesCreator(signingCertificate);
            if (!timestamp.HasValue)
                timestamp = DateTime.UtcNow;

            //TimeStampAuthorityClient tsa = new TimeStampAuthorityClient(new StsBinding(), new EndpointAddress("https://services-acpt.ehealth.fgov.be/TimestampAuthority/v2"));
            //tsa.ClientCredentials.ClientCertificate.Certificate = signingCertificate;
            //creator.TimestampProvider = new EHealthTimestampProvider(tsa);
            creator.TimestampProvider = new DigitalSignaturTimeProvider(xmlDocument);
            //creator.DataTransforms.Add(new XmlDsigBase64Transform());
            //creator.DataTransforms.Add(new OptionalDeflateTransform());
            XmlElement signatureXML = creator.CreateXadesT(xmlDocument);

            XmlNode signatureNode = signatureXML; // xmlDocument.ImportNode(signatureXML, true);
            XmlElement root = xmlDocument.DocumentElement;
            if (insertSignatureAtBeginning)
                root.InsertBefore(signatureNode, root.FirstChild);
            else
                root.AppendChild(signatureNode);
        }

        public static XmlElement Sign(X509Certificate2 signingCertificate, XmlDocument xmlDocument, bool withNewerC14N = true)
        {
            var signedXml = new SignedXml(xmlDocument);
            //signedXml.Signature.Id = C_SignatureId;
            signedXml.SigningKey = signingCertificate.GetRSAPrivateKey();
            if (withNewerC14N)
                signedXml.SignedInfo.CanonicalizationMethod = C_CanonicalizationMethod;

            var signatureReference = new Reference { Uri = "" };
            signatureReference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            if (withNewerC14N)
                signatureReference.AddTransform(new XmlDsigExcC14NTransform());
            signedXml.AddReference(signatureReference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(signingCertificate));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();

            return signedXml.GetXml();
        }
        #endregion Public methods

        #region Private methods
        private static void AddXAdESProperties(XmlDocument document, XadesSignedXml xadesSignedXml, X509Certificate2 signingCertificate, bool onlyTime = false)
        {
            var parametersSignature = new Reference
            {
                Uri = $"#{C_SignaturePropertiesId}",
                Type = XadesSignedXml.XmlDsigSignatureProperties,
            };
            xadesSignedXml.AddReference(parametersSignature);

            // <Object>
            var objectNode = document.CreateElement("Object", SignedXml.XmlDsigNamespaceUrl);

            // <Object><QualifyingProperties>
            var qualifyingPropertiesNode = document.CreateElement(XadesSignedXml.XadesPrefix, "QualifyingProperties", XadesSignedXml.XadesNamespaceUrl);
            qualifyingPropertiesNode.SetAttribute("Target", $"#{C_SignatureId}");
            objectNode.AppendChild(qualifyingPropertiesNode);

            // <Object><QualifyingProperties><SignedProperties>
            var signedPropertiesNode = document.CreateElement(XadesSignedXml.XadesPrefix, "SignedProperties", XadesSignedXml.XadesNamespaceUrl);
            signedPropertiesNode.SetAttribute("Id", C_SignaturePropertiesId);
            qualifyingPropertiesNode.AppendChild(signedPropertiesNode);

            // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties>
            var signedSignaturePropertiesNode = document.CreateElement(XadesSignedXml.XadesPrefix, "SignedSignatureProperties", XadesSignedXml.XadesNamespaceUrl);
            signedPropertiesNode.AppendChild(signedSignaturePropertiesNode);

            // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties> </SigningTime>
            var signingTime = document.CreateElement(XadesSignedXml.XadesPrefix, "SigningTime", XadesSignedXml.XadesNamespaceUrl);
            signingTime.InnerText = $"{DateTime.UtcNow.ToString("s")}Z";
            signedSignaturePropertiesNode.AppendChild(signingTime);

            if (!onlyTime)
            {
                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate>
                var signingCertificateNode = document.CreateElement(XadesSignedXml.XadesPrefix, "SigningCertificate", XadesSignedXml.XadesNamespaceUrl);
                signedSignaturePropertiesNode.AppendChild(signingCertificateNode);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert>
                var certNode = document.CreateElement(XadesSignedXml.XadesPrefix, "Cert", XadesSignedXml.XadesNamespaceUrl);
                signingCertificateNode.AppendChild(certNode);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><CertDigest>
                var certDigestNode = document.CreateElement(XadesSignedXml.XadesPrefix, "CertDigest", XadesSignedXml.XadesNamespaceUrl);
                certNode.AppendChild(certDigestNode);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><CertDigest> </DigestMethod>
                var digestMethod = document.CreateElement("DigestMethod", SignedXml.XmlDsigNamespaceUrl);
                var digestMethodAlgorithmAtribute = document.CreateAttribute("Algorithm");
                digestMethodAlgorithmAtribute.InnerText = SignedXml.XmlDsigSHA256Url;
                digestMethod.Attributes.Append(digestMethodAlgorithmAtribute);
                certDigestNode.AppendChild(digestMethod);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><CertDigest> </DigestMethod>
                var digestValue = document.CreateElement("DigestValue", SignedXml.XmlDsigNamespaceUrl);
                digestValue.InnerText = Convert.ToBase64String(signingCertificate.GetCertHash());
                certDigestNode.AppendChild(digestValue);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><IssuerSerial>
                var issuerSerialNode = document.CreateElement(XadesSignedXml.XadesPrefix, "IssuerSerial", XadesSignedXml.XadesNamespaceUrl);
                certNode.AppendChild(issuerSerialNode);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><IssuerSerial> </X509IssuerName>
                var x509IssuerName = document.CreateElement("X509IssuerName", SignedXml.XmlDsigNamespaceUrl);
                x509IssuerName.InnerText = signingCertificate.Issuer;
                issuerSerialNode.AppendChild(x509IssuerName);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><IssuerSerial> </X509SerialNumber>
                var x509SerialNumber = document.CreateElement("X509SerialNumber", SignedXml.XmlDsigNamespaceUrl);
                x509SerialNumber.InnerText = ToDecimalString(signingCertificate.SerialNumber);
                issuerSerialNode.AppendChild(x509SerialNumber);
            }

            if (!onlyTime)
            {
                // <Object><QualifyingProperties><SignedProperties><SignedDataObjectProperties>
                var signedDataObjectPropertiesNode = document.CreateElement(XadesSignedXml.XadesPrefix, "SignedDataObjectProperties", XadesSignedXml.XadesNamespaceUrl);
                signedPropertiesNode.AppendChild(signedDataObjectPropertiesNode);

                // <Object><QualifyingProperties><SignedProperties><SignedDataObjectProperties><CommitmentTypeIndication>
                var commitmentTypeIndicationNode = document.CreateElement(XadesSignedXml.XadesPrefix, "CommitmentTypeIndication", XadesSignedXml.XadesNamespaceUrl);
                signedDataObjectPropertiesNode.AppendChild(commitmentTypeIndicationNode);

                // <Object><QualifyingProperties><SignedProperties><SignedDataObjectProperties><CommitmentTypeIndication><CommitmentTypeId>
                var commitmentTypeIdNode = document.CreateElement(XadesSignedXml.XadesPrefix, "CommitmentTypeId", XadesSignedXml.XadesNamespaceUrl);
                commitmentTypeIndicationNode.AppendChild(commitmentTypeIdNode);

                // <Object><QualifyingProperties><SignedProperties><SignedDataObjectProperties><CommitmentTypeIndication><CommitmentTypeId><Identifier>
                var identifierNode = document.CreateElement(XadesSignedXml.XadesPrefix, "Identifier", XadesSignedXml.XadesNamespaceUrl);
                identifierNode.InnerText = XadesSignedXml.XadesProofOfApproval;
                commitmentTypeIdNode.AppendChild(identifierNode);

                // <Object><QualifyingProperties><SignedProperties><SignedDataObjectProperties><CommitmentTypeIndication><AllSignedDataObjects>
                var allSignedDataObjectsNode = document.CreateElement(XadesSignedXml.XadesPrefix, "AllSignedDataObjects", XadesSignedXml.XadesNamespaceUrl);
                commitmentTypeIndicationNode.AppendChild(allSignedDataObjectsNode);
            }

            var dataObject = new DataObject();
            dataObject.Data = qualifyingPropertiesNode.SelectNodes(".");
            xadesSignedXml.AddObject(dataObject);
        }

        private static string ToDecimalString(string serialNumber)
        {
            BigInteger bi;

            if (BigInteger.TryParse(serialNumber, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bi))
            {
                return bi.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                return serialNumber;
            }
        }
        #endregion Private methods
    }

    internal class DigitalSignaturTimeProvider : ITimestampProvider
    {
        public DigitalSignaturTimeProvider(XmlDocument document, string rfc3161Server = "https://rfc3161.ai.moda/microsoft")
        {
            _Document = document;
            _rfc3161Server = rfc3161Server;
        }

        private XmlDocument _Document;
        private string _rfc3161Server;

        public byte[] GetTimestampFromDocumentHash(byte[] hash, string digestMethod)
        {
            // https://gist.github.com/Manouchehri/fd754e402d98430243455713efada710?permalink_comment_id=3810141

            byte[] xmlBytes = Encoding.UTF8.GetBytes(_Document.OuterXml);
            byte[] reqHash;
            using (SHA256 sha256 = SHA256.Create())
            {
                reqHash = sha256.ComputeHash(xmlBytes);
            }

            var timeStampToken = Disig.TimeStampClient.TimeStampClient.RequestTimeStampToken(_rfc3161Server, reqHash);
            return timeStampToken.ToByteArray();
        }
    }
}
