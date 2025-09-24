using Avalonia.Media.Imaging;
using gip.core.datamodel;
using System;
using System.Collections.ObjectModel;

namespace gip.core.layoutengine.avui
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

        Bitmap CreatePrintableBitmap();

        void SwitchNormalPrintingMode();
    }
}
