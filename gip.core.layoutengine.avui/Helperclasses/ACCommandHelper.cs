using Avalonia.Interactivity;
#nullable enable
using Avalonia.Labs.Input;
using Avalonia.Threading;
using gip.core.datamodel;
using ReactiveUI;
// ReactiveUI 24.x (pinned in both fork and NuGet mode) uses RxVoid instead of Unit
using RxVoid = ReactiveUI.Primitives.RxVoid;
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
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
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

            ICommand? _Command = null;
            public ICommand? Command
            {
                get { return _Command; }
            }

            /// <summary>
            /// If the command is a ReactiveCommand, returns it as such.
            /// </summary>
            public IReactiveCommand? ReactiveCommand
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
        public static Result? ApplyACCommand(IACCommandControl control, IACComponent? interactiveObject, string methodNameForCommand, ACCommand acCommmand, ILogical? logical, 
            EventHandler<ExecutedRoutedEventArgs>? executeDelegate = null, EventHandler<CanExecuteRoutedEventArgs>? canExecuteDelegate = null)
        {
            if (interactiveObject == null)
                interactiveObject = control.ContextACObject as IACComponent;
            if (interactiveObject == null || control.Command != null)
                return null;

            Result? result = null;
            bool isBuiltInGesture = false;
            System.Windows.Input.ICommand? iCommand = AppCommands.FindVBApplicationCommand(acCommmand.ACUrl);
            KeyGesture? keyGesture = null;
            if (control.HotKey == null)
            {
                RoutedCommand? routedCmd = iCommand as RoutedCommand;
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
                        INotifyPropertyChanged? observableObject = penultimateSegment.Value as INotifyPropertyChanged;
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

                result = new Result(CreateACCommand(() => ReactiveExecuteCommand(control, methodNameForCommand, interactiveObject), methodNameForCommand, interactiveObject, observablePropertyTuples), false);
            }
            // Fallback to CommandBinding approach
            else
            {
                if (logical == null)
                    logical = control;
                if (logical == null)
                    return null;
                InputElement? topLevel = null;
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
        /// Creates an ICommand that follows the AC canExecute pipeline without ReactiveCommand's
        /// transient execution state. ReactiveCommand disables itself (CanExecuteChanged=false)
        /// while executing and relies on its internal in-flight counter being decremented when the
        /// execution completes. With synchronous/blocking execute actions (e.g. sync XHR in the
        /// browser WASM client) the balancing execution-end may never be processed, leaving the
        /// command permanently disabled - which excludes the bound Button from hit-testing.
        /// </summary>
        public static System.Windows.Input.ICommand CreateACCommand(
            Action executeAction,
            string canExecuteMethodName,
            IACComponent propertyOwner,
            IEnumerable<Tuple<INotifyPropertyChanged, string>> propertiesToObserve)
        {
            var manualRequeryTrigger = new Subject<RxVoid>();

            var propertyObservables = propertiesToObserve
                .Where(prop => prop != null)
                .Select(prop => ObservePropertyChangedOnUiThread(prop.Item1, prop.Item2))
                .ToArray();

            var canExecuteObservable = BuildCanExecuteObservable(canExecuteMethodName, propertyOwner, propertyObservables, manualRequeryTrigger);

            return new ACCanExecuteCommand(canExecuteObservable, WrapExecuteAction(executeAction, manualRequeryTrigger), canExecuteMethodName);
        }

        /// <summary>
        /// Minimal ICommand that caches the latest value of the canExecute pipeline and raises
        /// CanExecuteChanged when it changes. Execution always runs the given delegate.
        /// </summary>
        private sealed class ACCanExecuteCommand : System.Windows.Input.ICommand
        {
            private readonly Action _execute;
            private readonly string _methodName;
            private bool _canExecute = true;

            public event EventHandler? CanExecuteChanged;

            public ACCanExecuteCommand(IObservable<bool> canExecuteObservable, Action execute, string methodName)
            {
                _execute = execute;
                _methodName = methodName;
                canExecuteObservable.Subscribe(
                    value =>
                    {
                        if (_canExecute != value)
                        {
                            _canExecute = value;
                            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                        }
                    },
                    ex =>
                    {
                        // Keep the last canExecute value; do not let a pipeline error
                        // permanently disable the command.
                    });
            }

            public bool CanExecute(object? parameter) => _canExecute;

            public void Execute(object? parameter) => _execute();
        }

        /// <summary>
        /// Creates a ReactiveCommand that observes specified properties and evaluates a canExecute method dynamically.
        /// </summary>
        /// <param name="executeAction">The action to execute when the command is invoked</param>
        /// <param name="canExecuteMethodName">Name of the method that determines if the command can execute (e.g., "IsEnabledUnassignACProject")</param>
        /// <param name="propertiesToObserve">Names of properties to observe for changes</param>
        /// <returns>A ReactiveCommand configured with the specified behavior</returns>
        public static ReactiveCommand<RxVoid, RxVoid> CreateReactiveCommand(
            Action executeAction,
            string canExecuteMethodName,
            IACComponent propertyOwner,
            IEnumerable<string> propertiesToObserve)
        {
            var manualRequeryTrigger = new Subject<RxVoid>();

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

        public static ReactiveCommand<RxVoid, RxVoid> CreateReactiveCommand(
            Action executeAction,
            string canExecuteMethodName,
            IACComponent propertyOwner,
            IEnumerable<Tuple<INotifyPropertyChanged, string>> propertiesToObserve)
        {
            var manualRequeryTrigger = new Subject<RxVoid>();

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

        private static Action WrapExecuteAction(Action executeAction, IObserver<RxVoid> manualRequeryTrigger)
        {
            return () =>
            {
                executeAction();
                TraceReactiveCommand("Manual requery trigger after execute");
                manualRequeryTrigger.OnNext(RxVoid.Default);
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
            IObservable<RxVoid>[] propertyObservables,
            IObservable<RxVoid> manualRequeryTrigger)
        {
            int evaluateInFlight = 0;

            var triggerStream = Observable.Merge<RxVoid>(propertyObservables.Concat(new[] { manualRequeryTrigger }))
                .Synchronize()
                .Throttle(TimeSpan.FromMilliseconds(50))
                .StartWith(RxVoid.Default);

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
                });
            // NOTE: Do NOT apply DistinctUntilChanged here. ReactiveUI disables the command
            // (CanExecuteChanged=false) while it is executing. If the re-evaluated value equals
            // the previously emitted value, the suppressed emission would leave the bound
            // Button stuck with _commandCanExecute=false (IsEffectivelyEnabled=false), which
            // excludes it from hit-testing entirely - the button can never be clicked again.

            return ObserveOnUiThread(backgroundEvaluated);
        }

        private static IObservable<RxVoid> CreateRequerySchedule()
        {
            return Observable.Merge<RxVoid>(
                Observable.Return(RxVoid.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(200)).Select(_ => RxVoid.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(700)).Select(_ => RxVoid.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(1500)).Select(_ => RxVoid.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(2800)).Select(_ => RxVoid.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(4500)).Select(_ => RxVoid.Default),
                Observable.Timer(TimeSpan.FromMilliseconds(7000)).Select(_ => RxVoid.Default));
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
        private static IObservable<RxVoid> ObservePropertyChangedOnUiThread(INotifyPropertyChanged source, string propertyName)
        {
            return Observable.Create<RxVoid>(observer =>
            {
                void Handler(object? sender, PropertyChangedEventArgs e)
                {
                    if (e.PropertyName == propertyName)
                    {
                        TraceReactiveCommand($"PropertyChanged: {source.GetType().Name}.{propertyName}");
                        observer.OnNext(RxVoid.Default);
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
                object? result = propertyOwner.IsEnabledACUrlCommand(methodCmd);
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
                    object? result = methodInfo.Invoke(propertyOwner, null);
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

        private static readonly Dictionary<string, string> PickFileMimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", "image/jpeg" }, { ".jpeg", "image/jpeg" }, { ".png", "image/png" }, { ".gif", "image/gif" },
            { ".bmp", "image/bmp" }, { ".webp", "image/webp" }, { ".pdf", "application/pdf" }, { ".txt", "text/plain" },
            { ".mp3", "audio/mpeg" }, { ".wav", "audio/wav" }, { ".mp4", "video/mp4" }
        };

        private static List<FilePickerFileType>? CreateFilePickerTypes(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return null;
            string[] patterns = filter.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // Android filters by mime type, the other platforms by pattern
            string[] mimeTypes = patterns
                .Select(p => PickFileMimeTypes.TryGetValue(System.IO.Path.GetExtension(p), out string? mime) ? mime : null)
                .Where(m => m != null)
                .Distinct()
                .ToArray()!;
            return new List<FilePickerFileType>
            {
                new FilePickerFileType(string.Join(", ", patterns)) { Patterns = patterns, MimeTypes = mimeTypes.Length > 0 ? mimeTypes : null }
            };
        }

        /// <summary>
        /// Lets the user pick a file on the device and calls the server method with the file name and the file content.
        /// </summary>
        private static async Task PickFileAndExecuteAsync(VBButton button, string methodName, IACComponent propertyOwner)
        {
            try
            {
                IStorageProvider? provider = TopLevel.GetTopLevel(button)?.StorageProvider;
                if (provider == null || !provider.CanOpen)
                    return;

                IReadOnlyList<IStorageFile> files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    AllowMultiple = false,
                    FileTypeFilter = CreateFilePickerTypes(button.PickFileFilter)
                });
                if (files.Count == 0)
                    return;

                IStorageFile file = files[0];
                byte[] content;
                using (System.IO.Stream stream = await file.OpenReadAsync())
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    content = memoryStream.ToArray();
                }

                string methodCmd = methodName.StartsWith(ACUrlHelper.Delimiter_InvokeMethod) ? methodName : ACUrlHelper.Delimiter_InvokeMethod + methodName;
                // Stays on the UI thread: the method changes bound properties (e.g. the list), and their change notifications must not be raised on a background thread
                System.Diagnostics.Debug.WriteLine($"PickFileAndExecuteAsync: send {file.Name} ({content.Length} bytes) to {methodCmd}");
                propertyOwner.ACUrlCommand(methodCmd, file.Name, content);
                System.Diagnostics.Debug.WriteLine($"PickFileAndExecuteAsync: {methodCmd} returned");
            }
            catch (Exception ex)
            {
                Database.Root.Messages.LogException(nameof(ACCommandHelper), nameof(PickFileAndExecuteAsync), ex);
                System.Diagnostics.Debug.WriteLine($"PickFileAndExecuteAsync failed: {ex}");
                await Database.Root.Messages.ErrorAsync(propertyOwner, ex.Message, true);
            }
        }

        /// <summary>
        /// Calls the server method that returns the file and opens the result with the viewer of the device.
        /// </summary>
        private static async Task ExecuteAndOpenFileAsync(VBButton button, string methodName, IACComponent propertyOwner)
        {
            try
            {
                string methodCmd = methodName.StartsWith(ACUrlHelper.Delimiter_InvokeMethod) ? methodName : ACUrlHelper.Delimiter_InvokeMethod + methodName;
                // The remote call is synchronous and must not block the UI thread
                IVBFileContent? file = await Task.Run(() => propertyOwner.ACUrlCommand(methodCmd)) as IVBFileContent;
                if (file == null)
                    return;
                bool opened = await Dispatcher.UIThread.InvokeAsync(() => VBFileOpener.OpenAsync(button, file));
                if (!opened)
                    await Database.Root.Messages.WarningAsync(propertyOwner, "The file cannot be opened on this device.", true);
            }
            catch (Exception ex)
            {
                Database.Root.Messages.LogException(nameof(ACCommandHelper), nameof(ExecuteAndOpenFileAsync), ex);
            }
        }

        private static void ReactiveExecuteCommand(IACCommandControl control, string methodName, IACComponent propertyOwner)
        {
            try
            {
                if (control is VBButton pickFileButton && pickFileButton.PickFileOnClick)
                {
                    _ = PickFileAndExecuteAsync(pickFileButton, methodName, propertyOwner);
                    return;
                }
                if (control is VBButton openFileButton && openFileButton.OpenFileOnClick)
                {
                    _ = ExecuteAndOpenFileAsync(openFileButton, methodName, propertyOwner);
                    return;
                }

                if (Const.IsAvaloniaHotKeyManagerBugPresent && AppCommands.IsBuiltInAppCommand(methodName) && control is Visual visual)
                {
                    TopLevel? topLevel = TopLevel.GetTopLevel(visual);
                    if (topLevel == null)
                        return;
                    IFocusChangeListener? focusChangeListener = topLevel as IFocusChangeListener;
                    if (focusChangeListener == null)
                        focusChangeListener = topLevel.Content as IFocusChangeListener;
                    if (focusChangeListener == null)
                        return;
                    TextBox? inputElement = focusChangeListener.LastFocusedElement as TextBox; // topLevel.FocusManager.GetFocusedElement();
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
        public static void DefaultExecuteHandler(object? sender, ExecutedRoutedEventArgs e)
        {
            if (sender is not IACCommandControl control)
                return;

            RoutedUICommandEx? vbCommand = null;
            if (e != null)
                vbCommand = e.Command as RoutedUICommandEx;

            ACActionArgs? actionArgs = null;
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
        public static void DefaultCanExecuteHandler(object? sender, CanExecuteRoutedEventArgs e)
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

            IACComponent? acComponent = null;
            if (control is IVBContent vbContent)
                acComponent = vbContent.ContextACObject as IACComponent;

            if (control is IACCommandControl cmdControl && cmdControl.Command != null)
                acComponent = cmdControl as IACComponent;

            if (acComponent != null)
            {
                RoutedUICommandEx? vbCommand = e.Command as RoutedUICommandEx;
                ACActionArgs? actionArgs = null;

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

        public static void DefaultCannotExecuteHandler(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        /// <summary>
        /// Helper method to get or create parameter list with CommandParameter
        /// </summary>
        private static ACValueList GetOrCreateParameterList(IACCommandControl control)
        {
            ACValueList? parameterList = null;

            // Try to get existing parameter list
            if (control.ACCommand != null)
                parameterList = control.ACCommand.ParameterList;

            if (parameterList == null)
                parameterList = new ACValueList();

            // Update CommandParameter if needed - check if control has CommandParameter property
            object? commandParameter = null;
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
