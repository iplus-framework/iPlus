using System;
using Avalonia;
using Avalonia.Data;

namespace gip.core.layoutengine.avui.Utils;

/// <summary>
/// Helper class for evaluating a binding from an Item and IBinding instance.
/// Copied from Avalonia.Controls.Utils as it became internal in Avalonia 12.
/// </summary>
public class BindingEvaluator : StyledElement, IDisposable
{
    private IDisposable _subscription;
    private BindingBase _lastBinding;

    public static readonly StyledProperty<object> ValueProperty =
        AvaloniaProperty.Register<BindingEvaluator, object>(nameof(Value));

    /// <summary>
    /// Gets or sets the data item value.
    /// </summary>
    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public object Evaluate(object dataContext)
    {
        // Only update the DataContext if necessary
        if (!Equals(dataContext, DataContext))
            DataContext = dataContext;

        return Value;
    }

    public void UpdateBinding(BindingBase binding)
    {
        if (binding == _lastBinding)
            return;

        _subscription?.Dispose();
        _subscription = this.Bind(ValueProperty, binding);
        _lastBinding = binding;
    }

    public void ClearDataContext()
        => DataContext = null;

    public void Dispose()
    {
        _subscription?.Dispose();
        _subscription = null;
        _lastBinding = null;
        DataContext = null;
    }

    public static BindingEvaluator TryCreate(BindingBase binding)
    {
        if (binding is null)
            return null;

        var evaluator = new BindingEvaluator();
        evaluator.UpdateBinding(binding);
        return evaluator;
    }
}

public sealed class BindingEvaluator<T> : BindingEvaluator
{
    public new T Value
    {
        get => base.Value is T typed ? typed : default;
        set => base.Value = value;
    }

    public new T Evaluate(object dataContext)
    {
        var value = base.Evaluate(dataContext);
        return value is T typed ? typed : default;
    }

    public new static BindingEvaluator<T> TryCreate(BindingBase binding)
    {
        if (binding is null)
            return null;

        var evaluator = new BindingEvaluator<T>();
        evaluator.UpdateBinding(binding);
        return evaluator;
    }
}
