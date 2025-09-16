using System.Collections.Generic;

namespace gip.core.datamodel
{
    public class GS1Model
    {
        public const char GS = (char)0x1D;

        public List<(string ai, string val, bool variable)> Items { get; set;}
        public bool IsGs1 { get; set; }
        public string RawGs1Value { get; set; }
        public string EscPosPayload { get; set; }
        public string ZplPayload { get; set; }
        public string HriText { get; set; }

    }
}