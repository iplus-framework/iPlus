﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace gip.ext.design.Extensions
{
	/// <summary>
	/// Base class for extensions that have an parameter-less constructor and are initialized using the
	/// OnInitialize method.
	/// </summary>
	public class DefaultExtension : Extension
	{
		DesignItem _extendedItem;
		
		/// <summary>
		/// Gets the item that is being extended by the BehaviorExtension.
		/// </summary>
		public DesignItem ExtendedItem {
			get {
				if (_extendedItem == null)
					throw new InvalidOperationException("Cannot access BehaviorExtension.ExtendedItem: " +
					                                    "The property is not initialized yet. Please move initialization logic " +
					                                    "that depends on ExtendedItem into the OnInitialized method.");
				return _extendedItem;
			}
		}
		
		/// <summary>
		/// Gets the design context of the extended item. "Context" is equivalent to "ExtendedItem.Context".
		/// </summary>
		public DesignContext Context {
			get {
				return this.ExtendedItem.Context;
			}
		}
		
		/// <summary>
		/// Gets the service container of the extended item. "Services" is equivalent to "ExtendedItem.Services".
		/// </summary>
		public ServiceContainer Services {
			get {
				return this.ExtendedItem.Services;
			}
		}
		
		/// <summary>
		/// Is called after the ExtendedItem was set.
		/// Override this method to register your behavior with the item.
		/// </summary>
		protected virtual void OnInitialized()
		{
		}
		
		/// <summary>
		/// Is called when the extension is removed.
		/// </summary>
		protected virtual void OnRemove()
		{
		}
		
		internal void CallOnRemove() { OnRemove(); }
		
		internal void InitializeDefaultExtension(DesignItem extendedItem)
		{
			Debug.Assert(this._extendedItem == null);
			Debug.Assert(extendedItem != null);
			
			this._extendedItem = extendedItem;
			OnInitialized();
		}
	}
	
	/// <summary>
	/// Base class for extension servers that create extensions that derive from <see cref="DefaultExtension"/>.
	/// </summary>
	public abstract class DefaultExtensionServer : ExtensionServer
	{
		/// <summary>
		/// Creates an instance of the DefaultExtension and calls OnInitialize on it.
		/// </summary>
		public override Extension CreateExtension(Type extensionType, DesignItem extendedItem)
		{
			DefaultExtension ext = (DefaultExtension)Activator.CreateInstance(extensionType);
			ext.InitializeDefaultExtension(extendedItem);
			return ext;
		}
		
		/// <summary>
		/// Calls OnRemove() on the DefaultExtension.
		/// </summary>
		public override void RemoveExtension(Extension extension)
		{
			DefaultExtension defaultExtension = extension as DefaultExtension;
			Debug.Assert(defaultExtension != null);
			defaultExtension.CallOnRemove();
		}
		
		/// <summary>
		/// This event is raised when ShouldApplyExtensions is invalidated for a set of items.
		/// </summary>
		public override event EventHandler<DesignItemCollectionEventArgs> ShouldApplyExtensionsInvalidated;
		
		/// <summary>
		/// Raise the ShouldApplyExtensionsInvalidated event for the specified set of design items.
		/// </summary>
		protected void ReapplyExtensions(ICollection<DesignItem> items)
		{
			if (ShouldApplyExtensionsInvalidated != null) {
				ShouldApplyExtensionsInvalidated(this, new DesignItemCollectionEventArgs(items));
			}
		}
		
        /// <summary>
        /// 
        /// </summary>
		public sealed class Permanent : DefaultExtensionServer
		{
            /// <summary>
            /// 
            /// </summary>
            /// <param name="extendedItem"></param>
            /// <returns></returns>
			public override bool ShouldApplyExtensions(DesignItem extendedItem)
			{
				return true;
			}
		}
		
		// special extension server like 'permanent' - skips applying extensions if there
		// is no design panel (e.g. in designer unit tests).
		internal sealed class PermanentWithDesignPanel : DefaultExtensionServer
		{
			public override bool ShouldApplyExtensions(DesignItem extendedItem)
			{
				return Services.GetService(typeof(IDesignPanel)) != null;
			}
		}
	}
}
