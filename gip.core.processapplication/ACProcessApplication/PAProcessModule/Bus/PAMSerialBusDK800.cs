// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Serial Bus DK800'}de{'Serieller Bus DK800'}", Global.ACKinds.TPAProcessModule, Global.ACStorableTypes.Required, false, PWGroup.PWClassName, true)]
    public class PAMSerialBusDK800 : PAMSerialBus
    {
        public PAMSerialBusDK800(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }


        #region Properties
        [ACPropertyInfo(9999, DefaultValue = false)]
        public bool OnlyOneDevice { get; set; }
        #endregion

        #region Methods

        public override void WriteOnBus(string DeviceID, object[] parameter)
        {
            return;
            //ACEventArgs eventArgs = WriteOnBusEvent.GetNewEventArgs();
            //eventArgs.GetACValue("DeviceID").Value = DeviceID;
            //eventArgs.GetACValue("WriteData").Value = parameter;
            //WriteOnBusEvent.Raise(eventArgs);
        }

        protected override void ReadOnBus()
        {
            if (this.SerialPort == null)
                return;
            if (!this.SerialPort.IsOpen)
                return;
            try
            {
                if (OnlyOneDevice)
                {
                    PAEScaleDK800 scale = FindChildComponents<PAEScaleDK800>(c => c is PAEScaleDK800, null, 1).FirstOrDefault();
                    if (scale != null)
                    {
                        if (!String.IsNullOrEmpty(scale.DeviceID) && scale.DeviceID.Length == 1)
                        {
                            this.SerialPort.Write(new byte[] { 63 }, 0, 1); // Send '?'=64 for reading 
                            byte[] arrRespData = new byte[20];
                            int readlen = this.SerialPort.Read(arrRespData, 0, 20);
                            if (readlen >= 20)
                            {
                                scale.OnFullWeightDataRead(arrRespData);
                            }
                        }
                        else
                        {
                            // Error
                        }
                    }
                }
                else
                {
                    foreach (PAEScaleDK800 scale in FindChildComponents<PAEScaleDK800>(c => c is PAEScaleDK800, null, 1))
                    {
                        if (String.IsNullOrEmpty(scale.DeviceID))
                        {
                            // Error
                            continue;
                        }
                        else if (scale.DeviceID.Length > 1)
                        {
                            // Error
                            continue;
                        }

                        // Empty Input-Buffer
                        string existing = this.SerialPort.ReadExisting();

                        // 1. Send DeviceID on Bus to signal with wihich Device we want to communicate
                        byte[] arrDeviceID = Encoding.ASCII.GetBytes(scale.DeviceID);
                        this.SerialPort.Write(arrDeviceID, 0, arrDeviceID.Length);
                        byte[] arrRespDeviceID = new byte[1];
                        try
                        {
                            if (this.SerialPort.Read(arrRespDeviceID, 0, 1) > 0)
                            {
                                // If Response of DeviceID is the same, we can write a Request now
                                if (arrRespDeviceID[0] == arrDeviceID[0])
                                {
                                    this.SerialPort.Write(new byte[] { 63 }, 0, 1); // Send '?'=64 for reading 
                                    Thread.Sleep(50);
                                    byte[] arrRespData = new byte[20];
                                    int readlen = this.SerialPort.Read(arrRespData, 0, 20);
                                    if (readlen >= 20)
                                    {
                                        scale.OnFullWeightDataRead(arrRespData);
                                    }
                                }
                            }
                        }
                        catch (TimeoutException e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            Messages.LogException("PAMSerialBusDK800", "ReadOnBus", msg);
                        }
                    }
                }
            }
            catch (TimeoutException e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("PAMSerialBusDK800", "ReadOnBus(10)", msg);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("PAMSerialBusDK800", "ReadOnBus(20)", msg);
            }
        }

        #endregion
    }
}
