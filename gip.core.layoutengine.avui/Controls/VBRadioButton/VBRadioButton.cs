using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a button that can be selected with click on it, but deselected only by clicked on another <see cref="VBRadioButton"/>.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine Schaltfläche dar, die durch Anklicken ausgewählt werden kann, aber nur durch Anklicken eines anderen <see cref="VBRadioButton"/> deaktiviert wird.
    /// </summary>
    public class VBRadioButton : RadioButton, IVBDynamicIcon
    {
        #region c'tors
        public VBRadioButton() : base()
        {
        }

        #endregion

        #region Init

        protected override void OnInitialized()
        {
            base.OnInitialized();
            InitVBControl();
        }

        /// <summary>
        /// Overrides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        bool _isInitialized = false;

        private void InitVBControl()
        {
            if (_isInitialized)
                return;

            if (!string.IsNullOrEmpty(ACaption))
                this.Content = Translator.GetTranslation(null, ACCaption, this.Root().Environment.VBLanguageCode);

            UpdatePushButtonClass();
            _isInitialized = true;
        }

        #endregion

        #region Styled Properties (converted from Dependency Properties)

        /// <summary>
        /// Represents the styled property for ContentStroke.
        /// </summary>
        public static readonly StyledProperty<IBrush> ContentStrokeProperty
            = AvaloniaProperty.Register<VBRadioButton, IBrush>(nameof(ContentStroke));

        /// <summary>
        /// Gets or sets the stroke of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Strich des Inhalts.
        /// </summary>
        [Category("VBControl")]
        public IBrush ContentStroke
        {
            get { return GetValue(ContentStrokeProperty); }
            set { SetValue(ContentStrokeProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for ContentFill.
        /// </summary>
        public static readonly StyledProperty<IBrush> ContentFillProperty
            = AvaloniaProperty.Register<VBRadioButton, IBrush>(nameof(ContentFill));

        /// <summary>
        /// Gets or sets the fill of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Füllung des Inhalts.
        /// </summary>
        [Category("VBControl")]
        public IBrush ContentFill
        {
            get { return GetValue(ContentFillProperty); }
            set { SetValue(ContentFillProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for PushButtonStyle.
        /// </summary>
        public static readonly StyledProperty<bool> PushButtonStyleProperty
            = AvaloniaProperty.Register<VBRadioButton, bool>(nameof(PushButtonStyle), false, false, false, OnPushButtonStyleChanged);

        [Category("VBControl")]
        public bool PushButtonStyle
        {
            get { return GetValue(PushButtonStyleProperty); }
            set { SetValue(PushButtonStyleProperty, value); }
        }

        private static void OnPushButtonStyleChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is VBRadioButton radioButton)
            {
                radioButton.UpdatePushButtonClass();
            }
        }

        private void UpdatePushButtonClass()
        {
            if (Classes == null) return;

            if (PushButtonStyle)
            {
                Classes.Add("pushbutton");
            }
            else
            {
                Classes.Remove("pushbutton");
            }
        }

        /// <summary>
        /// Represents the styled property for IsMouseOverParent.
        /// </summary>
        public static readonly StyledProperty<bool> IsMouseOverParentProperty
            = AvaloniaProperty.Register<VBRadioButton, bool>(nameof(IsMouseOverParent), false, false, false, OnIsMouseOverParentChanged);

        public bool IsMouseOverParent
        {
            get { return GetValue(IsMouseOverParentProperty); }
            set { SetValue(IsMouseOverParentProperty, value); }
        }

        private static void OnIsMouseOverParentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is VBRadioButton radioButton)
            {
                radioButton.UpdateMouseOverParentClass();
            }
        }

        private void UpdateMouseOverParentClass()
        {
            if (Classes == null) return;

            if (IsMouseOverParent)
            {
                Classes.Add("mouseover-parent");
            }
            else
            {
                Classes.Remove("mouseover-parent");
            }
        }

        #endregion

        private string _Caption;
        /// <summary>
        /// Gets or sets the ACCaption.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption.
        /// </summary>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return _Caption;
            }
            set
            {
                _Caption = value;
                // Update content when caption changes
                if (_isInitialized && !string.IsNullOrEmpty(_Caption))
                {
                    this.Content = Translator.GetTranslation(null, _Caption, this.Root().Environment.VBLanguageCode);
                }
            }
        }
    }
}
