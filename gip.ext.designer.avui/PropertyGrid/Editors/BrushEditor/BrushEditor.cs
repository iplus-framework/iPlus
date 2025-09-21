// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.ext.design.avui.PropertyGrid;
using System.Reflection;
using gip.ext.design.avui;
using Avalonia.Media;
using Avalonia;
using gip.ext.design.avui.UIExtensions;

namespace gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor
{
    public class BrushEditor : INotifyPropertyChanged
    {
        public BrushEditor()
        {
            ResetBrushes();
        }

        private void ResetBrushes()
        {
            GradientStops stops = new GradientStops();
            stops.Add(new GradientStop(Colors.Black, 0));
            stops.Add(new GradientStop(Colors.White, 1));

            linearGradientBrush = new LinearGradientBrush();
            linearGradientBrush.GradientStops = stops;
            linearGradientBrush.EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative);
            radialGradientBrush = new RadialGradientBrush();
            radialGradientBrush.GradientStops = stops;
        }

        public static BrushItem[] SystemBrushes = typeof(Colors)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(SolidColorBrush))
            .Select(p => new BrushItem() { Name = p.Name, Brush = (Brush)p.GetValue(null, null) })
            .ToArray();

        public static BrushItem[] SystemColors = typeof(Colors)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(Color))
            .Select(p => new BrushItem()
            {
                Name = p.Name,
                Brush = new SolidColorBrush((Color)p.GetValue(null, null))
            })
            .ToArray();

        SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.White);
        LinearGradientBrush linearGradientBrush;
        RadialGradientBrush radialGradientBrush;

        IPropertyNode property;

        public IPropertyNode Property
        {
            get
            {
                return property;
            }
            set
            {
                property = value;
                if (property != null)
                {
                    var f = property.Value as IImmutableBrush;
                    if (f != null) 
                        property.Value = f.Clone();
                }
                DetermineCurrentKind();
                RaisePropertyChanged("Property");
                RaisePropertyChanged("Brush");
            }
        }

        public IBrush Brush
        {
            get
            {
                if (property != null)
                {
                    return property.Value as IBrush;
                }
                return null;
            }
            set
            {
                if (property != null && property.Value != value)
                {
                    if (value == null)
                    {
                        if (property.CanReset)
                            property.Reset();
                    }
                    else
                        property.Value = value;
                    DetermineCurrentKind();
                    RaisePropertyChanged("Brush");
                }
            }
        }

        public Color Color
        {
            get { return Brush is SolidColorBrush ? ((SolidColorBrush)Brush).Color : Colors.Black; }

            set
            {
                if (Brush is SolidColorBrush && !(Brush is IImmutableBrush))
                {
                    ((SolidColorBrush)Brush).Color = value;
                }
                else
                {
                    Brush = new SolidColorBrush(value);
                }
            }
        }

        void DetermineCurrentKind()
        {
            if (Brush == null)
            {
                currentKind = BrushEditorKind.None;
            }
            else if (Brush is SolidColorBrush)
            {
                solidColorBrush = Brush as SolidColorBrush;
                currentKind = BrushEditorKind.Solid;
            }
            else if (Brush is LinearGradientBrush)
            {
                linearGradientBrush = Brush as LinearGradientBrush;
                radialGradientBrush.GradientStops = linearGradientBrush.GradientStops;
                currentKind = BrushEditorKind.Linear;
            }
            else if (Brush is RadialGradientBrush)
            {
                radialGradientBrush = Brush as RadialGradientBrush;
                linearGradientBrush.GradientStops = linearGradientBrush.GradientStops;
                currentKind = BrushEditorKind.Radial;
            }
        }

        BrushEditorKind currentKind;

        public BrushEditorKind CurrentKind
        {
            get
            {
                return currentKind;
            }
            set
            {
                currentKind = value;
                RaisePropertyChanged("CurrentKind");

                switch (CurrentKind)
                {
                    case BrushEditorKind.None:
                        Brush = null;
                        break;

                    case BrushEditorKind.Solid:
                        Brush = solidColorBrush;
                        break;

                    case BrushEditorKind.Linear:
                        Brush = linearGradientBrush;
                        break;

                    case BrushEditorKind.Radial:
                        Brush = radialGradientBrush;
                        break;

                    case BrushEditorKind.List:
                        Brush = solidColorBrush;
                        break;
                }
            }
        }

        public double GradientAngle
        {
            get
            {
                var x = linearGradientBrush.EndPoint.Point.X - linearGradientBrush.StartPoint.Point.X;
                var y = linearGradientBrush.EndPoint.Point.Y - linearGradientBrush.StartPoint.Point.Y;
                return VectorExtensions.AngleBetween(new Vector(1, 0), new Vector(x, -y));
            }
            set
            {
                var d = value * Math.PI / 180;
                var p = new Point(Math.Cos(d), -Math.Sin(d));
                var k = 1 / Math.Max(Math.Abs(p.X), Math.Abs(p.Y));
                double x = p.X * k;
                double y = p.Y * k;
                var p2 = new Point(-x, -y);
                linearGradientBrush.StartPoint = new RelativePoint((p2.X + 1) / 2, (p2.Y + 1) / 2, RelativeUnit.Relative);
                linearGradientBrush.EndPoint = new RelativePoint((x + 1) / 2, (y + 1) / 2, RelativeUnit.Relative);
                RaisePropertyChanged("GradientAngle");
            }
        }

        public IEnumerable<BrushItem> AvailableColors
        {
            get { return SystemColors; }
        }

        public IEnumerable<BrushItem> AvailableBrushes
        {
            get { return SystemBrushes; }
        }

        public void MakeGradientHorizontal()
        {
            GradientAngle = 0;
        }

        public void MakeGradientVertical()
        {
            GradientAngle = -90;
        }

        public void Commit()
        {
            if (Brush != null)
            {
                Property.Value = Brush.Clone();
                if (Property.ValueItem != null)
                {
                    using (ChangeGroup changeGroup = Property.ValueItem.OpenGroup("Brush Properties"))
                    {
                        // Brush Member
                        Property.ValueItem.Properties["Opacity"].SetValue(Brush.Opacity);
                        //if (Brush.RelativeTransform != null)
                        //    Property.ValueItem.Properties["RelativeTransform"].SetValue(Brush.RelativeTransform);
                        if (Brush.Transform != null)
                            Property.ValueItem.Properties["Transform"].SetValue(Brush.Transform);
                        if (CurrentKind == BrushEditorKind.Solid)
                        {
                            Property.ValueItem.Properties["Color"].SetValue((Brush as SolidColorBrush).Color);
                        }
                        if ((CurrentKind == BrushEditorKind.Linear) || (CurrentKind == BrushEditorKind.Radial))
                        {
                            GradientBrush gradientBrush = (GradientBrush)Brush;
                            //Property.ValueItem.Properties["ColorInterpolationMode"].SetValue(gradientBrush.ColorInterpolationMode);
                            //Property.ValueItem.Properties["MappingMode"].SetValue(gradientBrush.MappingMode);
                            Property.ValueItem.Properties["SpreadMethod"].SetValue(gradientBrush.SpreadMethod);
                            if (Property.ValueItem.ContentProperty.CollectionElements.Count > 0)
                            {
                                int i = Property.ValueItem.ContentProperty.CollectionElements.Count;
                                while (i > 0)
                                {
                                    Property.ValueItem.ContentProperty.CollectionElements.RemoveAt(0);
                                    i--;
                                }
                            }

                            foreach (GradientStop gradientStop in gradientBrush.GradientStops.ToList())
                            {
                                DesignItem newGradientStopItem = Property.Services.Component.GetDesignItem(gradientStop);
                                if (newGradientStopItem == null)
                                    newGradientStopItem = Property.Services.Component.RegisterComponentForDesigner(gradientStop);
                                newGradientStopItem.Properties["Color"].SetValue(gradientStop.Color);
                                newGradientStopItem.Properties["Offset"].SetValue(gradientStop.Offset);
                                Property.ValueItem.ContentProperty.CollectionElements.Add(newGradientStopItem);
                            }

                            if (CurrentKind == BrushEditorKind.Linear)
                            {
                                LinearGradientBrush lBrush = (LinearGradientBrush)Brush;
                                Property.ValueItem.Properties["StartPoint"].SetValue(lBrush.StartPoint);
                                Property.ValueItem.Properties["EndPoint"].SetValue(lBrush.EndPoint);
                            }
                            else //if (CurrentKind == BrushEditorKind.Radial)
                            {
                                RadialGradientBrush rBrush = (RadialGradientBrush)Brush;
                                Property.ValueItem.Properties["Center"].SetValue(rBrush.Center);
                                Property.ValueItem.Properties["GradientOrigin"].SetValue(rBrush.GradientOrigin);
                                Property.ValueItem.Properties["RadiusX"].SetValue(rBrush.RadiusX);
                                Property.ValueItem.Properties["RadiusY"].SetValue(rBrush.RadiusY);
                            }
                        }
                        changeGroup.Commit();
                    }
                    ResetBrushes();
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }

    public enum BrushEditorKind
    {
        None,
        Solid,
        Linear,
        Radial,
        List
    }

    public class BrushItem
    {
        public string Name { get; set; }
        public Brush Brush { get; set; }
    }
}
