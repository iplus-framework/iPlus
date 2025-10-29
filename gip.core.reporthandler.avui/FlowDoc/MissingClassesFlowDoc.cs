// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media.TextFormatting;
using AvRichTextBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace gip.core.reporthandler.avui.Flowdoc
{
    public class Figure : Block // Not inhertiable from Inline 
    {
        public Figure()
        {
        }

        public BlockCollection Blocks         
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //internal override void AppendText(StringBuilder stringBuilder)
        //{
        //    throw new NotImplementedException();
        //}

        //internal override void BuildTextRun(IList<TextRun> textRuns, Size blockSize)
        //{
        //    throw new NotImplementedException();
        //}
    }

    //public class InlineCollection : List<Inline>
    //{
    //    public Inline FirstInline { get { return this.FirstOrDefault(); } }
    //}


    public static class FlowDocExt
    {
        public static Inline NextInline(this Inline block)
        {
            return null;
        }
    }

    public class BlockUIContainer : Block
    {
        public BlockUIContainer()
        {
        }

        public BlockUIContainer Child
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class Section : TextElement
    {
        public Section()
        {
        }

        public BlockCollection Blocks
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class Table : Block
    {
        public Table()
        {
        }

        public IEnumerable<TableRowGroup> RowGroups
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IList<TableColumn> Columns
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class TableRowGroup : TextElement
    {
        public TableRowGroup()
        {
        }

        public IList<TableRow> Rows
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class TableRow : TextElement
    {
        public TableRow()
        {
        }

        public IList<TableCell> Cells
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class TableColumn : TextElement
    {
        public TableColumn()
        {
        }

        public IEnumerable<TableCell> Cells
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class TableCell : TextElement
    {
        public TableCell()
        {
        }

        public BlockCollection Blocks
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public interface IDocumentPaginatorSource
    {
    }

    public class DocumentPage
    {
    }

    public class XpsDocument
    {
    }

    public class FixedPage
    {
    }

    public abstract class DocumentPaginator
    {
    }

    public class ContainerVisual : Block
    {
    }

    public class PrintTicket
    {
    }

    public class BlockCollection : List<Block>
    {
    }
}
