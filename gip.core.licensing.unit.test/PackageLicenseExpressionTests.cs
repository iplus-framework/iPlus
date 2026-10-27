// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using gip.core.licensing;
using Xunit;

namespace gip.core.licensing.unit.test
{
    /// <summary>
    /// Tests for the SPDX-based package license classification and the
    /// GPL/commercial mixing rule.
    /// </summary>
    public class PackageLicenseExpressionTests
    {
        [Theory]
        [InlineData("GPL-3.0-or-later", LicenseCompatibility.OpenOnly)]
        [InlineData("GPL-3.0-only", LicenseCompatibility.OpenOnly)]
        [InlineData("AGPL-3.0-or-later", LicenseCompatibility.OpenOnly)]
        [InlineData("LGPL-3.0-or-later", LicenseCompatibility.OpenOnly)]
        [InlineData("MPL-2.0", LicenseCompatibility.OpenOnly)]
        [InlineData("Commercial", LicenseCompatibility.CommercialOnly)]
        [InlineData("LicenseRef-Commercial", LicenseCompatibility.CommercialOnly)]
        [InlineData("LicenseRef-Commercial OR MIT", LicenseCompatibility.Both)]
        [InlineData("LicenseRef-Commercial OR GPL-3.0-or-later", LicenseCompatibility.Both)]
        [InlineData("MIT", LicenseCompatibility.Both)]
        [InlineData("Apache-2.0", LicenseCompatibility.Both)]
        [InlineData("BSD-3-Clause", LicenseCompatibility.Both)]
        [InlineData("GPL-3.0-or-later OR LicenseRef-Commercial", LicenseCompatibility.Both)]
        [InlineData("GPL-3.0-or-later OR Commercial", LicenseCompatibility.Both)]
        [InlineData("(GPL-3.0-or-later OR LicenseRef-Commercial)", LicenseCompatibility.Both)]
        [InlineData("MIT OR GPL-3.0-or-later", LicenseCompatibility.Both)]
        [InlineData("", LicenseCompatibility.Unknown)]
        [InlineData("SomeUnknownLicense", LicenseCompatibility.Unknown)]
        public void Classify_ReturnsExpectedCompatibility(string expression, LicenseCompatibility expected)
        {
            Assert.Equal(expected, PackageLicenseExpression.Classify(expression));
        }

        [Theory]
        [InlineData("GPL-3.0-or-later", false)]
        [InlineData("AGPL-3.0-or-later", false)]
        [InlineData("Commercial", true)]
        [InlineData("LicenseRef-Commercial", true)]
        [InlineData("MIT", true)]
        [InlineData("Apache-2.0", true)]
        [InlineData("GPL-3.0-or-later OR LicenseRef-Commercial", true)]
        [InlineData("GPL-3.0-or-later OR Commercial", true)]
        [InlineData("", true)]
        [InlineData("SomeUnknownLicense", true)]
        public void IsAllowedUnderCommercialRuntime_EnforcesMixingRule(string expression, bool expected)
        {
            Assert.Equal(expected, PackageLicenseExpression.IsAllowedUnderCommercialRuntime(expression));
        }

        [Theory]
        [InlineData("GPL-3.0-or-later", false)]
        [InlineData("GPL-3.0-or-later OR LicenseRef-Commercial", true)]
        [InlineData("MIT", true)]
        [InlineData("Commercial", true)]
        [InlineData("LicenseRef-Commercial", true)]
        [InlineData("", true)]
        public void IsAllowedUnderCommercialRuntime_AllowsEverythingButPureCopyleft(string expression, bool expected)
        {
            Assert.Equal(expected, PackageLicenseExpression.IsAllowedUnderCommercialRuntime(expression));
        }

        [Fact]
        public void SplitAlternatives_HandlesParenthesesAndWith()
        {
            var parts = PackageLicenseExpression.SplitAlternatives("(GPL-3.0-or-later WITH Classpath-exception-2.0 OR MIT)");

            Assert.Equal(2, parts.Count);
            Assert.Equal("GPL-3.0-or-later", parts[0]);
            Assert.Equal("MIT", parts[1]);
        }

        [Fact]
        public void SplitAlternatives_EmptyExpression_ReturnsEmpty()
        {
            Assert.Empty(PackageLicenseExpression.SplitAlternatives(null!));
            Assert.Empty(PackageLicenseExpression.SplitAlternatives("   "));
        }

        [Fact]
        public void IsCommercial_DetectsBothSpellings()
        {
            Assert.True(PackageLicenseExpression.IsCommercial("Commercial"));
            Assert.True(PackageLicenseExpression.IsCommercial("LicenseRef-Commercial"));
            Assert.True(PackageLicenseExpression.IsCommercial("GPL-3.0-or-later OR LicenseRef-Commercial"));
            Assert.False(PackageLicenseExpression.IsCommercial("GPL-3.0-or-later"));
            Assert.False(PackageLicenseExpression.IsCommercial("MIT"));
        }

        [Fact]
        public void DualLicensedExpression_IsTheDocumentedDefault()
        {
            Assert.Equal("GPL-3.0-or-later OR LicenseRef-Commercial", PackageLicenseExpression.DualLicensedExpression);
            Assert.Equal(LicenseCompatibility.Both, PackageLicenseExpression.Classify(PackageLicenseExpression.DualLicensedExpression));
        }
    }
}
