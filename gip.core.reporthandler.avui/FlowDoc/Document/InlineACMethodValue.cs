// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Represent the control for ACMethod value in reports.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellen Sie das Steuerelement für den Wert der AC-Methode in Berichten dar.
    /// </summary>
    public class InlineACMethodValue : InlinePropertyValueBase
    {
        public InlineACMethodValue()
        {

        }

       
        public int ParameterNameIndex
        {
            get { return (int)GetValue(ParameterNameIndexProperty); }
            set { SetValue(ParameterNameIndexProperty, value); }
        }
        public static readonly StyledProperty<int> ParameterNameIndexProperty = AvaloniaProperty.Register<InlineACMethodValue, int>(nameof(ParameterNameIndex));
        
    }
}
