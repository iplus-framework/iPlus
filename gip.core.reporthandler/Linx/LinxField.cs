
using System.Collections.Generic;

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

        public List<byte> GetBytes()
        {
            List<byte> bytes = new List<byte>();

            bytes.Add(Header.FieldHeaderCharacter);
            bytes.Add(Header.FieldType);
            bytes.AddRange(Header.FieldLenghtInBytes);
            bytes.Add(Header.YPosition);
            bytes.AddRange(Header.XPosition);
            bytes.AddRange(Header.FieldLengthInRasters);
            bytes.Add(Header.FieldHeightInDrops);
            bytes.Add(Header.Format3);
            bytes.Add(Header.BoldMultiplier);
            bytes.Add(Header.TextLenght);
            bytes.Add(Header.Format1);
            bytes.Add(Header.Format2);
            bytes.Add(Header.Linkage);
            bytes.AddRange(Header.DataSetName);

            return bytes;
        }

        #endregion
    }
}
