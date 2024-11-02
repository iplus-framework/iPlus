// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="Mxapi.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
/*****************************************************************************/
/*  MXAPI.CS   MS-WINDOWS Win32 (95/98/ME/NT/2K/XP)                          */
/*                                                                           */
/*  (C) TDi GmbH                                                             */
/*                                                                           */
/*  Defines to acces the Matrix API in C#                                    */
/*****************************************************************************/

namespace MXAPI
{
    using System;
    using System.Runtime.InteropServices;   /* Required namespace for the DllImport method */

    /// <summary>
    /// Struct DNGINFO
    /// </summary>
    public struct DNGINFO
    {
        /// <summary>
        /// The LP t_ nr
        /// </summary>
        public short LPT_Nr;
        /// <summary>
        /// The LP t_ adr
        /// </summary>
        public short LPT_Adr;
        /// <summary>
        /// The DN g_ CNT
        /// </summary>
        public short DNG_Cnt;
    };

    /// <summary>
    /// Class Matrix
    /// </summary>
    public class Matrix
    {
        /* This c#-class will import the Matrix API classes */

        /// <summary>
        /// Init_s the matrix API.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Init_MatrixAPI", CallingConvention = CallingConvention.StdCall)]
        public static extern short Init_MatrixAPI();

        /// <summary>
        /// Release_s the matrix API.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Release_MatrixAPI", CallingConvention = CallingConvention.StdCall)]
        public static extern short Release_MatrixAPI();

        /// <summary>
        /// Gets the version API.
        /// </summary>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "GetVersionAPI", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetVersionAPI();

        /// <summary>
        /// Gets the version DRV.
        /// </summary>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "GetVersionDRV", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetVersionDRV();

        /// <summary>
        /// Gets the version DR v_ USB.
        /// </summary>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "GetVersionDRV_USB", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetVersionDRV_USB();

        /// <summary>
        /// Sets the W95 access.
        /// </summary>
        /// <param name="Mode">The mode.</param>
        [DllImport("MATRIX32.DLL", EntryPoint = "GetVersionDRV_USB", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetW95Access(short Mode);

        /// <summary>
        /// Gets the port adr.
        /// </summary>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "GetPortAdr", CallingConvention = CallingConvention.StdCall)]
        public static extern short GetPortAdr(short Port);

        /// <summary>
        /// Pauses the printer activity.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "PausePrinterActivity", CallingConvention = CallingConvention.StdCall)]
        public static extern short PausePrinterActivity();

        /// <summary>
        /// Resumes the printer activity.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "ResumePrinterActivity", CallingConvention = CallingConvention.StdCall)]
        public static extern short ResumePrinterActivity();

        /// <summary>
        /// Dongle_s the find.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_Find", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_Find();

        //[DllImport("MATRIX32.DLL", EntryPoint="Dongle_FindEx", CallingConvention = CallingConvention.StdCall)]
        //unsafe public static extern short Dongle_FindEx(DNGINFO* DngInfo);

        /// <summary>
        /// Dongle_s the version.
        /// </summary>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_Version", CallingConvention = CallingConvention.StdCall)]
        public static extern int Dongle_Version(short DngNr, short Port);

        /// <summary>
        /// Dongle_s the model.
        /// </summary>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_Model", CallingConvention = CallingConvention.StdCall)]
        public static extern int Dongle_Model(short DngNr, short Port);

        /// <summary>
        /// Dongle_s the size of the mem.
        /// </summary>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_MemSize", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_MemSize(short DngNr, short Port);

        /// <summary>
        /// Dongle_s the count.
        /// </summary>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_Count", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_Count(short Port);

        /// <summary>
        /// Dongle_s the read data.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="Data">The data.</param>
        /// <param name="Count">The count.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_ReadData", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_ReadData(int UserCode, ref int Data, short Count, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the read data ex.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="Data">The data.</param>
        /// <param name="Fpos">The fpos.</param>
        /// <param name="Count">The count.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_ReadDataEx", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_ReadDataEx(int UserCode, ref int Data, short Fpos, short Count, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the read ser nr.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_ReadSerNr", CallingConvention = CallingConvention.StdCall)]
        public static extern int Dongle_ReadSerNr(int UserCode, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the write data.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="Data">The data.</param>
        /// <param name="Count">The count.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_WriteData", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_WriteData(int UserCode, ref int Data, short Count, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the write data ex.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="Data">The data.</param>
        /// <param name="Fpos">The fpos.</param>
        /// <param name="Count">The count.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_WriteDataEx", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_WriteDataEx(int UserCode, ref int Data, short Fpos, short Count, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the write key.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="KeyData">The key data.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_WriteKey", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_WriteKey(int UserCode, ref int KeyData, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the get key flag.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_GetKeyFlag", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_GetKeyFlag(int UserCode, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the exit.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_Exit", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_Exit();

        //[DllImport("MATRIX32.DLL", EntryPoint="SetConfig_MatrixNet", CallingConvention = CallingConvention.StdCall)]
        //unsafe public static extern short SetConfig_MatrixNet(short nAccess, char* nFile);
        // public static extern short SetConfig_MatrixNet(short nAccess, ref char nFile);

        /// <summary>
        /// Gets the config_ matrix net.
        /// </summary>
        /// <param name="Category">The category.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "GetConfig_MatrixNet", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetConfig_MatrixNet(short Category);

        /// <summary>
        /// Logs the in_ matrix net.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="AppSlot">The app slot.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "LogIn_MatrixNet", CallingConvention = CallingConvention.StdCall)]
        public static extern short LogIn_MatrixNet(int UserCode, short AppSlot, short DngNr);

        /// <summary>
        /// Logs the out_ matrix net.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="AppSlot">The app slot.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "LogOut_MatrixNet", CallingConvention = CallingConvention.StdCall)]
        public static extern short LogOut_MatrixNet(int UserCode, short AppSlot, short DngNr);

        /// <summary>
        /// Dongle_s the encrypt data.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="DataBlock">The data block.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_EncryptData", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_EncryptData(int UserCode, ref int DataBlock, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the decrypt data.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="DataBlock">The data block.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX32.DLL", EntryPoint = "Dongle_DecryptData", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_DecryptData(int UserCode, ref int DataBlock, short DngNr, short Port);



        /// <summary>
        /// Init_s the matrix API.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Init_MatrixAPI", CallingConvention = CallingConvention.StdCall)]
        public static extern short Init_MatrixAPI64();

        /// <summary>
        /// Release_s the matrix API.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Release_MatrixAPI", CallingConvention = CallingConvention.StdCall)]
        public static extern short Release_MatrixAPI64();

        /// <summary>
        /// Gets the version API.
        /// </summary>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "GetVersionAPI", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetVersionAPI64();

        /// <summary>
        /// Gets the version DRV.
        /// </summary>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "GetVersionDRV", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetVersionDRV64();

        /// <summary>
        /// Gets the version DR v_ USB.
        /// </summary>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "GetVersionDRV_USB", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetVersionDRV_USB64();

        /// <summary>
        /// Sets the W95 access.
        /// </summary>
        /// <param name="Mode">The mode.</param>
        [DllImport("MATRIX64.DLL", EntryPoint = "GetVersionDRV_USB", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetW95Access64(short Mode);

        /// <summary>
        /// Gets the port adr.
        /// </summary>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "GetPortAdr", CallingConvention = CallingConvention.StdCall)]
        public static extern short GetPortAdr64(short Port);

        /// <summary>
        /// Pauses the printer activity.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "PausePrinterActivity", CallingConvention = CallingConvention.StdCall)]
        public static extern short PausePrinterActivity64();

        /// <summary>
        /// Resumes the printer activity.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "ResumePrinterActivity", CallingConvention = CallingConvention.StdCall)]
        public static extern short ResumePrinterActivity64();

        /// <summary>
        /// Dongle_s the find.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_Find", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_Find64();

        //[DllImport("MATRIX64.DLL", EntryPoint="Dongle_FindEx", CallingConvention = CallingConvention.StdCall)]
        //unsafe public static extern short Dongle_FindEx(DNGINFO* DngInfo);

        /// <summary>
        /// Dongle_s the version.
        /// </summary>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_Version", CallingConvention = CallingConvention.StdCall)]
        public static extern int Dongle_Version64(short DngNr, short Port);

        /// <summary>
        /// Dongle_s the model.
        /// </summary>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_Model", CallingConvention = CallingConvention.StdCall)]
        public static extern int Dongle_Model64(short DngNr, short Port);

        /// <summary>
        /// Dongle_s the size of the mem.
        /// </summary>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_MemSize", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_MemSize64(short DngNr, short Port);

        /// <summary>
        /// Dongle_s the count.
        /// </summary>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_Count", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_Count64(short Port);

        /// <summary>
        /// Dongle_s the read data.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="Data">The data.</param>
        /// <param name="Count">The count.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_ReadData", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_ReadData64(int UserCode, ref int Data, short Count, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the read data ex.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="Data">The data.</param>
        /// <param name="Fpos">The fpos.</param>
        /// <param name="Count">The count.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_ReadDataEx", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_ReadDataEx64(int UserCode, ref int Data, short Fpos, short Count, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the read ser nr.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_ReadSerNr", CallingConvention = CallingConvention.StdCall)]
        public static extern int Dongle_ReadSerNr64(int UserCode, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the write data.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="Data">The data.</param>
        /// <param name="Count">The count.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_WriteData", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_WriteData64(int UserCode, ref int Data, short Count, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the write data ex.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="Data">The data.</param>
        /// <param name="Fpos">The fpos.</param>
        /// <param name="Count">The count.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_WriteDataEx", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_WriteDataEx64(int UserCode, ref int Data, short Fpos, short Count, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the write key.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="KeyData">The key data.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_WriteKey", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_WriteKey64(int UserCode, ref int KeyData, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the get key flag.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_GetKeyFlag", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_GetKeyFlag64(int UserCode, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the exit.
        /// </summary>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_Exit", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_Exit64();

        //[DllImport("MATRIX64.DLL", EntryPoint="SetConfig_MatrixNet", CallingConvention = CallingConvention.StdCall)]
        //unsafe public static extern short SetConfig_MatrixNet64(short nAccess, char* nFile);
        // public static extern short SetConfig_MatrixNet64(short nAccess, ref char nFile);

        /// <summary>
        /// Gets the config_ matrix net.
        /// </summary>
        /// <param name="Category">The category.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "GetConfig_MatrixNet", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetConfig_MatrixNet64(short Category);

        /// <summary>
        /// Logs the in_ matrix net.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="AppSlot">The app slot.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "LogIn_MatrixNet", CallingConvention = CallingConvention.StdCall)]
        public static extern short LogIn_MatrixNet64(int UserCode, short AppSlot, short DngNr);

        /// <summary>
        /// Logs the out_ matrix net.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="AppSlot">The app slot.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "LogOut_MatrixNet", CallingConvention = CallingConvention.StdCall)]
        public static extern short LogOut_MatrixNet64(int UserCode, short AppSlot, short DngNr);

        /// <summary>
        /// Dongle_s the encrypt data.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="DataBlock">The data block.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_EncryptData", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_EncryptData64(int UserCode, ref int DataBlock, short DngNr, short Port);

        /// <summary>
        /// Dongle_s the decrypt data.
        /// </summary>
        /// <param name="UserCode">The user code.</param>
        /// <param name="DataBlock">The data block.</param>
        /// <param name="DngNr">The DNG nr.</param>
        /// <param name="Port">The port.</param>
        /// <returns>System.Int16.</returns>
        [DllImport("MATRIX64.DLL", EntryPoint = "Dongle_DecryptData", CallingConvention = CallingConvention.StdCall)]
        public static extern short Dongle_DecryptData64(int UserCode, ref int DataBlock, short DngNr, short Port);

    }
}

