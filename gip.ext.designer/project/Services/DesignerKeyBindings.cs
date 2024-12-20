﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using gip.ext.design;

namespace gip.ext.designer.Services
{	
	class DesignerKeyBindings : IKeyBindingService
    {
        private readonly DesignSurface _surface;
        private Collection<KeyBinding> _bindings;

        public DesignerKeyBindings(DesignSurface surface)
        {
            Debug.Assert(surface != null);
            this._surface = surface;
            _bindings = new Collection<KeyBinding>();
        }

        public void RegisterBinding(KeyBinding binding)
        {
            if(binding!=null) {
                _surface.InputBindings.Add(binding);
                _bindings.Add(binding);
            }
        }

        public void DeregisterBinding(KeyBinding binding)
        {
            if(_bindings.Contains(binding)) {
                _surface.InputBindings.Remove(binding);
                _bindings.Remove(binding);
            }
        }

        public KeyBinding GetBinding(KeyGesture gesture)
        {
            return _bindings.FirstOrDefault(binding => binding.Key == gesture.Key && binding.Modifiers == gesture.Modifiers);
        }

        public KeyBinding GetBinding(InputEventArgs e)
        {
            return _bindings.FirstOrDefault(binding => binding.Gesture.Matches(null,e));
        }


        public object Owner{
            get { return _surface; }
        }

    }
}
