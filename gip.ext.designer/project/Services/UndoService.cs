﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using gip.ext.designer.Xaml;
using gip.ext.design;

namespace gip.ext.designer.Services
{
	#region ITransactionItem
	public interface ITransactionItem : IUndoAction
	{
		void Do();
		void Undo();
		bool MergeWith(ITransactionItem other);
	}
	#endregion
	
	#region IUndoAction
	/// <summary>
	/// Describes an action available on the undo or redo stack.
	/// </summary>
	public interface IUndoAction
	{
		/// <summary>
		/// The list of elements affected by the action.
		/// </summary>
		ICollection<DesignItem> AffectedElements { get; }
		
		/// <summary>
		/// The title of the action.
		/// </summary>
		string Title { get; }
	}
	#endregion
	
	#region UndoTransaction
	/// <summary>
	/// Supports ChangeGroup transactions and undo behavior.
	/// </summary>
	sealed class UndoTransaction : ChangeGroup, ITransactionItem
	{
		readonly ICollection<DesignItem> affectedElements;
		
		internal UndoTransaction(ICollection<DesignItem> affectedElements)
		{
			this.affectedElements = affectedElements;
		}
		
		public ICollection<DesignItem> AffectedElements {
			get { return affectedElements; }
		}
		
		public enum TransactionState
		{
			Open,
			Completed,
			Undone,
			Failed
		}
		
		TransactionState _state;
		
		public TransactionState State {
			get { return _state; }
		}
		
		List<ITransactionItem> items = new List<ITransactionItem>();
		
		public void Execute(ITransactionItem item)
		{
			AssertState(TransactionState.Open);
			item.Do();
			
			foreach (var existingItem in items) {
				if (existingItem.MergeWith(item))
					return;
			}
			
			items.Add(item);
		}
		
		private void AssertState(TransactionState expectedState)
		{
			if (_state != expectedState)
				throw new InvalidOperationException("Expected state " + expectedState + ", but state is " + _state);
		}
		
		public event EventHandler Committed;
		public event EventHandler RolledBack;
		
		public override void Commit()
		{
			AssertState(TransactionState.Open);
			_state = TransactionState.Completed;
			if (Committed != null)
				Committed(this, EventArgs.Empty);
		}
		
		public override void Abort()
		{
			AssertState(TransactionState.Open);
			_state = TransactionState.Failed;
			InternalRollback();
			if (RolledBack != null)
				RolledBack(this, EventArgs.Empty);
		}
		
		public void Undo()
		{
			AssertState(TransactionState.Completed);
			_state = TransactionState.Undone;
			InternalRollback();
		}
		
		void InternalRollback()
		{
			try {
				for (int i = items.Count - 1; i >= 0; i--) {
					items[i].Undo();
				}
			} catch (Exception ec)
            {
				_state = TransactionState.Failed;

                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null &&
                                                                      gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
                    gip.core.datamodel.Database.Root.Messages.LogException("UndoTransaction", "InternalRollback", msg);

                throw;
			}
		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
		                                                 Justification = "We rethrow the original exception, not the follow-up error.")]
		public void Redo()
		{
			AssertState(TransactionState.Undone);
			try {
				for (int i = 0; i < items.Count; i++) {
					items[i].Do();
				}
				_state = TransactionState.Completed;
			} catch (Exception e)
            {
				_state = TransactionState.Failed;

                string msgEc = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msgEc += " Inner:" + e.InnerException.Message;

                if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null &&
                                                                      gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
                    gip.core.datamodel.Database.Root.Messages.LogException("UndoTransaction", "Redo", msgEc);

                try
                {
					InternalRollback();
				} catch (Exception ec)
                {
					Debug.WriteLine("Exception rolling back after Redo error:\n" + ec.ToString());

                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null &&
                                                                          gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
                        gip.core.datamodel.Database.Root.Messages.LogException("UndoTransaction", "Redo(10)", msg);
                }
				throw;
			}
		}
		
		void ITransactionItem.Do()
		{
			if (_state != TransactionState.Completed) {
				Redo();
			}
		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
		                                                 Justification = "We avoid throwing exceptions here because a disposed transaction" +
		                                                 " indicates another exception happened earlier.")]
		protected override void Dispose()
		{
			if (_state == TransactionState.Open) {
				try {
					Abort();
				} catch (Exception ex) {
					Debug.WriteLine("Exception rolling back after failure:\n" + ex.ToString());
				}
			}
		}
		
		bool ITransactionItem.MergeWith(ITransactionItem other)
		{
			return false;
		}
	}
	#endregion
	
	#region UndoService

    /// <summary>
    /// iplus Extension
    /// </summary>
    public class UndoServiceTransactionEventArgs : EventArgs
    {
        #region c'tors
        public UndoServiceTransactionEventArgs(ITransactionItem affectedItem)
            : base()
        {
            _AffectedItem = affectedItem;
        }
        #endregion

        ITransactionItem _AffectedItem;
        public ITransactionItem AffectedItem
        {
            get
            {
                return _AffectedItem;
            }
        }
    }
    public delegate void UndoServiceTransactionExecuted(object sender, UndoServiceTransactionEventArgs e);

	/// <summary>
	/// Service supporting Undo/Redo actions on the design surface.
	/// </summary>
	public sealed class UndoService
	{
		Stack<UndoTransaction> _transactionStack = new Stack<UndoTransaction>();
		Stack<ITransactionItem> _undoStack = new Stack<ITransactionItem>();
		Stack<ITransactionItem> _redoStack = new Stack<ITransactionItem>();
		
		internal UndoTransaction StartTransaction(ICollection<DesignItem> affectedItems)
		{
			UndoTransaction t = new UndoTransaction(affectedItems);
			_transactionStack.Push(t);
			t.Committed += TransactionFinished;
			t.RolledBack += TransactionFinished;
			t.Committed += delegate(object sender, EventArgs e) {
				Execute((UndoTransaction)sender);
			};
			return t;
		}
		
		void TransactionFinished(object sender, EventArgs e)
		{
			if (sender != _transactionStack.Pop()) {
				throw new Exception("Invalid transaction finish, nested transactions must finish first");
			}
		}
		
		internal void Execute(ITransactionItem item)
		{
			if (_transactionStack.Count == 0) {
				item.Do();
				_undoStack.Push(item);
				_redoStack.Clear();
				OnUndoStackChanged(EventArgs.Empty);
                // iplus Extension
                OnTransactionExecuted(new UndoServiceTransactionEventArgs(item));
			} else {
				_transactionStack.Peek().Execute(item);
                OnTransactionExecuted(new UndoServiceTransactionEventArgs(item));
            }
		}
		
		/// <summary>
		/// Gets if undo actions are available.
		/// </summary>
		public bool CanUndo {
			get { return _undoStack.Count > 0; }
		}
		
		/// <summary>
		/// Is raised when the undo stack has changed.
		/// </summary>
		public event EventHandler UndoStackChanged;
		
		void OnUndoStackChanged(EventArgs e)
		{
			if (UndoStackChanged != null) {
				UndoStackChanged(this, e);
			}
		}

        public event UndoServiceTransactionExecuted TransactionExecuted;
        void OnTransactionExecuted(UndoServiceTransactionEventArgs e)
        {
            if (TransactionExecuted != null)
            {
                TransactionExecuted(this, e);
            }
        }
		
		/// <summary>
		/// Undoes the last action.
		/// </summary>
		public void Undo()
		{
			if (!CanUndo)
				throw new InvalidOperationException("Cannot Undo: undo stack is empty");
			if (_transactionStack.Count != 0)
				throw new InvalidOperationException("Cannot Undo while transaction is running");
			ITransactionItem item = _undoStack.Pop();
			try {
				item.Undo();
				_redoStack.Push(item);
				OnUndoStackChanged(EventArgs.Empty);
			} catch (Exception ec)
            {
				// state might be invalid now, clear stacks to prevent getting more inconsistencies
				Clear();

                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null &&
                                                                      gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
                    gip.core.datamodel.Database.Root.Messages.LogException("UndoService", "Undo", msg);

                throw;
			}
		}
		
		/// <summary>
		/// Gets the list of names of the available actions on the undo stack.
		/// </summary>
		public IEnumerable<IUndoAction> UndoActions {
			get {
				return GetActions(_undoStack);
			}
		}
		
		/// <summary>
		/// Gets the list of names of the available actions on the undo stack.
		/// </summary>
		public IEnumerable<IUndoAction> RedoActions {
			get {
				return GetActions(_redoStack);
			}
		}
		
		static IEnumerable<IUndoAction> GetActions(Stack<ITransactionItem> stack)
		{
			foreach (ITransactionItem item in stack)
				yield return item;
		}
		
		/// <summary>
		/// Gets if there are redo actions available.
		/// </summary>
		public bool CanRedo { get { return _redoStack.Count > 0; } }
		
		/// <summary>
		/// Redoes a previously undone action.
		/// </summary>
		public void Redo()
		{
			if (!CanRedo)
				throw new InvalidOperationException("Cannot Redo: redo stack is empty");
			if (_transactionStack.Count != 0)
				throw new InvalidOperationException("Cannot Redo while transaction is running");
			ITransactionItem item = _redoStack.Pop();
			try {
				item.Do();
				_undoStack.Push(item);
				OnUndoStackChanged(EventArgs.Empty);
			} catch (Exception ec)
            {
				// state might be invalid now, clear stacks to prevent getting more inconsistencies
				Clear();

                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null &&
                                                                      gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
                    gip.core.datamodel.Database.Root.Messages.LogException("UndoService", "Redo", msg);

                throw;
			}
		}
		
		/// <summary>
		/// Clears saved actions (both undo and redo stack).
		/// </summary>
		public void Clear()
		{
			_undoStack.Clear();
			_redoStack.Clear();
			OnUndoStackChanged(EventArgs.Empty);
		}
	}
	#endregion
}
