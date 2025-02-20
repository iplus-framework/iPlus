﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows.Input;
using gip.ext.design;

namespace gip.ext.designer.Services
{
    // See IToolService for description.
    sealed class DefaultToolService : IToolService, IDisposable
    {
        ITool _currentTool;
        IDesignPanel _designPanel;

        public DefaultToolService(DesignContext context)
        {
            _currentTool = this.PointerTool;
            context.Services.RunWhenAvailable<IDesignPanel>(
                delegate(IDesignPanel designPanel)
                {
                    _designPanel = designPanel;
                    _currentTool.Activate(designPanel);
                });
        }

        public void Dispose()
        {
            if (_designPanel != null)
            {
                _currentTool.Deactivate(_designPanel);
                _designPanel = null;
            }
        }

        public ITool PointerTool
        {
            get { return Services.PointerTool.Instance; }
        }

        public ITool CurrentTool
        {
            get { return _currentTool; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (_currentTool == value) return;
                if (_designPanel != null)
                {
                    _currentTool.Deactivate(_designPanel);
                }
                _currentTool = value;
                if (_designPanel != null)
                {
                    _currentTool.Activate(_designPanel);
                }
                if (CurrentToolChanged != null)
                {
                    CurrentToolChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler CurrentToolChanged;

        public IDesignPanel DesignPanel
        {
            get { return _designPanel; }
        }

        public event ToolEventHandler ToolEvents;

        public void RaiseToolEvent(ITool currentTool, ToolEventArgs eventArgs)
        {
            if (ToolEvents != null)
            {
                if (currentTool == null)
                    currentTool = CurrentTool;
                ToolEvents(currentTool, eventArgs);
            }
        }
    }
}
