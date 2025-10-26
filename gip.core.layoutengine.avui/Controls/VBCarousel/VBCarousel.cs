using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Metadata;
using gip.ext.designer.avui;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a control that allows user to select item in carousel style.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Steuerelement dar, mit dem der Benutzer ein Element im Karussell-Stil auswählen kann.
    /// </summary>
    [TemplatePart(Name = "PART_VBCarouselControl", Type = typeof(VBCarouselControl))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBCarousel'}de{'VBCarousel'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBCarousel : SelectingItemsControl, IVBContent, IVBSource, IACObject
    {
        #region c'tors
        string _DataSource;
        string _DataShowColumns;
        string _DataChilds;

        /// <summary>
        /// Createas a new instance of VBCarousel.
        /// </summary>
        public VBCarousel() : base()
        {
        
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            var partObj = e.NameScope.Find("PART_VBCarouselControl");
            if (partObj != null && partObj is VBCarouselControl)
            {
                _PART_VBCarouselControl = (VBCarouselControl)partObj;
                _PART_VBCarouselControl.OnElementSelected += _PART_VBCarouselControl_OnElementSelected;
                _PART_VBCarouselControl.RotationSpeed = RotationSpeed;
                _PART_VBCarouselControl.Fade = Fade;
                _PART_VBCarouselControl.Scale = Scale;
                _PART_VBCarouselControl.LookDownOffset = LookdownOffset;
                if (Orientation == "Vertical")
                {
                    PART_VBCarouselControl.Width = 0;
                    if (CarouselHeight > 400.0)
                        _PART_VBCarouselControl.Height = CarouselHeight;
                    else
                        _PART_VBCarouselControl.Height = 900;
                }
                else if (Orientation == "Horizontal")
                {
                    _PART_VBCarouselControl.Height = 0;
                    if (CarouselWidth > 400.0)
                        _PART_VBCarouselControl.Width = CarouselWidth;
                    else
                        _PART_VBCarouselControl.Width = 1350;
                }
            }
            InitVBControl();
        }

        private void _PART_VBCarouselControl_OnElementSelected(object sender)
        {
            if (_PART_VBCarouselControl.CurrentlySelected != null && ((Control)sender).Name == _PART_VBCarouselControl.CurrentlySelected.Name)
                SelectedItem = Items.Cast<ACClassDesign>().FirstOrDefault(c => c.ACClassDesignID.ToString() == _PART_VBCarouselControl.CurrentlySelected.Name);
        }

        #endregion

        #region Loaded-Event

        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized || DataContext == null || ContextACObject == null)
                return;
            _Initialized = true;
            if (String.IsNullOrEmpty(VBContent))
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBCarouselHelper", VBContent);
                return;
            }

            _ACTypeInfo = dcACTypeInfo;

            // Avalonia equivalent for dependency property value source checking
            if (dcRightControlMode < RightControlMode)
            {
                RightControlMode = dcRightControlMode;
            }

            try
            {
                if (BSOACComponent != null)
                    BSOACComponent.AddWPFRef(this.GetHashCode(), dcSource as IACObject);
            }
            catch (Exception exw)
            {
                this.Root().Messages.LogDebug("VBCarousel", "AddWPFRef", exw.Message);
            }

            System.Diagnostics.Debug.Assert(VBContent != "");

            if (IsVisible)
            {
                if (_ACTypeInfo == null)
                    return;

                if (string.IsNullOrEmpty(ACCaption))
                    ACCaptionTrans = _ACTypeInfo.ACCaption;
                else
                    ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);


                string acAccess = "";
                ACClassProperty sourceProperty = null;
                VBSource = VBContentPropertyInfo != null ? VBContentPropertyInfo.GetACSource(VBSource, out acAccess, out sourceProperty) : VBSource;
                IACType dsACTypeInfo = null;
                object dsSource = null;
                string dsPath = "";
                Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;

                if (!ContextACObject.ACUrlBinding(VBSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
                {
                    this.Root().Messages.LogDebug("Error00004", "VBCarouselHelperHelper", VBSource + " " + VBContent);
                    return;
                }

                var binding = new Binding();
                binding.Source = dsSource;
                if (!string.IsNullOrEmpty(dsPath))
                {
                    binding.Path = dsPath;
                }
                this.Bind(VBCarousel.ItemsSourceProperty, binding);

                // Gebundene Spalte setzen (VBContent)
                var binding2 = new Binding();
                binding2.Source = dcSource;
                binding2.Path = dcPath;
                if (VBContentPropertyInfo != null)
                    binding2.Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay;
                this.Bind(VBCarousel.SelectedValueProperty, binding2);

                if (BSOACComponent != null)
                {
                    binding = new Binding();
                    binding.Source = BSOACComponent;
                    binding.Path = Const.InitState;
                    binding.Mode = BindingMode.OneWay;
                    this.Bind(VBCarousel.ACCompInitStateProperty, binding);
                }
            }
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public void DeInitVBControl(IACComponent bso)
        {
            if (!_Initialized)
                return;
            _Initialized = false;

            try
            {
                if (BSOACComponent != null)
                    BSOACComponent.RemoveWPFRef(this.GetHashCode());
            }
            catch (Exception exw)
            {
                this.Root().Messages.LogDebug("VBCarousel", "RemoveWPFRef", exw.Message);
            }

            _ACTypeInfo = null;

            // TODO: Implement Avalonia equivalent for clearing bindings
            // this.ClearAllBindings();

            if (_PART_VBCarouselControl != null)
                _PART_VBCarouselControl.OnElementSelected -= _PART_VBCarouselControl_OnElementSelected;
            _PART_VBCarouselControl = null;
        }


        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

        void VBCarousel_TargetUpdated(object sender, EventArgs e)
        {
            UpdateControlMode();
            FillCarousel();
        }

        void VBCarousel_SourceUpdated(object sender, EventArgs e)
        {
            UpdateControlMode();
        }

        #endregion

        #region PART's

        private VBCarouselControl _PART_VBCarouselControl;
        /// <summary>
        /// Gets the PART VBCarouselControl.
        /// </summary>
        public VBCarouselControl PART_VBCarouselControl
        {
            get 
            {
                return _PART_VBCarouselControl;
            }
        }

        #endregion

        #region Methods
        bool _IsNewItem = false;
        /// <summary>
        /// This method creates images from design binary and put it in VBCarouselControl 
        /// </summary>
        public void FillCarousel()
        {
            if (_PART_VBCarouselControl.Children.Count < Items.Count && _PART_VBCarouselControl.Children.Count > 0)
                _IsNewItem = true;
            Control tempSelectedElement = null;
            _PART_VBCarouselControl.Children.Clear();
            ACClassDesign tempACClassDesign;
            foreach (var item in Items)
            {
                if (item is ACClassDesign)
                {
                    Bitmap imageSource = null;
                    Image img = new Image();
                    tempACClassDesign = item as ACClassDesign;
                    if (tempACClassDesign.DesignBinary != null)
                    {
                        using (MemoryStream stream = new MemoryStream(tempACClassDesign.DesignBinary))
                        {
                            try
                            {
                                imageSource = new Bitmap(stream);
                                img.Source = imageSource;
                            }
                            catch (Exception e)
                            {
                                // Use resource URI for Avalonia
                                img.Source = new Bitmap(System.Reflection.Assembly.GetExecutingAssembly()
                                    .GetManifestResourceStream("gip.core.layoutengine.avui.Images.questionMark2.jpg"));

                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                    datamodel.Database.Root.Messages.LogException("VBCarousel", "FillCarousel", msg);
                            }
                        }
                    }
                    else
                        img.Source = new Bitmap(System.Reflection.Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("gip.core.layoutengine.avui.Images.questionMark2.jpg"));

                    double imageWidth = 370;
                    double imageHeight = 250;

                    if (ImagesSize >= 0.5 && ImagesSize <= 1.5 && ImagesSize != 1.0)
                    {
                        imageWidth = imageWidth * ImagesSize;
                        imageHeight = imageHeight * ImagesSize;
                    }

                    img.Width = imageWidth;
                    img.Height = imageHeight;

                    Border border = new Border();
                    border.BorderThickness = new Thickness(2);
                    border.BorderBrush = new SolidColorBrush(Colors.White);
                    border.Child = img;

                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = tempACClassDesign.ACCaption;
                    textBlock.Width = 250 * ImagesSize;
                    textBlock.Height = img.Height * 0.15;
                    textBlock.FontSize = img.Height * 0.1;
                    textBlock.Foreground = new SolidColorBrush(Colors.Red);
                    textBlock.Opacity = 0.9;
                    textBlock.FontWeight = FontWeight.Bold;
                    textBlock.TextAlignment = TextAlignment.Center;

                    Canvas canvas = new Canvas();
                    canvas.Name = tempACClassDesign.ACClassDesignID.ToString();
                    canvas.Children.Add(border);
                    canvas.Children.Add(textBlock);
                    canvas.Height = img.Height;
                    canvas.Width = img.Width;
                    double textTop = (canvas.Height - textBlock.Height) / 2;
                    Canvas.SetTop(textBlock, textTop);
                    double textLeft = (canvas.Width - textBlock.Width) / 2;
                    Canvas.SetLeft(textBlock, textLeft);
                    _PART_VBCarouselControl.Children.Add(canvas);
                }
            }
            if (SelectedItem != null && _IsNewItem == false)
                tempSelectedElement = _PART_VBCarouselControl.Children.Cast<Control>().FirstOrDefault(c => c.Name == ((ACClassDesign)SelectedItem).ACClassDesignID.ToString());
            else if(_IsNewItem)
            {
                tempSelectedElement = _PART_VBCarouselControl.Children[_PART_VBCarouselControl.Children.Count-1] as Control;
                _IsNewItem = false;
            }
            _PART_VBCarouselControl.ReInitialize();
            _PART_VBCarouselControl.SelectElement(tempSelectedElement);
        }
        #endregion

        #region Additional Dependenc-Properties

        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty
            = AvaloniaProperty.Register<VBCarousel, Global.ControlModes>(nameof(ControlMode));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get
            {
                return GetValue(ControlModeProperty);
            }
            set
            {
                SetValue(ControlModeProperty, value);
            }
        }
        
        /// <summary>
        /// Represents the dependency property for RotationSpeed.
        /// </summary>
        public static readonly StyledProperty<double> RotationSpeedProperty
            = AvaloniaProperty.Register<VBCarousel, double>(nameof(RotationSpeed), 200.0);

        /// <summary>
        /// Gets or sets the rotation speed. It can be set in range 1 : 1000, default value = 200.0
        /// </summary>
        /// <summary>
        /// Liest oder setzt die Drehzahl. Einstellbar im Bereich 1 : 1000, Standardwert = 200,0
        /// </summary>
        [Category("VBControl")]
        public double RotationSpeed
        {
            get { return GetValue(RotationSpeedProperty); }
            set { SetValue(RotationSpeedProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for LookdownOffset.
        /// </summary>
        public static readonly StyledProperty<double> LookdownOffsetProperty
            = AvaloniaProperty.Register<VBCarousel, double>(nameof(LookdownOffset), 25.0);

        /// <summary>
        /// Gets or sets the lookdown offset. It can be set in range -100.0 : 100.0, default value = 25.0
        /// </summary>
        /// <summary>
        /// Liest oder setzt den Lookdown-Offset. Einstellbar im Bereich -100,0 : 100,0, Standardwert = 25,0
        /// </summary>
        [Category("VBControl")]
        public double LookdownOffset
        {
            get { return GetValue(LookdownOffsetProperty); }
            set { SetValue(LookdownOffsetProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for Fade.
        /// </summary>
        public static readonly StyledProperty<double> FadeProperty
            = AvaloniaProperty.Register<VBCarousel, double>(nameof(Fade), 0.1);

        /// <summary>
        /// Gets or sets the fade. It can be set in range 0.0 : 1.0, default value = 0.1
        /// </summary>
        /// <summary>
        /// Liest oder setzt den Fade. Einstellbar im Bereich 0,0 : 1,0, Standardwert = 0,1
        /// </summary>
        [Category("VBControl")]
        public double Fade
        {
            get { return GetValue(FadeProperty); }
            set { SetValue(FadeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for Scale.
        /// </summary>
        public static readonly StyledProperty<double> ScaleProperty
            = AvaloniaProperty.Register<VBCarousel, double>(nameof(Scale), 0.4);


        /// <summary>
        /// Gets or sets the scale. It can be set in range 0.0 : 1.0, default value = 0.4
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Maßstab. Einstellbar im Bereich 0,0 : 1,0, Standardwert = 0,4
        /// </summary>
        [Category("VBControl")]
        public double Scale
        {
            get { return GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        
        /// <summary>
        /// Represents the dependency property for Orientation.
        /// </summary>
        public static readonly StyledProperty<string> OrientationProperty
            = AvaloniaProperty.Register<VBCarousel, string>(nameof(Orientation), "Horizontal");


        /// <summary>
        /// Gets or sets the orientation. It can be Horizontal or Vertical, default value = Horizontal
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Orientierung. Es kann Horizontal oder Vertikal sein, Standardwert = Horizontal
        /// </summary>
        [Category("VBControl")]
        public string Orientation
        {
            get { return GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ImagesSize.
        /// </summary>
        public static readonly StyledProperty<double> ImagesSizeProperty
            = AvaloniaProperty.Register<VBCarousel, double>(nameof(ImagesSize), 1.0);


        /// <summary>
        /// Gets or sets the image size. It can be set in range 0.5 : 1.5 (50 % smaller or 50% bigger in relation to original image size)
        /// Default value for image size is 1.0, this is original image width and height
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Bildgröße. Einstellbar im Bereich 0,5 : 1,5 (50 % kleiner oder 50 % größer im Verhältnis zur Originalbildgröße)
        /// Standardwert für die Bildgröße ist 1.0, das ist die Breite und Höhe des Originalbildes.
        /// </summary>
        [Category("VBControl")]
        public double ImagesSize
        {
            get { return GetValue(ImagesSizeProperty); }
            set { SetValue(ImagesSizeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for CarouselWidth.
        /// </summary>
        public static readonly StyledProperty<double> CarouselWidthProperty
            = AvaloniaProperty.Register<VBCarousel, double>(nameof(CarouselWidth));


        /// <summary>
        /// The Carousel control width, min width = 400
        /// </summary>
        /// <summary xml:lang="de">
        /// Die Karussell-Steuerungsbreite, min. Breite = 400
        /// </summary>
        [Category("VBControl")]
        public double CarouselWidth
        {
            get { return GetValue(CarouselWidthProperty); }
            set { SetValue(CarouselWidthProperty, value); }
        }

        
        /// <summary>
        /// Represents the dependency property for CarouselHeight.
        /// </summary>
        public static readonly StyledProperty<double> CarouselHeightProperty
            = AvaloniaProperty.Register<VBCarousel, double>(nameof(CarouselHeight));


        /// <summary>
        /// The Carousel control height, min height = 400
        /// </summary>
        /// <summary>
        /// Die Karussell-Steuerungshöhe, min. Höhe = 400
        /// </summary>
        [Category("VBControl")]
        public double CarouselHeight
        {
            get { return GetValue(CarouselHeightProperty); }
            set { SetValue(CarouselHeightProperty, value); }
        }


        #endregion

        #region IVBContent Members

        IACType _ACTypeInfo = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _ACTypeInfo as ACClassProperty;
            }
        }

        /// <summary>
        /// Represents the dependency property for RightControlMode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> RightControlModeProperty = 
            AvaloniaProperty.Register<VBCarousel, Global.ControlModes>(nameof(RightControlMode));
        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Global.ControlModes RightControlMode
        {
            get { return GetValue(RightControlModeProperty); }
            set { SetValue(RightControlModeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty
            = AvaloniaProperty.Register<VBCarousel, string>(nameof(DisabledModes));
        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get { return GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentPropery = 
            AvaloniaProperty.Register<VBCarousel, string>(nameof(VBContent));

        /// <summary>
        /// Represents the property in which you enter the name of BSO's selected property marked with [ACPropertySelected(...)] attribute.
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent
        {
            get { return GetValue(VBContentPropery); }
            set { SetValue(VBContentPropery, value); }
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly StyledProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBCarousel>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBCarousel, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBCarousel, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == ACCompInitStateProperty)
                InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
                }
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        /// <summary>
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBCarousel, string>("ACCaption");
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get { return GetValue(ACCaptionProperty); }
            set { SetValue(ACCaptionProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionTransProperty
            = AvaloniaProperty.Register<VBCarousel, string>(nameof(ACCaptionTrans));

        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaptionTrans
        {
            get { return GetValue(ACCaptionTransProperty); }
            set { SetValue(ACCaptionTransProperty, value); }
        }

        #endregion

        #region IVBSource Members

        /// <summary>
        /// Represents the property in which you enter the name of BSO's list property of type ACClassDesign marked with attribute [ACPropertyList(...)]. 
        /// It's a items source property.
        /// </summary>
        /// <summary xml:lang="de">
        /// Stellt die Eigenschaft dar, in der Sie den Namen der BSO-Listeneigenschaft vom Typ ACClassDesign mit dem Attribut [ACPropertyList(....)] eingeben. 
        /// Es ist eine Quellen-Eigenschaft.
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                _DataSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the VBShow columns.
        /// </summary>
        public string VBShowColumns
        {
            get
            {
                return _DataShowColumns;
            }
            set
            {
                _DataShowColumns = value;
            }
        }

        /// <summary>
        /// Gets or sets the VBDisabled columns.
        /// </summary>
        public string VBDisabledColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the VBChilds property;
        /// </summary>
        public string VBChilds
        {
            get
            {
                return _DataChilds;
            }
            set
            {
                _DataChilds = value;
            }
        }

        #endregion

        #region IDataContent Members

        List<IACObject> _ACContentList = new List<IACObject>();
        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return _ACContentList;
            }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                    if (query.Any())
                    {
                        ACCommand acCommand = query.First() as ACCommand;
                        ACUrlCommand(acCommand.GetACUrl(), null);
                    }
                    break;
            }
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    {
                        var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                        if (query.Any())
                        {
                            ACCommand acCommand = query.First() as ACCommand;
                            return this.ReflectIsEnabledACUrlCommand(acCommand.GetACUrl(), null);
                        }
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Updates a control mode.
        /// </summary>
        public void UpdateControlMode()
        {
            IACComponent elementACComponent = ContextACObject as IACComponent;
            if (elementACComponent == null)
                return;
            Global.ControlModesInfo controlModeInfo = elementACComponent.GetControlModes(this);
            Global.ControlModes controlMode = controlModeInfo.Mode;
            if (controlMode != ControlMode)
                ControlMode = controlMode;

            if (controlMode >= Global.ControlModes.Enabled)
                this.IsTabStop = true;
            else
                this.IsTabStop = false;

            if (controlMode == Global.ControlModes.Collapsed)
            {
                if (this.IsVisible)
                    this.IsVisible = false;
            }
            else if (controlMode == Global.ControlModes.Hidden)
            {
                if (this.IsVisible)
                    this.IsVisible = false;
            }
            else
            {
                if (!this.IsVisible)
                    this.IsVisible = true;
                if (controlMode == Global.ControlModes.Disabled)
                {
                    if (IsEnabled)
                        IsEnabled = false;
                }
                else
                {
                    if (!IsEnabled)
                        IsEnabled = true;
                }
            }
        }

        #endregion

        #region IACObject

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        #endregion

        #region IACObject Member

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="acUrl"></param>
        /// <param name="acTypeInfo"></param>
        /// <param name="source"></param>
        /// <param name="path"></param>
        /// <param name="rightControlMode"></param>
        /// <returns></returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }
        #endregion

        /// <summary>
        /// Not implemented.
        /// </summary>
        public ACQueryDefinition ACQueryDefinition
        {
            get { throw new NotImplementedException(); }
        }
    }
}
