using DocumentFormat.OpenXml.ExtendedProperties;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace gip.core.layoutengine
{
    public class VBPage : Page, IACObject
    {
        public VBPage()
        {
        }

        public VBPage(VBFrameControl frameControl)
        {
            FrameControl = frameControl;
        }

        public VBPage(VBFrameControl frameControl, UIElement vbDesignContent)
        {
            FrameControl = frameControl;
            this.VBDesignContent = vbDesignContent;
            //this.GenerateContent(VBDesignContent);
            if ((this.VBDesignContent is FrameworkElement) && (this.VBDesignContent is IVBContent))
            {
                //TODO Implement event handling
                //if (this.VBDesignContent is VBDesign)
                //    (this.VBDesignContent as VBDesign).OnContextACObjectChanged += new EventHandler(VBDockingContainerBase_OnElementACComponentChanged);
                //else
                //    (this.VBDesignContent as FrameworkElement).DataContextChanged += new DependencyPropertyChangedEventHandler(VBDockingContainerBase_ContentDataContextChanged);
                //(this.VBDesignContent as FrameworkElement).Loaded += new RoutedEventHandler(VBDockingContainerBase_ContentLoaded);
            }
        }

        static VBPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBPage), new FrameworkPropertyMetadata(typeof(VBPage)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Debugger.Break();
            this.GenerateContent(VBDesignContent);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            Debugger.Break();
            this.GenerateContent(VBDesignContent);
        }

        public VBFrameControl FrameControl;

        public UIElement VBDesignContent
        {
            get;
            set;
        }

        private Grid _MainGrid;
        public Grid MainGrid
        {
            get
            {
                return _MainGrid;
            }
        }

        public void GenerateContent(object content)
        {
            if (content is VBDesign)
            {
                ((VBDesign)content).DataContext = ContextACObject;
                ((VBDesign)content).BSOACComponent = ContextACObject as IACBSO;

                object gridObj = (object)GetTemplateChild("MainGrid");
                if ((gridObj != null) && (gridObj is Grid))
                {
                    _MainGrid = ((Grid)gridObj);
                    _MainGrid.Children.Add((VBDesign)content);
                }

            }
            
        }

        IACObject _ACComponent = null;
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                if (_ACComponent != null)
                    return _ACComponent;
                if ((VBDesignContent != null) && (VBDesignContent is IVBContent))
                    return (VBDesignContent as IVBContent).ContextACObject;
                if (FrameControl == null)
                    return null;
                return FrameControl.ContextACObject;
            }
        }


        public IACObject ParentACObject => throw new NotImplementedException();

        public IACType ACType => throw new NotImplementedException();

        public IEnumerable<IACObject> ACContentList => throw new NotImplementedException();

        public string ACIdentifier => throw new NotImplementedException();

        public string ACCaption => throw new NotImplementedException();

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            throw new NotImplementedException();
        }

        public string GetACUrl(IACObject rootACObject = null)
        {
            throw new NotImplementedException();
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            throw new NotImplementedException();
        }
    }
}
