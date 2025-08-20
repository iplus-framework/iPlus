// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using gip.core.layoutenginewpf.Interfaces.Controls;
using gip.core.shared;
using gip.core.layoutenginewpf.LayoutRoots;
using gip.core.datamodell;
using gip.core.layoutenginewpf.Helperclasses;

namespace gip.core.layoutenginewpf
{
    /// <summary>
    /// Interaction logic for WPFButton.xaml
    /// </summary>
    public partial class VBButton : UserControl, IControl, IDataCommand
    {
        IBSO _BSO;


        public VBButton()
        {
            DragEnabled = false;
            DropEnabled = false;
            InitializeComponent();
        }
        #region IDataCommand Members

        string _InternalName;
        public string InternalName
        {
            get
            {
                return _InternalName;
            }
            set
            {
                _InternalName = value;
            }
        }

        #endregion

        #region IControl Members

        public IBSOBase BSO
        {
            get
            {
                return (IBSO)_BSO;
            }
        }

        public string DblClick 
        { 
            get; 
            set; 
        }

        public string PopUp
        {
            get;
            set;
        }

        //public string ToolTip
        //{
        //    get
        //    {
        //        return ucButton.ToolTip as string;
        //    }
        //    set
        //    {
        //        ucButton.ToolTip = value;
        //    }
        //}

        public Visibility Visible
        {
            get
            {
                return Visibility;
            }
            set
            {
                if (value == Visibility.Visible)
                {
                    if (BSO.AppContextBase.GetAccessRight(FullInternalName))
                    {
                        Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    Visibility = value;
                }
            }
        }

        private bool Enabled
        {
            get
            {
                return IsEnabled;
            }
            set
            {
                if (value == true)
                {
                    if (BSO == null)
                    {
                        IsEnabled = true;
                    }
                    else
                    {
                        IsEnabled = BSO.AppContextBase.GetAccessRight(FullInternalName);
                    }
                }
                else
                {
                    IsEnabled = false;
                }
                IsEnabled = value;
            }
        }

        public bool IsDefault
        {
            get
            {
                return ucButton.IsDefault;
            }
            set
            {
                ucButton.IsDefault = value;
            }
        }

        /// <summary>
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">Aktiviert oder deaktiviert den Autofokus.</summary>
        public bool AutoFocus { get; set; }

        public bool DragEnabled { get; set; }

        public bool DropEnabled { get; set; }

        //SolutionProperty _PropertySchema = null;
        public SolutionProperty PropertySchema
        {
            get
            {
                return null;
                //if (_PropertySchema == null && BSO != null && !string.IsNullOrEmpty(DataContent))
                //{
                //    _PropertySchema = BSO.GetPropertySchema(DataContent);
                //}
                //return _PropertySchema;
            }
        }

        public void BSOModeChanged()
        {
            Enabled = BSO.IsEnabled(InternalName, DisabledBSOModes);
        }

        public void ControlModeChanged()
        {
            if (Enabled)
            {
                ControlMode = ControlModes.Enabled;
            }
            else
            {
                ControlMode = ControlModes.Disabled;
            }
            if (BSO != null)
            {
                ControlMode = BSO.ControlMode(InternalName, ControlMode);
            }
        }

        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(ControlModes), typeof(VBButton));

        public ControlModes ControlMode
        {
            get
            {
                return (ControlModes)GetValue(ControlModeProperty);
            }
            set
            {
                SetValue(ControlModeProperty, value);
            }
        }

        public string DisabledBSOModes { get; set; }
        #endregion

        private void ucButton_Click(object sender, RoutedEventArgs e)
        {
            ((IBSO)BSO).AppContextClient.ExecuteCommand(InternalName);
        }

        private void ucButton_IsEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            if (BSO != null)
            {
                e.CanExecute = ((IBSO)BSO).AppContextClient.IsCommandEnabled(InternalName);
            }
            else
            {
                e.CanExecute = false;
            }
        }

        bool _Loaded = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_Loaded)
                return;
            _Loaded = true;
            _BSO = (IBSO)DataContext;

            IBSOContext bsoContext = VBLogicalTreeHelper.GetBSOContext(this);
            //bsoContext.VBControlList.Add(this);

            if (BSO == null || BSO.BSOType == "BSOLayoutTest")
            {
                ucButton.Content = InternalName;
                return;
            }
            ucButton.Command = new AppCommands(InternalName).Command;
            CommandBindings.Add(new CommandBinding(ucButton.Command, ucButton_Click, ucButton_IsEnabled));
            if (Visibility == Visibility.Visible)
            {
                if (!BSO.AppContextBase.GetAccessRight(FullInternalName))
                {
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (BSO.AvailableCommands().Where(c => c.InternalName == InternalName).Select(c => c.Caption).Count() > 0)
                    {
                        ucButton.Content = BSO.AvailableCommands().Where(c => c.InternalName == InternalName).Select(c => c.Caption).First();
                    }
                }
            }

            if (IsEnabled)
            {
                if (!BSO.AppContextBase.GetAccessRight(FullInternalName))
                {
                    IsEnabled = false;
                }
                else
                {
                    BSOModeChanged();
                }
            }

            
            if (AutoFocus)
            {
                Focus();
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        /// <summary>
        /// Gibt den vollen InternalName zurück, welcher auch für die Rechteverwaltung verwendet wird
        /// </summary>
        /// <returns></returns>
        private string FullInternalName
        {
            get
            {
                if (InternalName.IndexOf('!') == 0)
                {
                    return BSO.BSOType + "." + BSO.BSOContext + "." + InternalName.Substring(1);
                }
                else
                {
                    return InternalName;
                }
            }
        }
    }
}
