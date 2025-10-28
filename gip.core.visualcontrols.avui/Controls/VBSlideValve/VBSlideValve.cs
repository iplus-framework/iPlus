// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Avalonia;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Metadata;

namespace gip.core.visualcontrols.avui
{
    [TemplatePart(Name = "PART_TickBar", Type = typeof(Control))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBSlideValve'}de{'VBSlideValve'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBSlideValve : TemplatedControl
    {
        #region c'tors
        public VBSlideValve()
        {
        }
        #endregion

        #region Additional Styled Properties

        public static readonly StyledProperty<bool> IsOpenProperty
            = AvaloniaProperty.Register<VBSlideValve, bool>(nameof(IsOpen));
        /// <summary>
        /// Füllstandsstriche
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool IsOpen
        {
            get { return GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        #endregion

    }
}
