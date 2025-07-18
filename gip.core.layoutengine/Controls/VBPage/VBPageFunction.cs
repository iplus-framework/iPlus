﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Navigation;

namespace gip.core.layoutengine
{
    public class VBPageFunction<T> : PageFunction<T>, IACObject
    {
        public VBPageFunction()
        {
        }

        public VBPageFunction(VBFrameController frameController)
        {
            FrameController = frameController;
            Loaded += OnLoaded;
        }

        public VBPageFunction(VBFrameController frameController, UIElement vbDesignContent, bool isBusinessObject = false, string title = "")
        {
            FrameController = frameController;
            this.VBDesignContent = vbDesignContent;
            this.isBusinessObject = isBusinessObject;
            //this.Content = vbDesignContent;
            if (title != "")
                this.Title = title;
            else
                this.Title = ACCaption;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Debugger.Break();
            this.GenerateContent(VBDesignContent);
        }

        public override void OnApplyTemplate()
        {

        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            Debugger.Break();
            this.GenerateContent(VBDesignContent);
        }

        public VBFrameController FrameController;

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
            }

        }

        private Grid mainGridMobile;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            mainGridMobile = Template.FindName("MainGridMobile", this) as Grid;

            if (mainGridMobile != null && VBDesignContent is VBDesign content && !(mainGridMobile.Children.Contains(content)))
            {
                mainGridMobile.Children.Add(content);
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
                if (FrameController == null)
                    return null;
                return FrameController.ContextACObject;
            }
        }

        public bool isBusinessObject;

        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
        }

        public string ACIdentifier
        {
            get { return this.Name; }
        }

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

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
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
