using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gip.core.datamodel;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents the editor for values.
    /// </summary>
    public class VBValueEditor : ContentControl
    {
        // bool _themeApplied = false;
        public VBValueEditor(ACValueList acValueList, IACContainer acValue, ACClassProperty propertyInfo, IACObject contextACObject)
        {
            // TODO: Übergabe des VBSource
            _ACValueList = acValueList;
            _ACValue = acValue;
            _ACClassPropertyInfo = propertyInfo;
            DataContext = contextACObject;
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // TODO: VBSource in VBDataGridACVAlueColumn setzbar
            // TODO: Control auch ohne VBDataGridACVAlueColumn verwenden

            Boolean isComboBox = false;
            Database typeDatabase = null;

            if (_ACValue == null)
                return;

            if (_ACValue.ValueTypeACClass != null)
            {
                if (_ACValue.Value != null && _ACValue.Value is VBEntityObject)
                {
                    isComboBox = true;
                    if (typeDatabase == null)
                        typeDatabase = (_ACValue.Value as VBEntityObject).GetObjectContext() as Database;
                }
                else if ((_ACValue.ValueTypeACClass.ObjectType != null) && (typeof(VBEntityObject).IsAssignableFrom(_ACValue.ValueTypeACClass.ObjectType)))
                {
                    isComboBox = true;
                }
            }
            if (_ACClassPropertyInfo != null)
                typeDatabase = _ACClassPropertyInfo.Database;
            if (typeDatabase == null)
                typeDatabase = Database.GlobalDatabase;

            if (isComboBox)
            {
                VBComboBox comboBox = new VBComboBox();
                comboBox.ShowCaption = false;
                IACType dsACTypeInfo = null;
                object dsSource = null;
                string dsPath = "";
                Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;
                string acSource = "";
                bool isComboBound = false;

                // 1. Herausfinden, ob überhaupt Entity

                // Gibt es eine Datenquellenbeschreibung in den Eigenschaften einer abgeleiteten ACMethod
                if (_ACValueList != null && _ACValueList.ParentACMethod != null)
                {
                    ACClass acTypeACMethod = typeDatabase.GetACType(_ACValueList.ParentACMethod.GetType()) as ACClass;
                    if (acTypeACMethod != null)
                    {
                        ACClassProperty acClassPropertyOfValue = acTypeACMethod.GetProperty(_ACValue.ACIdentifier);
                        if (acClassPropertyOfValue != null)
                        {
                            acSource = acClassPropertyOfValue.ACSource;
                            if (!String.IsNullOrEmpty(acSource))
                            {
                                if (ContextACObject.ACUrlBinding(acSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
                                {
                                    if (dsPath.StartsWith(Const.ContextDatabase + "."))
                                    {
                                        ACQueryDefinition queryDef = this.Root().Queries.CreateQueryByClass(null, dsACTypeInfo.ValueTypeACClass.PrimaryNavigationquery(), dsACTypeInfo.ValueTypeACClass.PrimaryNavigationquery().ACIdentifier, true);

                                        Type t = dsSource.GetType();
                                        PropertyInfo pi = t.GetProperty(Const.ContextDatabase);

                                        var lastPath = dsPath.Substring(9);
                                        var database = pi.GetValue(dsSource, null) as IACObject;
                                        var result = database.ACSelect(queryDef, lastPath);
                                        comboBox.ItemsSource = result;

                                        List<ACColumnItem> vbShowColumns = queryDef.GetACColumns("");
                                        ACColumnItem col = vbShowColumns.First();
                                        comboBox.DisplayMemberPath = col.ACIdentifier;
                                        isComboBound = true;
                                    }
                                    else
                                    {
                                        // Listenbereich von Combobox füllen 
                                        Binding binding = new Binding();
                                        binding.Source = dsSource;
                                        if (!string.IsNullOrEmpty(dsPath))
                                        {
                                            binding.Path = new PropertyPath(dsPath);
                                        }
                                        comboBox.SetBinding(ComboBox.ItemsSourceProperty, binding);
                                        isComboBound = true;
                                    }
                                }
                            }
                        }

                    }
                }
                // Falls keine Quellenbeschreibung gefunden, dann suche in Klassenbeschreibung des Datentyps
                if (String.IsNullOrEmpty(acSource) && (_ACValue.ValueTypeACClass != null))
                {
                     ACClass queryACClass = _ACValue.ValueTypeACClass.PrimaryNavigationquery();
                     if (queryACClass != null)
                     {
                         IACEntityObjectContext database = null;
                         ACQueryDefinition queryDef = this.Root().Queries.CreateQueryByClass(null, queryACClass, queryACClass.ACIdentifier, true);
                         if (_ACValue.Value != null && _ACValue.Value is VBEntityObject)
                             database = (_ACValue.Value as VBEntityObject).GetObjectContext();
                         else
                             database = _ACValue.ValueTypeACClass.Database;
                         var result = database.ACSelect(queryDef);
                         comboBox.ItemsSource = result;

                         List<ACColumnItem> vbShowColumns = queryDef.GetACColumns("");
                         ACColumnItem col = vbShowColumns.First();
                         comboBox.DisplayMemberPath = col.ACIdentifier;
                         isComboBound = true;
                     }
                }

                if (isComboBound)
                {
                    Binding binding2 = new Binding();
                    binding2.Source = _ACValue;
                    binding2.Path = new PropertyPath(Const.Value);
                    binding2.Mode = BindingMode.TwoWay;
                    //SetBinding(ComboBox.SelectedItemProperty, binding2);
                    comboBox.SetBinding(ComboBox.SelectedValueProperty, binding2);
                    comboBox.ShowCaption = false;
                    Content = comboBox;
                }
                else // Textbox
                {
                    VBTextBox textBox = new VBTextBox();
                    textBox.TextAlignment = TextAlignment;
                    Binding binding2 = new Binding();
                    binding2.Source = _ACValue;
                    binding2.Path = new PropertyPath(Const.Value);
                    binding2.Mode = BindingMode.TwoWay;
                    textBox.SetBinding(VBTextBox.TextProperty, binding2);
                    textBox.ShowCaption = false;
                    Content = textBox;
                }
            }
            else
            {
                Type fullType = _ACValue.ValueTypeACClass.ObjectFullType;
                ACValue acValue = _ACValue as ACValue;
                if (acValue != null)
                    fullType = acValue.ObjectFullType;
                if (fullType.IsAssignableFrom(typeof(DateTime)))
                {
                    VBDateTimePicker dateTimePicker = new VBDateTimePicker();
                    Binding binding2 = new Binding();
                    binding2.Source = _ACValue;
                    binding2.Path = new PropertyPath(Const.Value);
                    binding2.Mode = BindingMode.TwoWay;
                    dateTimePicker.SetBinding(VBDateTimePicker.SelectedDateProperty, binding2);
                    dateTimePicker.ShowCaption = false;
                    Content = dateTimePicker;
                }
                else if (fullType.IsAssignableFrom(typeof(Boolean)))
                {
                    VBCheckBox checkBox = new VBCheckBox();
                    Binding binding2 = new Binding();
                    binding2.Source = _ACValue;
                    binding2.Path = new PropertyPath(Const.Value);
                    binding2.Mode = BindingMode.TwoWay;
                    checkBox.SetBinding(VBCheckBox.IsCheckedProperty, binding2);
                    checkBox.ShowCaption = false;
                    Content = checkBox;
                }
                else // Textbox
                {
                    VBTextBox textBox = new VBTextBox();
                    textBox.TextAlignment = TextAlignment;
                    Binding binding2 = new Binding();
                    binding2.Source = _ACValue;
                    binding2.Path = new PropertyPath(Const.Value);
                    if (fullType.IsValueType || (fullType.IsClass && typeof(String).IsAssignableFrom(fullType)))
                    {
                        binding2.Mode = BindingMode.TwoWay;
                        textBox.IsEnabled = true;
                    }
                    else
                    {
                        binding2.Mode = BindingMode.OneWay;
                        textBox.IsEnabled = false;
                    }
                    textBox.SetBinding(VBTextBox.TextProperty, binding2);
                    textBox.ShowCaption = false;
                    Content = textBox;
                }
            }
        }

        private IACContainer _ACValue = null;
        private ACValueList _ACValueList = null;
        private ACClassProperty _ACClassPropertyInfo = null;

        /// <summary>
        /// Gets the ContextACObject.
        /// </summary>
        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        /// <summary>
        /// Represents the dependency property for TextAlignment.
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty
            = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(VBValueEditor));

        /// <summary>
        /// Gets or sets the TextAlignment.
        /// </summary>
        [Category("VBControl")]
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }


    }
}
