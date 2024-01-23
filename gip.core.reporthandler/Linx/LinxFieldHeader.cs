namespace gip.core.reporthandler
{
    public class LinxFieldHeader
    {
        //    Field header character	1 byte
        //    Field type	1 byte
        //    Field length in bytes	2 bytes
        //    Y position	1 byte
        //    X position	2 bytes
        //    Field length in rasters	2 bytes
        //    Field height in drops	1 byte
        //    Format 3	1 byte
        //    Bold multiplier	1 byte
        //    Text length	1 byte
        //    Format 1	1 byte
        //    Format 2	1 byte
        //    Linkage	1 byte*
        //    Data set name	15 bytes + null*

        public LinxFieldHeader()
        {
            // Setup defaults

            FieldHeaderCharacter = 0x1C;
            FieldType = 0x00;
            YPosition = 0x00;
            XPosition = new byte[] { 0x00, 0x00 };
            FieldHeightInDrops = 0x07;
            Format3 = 0x00;
            BoldMultiplier = 0x01;
            Format1 = 0x00;
            Format2 = 0x00;
            Linkage = 0x00;
        }

        public byte FieldHeaderCharacter { get; set; }
        public byte FieldType { get; set; }
        public byte[] FieldLenghtInBytes { get; set; }
        public byte YPosition { get; set; }
        public byte[] XPosition { get; set; }
        public byte[] FieldLengthInRasters { get; set; }
        public byte FieldHeightInDrops { get; set; }
        public byte Format3 { get; set; }
        public byte BoldMultiplier { get; set; }
        public byte TextLenght { get; set; }
        public byte Format1 { get; set; }
        public byte Format2 { get; set; }
        public byte Linkage { get; set; }
        public byte[] DataSetName { get; set; }

    }
}
