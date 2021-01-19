using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IVBFindAndReplaceDBSearch
    {
        IEnumerable<IACObjectEntityWithCheckTrans> FARSearchInDB(VBBSOFindAndReplace bso, string wordToFind);
        bool IsEnabledFARSearchInDB(VBBSOFindAndReplace bso);
        void FARSearchItemSelected(VBBSOFindAndReplace bso, IACObjectEntityWithCheckTrans item);
    }
}
