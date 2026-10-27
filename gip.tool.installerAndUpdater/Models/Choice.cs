// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

namespace gip.tool.installerAndUpdater.Models
{
    /// <summary>
    /// A selectable value together with the text shown in the UI.
    /// </summary>
    /// <remarks>
    /// Avalonia's <c>ComboBox</c> renders the result of <c>ToString()</c>, which
    /// for a raw enum is the identifier (e.g. <c>SqlServerAuthentication</c>).
    /// Wrapping the value keeps the enum in the model while showing readable
    /// labels.
    /// </remarks>
    public sealed class Choice<T>
    {
        public Choice(T value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public T Value { get; }

        public string DisplayName { get; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
