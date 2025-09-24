using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using System;

namespace gip.core.layoutengine.avui
{
    public class GridUtils
    {
        #region RowDefinitions attached property

        /// <summary>
        /// Identified the RowDefinitions attached property
        /// </summary>
        public static readonly AttachedProperty<string> RowDefinitionsProperty =
            AvaloniaProperty.RegisterAttached<GridUtils, Control, string>("RowDefinitions", "");

        /// <summary>
        /// Gets the value of the RowDefinitions property
        /// </summary>
        public static string GetRowDefinitions(AvaloniaObject d)
        {
            return d.GetValue(RowDefinitionsProperty);
        }

        /// <summary>
        /// Sets the value of the RowDefinitions property
        /// </summary>
        public static void SetRowDefinitions(AvaloniaObject d, string value)
        {
            d.SetValue(RowDefinitionsProperty, value);
        }

        static GridUtils()
        {
            RowDefinitionsProperty.Changed.Subscribe(OnRowDefinitionsPropertyChanged);
            ColumnDefinitionsProperty.Changed.Subscribe(OnColumnDefinitionsPropertyChanged);
        }

        /// <summary>
        /// Handles property changed event for the RowDefinitions property, constructing
        /// the required RowDefinitions elements on the grid which this property is attached to.
        /// </summary>
        private static void OnRowDefinitionsPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is Grid targetGrid)
            {
                // construct the required row definitions
                targetGrid.RowDefinitions.Clear();
                string rowDefs = e.NewValue as string;
                if (!string.IsNullOrEmpty(rowDefs))
                {
                    var rowDefArray = rowDefs.Split(',');
                    foreach (string rowDefinition in rowDefArray)
                    {
                        if (rowDefinition.Trim() == "")
                        {
                            targetGrid.RowDefinitions.Add(new RowDefinition());
                        }
                        else
                        {
                            targetGrid.RowDefinitions.Add(new RowDefinition()
                            {
                                Height = ParseLength(rowDefinition)
                            });
                        }
                    }
                }
            }
        }

        #endregion


        #region ColumnDefinitions attached property

        /// <summary>
        /// Identifies the ColumnDefinitions attached property
        /// </summary>
        public static readonly AttachedProperty<string> ColumnDefinitionsProperty =
            AvaloniaProperty.RegisterAttached<GridUtils, Control, string>("ColumnDefinitions", "");

        /// <summary>
        /// Gets the value of the ColumnDefinitions property
        /// </summary>
        public static string GetColumnDefinitions(AvaloniaObject d)
        {
            return d.GetValue(ColumnDefinitionsProperty);
        }

        /// <summary>
        /// Sets the value of the ColumnDefinitions property
        /// </summary>
        public static void SetColumnDefinitions(AvaloniaObject d, string value)
        {
            d.SetValue(ColumnDefinitionsProperty, value);
        }

        /// <summary>
        /// Handles property changed event for the ColumnDefinitions property, constructing
        /// the required ColumnDefinitions elements on the grid which this property is attached to.
        /// </summary>
        private static void OnColumnDefinitionsPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is Grid targetGrid)
            {
                // construct the required column definitions
                targetGrid.ColumnDefinitions.Clear();
                string columnDefs = e.NewValue as string;
                if (!string.IsNullOrEmpty(columnDefs))
                {
                    var columnDefArray = columnDefs.Split(',');
                    foreach (string columnDefinition in columnDefArray)
                    {
                        if (columnDefinition.Trim() == "")
                        {
                            targetGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        }
                        else
                        {
                            targetGrid.ColumnDefinitions.Add(new ColumnDefinition()
                            {
                                Width = ParseLength(columnDefinition)
                            });
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Parses a string to create a GridLength
        /// </summary>
        private static GridLength ParseLength(string length)
        {
            length = length.Trim();

            if (length.ToLowerInvariant().Equals("auto"))
            {
                return new GridLength(0, GridUnitType.Auto);
            }
            else if (length.Contains("*"))
            {
                length = length.Replace("*", "");
                if (string.IsNullOrEmpty(length)) length = "1";
                return new GridLength(double.Parse(length), GridUnitType.Star);
            }

            return new GridLength(double.Parse(length), GridUnitType.Pixel);
        }
    }
}
