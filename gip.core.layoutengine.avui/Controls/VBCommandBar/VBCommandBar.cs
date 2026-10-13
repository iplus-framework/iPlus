using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Labs.Input;
using Avalonia.Media;
using Avalonia.Styling;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Mobile-mode replacement for the MAUI <c>ToolbarItem</c> row: a compact, single-row
    /// command bar that shows the standard BSO commands (New, Delete, Restore, Save,
    /// UndoSave, ...) as icon buttons. It reuses <see cref="VBRibbonButton"/> and the
    /// command/rights infrastructure of <see cref="VBRibbonBSODefault"/>, but without the
    /// multi-tab ribbon chrome that does not fit on phone screens.
    /// Bind <see cref="BSOACComponent"/> to the BSO of the currently shown design
    /// (VBDesignController.CurrentBSO) — the bar rebuilds itself whenever the BSO changes
    /// and hides itself when no BSO is active.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBCommandBar'}de{'VBCommandBar'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBCommandBar : StackPanel, ICommandBindingOwner
    {
        /// <summary>
        /// Default commands, mirroring the MAUI ToolbarItems of the mobile app
        /// (BSOFacilityChargeOverview: History/Print/Camera are BSO-specific; these are
        /// the generic record commands).
        /// </summary>
        public const string DefaultCommandList =
            Const.CmdNew + "," + Const.CmdDelete + "," + Const.CmdRestore + ","
            + Const.CmdSave + "," + Const.CmdUndoSave;

        /// <summary>Icon resource name ("Icon{IconName}Style") per command name.</summary>
        static readonly Dictionary<string, string> s_IconNames = new Dictionary<string, string>
        {
            { Const.CmdNameNew, "AddNew" },
            { Const.CmdNameLoad, "LoadRefresh" },
            { Const.CmdNameSave, "Filesave" },
            { Const.CmdNameUndoSave, "FilesaveUndo" },
            { Const.CmdNameDelete, "Delete" },
            { Const.CmdNameRestore, "Restore" },
            { Const.CmdNameSearch, "Find" },
            { Const.CmdNameNavigateFirst, "NavFirst" },
            { Const.CmdNameNavigatePrev, "NavPrev" },
            { Const.CmdNameNavigateNext, "NavNext" },
            { Const.CmdNameNavigateLast, "NavLast" },
        };

        static readonly Dictionary<string, string> s_Captions = new Dictionary<string, string>
        {
            { Const.CmdNameNew, Const.CmdNameNew },
            { Const.CmdNameLoad, Const.CmdNameLoad },
            { Const.CmdNameSave, Const.CmdNameSave },
            { Const.CmdNameUndoSave, "Undo save" },
            { Const.CmdNameDelete, Const.CmdNameDelete },
            { Const.CmdNameRestore, Const.CmdNameRestore },
            { Const.CmdNameSearch, Const.CmdNameSearch },
            { Const.CmdNameNavigateFirst, "First record" },
            { Const.CmdNameNavigatePrev, "Previous record" },
            { Const.CmdNameNavigateNext, "Next record" },
            { Const.CmdNameNavigateLast, "Last record" },
        };

        bool _Initialized;

        public VBCommandBar() : base()
        {
            Orientation = Orientation.Horizontal;
            Spacing = 4;
            VerticalAlignment = VerticalAlignment.Center;
        }

        /// <summary>
        /// Represents the styled property for BSOACComponent.
        /// </summary>
        public static readonly StyledProperty<IACBSO> BSOACComponentProperty =
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBCommandBar>();

        /// <summary>
        /// Gets or sets the BSOACComponent whose commands are shown.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for CommandList.
        /// </summary>
        public static readonly StyledProperty<string> CommandListProperty =
            AvaloniaProperty.Register<VBCommandBar, string>(nameof(CommandList), DefaultCommandList);

        /// <summary>
        /// Comma-separated list of ACUrl commands (e.g. "!New,!Delete,!Save").
        /// Commands that the current user may not execute (rights management) or that do
        /// not exist on the BSO are skipped.
        /// </summary>
        [ACPropertyInfo(9999)]
        public string CommandList
        {
            get { return GetValue(CommandListProperty); }
            set { SetValue(CommandListProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == BSOACComponentProperty)
            {
                IACBSO oldBso = change.OldValue as IACBSO;
                if (oldBso != null)
                    DeInitVBControl(oldBso);
                if (change.NewValue != null)
                    InitVBControl();
            }
            else if (change.Property == CommandListProperty)
            {
                if (BSOACComponent != null)
                    InitVBControl();
            }
        }

        /// <summary>
        /// Builds one <see cref="VBRibbonButton"/> per configured command.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized && Children.Count > 0)
                Children.Clear();

            _Initialized = true;
            IACBSO bso = BSOACComponent;
            if (bso == null || String.IsNullOrEmpty(CommandList))
                return;

            foreach (string command in CommandList.Split(',')
                .Select(c => c.Trim())
                .Where(c => !String.IsNullOrEmpty(c)))
            {
                string commandName = command.Replace("!", "");

                // Rights check, same pattern as VBRibbonBSODefault.CreatePanel:
                // the command must exist on the BSO and the user needs at least
                // "enabled" rights for the method.
                IACType dcACTypeInfo = null;
                object dcSource = null;
                string dcPath = "";
                Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;
                if (!bso.ACUrlBinding(command, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                    continue;
                if (dcRightControlMode < Global.ControlModes.Enabled)
                    continue;

                ACClassMethod mth = (bso as IACBSO)?.ACClassMethods
                    .FirstOrDefault(x => x.ACIdentifier == commandName);
                if (mth != null && mth.IsRightmanagement)
                {
                    if (mth.ACClass.RightManager.GetControlMode(mth) < Global.ControlModes.Enabled)
                        continue;
                }

                string iconName;
                if (!s_IconNames.TryGetValue(commandName, out iconName))
                    iconName = commandName;
                string caption;
                if (!s_Captions.TryGetValue(commandName, out caption))
                    caption = commandName;
                caption = Database.Root.Environment.TranslateText(bso, caption);

                VBRibbonButton button = new VBRibbonButton();
                // Large ribbon style: rectangular button with the icon on top and the
                // caption below (RibbonButton[Size=Large] template). IconName is NOT set,
                // because VBRibbonButton.OnInitialized would force Size=Medium and only
                // fill LargeIcon when it runs.
                if (this.TryFindResource("Icon" + iconName + "Style", ThemeVariant.Default, out object resource)
                    && resource is ControlTheme iconTheme)
                {
                    button.LargeIcon = new ContentControl { Theme = iconTheme };
                }
                button.Size = AvaloniaRibbon.Models.RibbonControlSize.Large;
                button.ACCaption = caption;
                button.Content = caption;
                button.RightControlMode = Global.ControlModes.Enabled;
                button.ACCommand = new ACCommand(caption, command, null);
                button.ACCommandHandling = ACCommandHelper.ApplyACCommand(
                    button, bso, command, button.ACCommand, this,
                    ExecutedCommandHandler, CanExecuteCommandHandler);
                ToolTip.SetTip(button, caption);

                Children.Add(button);
            }
        }

        /// <summary>
        /// Removes all buttons and references so the garbage collector can collect the
        /// control when it is removed from the logical tree.
        /// </summary>
        public virtual void DeInitVBControl(IACComponent bso)
        {
            Children.Clear();
        }

        void ExecutedCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            ACCommand acCommand = RoutedEventHelper.GetACCommand(e);
            if (acCommand == null)
                return;
            IACBSO bso = BSOACComponent;
            if (bso == null)
                return;
            string url = acCommand.GetACUrl();
            if (String.IsNullOrEmpty(url) || !bso.IsEnabledACUrlCommand(url))
                return;
            if (acCommand.ParameterList.Any())
                bso.ACUrlCommand(url, acCommand.ParameterList.ToValueArray());
            else
                bso.ACUrlCommand(url);
            e.Handled = true;
        }

        void CanExecuteCommandHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            ACCommand acCommand = RoutedEventHelper.GetACCommand(e);
            if (acCommand == null)
            {
                e.CanExecute = false;
                return;
            }
            IACBSO bso = BSOACComponent;
            string url = acCommand.GetACUrl();
            e.CanExecute = bso != null
                && !String.IsNullOrEmpty(url)
                && bso.IsEnabledACUrlCommand(url);
        }
    }
}
