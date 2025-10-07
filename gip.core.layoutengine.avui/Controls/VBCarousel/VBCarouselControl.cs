using System;
using Avalonia.Controls;
using Avalonia.Threading;
using gip.core.datamodel;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;


namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// VBCarousel control
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBCarouselControl'}de{'VBCarouselControl'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Optional, true, false)]
    public class VBCarouselControl : Canvas
    {
        /// <summary>
        /// Creates a new instance of VBCarouselControl
        /// </summary>
        public VBCarouselControl()
        {
            _timer.Tick += new EventHandler(TimerTick);
            _timer.Interval = TimeSpan.FromMilliseconds(10);
        }

        DateTime _previousTime;
        DateTime _currentTime;

        /// <summary>
        /// Reinitialize control.
        /// </summary>
        public void ReInitialize()
        {
            Init();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            Init();
        }

        /// <summary>
        /// The event handler.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void OnElementSelectedHandler(object sender);

        /// <summary>
        /// The OnElementSelected events. Fires on selected element changed.
        /// </summary>
        public event OnElementSelectedHandler OnElementSelected;

        /// <summary>
        /// Selects the element.
        /// </summary>
        /// <param name="element">The element to select.</param>
        public void SelectElement(Control element)
        {
            if (element != null)
            {
                _previousTime = DateTime.Now;

                RotateToElement(element);

                if (OnElementSelected != null)
                    OnElementSelected(element);

                _currentlySelected = element;
            }
        }

        private const double DEFAULT_ROTATION_SPEED = 100;
        private const double MINIMUM_ROTATION_SPEED = 1;
        private const double MAXIMUM_ROTATION_SPEED = 1000;
        private double _rotationSpeed = DEFAULT_ROTATION_SPEED;

        /// <summary>
        /// Gets or sets the rotation speed.
        /// </summary>
        public double RotationSpeed
        {
            get
            {
                return _rotationSpeed;

            }
            set
            {
                _rotationSpeed = Math.Min(Math.Max(value, MINIMUM_ROTATION_SPEED), MAXIMUM_ROTATION_SPEED);
            }
        }

        private const double DEFAULT_LOOKDOWN_OFFSET = 100;
        private const double MINIMUM_LOOKDOWN_OFFSET = -100;
        private const double MAXIMUM_LOOKDOWN_OFFSET = 100;
        private double _lookdownOffset = DEFAULT_LOOKDOWN_OFFSET;

        /// <summary>
        /// Gets or sets lookdown offset.
        /// </summary>
        public double LookDownOffset
        {
            get
            {
                return _lookdownOffset;

            }
            set
            {
                _lookdownOffset = Math.Min(Math.Max(value, MINIMUM_LOOKDOWN_OFFSET), MAXIMUM_LOOKDOWN_OFFSET);
            }
        }

        private const double DEFAULT_FADE = 0.3;
        private const double MINIMUM_FADE = 0;
        private const double MAXIMUM_FADE = 1;
        private double _fade = DEFAULT_FADE;

        /// <summary>
        /// Gets or sets the fade.
        /// </summary>
        public double Fade
        {
            get
            {
                return _fade;

            }
            set
            {
                _fade = Math.Min(Math.Max(value, MINIMUM_FADE), MAXIMUM_FADE);
            }
        }

        private const double DEFAULT_SCALE = 0.3;
        private const double MINIMUM_SCALE = 0;
        private const double MAXIMUM_SCALE = 1;
        private double _scale = DEFAULT_SCALE;

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        public double Scale
        {
            get
            {
                return _scale;

            }
            set
            {
                _scale = Math.Min(Math.Max(value, MINIMUM_SCALE), MAXIMUM_SCALE);
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                SelectElement(e.Source as Control);
            }
        }

        /// <summary>
        /// Handles OnPointerWheelChanged event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            int index = Children.IndexOf(CurrentlySelected);
            if (e.Delta.Y > 0 && !_timer.IsEnabled)
            {
                if (index == 0)
                    SelectElement(Children[Children.Count - 1] as Control);
                else
                    SelectElement(Children[index - 1] as Control);

            }
            else if (e.Delta.Y < 0 && !_timer.IsEnabled)
            {
                if (index == Children.Count - 1)
                    SelectElement(Children[0] as Control);
                else
                    SelectElement(Children[index + 1] as Control);
            }
        }

        private void RotateToElement(Control element)
        {
            if (element != _currentlySelected)
            {
                //_currentlySelected = element;
                int targetIndex = Children.IndexOf(element);

                double degreesToRotate = GetDegreesNeededToPlaceElementInFront(_currentRotation, targetIndex, TotalNumberOfElements);
                _targetRotation = ClampDegrees(_currentRotation - degreesToRotate);

                StartRotation(degreesToRotate);
            }
        }

        internal static double GetDegreesNeededToPlaceElementInFront(double currentRotation, int targetIndex, int totalNumberOfElements)
        {
            double rawDegrees = -(180.0 - (currentRotation + 360.0 * ((double)targetIndex / (double)totalNumberOfElements)));

            if (rawDegrees > 180)
                return -(360 - rawDegrees);

            return rawDegrees;
        }

        private double RotationAmount
        {
            get
            {
                return (_currentTime - _previousTime).TotalSeconds * _rotationSpeed;
            }
        }


        private const double INTERNAL_SCALE_COEFFICIENT = 0.6;
        private void Init()
        {
            _previousTime = _currentTime = DateTime.Now;

            X_SCALE = CenterX * INTERNAL_SCALE_COEFFICIENT;
            Y_SCALE = CenterY * INTERNAL_SCALE_COEFFICIENT;

            foreach (Control element in Children)
            {
                element.PointerPressed += Element_PointerPressed;
                element.Cursor = new Cursor(StandardCursorType.Hand);
            }

            SelectElement(GetChild(0));

            SetElementPositions();
        }

        private void Element_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                SelectElement(sender as Control);
            }
        }

        private Control _currentlySelected = null;

        /// <summary>
        /// Gets the CurrentlySelected.
        /// </summary>
        public Control CurrentlySelected { get { return _currentlySelected; } }

        /// <summary>
        /// Gets the CenterX.
        /// </summary>
        protected double CenterX { get { return this.Width / 2.0; } }

        /// <summary>
        /// Gets the CenterY.
        /// </summary>
        protected double CenterY { get { return this.Height / 2.0; } }

        /// <summary>
        /// The X_SCALE.
        /// </summary>
        protected double X_SCALE = 0;

        /// <summary>
        /// The Y_SCALE.
        /// </summary>
        protected double Y_SCALE = 0;

        /// <summary>
        /// The dispatcher timer.
        /// </summary>
        protected DispatcherTimer _timer = new DispatcherTimer();
        private double _rotationToGo = 0;

        private int TotalNumberOfElements { get { return Children.Count; } }

        /// <summary>
        /// The current rotation.
        /// </summary>
        protected double _currentRotation = 0;

        /// <summary>
        /// The target rotation.
        /// </summary>
        protected double _targetRotation = 0;

        /// <summary>
        /// Timer ticks.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void TimerTick(object sender, EventArgs e)
        {
            _currentTime = DateTime.Now;

            if ((_rotationToGo < RotationAmount) && (_rotationToGo > -RotationAmount))
            {
                _rotationToGo = 0;

                if (_currentRotation != _targetRotation)
                {
                    _currentRotation = _targetRotation;
                }
                else
                {
                    _timer.Stop();
                    return;
                }
            }
            else if (_rotationToGo < 0)
            {
                _rotationToGo += RotationAmount;
                _currentRotation = ClampDegrees(_currentRotation + RotationAmount);
            }
            else
            {
                _rotationToGo -= RotationAmount;
                _currentRotation = ClampDegrees(_currentRotation - RotationAmount);
            }

            SetElementPositions();

            _previousTime = _currentTime;
        }

        /// <summary>
        /// Clamps the degrees.
        /// </summary>
        /// <param name="rawDegrees">The raw degrees parameter.</param>
        /// <returns>Returns the clamped degrees.</returns>
        protected double ClampDegrees(double rawDegrees)
        {
            if (rawDegrees > 360)
                return rawDegrees - 360;

            if (rawDegrees < 0)
                return rawDegrees + 360;

            return rawDegrees;
        }

        /// <summary>
        /// Sets a positions of the element.
        /// </summary>
        public void SetElementPositions()
        {
            for (int index = 0; index < TotalNumberOfElements; index++)
            {
                Control element = GetChild(index);

                double elementWidthCenter = GetElementCenter(element.Width, element.Bounds.Width);
                double elementHeightCenter = GetElementCenter(element.Height, element.Bounds.Height);

                double degrees = 360 * ((double)index / (double)TotalNumberOfElements) + _currentRotation;

                double x = -X_SCALE * Math.Sin(ConvertToRads(degrees)) - (double.IsNaN(Y_SCALE) ? 0.0 : Y_SCALE / 100.0) * (Math.Cos(ConvertToRads(degrees)) * LookDownOffset);
                Canvas.SetLeft(element, x + CenterX - elementWidthCenter);

                double y = Y_SCALE * Math.Sin(ConvertToRads(degrees)) - (double.IsNaN(X_SCALE) ? 0.0 : X_SCALE / 100.0) * (Math.Cos(ConvertToRads(degrees)) * LookDownOffset);
                Canvas.SetTop(element, y + CenterY - elementHeightCenter);

                // Set transforms for Avalonia
                var scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = scaleTransform.ScaleY = GetScaledSize(degrees);
                element.RenderTransform = scaleTransform;
                element.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                element.ZIndex = GetZValue(degrees);

                SetOpacity(element, degrees);
            }
        }

        private Control GetChild(int index)
        {
            if (Children.Count == 0)
                return null;

            Control element = Children[index] as Control;

            if (element == null)
                throw new NotSupportedException("Carousel only supports children that are Control elements");

            return element;
        }

        internal static double GetElementCenter(double elementDimension, double elementActualDimension)
        {
            return double.IsNaN(elementDimension) ? elementActualDimension / 2.0 : elementDimension / 2.0;
        }


        private void SetOpacity(Control element, double degrees)
        {
            element.Opacity = (1.0 - Fade) + Fade * GetCoefficient(degrees);
        }

        private int GetZValue(double degrees)
        {
            return (int)(360 * GetCoefficient(degrees));
        }

        private double GetScaledSize(double degrees)
        {
            return (1.0 - Scale) + Scale * GetCoefficient(degrees);
        }

        private double GetCoefficient(double degrees)
        {
            return 1.0 - Math.Cos(ConvertToRads(degrees)) / 2 - 0.5;
        }

        private double ConvertToRads(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private void StartRotation(double numberOfDegrees)
        {
            _rotationToGo = numberOfDegrees;
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
        }
    }
}
