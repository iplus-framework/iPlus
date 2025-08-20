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
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System.Transactions;
using System.ComponentModel;
using System.Windows.Forms;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control button to open a file dialog.
    /// The following properties must be linked using <see cref="VBBinding"/>
    /// FileDlgFilter           ="{vb:VBBinding VBContent=BitmapFilter}" 
    /// FileDlgRestoreDirectory ="{vb:VBBinding VBContent=BitmapRestoreDirectory}" 
    /// FileDlgFileName         ="{vb:VBBinding VBContent=BitmapFileName}"
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement Schalter zum öffnen eines Dateidialogs.
    /// Folgende Eigenschaften sind mittels <see cref="VBBinding"/> zu verknüpfen:
    /// FileDlgFilter           ="{vb:VBBinding VBContent=BitmapFilter}" 
    /// FileDlgRestoreDirectory ="{vb:VBBinding VBContent=BitmapRestoreDirectory}" 
    /// FileDlgFileName         ="{vb:VBBinding VBContent=BitmapFileName}"
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFileDialogButton'}de{'VBFileDialogButton'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBFileDialogButton : VBButton
    {
        /// <summary>
        /// Creates a new instance of VBFileDialogButton.
        /// </summary>
        public VBFileDialogButton() : base()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        /// <summary>
        /// Handles the Button click event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The RoutedEvent agruments.</param>
        protected override void ucButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (!String.IsNullOrEmpty(FileDlgFilter))
                dlg.Filter = FileDlgFilter;
            if (!String.IsNullOrEmpty(FileDlgInitialDirectory))
                dlg.InitialDirectory = FileDlgInitialDirectory;
            dlg.RestoreDirectory = FileDlgRestoreDirectory;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.FileDlgFileName = dlg.FileName;
                this.FileDlgFileNames = dlg.FileNames;
                base.ucButton_Click(sender, e);
            }
        }

        #region Dependency-Properties
        /// <summary>
        /// Represents the dependency property for FileDlgFilter.
        /// </summary>
        public static readonly DependencyProperty FileDlgFilterProperty
            = DependencyProperty.Register("FileDlgFilter", typeof(String), typeof(VBFileDialogButton));

        /// <summary>
        /// Gets or sets the filter for file dialog.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Filter für den Dateidialog.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        public String FileDlgFilter
        {
            get { return (String)GetValue(FileDlgFilterProperty); }
            set { SetValue(FileDlgFilterProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for FileDlgInitialDirectory.
        /// </summary>
        public static readonly DependencyProperty FileDlgInitialDirectoryProperty
            = DependencyProperty.Register("FileDlgInitialDirectory", typeof(String), typeof(VBFileDialogButton));

        /// <summary>
        /// Gets or sets the initial directory for file dialog.
        /// </summary>
        /// <summary xml:lang="de">
        ///  Liest oder setzt das Anfangsverzeichnis für den Dateidialog.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        public String FileDlgInitialDirectory
        {
            get { return (String)GetValue(FileDlgInitialDirectoryProperty); }
            set { SetValue(FileDlgInitialDirectoryProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for FileDlgRestoreDirectory.
        /// </summary>
        public static readonly DependencyProperty FileDlgRestoreDirectoryProperty
            = DependencyProperty.Register("FileDlgRestoreDirectory", typeof(Boolean), typeof(VBFileDialogButton), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets the restore directory for file dialog.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt das Wiederherstellungsverzeichnis für den Dateidialog.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        public Boolean FileDlgRestoreDirectory
        {
            get { return (Boolean)GetValue(FileDlgRestoreDirectoryProperty); }
            set { SetValue(FileDlgRestoreDirectoryProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for FileDlgFileName.
        /// </summary>
        public static readonly DependencyProperty FileDlgFileNameProperty
            = DependencyProperty.Register("FileDlgFileName", typeof(String), typeof(VBFileDialogButton));

        /// <summary>
        /// Gets or sets the file name for file dialog.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Dateinamen für den Dateidialog.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        public String FileDlgFileName
        {
            get { return (String)GetValue(FileDlgFileNameProperty); }
            set { SetValue(FileDlgFileNameProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for FileDlgFileNames.
        /// </summary>
        public static readonly DependencyProperty FileDlgFileNamesProperty
            = DependencyProperty.Register("FileDlgFileNames", typeof(String[]), typeof(VBFileDialogButton));

        /// <summary>
        /// Gets or sets the file names for file dialog.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Dateinamen für den Dateidialog.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        public String[] FileDlgFileNames
        {
            get { return (String[])GetValue(FileDlgFileNamesProperty); }
            set { SetValue(FileDlgFileNamesProperty, value); }
        }
        #endregion
    }
}
