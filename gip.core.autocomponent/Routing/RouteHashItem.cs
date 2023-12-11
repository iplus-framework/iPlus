using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RouteHashItem'}de{'RouteHashItem'}", Global.ACKinds.TACSimpleClass)]
    public class RouteHashItem : ICloneable
    {
        public RouteHashItem()
        {

        }

        public RouteHashItem(List<int> routeHashCodes)
        {
            RouteHashCodes = new SafeList<int>(routeHashCodes);
            UseFactor = 1;
        }

        private SafeList<int> _RouteHashCodes;
        public SafeList<int> RouteHashCodes
        {
            get => _RouteHashCodes;
            set => _RouteHashCodes = value;
        }

        private int _UseFactor;
        public int UseFactor
        {
            get => _UseFactor;
            set => _UseFactor = value;
        }

        public Guid ACClassRouteUsageID
        {
            get;
            set;
        }

        private DateTime _LastManipulation;
        public DateTime LastManipulation
        {
            get
            {
                using(ACMonitor.Lock(_LockMember20900))
                {
                    return _LastManipulation;
                }
            }
            set
            {
                using (ACMonitor.Lock(_LockMember20900))
                {
                    _LastManipulation = value;
                }
            }
        }

        private ACMonitorObject _LockMember20900 = new ACMonitorObject(20900);

        public object Clone()
        {
            return new RouteHashItem()
            {
                RouteHashCodes = this.RouteHashCodes,
                UseFactor = this.UseFactor,
                LastManipulation = this.LastManipulation
            };
        }
    }    
}
