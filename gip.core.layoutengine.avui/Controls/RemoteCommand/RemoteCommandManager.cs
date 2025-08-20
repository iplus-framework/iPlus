// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using gip.core.datamodel;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace gip.core.layoutengine.avui
{
   public class RemoteCommandManager
   {
        private static int _ExpiredTimeOutSec = 5;
        private static RemoteCommandManager _instance;
        public static RemoteCommandManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RemoteCommandManager();
                }
                return _instance;
            }
        }
        private RemoteCommandManager()
        {
            // Initialize commands here if needed
        }

        public void AddNewRemoteCommand(string ACUrl)
        {
            _remoteCommands.Add(new RemoteCommand(ACUrl));
            CommandManager.InvalidateRequerySuggested();
            RemoveExpiredCommands();
        }

        public void AddNewRemoteCommand(IACInteractiveObject iObj, string acUrl, bool isMethod = true)
        {
            if (string.IsNullOrEmpty(acUrl))
                return;
            if (iObj != null)
            {
                if (isMethod)
                {
                    if (!acUrl.StartsWith(ACUrlHelper.Delimiter_InvokeMethod))
                        acUrl = iObj.GetACUrl() + ACUrlHelper.Delimiter_InvokeMethod + acUrl;
                    else
                        acUrl = iObj.GetACUrl() + acUrl;
                }
                else
                {
                    if (!acUrl.StartsWith(ACUrlHelper.Delimiter_DirSeperator))
                        acUrl = iObj.GetACUrl() + ACUrlHelper.Delimiter_DirSeperator + acUrl;
                    else
                        acUrl = iObj.GetACUrl() + acUrl;
                }
            }
            AddNewRemoteCommand(acUrl);
        }

        public bool HasRemoteCommand(string acUrl)
        {
            var hasCommands = _remoteCommands.Any(cmd => cmd.ACUrl == acUrl);
            if (hasCommands)
                _remoteCommands.RemoveAll(cmd => cmd.ACUrl == acUrl);
            RemoveExpiredCommands();
            return hasCommands;
        }

        public bool HasRemoteCommand(IACInteractiveObject iObj, string acUrl, bool isMethod = true)
        {
            if (string.IsNullOrEmpty(acUrl))
                return false;
            if (iObj != null)
            {
                if (isMethod)
                {
                    if (!acUrl.StartsWith(ACUrlHelper.Delimiter_InvokeMethod))
                        acUrl = iObj.GetACUrl() + ACUrlHelper.Delimiter_InvokeMethod + acUrl;
                    else
                        acUrl = iObj.GetACUrl() + acUrl;
                }
                else
                {
                    if (!acUrl.StartsWith(ACUrlHelper.Delimiter_DirSeperator))
                        acUrl = iObj.GetACUrl() + ACUrlHelper.Delimiter_DirSeperator + acUrl;
                    else
                        acUrl = iObj.GetACUrl() + acUrl;
                }
            }
            return HasRemoteCommand(acUrl);
        }

        private void RemoveExpiredCommands()
        {
            DateTime now = DateTime.Now;
            _remoteCommands.RemoveAll(cmd => (now - cmd.Time).TotalSeconds > _ExpiredTimeOutSec);
        }

        private SafeList<RemoteCommand> _remoteCommands = new SafeList<RemoteCommand>();

        internal class RemoteCommand
        {
            public DateTime Time { get; set; }
            public string ACUrl { get; set; }
            public RemoteCommand(string acUrl)
            {
                ACUrl = acUrl;
                Time = DateTime.Now;
            }
        }
    }
}
