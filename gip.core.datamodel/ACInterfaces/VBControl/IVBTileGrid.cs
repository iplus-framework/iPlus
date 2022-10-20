using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface IVBTileGrid
    {
        Nullable<short> TileColumn { get; set; }

        short TileRow {get; set;}

        string Title { get; set; }

        string ACUrl { get; set; }

        ACValueList Parameters { get; set; }

        string IconACUrl { get; set; }

        Global.VBTileType VBTileType { get; set; }
    }
}
