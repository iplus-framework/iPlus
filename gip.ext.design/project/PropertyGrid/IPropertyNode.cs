// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Markup;

namespace gip.ext.design.PropertyGrid
{
	/// <summary>
	/// View-Model class for the property grid.
	/// </summary>
	public interface IPropertyNode : INotifyPropertyChanged
	{
		ReadOnlyCollection<DesignItemProperty> Properties { get; }
		
		/// <summary>
		/// Gets the name of the property.
		/// </summary>
		string Name { get; }
		
		/// <summary>
		/// Gets if this property node represents an event.
		/// </summary>
		bool IsEvent { get; }

        /// <summary>
        /// Gets if this property node represents an event.
        /// </summary>
        bool IsDependencyProperty { get; }
        
        /// <summary>
		/// Gets the design context associated with this set of properties.
		/// </summary>
		DesignContext Context { get; }
		
		/// <summary>
		/// Gets the service container associated with this set of properties.
		/// </summary>
		ServiceContainer Services { get; }
		
		/// <summary>
		/// Gets the editor control that edits this property.
		/// </summary>
		FrameworkElement Editor { get; }
		
		/// <summary>
		/// Gets the first property (equivalent to Properties[0])
		/// </summary>
		DesignItemProperty FirstProperty { get; }
		
		/// <summary>
		/// For nested property nodes, gets the parent node.
		/// </summary>
		PropertyNode Parent { get; }
		
		/// <summary>
		/// For nested property nodes, gets the level of this node.
		/// </summary>
		int Level { get; }
		
		/// <summary>
		/// Gets the category of this node.
		/// </summary>
		Category Category { get; set; }
		
		/// <summary>
		/// Gets the list of child nodes.
		/// </summary>
		ObservableCollection<PropertyNode> Children { get; }
		
		/// <summary>
		/// Gets the list of advanced child nodes (not visible by default).
		/// </summary>
		ObservableCollection<PropertyNode> MoreChildren { get; }

		
		/// <summary>
		/// Gets whether this property node is currently expanded.
		/// </summary>
		bool IsExpanded { get; set; }


		/// <summary>
		/// Gets whether this property node has children.
		/// </summary>
		bool HasChildren { get; }
		
		/// <summary>
		/// Gets the description object using the IPropertyDescriptionService.
		/// </summary>
		object Description { get; }
		
		/// <summary>
		/// Gets/Sets the value of this property.
		/// </summary>
		object Value { get;set; }
		
		/// <summary>
		/// Gets/Sets the value of this property in string form
		/// </summary>
		string ValueString { get; set; }

		/// <summary>
		/// Gets whether the property node is enabled for editing.
		/// </summary>
		bool IsEnabled { get; }
		
		/// <summary>
		/// Gets whether this property was set locally.
		/// </summary>
		bool IsSet { get; }
		
		/// <summary>
		/// Gets the color of the name.
		/// Depends on the type of the value (binding/resource/etc.)
		/// </summary>
		Brush NameForeground { get; }
		
		/// <summary>
		/// Returns the DesignItem that owns the property (= the DesignItem that is currently selected).
		/// Returns null if multiple DesignItems are selected.
		/// </summary>
		DesignItem ValueItem { get; }
		
		/// <summary>
		/// Gets whether the property value is ambiguous (multiple controls having different values are selected).
		/// </summary>
		bool IsAmbiguous { get; }

		/// <summary>
		/// Gets/Sets whether the property is visible.
		/// </summary>
		bool IsVisible { get; }

        /// <summary>
		/// Gets whether resetting the property is possible.
		/// </summary>
		bool CanReset { get; }
		
		/// <summary>
		/// Resets the property.
		/// </summary>
		void Reset();

		/// <summary>
		/// Replaces the value of this node with a new binding.
		/// </summary>
		void CreateBindings();
        /// <summary>
        /// Replaces the value of this node with a new binding.
        /// </summary>
        void CreateMultiBindings();
	}
}
