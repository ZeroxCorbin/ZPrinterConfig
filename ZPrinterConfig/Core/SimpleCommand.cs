﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows.Input;

namespace ZPrinterConfig.Core
{
    public class SimpleCommand<T> : ICommand
    {
#if NET5_0_OR_GREATER
#nullable enable
#endif
        public SimpleCommand(Func<T, bool> canExecute, Action<T> execute)
        {
            this.CanExecuteDelegate = canExecute;
            this.ExecuteDelegate = execute;
        }

        public Func<T, bool> CanExecuteDelegate { get; }

        public Action<T> ExecuteDelegate { get; }

#if NET5_0_OR_GREATER
        public bool CanExecute(object? parameter)
#else
        public bool CanExecute(object parameter)
#endif
        {
            var canExecute = this.CanExecuteDelegate;
#pragma warning disable CS8604 // Possible null reference argument.
            return canExecute is null || canExecute(parameter is T t ? t : default);
#pragma warning restore CS8604 // Possible null reference argument.
        }

#if NET5_0_OR_GREATER
        public event EventHandler? CanExecuteChanged
#else
        public event EventHandler CanExecuteChanged
#endif
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

#if NET5_0_OR_GREATER
        public void Execute(object? parameter)
#else
        public void Execute(object parameter)
#endif
        {
#pragma warning disable CS8604 // Possible null reference argument.
            this.ExecuteDelegate?.Invoke(parameter is T t ? t : default);
#pragma warning restore CS8604 // Possible null reference argument.
        }
#if NET5_0_OR_GREATER
#nullable disable
#endif
    }
}