// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.autocomponent
{
    static public class LayoutHelper
    {
        #region DockingManager
        static public string DockingManagerBegin(string name, string attributes)
        {
            return "<vb:VBDockingManager x:Name=\"" + name + "\" " + attributes + ">";
        }

        static public string DockingManagerEnd()
        {
            return "</vb:VBDockingManager>";
        }

        static public string DockingManagerAdd(string vbContent, string name, string acCaption = null)
        {
            // * nicht vergessen
            string xaml = "<vb:VBDesign VBContent=\"";
            xaml += vbContent;
            xaml += "\" vb:VBDockingManager.IsCloseableBSORoot=\"False\" vb:VBDockingManager.Container=\"TabItem\" vb:VBDockingManager.DockState=\"Tabbed\" vb:VBDockingManager.DockPosition=\"Bottom\" vb:VBDockingManager.RibbonBarVisibility=\"Hidden\" vb:VBDockingManager.WindowSize=\"0,0\" Name=\"";
            xaml += name + "\"";
            if (!string.IsNullOrEmpty(acCaption))
            {
                xaml += " ACCaption=\"" + acCaption + "\"";
            }
            xaml += "/>";
            return xaml;
        }
        #endregion

        #region VBGrid
        static public string VBGridBegin(string name, string columns ="*,*", string rows="30,30", string dock = "Top")
        {
            string xaml = "<vb:VBGrid";
            
            if (!string.IsNullOrEmpty(dock))
            {
                xaml += " DockPanel.Dock=\"" + dock + "\"";
            }
            xaml += ">\n";

            string[] columnInfo = columns.Split(',');
            int colCount = columnInfo.Count();
            if (columnInfo.Any())
            {
                xaml += "        <Grid.ColumnDefinitions>\n";
                foreach (var col in columnInfo)
                {
                    if (col == "*")
                    {
                        xaml += "            <ColumnDefinition></ColumnDefinition>\n";
                    }
                    else
                    {
                        xaml += string.Format("<ColumnDefinition Width=\"{0}\"></ColumnDefinition>\n", col);
                    }
                }
                xaml += "        </Grid.ColumnDefinitions>\n";
            }
            else
            {
                colCount = 1;
            }
            string[] rowInfo = rows.Split(',');
            int rowCount = rowInfo.Count();
            if (rowInfo.Any())
            {
                xaml += "        <Grid.RowDefinitions>\n";

                foreach (var row in rowInfo)
                {
                    if (row == "*")
                    {
                        xaml += "            <RowDefinition></RowDefinition>\n";
                    }
                    else
                    {
                        xaml += string.Format("<RowDefinition Height=\"{0}\"></RowDefinition>\n", row);
                    }
                }

                xaml += "        </Grid.RowDefinitions>\n";
            }
            else
            {
                rowCount = 1;
            }
            xaml += string.Format("<vb:VBFrame Grid.ColumnSpan=\"{0}\" Grid.RowSpan=\"{1}\"></vb:VBFrame>\n", colCount, rowCount);
            return xaml;
        }

        static public string VBGridEnd()
        {
            return "    </vb:VBGrid>\n";
        }

        static public string VBGridAdd(string control)
        {
            return control;
        }
        #endregion

        #region VBDockPanel
        static public string VBDockPanelEmpty()
        {
            return "<vb:VBDockPanel></vb:VBDockPanel>";
        }

        static public string VBDockPanelBegin()
        {
            return "<vb:VBDockPanel>\n";
        }

        static public string VBDockPanelEnd()
        {
            return "</vb:VBDockPanel>\n"; 
        }
        #endregion
    }
}
