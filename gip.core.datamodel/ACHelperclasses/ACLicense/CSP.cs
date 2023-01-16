using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel.Licensing
{
    internal class CSP
    {
        internal static bool Verify(byte[] content, byte[] signature, string key)
        {
            using (var csp = new RSACryptoServiceProvider())
            {
                try
                {
                    csp.FromXmlString(key);
                    return csp.VerifyData(content, SHA256.Create(), signature);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("CSP", "Verify", msg);

                    return false;
                }
            }
        }

        internal static bool VerifyDSA(byte[] content, byte[] signature, string key)
        {
            using (var dsa = new DSACryptoServiceProvider())
            {
                try
                {
                    dsa.FromXmlString(key);
                    return dsa.VerifyData(content, signature);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("CSP", "VerifyDSA", msg);

                    return false;
                }
            }
        }

        internal static SignInfo SignData(byte[] content)
        {
            SignInfo info = new SignInfo();
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                info.PrivateKey = rsa.ToXmlString(true);
                info.PublicKey = rsa.ToXmlString(false);
                info.Signature = rsa.SignData(content, SHA256.Create());
            }
            return info;
        }

        internal static SignInfo SignData(byte[] content, string privateKey)
        {
            SignInfo info = new SignInfo();
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                info.PrivateKey = rsa.ToXmlString(true);
                info.PublicKey = rsa.ToXmlString(false);
                info.Signature = rsa.SignData(content, SHA256.Create());
            }
            return info;
        }

        internal static SignInfo SignDataDSA(byte[] content)
        {
            SignInfo info = new SignInfo();
            using (DSACryptoServiceProvider rsa = new DSACryptoServiceProvider())
            {
                info.PrivateKey = rsa.ToXmlString(true);
                info.PublicKey = rsa.ToXmlString(false);
                info.Signature = rsa.SignData(content);
            }
            return info;
        }
    }

    public class SignInfo
    {
        public string PrivateKey
        {
            get;
            set;
        }

        public string PublicKey
        {
            get;
            set;
        }

        public byte[] Signature
        {
            get;
            set;
        }
    }

    internal class VBCryptoStream : CryptoStream
    {
        private readonly Stream underlyingStream;
        public VBCryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode) : base(stream, transform, mode)
        {
            underlyingStream = stream;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
            }
            catch (CryptographicException ce)
            {
                if (disposing)
                {
                    this.underlyingStream.Dispose();
                }

                string msg = ce.Message;
                if (ce.InnerException != null && ce.InnerException.Message != null)
                    msg += " Inner:" + ce.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("CSP", "Dispose", msg);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("CSP", "Dispose(1)", msg);
            }
        }
    }
}
