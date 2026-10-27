// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using gip.tool.installerAndUpdater.ViewModels;

namespace gip.tool.installerAndUpdater.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.FolderPicker = PickFolderAsync;
                viewModel.FilePicker = PickFileAsync;
            }
        }

        private async Task<string?> PickFolderAsync(string title)
        {
            IReadOnlyList<IStorageFolder> folders = await StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    Title = title,
                    AllowMultiple = false
                });

            return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
        }

        private async Task<string?> PickFileAsync(string title, string filter)
        {
            FilePickerFileType fileType = new FilePickerFileType("Supported files")
            {
                Patterns = new[] { filter }
            };

            IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = title,
                    AllowMultiple = false,
                    FileTypeFilter = new[] { fileType }
                });

            return files.Count > 0 ? files[0].TryGetLocalPath() : null;
        }
    }
}
