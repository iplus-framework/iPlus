using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACChildItem'}de{'ACChildItem'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACChildItem<T> where T : IACComponent
    {
        public ACChildItem(IACComponent component, string instanceName)
        {
            ParentComponent = component;
            InstanceName = instanceName;
        }

        #region properties

        [ACPropertyInfo(9999)]
        [NotMapped]
        public string InstanceName { get; set; }

        public IACComponent ParentComponent { get; set; }

        private T value;
        [ACPropertyInfo(9999)]
        [NotMapped]
        public T Value
        {
            get
            {
                if (value == null)
                {
                    IACComponent item = ParentComponent.FindChildComponents<T>((IACComponent c) => c.ACIdentifier.StartsWith(InstanceName)).FirstOrDefault();
                    if (item == null)
                        item = ParentComponent.ACUrlCommand("?" + InstanceName) as IACComponent;
                    if (item == null)
                        item = ParentComponent.StartComponent(InstanceName, null, new object[] { });
                    if (item != null)
                        value = (T)item;
                }
                return value;
            }
        }
        #endregion
    }
}
