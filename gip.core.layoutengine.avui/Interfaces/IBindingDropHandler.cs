using gip.core.datamodel;
using System;

namespace gip.core.layoutengine.avui
{
    public interface IBindingDropHandler
    {
        void AddOrUpdateBindingWithMethod(String VBContent, bool isGlobalFunc, IACObject acObjectSource);

        void AddOrUpdateBindingWithProperty(String VBContent, IACObject acObjectSource);
    }
}
