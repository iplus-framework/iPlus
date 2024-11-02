// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace gip.core.communication
{
    public static class Extensions
    {
        internal const string _LibExSource = "gip.core.communication.";


        #region Socket

        public static void Connect(this Socket Source, IPEndPoint EndPoint, int Timeout)
        {
            if (Source == null)
                throw new ArgumentNullException("Source") { Source = _LibExSource + "Extensions.Connect" };
            else if (Source.ProtocolType !=  ProtocolType.Tcp)
                throw new Exception("Invalid socket protocol type. Only TCP is supported.") { Source = _LibExSource + "Extensions.Connect" };

            try
            {
                Source.Blocking = false;
                Source.Connect(EndPoint);
            }
            catch (SocketException exx)
            {
                if (exx.ErrorCode != 10035)
                    throw new Exception("Error while connecting to " + EndPoint.ToString() + ".", exx) { Source = _LibExSource + "Extensions.Connect" };
                else if (!Source.Poll(Timeout * 1000, SelectMode.SelectWrite))
                    throw new TimeoutException("Failed to connect to " + EndPoint.ToString() + ", because remote side didn't respond.") { Source = _LibExSource + "Extensions.Connect" };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally 
            {
                Source.Blocking = true;
            }
        }

        #endregion


        #region ByteArray

        public static void WriteUShort(this byte[] Buffer, int Offset, ushort Value)
        {
            Buffer[Offset] = (byte)((Value >> 8) & 0xFF);
            Buffer[Offset + 1] = (byte)(Value & 0xFF);
        }

        public static ushort ReadUShort(this byte[] Buffer, int Offset)
        {
            ushort TMP;

            TMP = (ushort)((ushort)Buffer[Offset] << 8);
            TMP |= (ushort)Buffer[Offset + 1];

            return TMP;
        }

        public static void Write24bitUInteger(this byte[] Buffer, int Offset, uint Value)
        {
            Buffer[Offset] = (byte)((Value >> 16) & 0xFF);
            Buffer[Offset + 1] = (byte)((Value >> 8) & 0xFF);
            Buffer[Offset + 2] = (byte)(Value & 0xFF);
        }

        public static uint Read24bitUInteger(this byte[] Buffer, int Offset)
        {
            uint TMP;

            TMP = (uint)((uint)Buffer[Offset] << 16);
            TMP |= (uint)((uint)Buffer[Offset + 1] << 8);
            TMP |= (uint)Buffer[Offset + 2];

            return TMP;
        }
        
        public static void WriteUInteger(this byte[] Buffer, int Offset, uint Value)
        {
            Buffer[Offset] = (byte)((Value >> 24) & 0xFF);
            Buffer[Offset + 1] = (byte)((Value >> 16) & 0xFF);
            Buffer[Offset + 2] = (byte)((Value >> 8) & 0xFF);
            Buffer[Offset + 3] = (byte)(Value & 0xFF);
        }

        public static uint ReadUInteger(this byte[] Buffer, int Offset)
        {
            uint TMP;

            TMP = (uint)((uint)Buffer[Offset] << 24);
            TMP |= (uint)((uint)Buffer[Offset + 1] << 16);
            TMP |= (uint)((uint)Buffer[Offset + 2] << 8);
            TMP |= (uint)Buffer[Offset + 3];

            return TMP;
        }

        #endregion


        #region Stream

        public static void WriteUShort(this Stream Buffer, ushort Value)
        {
            Buffer.WriteByte((byte)(Value >> 8));
            Buffer.WriteByte((byte)(Value & 0x00FF));
        }

        public static ushort ReadUShort(this Stream Buffer)
        {
            ushort Value = 0;

            Value = (ushort)((ushort)Buffer.ReadByte() << 8);
            Value |= (ushort)Buffer.ReadByte();

            return Value;
        }

        #endregion
    }
}
