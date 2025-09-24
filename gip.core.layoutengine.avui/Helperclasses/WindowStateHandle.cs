using Avalonia;
using Avalonia.Controls;
using System;
using System.Linq;

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
                   (int)window.Position.X, (int)window.Position.Y,
                   (int)window.Width, (int)window.Height));
            settings.ScreenName = usedScreen.DeviceName;
            if (!onlyUsedScreen)
            {
                settings.WindowMaximized = window.WindowState == WindowState.Maximized;
                settings.WindowPosition =
                    new Rect(
                                Math.Abs(window.Position.X - usedScreen.WorkingArea.Left) / usedScreen.WorkingArea.Width,
                                Math.Abs(window.Position.Y - usedScreen.WorkingArea.Top) / usedScreen.WorkingArea.Height,
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

            window.WindowState = WindowState.Normal;
            if (settings.WindowMaximized)
            {
                PixelPoint restoreLocation = new PixelPoint(usedScreen.WorkingArea.Left, usedScreen.WorkingArea.Top);
                window.Position = restoreLocation;
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                PixelPoint restoreLocation = new PixelPoint((int) (usedScreen.WorkingArea.Left + restoreBounds.Left * usedScreen.WorkingArea.Width), 
                                                            (int) (usedScreen.WorkingArea.Top + restoreBounds.Top * usedScreen.WorkingArea.Height));
                double width = restoreBounds.Width * usedScreen.WorkingArea.Width;
                double height = restoreBounds.Height * usedScreen.WorkingArea.Height;

                bool outsideBorder = !usedScreen.WorkingArea.IntersectsWith(new System.Drawing.Rectangle(
                    (int)restoreLocation.X, (int)restoreLocation.Y,
                    (int)width, (int)height));
                if (outsideBorder)
                    restoreLocation = new PixelPoint(usedScreen.WorkingArea.Left, usedScreen.WorkingArea.Top);

                window.Position = restoreLocation;
                window.Width = width;
                window.Height = height;
                if (outsideBorder && window.IsLoaded)
                    window.WindowState = WindowState.Maximized;
            }
        }
    }
}
