using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gip.core.reporthandler.Flowdoc
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

        // Using a DependencyProperty as the backing store for ParameterNameIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParameterNameIndexProperty =
            DependencyProperty.Register("ParameterNameIndex", typeof(int), typeof(InlineACMethodValue));

        
    }
}
