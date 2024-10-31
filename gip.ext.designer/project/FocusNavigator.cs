// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Linq;
using System.Windows.Input;
using gip.ext.design;

namespace gip.ext.designer
{
    /// <summary>
    /// Manages the Focus/Primary Selection using TAB for down-the-tree navigation and Shift+TAB for up-the-tree navigation. 
    /// </summary>
    class FocusNavigator
    {
        /* The Focus navigator do not involves the concept of Logical Focus or KeyBoard Focus
         * since nothing is getting focoused on the designer except for the DesignPanel. It just changes
         * the primary selection between the hierarchy of elements present on the designer. */

        private readonly DesignSurface _surface;
        private KeyBinding _tabBinding;
        private KeyBinding _shiftTabBinding;
        private KeyBinding _ctrlUpBinding;
        private KeyBinding _ctrlDownBinding;
        private KeyBinding _ctrlLeftBinding;
        private KeyBinding _ctrlRightBinding;
        private KeyBinding _ShiftUpBinding;
        private KeyBinding _ShiftDownBinding;
        private KeyBinding _ShiftLeftBinding;
        private KeyBinding _ShiftRightBinding;

        public FocusNavigator(DesignSurface surface)
        {
            this._surface = surface;
        }

        /// <summary>
        /// Starts the navigator on the Design surface and add bindings.
        /// </summary>
        public void Start()
        {
            DesignCommand tabFocus = new DesignCommand(parameter => this.MoveFocusForward(_surface), parameter => CanMoveFocusForward(_surface));
            DesignCommand shiftTabFocus = new DesignCommand(parameter => this.MoveFocusBack(_surface), parameter => this.CanMoveFocusBack(_surface));
            DesignCommand itemCtrlUp = new DesignCommand(parameter => this.MoveItem(_surface, Key.Up, false), parameter => CanMoveItem(_surface));
            DesignCommand itemCtrlDown = new DesignCommand(parameter => this.MoveItem(_surface, Key.Down, false), parameter => CanMoveItem(_surface));
            DesignCommand itemCtrlLeft = new DesignCommand(parameter => this.MoveItem(_surface, Key.Left, false), parameter => CanMoveItem(_surface));
            DesignCommand itemCtrlRight = new DesignCommand(parameter => this.MoveItem(_surface, Key.Right, false), parameter => CanMoveItem(_surface));
            DesignCommand itemShiftUp = new DesignCommand(parameter => this.MoveItem(_surface, Key.Up, true), parameter => CanMoveItem(_surface));
            DesignCommand itemShiftDown = new DesignCommand(parameter => this.MoveItem(_surface, Key.Down, true), parameter => CanMoveItem(_surface));
            DesignCommand itemShiftLeft = new DesignCommand(parameter => this.MoveItem(_surface, Key.Left, true), parameter => CanMoveItem(_surface));
            DesignCommand itemShiftRight = new DesignCommand(parameter => this.MoveItem(_surface, Key.Right, true), parameter => CanMoveItem(_surface));

            _tabBinding = new KeyBinding(tabFocus, new KeyGesture(Key.Tab));
            _shiftTabBinding = new KeyBinding(shiftTabFocus, new KeyGesture(Key.Tab, ModifierKeys.Shift));
            _ctrlUpBinding = new KeyBinding(itemCtrlUp, new KeyGesture(Key.Up, ModifierKeys.Control));
            _ctrlDownBinding = new KeyBinding(itemCtrlDown, new KeyGesture(Key.Down, ModifierKeys.Control));
            _ctrlLeftBinding = new KeyBinding(itemCtrlLeft, new KeyGesture(Key.Left, ModifierKeys.Control));
            _ctrlRightBinding = new KeyBinding(itemCtrlRight, new KeyGesture(Key.Right, ModifierKeys.Control));
            _ShiftUpBinding = new KeyBinding(itemShiftUp, new KeyGesture(Key.Up, ModifierKeys.Shift));
            _ShiftDownBinding = new KeyBinding(itemShiftDown, new KeyGesture(Key.Down, ModifierKeys.Shift));
            _ShiftLeftBinding = new KeyBinding(itemShiftLeft, new KeyGesture(Key.Left, ModifierKeys.Shift));
            _ShiftRightBinding = new KeyBinding(itemShiftRight, new KeyGesture(Key.Right, ModifierKeys.Shift));
            IKeyBindingService kbs = _surface.DesignContext.Services.GetService(typeof(IKeyBindingService)) as IKeyBindingService;
            if (kbs != null)
            {
                kbs.RegisterBinding(_tabBinding);
                kbs.RegisterBinding(_shiftTabBinding);
                kbs.RegisterBinding(_ctrlUpBinding);
                kbs.RegisterBinding(_ctrlDownBinding);
                kbs.RegisterBinding(_ctrlLeftBinding);
                kbs.RegisterBinding(_ctrlRightBinding);
                kbs.RegisterBinding(_ShiftUpBinding);
                kbs.RegisterBinding(_ShiftDownBinding);
                kbs.RegisterBinding(_ShiftLeftBinding);
                kbs.RegisterBinding(_ShiftRightBinding);
            }
        }

        /// <summary>
        /// De-register the bindings from the Design Surface
        /// </summary>
        public void End()
        {
            IKeyBindingService kbs = _surface.DesignContext.Services.GetService(typeof(IKeyBindingService)) as IKeyBindingService;
            if (kbs != null)
            {
                kbs.DeregisterBinding(_tabBinding);
                kbs.DeregisterBinding(_shiftTabBinding);
                kbs.DeregisterBinding(_ctrlUpBinding);
                kbs.DeregisterBinding(_ctrlDownBinding);
                kbs.DeregisterBinding(_ctrlLeftBinding);
                kbs.DeregisterBinding(_ctrlRightBinding);
                kbs.DeregisterBinding(_ShiftUpBinding);
                kbs.DeregisterBinding(_ShiftDownBinding);
                kbs.DeregisterBinding(_ShiftLeftBinding);
                kbs.DeregisterBinding(_ShiftRightBinding);
            }
        }

        /// <summary>
        /// Moves the Foucus down the tree.
        /// </summary>        
        void MoveFocusForward(object surface)
        {
            var designSurface = surface as DesignSurface;
            if (designSurface != null)
            {
                var context = designSurface.DesignContext;
                ISelectionService selection = context.Services.Selection;
                DesignItem item = selection.PrimarySelection;
                selection.SetSelectedComponents(selection.SelectedItems, SelectionTypes.Remove);
                if (item != GetLastElement())
                {
                    if (item.ContentProperty != null)
                    {
                        if (item.ContentProperty.IsCollection)
                        {
                            if (item.ContentProperty.CollectionElements.Count != 0)
                            {
                                if (ModelTools.CanSelectComponent(item.ContentProperty.CollectionElements.First()))
                                    selection.SetSelectedComponents(new DesignItem[] { item.ContentProperty.CollectionElements.First() }, SelectionTypes.Primary);
                                else
                                    SelectNextInPeers(item);
                            }
                            else
                                SelectNextInPeers(item);
                        }
                        else if (item.ContentProperty.Value != null)
                        {
                            if (ModelTools.CanSelectComponent(item.ContentProperty.Value))
                                selection.SetSelectedComponents(new DesignItem[] { item.ContentProperty.Value }, SelectionTypes.Primary);
                            else
                                SelectNextInPeers(item);
                        }
                        else
                        {
                            SelectNextInPeers(item);
                        }
                    }
                    else
                    {
                        SelectNextInPeers(item);
                    }
                }
                else
                { //if the element was last element move focus to the root element to keep a focus cycle.
                    selection.SetSelectedComponents(new DesignItem[] { context.RootItem }, SelectionTypes.Primary);
                }
            }
        }

        /// <summary>
        /// Checks if focus navigation should be for down-the-tree be done.
        /// </summary>
        /// <param name="surface">Design Surface</param>        
        bool CanMoveFocusForward(object surface)
        {
            var designSurface = surface as DesignSurface;
            if (designSurface != null)
                if (Keyboard.FocusedElement == designSurface._designPanel)
                    return true;
            return false;
        }

        /// <summary>
        /// Moves focus up-the-tree.
        /// </summary>        
        void MoveFocusBack(object surface)
        {
            var designSurface = surface as DesignSurface;
            if (designSurface != null)
            {
                var context = designSurface.DesignContext;
                ISelectionService selection = context.Services.Selection;
                DesignItem item = selection.PrimarySelection;
                if (item != context.RootItem)
                {
                    if (item.Parent != null && item.Parent.ContentProperty.IsCollection)
                    {
                        int index = item.Parent.ContentProperty.CollectionElements.IndexOf(item);
                        if (index != 0)
                        {
                            if (ModelTools.CanSelectComponent(item.Parent.ContentProperty.CollectionElements.ElementAt(index - 1)))
                                selection.SetSelectedComponents(new DesignItem[] { item.Parent.ContentProperty.CollectionElements.ElementAt(index - 1) }, SelectionTypes.Primary);
                        }
                        else
                        {
                            if (ModelTools.CanSelectComponent(item.Parent))
                                selection.SetSelectedComponents(new DesignItem[] { item.Parent }, SelectionTypes.Primary);
                        }

                    }
                    else
                    {
                        if (ModelTools.CanSelectComponent(item.Parent))
                            selection.SetSelectedComponents(new DesignItem[] { item.Parent }, SelectionTypes.Primary);
                    }
                }
                else
                {// if the element was root item move focus again to the last element.
                    selection.SetSelectedComponents(new DesignItem[] { GetLastElement() }, SelectionTypes.Primary);
                }
            }
        }

        /// <summary>
        /// Checks if focus navigation for the up-the-tree should be done.
        /// </summary>
        /// <param name="surface">Design Surface</param>        
        bool CanMoveFocusBack(object surface)
        {
            var designSurface = surface as DesignSurface;
            if (designSurface != null)
                if (Keyboard.FocusedElement == designSurface._designPanel)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets the last element in the element hierarchy.
        /// </summary>        
        DesignItem GetLastElement()
        {
            DesignItem item = _surface.DesignContext.RootItem;
            while (item != null && item.ContentProperty != null)
            {
                if (item.ContentProperty.IsCollection)
                {
                    if (item.ContentProperty.CollectionElements.Count != 0)
                    {
                        if (ModelTools.CanSelectComponent(item.ContentProperty.CollectionElements.Last()))
                            item = item.ContentProperty.CollectionElements.Last();
                        else
                            break;
                    }
                    else
                        break;
                }
                else
                {
                    if (item.ContentProperty.Value != null)
                        item = item.ContentProperty.Value;
                    else
                        break;
                }
            }
            return item;
        }

        /// <summary>
        /// Select the next element in the element collection if <paramref name="item"/> parent's had it's content property as collection.
        /// </summary>        
        void SelectNextInPeers(DesignItem item)
        {
            ISelectionService selection = _surface.DesignContext.Services.Selection;
            if (item.Parent != null && item.Parent.ContentProperty != null)
            {
                if (item.Parent.ContentProperty.IsCollection)
                {
                    int index = item.Parent.ContentProperty.CollectionElements.IndexOf(item);
                    if (index != item.Parent.ContentProperty.CollectionElements.Count)
                        selection.SetSelectedComponents(new DesignItem[] { item.Parent.ContentProperty.CollectionElements.ElementAt(index + 1) }, SelectionTypes.Primary);
                }
            }
        }

        /// <summary>
        /// Moves the Foucus down the tree.
        /// </summary>        
        void MoveItem(object surface, Key direction, bool large)
        {
            var designSurface = surface as DesignSurface;
            if (designSurface != null)
            {
                try
                {
                    var context = designSurface.DesignContext;
                    ISelectionService selection = context.Services.Selection;
                    DesignItem item = selection.PrimarySelection;
                    PlacementOperation operation = PlacementOperation.Start(context.Services.Selection.SelectedItems, PlacementType.Move);
                    if (operation != null)
                    {
                        foreach (PlacementInformation info in operation.PlacedItems)
                        {
                            if (direction == Key.Up)
                            {
                                if (large)
                                {
                                    if (designSurface.IsRasterOn)
                                    {
                                        int rest = 0;
                                        if (System.Convert.ToInt32(info.OriginalBounds.Top) != 0)
                                            rest = System.Convert.ToInt32(info.OriginalBounds.Top) % System.Convert.ToInt32(designSurface.RasterSize);
                                        if (rest == 0)
                                            rest = System.Convert.ToInt32(designSurface.RasterSize);
                                        else
                                        {
                                            if (rest < 0)
                                                rest *= -1;
                                            //if (rest > 0)
                                            //rest = System.Convert.ToInt32(designSurface.RasterSize) - rest;
                                        }
                                        info.Bounds = new System.Windows.Rect(info.OriginalBounds.Left,
                                                               System.Convert.ToInt32(info.OriginalBounds.Top) - rest,
                                                               info.OriginalBounds.Width,
                                                               info.OriginalBounds.Height);
                                    }
                                    else
                                    {
                                        info.Bounds = new System.Windows.Rect(info.OriginalBounds.Left,
                                                               info.OriginalBounds.Top - 10,
                                                               info.OriginalBounds.Width,
                                                               info.OriginalBounds.Height);
                                    }
                                }
                                else
                                {
                                    info.Bounds = new System.Windows.Rect(info.OriginalBounds.Left,
                                                            info.OriginalBounds.Top - 1,
                                                            info.OriginalBounds.Width,
                                                            info.OriginalBounds.Height);
                                }
                            }
                            else if (direction == Key.Down)
                            {
                                if (large)
                                {
                                    if (designSurface.IsRasterOn)
                                    {
                                        int rest = 0;
                                        if (System.Convert.ToInt32(info.OriginalBounds.Top) != 0)
                                            rest = System.Convert.ToInt32(info.OriginalBounds.Top) % System.Convert.ToInt32(designSurface.RasterSize);
                                        if (rest == 0)
                                            rest = System.Convert.ToInt32(designSurface.RasterSize);
                                        else
                                        {
                                            if (rest < 0)
                                                rest *= -1;
                                            if (rest > 0)
                                                rest = System.Convert.ToInt32(designSurface.RasterSize) - rest;
                                        }
                                        info.Bounds = new System.Windows.Rect(info.OriginalBounds.Left,
                                                               System.Convert.ToInt32(info.OriginalBounds.Top) + rest,
                                                               info.OriginalBounds.Width,
                                                               info.OriginalBounds.Height);
                                    }
                                    else
                                    {
                                        info.Bounds = new System.Windows.Rect(info.OriginalBounds.Left,
                                                               info.OriginalBounds.Top + 10,
                                                               info.OriginalBounds.Width,
                                                               info.OriginalBounds.Height);
                                    }
                                }
                                else
                                {
                                    info.Bounds = new System.Windows.Rect(info.OriginalBounds.Left,
                                                           info.OriginalBounds.Top + 1,
                                                           info.OriginalBounds.Width,
                                                           info.OriginalBounds.Height);
                                }
                            }
                            else if (direction == Key.Left)
                            {
                                if (large)
                                {
                                    if (designSurface.IsRasterOn)
                                    {
                                        int rest = 0;
                                        if (System.Convert.ToInt32(info.OriginalBounds.Left) != 0)
                                            rest = System.Convert.ToInt32(info.OriginalBounds.Left) % System.Convert.ToInt32(designSurface.RasterSize);
                                        if (rest == 0)
                                            rest = System.Convert.ToInt32(designSurface.RasterSize);
                                        else
                                        {
                                            if (rest < 0)
                                                rest *= -1;
                                            //if (rest > 0)
                                            //rest = System.Convert.ToInt32(designSurface.RasterSize) - rest;
                                        }
                                        info.Bounds = new System.Windows.Rect(System.Convert.ToInt32(info.OriginalBounds.Left) - rest,
                                                               info.OriginalBounds.Top,
                                                               info.OriginalBounds.Width,
                                                               info.OriginalBounds.Height);
                                    }
                                    else
                                    {
                                        info.Bounds = new System.Windows.Rect(info.OriginalBounds.Left - 10,
                                                               info.OriginalBounds.Top,
                                                               info.OriginalBounds.Width,
                                                               info.OriginalBounds.Height);
                                    }
                                }
                                else
                                {
                                    info.Bounds = new System.Windows.Rect(info.OriginalBounds.Left - 1,
                                                           info.OriginalBounds.Top,
                                                           info.OriginalBounds.Width,
                                                           info.OriginalBounds.Height);
                                }
                            }
                            else if (direction == Key.Right)
                            {
                                if (large)
                                {
                                    if (designSurface.IsRasterOn)
                                    {
                                        int rest = 0;
                                        if (System.Convert.ToInt32(info.OriginalBounds.Left) != 0)
                                            rest = System.Convert.ToInt32(info.OriginalBounds.Left) % System.Convert.ToInt32(designSurface.RasterSize);
                                        if (rest == 0)
                                            rest = System.Convert.ToInt32(designSurface.RasterSize);
                                        else
                                        {
                                            if (rest < 0)
                                                rest *= -1;
                                            if (rest > 0)
                                                rest = System.Convert.ToInt32(designSurface.RasterSize) - rest;
                                        }
                                        info.Bounds = new System.Windows.Rect(System.Convert.ToInt32(info.OriginalBounds.Left) + rest,
                                                               info.OriginalBounds.Top,
                                                               info.OriginalBounds.Width,
                                                               info.OriginalBounds.Height);
                                    }
                                    else
                                    {
                                        info.Bounds = new System.Windows.Rect(info.OriginalBounds.Left + 10,
                                                               info.OriginalBounds.Top,
                                                               info.OriginalBounds.Width,
                                                               info.OriginalBounds.Height);
                                    }
                                }
                                else
                                {
                                    info.Bounds = new System.Windows.Rect(info.OriginalBounds.Left + 1,
                                                           info.OriginalBounds.Top,
                                                           info.OriginalBounds.Width,
                                                           info.OriginalBounds.Height);
                                }
                            }
                        }
                        //operation.CurrentContainerBehavior.BeforeSetPosition(operation);
                        foreach (PlacementInformation info in operation.PlacedItems)
                        {
                            if (operation.CurrentContainerBehavior != null)
                                operation.CurrentContainerBehavior.SetPosition(info);
                            if (operation.CurrentDependentDrawingsBehavior != null)
                                operation.CurrentDependentDrawingsBehavior.SetPosition(info);
                        }
                        operation.Commit();
                    }
                }
                catch (Exception ec)
                {
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null 
                                                                   && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                        core.datamodel.Database.Root.Messages.LogException("FocusNavigator", "MoveItem", msg);
                }
            }
        }

        bool CanMoveItem(object surface)
        {
            var designSurface = surface as DesignSurface;
            if (designSurface != null)
            {
                if (Keyboard.FocusedElement == designSurface._designPanel)
                    return true;
            }
            return false;
        }

    }
}
