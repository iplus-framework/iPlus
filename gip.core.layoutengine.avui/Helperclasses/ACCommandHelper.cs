using Avalonia.Interactivity;
using Avalonia.Labs.Input;
using gip.core.datamodel;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.LogicalTree;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;

namespace gip.core.layoutengine.avui.Helperclasses
{
    public interface ICommandBindingOwner : ILogical
    {
    }

    public interface IACCommandControl : IVBContent, ILogical
    {
        ACCommand ACCommand { get; set; }

        ICommand Command { get; set; }
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
            // Determine observable properties for ReactiveCommand
            methodNameForCommand = methodNameForCommand.Replace(ACUrlHelper.Delimiter_InvokeMethod.ToString(), string.Empty);
            IEnumerable<string> observableProperties = interactiveObject.GetPropsToObserveForIsEnabled(methodNameForCommand);
            // If there are observable properties, create a ReactiveCommand
            if (observableProperties != null && observableProperties.Any())
            {
                List<Tuple<INotifyPropertyChanged, string>> observablePropertyTuples = new List<Tuple<INotifyPropertyChanged, string>>();
                foreach (var vbContent in observableProperties)
                {
                    //IACType dcACTypeInfo = null;
                    //object dcSource = null;
                    //string dcPath = "";
                    //Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;
                    //if (interactiveObject.ACUrlBinding(vbContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                    //{
                    //    INotifyPropertyChanged observableObject = dcSource as INotifyPropertyChanged;
                    //    if (observableObject != null)
                    //    {
                    //        observablePropertyTuples.Add(new Tuple<INotifyPropertyChanged, string>(observableObject, dcPath));
                    //    }
                    //}
                    ACUrlTypeInfo acUrlTypeInfo = new ACUrlTypeInfo();
                    interactiveObject.ACUrlTypeInfo(vbContent, ref acUrlTypeInfo);
                    int count = acUrlTypeInfo.Count;
                    if (count < 2)
                        continue;
                    var lastSegment = acUrlTypeInfo[count - 1];
                    var penultimateSegment = acUrlTypeInfo[count - 2];
                    if (penultimateSegment.Value is INotifyPropertyChanged observableObject)
                    {
                        observablePropertyTuples.Add(new Tuple<INotifyPropertyChanged, string>(observableObject, lastSegment.SegmentName));
                    }
                }

                result = new Result(CreateReactiveCommand(() => ReactiveExecuteCommand(methodNameForCommand, interactiveObject), methodNameForCommand, interactiveObject, observablePropertyTuples), false);
            }
            // Fallback to CommandBinding approach
            else
            {
                if (logical == null)
                    logical = control;
                if (logical == null)
                    return null;
                InputElement topLevel = null;
                if (logical is ICommandBindingOwner)
                    topLevel = logical as InputElement;
                if (topLevel == null)
                    topLevel = logical.FindLogicalAncestorOfType<ICommandBindingOwner>() as InputElement;
                if (topLevel == null)
                    topLevel = TopLevel.GetTopLevel(logical as Visual);
                if (topLevel == null)
                    return null;
                bool removeExistingCommand = false;
                System.Windows.Input.ICommand iCommand = AppCommands.FindVBApplicationCommand(acCommmand.ACUrl);
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
                cb.CanExecute += canExecuteDelegate ?? DefaultCanExecuteHandler;
                        
                commandBindings.Add(cb);
                CommandManager.SetCommandBindings(topLevel, commandBindings);

                result = new Result(iCommand, removeExistingCommand);
            }
            if (result != null && result.Command != null)
                control.Command = result.Command;
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
            // Create observables for each property dynamically
            var propertyObservables = propertiesToObserve
                .Select(propName => propertyOwner.GetProperty(propName))
                .Where(prop => prop != null)
                .Select(prop => Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => propertyOwner.PropertyChanged += h,
                    h => propertyOwner.PropertyChanged -= h)
                    .Where(evt => evt.EventArgs.PropertyName == prop.ACIdentifier)
                    .Select(_ => Unit.Default))
                .ToArray();

            // Merge all observables and evaluate the condition
            var canExecuteObservable = Observable.Merge(propertyObservables)
                .Select(_ => ReactiveEvaluateCanExecute(canExecuteMethodName, propertyOwner))
                .StartWith(ReactiveEvaluateCanExecute(canExecuteMethodName, propertyOwner));

            return ReactiveCommand.Create(executeAction, canExecuteObservable);
        }

        public static ReactiveCommand<Unit, Unit> CreateReactiveCommand(
            Action executeAction,
            string canExecuteMethodName,
            IACComponent propertyOwner,
            IEnumerable<Tuple<INotifyPropertyChanged, string>> propertiesToObserve)
        {
            // Create observables for each property dynamically
            var propertyObservables = propertiesToObserve
                .Where(prop => prop != null)
                .Select(prop => Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => prop.Item1.PropertyChanged += h,
                    h => prop.Item1.PropertyChanged -= h)
                    .Where(evt => evt.EventArgs.PropertyName == prop.Item2)
                    .Select(_ => Unit.Default))
                .ToArray();

            // Merge all observables and evaluate the condition
            var canExecuteObservable = Observable.Merge(propertyObservables)
                .Select(_ => ReactiveEvaluateCanExecute(canExecuteMethodName, propertyOwner))
                .StartWith(ReactiveEvaluateCanExecute(canExecuteMethodName, propertyOwner));

            return ReactiveCommand.Create(executeAction, canExecuteObservable);
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

        private static void ReactiveExecuteCommand(string methodName, IACComponent propertyOwner)
        {
            try
            {
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
