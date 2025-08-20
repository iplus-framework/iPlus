using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the control that provides rating evaulation.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt das Steuerelement dar, das die Rating-Evaluierung bereitstellt.
    /// </summary>
    public partial class VBRatingControl : UserControl
    {

        #region ctor's

        public VBRatingControl()
        {
            InitializeComponent();
            ratingList.PreviewMouseDown += ratingList_PreviewMouseDown;
        }

        #endregion

        #region Event Hanlders

        void ratingList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ratingSetupFromClick = true;
            var item = ItemsControl.ContainerFromElement(ratingList, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                RatingItem dataItem = item.DataContext as RatingItem;
                int sn = (dataItem.IsSelected && RatingItemsSource.Where(x => x.IsSelected).Max(x => x.Sn) == dataItem.Sn) ? dataItem.Sn - 1 : dataItem.Sn;
                Rating = (decimal)sn / (decimal)RatingStarCount;
                SetupRatingItemsSource(sn);
            }
            ratingSetupFromClick = false;
        }

        #endregion

        #region Properties

        private ObservableCollection<RatingItem> ratingItemsSource;
        public ObservableCollection<RatingItem> RatingItemsSource
        {
            get
            {
                if (ratingItemsSource == null)
                {
                    LoadRatingItemsSource();
                }
                return ratingItemsSource;
            }
        }

        //public decimal Step
        //{
        //    get
        //    {
        //        return ((decimal)1) / ((decimal)RatingStarCount);
        //    }
        //}

        #endregion

        #region Methods

        private void LoadRatingItemsSource()
        {
            decimal step = ((decimal)1) / ((decimal)RatingStarCount);
            ratingItemsSource = new ObservableCollection<RatingItem>();
            for (int i = 1; i <= RatingStarCount; i++)
            {
                RatingItem item = new RatingItem();
                item.Sn = i;
                ratingItemsSource.Add(item);
            }
            ratingList.ItemsSource = RatingItemsSource;
        }

        private void SetupRatingItemsSource(int sn)
        {
            foreach (var rt in RatingItemsSource)
            {
                rt.IsSelected = rt.Sn <= sn;
            }
            ratingList.Items.Refresh();
        }

        #endregion

        #region Dependency Property


        /// <summary>
        /// Percentage value of rating
        /// </summary>
        [Category("VBControl")]
        public decimal Rating
        {
            get { return (decimal)this.GetValue(RatingProperty); }
            set
            {
                if (value > 1)
                    throw new InvalidOperationException("Unallowed value for percentage!");
                this.SetValue(RatingProperty, value);
                if(!ratingSetupFromClick)
                {
                    int sn = (int)Math.Round(Rating * RatingStarCount, 0);
                    SetupRatingItemsSource(sn);
                }
            }
        }


        /// <summary>
        /// Rating star counts
        /// </summary>
        [Category("VBControl")]
        public int RatingStarCount
        {
            get { return (int)this.GetValue(RatingStarCountProperty); }
            set
            {
                if (value > 10) value = 10;
                if (value < 3) value = 3;
                this.SetValue(RatingStarCountProperty, value);
            }
        }

        #endregion

        #region Dependency property - static

        public static readonly DependencyProperty RatingStarCountProperty = DependencyProperty.Register(
          "RatingStarCount", typeof(int), typeof(VBRatingControl), new PropertyMetadata(5, new PropertyChangedCallback(OnRatingStarCountChanged)));


        public static readonly DependencyProperty RatingProperty = DependencyProperty.Register(
          "Rating", typeof(decimal), typeof(VBRatingControl), new PropertyMetadata(new PropertyChangedCallback(OnRatingChanged)));

        private static void OnRatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VBRatingControl ctrl = d as VBRatingControl;
            if(ctrl != null /*&& ctrl.IsLoaded*/)
            {
                int sn = (int)Math.Round(ctrl.Rating * ctrl.RatingStarCount, 0);
                ctrl.SetupRatingItemsSource(sn);
            }
        }

        private static void OnRatingStarCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VBRatingControl ctrl = d as VBRatingControl;
            if (ctrl != null /*&& ctrl.IsLoaded*/)
            {
                ctrl.LoadRatingItemsSource();
            }
        }

        private bool ratingSetupFromClick;
        #endregion
    }
}
