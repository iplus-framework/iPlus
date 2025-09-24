using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using gip.ext.design.avui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    public enum eWpfTheme : short
    {
        Gip,
        Aero,
    };

    public class CustomControlStyleInfo
    {
        public eWpfTheme wpfTheme { get; set; }
        public string styleName { get; set; }
        public string styleUri { get; set; }
        public bool hasImplicitStyles { get; set; }

        private bool _isRegisteredAsImplicitStyle = false;
        public bool IsRegisteredAsImplicitStyle
        {
            get
            {
                return _isRegisteredAsImplicitStyle;
            }
        }

        internal void WasRegisteredAsImplicitStyle()
        {
            _isRegisteredAsImplicitStyle = true;
        }
    }

    public class VBStyle : ControlTheme
    {
        public VBStyle()
            : base()
        {
        }
        public VBStyle(Type targetType)
            : base(targetType)
        {
        }

        public VBStyle(Type targetType, ControlTheme basedOn)
            : base(targetType)
        {
            this.BasedOn = basedOn;
        }
    }

    public class ControlManager
    {
        private static List<List<CustomControlStyleInfo>> _listOfImplicitStyles = new List<List<CustomControlStyleInfo>> {
            new List<CustomControlStyleInfo> { 
                        new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                                     styleName = "{x:Type ScrollViewer}", 
                                                     styleUri = "avares://gip.core.layoutengine.avui/Controls/VBScrollViewer/Themes/ScrollViewerStyleGip.xaml" },
                        new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                                     styleName = "{x:Type ScrollViewer}", 
                                                     styleUri = "avares://gip.core.layoutengine.avui/Controls/VBScrollViewer/Themes/ScrollViewerStyleAero.xaml" },
                },
            new List<CustomControlStyleInfo> { 
                        new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                                     styleName = "{x:Type ToggleButton}", 
                                                     styleUri = "avares://gip.core.layoutengine.avui/Controls/VBButton/Themes/ToggleButtonStyleGip.xaml" },
                        new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                                     styleName = "{x:Type ToggleButton}", 
                                                     styleUri = "avares://gip.core.layoutengine.avui/Controls/VBButton/Themes/ToggleButtonStyleGip.xaml" },
                },
            new List<CustomControlStyleInfo> { 
                        new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                                     styleName = "WpfDesignStyle", 
                                                     styleUri = "avares://gip.core.layoutengine.avui/Themes/WpfDesignStyles.xaml" },
                        new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                                     styleName = "WpfDesignStyle", 
                                                     styleUri = "avares://gip.core.layoutengine.avui/Themes/WpfDesignStyles.xaml" },
                },
            new List<CustomControlStyleInfo> { 
                        new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                                     styleName = "sharedtestdamir", 
                                                     styleUri = "avares://gip.core.layoutengine.avui/Controls/VBButton/Themes/SharedResourceTest.xaml" },
                },
            /*new List<CustomControlStyleInfo> { 
                        new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                                     styleName = "TextBlockStyleGip", 
                                                     styleUri = "avares://gip.core.layoutengine.avui/Controls/VBTextBlock/Themes/TextBlockStyleGip.xaml" },
                        new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                                     styleName = "TextBlockStyleAero", 
                                                     styleUri = "avares://gip.core.layoutengine.avui/Controls/VBTextBlock/Themes/TextBlockStyleAero.xaml" },
                    },*/
        };

        //#region private members
        //List<ControlInfo> mControlInfos = new List<ControlInfo>();
        //#endregion

        #region c´tors
        public ControlManager()
        {
        }
        #endregion

        private static eWpfTheme _WpfTheme = eWpfTheme.Gip;
        public static eWpfTheme WpfTheme
        {
            set
            {
                _WpfTheme = value;
            }

            get
            {
                return _WpfTheme;
            }
        }

        private static bool _RestoreWindowsOnSameScreen = false;
        public static bool RestoreWindowsOnSameScreen
        {
            set
            {
                _RestoreWindowsOnSameScreen = value;
            }

            get
            {
                return _RestoreWindowsOnSameScreen;
            }
        }


        private static bool _TouchScreenMode = false;
        public static bool TouchScreenMode
        {
            set
            {
                _TouchScreenMode = value;
            }

            get
            {
                return _TouchScreenMode;
            }
        }

        private static Application App
        {
            get;
            set;
        }

        static public void RegisterImplicitStyles(Control oneElementInTree)
        {
            if (oneElementInTree == null)
                return;
            foreach (List<CustomControlStyleInfo> styleInfoList in ControlManager._listOfImplicitStyles)
            {
                RegisterImplicitStyle(oneElementInTree, styleInfoList, false);
            }
        }

        static public void RegisterImplicitStyles(Application app)
        {
            //BamlWriter.Save(app);
            if (app == null)
                return;
            App = app;
            foreach (List<CustomControlStyleInfo> styleInfoList in ControlManager._listOfImplicitStyles)
            {
                CustomControlStyleInfo CustomControlStyleInfo = (from o in styleInfoList where o.wpfTheme == ControlManager.WpfTheme select o).FirstOrDefault();
                if (CustomControlStyleInfo == null)
                    continue;
                app.Resources.MergedDictionaries.Add(new ResourceInclude(new Uri(CustomControlStyleInfo.styleUri, UriKind.Relative)));
            }

            if (WpfTheme == eWpfTheme.Aero)
            {
                app.Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://gip.core.layoutengine.avui/Themes/AeroStyles.xaml", UriKind.Relative)));
                //app.Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("/Fluent;Component/Themes/Office2010/Silver.xaml", UriKind.Relative)));
            }
            else if (WpfTheme == eWpfTheme.Gip)
            {
                app.Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://gip.core.layoutengine.avui/Themes/Generic.xaml", UriKind.Relative)));
                //app.Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("/Fluent;Component/Themes/Office2010/Black.xaml", UriKind.Relative)));
            }
        }

        static public bool RegisterImplicitStyle(Control guiObject, List<CustomControlStyleInfo> CustomControlStyleInfoList, bool isInInitializing, bool setGipDefaultStyleIntoLogicalTree = false)
        {
            return true;

            //if ((guiObject == null) || (CustomControlStyleInfoList == null))
            //    return false;

            //// Falls gip = default, dann werden Templates aus SystemResource geholt, weil sie mit DefaultStyleKeyProperty.OverrideMetadata registriert worden sind
            //if (ControlManager.WpfTheme == eWpfTheme.Gip && !setGipDefaultStyleIntoLogicalTree)
            //{
            //    return true;
            //}

            //CustomControlStyleInfo CustomControlStyleInfo = (from o in CustomControlStyleInfoList where o.wpfTheme == ControlManager.WpfTheme select o).FirstOrDefault();
            //if (CustomControlStyleInfo == null)
            //    return false;
            //// Falls anderer Style schon im Logical-Tree als impliziter Style in der Resource-Dictionary eingetragen worden ist, springe zurück
            ////if (CustomControlStyleInfo.IsRegisteredAsImplicitStyle)
            ////return true;

            ////AvaloniaObject elementOnTopInLogicalTree = GetHighestControlInLogicalTree(guiObject);
            ////if (elementOnTopInLogicalTree == null)
            ////    return false;

            ////ResourceDictionary dict = new ResourceDictionary();
            ////dict.Source = new Uri(CustomControlStyleInfo.styleUri, UriKind.Relative);
            ////// Falls kein ParentObjekt gefunden, dann trage RessourceDictionary in lokales Objekt ein
            ////(elementOnTopInLogicalTree as Control).Resources.MergedDictionaries.Add(dict);

            //// Falls anderer Style, dann soll dieser auf eine möglichst hohe Ebene im Logical Tress gesetzt werden,
            //// damit nicht unnötige Kopien lokal herrschen
            //if (App != null)
            //{
            //    object resource = App.TryFindResource(CustomControlStyleInfo.styleName);
            //    if (resource == null)
            //    {
            //        ResourceDictionary dict = new ResourceDictionary();
            //        dict.Source = new Uri(CustomControlStyleInfo.styleUri, UriKind.Relative);
            //        App.Resources.MergedDictionaries.Add(dict);
            //    }
            //}
            //else
            //{
            //    object resource = guiObject.TryFindResource(CustomControlStyleInfo.styleName);
            //    if (resource == null)
            //    {
            //        AvaloniaObject parentLogicalObject = GetHighestControlInLogicalTree(guiObject);
            //        if (parentLogicalObject != null)
            //        {
            //            if (!(parentLogicalObject is Control))
            //                parentLogicalObject = null;
            //            else if (isInInitializing && (parentLogicalObject is UserControl))
            //            {
            //                parentLogicalObject = null;
            //            }
            //        }

            //        if ((parentLogicalObject == null) || (parentLogicalObject == guiObject))
            //        {
            //            // Falls Aufruf von Konstruktor der ohne Parent übergeben worden ist und auch nicht von ApplyTemplate, 
            //            // dann warte, weil im OnApplyTemplate der Aufruf nochmals kommt
            //            if (isInInitializing)
            //                return false;
            //            if (parentLogicalObject == null && isInInitializing)
            //                return false;
            //        }

            //        ResourceDictionary dict = new ResourceDictionary();
            //        dict.Source = new Uri(CustomControlStyleInfo.styleUri, UriKind.Relative);
            //        // Falls kein ParentObjekt gefunden, dann trage RessourceDictionary in lokales Objekt ein
            //        if (parentLogicalObject == null)
            //            guiObject.Resources.MergedDictionaries.Add(dict);
            //        else
            //        {
            //            if (parentLogicalObject is Control)
            //                (parentLogicalObject as Control).Resources.MergedDictionaries.Add(dict);
            //            else
            //                guiObject.Resources.MergedDictionaries.Add(dict);
            //        }

            //        CustomControlStyleInfo.WasRegisteredAsImplicitStyle();
            //    }
            //}

            //return true;
        }

        static public ResourceInclude GetResourceDict(List<CustomControlStyleInfo> CustomControlStyleInfoList)
        {
            CustomControlStyleInfo CustomControlStyleInfo = (from o in CustomControlStyleInfoList where o.wpfTheme == ControlManager.WpfTheme select o).FirstOrDefault();
            if (CustomControlStyleInfo == null)
                return null;
            return new ResourceInclude(new Uri(CustomControlStyleInfo.styleUri, UriKind.Relative));
        }

        static public ControlTheme GetStyleOfTheme(List<CustomControlStyleInfo> CustomControlStyleInfoList)
        {
            CustomControlStyleInfo CustomControlStyleInfo = (from o in CustomControlStyleInfoList where o.wpfTheme == ControlManager.WpfTheme select o).FirstOrDefault();
            if (CustomControlStyleInfo == null)
                return null;

            try
            {
                ResourceInclude dict = new ResourceInclude(new Uri(CustomControlStyleInfo.styleUri, UriKind.Relative));
                object res;
                if (!dict.TryGetResource(CustomControlStyleInfo.styleName, null, out res))
                    return null;
                ControlTheme style = res as ControlTheme;
                if (style != null)
                    return style;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ControlManager", "GetStyleOfTheme", msg);
            }
            return null;
        }

        static public bool OverrideStyleWithTheme(Control guiObject, List<CustomControlStyleInfo> CustomControlStyleInfoList)
        {
            CustomControlStyleInfo CustomControlStyleInfo = (from o in CustomControlStyleInfoList where o.wpfTheme == ControlManager.WpfTheme select o).FirstOrDefault();
            if (CustomControlStyleInfo == null)
                return false;
            object resource;
            if (!guiObject.TryFindResource(CustomControlStyleInfo.styleName, out resource))
                return false;
            if ((resource != null) && (resource is ControlTheme))
                guiObject.Theme = resource as ControlTheme;
            //else
            //    guiObject SetResourceReference(StyledElement.ThemeProperty, CustomControlStyleInfo.styleName);
            return true;
        }

        static public AvaloniaObject GetHighestControlInLogicalTree(AvaloniaObject guiObject)
        {
            if (guiObject == null)
                return null;
            AvaloniaObject parent = LogicalTreeHelper.GetParent(guiObject);
            // Ganz oben angelangt
            if (parent == null)
            {
                if (guiObject is Control)
                    return guiObject;
                return null;
            }

            // Sonst untersuche ob parent parents hat
            AvaloniaObject parent2 = GetHighestControlInLogicalTree(parent); ;
            if (parent2 != null)
            {
                if (parent2 is Control)
                    return parent2;
            }
            if (parent is Control)
                return parent;
            return guiObject;
        }

        //public void AddControlType(string control, Type controlType)
        //{
        //    mControlInfos.Add(new ControlInfo(control, controlType));
        //}
    }
}
