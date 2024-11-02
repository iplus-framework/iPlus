// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Linq;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    public class RPDynCacheEntry
    {
        public RPDynCacheEntry(object currentObj, object parentObj, int recusrsionDepth)
        {
            CurrentObj = currentObj;
            ParentObj = parentObj;
            RecursionDepth = recusrsionDepth;
        }
        public object CurrentObj { get; private set; }
        public object ParentObj { get; private set; }
        public int RecursionDepth { get; private set; }
    }

    /// <summary>
    /// Dynamic cache class for report paginator
    /// </summary>
    public class ReportPaginatorDynamicCache
    {
        #region private members
        private FlowDocument _flowDocument = null;

        private TableRow _tableRow = null;
        private RPDynCacheEntry _rootTableRowCacheEntry = null;
        private TableCell _cellToFillWithData = null;
        private RPDynCacheEntry _rootCellToFillCacheEntry = null;

        private Dictionary<Type, List<RPDynCacheEntry>> _documentByType = new Dictionary<Type, List<RPDynCacheEntry>>();
        private Dictionary<Type, List<RPDynCacheEntry>> _documentByInterface = new Dictionary<Type, List<RPDynCacheEntry>>();
        private List<RPDynCacheEntry> _Entries = new List<RPDynCacheEntry>();
        Stack<RPDynCacheEntry> _tableStack = new Stack<RPDynCacheEntry>();
        private int _MaxTableStackDepth = 1;
        #endregion

        #region c'tors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="flowDocument">flow document</param>
        public ReportPaginatorDynamicCache(FlowDocument flowDocument, int maxTableStackDepth = 1)
        {
            _flowDocument = flowDocument;
            _MaxTableStackDepth = maxTableStackDepth;
            BuildCache();
        }

        public ReportPaginatorDynamicCache(TableRow tableRow, TableCell cellToFillWithData, int maxTableStackDepth = 1)
        {
            _tableRow = tableRow;
            _cellToFillWithData = cellToFillWithData;
            _MaxTableStackDepth = maxTableStackDepth;

            BuildCache();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the associacted flow document
        /// </summary>
        public FlowDocument FlowDocument
        {
            get { return _flowDocument; }
        }

        public RPDynCacheEntry RootTableRowCacheEntry
        {
            get { return _rootTableRowCacheEntry; }
        }

        public RPDynCacheEntry RootTabelRowCacheEntry
        {
            get { return _rootCellToFillCacheEntry; }
        }

        string _reportHandlerNamespace = "";
        protected string ReportHandlerNamespace
        {
            get
            {
                if (String.IsNullOrEmpty(_reportHandlerNamespace))
                    _reportHandlerNamespace = this.GetType().Namespace;
                return _reportHandlerNamespace;
            }
        }

        #endregion

        #region methods

        #region private
        /// <summary>
        /// Build cache
        /// </summary>
        private void BuildCache()
        {
            DocumentWalker walker = new DocumentWalker();
            walker.VisualVisited += new DocumentVisitedEventHandler(walker_VisualVisited);
            if (_flowDocument != null)
                walker.Walk(_flowDocument);
            else if (_tableRow != null && _cellToFillWithData != null)
            {
                _rootTableRowCacheEntry = new RPDynCacheEntry(_tableRow, null, 0);
                _tableStack.Push(_rootTableRowCacheEntry);
                _Entries.Add(_rootTableRowCacheEntry);
                
                Type type = _tableRow.GetType();
                if (!_documentByType.ContainsKey(type))
                    _documentByType[type] = new List<RPDynCacheEntry>();
                _documentByType[type].Add(_rootTableRowCacheEntry);
                
                Type baseType = type;
                bool wasInNamespace = false;
                while (baseType.BaseType != null)
                {
                    baseType = baseType.BaseType;
                    if (baseType.Namespace == ReportHandlerNamespace)
                    {
                        wasInNamespace = true;
                        if (!_documentByType.ContainsKey(baseType))
                            _documentByType[baseType] = new List<RPDynCacheEntry>();
                        _documentByType[baseType].Add(_rootTableRowCacheEntry);
                    }
                    else if (wasInNamespace)
                        break;
                }

                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.Namespace != this.GetType().Namespace)
                        continue;
                    if (!_documentByInterface.ContainsKey(interfaceType))
                        _documentByInterface[interfaceType] = new List<RPDynCacheEntry>();
                    _documentByInterface[interfaceType].Add(_rootTableRowCacheEntry);
                }

                _rootCellToFillCacheEntry = new RPDynCacheEntry(_cellToFillWithData, _rootTableRowCacheEntry, 0);
                _Entries.Add(_rootCellToFillCacheEntry);

                type = _cellToFillWithData.GetType();
                if (!_documentByType.ContainsKey(type))
                    _documentByType[type] = new List<RPDynCacheEntry>();
                _documentByType[type].Add(_rootCellToFillCacheEntry);

                baseType = type;
                wasInNamespace = false;
                while (baseType.BaseType != null)
                {
                    baseType = baseType.BaseType;
                    if (baseType.Namespace == ReportHandlerNamespace)
                    {
                        wasInNamespace = true;
                        if (!_documentByType.ContainsKey(baseType))
                            _documentByType[baseType] = new List<RPDynCacheEntry>();
                        _documentByType[baseType].Add(_rootCellToFillCacheEntry);
                    }
                    else if (wasInNamespace)
                        break;
                }

                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.Namespace != this.GetType().Namespace)
                        continue;
                    if (!_documentByInterface.ContainsKey(interfaceType))
                        _documentByInterface[interfaceType] = new List<RPDynCacheEntry>();
                    _documentByInterface[interfaceType].Add(_rootCellToFillCacheEntry);
                }

                walker.TraverseBlockCollection<Inline>(_cellToFillWithData.Blocks, 0);
            }
            walker.VisualVisited -= walker_VisualVisited;
        }

        private void walker_VisualVisited(object sender, DocumentVisitedEventArgs e)
        {
            if (e.VisitedObject == null) return;
            Type type = e.VisitedObject.GetType();
            if (!_documentByType.ContainsKey(type))
                _documentByType[type] = new List<RPDynCacheEntry>();

            Type baseType = type;
            bool wasInNamespace = false;
            while (baseType.BaseType != null)
            {
                baseType = baseType.BaseType;
                if (baseType.Namespace == ReportHandlerNamespace)
                {
                    wasInNamespace = true;
                    if (!_documentByType.ContainsKey(baseType))
                        _documentByType[baseType] = new List<RPDynCacheEntry>();
                }
                else if (wasInNamespace)
                    break;
            }

            RPDynCacheEntry entry = null;
            if (e.VisitedObject is TableRowDataBase)
            {
                if (_tableStack.Count <= 0)
                {
                    entry = new RPDynCacheEntry(e.VisitedObject, null, e.RecursionDepth);
                    _tableStack.Push(entry);
                }
                else
                {
                    RPDynCacheEntry lastEntry = _tableStack.Peek();
                    if (lastEntry.RecursionDepth < e.RecursionDepth)
                    {
                        entry = new RPDynCacheEntry(e.VisitedObject, lastEntry != null ? lastEntry.CurrentObj : null, e.RecursionDepth);
                        _tableStack.Push(entry);
                    }
                    else if (lastEntry.RecursionDepth >= e.RecursionDepth)
                    {
                        _tableStack.Pop();
                        lastEntry = null;
                        if (_tableStack.Count > 0)
                            lastEntry = _tableStack.Peek();
                        entry = new RPDynCacheEntry(e.VisitedObject, lastEntry != null ? lastEntry.CurrentObj : null, e.RecursionDepth);
                        _tableStack.Push(entry);
                    }
                }
                if ((_rootTableRowCacheEntry != null && _tableStack.Count >= _MaxTableStackDepth)
                    || (_rootTableRowCacheEntry == null && _tableStack.Count >= _MaxTableStackDepth + 1))
                {
                    e.Handled = true;
                }
            }
            else
            {
                RPDynCacheEntry lastEntry = null;
                if (_tableStack.Count > 0)
                {
                    lastEntry = _tableStack.Peek();
                    if (lastEntry.RecursionDepth >= e.RecursionDepth)
                        _tableStack.Pop();
                    lastEntry = null;
                    if (_tableStack.Count > 0)
                        lastEntry = _tableStack.Peek();
                }
                entry = new RPDynCacheEntry(e.VisitedObject, lastEntry != null ? lastEntry.CurrentObj : null, e.RecursionDepth);
            }

            _Entries.Add(entry);
            _documentByType[type].Add(entry);

            baseType = type;
            wasInNamespace = false;
            while (baseType.BaseType != null)
            {
                baseType = baseType.BaseType;
                if (baseType.Namespace == ReportHandlerNamespace)
                {
                    wasInNamespace = true;
                    if (!_documentByType.ContainsKey(baseType))
                        _documentByType[baseType] = new List<RPDynCacheEntry>();
                    _documentByType[baseType].Add(entry);
                }
                else if (wasInNamespace)
                    break;
            }


            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.Namespace != this.GetType().Namespace)
                    continue;
                if (!_documentByInterface.ContainsKey(interfaceType))
                    _documentByInterface[interfaceType] = new List<RPDynCacheEntry>();
                _documentByInterface[interfaceType].Add(entry);
            }
        }
        #endregion

        #region public

        #region Find by Type
        public List<TResult> GetObjectsOfType<TResult>(RPDynCacheEntry filterParentEntry = null, bool onlyRootObjects = false) where TResult : class
        {
            Type type = typeof(TResult);
            if (!_documentByType.ContainsKey(type))
                return new List<TResult>();
            if (filterParentEntry != null)
                return _documentByType[type].Where(c => c.ParentObj == filterParentEntry.CurrentObj).Select(c => c.CurrentObj as TResult).ToList();
            else if (onlyRootObjects)
                return _documentByType[type].Where(c => c.ParentObj == null).Select(c => c.CurrentObj as TResult).ToList();
            else
                return _documentByType[type].Select(c => c.CurrentObj as TResult).ToList();
        }

        public List<RPDynCacheEntry> GetEntriesOfType<TResult>(RPDynCacheEntry filterParentEntry = null, bool onlyRootObjects = false)
        {
            Type type = typeof(TResult);
            if (!_documentByType.ContainsKey(type))
                return new List<RPDynCacheEntry>();
            if (filterParentEntry != null)
                return _documentByType[type].Where(c => c.ParentObj == filterParentEntry.CurrentObj).ToList();
            else if (onlyRootObjects)
                return _documentByType[type].Where(c => c.ParentObj == null).ToList();
            else
                return _documentByType[type];        
        }
        #endregion

        #region Find by interface
        public List<TResult> GetObjectsOfInterface<TResult>(RPDynCacheEntry filterParentEntry = null, bool onlyRootObjects = false) where TResult : class
        {
            Type type = typeof(TResult);
            if (!_documentByInterface.ContainsKey(type))
                return new List<TResult>();
            if (filterParentEntry != null)
                return _documentByInterface[type].Where(c => c.ParentObj == filterParentEntry.CurrentObj).Select(c => c.CurrentObj as TResult).ToList();
            else if (onlyRootObjects)
                return _documentByInterface[type].Where(c => c.ParentObj == null).Select(c => c.CurrentObj as TResult).ToList();
            else
                return _documentByInterface[type].Select(c => c.CurrentObj as TResult).ToList();
        }

        public List<RPDynCacheEntry> GetEntriesOfInterface<TResult>(RPDynCacheEntry filterParentEntry = null, bool onlyRootObjects = false)
        {
            Type type = typeof(TResult);
            if (!_documentByInterface.ContainsKey(type))
                return new List<RPDynCacheEntry>();
            if (filterParentEntry != null)
                return _documentByInterface[type].Where(c => c.ParentObj == filterParentEntry.CurrentObj).ToList();
            else if (onlyRootObjects)
                return _documentByInterface[type].Where(c => c.ParentObj == null).ToList();
            else
                return _documentByInterface[type];
        }
        #endregion

        #region Find Childs
        public IEnumerable<RPDynCacheEntry> GetChildEntriesOfParent(RPDynCacheEntry parentEntry, bool onlyTables = false)
        {
            if (onlyTables)
                return _Entries.Where(c => c.ParentObj == parentEntry.CurrentObj && c.CurrentObj is TableRowDataBase);
            else
                return _Entries.Where(c => c.ParentObj == parentEntry.CurrentObj);
        }

        public IEnumerable<RPDynCacheEntry> GetObjectsOfParent(object parentReportObject, bool onlyTables = false)
        {
            if (onlyTables)
                return _Entries.Where(c => c.ParentObj == parentReportObject && parentReportObject is TableRowDataBase);
            else
                return _Entries.Where(c => c.ParentObj == parentReportObject);
        }
        #endregion

        #endregion

        #endregion
    }
}
