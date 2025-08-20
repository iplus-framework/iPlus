using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls;

namespace gip.core.layoutengine.avui.timeline
{
    public class RulerBlockItem : INotifyPropertyChanged
    {

        private string text;

        public RulerBlockItem(TimelineRulerControl rulerControl, int rulerBlockItemIndex) : base()
        {
            _RulerControl = rulerControl;
            RulerBlockItemIndex = rulerBlockItemIndex;
        }

        public void DeInitControl()
        {
            RulerBlockItemIndex = -1;
            onPropertyChanged("RulerBlockItemIndex");
        }

        public TimeSpan Span { get; set; }

        public DateTime Start { get; set; }

        public int RulerBlockItemIndex
        {
            get;
            set;
        }

        private string calcText;

        private string headerText;
        public string HeaderText
        {
            get
            {
                if(headerText == null)
                {
                    if (Span.TotalDays > 50)
                        headerText = Start.ToString("MM/yyyy");

                    else
                        headerText = Start.AddSeconds(Span.TotalSeconds / 2).ToString("dd/MM/yyyy");
                }

                return headerText;
            }
        }

        public string Text
        {
            get
            {
                if (text == null)
                {
                    if (calcText == null)
                    {
                        if (Span.TotalDays > 50)
                        {
                            calcText = Start.ToString("MM/yy");
                        }
                        else if (Span.TotalDays > 1)
                        {
                            calcText = Start.ToString("dd/MM/yy");
                        }
                        else
                        {
                            calcText = Start.ToString("HH:mm");
                        }
                    }

                    return calcText;
                }
                return text;
            }
            set { text = value; }
        }

        public DateTime End
        {
            get { return Start.Add(Span); }
        }

        private TimelineRulerControl _RulerControl;

        private List<double> _RulerBlockTimeItemsOffsets;
        public List<double> RulerBlockTimeItemsOffsets
        {
            get => _RulerBlockTimeItemsOffsets;
            set
            {
                _RulerBlockTimeItemsOffsets = value;
                if (_RulerControl != null)
                    _RulerControl.UpdateTimeLines(this);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string name)
        {
            var handlers = PropertyChanged;
            if (handlers != null)
            {
                handlers(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
