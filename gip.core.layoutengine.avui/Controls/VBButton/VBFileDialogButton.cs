// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System.Transactions;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;

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
        /// Handles the Button click event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The RoutedEvent agruments.</param>
        protected override async void ucButton_Click(object sender, Avalonia.Labs.Input.ExecutedRoutedEventArgs e)
        {
            if (   Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop 
                || desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider instance.");

            // Setup the starting folder
            IStorageFolder startingLocation = null;
            if (!String.IsNullOrEmpty(FileDlgInitialDirectory))
                startingLocation = await provider.TryGetFolderFromPathAsync(FileDlgInitialDirectory);

            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(this);

            // Parse filter string and convert to FilePickerFileTypes
            List<FilePickerFileType> fileTypes = null;
            if (!string.IsNullOrEmpty(FileDlgFilter))
            {
                fileTypes = ParseFilterString(FileDlgFilter);
            }

            // Start async operation to open the dialog.
            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open File",
                AllowMultiple = false,
                SuggestedStartLocation = startingLocation,
                FileTypeFilter = fileTypes
                // dlg.RestoreDirectory = FileDlgRestoreDirectory is not supported in Avalonia
            });

            if (files.Count > 0)
            {
                var storagefile= files.FirstOrDefault();
                this.FileDlgFileName = storagefile.Path.ToString();
                this.FileDlgFileNames = files.Select(c => c.Path.ToString()).ToArray();
                base.ucButton_Click(sender, e);
            }
        }

        /// <summary>
        /// Parses WPF-style filter string and converts to Avalonia FilePickerFileTypes
        /// Format: "Description|*.ext1;*.ext2|Description2|*.ext3"
        /// </summary>
        /// <param name="filterString">The filter string to parse</param>
        /// <returns>List of FilePickerFileType objects</returns>
        private List<FilePickerFileType> ParseFilterString(string filterString)
        {
            var fileTypes = new List<FilePickerFileType>();
            
            if (string.IsNullOrEmpty(filterString))
                return fileTypes;

            var parts = filterString.Split('|');
            
            for (int i = 0; i < parts.Length; i += 2)
            {
                if (i + 1 < parts.Length)
                {
                    string description = parts[i];
                    string patterns = parts[i + 1];
                    
                    var extensions = patterns.Split(';')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToArray();
                    
                    if (extensions.Length > 0)
                    {
                        fileTypes.Add(new FilePickerFileType(description)
                        {
                            Patterns = extensions
                        });
                    }
                }
            }
            
            return fileTypes;
        }

        #region Dependency-Properties
        /// <summary>
        /// Represents the dependency property for FileDlgFilter.
        /// </summary>
        public static readonly StyledProperty<String> FileDlgFilterProperty =
            AvaloniaProperty.Register<VBFileDialogButton, String>(nameof(FileDlgFilter));

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
            get { return GetValue(FileDlgFilterProperty); }
            set { SetValue(FileDlgFilterProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for FileDlgInitialDirectory.
        /// </summary>
        public static readonly StyledProperty<String> FileDlgInitialDirectoryProperty =
            AvaloniaProperty.Register<VBFileDialogButton, String>(nameof(FileDlgInitialDirectory));

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
            get { return GetValue(FileDlgInitialDirectoryProperty); }
            set { SetValue(FileDlgInitialDirectoryProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for FileDlgRestoreDirectory.
        /// </summary>
        public static readonly StyledProperty<Boolean> FileDlgRestoreDirectoryProperty =
            AvaloniaProperty.Register<VBFileDialogButton, Boolean>(nameof(FileDlgRestoreDirectory), true);

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
            get { return GetValue(FileDlgRestoreDirectoryProperty); }
            set { SetValue(FileDlgRestoreDirectoryProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for FileDlgFileName.
        /// </summary>
        public static readonly StyledProperty<String> FileDlgFileNameProperty =
            AvaloniaProperty.Register<VBFileDialogButton, String>(nameof(FileDlgFileName));

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
            get { return GetValue(FileDlgFileNameProperty); }
            set { SetValue(FileDlgFileNameProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for FileDlgFileNames.
        /// </summary>
        public static readonly StyledProperty<String[]> FileDlgFileNamesProperty =
            AvaloniaProperty.Register<VBFileDialogButton, String[]>(nameof(FileDlgFileNames));

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
            get { return GetValue(FileDlgFileNamesProperty); }
            set { SetValue(FileDlgFileNamesProperty, value); }
        }
        #endregion
    }
}
