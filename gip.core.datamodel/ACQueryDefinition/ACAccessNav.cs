// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Linq;
using gip.core.datamodel;
using System.Data;

namespace gip.core.datamodel
{
    /// <summary>
    ///   <para>Implementation of IAccessNavT&lt;T&gt;.</para>
    ///   <para>Extends ACAccess&lt;T&gt; with navigation features</para>
    /// </summary>
    /// <typeparam name="T">Type of a EF-Class</typeparam>
    /// <seealso cref="gip.core.datamodel.ACAccess{T}"/>
    /// <seealso cref="gip.core.datamodel.IAccessNavT{T}"/>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACAccessNav'}de{'ACAccessNav'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACAccessNav<T> : ACAccess<T>, IAccessNavT<T> where T : class
    {
        #region c'tors
        public ACAccessNav(ACClass acType, IACObject content, IACObjectWithInit parentACObject, ACValueList parameter, string acIdentifier = "") :
            this(acType, content, parentACObject, parameter, acIdentifier, null)
        {
        }

        public ACAccessNav(ACClass acType, IACObject content, IACObjectWithInit parentACObject, ACValueList parameter, IACObject contextForQuery) :
            this(acType, content, parentACObject, parameter, "", contextForQuery)
        {
        }

        public ACAccessNav(ACClass acType, IACObject content, IACObjectWithInit parentACObject, ACValueList parameter, string acIdentifier, IACObject contextForQuery) :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        override public bool ACDeInit(bool deleteACClassTask = false)
        {
            Current = null;
            Selected = null;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion


        #region Properties

        private Nullable<bool> _AutoSaveOnNavigation;
        /// <summary>Controls the automatic invocation of the ACSaveOrUndoChanges()-Method when the CurrentNavObject should be changed (Navigation)</summary>
        /// <value>
        ///   <c>true</c> if ACSaveOrUndoChanges() should be automatically invoked; otherwise, <c>false</c>.</value>
        public bool AutoSaveOnNavigation
        {
            get
            {
                if (_AutoSaveOnNavigation.HasValue)
                    return _AutoSaveOnNavigation.Value;
                else
                    return false;
            }
            set
            {
                _AutoSaveOnNavigation = value;
            }
        }

        private T _Selected;
        /// <summary>
        /// The "Selected"-Property points to a EF-Object in the NavList-Collection that is highlighted in a Items-Control on the GUI.
        /// Normally the "Current"-Property and the "Selected"-Property should point to the same object.
        /// </summary>
        /// <value>The selected object of type T</value>
        public T Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }


        private T _Current;
        /// <summary>
        /// The "Current"-Property points to a EF-Object in the NavList-Collection that is displayed on a Form-View on the GUI.
        /// Normally the "Current"-Property and the "Selected"-Property should point to the same object.
        /// </summary>
        /// <value>The current object of type T</value>
        public T Current
        {
            get
            {
                return _Current;
            }
            set
            {
                _Current = value;
                if (value == null)
                {
                    _NavRowCurrent = -1;
                }
                else
                {
                    try
                    {
                        _NavRowCurrent = _NavList.IndexOf(value);
                    }
                    catch (Exception e)
                    {
                        _NavRowCurrent = -1;

                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("ACAccessNav<T>", "Current", msg);
                    }
                }
            }
        }


        /// <summary>
        /// The "Selected"-Property points to a EF-Object in the NavObjectList-Collection that is highlighted in a Items-Control on the GUI.
        /// Normally the "Current"-Property and the "Selected"-Property should point to the same object.
        /// </summary>
        /// <value>The selected nav object.</value>
        public object SelectedNavObject
        {
            get
            {
                return Selected;
            }
            set
            {
                Selected = (T)value;
            }
        }


        /// <summary>
        /// The "Current"-Property points to a EF-Object in the NavObjectList-Collection that is displayed on a Form-View on the GUI.
        /// Normally the "Current"-Property and the "Selected"-Property should point to the same object.
        /// </summary>
        /// <value>The current nav object.</value>
        public object CurrentNavObject
        {
            get
            {
                return Current;
            }
            set
            {
                Current = (T)value;
            }
        }


        int _NavRowCurrent = -1;
        /// <summary>
        /// Index of the CurrentNavObject in the NavObjectList-Collection.
        /// </summary>
        /// <value>Index from 0. If NavObjectList is empty "-1" is returned.</value>
        public int NavRowCurrent
        {
            get
            {
                return _NavRowCurrent;
            }
            set
            {
                if (_NavList.Count >= value + 1)
                {
                    _NavRowCurrent = value;
                    SelectedNavObject = _NavList[_NavRowCurrent];
                }
                else
                {
                    _NavRowCurrent = -1;
                }
            }
        }
        #endregion


        #region Methods

        #region override
        protected override void OnPostNavSearch()
        {
            NavigateFirst();
        }
        #endregion


        #region Public
        /// <summary>
        /// Navigates to the first entry in the NavObjectList-Property.
        /// </summary>
        [ACMethodCommand("Navigation", "en{'Navigate to the first record'}de{'Zum ersten Datensatz navigieren'}", (short)MISort.NavigateFirst)]
        public void NavigateFirst()
        {
            if (ParentACComponent == null)
                return;
            // Nur BSOS's auf Hauptebene sollten automatisch gespeichert werden bei Datensatzwechsel,
            // damit nicht eine Speicherabfrage kommt wenn Child-BSO's instanziiert werden
            if (AutoSaveOnNavigation)
            {
                if (!ParentACComponent.ACSaveOrUndoChanges())
                    return;
            }
            NavRowCurrent = 0;
            LoadCurrent();
        }


        /// <summary>Can navigate to the first entry of the CurrentNavObject</summary>
        /// <returns>
        ///   <c>true</c> if a first entry exist; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNavigateFirst()
        {
            return NavRowCurrent > 0 && NavRowCount > 0;
        }


        /// <summary>
        /// Navigates to the previous element of the CurrentNavObject
        /// </summary>
        [ACMethodCommand("Navigate to the previous record", "en{'Previous'}de{'Zum vorherigen Datensatz navigieren'}", (short)MISort.NavigatePrev)]
        public void NavigatePrev()
        {
            if (ParentACComponent == null)
                return;
            // Nur BSOS's auf Hauptebene sollten automatisch gespeichert werden bei Datensatzwechsel,
            // damit nicht eine Speicherabfrage kommt wenn Child-BSO's instanziiert werden
            if (AutoSaveOnNavigation)
            {
                if (!ParentACComponent.ACSaveOrUndoChanges())
                    return;
            }
            if (NavRowCurrent > 0)
                NavRowCurrent--;
            LoadCurrent();
        }


        /// <summary>Can navigate to the previous entry of the CurrentNavObject</summary>
        /// <returns>
        ///   <c>true</c> if a previous entry exist; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNavigatePrev()
        {
            return NavRowCurrent > 0 && NavRowCount > 0;
        }


        /// <summary>
        /// Navigates to the next entry of the CurrentNavObject
        /// </summary>
        [ACMethodCommand("Navigation", "en{'Navigate to the next record'}de{'Zum nächsten Datensatz navigieren'}", (short)MISort.NavigateNext)]
        public void NavigateNext()
        {
            if (ParentACComponent == null)
                return;
            // Nur BSOS's auf Hauptebene sollten automatisch gespeichert werden bei Datensatzwechsel,
            // damit nicht eine Speicherabfrage kommt wenn Child-BSO's instanziiert werden
            if (AutoSaveOnNavigation)
            {
                if (!ParentACComponent.ACSaveOrUndoChanges())
                    return;
            }
            if (NavRowCurrent - 1 < NavRowCount)
                NavRowCurrent++;
            LoadCurrent();
        }


        /// <summary>Can navigate to the next entry of the CurrentNavObject</summary>
        /// <returns>
        ///   <c>true</c> if a next entry exist; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNavigateNext()
        {
            return NavRowCurrent < NavRowCount - 1 && NavRowCount > 0;
        }


        /// <summary>
        /// Navigates to the last entry in the NavObjectList-Property.
        /// </summary>
        [ACMethodCommand("Navigation", "en{'Navigate to the last record'}de{'Zum letzten Datensatz navigieren'}", (short)MISort.NavigateLast)]
        public void NavigateLast()
        {
            if (ParentACComponent == null)
                return;
            // Nur BSOS's auf Hauptebene sollten automatisch gespeichert werden bei Datensatzwechsel,
            // damit nicht eine Speicherabfrage kommt wenn Child-BSO's instanziiert werden
            if (AutoSaveOnNavigation)
            {
                if (!ParentACComponent.ACSaveOrUndoChanges())
                    return;
            }
            if (NavRowCurrent - 1 < NavRowCount)
                NavRowCurrent = NavRowCount - 1;
            LoadCurrent();
        }


        /// <summary>Can navigate to the last entry of the CurrentNavObject</summary>
        /// <returns>
        ///   <c>true</c> if a last entry exist; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNavigateLast()
        {
            return NavRowCurrent < NavRowCount - 1 && NavRowCount > 0;
        }


        public void LoadCurrent()
        {
            LoadCurrentProperty(ParentACComponent, this.ACGroup, SelectedNavObject);
        }

        public static void LoadCurrentProperty(IACComponent acComponent, IACType propertyType, object data)
        {
            if (acComponent == null || propertyType == null || String.IsNullOrEmpty(propertyType.ACGroup))
                return;
            LoadCurrentProperty(acComponent, propertyType.ACGroup, data);
        }

        public static void LoadCurrentProperty(IACComponent acComponent, string acGroup, object data)
        {
            if (acComponent == null || String.IsNullOrEmpty(acGroup) || acComponent.ComponentClass == null)
                return;
            ACClassMethod acClassMethod = acComponent.ACClassMethods.FirstOrDefault(c => c.ACGroup == acGroup && c.SortIndex == (short)MISort.Load);
            if (acClassMethod != null)
            {
                acComponent.ACUrlCommand("!" + acClassMethod.ACIdentifier);
                if (!String.IsNullOrEmpty(acClassMethod.InteractionVBContent))
                    acComponent.OnPropertyChanged(acClassMethod.InteractionVBContent);
                else
                {
                    ACClassProperty property = acComponent.ComponentClass.Properties.Where(c => c.ACGroup == acGroup && c.ACPropUsageIndex == (short)Global.ACPropUsages.Selected).FirstOrDefault();
                    if (property != null)
                        acComponent.OnPropertyChanged(property.ACIdentifier);
                }
            }
            else
            {
                ACClassProperty property = acComponent.ComponentClass.Properties.Where(c => c.ACGroup == acGroup && c.ACPropUsageIndex == (short)Global.ACPropUsages.Current).FirstOrDefault();
                if (property != null)
                {
                    acComponent.ACUrlCommand(property.ACIdentifier, new object[] { data });
                }
                property = acComponent.ComponentClass.Properties.Where(c => c.ACGroup == acGroup && c.ACPropUsageIndex == (short)Global.ACPropUsages.Selected).FirstOrDefault();
                if (property != null)
                    acComponent.OnPropertyChanged(property.ACIdentifier);
            }
        }
        #endregion

        #endregion
    }
}