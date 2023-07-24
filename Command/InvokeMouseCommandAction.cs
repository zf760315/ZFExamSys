﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Command
{
    //public class InvokeMouseCommandAction : TriggerAction<UIElement>
    //{
    //    private ExecutableCommandBehavior _commandBehavior;

    //    /// <summary>
    //    /// Dependency property identifying if the associated element should automaticlaly be enabled or disabled based on the result of the Command's CanExecute
    //    /// </summary>
    //    public static readonly DependencyProperty AutoEnableProperty =
    //        DependencyProperty.Register("AutoEnable", typeof(bool), typeof(InvokeMouseCommandAction),
    //            new PropertyMetadata(true, (d, e) => ((InvokeMouseCommandAction)d).OnAllowDisableChanged((bool)e.NewValue)));

    //    /// <summary>
    //    /// Gets or sets whther or not the associated element will automatically be enabled or disabled based on the result of the commands CanExecute
    //    /// </summary>
    //    public bool AutoEnable
    //    {
    //        get { return (bool)GetValue(AutoEnableProperty); }
    //        set { SetValue(AutoEnableProperty, value); }
    //    }

    //    private void OnAllowDisableChanged(bool newValue)
    //    {
    //        var behavior = GetOrCreateBehavior();
    //        if (behavior != null)
    //            behavior.AutoEnable = newValue;
    //    }

    //    /// <summary>
    //    /// Dependency property identifying the command to execute when invoked.
    //    /// </summary>
    //    public static readonly DependencyProperty CommandProperty =
    //        DependencyProperty.Register("Command", typeof(ICommand), typeof(InvokeMouseCommandAction),
    //            new PropertyMetadata(null, (d, e) => ((InvokeMouseCommandAction)d).OnCommandChanged((ICommand)e.NewValue)));

    //    /// <summary>
    //    /// Gets or sets the command to execute when invoked.
    //    /// </summary>
    //    public ICommand Command
    //    {
    //        get { return GetValue(CommandProperty) as ICommand; }
    //        set { SetValue(CommandProperty, value); }
    //    }

    //    private void OnCommandChanged(ICommand newValue)
    //    {
    //        var behavior = GetOrCreateBehavior();
    //        if (behavior != null)
    //            behavior.Command = newValue;
    //    }

    //    /// <summary>
    //    /// Dependency property identifying the command parameter to supply on command execution.
    //    /// </summary>
    //    public static readonly DependencyProperty CommandParameterProperty =
    //        DependencyProperty.Register("CommandParameter", typeof(object), typeof(InvokeMouseCommandAction),
    //            new PropertyMetadata(null, (d, e) => ((InvokeMouseCommandAction)d).OnCommandParameterChanged(e.NewValue)));

    //    /// <summary>
    //    /// Gets or sets the command parameter to supply on command execution.
    //    /// </summary>
    //    public object CommandParameter
    //    {
    //        get { return GetValue(CommandParameterProperty); }
    //        set { SetValue(CommandParameterProperty, value); }
    //    }

    //    private void OnCommandParameterChanged(object newValue)
    //    {
    //        var behavior = GetOrCreateBehavior();
    //        if (behavior != null)
    //            behavior.CommandParameter = newValue;
    //    }

    //    /// <summary>
    //    /// Dependency property identifying the TriggerParameterPath to be parsed to identify the child property of the trigger parameter to be used as the command parameter.
    //    /// </summary>
    //    public static readonly DependencyProperty TriggerParameterPathProperty =
    //        DependencyProperty.Register("TriggerParameterPath", typeof(string), typeof(InvokeMouseCommandAction),
    //            new PropertyMetadata(null, (d, e) => { }));

    //    /// <summary>
    //    /// Gets or sets the TriggerParameterPath value.
    //    /// </summary>
    //    public string TriggerParameterPath
    //    {
    //        get { return GetValue(TriggerParameterPathProperty) as string; }
    //        set { SetValue(TriggerParameterPathProperty, value); }
    //    }

    //    /// <summary>
    //    /// Public wrapper of the Invoke method.
    //    /// </summary>
    //    public void InvokeAction(object parameter)
    //    {
    //        Invoke(parameter);
    //    }

    //    /// <summary>
    //    /// Executes the command
    //    /// </summary>
    //    /// <param name="parameter">This parameter is passed to the command; the CommandParameter specified in the CommandParameterProperty is used for command invocation if not null.</param>
    //    protected override void Invoke(object parameter)
    //    {
    //        //判断是否对TriggerParameterPath进行了赋值，如果赋值则抛出异常
    //        if (!string.IsNullOrEmpty(TriggerParameterPath))
    //        {
    //            throw new ArgumentOutOfRangeException("TriggerParameterPath should not be assigned any value");
    //        }
    //        //判断传入的parameter是否为 MouseEventArgs类型，如果布置，则抛出异常。如果是，则计算得到鼠标点坐标（相对于出发时间的UI对象），然后点坐标传入命令中
    //        if (parameter is MouseEventArgs mArgs)
    //        {
    //            Point p = mArgs.GetPosition((IInputElement)mArgs.Source);

    //            var behavior = GetOrCreateBehavior();

    //            if (behavior != null)
    //            {
    //                behavior.ExecuteCommand(p);
    //            }
    //        }
    //        else
    //        {
    //            throw new InvalidOperationException("The Trigger event is not mouse relevant");
    //        }

    //    }

    //    /// <summary>
    //    /// Sets the Command and CommandParameter properties to null.
    //    /// </summary>
    //    protected override void OnDetaching()
    //    {
    //        base.OnDetaching();

    //        Command = null;
    //        CommandParameter = null;

    //        _commandBehavior = null;
    //    }

    //    /// <summary>
    //    /// This method is called after the behavior is attached.
    //    /// It updates the command behavior's Command and CommandParameter properties if necessary.
    //    /// </summary>
    //    protected override void OnAttached()
    //    {
    //        base.OnAttached();

    //        // In case this action is attached to a target object after the Command and/or CommandParameter properties are set,
    //        // the command behavior would be created without a value for these properties.
    //        // To cover this scenario, the Command and CommandParameter properties of the behavior are updated here.
    //        var behavior = GetOrCreateBehavior();

    //        behavior.AutoEnable = AutoEnable;

    //        if (behavior.Command != Command)
    //            behavior.Command = Command;

    //        if (behavior.CommandParameter != CommandParameter)
    //            behavior.CommandParameter = CommandParameter;
    //    }

    //    private ExecutableCommandBehavior GetOrCreateBehavior()
    //    {
    //        // In case this method is called prior to this action being attached, 
    //        // the CommandBehavior would always keep a null target object (which isn't changeable afterwards).
    //        // Therefore, in that case the behavior shouldn't be created and this method should return null.
    //        if (_commandBehavior == null && AssociatedObject != null)
    //        {
    //            _commandBehavior = new ExecutableCommandBehavior(AssociatedObject);
    //        }

    //        return _commandBehavior;
    //    }

    //    /// <summary>
    //    /// A CommandBehavior that exposes a public ExecuteCommand method. It provides the functionality to invoke commands and update Enabled state of the target control.
    //    /// It is not possible to make the <see cref="InvokeCommandAction"/> inherit from <see cref="CommandBehaviorBase{T}"/>, since the <see cref="InvokeCommandAction"/>
    //    /// must already inherit from <see cref="TriggerAction{T}"/>, so we chose to follow the aggregation approach.
    //    /// </summary>
    //    private class ExecutableCommandBehavior : CommandBehaviorBase<UIElement>
    //    {
    //        /// <summary>
    //        /// Constructor specifying the target object.
    //        /// </summary>
    //        /// <param name="target">The target object the behavior is attached to.</param>
    //        public ExecutableCommandBehavior(UIElement target)
    //            : base(target)
    //        {
    //        }

    //        /// <summary>
    //        /// Executes the command, if it's set.
    //        /// </summary>
    //        public new void ExecuteCommand(object parameter)
    //        {
    //            base.ExecuteCommand(parameter);
    //        }
    //    }

    //}
}
