using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;


namespace gip.core.layoutengine.avui
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
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            object partObj = e.NameScope.Find("PART_TextBlockSum");
            if (partObj != null && partObj is TextBlock)
                _SumTextBlock = partObj as TextBlock;
            AddBinding();
        }

        /// <summary>
        /// Bind textpropety to dictionary
        /// </summary>
        void AddBinding()
        {
            DataGridColumn dColumn = this.GetOwningColumnViaReflection() as DataGridColumn;
            IGriColumn column = this.GetOwningColumnViaReflection() as IGriColumn;
            Binding bind = null;
            if (column != null && column.VBDataGrid.IsSumEnabled)
            {
                if (column.VBContent != null)
                {
                    bind = new Binding("DictionarySumProperties[" + column.VBContent + "]");
                }
                else if (dColumn.Header != null && column.VBDataGrid.VBSumColumns != null && column.VBDataGrid.VBSumColumns.Contains(dColumn.Header.ToString()))
                {
                    bind = new Binding("DictionarySumProperties[" + dColumn.Header.ToString() + "]");
                }
            }

            if (bind != null)
            {
                bind.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture;
                bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                bind.RelativeSource = new RelativeSource() { AncestorType = typeof(VBDataGrid), AncestorLevel = 1 };
                _SumTextBlock.Bind(TextBlock.TextProperty, bind);
            }
           
        }
    }
}
