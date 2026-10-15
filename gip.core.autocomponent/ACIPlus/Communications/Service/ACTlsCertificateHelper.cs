// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace gip.core.autocomponent
{
    /// <summary>
    /// TLS certificate handling for the server-side HTTP/WebSocket hosts (SQL tunnel relay
    /// <see cref="PASqlTunnelHost"/> and <see cref="WCFServiceManager"/>, docs
    /// SqlTunnel-OptionB-Implementation.md §9.28).
    ///
    /// Loads a PFX/PKCS#12 certificate from disk, or — when none exists yet — generates a
    /// self-signed certificate (RSA-2048, SHA-256, SANs for the machine name, localhost and
    /// all IPv4 addresses) and persists it, so the same certificate is served across
    /// restarts and clients can pin it. Intended for LAN/dev deployments; production should
    /// use a CA-issued certificate via <c>TlsCertificatePath</c>.
    /// </summary>
    public static class ACTlsCertificateHelper
    {
        /// <summary>
        /// Returns the configured certificate. When <paramref name="certPath"/> is empty the
        /// default location next to the executable is used
        /// (<c>iplus-relay-tls.pfx</c>); a missing file is generated as self-signed.
        /// </summary>
        /// <param name="certPath">PFX file path (empty = default location).</param>
        /// <param name="certPassword">PFX password (empty = default "iplus").</param>
        /// <param name="logContext">Name used in console/log output.</param>
        /// <returns>The certificate, or null when loading/generation failed.</returns>
        public static X509Certificate2 GetOrCreateCertificate(string certPath, string certPassword, string logContext)
        {
            try
            {
                if (String.IsNullOrEmpty(certPassword))
                    certPassword = "iplus";
                if (String.IsNullOrEmpty(certPath))
                    certPath = Path.Combine(AppContext.BaseDirectory, "iplus-relay-tls.pfx");

                if (File.Exists(certPath))
                {
                    X509Certificate2 cert = new X509Certificate2(certPath, certPassword);
                    Console.WriteLine($"{logContext}: TLS certificate loaded from '{certPath}' (subject '{cert.Subject}', thumbprint {cert.Thumbprint}, expires {cert.NotAfter:yyyy-MM-dd}).");
                    return cert;
                }

                X509Certificate2 generated = CreateSelfSignedCertificate();
                byte[] pfx = generated.Export(X509ContentType.Pfx, certPassword);
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(certPath)));
                File.WriteAllBytes(certPath, pfx);
                Console.WriteLine($"{logContext}: self-signed TLS certificate generated and saved to '{certPath}' (subject '{generated.Subject}', thumbprint {generated.Thumbprint}, expires {generated.NotAfter:yyyy-MM-dd}).");
                Console.WriteLine($"{logContext}: distribute the certificate (or its thumbprint) to clients, or install it into their trust store, to avoid certificate warnings.");
                // Reload from the persisted PFX so the key is bound to the file (stable across restarts).
                return new X509Certificate2(certPath, certPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{logContext}: TLS certificate could not be loaded/generated. {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>Generates a self-signed server certificate with useful SANs.</summary>
        private static X509Certificate2 CreateSelfSignedCertificate()
        {
            string hostName = Dns.GetHostName();
            using RSA rsa = RSA.Create(2048);
            CertificateRequest request = new CertificateRequest(
                new X500DistinguishedName("CN=" + hostName), rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, true)); // serverAuth

            SubjectAlternativeNameBuilder san = new SubjectAlternativeNameBuilder();
            san.AddDnsName(hostName);
            san.AddDnsName("localhost");
            try
            {
                foreach (IPAddress ip in Dns.GetHostEntry(hostName).AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        san.AddIpAddress(ip);
                }
            }
            catch { /* DNS failures must not block certificate generation */ }
            request.CertificateExtensions.Add(san.Build());

            // 397 days: the maximum lifetime macOS/iOS accept without extra configuration.
            using X509Certificate2 cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(397));
            return new X509Certificate2(cert.Export(X509ContentType.Pfx, "iplus"), "iplus");
        }
    }
}
