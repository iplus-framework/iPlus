using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Reflection;
using System.Xaml;
using gip.core.datamodel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace gip.core.layoutengine
{
    [MarkupExtensionReturnType(typeof(Delegate))]
    [Localizability(LocalizationCategory.NeverLocalize)]
    public class VBDelegateExtension : MarkupExtension, IACObject
    {
        public VBDelegateExtension()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            UpdateACComponent();
            IProvideValueTarget service = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            if (service == null)
                throw new ArgumentException("DelegateExtension is used in wrong context");

            DependencyObject target = service.TargetObject as DependencyObject;
            if (!(service.TargetProperty is EventInfo))
                throw new ArgumentException("TargetProperty is not an Event");
            EventInfo eventInfo = service.TargetProperty as EventInfo;
            if (eventInfo.EventHandlerType.Name == "MouseEventHandler")
            {
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((MouseEventHandler)OnMouseEvent).Method);
            }
            else if (eventInfo.EventHandlerType.Name == "KeyEventHandler")
            {
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((KeyEventHandler)OnKeyEvent).Method);
            }
            else
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((MouseButtonEventHandler)OnMouseButtonEvent).Method);
        }

        #region XAML-Properties
        private bool _GlobalFunction;
        public bool GlobalFunction
        {
            get
            {
                return _GlobalFunction;
            }
            set
            {
                _GlobalFunction = value;
            }
        }

        // Property kann nicht ACUrlCommand heissen, da ACUrlCommand bereits eine Methode ist.
        private string _ACUrlCmd;
        /// <summary>
        /// Name of Method which should be called
        /// </summary>
        public string ACUrlCmd
        {
            get
            {
                return _ACUrlCmd;
            }
            set
            {
                _ACUrlCmd = value;
            }
        }

        /// <summary>
        /// If set, then EventArgs are passed as first parameter in ACUrlCommand
        /// </summary>
        public bool PassEventArgs
        {
            get;
            set;
        }

        /// <summary>
        /// If Empty or Null, then ACUrlCommand is called without Parameter
        /// If Not Empty or commasaparated, Parameter-String is splitted and passed as Parameters
        /// </summary>
        public string Parameter
        {
            get;
            set;
        }

        public bool HandledEventsToo
        {
            get;
            set;
        }

        public bool OnlyOnDoubleClick
        {
            get;
            set;
        }

        public Type InvokeAtVBControl
        {
            get;
            set;
        }

        #endregion

        #region public member
        private ACRef<IACComponent> _ACComponentRef;
        public IACComponent ACComponent
        {
            get
            {
                if (_ACComponentRef == null)
                    return null;
                return _ACComponentRef.ValueT;
            }
            set
            {
                if (_ACComponentRef != null)
                    _ACComponentRef.Detach();
                if (value == null)
                    _ACComponentRef = null;
                else
                    _ACComponentRef = new ACRef<IACComponent>(value, this, true);
            }
        }
        #endregion

        #region methods and eventhandler

        public void OnMouseEvent(object sender, MouseEventArgs e)
        {
            if (InvokeAtVBControl != null && sender is DependencyObject)
            {
                IACObject vbControl = gip.core.layoutengine.Helperclasses.VBLogicalTreeHelper.FindObjectInLogicalTree(sender as DependencyObject, InvokeAtVBControl) as IACObject;
                if (vbControl != null)
                {
                    vbControl.ACUrlCommand(ACUrlCmd, null);
                    return;
                }
            } 
            if (ACComponent != null)
            {
                bool handled = false;
                object result;
                if (PassEventArgs)
                {
                    if (String.IsNullOrEmpty(Parameter))
                        result = ACComponent.ACUrlCommand(ACUrlCmd, e);
                    else
                    {
                        string[] sParams = Parameter.Split(',');
                        object[] oParams = new object[sParams.Count() + 1];
                        oParams[0] = e;
                        for (int i = 0; i < sParams.Count(); i++)
                        {
                            oParams[i + 1] = sParams[i];
                        }
                        result = ACComponent.ACUrlCommand(ACUrlCmd, oParams);
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(Parameter))
                        result = ACComponent.ACUrlCommand(ACUrlCmd);
                    else
                    {
                        string[] sParams = Parameter.Split(',');
                        object[] oParams = new object[sParams.Count() + 1];
                        oParams[0] = e;
                        for (int i = 0; i < sParams.Count(); i++)
                        {
                            oParams[i + 1] = sParams[i];
                        }
                        result = ACComponent.ACUrlCommand(ACUrlCmd, oParams);
                    }
                }

                if (result != null)
                    handled = (bool)result;
                if (typeof(RoutedEventArgs).IsAssignableFrom(e.GetType()))
                    ((RoutedEventArgs)e).Handled = handled;
            }
        }

        public void OnMouseButtonEvent(object sender, MouseButtonEventArgs e)
        {
            if (OnlyOnDoubleClick)
            {
                if (e.ClickCount < 2)
                    return;
            }
            if (InvokeAtVBControl != null && sender is DependencyObject)
            {
                IACObject vbControl = gip.core.layoutengine.Helperclasses.VBLogicalTreeHelper.FindObjectInLogicalTree(sender as DependencyObject, InvokeAtVBControl) as IACObject;
                if (vbControl != null)
                {
                    vbControl.ACUrlCommand(ACUrlCmd, null);
                    return;
                }
            }
            if (ACComponent != null)
            {
                bool handled = false;
                object result;
                if (PassEventArgs)
                {
                    if (String.IsNullOrEmpty(Parameter))
                        result = ACComponent.ACUrlCommand(ACUrlCmd, e);
                    else
                    {
                        string[] sParams = Parameter.Split(',');
                        object[] oParams = new object[sParams.Count() + 1];
                        oParams[0] = e;
                        for (int i = 0; i < sParams.Count(); i++)
                        {
                            oParams[i + 1] = sParams[i];
                        }
                        result = ACComponent.ACUrlCommand(ACUrlCmd, oParams);
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(Parameter))
                        result = ACComponent.ACUrlCommand(ACUrlCmd);
                    else
                    {
                        string[] sParams = Parameter.Split(',');
                        object[] oParams = new object[sParams.Count()];
                        //oParams[0] = e;
                        for (int i = 0; i < sParams.Count(); i++)
                        {
                            oParams[i] = sParams[i];
                        }
                        result = ACComponent.ACUrlCommand(ACUrlCmd, oParams);
                    }
                }

                if (result != null)
                    handled = (bool)result;
                if (typeof(RoutedEventArgs).IsAssignableFrom(e.GetType()))
                    ((RoutedEventArgs)e).Handled = handled;
            }
        }


        public void OnKeyEvent(object sender, KeyEventArgs e)
        {
            if (InvokeAtVBControl != null && sender is DependencyObject)
            {
                IACObject vbControl = gip.core.layoutengine.Helperclasses.VBLogicalTreeHelper.FindObjectInLogicalTree(sender as DependencyObject, InvokeAtVBControl) as IACObject;
                if (vbControl != null)
                {
                    vbControl.ACUrlCommand(ACUrlCmd, null);
                    return;
                }
            }

            if (ACComponent != null)
            {
                bool handled = false;
                object result;
                if (PassEventArgs)
                {
                    if (String.IsNullOrEmpty(Parameter))
                        result = ACComponent.ACUrlCommand(ACUrlCmd, e);
                    else
                    {
                        string[] sParams = Parameter.Split(',');
                        object[] oParams = new object[sParams.Count() + 1];
                        oParams[0] = e;
                        for (int i = 0; i < sParams.Count(); i++)
                        {
                            oParams[i + 1] = sParams[i];
                        }
                        result = ACComponent.ACUrlCommand(ACUrlCmd, oParams);
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(Parameter))
                        result = ACComponent.ACUrlCommand(ACUrlCmd);
                    else
                    {
                        string[] sParams = Parameter.Split(',');
                        object[] oParams = new object[sParams.Count() + 1];
                        oParams[0] = e;
                        for (int i = 0; i < sParams.Count(); i++)
                        {
                            oParams[i + 1] = sParams[i];
                        }
                        result = ACComponent.ACUrlCommand(ACUrlCmd, oParams);
                    }
                }

                if (result != null)
                    handled = (bool)result;
                if (typeof(RoutedEventArgs).IsAssignableFrom(e.GetType()))
                    ((RoutedEventArgs)e).Handled = handled;
            }
        }


        private void UpdateACComponent()
        {
            if (GlobalFunction && (Layoutgenerator.Root != null))
                this.ACComponent = Layoutgenerator.Root.Environment;
            else if (!GlobalFunction && Layoutgenerator.CurrentACComponent != null)
                this.ACComponent = Layoutgenerator.CurrentACComponent;
            else
                this.ACComponent = null;
        }
        #endregion

        #region IACObject member
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return ""; }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return null; }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return null; }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return null;
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return false;
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return null; }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return ""; }
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }
        #endregion

    }
}
