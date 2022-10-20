// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using gip.ext.design.Adorners;
using gip.ext.design.Extensions;
using gip.ext.designer.Controls;
namespace gip.ext.designer.Extensions
{
	/// <summary>
	/// Display Height/Width on the primary selection
	/// </summary>
    [ExtensionFor(typeof(UIElement))]
    public class SizeDisplayExtension : PrimarySelectionAdornerProvider
    {
        HeightDisplay _heightDisplay;
        WidthDisplay _widthDisplay;
        
        public HeightDisplay HeightDisplay{
        	get { return _heightDisplay; }
        }
        
        public WidthDisplay WidthDisplay{
        	get { return _widthDisplay; }
        }
		         
        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (this.ExtendedItem != null)
            {
                RelativePlacement placementHeight = new RelativePlacement(HorizontalAlignment.Right, VerticalAlignment.Stretch);
                placementHeight.XOffset = 10;
                _heightDisplay = new HeightDisplay();
                _heightDisplay.DataContext = this.ExtendedItem.Component;

                RelativePlacement placementWidth = new RelativePlacement(HorizontalAlignment.Stretch, VerticalAlignment.Bottom);
                placementWidth.YOffset = 10;
                _widthDisplay = new WidthDisplay();
                _widthDisplay.DataContext = this.ExtendedItem.Component;

                this.AddAdorners(placementHeight, _heightDisplay);
                this.AddAdorners(placementWidth, _widthDisplay); 
                _heightDisplay.Visibility=Visibility.Hidden;
                _widthDisplay.Visibility=Visibility.Hidden;
            }
        }   
    }
}
