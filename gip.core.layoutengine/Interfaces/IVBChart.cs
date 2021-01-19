using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace gip.core.layoutengine
{
    public interface IVBChart : IVBContent
    {
        bool DisplayAsArchive
        {
            get;
            set;
        }

        /// <summary>
        /// PropertyLogs which shoud be displayed
        /// DisplayMode must be set to PropertyLog
        /// </summary>
        ObservableCollection<IVBChartItem> PropertyLogItems
        {
            get;
            set;
        }

        void InitializeChartArchive(DateTime from, DateTime to, Global.InterpolationMethod interpolation = Global.InterpolationMethod.None, int? range = null, double? decay=null);

        void InterpolationParamsChangedInView(Global.InterpolationMethod interpolation = Global.InterpolationMethod.None, int? range = null, double? decay = null);

        BitmapSource CreatePrintableBitmap();

        void SwitchNormalPrintingMode();
    }
}
