// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using gip.core.licensing;
using Xunit;

namespace gip.core.licensing.unit.test
{
    /// <summary>
    /// Tests for Authenticode publisher verification and the trusted-publisher
    /// store.
    /// </summary>
    public class AuthenticodeVerifierTests
    {
        [Fact]
        public void Verify_MissingFile_ReportsNotFound()
        {
            AuthenticodeResult result = AuthenticodeVerifier.Verify("/nonexistent/file.dll");

            Assert.False(result.IsSigned);
            Assert.False(result.IsTrusted);
            Assert.Contains("not found", result.Status, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Verify_EmptyPath_ReportsNotFound()
        {
            AuthenticodeResult result = AuthenticodeVerifier.Verify(string.Empty);

            Assert.False(result.IsSigned);
            Assert.Contains("not found", result.Status, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Verify_OnNonWindows_ReportsPlatformUnsupported()
        {
            if (OperatingSystem.IsWindows())
                return; // Covered by the Windows-specific tests below.

            string path = typeof(AuthenticodeVerifierTests).Assembly.Location;
            AuthenticodeResult result = AuthenticodeVerifier.Verify(path);

            Assert.True(result.IsPlatformUnsupported);
            Assert.False(result.IsSigned);
        }

        [Theory]
        [InlineData("a1b2c3", "A1B2C3")]
        [InlineData("A1:B2:C3", "A1B2C3")]
        [InlineData("a1 b2 c3", "A1B2C3")]
        [InlineData("a1-b2-c3", "A1B2C3")]
        [InlineData("", "")]
        public void NormalizeThumbprint_StripsSeparatorsAndUppercases(string input, string expected)
        {
            Assert.Equal(expected, AuthenticodeVerifier.NormalizeThumbprint(input));
        }

        [Fact]
        public void NormalizeThumbprint_NullInput_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, AuthenticodeVerifier.NormalizeThumbprint(null!));
        }

        [Fact]
        public void IsSignedByAnyOf_NullList_ReturnsFalse()
        {
            string path = typeof(AuthenticodeVerifierTests).Assembly.Location;
            Assert.False(AuthenticodeVerifier.IsSignedByAnyOf(path, null!));
        }

        [Fact]
        public void IsSignedByAnyOf_EmptyList_ReturnsFalse()
        {
            string path = typeof(AuthenticodeVerifierTests).Assembly.Location;
            Assert.False(AuthenticodeVerifier.IsSignedByAnyOf(path, Array.Empty<string>()));
        }
    }

    /// <summary>
    /// Tests for <see cref="TrustedPublisherStore"/>.
    /// </summary>
    public class TrustedPublisherStoreTests
    {
        [Fact]
        public void DefaultMode_IsOff()
        {
            TrustedPublisherStore.Reset();
            Assert.Equal(PublisherVerificationMode.Off, TrustedPublisherStore.GetMode());
        }

        [Fact]
        public void IsAllowed_WhenOff_AlwaysAllows()
        {
            TrustedPublisherStore.Reset();
            TrustedPublisherStore.SetMode(PublisherVerificationMode.Off);

            bool allowed = TrustedPublisherStore.IsAllowed(
                typeof(TrustedPublisherStoreTests).Assembly.Location, out string reason);

            Assert.True(allowed);
            Assert.Contains("off", reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IsAllowed_OnNonWindows_AlwaysAllows()
        {
            if (OperatingSystem.IsWindows())
                return;

            TrustedPublisherStore.Reset();
            TrustedPublisherStore.SetMode(PublisherVerificationMode.Enforce);

            bool allowed = TrustedPublisherStore.IsAllowed(
                typeof(TrustedPublisherStoreTests).Assembly.Location, out string reason);

            Assert.True(allowed);
            Assert.Contains("not available", reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IsAllowed_EnforceWithoutTrustedPublishers_Allows()
        {
            if (!OperatingSystem.IsWindows())
                return;

            TrustedPublisherStore.Reset();
            TrustedPublisherStore.SetMode(PublisherVerificationMode.Enforce);
            TrustedPublisherStore.SetTrustedThumbprints(Array.Empty<string>());

            bool allowed = TrustedPublisherStore.IsAllowed(
                typeof(TrustedPublisherStoreTests).Assembly.Location, out string reason);

            // An empty list must not lock the runtime out of every assembly.
            Assert.True(allowed);
            Assert.Contains("No trusted publishers", reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void SetTrustedThumbprints_NormalizesAndDeduplicates()
        {
            TrustedPublisherStore.Reset();
            TrustedPublisherStore.SetTrustedThumbprints(new[]
            {
                "a1:b2:c3",
                "A1B2C3",
                "  ",
                "d4e5f6"
            });

            var thumbprints = TrustedPublisherStore.GetTrustedThumbprints();

            Assert.Equal(2, thumbprints.Count);
            Assert.Contains("A1B2C3", thumbprints);
            Assert.Contains("D4E5F6", thumbprints);
        }

        [Fact]
        public void SetMode_IsHonoured()
        {
            TrustedPublisherStore.Reset();
            TrustedPublisherStore.SetMode(PublisherVerificationMode.Warn);

            Assert.Equal(PublisherVerificationMode.Warn, TrustedPublisherStore.GetMode());
        }

        [Fact]
        public void Reset_ClearsConfiguration()
        {
            TrustedPublisherStore.SetMode(PublisherVerificationMode.Enforce);
            TrustedPublisherStore.SetTrustedThumbprints(new[] { "A1B2C3" });

            TrustedPublisherStore.Reset();

            Assert.Equal(PublisherVerificationMode.Off, TrustedPublisherStore.GetMode());
            Assert.Empty(TrustedPublisherStore.GetTrustedThumbprints());
        }
    }
}
