// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using gip.core.licensing;
using Xunit;

namespace gip.core.licensing.unit.test
{
    /// <summary>
    /// Tests for the signed license file format: issuing, parsing and verification.
    /// </summary>
    public class LicenseFileTests
    {
        private static (LicenseIssuer issuer, LicenseVerifier verifier) CreatePair()
        {
            RSA key = LicenseKeys.CreateKeyPair();
            return (new LicenseIssuer(key), new LicenseVerifier(key));
        }

        private static LicensePayload CreateProductPayload()
        {
            return new LicensePayload
            {
                Kind = LicenseKind.Product,
                CustomerName = "ACME GmbH",
                ProjectNo = "P-2026-001",
                IssuerName = "ACME GmbH",
                Packages = { "ACME.Mes", "ACME.Mes.Extension" },
                DatabaseFingerprint = "ABCDEF0123456789"
            };
        }

        [Fact]
        public void Issue_ThenVerify_RoundTrips()
        {
            var (issuer, verifier) = CreatePair();
            LicensePayload payload = CreateProductPayload();

            string text = issuer.Issue(payload);
            LicenseValidationResult result = verifier.Verify(text);

            Assert.True(result.IsValid, result.ToString());
            Assert.Equal(LicenseKind.Product, result.Payload!.Kind);
            Assert.Equal("ACME GmbH", result.Payload.CustomerName);
            Assert.Equal(2, result.Payload.Packages.Count);
        }

        [Fact]
        public void Verify_WithMatchingDatabaseFingerprint_Succeeds()
        {
            var (issuer, verifier) = CreatePair();
            string text = issuer.Issue(CreateProductPayload());

            var context = new LicenseBindingContext { DatabaseFingerprint = "ABCDEF0123456789" };
            LicenseValidationResult result = verifier.Verify(text, context);

            Assert.True(result.IsValid, result.ToString());
        }

        [Fact]
        public void Verify_WithDifferentDatabaseFingerprint_Fails()
        {
            var (issuer, verifier) = CreatePair();
            string text = issuer.Issue(CreateProductPayload());

            var context = new LicenseBindingContext { DatabaseFingerprint = "0000000000000000" };
            LicenseValidationResult result = verifier.Verify(text, context);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.DatabaseMismatch, result.Error);
        }

        [Fact]
        public void Verify_WithDifferentMachineFingerprint_Fails()
        {
            var (issuer, verifier) = CreatePair();
            LicensePayload payload = CreateProductPayload();
            payload.MachineFingerprint = "MACHINE-A";
            string text = issuer.Issue(payload);

            var context = new LicenseBindingContext
            {
                DatabaseFingerprint = "ABCDEF0123456789",
                MachineFingerprint = "MACHINE-B"
            };
            LicenseValidationResult result = verifier.Verify(text, context);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.MachineMismatch, result.Error);
        }

        [Fact]
        public void Verify_WithUnlicensedPackage_Fails()
        {
            var (issuer, verifier) = CreatePair();
            string text = issuer.Issue(CreateProductPayload());

            var context = new LicenseBindingContext
            {
                DatabaseFingerprint = "ABCDEF0123456789",
                RequiredPackage = "ACME.Other"
            };
            LicenseValidationResult result = verifier.Verify(text, context);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.PackageNotLicensed, result.Error);
        }

        [Fact]
        public void Verify_WithLicensedPackage_Succeeds()
        {
            var (issuer, verifier) = CreatePair();
            string text = issuer.Issue(CreateProductPayload());

            var context = new LicenseBindingContext
            {
                DatabaseFingerprint = "ABCDEF0123456789",
                RequiredPackage = "acme.mes"
            };
            LicenseValidationResult result = verifier.Verify(text, context);

            Assert.True(result.IsValid, result.ToString());
        }

        [Fact]
        public void Verify_DeveloperLicense_CoversEveryPackage()
        {
            var (issuer, verifier) = CreatePair();
            LicensePayload payload = new LicensePayload
            {
                Kind = LicenseKind.DeveloperIssuer,
                CustomerName = "ACME GmbH",
                IssuerName = "gip"
            };
            string text = issuer.Issue(payload);

            var context = new LicenseBindingContext { RequiredPackage = "Anything.At.All" };
            LicenseValidationResult result = verifier.Verify(text, context);

            Assert.True(result.IsValid, result.ToString());
        }

        [Fact]
        public void Verify_ExpiredLicense_Fails()
        {
            var (issuer, verifier) = CreatePair();
            LicensePayload payload = CreateProductPayload();
            payload.ExpiresUtc = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
            string text = issuer.Issue(payload);

            LicenseValidationResult result = verifier.Verify(text, nowUtc: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.Expired, result.Error);
        }

        [Fact]
        public void Verify_TamperedPayload_Fails()
        {
            var (issuer, verifier) = CreatePair();
            string text = issuer.Issue(CreateProductPayload());

            // Change the customer name inside the signed payload.
            string tampered = text.Replace("ACME GmbH", "EVIL Corp");

            LicenseValidationResult result = verifier.Verify(tampered);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.InvalidSignature, result.Error);
        }

        [Fact]
        public void Verify_WithForeignPublicKey_Fails()
        {
            var (issuer, _) = CreatePair();
            RSA otherKey = LicenseKeys.CreateKeyPair();
            var foreignVerifier = new LicenseVerifier(otherKey);

            string text = issuer.Issue(CreateProductPayload());
            LicenseValidationResult result = foreignVerifier.Verify(text);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.InvalidSignature, result.Error);
        }

        [Fact]
        public void Verify_MissingHeader_Fails()
        {
            var (_, verifier) = CreatePair();

            LicenseValidationResult result = verifier.Verify("not a license file");

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.Malformed, result.Error);
        }

        [Fact]
        public void Verify_EmptyText_Fails()
        {
            var (_, verifier) = CreatePair();

            LicenseValidationResult result = verifier.Verify(string.Empty);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.Malformed, result.Error);
        }

        [Fact]
        public void Verify_UnsupportedFormatVersion_Fails()
        {
            var (issuer, verifier) = CreatePair();
            LicensePayload payload = CreateProductPayload();
            payload.FormatVersion = 999;
            string text = issuer.Issue(payload);

            LicenseValidationResult result = verifier.Verify(text);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.UnsupportedFormatVersion, result.Error);
        }

        [Fact]
        public void Verify_WrongLicenseKind_Fails()
        {
            var (issuer, verifier) = CreatePair();
            string text = issuer.Issue(CreateProductPayload());

            var context = new LicenseBindingContext
            {
                DatabaseFingerprint = "ABCDEF0123456789",
                RequiredKind = LicenseKind.DeveloperIssuer
            };
            LicenseValidationResult result = verifier.Verify(text, context);

            Assert.False(result.IsValid);
            Assert.Equal(LicenseValidationError.WrongLicenseKind, result.Error);
        }

        [Fact]
        public void Verify_WindowsLineEndings_StillParse()
        {
            var (issuer, verifier) = CreatePair();
            string text = issuer.Issue(CreateProductPayload()).Replace("\n", "\r\n");

            LicenseValidationResult result = verifier.Verify(text);

            Assert.True(result.IsValid, result.ToString());
        }

        [Fact]
        public void IssueToFile_ThenVerifyFile_RoundTrips()
        {
            var (issuer, verifier) = CreatePair();
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() + LicenseFormat.FileExtension);
            try
            {
                issuer.IssueToFile(CreateProductPayload(), path);
                LicenseValidationResult result = verifier.VerifyFile(path);
                Assert.True(result.IsValid, result.ToString());
            }
            finally
            {
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
        }

        [Fact]
        public void Issuer_GetPublicKeyPem_VerifiesIssuedLicenses()
        {
            var (issuer, _) = CreatePair();
            string text = issuer.Issue(CreateProductPayload());

            var verifier = LicenseVerifier.FromPublicKeyText(issuer.GetPublicKeyPem());
            LicenseValidationResult result = verifier.Verify(text);

            Assert.True(result.IsValid, result.ToString());
        }
    }
}
