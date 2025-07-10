// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
namespace gip.core.datamodel
{
    public interface IACObjectKeyComparer : IACObjectBase
    {
        /// <summary>
        /// Helps selecting the current value via passed string key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool KeyEquals(object key);

        bool IsKey(string propertyName);
    }
}
