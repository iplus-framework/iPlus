using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    public interface IBindingDropHandler
    {
        void AddOrUpdateBindingWithMethod(String VBContent, bool isGlobalFunc, IACObject acObjectSource);

        void AddOrUpdateBindingWithProperty(String VBContent, IACObject acObjectSource);
    }
}
