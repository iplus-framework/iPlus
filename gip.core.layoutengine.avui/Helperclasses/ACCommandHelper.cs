using Avalonia.Interactivity;
using Avalonia.Labs.Input;
using Avalonia.Threading;
using gip.core.datamodel;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows.Input;
using Avalonia.LogicalTree;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using System.Diagnostics;

namespace gip.core.layoutengine.avui.Helperclasses
{
    public interface ICommandBindingOwner : ILogical
    {
    }

    public interface IFocusChangeListener
    {
        IInputElement LastFocusedElement { get; }
    }

    public interface IACCommandControl : IVBContent, ILogical
    {
        ACCommand ACCommand { get; set; }

        ICommand Command { get; set; }

        KeyGesture HotKey { get; set; }
    }

    public static class ACCommandHelper
    {
        public class Result
        {
            public Result(ICommand command, bool removeExistingAppCommand)
            {
                _Command = command;
                _RemoveExistingAppCommand = removeExistingAppCommand;
            }

            ICommand _Command = null;
            public ICommand Command
            {
                get { return _Command; }
            }

            /// <summary>
            /// If the command is a ReactiveCommand, returns it as such.
            /// </summary>
            public IReactiveCommand ReactiveCommand
            {
                get { return _Command as IReactiveCommand; }
            }

            bool _RemoveExistingAppCommand = false;
            public bool RemoveExistingAppCommand
            {
                get { return _RemoveExistingAppCommand; }
            }
        }

        /// <summary>
        /// Applies an ACCommand to the specified control, either as a ReactiveCommand or via CommandBinding.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="interactiveObject"></param>
        /// <param name="methodNameForCommand"></param>
        /// <param name="acCommmand"></param>
        /// <param name="logical">This or parent ui element, that is attached to the logical tree. From this control the first VBDesign parent is searched. At this VBDesign the CommandBinding will be added if Reactive Command can't be used.</param>
        /// <param name="executeDelegate">Delegate for command execution (optional, uses DefaultExecuteHandler if null)</param>
        /// <param name="canExecuteDelegate">Delegate for checking if command can execute (optional, uses DefaultCanExecuteHandler if null)</param>
        /// <returns>If Command was applied, then Result is not null</returns>
        public static Result ApplyACCommand(IACCommandControl control, IACComponent interactiveObject, string methodNameForCommand, ACCommand acCommmand, ILogical logical, 
            EventHandler<ExecutedRoutedEventArgs> executeDelegate = null, EventHandler<CanExecuteRoutedEventArgs> canExecuteDelegate = null)
        {
            if (interactiveObject == null)
                interactiveObject = control.ContextACObject as IACComponent;
            if (interactiveObject == null || control.Command != null)
                return null;

            Result result = null;
            bool isBuiltInGesture = false;
            System.Windows.Input.ICommand iCommand = AppCommands.FindVBApplicationCommand(acCommmand.ACUrl);
            KeyGesture keyGesture = null;
            if (control.HotKey == null)
            {
                RoutedCommand routedCmd = iCommand as RoutedCommand;
                if (routedCmd != null && routedCmd.Gestures != null && routedCmd.Gestures.Any())
                {
                    keyGesture = routedCmd.Gestures[0] as KeyGesture;
                    if (keyGesture != null)
                    {
                        isBuiltInGesture = AppCommands.IsBuiltInGesture(keyGesture);
                        if (!isBuiltInGesture || !Const.IsAvaloniaHotKeyManagerBugPresent)
                            control.HotKey = keyGesture;
                    }
                }
            }

            // Determine observable properties for ReactiveCommand
            methodNameForCommand = methodNameForCommand.Replace(ACUrlHelper.Delimiter_InvokeMethod.ToString(), string.Empty);
            IEnumerable<string> observableProperties = interactiveObject.GetPropsToObserveForIsEnabled(methodNameForCommand);
            // If there are observable properties, create a ReactiveCommand
            if (observableProperties != null && observableProperties.Any())
            {
                List<Tuple<INotifyPropertyChanged, string>> observablePropertyTuples = new List<Tuple<INotifyPropertyChanged, string>>();
                foreach (var vbContent in observableProperties)
                {
                    ACUrlTypeInfo acUrlTypeInfo = new ACUrlTypeInfo();
                    interactiveObject.ACUrlTypeInfo(vbContent, ref acUrlTypeInfo);
                    int count = acUrlTypeInfo.Count;
                    while (count >= 2)
                    {
                        var lastSegment = acUrlTypeInfo[count - 1];
                        var penultimateSegment = acUrlTypeInfo[count - 2];
                        string path = lastSegment.SegmentName;
                        INotifyPropertyChanged observableObject = penultimateSegment.Value as INotifyPropertyChanged;
                        if (lastSegment.Property != null && lastSegment.Property is IACPropertyNetBase netProp)
                        {
                            observableObject = netProp;
                            path = nameof(IACMember.Value);
                        }
                        if (observableObject != null)
                        {
                            observablePropertyTuples.Add(new Tuple<INotifyPropertyChanged, string>(observableObject, path));
                            break;
                        }
                        count--;
                    }
                }

                result = new Result(CreateReactiveCommand(() => ReactiveExecuteCommand(control, methodNameForCommand, interactiveObject), methodNameForCommand, interactiveObject, observablePropertyTuples), false);
            }
            // Fallback to CommandBinding approach
            else
            {
                if (logical == null)
                    logical = control;
                if (logical == null)
                    return null;
                InputElement topLevel = null;
                if (topLevel == null)
                    topLevel = logical.FindLogicalAncestorOfType<VBDockingManager>() as InputElement;
                if (logical is ICommandBindingOwner)
                    topLevel = logical as InputElement;
                if (topLevel == null)
                    topLevel = logical.FindLogicalAncestorOfType<ICommandBindingOwner>() as InputElement;
                if (topLevel == null)
                    topLevel = TopLevel.GetTopLevel(logical as Visual);
                if (topLevel == null)
                    return null;
                bool removeExistingCommand = false;
                if (iCommand == null)
                {
                    removeExistingCommand = true;
                    iCommand = AppCommands.AddApplicationCommand(acCommmand);
                }
                if (control.ACCommand == null || control.ACCommand != acCommmand)
                    control.ACCommand = acCommmand;
                IList<CommandBinding> commandBindings = CommandManager.GetCommandBindings(topLevel);
                if (commandBindings == null)
                    commandBindings = new List<CommandBinding>();

                CommandBinding cb = new CommandBinding();
                cb.Command = iCommand;
                        
                // Use default handlers if not provided
                cb.Executed += executeDelegate ?? DefaultExecuteHandler;
                if (canExecuteDelegate != null)
                    cb.CanExecute += canExecuteDelegate;
                else
                    cb.CanExecute += isBuiltInGesture ? DefaultCannotExecuteHandler : DefaultCanExecuteHandler;

                if (!isBuiltInGesture)
                {
                    commandBindings.Add(cb);
                    CommandManager.SetCommandBindings(topLevel, commandBindings);
                }

                result = new Result(iCommand, removeExistingCommand);
            }

            if (result != null && result.Command != null && (!isBuiltInGesture || Const.IsAvaloniaHotKeyManagerBugPresent))
            {
                control.Command = result.Command;
            }

            return result;
        }

        #region Reactive Command
        /// <summary>
        /// Creates a ReactiveCommand that observes specified properties and evaluates a canExecute method dynamically.
        /// </summary>
        /// <param name="executeAction">The action to execute when the command is invoked</param>
        /// <param name="canExecuteMethodName">Name of the method that determines if the command can execute (e.g., "IsEnabledUnassignACProject")</param>
        /// <param name="propertiesToObserve">Names of properties to observe for changes</param>
        /// <returns>A ReactiveCommand configured with the specified behavior</returns>
        public static ReactiveCommand<Unit, Unit> CreateReactiveCommand(
            Action executeAction,
            string canExecuteMethodName,
            IACComponent propertyOwner,
            IEnumerable<string> propertiesToObserve)
        {
            var manualRequeryTrigger = new Subject<Unit>();

            // Create observables for each property, marshaling PropertyChanged events from background threads
            // (e.g., server communication threads) to the UI thread.
            // Use non-blocking dispatch so communication threads do not wait on UI work,
            // which can deadlock when CanExecute evaluation performs synchronous remote calls.
            var propertyObservables = propertiesToObserve
                .Select(propName => propertyOwner.GetProperty(propName))
                .Where(prop => prop != null)
                .Select(prop => ObservePropertyChangedOnUiThread(propertyOwner, prop.ACIdentifier))
                .ToArray();

            var canExecuteObservable = BuildCanExecuteObservable(canExecuteMethodName, propertyOwner, propertyObservables, manualRequeryTrigger);

            return ReactiveCommand.Create(WrapExecuteAction(executeAction, manualRequeryTrigger), canExecuteObservable);
        }

        public static ReactiveCommand<Unit, Unit> CreateReactiveCommand(
            Action executeAction,
            string canExecuteMethodName,
            IACComponent propertyOwner,
            IEnumerable<Tuple<INotifyPropertyChanged, string>> propertiesToObserve)
        {
            var manualRequeryTrigger = new Subject<Unit>();

            // Create observables for each property, marshaling PropertyChanged events from background threads
            // (e.g., server communication threads) to the UI thread.
            // Use non-blocking dispatch so communication threads do not wait on UI work,
            // which can deadlock when CanExecute evaluation performs synchronous remote calls.
            var propertyObservables = propertiesToObserve
                .Where(prop => prop != null)
                .Select(prop => ObservePropertyChangedOnUiThread(prop.Item1, prop.Item2))
                .ToArray();

            var canExecuteObservable = BuildCanExecuteObservable(canExecuteMethodName, propertyOwner, propertyObservables, manualRequeryTrigger);

            return ReactiveCommand.Create(WrapExecuteAction(executeAction, manualRequeryTrigger), canExecuteObservable);
        }

        private static Action WrapExecuteAction(Action executeAction, IObserver<Unit> manualRequeryTrigger)
        {
            return () =>
            {
                executeAction();
                TraceReactiveCommand("Manual requery trigger after execute");
                manualRequeryTrigger.OnNext(Unit.Default);
            };
        }

        /// <summary>
        /// Builds a CanExecute stream that coalesces bursts of property changes and performs
        /// remote IsEnabled checks off the UI thread, then marshals only the boolean result
        /// back to the UI thread.
        /// </summary>
        private static IObservable<bool> BuildCanExecuteObservable(
            string canExecuteMethodName,
            IACComponent propertyOwner,
            IObservable<Unit>[] propertyObservables,
            IObservable<Unit> manualRequeryTrigger)
        {
            int evaluateInFlight = 0;

            var triggerStream = Observable.Merge(propertyObservables.Concat(new[] { manualRequeryTrigger }))
                .Synchronize()
                .Throttle(TimeSpan.FromMilliseconds(50))
                .StartWith(Unit.Default);

            // Re-evaluate immediately and then at sparse delayed checkpoints.
            // This keeps late-state reliability while reducing remote IsEnabled traffic.
            var requeryTicks = triggerStream
                .Select(_ => CreateRequerySchedule())
                .Switch();

            var backgroundEvaluated = requeryTicks
                .SelectMany(_ =>
                {
                    if (Interlocked.CompareExchange(ref evaluateInFlight, 1, 0) != 0)
                    {
                        TraceReactiveCommand($"Skip requery tick for {canExecuteMethodName}: evaluation already running");
                        return Observable.Empty<bool>();
                    }

                    return EvaluateCanExecuteOnUiThreadAsync(canExecuteMethodName, propertyOwner)
                        .Finally(() => Interlocked.Exchange(ref evaluateInFlight, 0));
                })
                .DistinctUntilChanged();

            return ObserveOnUiThread(backgroundEvaluated);
        }

        private static IObservable<Unit> CreateRequerySchedule()
        {
            return Observable.Merge(
                Observable.Return(Unit.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(200)).Select(_ => Unit.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(700)).Select(_ => Unit.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(1500)).Select(_ => Unit.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(2800)).Select(_ => Unit.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(4500)).Select(_ => Unit.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(7000)).Select(_ => Unit.Default));
        }

        private static IObservable<bool> EvaluateCanExecuteOnUiThreadAsync(string canExecuteMethodName, IACComponent propertyOwner)
        {
            return Observable.Create<bool>(observer =>
            {
                void Evaluate()
                {
                    try
                    {
                        TraceReactiveCommand($"Requery tick for {canExecuteMethodName}");
                        TraceReactiveCommand($"Evaluate on UI for {canExecuteMethodName}");
                        var isEnabled = ReactiveEvaluateCanExecute(canExecuteMethodName, propertyOwner);
                        TraceReactiveCommand($"Evaluate result for {canExecuteMethodName}: {isEnabled}");
                        observer.OnNext(isEnabled);
                        observer.OnCompleted();
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                }

                if (Dispatcher.UIThread.CheckAccess())
                    Evaluate();
                else
                    Dispatcher.UIThread.Post(Evaluate, DispatcherPriority.Send);

                return Disposable.Empty;
            });
        }

        /// <summary>
        /// Wraps a PropertyChanged event subscription so that notifications are dispatched
        /// to the UI thread in a non-blocking way, ensuring cross-thread safety while
        /// keeping communication threads free to process synchronous server responses.
        /// </summary>
        private static IObservable<Unit> ObservePropertyChangedOnUiThread(INotifyPropertyChanged source, string propertyName)
        {
            return Observable.Create<Unit>(observer =>
            {
                void Handler(object sender, PropertyChangedEventArgs e)
                {
                    if (e.PropertyName == propertyName)
                    {
                        TraceReactiveCommand($"PropertyChanged: {source.GetType().Name}.{propertyName}");
                        observer.OnNext(Unit.Default);
                    }
                }
                source.PropertyChanged += Handler;
                return Disposable.Create(() => source.PropertyChanged -= Handler);
            });
        }

        /// <summary>
        /// Marshals an observable sequence to the UI thread without blocking the source thread.
        /// </summary>
        private static IObservable<T> ObserveOnUiThread<T>(IObservable<T> source)
        {
            return Observable.Create<T>(observer => source.Subscribe(
                value =>
                {
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        TraceReactiveCommand($"Deliver CanExecute on UI: {value}");
                        observer.OnNext(value);
                    }
                    else
                        Dispatcher.UIThread.Post(() =>
                        {
                            TraceReactiveCommand($"Deliver CanExecute (posted) on UI: {value}");
                            observer.OnNext(value);
                        }, DispatcherPriority.Send);
                },
                error =>
                {
                    if (Dispatcher.UIThread.CheckAccess())
                        observer.OnError(error);
                    else
                        Dispatcher.UIThread.Post(() => observer.OnError(error), DispatcherPriority.Send);
                },
                () =>
                {
                    if (Dispatcher.UIThread.CheckAccess())
                        observer.OnCompleted();
                    else
                        Dispatcher.UIThread.Post(observer.OnCompleted, DispatcherPriority.Send);
                }));
        }

        [Conditional("ACCOMMANDHELPER_TRACE")]
        private static void TraceReactiveCommand(string message)
        {
            Debug.WriteLine($"[ACCommandHelper] {message}; thread={Environment.CurrentManagedThreadId}; ui={Dispatcher.UIThread.CheckAccess()}");
        }

        /// <summary>
        /// Evaluates the canExecute method by name using reflection or ACUrlCommand.
        /// Falls back to reflection if ACUrlCommand is not available.
        /// </summary>
        /// <param name="methodName">Name of the method to evaluate</param>
        /// <returns>Boolean result of the method invocation</returns>
        private static bool ReactiveEvaluateCanExecute(string methodName, IACComponent propertyOwner)
        {
            try
            {
                if (Const.IsAvaloniaHotKeyManagerBugPresent && AppCommands.IsBuiltInAppCommand(methodName))
                    return true;
                string methodCmd = methodName;
                if (!methodCmd.StartsWith(ACUrlHelper.Delimiter_InvokeMethod))
                    methodCmd = ACUrlHelper.Delimiter_InvokeMethod + methodName;
                // Try using ACUrlCommand first (preferred method)
                object result = propertyOwner.IsEnabledACUrlCommand(methodCmd);
                if (result is bool boolResult)
                    return boolResult;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                System.Diagnostics.Debug.WriteLine($"ACUrlCommand failed for {methodName}: {ex.Message}");
            }

            try
            {
                // Fallback to reflection
                methodName = Const.IsEnabled + methodName;
                var methodInfo = propertyOwner.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (methodInfo != null && methodInfo.ReturnType == typeof(bool))
                {
                    object result = methodInfo.Invoke(propertyOwner, null);
                    if (result is bool boolResult)
                        return boolResult;
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                System.Diagnostics.Debug.WriteLine($"Reflection failed for {methodName}: {ex.Message}");
            }

            // Default to false if evaluation fails
            return false;
        }

        private static void ReactiveExecuteCommand(IACCommandControl control, string methodName, IACComponent propertyOwner)
        {
            try
            {
                if (Const.IsAvaloniaHotKeyManagerBugPresent && AppCommands.IsBuiltInAppCommand(methodName) && control is Visual visual)
                {
                    TopLevel topLevel = TopLevel.GetTopLevel(visual);
                    if (topLevel == null)
                        return;
                    IFocusChangeListener focusChangeListener = topLevel as IFocusChangeListener;
                    if (focusChangeListener == null)
                        focusChangeListener = topLevel.Content as IFocusChangeListener;
                    if (focusChangeListener == null)
                        return;
                    TextBox inputElement = focusChangeListener.LastFocusedElement as TextBox; // topLevel.FocusManager.GetFocusedElement();
                    if (inputElement == null)
                        return;
                    // Select all because Avalonia doesn't have a separate Keyboard-Focus and Logical Focus like WPF
                    if (methodName == Const.CmdNameCopy || methodName == Const.CmdNameCut)
                        inputElement.SelectAll();
                    //else if (methodName == Const.CmdNamePaste)
                    //    inputElement.MoveEnd(true);
                    KeyGesture keyGesture = AppCommands.GetBuiltInKeyGesture(methodName);
                    if (keyGesture != null)
                    {
                        inputElement.RaiseEvent(new KeyEventArgs
                        {
                            RoutedEvent = InputElement.KeyDownEvent,
                            Key = keyGesture.Key,
                            KeyModifiers = keyGesture.KeyModifiers
                        });
                    }
                    return;
                }

                string methodCmd = methodName;
                if (!methodCmd.StartsWith(ACUrlHelper.Delimiter_InvokeMethod))
                    methodCmd = ACUrlHelper.Delimiter_InvokeMethod + methodName;
                // Try using ACUrlCommand first (preferred method)
                propertyOwner.ACUrlCommand(methodCmd);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                System.Diagnostics.Debug.WriteLine($"ACUrlCommand failed for {methodName}: {ex.Message}");
            }

            try
            {
                // Fallback to reflection
                var methodInfo = propertyOwner.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (methodInfo != null && methodInfo.ReturnType == typeof(bool))
                {
                    methodInfo.Invoke(propertyOwner, null);
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                System.Diagnostics.Debug.WriteLine($"Reflection failed for {methodName}: {ex.Message}");
            }
        }
        #endregion


        #region CommandBinding Handlers

        /// <summary>
        /// Default execute handler for AC commands via CommandBinding
        /// </summary>
        public static void DefaultExecuteHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is not IACCommandControl control)
                return;

            RoutedUICommandEx vbCommand = null;
            if (e != null)
                vbCommand = e.Command as RoutedUICommandEx;

            ACActionArgs actionArgs = null;
            if (vbCommand != null)
            {
                if (vbCommand.ACCommand.HandlerACElement != null && vbCommand.ACCommand.HandlerACElement != control)
                {
                    actionArgs = new ACActionArgs(control, 0, 0, Global.ElementActionType.ACCommand);
                    vbCommand.ACCommand.HandlerACElement.ACAction(actionArgs);
                    return;
                }
            }

            if (actionArgs == null)
            {
                // Handle CommandParameter updates
                if (control is IVBContent vbContent)
                {
                    var parameterList = GetOrCreateParameterList(control);

                    if (vbContent.ContextACObject is IACInteractiveObject interactiveObject)
                    {
                        actionArgs = new ACActionArgs(control, 0, 0, Global.ElementActionType.ACCommand);
                        interactiveObject.ACAction(actionArgs);
                    }
                }
            }
        }

        /// <summary>
        /// Default canExecute handler for AC commands via CommandBinding
        /// </summary>
        public static void DefaultCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is not IACCommandControl control)
            {
                e.CanExecute = false;
                return;
            }

            if (sender is Control uiControl && (!uiControl.IsVisible || (control as IVBContent)?.RightControlMode <= Global.ControlModes.Disabled))
            {
                e.CanExecute = false;
                return;
            }

            IACComponent acComponent = null;
            if (control is IVBContent vbContent)
                acComponent = vbContent.ContextACObject as IACComponent;

            if (control is IACCommandControl cmdControl && cmdControl.Command != null)
                acComponent = cmdControl as IACComponent;

            if (acComponent != null)
            {
                RoutedUICommandEx vbCommand = e.Command as RoutedUICommandEx;
                ACActionArgs actionArgs = null;

                if (vbCommand != null)
                {
                    if (vbCommand.ACCommand.HandlerACElement != null && vbCommand.ACCommand.HandlerACElement != control)
                    {
                        actionArgs = new ACActionArgs(control, 0, 0, Global.ElementActionType.ACCommand);
                        if (sender is Control ctrl && ctrl.IsLoaded)
                            e.CanExecute = vbCommand.ACCommand.HandlerACElement.IsEnabledACAction(actionArgs);
                    }
                }

                if (actionArgs == null)
                {
                    var parameterList = GetOrCreateParameterList(control);

                    actionArgs = new ACActionArgs(control, 0, 0, Global.ElementActionType.ACCommand);
                    if (sender is Control ctrl && ctrl.IsLoaded)
                    {
                        e.CanExecute = acComponent.IsEnabledACAction(actionArgs);
                        RemoteCommandAdornerManager.Instance.VisualizeIfRemoteControlled(ctrl, acComponent, true);
                    }
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        public static void DefaultCannotExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        /// <summary>
        /// Helper method to get or create parameter list with CommandParameter
        /// </summary>
        private static ACValueList GetOrCreateParameterList(IACCommandControl control)
        {
            ACValueList parameterList = null;

            // Try to get existing parameter list
            if (control.ACCommand != null)
                parameterList = control.ACCommand.ParameterList;

            if (parameterList == null)
                parameterList = new ACValueList();

            // Update CommandParameter if needed - check if control has CommandParameter property
            object commandParameter = null;
            var commandParamProperty = control.GetType().GetProperty("CommandParameter");
            if (commandParamProperty != null)
            {
                commandParameter = commandParamProperty.GetValue(control);
            }

            if (commandParameter != null)
            {
                foreach (ACValue valueItem in parameterList.ToArray())
                {
                    if (valueItem.ACIdentifier == "CommandParameter")
                        parameterList.Remove(valueItem);
                }
                parameterList.Add(new ACValue("CommandParameter", commandParameter));

                if (control.ACCommand != null)
                    control.ACCommand.ParameterList = parameterList;
            }

            return parameterList;
        }
        
        #endregion

    }
}
