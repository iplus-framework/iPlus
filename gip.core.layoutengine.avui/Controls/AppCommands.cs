// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Labs.Input;
using gip.core.datamodel;
using RoutedCommand = Avalonia.Labs.Input.RoutedCommand;
using KeyGesture = Avalonia.Input.KeyGesture;
using Key = Avalonia.Input.Key;
using gip.ext.designer.avui;

namespace gip.core.layoutengine.avui
{
    public class RoutedUICommandEx : RoutedCommand
    {
        public RoutedUICommandEx(ACCommand acCommand, Avalonia.Input.KeyGesture keyGesture) :
            base(acCommand.GetACUrl(), keyGesture)
        {
            ACCommand = acCommand;
        }

        public RoutedUICommandEx(ACCommand acCommand) : base(acCommand.GetACUrl())
        {
            ACCommand = acCommand;
        }

        /// <summary>
        /// Gets or sets the ACCommand.
        /// </summary>
        public ACCommand ACCommand { get; set; }
    }

    public static class AppCommands
    {
        private static List<RoutedUICommandEx> _CommandList;
        private static RoutedUICommandEx cmdNew;
        private static RoutedUICommandEx cmdLoad;
        private static RoutedUICommandEx cmdDelete;
        private static RoutedUICommandEx cmdRestore;
        private static RoutedUICommandEx cmdSave;
        private static RoutedUICommandEx cmdUndoSave;
        private static RoutedUICommandEx cmdSearch;
        //private static RoutedUICommandEx cmdRequery;
        private static RoutedUICommandEx cmdNavigateFirst;
        private static RoutedUICommandEx cmdNavigatePrev;
        private static RoutedUICommandEx cmdNavigateNext;
        private static RoutedUICommandEx cmdNavigateLast;
        private static RoutedUICommandEx cmdQueryPrintDlg;
        private static RoutedUICommandEx cmdQueryPreviewDlg;
        private static RoutedUICommandEx cmdQueryDesignDlg;
        private static RoutedUICommandEx cmdFindAndReplace;
        private static RoutedUICommandEx cmdExport;

        public static readonly KeyGesture NewGesture = new KeyGesture(Key.N, ApplicationCommands.PlatformCommandKey);
        public static readonly KeyGesture SaveGesture = new KeyGesture(Key.S, ApplicationCommands.PlatformCommandKey);
        public static readonly KeyGesture UndoSaveGesture = new KeyGesture(Key.S, KeyModifiers.Alt);
        public static readonly KeyGesture LoadGesture = new KeyGesture(Key.L, ApplicationCommands.PlatformCommandKey);
        public static readonly KeyGesture RestoreGesture = new KeyGesture(Key.D, ApplicationCommands.PlatformCommandKey | KeyModifiers.Shift);
        public static readonly KeyGesture PrintGesture = new KeyGesture(Key.P, ApplicationCommands.PlatformCommandKey);

        public static readonly KeyGesture MoveCursorToTheStartOfDocumentGesture = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration.MoveCursorToTheStartOfDocument.FirstOrDefault() ?? new KeyGesture(Key.Home, ApplicationCommands.PlatformCommandKey);
        public static readonly KeyGesture MoveCursorToTheEndOfDocumentGesture = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration.MoveCursorToTheEndOfDocument.FirstOrDefault() ?? new KeyGesture(Key.End, ApplicationCommands.PlatformCommandKey);
        public static readonly KeyGesture PageUpGesture = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration.PageUp.FirstOrDefault() ?? new KeyGesture(Key.PageUp);
        public static readonly KeyGesture PageDownGesture = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration.PageDown.FirstOrDefault() ?? new KeyGesture(Key.PageDown);

        static AppCommands()
        {
            _CommandList = new List<RoutedUICommandEx>();
            cmdNew = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdNew, null), NewGesture);
            ACValueList parameterListLoad = new ACValueList();
            parameterListLoad.Add(new ACValue("requery", true));
            cmdLoad = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdLoad, parameterListLoad), LoadGesture);
            cmdDelete = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdDelete, null), ApplicationCommands.DeleteGesture);
            cmdRestore = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdRestore, null), RestoreGesture);
            cmdSave = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdSave, null), SaveGesture);
            cmdUndoSave = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdUndoSave, null), UndoSaveGesture);
            cmdSearch = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdSearch, null), ApplicationCommands.FindGesture);
            cmdNavigateFirst = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdNavigateFirstPrimary, null), MoveCursorToTheStartOfDocumentGesture);
            cmdNavigatePrev = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdNavigatePrevPrimary, null), PageUpGesture);
            cmdNavigateNext = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdNavigateNextPrimary, null), PageDownGesture);
            cmdNavigateLast = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdNavigateLastPrimary, null), MoveCursorToTheEndOfDocumentGesture);
            cmdQueryPrintDlg = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdQueryPrintDlg, null), PrintGesture);
            cmdQueryPreviewDlg = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdQueryPreviewDlg, null), new KeyGesture(Key.F2, KeyModifiers.Control));
            cmdQueryDesignDlg = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdQueryDesignDlg, null), new KeyGesture(Key.F2, KeyModifiers.Alt));
            cmdFindAndReplace = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdFindAndReplace, null), ApplicationCommands.ReplaceGesture);
            cmdExport = (RoutedUICommandEx)AppCommands.AddApplicationCommand(new ACCommand("", Const.CmdExport, null), new KeyGesture(Key.E, KeyModifiers.Control));
        }

        public static bool IsBuiltInGesture(KeyGesture gesture)
        {
            if (gesture == null)
                return false;
            if (gesture.Equals(ApplicationCommands.PasteGesture)
                || gesture.Equals(ApplicationCommands.CopyGesture)
                || gesture.Equals(ApplicationCommands.CutGesture)
                || gesture.Equals(ApplicationCommands.SelectAllGesture)
                || gesture.Equals(ApplicationCommands.UndoGesture)
                || gesture.Equals(ApplicationCommands.RedoGesture)
                )
            {
                return true;
            }
            return false;
        }

        public static bool IsBuiltInAppCommand(string acMethodName)
        {
            return Const.IsBuiltInAppCommand(acMethodName);
        }

        public static KeyGesture GetBuiltInKeyGesture(string acMethodName)
        {
            switch (acMethodName)
            {
                case Const.CmdNamePaste:
                case Const.CmdPaste:
                    return ApplicationCommands.PasteGesture;
                case Const.CmdNameCopy:
                case Const.CmdCopy:
                    return ApplicationCommands.CopyGesture;
                case Const.CmdNameCut:
                case Const.CmdCut:
                    return ApplicationCommands.CutGesture;
                case Const.CmdNameSelectAll:
                case Const.CmdSelectAll:
                    return ApplicationCommands.SelectAllGesture;
                case Const.CmdNameUndo:
                case Const.CmdUndo:
                    return ApplicationCommands.UndoGesture;
                case Const.CmdNameRedo:
                case Const.CmdRedo:
                    return ApplicationCommands.RedoGesture;
                default:
                    return null;
            }
        }

        #region public Methods
        public static ICommand AddApplicationCommand(ACCommand acCommand, Avalonia.Input.KeyGesture keyGesture = null)
        {
            ICommand appCmd = null;
            if (keyGesture != null || !acCommand.ParameterList.Any())
                appCmd = FindVBApplicationCommand(acCommand.GetACUrl());
            if (appCmd != null)
                return appCmd;

            RoutedUICommandEx appCmd2;
            if (keyGesture == null)
                appCmd2 = new RoutedUICommandEx(acCommand);
            else
                appCmd2 = new RoutedUICommandEx(acCommand, keyGesture);
            if (keyGesture != null || !acCommand.ParameterList.Any())
                _CommandList.Add(appCmd2);
            return appCmd2;
        }
        #endregion

        #region private Methods
        public static ICommand FindVBApplicationCommand(string acUrl)
        {
            try
            {
                ICommand appCmd = IsStandardApplicationCommand(acUrl);
                if (appCmd != null)
                    return appCmd;

                appCmd = _CommandList.Where(c=> c.Name == acUrl).FirstOrDefault();
                return appCmd;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("AppCommands", "FindVBApplicationCommand", msg);

                return null;
            }
        }

        public static bool RemoveVBApplicationCommand(ICommand command)
        {
            if (command == null)
                return false;
            if (!(command is RoutedUICommandEx))
                return false;
            RoutedUICommandEx commandEx = command as RoutedUICommandEx;
            if ((commandEx.ACCommand.ACUrl == Const.CmdNew)
                || (commandEx.ACCommand.ACUrl == Const.CmdLoad)
                || (commandEx.ACCommand.ACUrl == Const.CmdDelete)
                || (commandEx.ACCommand.ACUrl == Const.CmdRestore)
                || (commandEx.ACCommand.ACUrl == Const.CmdSave)
                || (commandEx.ACCommand.ACUrl == Const.CmdUndoSave)
                || (commandEx.ACCommand.ACUrl == Const.CmdSearch)
                || (commandEx.ACCommand.ACUrl == Const.CmdNavigateFirstPrimary)
                || (commandEx.ACCommand.ACUrl == Const.CmdNavigatePrevPrimary)
                || (commandEx.ACCommand.ACUrl == Const.CmdNavigateNextPrimary)
                || (commandEx.ACCommand.ACUrl == Const.CmdNavigateLastPrimary)
                || (commandEx.ACCommand.ACUrl == Const.CmdQueryPrintDlg)
                || (commandEx.ACCommand.ACUrl == Const.CmdQueryPreviewDlg)
                || (commandEx.ACCommand.ACUrl == Const.CmdQueryDesignDlg)
                || (commandEx.ACCommand.ACUrl == Const.CmdFindAndReplace)
                )
            {
                return false;
            }

            try
            {
                return _CommandList.Remove(commandEx);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("AppCommands", "RemoveVBApplicationCommand", msg);

                return false;
            }
        }

        public static ICommand IsStandardApplicationCommand(string cmdName)
        {
            switch (cmdName)
            {
                case Const.CmdCut:
                    return ApplicationCommands.Cut;
                case Const.CmdCopy:
                    return ApplicationCommands.Copy;
                case Const.CmdPaste:
                    return ApplicationCommands.Paste;
                case Const.CmdUndo:
                    return ApplicationCommands.Undo;
                case Const.CmdRedo:
                    return ApplicationCommands.Redo;
                default:
                    return null;
            }
        }

        public static ACCommand GetACCommandIfStandardApplicationCommands(RoutedCommand n)
        {
            //switch (n.Name)
            //{
            //    case Const.CmdNameCut:
            //        return new ACCommand(Const.CmdNameCut, Const.CmdCut, null);
            //    case Const.CmdNameCopy:
            //        return new ACCommand(Const.CmdNameCopy, Const.CmdCopy, null);
            //    case Const.CmdNamePaste:
            //        return new ACCommand(Const.CmdNamePaste, Const.CmdPaste, null);
            //    case Const.CmdNameUndo:
            //        return new ACCommand(Const.CmdNameUndo, Const.CmdUndo, null);
            //    case Const.CmdNameRedo:
            //        return new ACCommand(Const.CmdNameRedo, Const.CmdRedo, null);
            //    default:
            //        return null;
            //}
            return null;
        }
        #endregion

        #region public Properties
        public static RoutedUICommandEx CmdNew { get { return cmdNew; } }
        public static RoutedUICommandEx CmdLoad { get { return cmdLoad; } }
        public static RoutedUICommandEx CmdDelete { get { return cmdDelete; } }
        public static RoutedUICommandEx CmdRestore { get { return cmdRestore; } }
        public static RoutedUICommandEx CmdSave { get { return cmdSave; } }
        public static RoutedUICommandEx CmdUndoSave { get { return cmdUndoSave; } }
        public static RoutedUICommandEx CmdSearch { get { return cmdSearch; } }
        //public static RoutedUICommandEx CmdRequery { get { return cmdRequery; } }
        public static RoutedUICommandEx CmdNavigateFirst { get { return cmdNavigateFirst; } }
        public static RoutedUICommandEx CmdNavigatePrev { get { return cmdNavigatePrev; } }
        public static RoutedUICommandEx CmdNavigateNext { get { return cmdNavigateNext; } }
        public static RoutedUICommandEx CmdNavigateLast { get { return cmdNavigateLast; } }
        public static RoutedUICommandEx CmdQueryPrintDlg { get { return cmdQueryPrintDlg; } }
        public static RoutedUICommandEx CmdQueryPreviewDlg { get { return cmdQueryPreviewDlg; } }
        public static RoutedUICommandEx CmdQueryDesignDlg { get { return cmdQueryDesignDlg; } }
        public static RoutedUICommandEx CmdFindAndReplace { get { return cmdFindAndReplace; } }

        public static ICommand CmdCut { get { return ApplicationCommands.Cut; } }
        public static ICommand CmdCopy { get { return ApplicationCommands.Copy; } }
        public static ICommand CmdPaste { get { return ApplicationCommands.Paste; } }
        public static ICommand CmdUndo { get { return ApplicationCommands.Undo; } }
        public static ICommand CmdRedo { get { return ApplicationCommands.Redo; } }
        #endregion
    }

}
