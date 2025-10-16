using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            ratingList.PointerPressed += RatingList_PointerPressed;
        }

        private void RatingList_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            ratingSetupFromClick = true;
            
            // Find the clicked element and traverse up to find the container
            var element = e.Source as Visual;
            while (element != null && element != ratingList)
            {
                if (element is Control control && control.DataContext is RatingItem dataItem)
                {
                    var selectedItems = RatingItemsSource.Where(x => x.IsSelected);
                    int sn = (dataItem.IsSelected && selectedItems.Any() && selectedItems.Max(x => x.Sn) == dataItem.Sn) 
                        ? dataItem.Sn - 1 
                        : dataItem.Sn;
                    
                    if (RatingStarCount > 0)
                    {
                        Rating = (decimal)sn / (decimal)RatingStarCount;
                    }
                    SetupRatingItemsSource(sn);
                    break;
                }
                element = element.GetVisualParent();
            }
            
            ratingSetupFromClick = false;
        }

        #endregion

        #region Event Handlers

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
            // No need to call Refresh() in Avalonia - property changes are automatically handled
        }

        #endregion

        #region Styled Properties

        /// <summary>
        /// Percentage value of rating
        /// </summary>
        [Category("VBControl")]
        public decimal Rating
        {
            get { return GetValue(RatingProperty); }
            set
            {
                if (value > 1)
                    throw new InvalidOperationException("Unallowed value for percentage!");
                SetValue(RatingProperty, value);
            }
        }

        /// <summary>
        /// Rating star counts
        /// </summary>
        [Category("VBControl")]
        public int RatingStarCount
        {
            get { return GetValue(RatingStarCountProperty); }
            set
            {
                if (value > 10) value = 10;
                if (value < 3) value = 3;
                SetValue(RatingStarCountProperty, value);
            }
        }

        #endregion

        #region Styled Properties - Static

        public static readonly StyledProperty<int> RatingStarCountProperty =
            AvaloniaProperty.Register<VBRatingControl, int>(nameof(RatingStarCount), defaultValue: 5);

        public static readonly StyledProperty<decimal> RatingProperty =
            AvaloniaProperty.Register<VBRatingControl, decimal>(nameof(Rating), defaultValue: 0m);

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == RatingStarCountProperty)
            {
                LoadRatingItemsSource();
            }
            else if (change.Property == RatingProperty)
            {
                if (!ratingSetupFromClick)
                {
                    int sn = (int)Math.Round(Rating * RatingStarCount, 0);
                    SetupRatingItemsSource(sn);
                }
            }
        }

        private bool ratingSetupFromClick;

        #endregion
    }
}
