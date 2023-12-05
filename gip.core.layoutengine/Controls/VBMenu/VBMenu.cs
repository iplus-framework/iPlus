using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a Windows menu control that enables you to hierarchically organize elements associated with commands and event handlers.
    /// </summary>
    public class VBMenu : Menu
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "MenuStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBMenu/Themes/MenuStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "MenuStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBMenu/Themes/MenuStyleAero.xaml" },
        };
        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBMenu), new FrameworkPropertyMetadata(typeof(VBMenu)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBMenu.
        /// </summary>
        public VBMenu()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        #region Mobile

        private bool isMenuOpen = false;

        public void ToggleMenu()
        {
            ThicknessAnimation animation = new ThicknessAnimation
            {
                Duration = TimeSpan.FromSeconds(0.3)
            };

            if (!isMenuOpen)
            {
                animation.From = new Thickness(-this.ActualWidth, 0, 0, 0);
                animation.To = new Thickness(0, 0, 0, 0);
                this.Visibility = Visibility.Visible;
            }
            else
            {
                animation.From = new Thickness(0, 0, 0, 0);
                animation.To = new Thickness(-this.ActualWidth, 0, 0, 0);
                animation.Completed += (s, e) =>
                {
                    this.Visibility = Visibility.Hidden;
                    this.Margin = new Thickness(-this.ActualWidth, 0, 0, 0);
                };
            }

            this.BeginAnimation(FrameworkElement.MarginProperty, animation);

            isMenuOpen = !isMenuOpen;
        }

        public void ToggleMenuDelay(bool delayAnimation)
        {
            if (delayAnimation)
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(150);
                timer.Tick += (sender, args) =>
                {
                    timer.Stop();
                    ToggleMenu();
                };
                timer.Start();
            }
            else
            {
                ToggleMenu();
            }
        }

        #endregion
    }
}
