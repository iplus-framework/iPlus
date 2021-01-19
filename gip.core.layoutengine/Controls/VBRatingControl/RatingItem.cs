using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a rating item in <see cref="VBRatingControl"/>.
    /// </summary>
    public class RatingItem
    {
        public int Sn { get; set; }
        public bool IsSelected { get; set; }
    }
}
