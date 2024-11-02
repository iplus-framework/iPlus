// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using EasyHook;
using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gip.tool.diagnose
{
#if DIAGNOSE
    public static class GCHook
    {
        #region Methods
        public static unsafe void ActivateHook()
        {
            _memcpyHook = LocalHook.Create(LocalHook.GetProcAddress("msvcrt.dll", "memcpy"),
                new MemcpyDelegate(memcpyHook),
                null);
            _memcpyHook.ThreadACL.SetExclusiveACL(new int[] { });

            //_VirtualAllocHook = LocalHook.Create(LocalHook.GetProcAddress("kernel32.dll", "VirtualAlloc"),
            //    new VirtualAllocDelegate(VirtualAllocHook),
            //    null);
            //_VirtualAllocHook.ThreadACL.SetExclusiveACL(new int[] { Thread.CurrentThread.ManagedThreadId });

            //_VirtualFreeHook = LocalHook.Create(LocalHook.GetProcAddress("kernel32.dll", "VirtualFree"),
            //    new VirtualFreeDelegate(VirtualFreeHook),
            //    null);
            //_VirtualFreeHook.ThreadACL.SetExclusiveACL(new int[] { });

            //_GCLogHook = LocalHook.Create(LocalHook.GetProcAddress("clr.dll", "GCLog"),
            //    new GCLogDelegate(GCLogHook),
            //    null);
            //_GCLogHook.ThreadACL.SetExclusiveACL(new int[] { });
        }

        public static unsafe void DisposeHook()
        {
            if (_memcpyHook != null)
            {
                _memcpyHook.Dispose();
                _memcpyHook = null;
            }
            //if (_VirtualAllocHook != null)
            //{
            //    _VirtualAllocHook.Dispose();
            //    _VirtualAllocHook = null;
            //}
            //if (_VirtualFreeHook != null)
            //{
            //    _VirtualFreeHook.Dispose();
            //    _VirtualFreeHook = null;
            //}
            //if (_GCLogHook != null)
            //{
            //    _GCLogHook.Dispose();
            //    _GCLogHook = null;
            //}
        }
        #endregion

        #region Mem-Copy
        static LocalHook _memcpyHook;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        unsafe delegate void* MemcpyDelegate(void* dest, IntPtr src, UIntPtr count);

        //[DllImport("msvcr120_clr0400.dll")]
        [DllImport("msvcrt.dll")]
        internal static extern unsafe void* memcpy(void* dest, IntPtr src, UIntPtr count);
        static unsafe void* memcpyHook(void* dest, IntPtr src, UIntPtr count)
        {
            ACActivator.DiagIncMemCpyCount();
            return memcpy(dest, src, count);
        }
        #endregion


        #region VirtualAlloc
        static LocalHook _VirtualAllocHook;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        unsafe delegate void* VirtualAllocDelegate(void* address, UIntPtr numBytes, int commitOrReserve, int pageProtectionMode);

        [DllImport("kernel32.dll")]
        internal static extern unsafe void* VirtualAlloc(void* address, UIntPtr numBytes, int commitOrReserve, int pageProtectionMode);

        static unsafe void* VirtualAllocHook(void* address, UIntPtr numBytes, int commitOrReserve, int pageProtectionMode)
        {
            ACActivator.DiagIncVirtualAllocCount();
            return VirtualAlloc(address, numBytes, commitOrReserve, pageProtectionMode);
        }
        #endregion


        #region VirtualFree
        static LocalHook _VirtualFreeHook;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        unsafe delegate bool VirtualFreeDelegate(void* address, UIntPtr numBytes, int pageFreeMode);

        [DllImport("kernel32.dll")]
        internal static extern unsafe bool VirtualFree(void* address, UIntPtr numBytes, int pageFreeMode);

        static unsafe bool VirtualFreeHook(void* address, UIntPtr numBytes, int pageFreeMode)
        {
            ACActivator.DiagIncVirtualFreeCount();
            return VirtualFree(address, numBytes, pageFreeMode);
        }
        #endregion

        //#region Gargabe-Collector
        //LocalHook _GCLogHook;

        //[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        //delegate bool GCLogDelegate(params string[] s);

        ///// <summary>
        ///// https://www.pinvoke.net/default.aspx/kernel32/VirtualFree.html
        ///// https://msdn.microsoft.com/library/aa366892.aspx
        ///// </summary>
        //[DllImport("clr.dll", EntryPoint = "GCLog", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        //internal extern static bool GCLog(__arglist);

        //static bool GCLogHook(params string[] s)
        //{
        //    return GCLog(__arglist(s));
        //}
        //#endregion
    }
#endif
}
