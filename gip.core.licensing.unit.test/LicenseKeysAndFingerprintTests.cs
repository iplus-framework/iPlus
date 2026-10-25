// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using gip.core.licensing;
using Xunit;

namespace gip.core.licensing.unit.test
{
    /// <summary>
    /// Tests for key import/export and the fingerprint helpers.
    /// </summary>
    public class LicenseKeysAndFingerprintTests
    {
        [Fact]
        public void PemRoundTrip_PrivateKey()
        {
            using RSA key = LicenseKeys.CreateKeyPair();
            string pem = LicenseKeys.ExportPrivateKeyPem(key);

            using RSA imported = LicenseKeys.ImportPrivateKey(pem);

            Assert.Equal(key.KeySize, imported.KeySize);
            Assert.Equal(LicenseKeys.ExportPublicKeyPem(key), LicenseKeys.ExportPublicKeyPem(imported));
        }

        [Fact]
        public void PemRoundTrip_PublicKey()
        {
            using RSA key = LicenseKeys.CreateKeyPair();
            string pem = LicenseKeys.ExportPublicKeyPem(key);

            using RSA imported = LicenseKeys.ImportPublicKey(pem);

            Assert.Equal(key.KeySize, imported.KeySize);
        }

        [Fact]
        public void LegacyXmlKey_IsAccepted()
        {
            using RSA key = LicenseKeys.CreateKeyPair();
#pragma warning disable SYSLIB0021
            string xml = key.ToXmlString(false);
#pragma warning restore SYSLIB0021

            using RSA imported = LicenseKeys.ImportPublicKey(xml);

            Assert.Equal(key.KeySize, imported.KeySize);
        }

        [Fact]
        public void IsPem_DetectsFormat()
        {
            using RSA key = LicenseKeys.CreateKeyPair();

            Assert.True(LicenseKeys.IsPem(LicenseKeys.ExportPublicKeyPem(key)));
#pragma warning disable SYSLIB0021
            Assert.False(LicenseKeys.IsPem(key.ToXmlString(false)));
#pragma warning restore SYSLIB0021
        }

        [Fact]
        public void ImportPrivateKey_EmptyText_Throws()
        {
            Assert.Throws<ArgumentException>(() => LicenseKeys.ImportPrivateKey(""));
        }

        [Fact]
        public void DatabaseFingerprint_IsStable()
        {
            DateTime createDate = new DateTime(2026, 1, 2, 3, 4, 5, 678);
            Guid broker = Guid.Parse("11111111-2222-3333-4444-555555555555");
            Guid db = Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa");

            string first = DatabaseFingerprint.Compute(createDate, broker, db);
            string second = DatabaseFingerprint.Compute(createDate, broker, db);

            Assert.Equal(first, second);
            Assert.Equal(64, first.Length); // SHA-256 hex
        }

        [Fact]
        public void DatabaseFingerprint_DiffersForDifferentDatabases()
        {
            DateTime createDate = new DateTime(2026, 1, 2, 3, 4, 5, 678);
            Guid broker = Guid.Parse("11111111-2222-3333-4444-555555555555");

            string a = DatabaseFingerprint.Compute(createDate, broker, Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa"));
            string b = DatabaseFingerprint.Compute(createDate, broker, Guid.Parse("66666666-7777-8888-9999-bbbbbbbbbbbb"));

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void DatabaseFingerprint_FromLegacyChecksumCode_IsStable()
        {
            string legacy = "12-34-56-78";

            Assert.Equal(
                DatabaseFingerprint.FromLegacyChecksumCode(legacy),
                DatabaseFingerprint.FromLegacyChecksumCode(" " + legacy + " "));
        }

        [Fact]
        public void DatabaseFingerprint_EmptyLegacyCode_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, DatabaseFingerprint.FromLegacyChecksumCode(null!));
            Assert.Equal(string.Empty, DatabaseFingerprint.FromLegacyChecksumCode("   "));
        }

        [Fact]
        public void MachineFingerprint_IsOrderIndependent()
        {
            string a = MachineFingerprint.Compute(new[] { "host", "x64", "disk1" });
            string b = MachineFingerprint.Compute(new[] { "disk1", "X64", "HOST" });

            Assert.Equal(a, b);
        }

        [Fact]
        public void MachineFingerprint_IgnoresEmptyComponents()
        {
            string a = MachineFingerprint.Compute(new[] { "host", "", "  ", "disk1" });
            string b = MachineFingerprint.Compute(new[] { "host", "disk1" });

            Assert.Equal(a, b);
        }

        [Fact]
        public void MachineFingerprint_ComputeCurrent_ReturnsHash()
        {
            string fingerprint = MachineFingerprint.ComputeCurrent();

            Assert.False(string.IsNullOrWhiteSpace(fingerprint));
            Assert.Equal(64, fingerprint.Length);
        }

        [Fact]
        public void FrameworkLicenseKeys_EmbeddedPublicKey_IsAvailable()
        {
            FrameworkLicenseKeys.ResetCache();

            using RSA? key = FrameworkLicenseKeys.TryGetPublicKey();

            Assert.NotNull(key);
            Assert.Equal(2048, key!.KeySize);
        }

        [Fact]
        public void FrameworkLicenseKeys_CreateVerifier_UsesEmbeddedKey()
        {
            FrameworkLicenseKeys.ResetCache();

            LicenseVerifier verifier = FrameworkLicenseKeys.CreateVerifier();

            Assert.NotNull(verifier);
        }
    }
}
