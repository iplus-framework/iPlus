// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Avalonia;
using Avalonia.Collections;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.autocomponent;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.visualcontrols.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFillLevel'}de{'VBFillLevel'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBFillLevel : VBProgressBar
    {
        #region c'tors
        static VBFillLevel()
        {
            VBProgressBar.ProgressBarStyleProperty.OverrideMetadata(typeof(VBFillLevel), new StyledPropertyMetadata<ProgressBarStyles>(ProgressBarStyles.PerformantBar));
        }
        #endregion

        #region Additional Styled Properties

        public static readonly StyledProperty<double> TickFrequencyProperty
            = AvaloniaProperty.Register<VBFillLevel, double>(nameof(TickFrequency), 10.0);
        /// <summary>
        /// Der Abstand zwischen Teilstrichen.Der Standardwert ist (1.0).
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double TickFrequency
        {
            get { return GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }


        public static readonly StyledProperty<bool> ShowTickBarProperty
            = AvaloniaProperty.Register<VBFillLevel, bool>(nameof(ShowTickBar));
        /// <summary>
        /// Füllstandsstriche
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ShowTickBar
        {
            get { return GetValue(ShowTickBarProperty); }
            set { SetValue(ShowTickBarProperty, value); }
        }

        public static readonly StyledProperty<IEnumerable<double>> TicksCollectionProperty
            = AvaloniaProperty.Register<VBFillLevel, IEnumerable<double>>(nameof(TicksCollection));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public IEnumerable<double> TicksCollection
        {
            get { return GetValue(TicksCollectionProperty); }
            set { SetValue(TicksCollectionProperty, value); }
        }

        public static readonly StyledProperty<AvaloniaList<double>> TicksViewProperty
            = AvaloniaProperty.Register<VBFillLevel, AvaloniaList<double>>(nameof(TicksView));
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public AvaloniaList<double> TicksView
        {
            get { return GetValue(TicksViewProperty); }
            set { SetValue(TicksViewProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == TicksCollectionProperty)
            {
                if (TicksCollection == null)
                    TicksView = null;
                else
                    TicksView = new AvaloniaList<double>(TicksCollection);
            }
        }
        #endregion

    }
}
