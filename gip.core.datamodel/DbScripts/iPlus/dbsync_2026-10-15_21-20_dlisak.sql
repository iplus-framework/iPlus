-- dbsync 2026-10-15 dlisak
-- TLS configuration for the HTTP/WebSocket host (WCFServiceManager + browser WCF
-- transport), docs SqlTunnel-OptionB-Implementation.md §9.28.
-- ServiceUseTLS: encrypt the HTTP host (https/wss). When enabled, browser WCF clients
--   connect via wss://host:ServicePortHTTP/wcfbrowser and desktop HTTP-only clients via
--   https://. The SQL tunnel relay (PASqlTunnelHost) reads its own UseTls config.
-- ServiceTlsCertificatePath: path to a PFX/PKCS#12 certificate. Empty + ServiceUseTLS=1
--   -> a self-signed certificate is generated and persisted next to the executable
--   (iplus-relay-tls.pfx). Production: CA-issued certificate.
-- ServiceTlsCertificatePassword: password of the PFX file (empty = "iplus" default for
--   auto-generated certificates).
ALTER TABLE [dbo].[VBUserInstance] ADD [ServiceUseTLS] [bit] NOT NULL DEFAULT 0;
ALTER TABLE [dbo].[VBUserInstance] ADD [ServiceTlsCertificatePath] [nvarchar](500) NULL;
ALTER TABLE [dbo].[VBUserInstance] ADD [ServiceTlsCertificatePassword] [nvarchar](500) NULL;
