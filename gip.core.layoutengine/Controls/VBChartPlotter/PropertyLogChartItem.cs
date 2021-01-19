using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Specialized;

using System.Runtime.Serialization;
using gip.core.datamodel;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    public interface IPropertyLogChartItem : IVBChartItem
    {
        PropertyLogListInfo GetLiveLogList();
        PropertyLogListInfo GetArchiveLogList(DateTime from, DateTime to);
    }

    public class PropertyLogChartItem : ChartItem, IPropertyLogChartItem
    {
        public PropertyLogChartItem() : base()
        {
            _DisplayMode = VBChartItemDisplayMode.PropertyLog;
        }

        public PropertyLogChartItem(IACObject acComponent, string dataContent) : base(acComponent, dataContent)
        {
            _DisplayMode = VBChartItemDisplayMode.PropertyLog;
        }

        public PropertyLogChartItem(IACPropertyBase acProperty) : base(acProperty)
        {
            _DisplayMode = VBChartItemDisplayMode.PropertyLog;
        }


        public PropertyLogListInfo GetLiveLogList()
        {
            if (ACProperty == null)
                return null;
            if (ACProperty.LiveLog == null)
                return null;
            return ACProperty.LiveLog;
        }

        public PropertyLogListInfo GetArchiveLogList(DateTime from, DateTime to)
        {
            if (ACProperty == null)
                return null;
            if (!(ACProperty is IACPropertyNetBase))
                return null;
            if ((from == DateTime.MinValue) || (to == DateTime.MinValue))
                return null;
            if (from >= to)
                return null;
            PropertyLogListInfo result = (ACProperty as IACPropertyNetBase).GetArchiveLog(from, to);
            if (result == null)
                return null;
            return result;
        }
    }

}
