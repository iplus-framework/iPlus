// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿
using System.Collections.Generic;
using System.Text;

namespace gip.core.reporthandler.avui
{
    public class LinxField
    {

        #region ctor's

        #endregion

        #region Properties

        public LinxFieldHeader Header { get; set; } = new LinxFieldHeader();

        public string Value { get; set; }
        public byte[] ValueByte { get; set; }

        #endregion

        #region Methods

        public List<byte[]> GetBytes()
        {
            List<byte[]> bytes = new List<byte[]>();

            bytes.Add(new byte[]{Header.FieldHeaderCharacter });
            bytes.Add(new byte[] { Header.FieldType });
            bytes.Add(Header.FieldLengthInBytes);
            bytes.Add(new byte[] { Header.YPosition });
            bytes.Add(Header.XPosition);
            bytes.Add(Header.FieldLengthInRasters);
            bytes.Add(new byte[] { Header.FieldHeightInDrops });
            bytes.Add(new byte[] { Header.Format3 });
            bytes.Add(new byte[] { Header.BoldMultiplier });
            bytes.Add(new byte[] { Header.TextLength });
            bytes.Add(new byte[] { Header.Format1 });
            bytes.Add(new byte[] { Header.Format2 });
            bytes.Add(new byte[] { Header.Linkage });
            bytes.Add(Header.DataSetName);
            bytes.Add(ValueByte);

            return bytes;
        }

        #endregion
    }
}
