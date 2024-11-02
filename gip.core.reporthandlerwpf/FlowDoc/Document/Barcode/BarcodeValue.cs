// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Windows;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    public class BarcodeValue : InlineDocumentValue
    {

        #region Properties

        public virtual string AI
        {
            get { return (string)GetValue(AIProperty); }
            set { SetValue(AIProperty, value); }
        }
        public static readonly DependencyProperty AIProperty = ReportDocument.StringFormatProperty.AddOwner(typeof(BarcodeValue));


        #endregion

    }
}
