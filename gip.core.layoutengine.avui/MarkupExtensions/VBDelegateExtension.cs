using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui
{
    //[Localizability(LocalizationCategory.NeverLocalize)]
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

            Delegate eventDelegate = null;
            AvaloniaObject target = service.TargetObject as AvaloniaObject;
            if (service.TargetProperty is string eventName)
            {
                EventInfo eventInfoByName = target?.GetType().GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                if (target is Interactive interactiveForNamedEvent)
                {
                    var strategy = HandlePreviewEvents ? RoutingStrategies.Tunnel : RoutingStrategies.Bubble;
                    if (eventName == nameof(IInputElement.PointerEntered))
                    {
                        interactiveForNamedEvent.AddHandler(InputElement.PointerEnteredEvent, (EventHandler<PointerEventArgs>)OnPointerEvent, strategy, HandledEventsToo);
                        return CreateDummyDelegateForEventName(eventName);
                    }
                    else if (eventName == nameof(IInputElement.PointerExited))
                    {
                        interactiveForNamedEvent.AddHandler(InputElement.PointerExitedEvent, (EventHandler<PointerEventArgs>)OnPointerEvent, strategy, HandledEventsToo);
                        return CreateDummyDelegateForEventName(eventName);
                    }
                    else if (eventName == nameof(IInputElement.PointerMoved))
                    {
                        interactiveForNamedEvent.AddHandler(InputElement.PointerMovedEvent, (EventHandler<PointerEventArgs>)OnPointerEvent, strategy, HandledEventsToo);
                        return CreateDummyDelegateForEventName(eventName);
                    }
                    else if (eventName == nameof(IInputElement.KeyDown))
                    {
                        interactiveForNamedEvent.AddHandler(InputElement.KeyDownEvent, (EventHandler<KeyEventArgs>)OnKeyEvent, strategy, HandledEventsToo);
                        return CreateDummyDelegateForEventName(eventName);
                    }
                    else if (eventName == nameof(IInputElement.KeyUp))
                    {
                        interactiveForNamedEvent.AddHandler(InputElement.KeyUpEvent, (EventHandler<KeyEventArgs>)OnKeyEvent, strategy, HandledEventsToo);
                        return CreateDummyDelegateForEventName(eventName);
                    }
                    else if (eventName == nameof(IInputElement.PointerPressed))
                    {
                        interactiveForNamedEvent.AddHandler(InputElement.PointerPressedEvent, (EventHandler<PointerPressedEventArgs>)OnPointerPressedEvent, strategy, HandledEventsToo);
                        return CreateDummyDelegateForEventName(eventName);
                    }
                }

                if (HandlePreviewEvents && target is Interactive interactive)
                {
                    if (eventName == nameof(IInputElement.PointerEntered))
                        interactive.AddHandler(InputElement.PointerEnteredEvent, (EventHandler<PointerEventArgs>)OnPointerEvent, RoutingStrategies.Tunnel, HandledEventsToo);
                    else if (eventName == nameof(IInputElement.PointerExited))
                        interactive.AddHandler(InputElement.PointerExitedEvent, (EventHandler<PointerEventArgs>)OnPointerEvent, RoutingStrategies.Tunnel, HandledEventsToo);
                    else if (eventName == nameof(IInputElement.PointerMoved))
                        interactive.AddHandler(InputElement.PointerMovedEvent, (EventHandler<PointerEventArgs>)OnPointerEvent, RoutingStrategies.Tunnel, HandledEventsToo);
                    else if (eventName == nameof(IInputElement.KeyDown))
                        interactive.AddHandler(InputElement.KeyDownEvent, (EventHandler<KeyEventArgs>)OnKeyEvent, RoutingStrategies.Tunnel, HandledEventsToo);
                    else if (eventName == nameof(IInputElement.KeyUp))
                        interactive.AddHandler(InputElement.KeyUpEvent, (EventHandler<KeyEventArgs>)OnKeyEvent, RoutingStrategies.Tunnel, HandledEventsToo);
                    else if (eventName == nameof(IInputElement.PointerPressed))
                        interactive.AddHandler(InputElement.PointerPressedEvent, (EventHandler<PointerPressedEventArgs>)OnPointerPressedEvent, RoutingStrategies.Tunnel, HandledEventsToo);

                    // Return a matching delegate type so generated XAML IL can cast safely.
                    return CreateDummyDelegateFromEventInfo(eventInfoByName) ?? CreateDummyDelegateForEventName(eventName);
                }

                if (eventName == nameof(IInputElement.PointerEntered)
                    || eventName == nameof(IInputElement.PointerExited)
                    || eventName == nameof(IInputElement.PointerMoved))
                {
                    if (target != null && eventInfoByName != null)
                        eventInfoByName.AddEventHandler(target, new EventHandler<PointerEventArgs>(OnPointerEvent));
                    return CreateDummyDelegateForEventName(eventName);
                }
                else if (eventName == nameof(IInputElement.KeyDown)
                    || eventName == nameof(IInputElement.KeyUp))
                {
                    if (target != null && eventInfoByName != null)
                        eventInfoByName.AddEventHandler(target, new EventHandler<KeyEventArgs>(OnKeyEvent));
                    return CreateDummyDelegateForEventName(eventName);
                }
                else if (eventName == nameof(IInputElement.PointerPressed))
                {
                    if (target != null && eventInfoByName != null)
                        eventInfoByName.AddEventHandler(target, new EventHandler<PointerPressedEventArgs>(OnPointerPressedEvent));
                    return CreateDummyDelegateForEventName(eventName);
                }
                else
                {
                    eventDelegate = CreateDelegateFromEventInfo(eventInfoByName);
                }
            }
            else if (service.TargetProperty is EventInfo eventInfo)
            {
                if (HandlePreviewEvents && target is Interactive interactive)
                {
                    var routedEventField = target.GetType().GetField(eventInfo.Name + "Event", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    if (routedEventField != null)
                    {
                        var routedEvent = routedEventField.GetValue(null) as RoutedEvent;
                        if (routedEvent != null)
                        {
                            interactive.AddHandler(routedEvent, CreateDelegateFromEventInfo(eventInfo), RoutingStrategies.Tunnel, HandledEventsToo);
                            return CreateDummyDelegateFromEventInfo(eventInfo);
                        }
                    }
                }

                eventDelegate = CreateDelegateFromEventInfo(eventInfo);
            }

            if (eventDelegate == null)
            {
                if (HandlePreviewEvents)
                    return null;
                
                throw new ArgumentException("TargetProperty is not an Event");
            }
            return eventDelegate;
        }

        private Delegate CreateDummyDelegateForEventName(string eventName)
        {
            if (eventName == nameof(IInputElement.PointerEntered)
                || eventName == nameof(IInputElement.PointerExited)
                || eventName == nameof(IInputElement.PointerMoved))
            {
                return new EventHandler<PointerEventArgs>(OnPointerEventNoOp);
            }

            if (eventName == nameof(IInputElement.KeyDown)
                || eventName == nameof(IInputElement.KeyUp))
            {
                return new EventHandler<KeyEventArgs>(OnKeyEventNoOp);
            }

            if (eventName == nameof(IInputElement.PointerPressed))
            {
                // XamlIl can choose the EventHandler<RoutedEventArgs> setter overload for PointerPressed.
                // Return a compatible delegate type to avoid runtime InvalidCastException.
                return new EventHandler<RoutedEventArgs>(OnRoutedEvent);
            }

            return Delegate.CreateDelegate(typeof(EventHandler<RoutedEventArgs>), this, ((EventHandler<RoutedEventArgs>)OnRoutedEvent).Method);
        }

        private Delegate CreateDummyDelegateFromEventInfo(EventInfo eventInfo)
        {
            if (eventInfo == null || eventInfo.EventHandlerType == null)
                return null;

            if (typeof(EventHandler<PointerEventArgs>).IsAssignableFrom(eventInfo.EventHandlerType))
            {
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((EventHandler<PointerEventArgs>)OnPointerEventNoOp).Method);
            }

            if (typeof(EventHandler<KeyEventArgs>).IsAssignableFrom(eventInfo.EventHandlerType))
            {
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((EventHandler<KeyEventArgs>)OnKeyEventNoOp).Method);
            }

            if (typeof(EventHandler<PointerPressedEventArgs>).IsAssignableFrom(eventInfo.EventHandlerType))
            {
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((EventHandler<PointerPressedEventArgs>)OnPointerPressedEventNoOp).Method);
            }

            if (typeof(EventHandler<RoutedEventArgs>).IsAssignableFrom(eventInfo.EventHandlerType))
            {
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((EventHandler<RoutedEventArgs>)OnRoutedEvent).Method);
            }

            return null;
        }

        private Delegate CreateDelegateFromEventInfo(EventInfo eventInfo)
        {
            if (typeof(EventHandler<PointerEventArgs>).IsAssignableFrom(eventInfo.EventHandlerType))
            {
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((EventHandler<PointerEventArgs>)OnPointerEvent).Method);
            }
            else if (typeof(EventHandler<KeyEventArgs>).IsAssignableFrom(eventInfo.EventHandlerType))
            {
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((EventHandler<KeyEventArgs>)OnKeyEvent).Method);
            }
            else if (typeof(EventHandler<PointerPressedEventArgs>).IsAssignableFrom(eventInfo.EventHandlerType))
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((EventHandler<PointerPressedEventArgs>)OnPointerPressedEvent).Method);
            else if (typeof(EventHandler<RoutedEventArgs>).IsAssignableFrom(eventInfo.EventHandlerType))
                return Delegate.CreateDelegate(eventInfo.EventHandlerType, this, ((EventHandler<RoutedEventArgs>)OnRoutedEvent).Method);

            return null;
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

        public bool HandlePreviewEvents
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

        public void OnPointerEvent(object sender, PointerEventArgs e)
        {
            if (InvokeAtVBControl != null && sender is AvaloniaObject)
            {
                IACObject vbControl = gip.core.layoutengine.avui.Helperclasses.VBLogicalTreeHelper.FindObjectInLogicalTree(sender as AvaloniaObject, InvokeAtVBControl) as IACObject;
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

        public void OnPointerPressedEvent(object sender, PointerPressedEventArgs e)
        {
            if (OnlyOnDoubleClick)
            {
                if (e.ClickCount < 2)
                    return;
            }
            if (InvokeAtVBControl != null && sender is AvaloniaObject)
            {
                IACObject vbControl = gip.core.layoutengine.avui.Helperclasses.VBLogicalTreeHelper.FindObjectInLogicalTree(sender as AvaloniaObject, InvokeAtVBControl) as IACObject;
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
            if (InvokeAtVBControl != null && sender is AvaloniaObject)
            {
                IACObject vbControl = gip.core.layoutengine.avui.Helperclasses.VBLogicalTreeHelper.FindObjectInLogicalTree(sender as AvaloniaObject, InvokeAtVBControl) as IACObject;
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

        public void OnRoutedEvent(object sender, RoutedEventArgs e)
        {
        }

        public void OnPointerEventNoOp(object sender, PointerEventArgs e)
        {
        }

        public void OnPointerPressedEventNoOp(object sender, PointerPressedEventArgs e)
        {
        }

        public void OnKeyEventNoOp(object sender, KeyEventArgs e)
        {
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
