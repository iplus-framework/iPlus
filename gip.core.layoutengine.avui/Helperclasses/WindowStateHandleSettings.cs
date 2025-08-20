using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace gip.core.layoutengine.avui
{
    public class WindowStateHandleSettings
    {

        #region ctor's

        public WindowStateHandleSettings()
        {
            WindowMaximized = true;
            WindowPosition = new Rect(0, 0, 800, 600);
            Zoom = 100;
        }

        #endregion

        #region settings properties

        public Rect WindowPosition { get; set; }
        public int Zoom { get; set; }

        public bool WindowMaximized { get; set; }

        public string ScreenName { get; set; }

        #endregion

        #region Manipulating methods

        public void Save()
        {
            Properties.Settings.Default.WindowStateHandleSettings = WindowStateHandleSettings.Serialize(this);
            Properties.Settings.Default.Save();
        }

        public static string Serialize(WindowStateHandleSettings item)
        {
            string returnValue = "";
            using (StringWriter writer = new StringWriter())
            {
                XmlSerializer ser = XmlSerializer.FromTypes(new[] { typeof(WindowStateHandleSettings) })[0];
                ser.Serialize(writer, item);
                returnValue = writer.ToString();
            }
            return returnValue;
        }

        public static WindowStateHandleSettings Deserialize(string value)
        {
            WindowStateHandleSettings item = null;
            using (StringReader rr = new StringReader(value))
            {
                XmlSerializer ser = XmlSerializer.FromTypes(new[] { typeof(WindowStateHandleSettings) })[0];
                item = (WindowStateHandleSettings)ser.Deserialize(rr);
            }
            return item;
        }

        public static WindowStateHandleSettings Factory()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.WindowStateHandleSettings))
            {
                Properties.Settings.Default.WindowStateHandleSettings = WindowStateHandleSettings.Serialize(new WindowStateHandleSettings()
                {
                    WindowMaximized = true,
                    WindowPosition = new Rect(0, 0, 800, 600),
                    Zoom = 100
                });
                Properties.Settings.Default.Save();
            }
            return WindowStateHandleSettings.Deserialize(Properties.Settings.Default.WindowStateHandleSettings);
        }

        #endregion

    }
}
