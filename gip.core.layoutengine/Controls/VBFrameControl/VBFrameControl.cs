using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace gip.core.layoutengine
{
    public class VBFrameControl : Frame
    {
        public VBFrameControl()
        {
        }

        static VBFrameControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBFrameControl), new FrameworkPropertyMetadata(typeof(VBFrameControl)));
        }

        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        public List<UIElement> _VBDesignList = new List<UIElement>();
        public List<UIElement> VBDesignList
        {
            get
            {
                return _VBDesignList;
            }
            set
            {
                _VBDesignList = value;
            }
        }


        public void ChangeFrameContent(object content)
        {
            try
            {
                this.Navigate(content);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShowWindow(IACComponent forObject, string acClassDesignName, bool isClosableBSORoot, Global.VBDesignContainer containerType, Global.VBDesignDockState dockState,
            Global.VBDesignDockPosition dockPosition, Global.ControlModes ribbonVisibility, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {
            VBDesign vbDesign = new VBDesign();
            vbDesign.DataContext = forObject;
            vbDesign.VBContent = "*" + acClassDesignName;

            VBDesignList.Add(vbDesign);
            ShowVBDesign(vbDesign);
        }

        public void StartBusinessobject(string acUrl, ACValueList parameterList, string acCaption, bool ribbonVisibilityOff = false, Global.VBDesignDockState dockState = Global.VBDesignDockState.Tabbed)
        {
            if ((ContextACObject != null))
            {
                if (acUrl.IndexOf('#') != -1)
                {
                    string checkACUrl = acUrl.Replace("#", "\\?");
                    var x = ContextACObject.ACUrlCommand(checkACUrl);
                    if (x != null)
                        return;
                }
                VBDesign vbDesign = new VBDesign();
                vbDesign.Name = String.Format("BSO{0}", VBDesignList.Count);
                if (!string.IsNullOrEmpty(acCaption))
                    vbDesign.ACCaption = acCaption;

                vbDesign.AutoStartACComponent = acUrl;
                vbDesign.AutoStartParameter = parameterList;


                VBDesignList.Add(vbDesign);
                ShowVBDesign(vbDesign);
            }
        }

        private void ShowVBDesign(UIElement uiElement, string acCaption = "")
        {
            if (uiElement == null)
                return;
            IVBContent uiElementAsDataContent = null;
            if (uiElement is IVBContent)
            {
                uiElementAsDataContent = (uiElement as IVBContent);
                if (uiElementAsDataContent.ContextACObject == null)
                {
                    if (uiElement is FrameworkElement)
                    {
                        if ((uiElement as FrameworkElement).DataContext == null)
                            (uiElement as FrameworkElement).DataContext = this.ContextACObject;
                    }
                }
                if (uiElementAsDataContent.ContextACObject == null)
                    return;
            }

            if (uiElement is VBDesign)
            {
                VBDesign uiElementAsDataDesign = (uiElement as VBDesign);
                // Rechtepr�fung ob Design ge�ffnet werden darf
                if (uiElementAsDataContent != null && uiElementAsDataDesign.ContentACObject != null && uiElementAsDataDesign.ContentACObject is IACType)
                {
                    if (uiElementAsDataContent.ContextACObject is IACComponent)
                    {
                        if (((ACClass)uiElementAsDataContent.ContextACObject.ACType).RightManager.GetControlMode(uiElementAsDataDesign.ContentACObject as IACType) != Global.ControlModes.Enabled)
                            return;
                    }
                }
            }

            ChangeFrameContent(uiElement);
        }
    }
}
