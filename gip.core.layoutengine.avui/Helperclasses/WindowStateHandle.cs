using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Helper object for calculate and manpulate position of VB window on the screen
    /// memorize and use last used position and display
    /// </summary>
    public class WindowStateHandle
    {
        /// <summary>
        /// Save window poistion and zoom in user settings
        /// </summary>
        /// <param name="window"></param>
        /// <param name="sldZoom"></param>
        /// <param name="onlyUsedScreen"></param>
        public static void Save(Window window, int sldZoom, bool onlyUsedScreen = false)
        {
            WindowStateHandleSettings settings = WindowStateHandleSettings.Factory();
            System.Windows.Forms.Screen usedScreen = System.Windows.Forms.Screen.FromRectangle(
               new System.Drawing.Rectangle(
                   (int)window.Left, (int)window.Top,
                   (int)window.Width, (int)window.Height));
            settings.ScreenName = usedScreen.DeviceName;
            if (!onlyUsedScreen)
            {
                settings.WindowMaximized = window.WindowState == System.Windows.WindowState.Maximized;
                settings.WindowPosition =
                    new Rect(
                                Math.Abs(window.Left - usedScreen.WorkingArea.Left) / usedScreen.WorkingArea.Width,
                                Math.Abs(window.Top - usedScreen.WorkingArea.Top) / usedScreen.WorkingArea.Height,
                                window.Width / usedScreen.WorkingArea.Width,
                                window.Height / usedScreen.WorkingArea.Height
                             );
                settings.Zoom = sldZoom;
            }
            settings.Save();
        }

        /// <summary>
        /// Read from user settings if exist one
        /// or doing default window setup
        /// </summary>
        /// <param name="window"></param>
        public static void RestoreState(Window window)
        {
            WindowStateHandleSettings settings = WindowStateHandleSettings.Factory();
            System.Windows.Forms.Screen usedScreen = null;
            if (!string.IsNullOrEmpty(settings.ScreenName))
            {
                usedScreen = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == settings.ScreenName);
            }

            if (usedScreen == null)
                usedScreen = System.Windows.Forms.Screen.PrimaryScreen;

            Rect restoreBounds = settings.WindowPosition;

            window.Left = 0;
            window.Top = 0;
            window.WindowState = System.Windows.WindowState.Normal;
            if (settings.WindowMaximized)
            {
                window.Left = usedScreen.WorkingArea.Left;
                window.Top = usedScreen.WorkingArea.Top;
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                window.Left = usedScreen.WorkingArea.Left + restoreBounds.Left * usedScreen.WorkingArea.Width;
                window.Top = usedScreen.WorkingArea.Top + restoreBounds.Top * usedScreen.WorkingArea.Height;
                window.Width = restoreBounds.Width * usedScreen.WorkingArea.Width;
                window.Height = restoreBounds.Height * usedScreen.WorkingArea.Height;

                bool outsideBorder = !usedScreen.WorkingArea.IntersectsWith(new System.Drawing.Rectangle(
                    (int)window.Left, (int)window.Top,
                    (int)window.Width, (int)window.Height));

                if (outsideBorder)
                {
                    window.Left = usedScreen.WorkingArea.Left;
                    window.Top = usedScreen.WorkingArea.Top;
                    if (window.IsLoaded)
                        window.WindowState = WindowState.Maximized;
                }
            }
        }
    }
}
