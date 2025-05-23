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
using System.ComponentModel;
using System.Windows.Markup;
using System.Globalization;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a <see cref="DateTime"/> up or down switch control.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein <see cref="DateTime"/> Up- oder Down-Steuerelement dar.
    /// </summary>
    public class VBDateTimeUpDown : UpDownBase, IClearVBContent
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "DateTimeUpDownStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDateTimeUpDown/Themes/DateTimeUpDownStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "DateTimeUpDownStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDateTimeUpDown/Themes/DateTimeUpDownStyleAero.xaml" },
        };
        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBDateTimeUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBDateTimeUpDown), new FrameworkPropertyMetadata(typeof(VBDateTimeUpDown)));
            ValueTypeProperty.OverrideMetadata(typeof(VBDateTimeUpDown), new FrameworkPropertyMetadata(typeof(Nullable<DateTime>)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBDateTimeUpDown.
        /// </summary>
        public VBDateTimeUpDown()
        {
            DateTimeFormatInfo = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentCulture);
            InitializeDateTimeInfoList();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
            TextBox.SelectionChanged += TextBox_SelectionChanged;
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        #region Members

        private List<DateTimeInfo> _dateTimeInfoList = new List<DateTimeInfo>();

        protected int _keyPressedCounterInSel = 0;
        protected int _lastEnteredDigit = 0;
        private DateTimeInfo _selectedDateTimeInfo;
        private DateTimeInfo SelectedDateTimeInfo
        {
            get
            {
                return _selectedDateTimeInfo;
            }
            set
            {
                if (_selectedDateTimeInfo != value)
                    _keyPressedCounterInSel = 0;
                _selectedDateTimeInfo = value;
            }
        }

        private bool _fireSelectionChangedEvent = true;

        #endregion //Members

        #region Properties

        private DateTimeFormatInfo DateTimeFormatInfo { get; set; }

        #region Format

        /// <summary>
        /// Represents the dependency property for Format.
        /// </summary>
        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(DateTimeFormat), typeof(VBDateTimeUpDown), new UIPropertyMetadata(DateTimeFormat.FullDateTime, OnFormatChanged));

        /// <summary>
        /// Gets or sets the DateTime format according DateTimeFormat enumeration.
        /// </summary>
        [Category("VBControl")]
        public DateTimeFormat Format
        {
            get { return (DateTimeFormat)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        private static void OnFormatChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBDateTimeUpDown dateTimeUpDown = o as VBDateTimeUpDown;
            if (dateTimeUpDown != null)
                dateTimeUpDown.OnFormatChanged((DateTimeFormat)e.OldValue, (DateTimeFormat)e.NewValue);
        }

        protected virtual void OnFormatChanged(DateTimeFormat oldValue, DateTimeFormat newValue)
        {
            //if using a CustomFormat then the initialization occurs on the CustomFormatString property
            if (newValue != DateTimeFormat.Custom)
                InitializeDateTimeInfoListAndParseValue();
        }

        #endregion //Format

        #region FormatString

        /// <summary>
        /// Represents the dependency property for FormatString.
        /// </summary>
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register("FormatString", typeof(string), typeof(VBDateTimeUpDown), new UIPropertyMetadata(default(String), OnFormatStringChanged));

        /// <summary>
        /// Gets or sets the format string.
        /// </summary>
        [Category("VBControl")]
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        private static void OnFormatStringChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBDateTimeUpDown dateTimeUpDown = o as VBDateTimeUpDown;
            if (dateTimeUpDown != null)
                dateTimeUpDown.OnFormatStringChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnFormatStringChanged(string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
                throw new ArgumentException("CustomFormat should be specified.", FormatString);

            InitializeDateTimeInfoListAndParseValue();
        }

        #endregion //FormatString

        #region controlmode
        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBDateTimeUpDown));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get
            {
                return (Global.ControlModes)GetValue(ControlModeProperty);
            }
            set
            {
                SetValue(ControlModeProperty, value);
            }
        }
        #endregion

        #endregion //Properties

        #region Base Class Overrides

        /// <summary>
        /// Handles the OnPreviewKeyDown event.
        /// </summary>
        /// <param name="e">The event parameters.</param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    {
                        return;
                    }
                case Key.Delete:
                    {
                        Value = null;
                        break;
                    }
                case Key.Left:
                    {
                        PerformKeyboardSelection(-1);
                        break;
                    }
                case Key.Right:
                    {
                        PerformKeyboardSelection(1);
                        break;
                    }
                default:
                    {
                        if (e.Key >= Key.D0 && e.Key <= Key.D9)
                        {
                            DigitEntered((int)(e.Key - Key.D0));
                        }
                        else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                        {
                            DigitEntered((int)(e.Key - Key.NumPad0));
                        }
                        break;
                    }
            }

            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// Handles the OnMouseDoubleClick event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (Value == null)
                Value = DateTime.Now;
            else
            {
                try
                {
                    DateTime time = (DateTime)Value;
                    if (time.Year < 1900)
                        Value = DateTime.Now;
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBDateTimeUpDown", "OnMouseDubleClick", msg);
                }
            }
            base.OnMouseDoubleClick(e);
        }

        /// <summary>
        /// Handles a on value changed and parses a new value if is not null.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnValueChanged(object oldValue, object newValue)
        {
            //whenever the value changes we need to parse out the value into out DateTimeInfo segments so we can keep track of the individual pieces
            //but only if it is not null
            if (newValue != null)
                ParseValueIntoDateTimeInfo();

            base.OnValueChanged(oldValue, newValue);
        }

        /// <summary>
        /// Parses a value if is not null and is not a type of DateTime.
        /// </summary>
        /// <param name="value">The value parameter.</param>
        /// <returns>The date and time object if a parsing is successfull.</returns>
        protected override object OnCoerceValue(object value)
        {
            //if the user entered a string value to represent a date or time, we need to parse that string into a valid DatTime value
            if (value != null && !(value is DateTime))
            {
                return DateTime.Parse(value.ToString(), DateTimeFormatInfo);
            }

            return base.OnCoerceValue(value);
        }

        #endregion //Base Class Overrides

        #region Event Hanlders

        void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_fireSelectionChangedEvent)
                PerformMouseSelection();
            else
                _fireSelectionChangedEvent = true;
        }

        #endregion //Event Hanlders

        #region Methods

        #region Abstract

        protected override void OnIncrement()
        {
            if (Value != null)
                UpdateDateTime(1);
        }

        protected override void OnDecrement()
        {
            if (Value != null)
                UpdateDateTime(-1);
        }

        protected override object ConvertTextToValue(string text)
        {
            if (String.IsNullOrEmpty(text))
                return DateTime.MinValue;
            try
            {
                DateTime dt = DateTime.Parse(text, CultureInfo.CurrentCulture);
                return dt;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDateTimeUpDown", "ConvertTextToValue", msg);

                return DateTime.MinValue;
            }
        }

        protected override string ConvertValueToText(object value)
        {
            if (value == null)
                return string.Empty;
            if (value is string)
                return value as string;

            DateTime dt = DateTime.Parse(value.ToString(), CultureInfo.CurrentCulture);
            return dt.ToString(GetFormatString(Format), CultureInfo.CurrentCulture);
        }

        #endregion //Abstract

        #region Private

        private void InitializeDateTimeInfoListAndParseValue()
        {
            InitializeDateTimeInfoList();
            if (Value != null)
                ParseValueIntoDateTimeInfo();
        }

        private void InitializeDateTimeInfoList()
        {
            _dateTimeInfoList.Clear();

            string format = GetFormatString(Format);
            while (format.Length > 0)
            {
                int elementLength = GetElementLengthByFormat(format);
                DateTimeInfo info = null;

                switch (format[0])
                {
                    case '"':
                    case '\'':
                        {
                            int closingQuotePosition = format.IndexOf(format[0], 1);
                            info = new DateTimeInfo { IsReadOnly = true, Type = DateTimePart.Other, Length = 1, Content = format.Substring(1, Math.Max(1, closingQuotePosition - 1)).ToString() };
                            elementLength = Math.Max(1, closingQuotePosition + 1);
                            break;
                        }
                    case 'D':
                    case 'd':
                        {
                            string d = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                d = "%" + d;

                            if (elementLength > 2)
                                info = new DateTimeInfo { IsReadOnly = true, Type = DateTimePart.DayName, Format = d };
                            else
                                info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Day, Format = d };
                            break;
                        }
                    case 'F':
                    case 'f':
                        {
                            string f = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                f = "%" + f;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Millisecond, Format = f };
                            break;
                        }
                    case 'h':
                        {
                            string h = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                h = "%" + h;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Hour12, Format = h };
                            break;
                        }
                    case 'H':
                        {
                            string H = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                H = "%" + H;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Hour24, Format = H };
                            break;
                        }
                    case 'M':
                        {
                            string M = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                M = "%" + M;

                            if (elementLength >= 3)
                                info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.MonthName, Format = M };
                            else
                                info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Month, Format = M };
                            break;
                        }
                    case 'S':
                    case 's':
                        {
                            string s = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                s = "%" + s;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Second, Format = s };
                            break;
                        }
                    case 'T':
                    case 't':
                        {
                            string t = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                t = "%" + t;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.AmPmDesignator, Format = t };
                            break;
                        }
                    case 'Y':
                    case 'y':
                        {
                            string y = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                y = "%" + y;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Year, Format = y };
                            break;
                        }
                    case '\\':
                        {
                            if (format.Length >= 2)
                            {
                                info = new DateTimeInfo { IsReadOnly = true, Content = format.Substring(1, 1), Length = 1, Type = DateTimePart.Other };
                                elementLength = 2;
                            }
                            break;
                        }
                    case 'g':
                        {
                            string g = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                g = "%" + g;

                            info = new DateTimeInfo { IsReadOnly = true, Type = DateTimePart.Period, Format = format.Substring(0, elementLength) };
                            break;
                        }
                    case 'm':
                        {
                            string m = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                m = "%" + m;

                            info = new DateTimeInfo { IsReadOnly = false, Type = DateTimePart.Minute, Format = m };
                            break;
                        }
                    case 'z':
                        {
                            string z = format.Substring(0, elementLength);
                            if (elementLength == 1)
                                z = "%" + z;

                            info = new DateTimeInfo { IsReadOnly = true, Type = DateTimePart.TimeZone, Format = z };
                            break;
                        }
                    default:
                        {
                            elementLength = 1;
                            info = new DateTimeInfo { IsReadOnly = true, Length = 1, Content = format[0].ToString(), Type = DateTimePart.Other };
                            break;
                        }
                }

                _dateTimeInfoList.Add(info);
                format = format.Substring(elementLength);
            }
        }

        private static int GetElementLengthByFormat(string format)
        {
            for (int i = 1; i < format.Length; i++)
            {
                if (String.Compare(format[i].ToString(), format[0].ToString(), false) != 0)
                {
                    return i;
                }
            }
            return format.Length;
        }

        private void ParseValueIntoDateTimeInfo()
        {
            string text = string.Empty;

            _dateTimeInfoList.ForEach(info =>
            {
                if (info.Format == null)
                {
                    info.StartPosition = text.Length;
                    info.Length = info.Content.Length;
                    text += info.Content;
                }
                else
                {
                    DateTime date = DateTime.Parse(Value.ToString());
                    info.StartPosition = text.Length;
                    info.Content = date.ToString(info.Format, DateTimeFormatInfo);
                    info.Length = info.Content.Length;
                    text += info.Content;
                }
            });
        }

        private void PerformMouseSelection()
        {
            _dateTimeInfoList.ForEach(info =>
            {
                if ((info.StartPosition <= TextBox.SelectionStart) && (TextBox.SelectionStart < (info.StartPosition + info.Length)))
                {
                    Select(info);
                    return;
                }
            });
        }

        /// <summary>
        /// Performs the keyboard selection.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <remarks>-1 = Left, 1 = Right</remarks>
        private void PerformKeyboardSelection(int direction)
        {
            DateTimeInfo info;
            int index = _dateTimeInfoList.IndexOf(SelectedDateTimeInfo);

            //make sure we stay within the selection ranges
            if ((index == 0 && direction == -1) || (index == _dateTimeInfoList.Count - 1 && direction == 1))
                return;

            //get the DateTimeInfo at the next position
            index += direction;
            info = _dateTimeInfoList[index];

            //we don't care about spaces and commas, only select valid DateTimeInfos
            while (info.Type == DateTimePart.Other)
            {
                info = _dateTimeInfoList[index += direction];
            }

            //perform selection
            Select(info);
        }

        private void Select(DateTimeInfo info)
        {
            _fireSelectionChangedEvent = false;
            TextBox.Select(info.StartPosition, info.Length);
            _fireSelectionChangedEvent = true;
            SelectedDateTimeInfo = info;
        }

        private string GetFormatString(DateTimeFormat dateTimeFormat)
        {
            switch (dateTimeFormat)
            {
                case DateTimeFormat.ShortDate:
                    return DateTimeFormatInfo.ShortDatePattern;
                case DateTimeFormat.LongDate:
                    return DateTimeFormatInfo.LongDatePattern;
                case DateTimeFormat.ShortTime:
                    return DateTimeFormatInfo.ShortTimePattern;
                case DateTimeFormat.LongTime:
                    return DateTimeFormatInfo.LongTimePattern;
                case DateTimeFormat.FullDateTime:
                    return DateTimeFormatInfo.FullDateTimePattern;
                case DateTimeFormat.MonthDay:
                    return DateTimeFormatInfo.MonthDayPattern;
                case DateTimeFormat.RFC1123:
                    return DateTimeFormatInfo.RFC1123Pattern;
                case DateTimeFormat.SortableDateTime:
                    return DateTimeFormatInfo.SortableDateTimePattern;
                case DateTimeFormat.UniversalSortableDateTime:
                    return DateTimeFormatInfo.UniversalSortableDateTimePattern;
                case DateTimeFormat.YearMonth:
                    return DateTimeFormatInfo.YearMonthPattern;
                case DateTimeFormat.Custom:
                    return FormatString;
                default:
                    throw new ArgumentException("Not a supported format");
            }
        }

        private void DigitEntered(int digit)
        {
            _fireSelectionChangedEvent = false;
            DateTimeInfo info = SelectedDateTimeInfo;
            _keyPressedCounterInSel++;
            bool goToNextSel = false;

            //this only occurs when the user manually type in a value for the Value Property
            if (info == null)
                info = _dateTimeInfoList[0];
            if (info == null)
                return;

            int digitPosition = _keyPressedCounterInSel;
            DateTime dtValue = DateTime.Now;
            if (Value != null)
                dtValue = (DateTime)Value;

            try
            {
                switch (info.Type)
                {
                    case DateTimePart.Year:
                        {
                            if (info.Length == 4 && digitPosition <= 4)
                            {
                                int year = dtValue.Year;
                                if (digitPosition == 1)
                                    year = digit * 1000;
                                else if (digitPosition == 2)
                                    year = dtValue.Year + (digit * 100);
                                else if (digitPosition == 3)
                                    year = dtValue.Year + (digit * 10);
                                else if (digitPosition == 4)
                                {
                                    year = dtValue.Year + digit;
                                    _keyPressedCounterInSel = 0;
                                    goToNextSel = true;
                                }
                                Value = new DateTime(year, dtValue.Month, dtValue.Day, dtValue.Hour, dtValue.Minute, dtValue.Second);
                            }
                            else
                                _keyPressedCounterInSel = 0;
                            break;
                        }
                    case DateTimePart.Month:
                        {
                            if (info.Length == 2 && digitPosition <= 2)
                            {
                                int month = dtValue.Month;
                                if (digitPosition == 1)
                                    month = digit * 10;
                                else if (digitPosition == 2)
                                {
                                    if (month == 12)
                                        month = digit;
                                    else
                                        month = dtValue.Month + digit;
                                    _keyPressedCounterInSel = 0;
                                    goToNextSel = true;
                                }
                                if (month <= 0 || month > 12)
                                    month = 12;
                                Value = new DateTime(dtValue.Year, month, dtValue.Day, dtValue.Hour, dtValue.Minute, dtValue.Second);
                            }
                            else
                                _keyPressedCounterInSel = 0;
                            break;
                        }
                    case DateTimePart.Day:
                        {
                            if (info.Length == 2 && digitPosition <= 2)
                            {
                                int day = dtValue.Day;
                                if (digitPosition == 1)
                                {
                                    day = digit * 10;
                                }
                                else if (digitPosition == 2)
                                {
                                    if (_lastEnteredDigit > 3)
                                        day = digit;
                                    else
                                    {
                                        day = _lastEnteredDigit * 10 + digit;
                                        //day = dtValue.Day + digit;
                                    }
                                    _keyPressedCounterInSel = 0;
                                    goToNextSel = true;
                                }
                                if (day <= 0 || day > 31)
                                    day = 31;
                                Value = new DateTime(dtValue.Year, dtValue.Month, day, dtValue.Hour, dtValue.Minute, dtValue.Second);
                            }
                            else
                                _keyPressedCounterInSel = 0;
                            break;
                        }
                    case DateTimePart.Hour12:
                    case DateTimePart.Hour24:
                        {
                            if (info.Length == 2 && digitPosition <= 2)
                            {
                                int hour = dtValue.Hour;
                                if (digitPosition == 1)
                                    hour = digit * 10;
                                else if (digitPosition == 2)
                                {
                                    hour = dtValue.Hour + digit;
                                    _keyPressedCounterInSel = 0;
                                    goToNextSel = true;
                                }
                                if (hour > 12 && info.Type == DateTimePart.Hour12)
                                    hour = 12;
                                else if (hour > 23 && info.Type == DateTimePart.Hour24)
                                    hour = 0;
                                Value = new DateTime(dtValue.Year, dtValue.Month, dtValue.Day, hour, dtValue.Minute, dtValue.Second);
                            }
                            else
                                _keyPressedCounterInSel = 0;
                            break;
                        }
                    case DateTimePart.Minute:
                        {
                            if (info.Length == 2 && digitPosition <= 2)
                            {
                                int minute = dtValue.Minute;
                                if (digitPosition == 1)
                                    minute = digit * 10;
                                else if (digitPosition == 2)
                                {
                                    minute = dtValue.Minute + digit;
                                    _keyPressedCounterInSel = 0;
                                    goToNextSel = true;
                                }
                                if (minute > 59)
                                    minute = 0;
                                Value = new DateTime(dtValue.Year, dtValue.Month, dtValue.Day, dtValue.Hour, minute, dtValue.Second);
                            }
                            else
                                _keyPressedCounterInSel = 0;
                            break;
                        }
                    case DateTimePart.Second:
                        {
                            if (info.Length == 2 && digitPosition <= 2)
                            {
                                int second = dtValue.Second;
                                if (digitPosition == 1)
                                    second = digit * 10;
                                else if (digitPosition == 2)
                                {
                                    second = dtValue.Second + digit;
                                    _keyPressedCounterInSel = 0;
                                    goToNextSel = true;
                                }
                                if (second > 59)
                                    second = 0;
                                Value = new DateTime(dtValue.Year, dtValue.Month, dtValue.Day, dtValue.Hour, dtValue.Minute, second);
                            }
                            else
                                _keyPressedCounterInSel = 0;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                _lastEnteredDigit = digit;
                _fireSelectionChangedEvent = true;

                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDateTimeUpDown", "DigitEntered", msg);

                return;
            }

            _lastEnteredDigit = digit;
            //we loose our selection when the Value is set so we need to reselect it without firing the selection changed event
            TextBox?.Select(info.StartPosition, info.Length);
            _fireSelectionChangedEvent = true;
            if (goToNextSel)
                PerformKeyboardSelection(1);
        }

        private void UpdateDateTime(int value)
        {
            _fireSelectionChangedEvent = false;
            DateTimeInfo info = SelectedDateTimeInfo;

            //this only occurs when the user manually type in a value for the Value Property
            if (info == null)
                info = _dateTimeInfoList[0];


            switch (info.Type)
            {
                case DateTimePart.Year:
                    {
                        Value = ((DateTime)Value).AddYears(value);
                        break;
                    }
                case DateTimePart.Month:
                case DateTimePart.MonthName:
                    {
                        Value = ((DateTime)Value).AddMonths(value);
                        break;
                    }
                case DateTimePart.Day:
                case DateTimePart.DayName:
                    {
                        Value = ((DateTime)Value).AddDays(value);
                        break;
                    }
                case DateTimePart.Hour12:
                case DateTimePart.Hour24:
                    {
                        Value = ((DateTime)Value).AddHours(value);
                        break;
                    }
                case DateTimePart.Minute:
                    {
                        Value = ((DateTime)Value).AddMinutes(value);
                        break;
                    }
                case DateTimePart.Second:
                    {
                        Value = ((DateTime)Value).AddSeconds(value);
                        break;
                    }
                case DateTimePart.Millisecond:
                    {
                        Value = ((DateTime)Value).AddMilliseconds(value);
                        break;
                    }
                case DateTimePart.AmPmDesignator:
                    {
                        Value = ((DateTime)Value).AddHours(value * 12);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            //we loose our selection when the Value is set so we need to reselect it without firing the selection changed event
            TextBox.Select(info.StartPosition, info.Length);
            _fireSelectionChangedEvent = true;
        }

        public void Clear()
        {
            if (TemplatedParent is IClearVBContent)
                (TemplatedParent as IClearVBContent).Clear();
            else if (Value != null && Value is DateTime?)
                Value = null;
            else if (Value != null && Value is DateTime)
                Value = DateTime.MinValue;
        }
        #endregion //Private

        #endregion //Methods

    }
}
