// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
namespace gip.core.datamodel
{
    /// <summary>
    /// Result of a server method that delivers a file to the client, e.g. to open it with the viewer of the device.
    /// </summary>
    public interface IVBFileContent
    {
        string FileName { get; }
        string Extension { get; }
        string MimeType { get; }
        byte[] Content { get; }
    }
}
