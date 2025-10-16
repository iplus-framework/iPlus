using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a rating item in <see cref="VBRatingControl"/>.
    /// </summary>
    public class RatingItem : INotifyPropertyChanged
    {
        private int _sn;
        private bool _isSelected;

        public int Sn 
        { 
            get => _sn;
            set
            {
                if (_sn != value)
                {
                    _sn = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected 
        { 
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
