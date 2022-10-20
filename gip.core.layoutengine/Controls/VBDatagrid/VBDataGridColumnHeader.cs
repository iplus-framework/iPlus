using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using gip.core.datamodel;


namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents an individual <see cref="VBDataGrid"/> column header.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt einen individuellen <see cref="VBDataGrid"/>-Spaltenkopf dar.
    /// </summary>
    [TemplatePart(Name = "PART_TextBlockSum", Type = typeof(TextBlock))]
    public class VBDataGridColumnHeader : DataGridColumnHeader
    {
        public VBDataGridColumnHeader()
        {
        }

        private TextBlock _SumTextBlock;
        //private bool IsEventHandlerSet = false;

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            object partObj = GetTemplateChild("PART_TextBlockSum");
            if (partObj != null && partObj is TextBlock)
                _SumTextBlock = partObj as TextBlock;
            AddBinding();
        }

        /// <summary>
        /// Bind textpropety to dictionary
        /// </summary>
        void AddBinding()
        {
            IGriColumn column = Column as IGriColumn;
            Binding bind = null;
            if (column != null && column.VBDataGrid.IsSumEnabled)
            {
                if (column.VBContent != null)
                {
                    bind = new Binding("DictionarySumProperties[" + column.VBContent + "]");
                }
                else if (Column.Header != null && column.VBDataGrid.VBSumColumns != null && column.VBDataGrid.VBSumColumns.Contains(Column.Header.ToString()))
                {
                    bind = new Binding("DictionarySumProperties[" + Column.Header.ToString() + "]");
                }
            }

            if(bind != null)
            {
                bind.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture;
                bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                bind.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(VBDataGrid), 1);
                _SumTextBlock.SetBinding(TextBlock.TextProperty, bind);
            }
           
        }
    }
}
