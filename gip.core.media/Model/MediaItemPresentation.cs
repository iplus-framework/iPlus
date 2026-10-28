// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Runtime.CompilerServices;
using System;
using System.ComponentModel;
using System.IO;
using gip.core.datamodel;

namespace gip.core.media
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'MediaItemPresentations'}de{'MediaItemPresentation'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class MediaItemPresentation : INotifyPropertyChanged
    {
        public MediaItemPresentation()
        {
            IsNew = true;
        }
        #region File Path

        private string _ThumbPath;
        [ACPropertyInfo(1, "ThumbPath", "en{'Stored thumbnail'}de{'Gespeichertes Daumenbild'}")]
        public string ThumbPath
        {
            get { return _ThumbPath; }
            set
            {
                if (_ThumbPath != value)
                {
                    _ThumbPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _FilePath;
        [ACPropertyInfo(2, "FilePath", "en{'Stored file'}de{'Gespeicherte Datei'}")]
        public string FilePath
        {
            get { return _FilePath; }
            set
            {
                if (_FilePath != value)
                {
                    _FilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Edit File Path

        private string _EditThumbPath;
        [ACPropertyInfo(3, "ThumbPath", "en{'Thumbnail to upload'}de{'Hochzuladendes Daumenbild'}")]
        public string EditThumbPath
        {
            get { return _EditThumbPath; }
            set
            {
                if (_EditThumbPath != value)
                {
                    _EditThumbPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _EditFilePath;
        [ACPropertyInfo(4, "FilePath", "en{'File to upload'}de{'Hochzuladende Datei'}")]
        public string EditFilePath
        {
            get { return _EditFilePath; }
            set
            {
                if (_EditFilePath != value)
                {
                    _EditFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Temp File Path

        private string _TempThumbPath;
        [ACPropertyInfo(5, "TempThumbPath", "en{'Thumb Image path'}de{'Standard-Daumenbildpfad'}")]
        public string TempThumbPath
        {
            get { return _TempThumbPath; }
            set
            {
                if (_TempThumbPath != value)
                {
                    _TempThumbPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _TempFilePath;
        [ACPropertyInfo(6, "TempFilePath", "en{'File path'}de{'Dateipfad'}")]
        public string TempFilePath
        {
            get { return _TempFilePath; }
            set
            {
                if (_TempFilePath != value)
                {
                    _TempFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        private byte[] _ImageContent;
        /// <summary>
        /// Image data as received from the media service. The client may be a browser or mobile app
        /// without access to the media folder, so images are transported as bytes and not as paths.
        /// Avalonia: Source="{Binding SelectedImage.Image, Converter={x:Static vb:ConverterImagePath.Current}}"
        /// </summary>
        public byte[] ImageContent
        {
            get { return _ImageContent; }
            set
            {
                _ImageContent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Image));
            }
        }

        private byte[] _ThumbContent;
        public byte[] ThumbContent
        {
            get { return _ThumbContent; }
            set
            {
                _ThumbContent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImageThumb));
            }
        }

        /// <summary>
        /// Image bytes for an Image control (needs a converter in Avalonia).
        /// </summary>
        public byte[] Image
        {
            get { return _ImageContent; }
        }

        /// <summary>
        /// Thumbnail bytes for an Image control (needs a converter in Avalonia).
        /// </summary>
        public byte[] ImageThumb
        {
            get { return _ThumbContent; }
        }

        #endregion

        #endregion

        #region presentation

        private string _Name;
        [ACPropertyInfo(5, "Name", "en{'Name'}de{'Name'}")]
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsDefault;
        [ACPropertyInfo(6, "IsDefault", "en{'Is default'}de{'Ist Standard'}")]
        public bool IsDefault
        {
            get { return _IsDefault; }
            set
            {
                if (_IsDefault != value)
                {
                    _IsDefault = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsGenerateThumb;
        [ACPropertyInfo(7, "IsGenerateThumb", "en{'Generate Thumb Image'}de{'Standard-Daumenbild generieren'}")]
        public bool IsGenerateThumb
        {
            get { return _IsGenerateThumb; }
            set
            {
                if (_IsGenerateThumb != value)
                {
                    _IsGenerateThumb = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsNew { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MediaItemTypeEnum MediaType { get; set; }

        #endregion

        #region Methods

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
