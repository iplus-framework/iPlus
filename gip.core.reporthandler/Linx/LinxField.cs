﻿
using System.Collections.Generic;
using System.Text;

namespace gip.core.reporthandler
{
    public class LinxField
    {

        #region ctor's

        #endregion

        #region Properties

        public LinxFieldHeader Header { get; set; } = new LinxFieldHeader();

        public string Value { get; set; }

        #endregion

        #region Methods

        public List<byte[]> GetBytes(Encoding encoding)
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
            bytes.Add(new byte[] { Header.TextLenght });
            bytes.Add(new byte[] { Header.Format1 });
            bytes.Add(new byte[] { Header.Format2 });
            bytes.Add(new byte[] { Header.Linkage });
            bytes.Add(Header.DataSetName);

            byte[] messageByte = encoding.GetBytes(Value);
            bytes.Add(messageByte);

            return bytes;
        }

        #endregion
    }
}
