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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Control for displaying XAML strings formatted in the source code. 
    /// XAML Attributes: VBContent: Relative or absolute ACUrl to an ACProperty that returns an XAML string
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von im Quelltext aufbereiteten XAML-Strings. 
    /// XAML-Attribute: VBContent: Relative oder absolute ACUrl zu einem ACProperty, welches einen XAML-String zurückliefert
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDynamic'}de{'VBDynamic'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBDynamic : VBDesignBase, IVBContent, IACMenuBuilderWPFTree, IACObject
    {
        #region c'tors
        public VBDynamic()
            : base()
        {
        }

        #endregion

        #region Control Loaded-Event
        bool _Loaded = false;

        /// <summary>
        /// Handles the Loaded event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        protected override void VBDesignBase_Loaded(object sender, RoutedEventArgs e)
        {
            base.VBDesignBase_Loaded(sender, e);
        }

        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        protected override void InitVBControl()
        {
            base.InitVBControl();
            if (_Loaded)
                return;

            if (UseDataContextAsBSO)
            {
                BSOACComponent = DataContext as IACBSO;
            }
            if (BSOACComponent != null && !string.IsNullOrEmpty(VBContent))
            {
                Binding binding = new Binding();
                binding.Source = DataContext;
                binding.Path = new PropertyPath(VBContent);
                binding.Mode = BindingMode.OneWay;
                binding.NotifyOnSourceUpdated = true;
                binding.NotifyOnTargetUpdated = true;
                this.SetBinding(VBDynamic.XMLDesignProperty, binding);
                _Loaded = true;
            }

            _LastSelectionActive = false;
            if (!string.IsNullOrEmpty(LastSelectionProperty) && BSOACComponent.GetType().GetProperty(LastSelectionProperty) != null  && (bool)BSOACComponent.ACUrlCommand(LastSelectionProperty))
                _LastSelectionActive = true;
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            if (_Loaded)
            {
                Content = null;
                if (ReadLocalValue(DataContextProperty) != DependencyProperty.UnsetValue)
                    DataContext = null;
                BindingOperations.ClearBinding(this, VBDynamic.XMLDesignProperty);
            }
            
            base.DeInitVBControl(bso);
            //_Loaded = false;
        }


        #endregion

        #region Dynamic XAML over Dependency-Property
        void LoadDesign()
        {
            List<Tuple<string,string>> tabNamesList = null;
            if (_LastSelectionActive)
                tabNamesList = CheckContent(Content);
            if (Content != null)
            {
                VBLogicalTreeHelper.DeInitVBControls(BSOACComponent, this.Content);
                VBVisualTreeHelper.DeInitVBControls(BSOACComponent, this.Content);
            }
            Content = null;
            if (!string.IsNullOrEmpty(VBDynamicACComponent) && BSOACComponent != null)
            {
                DataContext = BSOACComponent.ACUrlCommand(VBDynamicACComponent);
            }

            UIElement uiElement = null;
            if (string.IsNullOrEmpty(XMLDesign))
            {
                ContentControl contentControl = new ContentControl();
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri("/gip.core.layoutengine;Component/Controls/VBRibbon/Icons/Design.xaml", UriKind.Relative);
                contentControl.Style = (Style)dict["IconDesignStyleGip"];
                uiElement = contentControl;
            }
            else
            {
                uiElement = Layoutgenerator.LoadLayout(XMLDesign, DataContextForContent != null ? DataContextForContent : ContextACObject, BSOACComponent, VBContent);
                FrameworkElement fw = uiElement as FrameworkElement;
                if (fw != null && DataContextForContent != null)
                {
                    Binding binding = new Binding();
                    binding.Source = DataContextForContent;
                    SetBinding(FrameworkElement.DataContextProperty, binding);
                    //fw.DataContext = DataContextForContent;
                }
            }

            if (tabNamesList != null && uiElement is FrameworkElement)
            {
                ((FrameworkElement)uiElement).Resources.Add("LastSelectedTabsList", tabNamesList);
                SetLastSelectedTabsOnVBTabControl(uiElement);

            }

            Content = uiElement;
            this.Focus();
        }

        /// <summary>
        /// Gets or sets the XML design.
        /// </summary>
        public string XMLDesign
        {
            get 
            { 
                return (string)GetValue(XMLDesignProperty); 
            }
            set
            {
                SetValue(XMLDesignProperty, value);
            }
        }
        
        /// <summary>
        /// Represents the dependency property for XMLDesign.
        /// </summary>
        public static readonly DependencyProperty XMLDesignProperty
            = DependencyProperty.Register("XMLDesign", typeof(string), typeof(VBDynamic), new PropertyMetadata(new PropertyChangedCallback(XMLDesignChanged)));

        private static void XMLDesignChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VBDynamic)
            {
                VBDynamic vbDynamic = d as VBDynamic;
                vbDynamic.LoadDesign();
            }
        }

        /// <summary>
        /// Gets or sets the data context for content.
        /// </summary>
        public IACObject DataContextForContent
        {
            get
            {
                return (IACObject)GetValue(DataContextForContentProperty);
            }
            set
            {
                SetValue(DataContextForContentProperty, value);
            }
        }

        /// <summary>
        /// Represents the dependency property for DataContextForContent.
        /// </summary>
        public static readonly DependencyProperty DataContextForContentProperty
            = DependencyProperty.Register("DataContextForContent", typeof(IACObject), typeof(VBDynamic), new PropertyMetadata());
        #endregion

        #region Overridden IACInteractiveObject Member

        #endregion

        #region IVBContent Member
        #endregion

        #region IACInteractiveObject Member

        /// <summary>
        /// Gets or sets the ACUrl of VBDynamicACComponent.
        /// </summary>
        [Category("VBControl")]
        public string VBDynamicACComponent
        {
            get;
            set;
        }
        #endregion

        #region IACObject Member

        protected string _ACCaption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public override string ACCaption
        {
            get
            {
                if (!String.IsNullOrEmpty(_ACCaption))
                    return _ACCaption;
                if (ContextACObject != null)
                    return ContextACObject.ACCaption;
                return "";
            }
            set
            {
                _ACCaption = value;
            }
        }

        #endregion

        #region Last selection

        private bool _LastSelectionActive;

        /// <summary>
        /// Gets or sets the last selection property.
        /// </summary>
        public string LastSelectionProperty
        {
            get;
            set;
        }

        private List<Tuple<string,string>> CheckContent(object oldContent)
        {
            if (oldContent == null || oldContent.GetType().IsAssignableFrom(typeof(FrameworkElement)))
                return null;

            List<Tuple<string, string>> tabItemNameList = new List<Tuple<string, string>>();
            VBTabControl tabControl = VBLogicalTreeHelper.FindChildObjectInLogicalTree((DependencyObject)oldContent, typeof(VBTabControl)) as VBTabControl;
            if (tabControl == null)
                return null;

            VBTabItem tabItem = tabControl.SelectedItem as VBTabItem;
            if (tabItem == null)
                return null;

            if(!string.IsNullOrEmpty(tabItem.ACCaption))
                tabItemNameList.Add(new Tuple<string,string>("ACCaption",tabItem.ACCaption));
            else if(!string.IsNullOrEmpty(tabItem.Header.ToString()))
                tabItemNameList.Add(new Tuple<string, string>("Header", tabItem.Header.ToString()));

            while (tabControl != null)
            {
                tabControl = VBLogicalTreeHelper.FindChildObjectInLogicalTree(tabItem, typeof(VBTabControl)) as VBTabControl;
                if(tabControl != null)
                {
                    tabItem = tabControl.SelectedItem as VBTabItem;
                    if (tabItem != null)
                    {
                        if (!string.IsNullOrEmpty(tabItem.ACCaption))
                            tabItemNameList.Add(new Tuple<string, string>("ACCaption", tabItem.ACCaption));
                        else
                            tabItemNameList.Add(new Tuple<string, string>("Header", tabItem.Header.ToString()));
                    }
                }
            }
            return tabItemNameList;
        }

        private void SetLastSelectedTabsOnVBTabControl(object content)
        {
            VBTabControl tabControl = VBLogicalTreeHelper.FindChildObjectInLogicalTree((DependencyObject)content, typeof(VBTabControl)) as VBTabControl;
            if (tabControl == null)
            {
                return;
            }
            tabControl.IsActiveLastSelectedTab = true;
        }

        #endregion

        /// <summary>
        /// Determines is data context used as BSO or not.
        /// </summary>
        [Category("VBControl")]
        public bool UseDataContextAsBSO
        {
            get;
            set;
        }  
    }
}
