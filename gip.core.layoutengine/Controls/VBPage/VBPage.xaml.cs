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
    partial class VBPage : Page, IACObject
    {
        public VBPage()
        {
        }

        public VBPage(VBFrameControl frameControl)
        {
            FrameControl = frameControl;
            Loaded += OnLoaded;
        }

        public VBPage(VBFrameControl frameControl, UIElement vbDesignContent, bool isBusinessObject = false, string title = "")
        {
            FrameControl = frameControl;
            this.VBDesignContent = vbDesignContent;
            this.isBusinessObject = isBusinessObject;
            //this.Content = vbDesignContent;
            if (title != "")
                this.Title = title;
            else
                this.Title = ACCaption;
            //this.GenerateContent(VBDesignContent);
            if ((this.VBDesignContent is FrameworkElement) && (this.VBDesignContent is IVBContent))
            {
                //TODO Implement event handling
                //if (this.VBDesignContent is VBDesign)
                //    (this.VBDesignContent as VBDesign).OnContextACObjectChanged += new EventHandler(VBDockingContainerBase_OnElementACComponentChanged);
                //else
                //    (this.VBDesignContent as FrameworkElement).DataContextChanged += new DependencyPropertyChangedEventHandler(VBDockingContainerBase_ContentDataContextChanged);
                //(this.VBDesignContent as FrameworkElement).Loaded += new RoutedEventHandler(VBDockingContainerBase_ContentLoaded);
            Loaded += OnLoaded;
            }
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

        public void GenerateContent(object content)
        {
            if (content is VBDesign)
            {
                ((VBDesign)content).DataContext = ContextACObject;
                ((VBDesign)content).BSOACComponent = ContextACObject as IACBSO;

                if (MainGrid != null)
                {
                    MainGrid.Children.Add((VBDesign)content);
                }
                else
                {
                    Debugger.Break();
                }
            mainGrid = Template.FindName("MainGrid", this) as Grid;
            }

        }

        private Grid mainGrid;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            mainGrid = Template.FindName("MainGrid", this) as Grid;

            if (mainGrid != null && VBDesignContent is VBDesign content && !(mainGrid.Children.Contains(content)))
            {
                mainGrid.Children.Add(content);
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

        public bool isBusinessObject;

        public IEnumerable<IACObject> ACContentList => throw new NotImplementedException();

        public string ACIdentifier => throw new NotImplementedException();

        public string ACCaption 
        { 
            get
            {
                if (this.VBDesignContent == null)
                {
                    return "";
                }
                string acCaption = "";

                if ((VBDesignContent != null) && (VBDesignContent is IVBContent))
                    acCaption = (VBDesignContent as IVBContent).ACCaption;

                if (VBDesignContent is VBDesign)
                {
                    if (((gip.core.layoutengine.VBDesign)(VBDesignContent)) != null &&
                        ((gip.core.layoutengine.VBDesign)(VBDesignContent)).DataContext is IACComponent &&
                        !string.IsNullOrEmpty(((gip.core.layoutengine.VBDesign)(VBDesignContent)).VBContent) &&
                        ((gip.core.layoutengine.VBDesign)(VBDesignContent)).VBContent.StartsWith("*"))
                    {
                        IACComponent acComponent = ((gip.core.layoutengine.VBDesign)(VBDesignContent)).DataContext as IACComponent;
                        ACClassDesign acClassDesign = acComponent.GetDesign(((gip.core.layoutengine.VBDesign)(VBDesignContent)).VBContent.Substring(1));
                        if (acClassDesign != null)
                            return acClassDesign.ACCaption;
                    }
                }


                string title = VBDockingManager.GetWindowTitle(VBDesignContent);
                if (!String.IsNullOrEmpty(title))
                    acCaption = title;
                if (String.IsNullOrEmpty(acCaption) && (ContextACObject != null))
                    acCaption = ContextACObject.ACCaption;
                return acCaption;
            }
        }

        #region IACObject

        public IACType ACType => throw new NotImplementedException();

        public IACObject ParentACObject => throw new NotImplementedException();

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

        #endregion

        protected override void OnContextMenuClosing(ContextMenuEventArgs e)
        {
            base.OnContextMenuClosing(e);
        }

        protected override void OnToolTipClosing(ToolTipEventArgs e)
        {
            base.OnToolTipClosing(e); 
        }
    }
}
