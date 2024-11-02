// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace gip.core.layoutengine
{
    public enum SynchronizationModes
    {
        None,
        SourceToTarget,
        TargetToSource
    }

    public static class Synchronization
    {

        #region Initial synchronization mode

        public static readonly DependencyProperty InitialSynchronizationModeProperty = DependencyProperty.RegisterAttached("InitialSynchronizationMode", typeof(SynchronizationModes), typeof(Synchronization), new FrameworkPropertyMetadata(SynchronizationModes.None));

        public static void SetInitialSynchronizationMode( UIElement element, SynchronizationModes value)
        {
            element.SetValue(InitialSynchronizationModeProperty, value);
        }
        
        public static SynchronizationModes GetInitialSynchronizationMode(UIElement element)
        {
            return (SynchronizationModes)element.GetValue(InitialSynchronizationModeProperty);
        }

        #endregion 


        #region MultiSelect

        public static readonly DependencyProperty MultiSelectProperty = DependencyProperty.RegisterAttached("MultiSelect", typeof(bool), typeof(Synchronization), new FrameworkPropertyMetadata(true));

        public static void SetMultiSelect(UIElement element, bool value)
        {
            element.SetValue(MultiSelectProperty, value);
        }

        public static bool GetMultiSelect(UIElement element)
        {
            return (bool)element.GetValue(MultiSelectProperty);
        }

        #endregion 


        #region Item converter

        public static readonly DependencyProperty ItemConverterProperty = DependencyProperty.RegisterAttached("ItemConverter", typeof(IValueConverter), typeof(Synchronization), new FrameworkPropertyMetadata(null));

        public static void SetItemConverter(UIElement element, IValueConverter value)
        {
            element.SetValue(ItemConverterProperty, value);
        }

        public static IValueConverter GetItemConverter(UIElement element)
        {
            return (IValueConverter)element.GetValue(ItemConverterProperty);
        }

        #endregion


        #region Synchronizer

        private static readonly DependencyProperty SynchronizerProperty = DependencyProperty.RegisterAttached("Synchronizer", typeof(ItemsSynchronizer), typeof(Synchronization), new FrameworkPropertyMetadata(null));

        private static void SetSynchronizer(UIElement element, ItemsSynchronizer value)
        {
            element.SetValue(SynchronizerProperty, value);
        }

        private static ItemsSynchronizer GetSynchronizer(UIElement element)
        {
            return (ItemsSynchronizer)element.GetValue(SynchronizerProperty);
        }

        #endregion


        #region Selected items target

        public static readonly DependencyProperty SelectedItemsTargetProperty = DependencyProperty.RegisterAttached("SelectedItemsTarget", typeof(ICollection), typeof(Synchronization), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(SelectedItemsTargetChangedHandler)));

        public static void SetSelectedItemsTarget(UIElement element, ICollection value)
        {
            element.SetValue(SelectedItemsTargetProperty, value);
        }

        public static ICollection GetSelectedItemsTarget(UIElement element)
        {
            return (ICollection)element.GetValue(SelectedItemsTargetProperty);
        }

        private static void SelectedItemsTargetChangedHandler(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = sender as UIElement;

            if (element != null)
            {
                IList source;
                ItemsSynchronizer synchronizer = GetSynchronizer(element);

                // Dispose old synchronizer
                if (synchronizer != null)
                {
                    synchronizer.Dispose();
                    synchronizer = null;
                }

                // Get selected items list
                if (element is System.Windows.Controls.Primitives.MultiSelector)
                    source = ((System.Windows.Controls.Primitives.MultiSelector)element).SelectedItems;
                else if (element is ListBox)
                    source = ((ListBox)element).SelectedItems;
                else
                    return;

                // Check target (NewValue) type
                if (e.NewValue != null)
                {
                    Type targetType = e.NewValue.GetType();

                    // Arrays are not supported
                    if (!targetType.IsArray)
                    {
                        if (e.NewValue is IList && !((IList)e.NewValue).IsReadOnly)
                        {
                            synchronizer = new ListItemsSynchronizer(source, (IList)e.NewValue, GetInitialSynchronizationMode(element), GetItemConverter(element));
                        }
                        else
                        {
                            Type genericType = (from Type i in targetType.GetInterfaces() where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>) select i.GetGenericArguments()[0]).FirstOrDefault(); 

                            try
                            {
                                if (genericType != null)
                                    synchronizer = (ItemsSynchronizer)Activator.CreateInstance(typeof(CollectionItemsSynchronizer<>).MakeGenericType(genericType), source, e.NewValue, GetInitialSynchronizationMode(element), GetItemConverter(element));
                            }
                            catch (Exception ec)
                            {
                                string msg = ec.Message;
                                if (ec.InnerException != null && ec.InnerException.Message != null)
                                    msg += " Inner:" + ec.InnerException.Message;

                                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                                    datamodel.Database.Root.Messages.LogException("Synchronization", "SelectedItemsTargetChangedHandler", msg);
                            }
                        }
                    }
                }

                SetSynchronizer(element, synchronizer);
            }
        }

        #endregion
    }
}
